using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.AnimationTools.CustomModules
{
    public class ADModule_FramerateStylizer : ADCustomModuleBase
    {
        public int DefaultTargetFPS = 15;

        public override string ModuleTitleName { get { return "Animation Framerate Stylizer"; } }
        public override bool GUIFoldable { get { return true; } }


        ADClipSettings_Main _lastMainSet = null;
        string _edebug = "";
        float _lastProgress = 0f;
        bool complexMode = true;

        int GetFPS(int a, int b, float t)
        {
            return Mathf.RoundToInt(Mathf.LerpUnclamped(a, b, t));
        }

        int GetFPS(float t)
        {
            return GetFPS(_FPS, _MaxFPS, t);
        }

        public override void OnPreUpdateSampling(AnimationDesignerSave s, ADClipSettings_Main anim_MainSet, ADClipSettings_CustomModules.CustomModuleSet set, ref float animProgress, ref float animationProgressClipTime)
        {
            if (relevantSet == null) return;
            if (set.ModuleVariables.Count < 5) return;
            _lastMainSet = anim_MainSet;
            float clipLen = anim_MainSet.SettingsForClip.length;
            if (clipLen <= 0f) clipLen = 1f;

            if (_FPS < 0) _FPS = 1;
            int _fps = _FPS;
            int _maxfps = _MaxFPS;

            #region Simple mode without sampling

            if (!complexMode)
            {
                float eval = _FPSCurve.Evaluate(animProgress / clipLen);
                int finalFPS = GetFPS(_fps, _maxfps, eval);

                if (eval <= 0f) eval = 1f;
                _edebug = "Variable FPS = " + finalFPS;

                float target = 1f / ( (float)finalFPS);

                #region Commented but can be helpful later

                // Look Back to avoid jitter
                //float step = 1f / 40f;
                //for (int i = 0; i < 40; i++) // 40 samples back
                //{
                //    float progr = animProgress - step * (float)i;
                //    if (progr < 0f) break;
                //    float sampleEval = set.ModuleVariables[0].GetCurve().Evaluate(progr / clipLen);
                //    float sampleTarget = 1f / (sampleEval * (float)set.ModuleVariables[1].IntV);
                //    if (sampleTarget < target) target = sampleTarget;
                //}
                //_debug_targetV = target;

                #endregion  

                if (_Main) //[2] is main clip [3] are morphs [4] is main time for curves
                    animProgress = (Mathf.Floor(animProgress / target) * target);

                if (_Time)
                    animationProgressClipTime = (Mathf.Floor(animationProgressClipTime / target) * target) % 1;

            }

            #endregion

            else // Complex Mode
            {
                if (_Main) //[2] is main clip [3] are morphs [4] is main time for curves
                    animProgress = GetTargetClipFrameProgressFor(animProgress, _FPS, _maxfps, clipLen, _FPSCurve);

                if (_Time)
                    animationProgressClipTime = GetTargetClipFrameProgressFor(animationProgressClipTime, _FPS, _maxfps, clipLen, _FPSCurve);
            }

        }

        public override void OnPreUpdateSamplingMorph(AnimationDesignerSave s, AnimationClip clip, ADClipSettings_Morphing.MorphingSet morphSet, ADClipSettings_CustomModules.CustomModuleSet set, ref float animProgress)
        {
            if (set.ModuleVariables.Count < 5) return;
            if (_Morphs == false) return;

            if (_FPS < 0) _FPS = 1;
            int _fps = _FPS;
            int _maxfps = _MaxFPS;

            if (!complexMode)
            {
                float eval = _FPSCurve.Evaluate(animProgress);
                if (eval <= 0f) eval = 1f;
                int finalFPS = GetFPS(_fps, _maxfps, eval);
                _edebug = "Variable FPS = " + finalFPS;
                float target = 1f / ((float)finalFPS);
                animProgress = (Mathf.Floor(animProgress / target) * target);
            }
            else
            {
                animProgress = GetTargetClipFrameProgressFor(animProgress * clip.length, _FPS, _maxfps, clip.length, _FPSCurve);
            }
        }


        float GetTargetClipFrameProgressFor(float mainClipTime, float fps, int maxFps, float clipLength, AnimationCurve curv)
        {
            if (curv == null) return 1f;
            if (curv.keys.Length < 2) return 1f;

            float dens = _Density;

            float target = 1f / (dens * (float)fps);

            mainClipTime = (Mathf.Floor(mainClipTime / target) * target);
            float mainProgr = mainClipTime / clipLength;

            int samplesCount = Mathf.RoundToInt(clipLength / (1f / fps));
            float fpsStep = 1f / (float)samplesCount;

            int maxSamples = samplesCount * 4;

            int _fps = GetFPS(Mathf.RoundToInt(fps), maxFps, curv.Evaluate(mainProgr));
            _edebug = "Variable FPS = " + _fps;

            maxSamples += 2;

            int sample = 0;
            float progr = 0f;
            float outProgress = 0f;

            float lastKeyProgress = 0;
            float lastKeyRange = -1f;
            //float lastStepMul = 1f;
            float stepMul = 1f;
            float maxKey = 0f;

            float denseFactor = 10f;
            RefreshCurveVarRef(); if (denseFactor < dens) denseFactor = dens;
            maxSamples *= (int)denseFactor;

            //_edebug = "MainProgr: " + mainProgr + " samples: " + samplesCount + " step = " + fpsStep;
            // Measure each time from start of the clip towards current time progress value
            // to get most relevant frame for stylized pose.
            // Don't allow to change pose progress when previous FPS delay is still in range
            while (sample < maxSamples)
            {

                if (progr >= mainProgr)
                {
                    if (lastKeyProgress < 0f) lastKeyProgress = 0f;
                    if (lastKeyProgress > 1f) lastKeyProgress = 1f;
                    outProgress = lastKeyProgress;
                    break;
                }

                if (System.Single.IsInfinity(mainProgr) || System.Single.IsNaN(mainProgr))
                {
                    break;
                }

                stepMul = 1f / ((float) _fps / fps);
                if (stepMul < 0.01f) stepMul = 0.01f;

                // Define new key sample
                if (progr >= lastKeyProgress + lastKeyRange)
                {
                    if (progr > maxKey)
                    {
                        //_edebug = "progr " + progr + " > " + maxKey;
                        lastKeyProgress = progr;
                        lastKeyRange = fpsStep * stepMul;
                        outProgress = lastKeyProgress;
                        maxKey = progr;
                    }
                }

                progr += fpsStep * (1f / denseFactor);
                sample += 1;
            }

            return outProgress * clipLength;
        }


        public override void OnInheritElasticnessUpdate(float animationProgress, float deltaTime, AnimationDesignerSave s, ADClipSettings_Main anim_MainSet, ADClipSettings_CustomModules customModules, ADClipSettings_CustomModules.CustomModuleSet set)
        {
            _lastProgress = animationProgress;

            var cmplx = GetVariable("Complx", set, true); cmplx.HideFlag = true;
            complexMode = cmplx.GetBoolValue();

            base.OnInheritElasticnessUpdate(animationProgress, deltaTime, s, anim_MainSet, customModules, set);
        }



        #region Editor GUI Related Code


        [HideInInspector] public bool _InitialInfoClicked = false;
        ADVariable curveVarRef = null;
        bool RefreshCurveVarRef()
        {
            if (curveVarRef == null) GetFPSCurveVar();
            return curveVarRef != null;
        }
        public override void InspectorGUI_HeaderFoldown(ADClipSettings_CustomModules.CustomModuleSet customModuleSet)
        {
            if (RefreshCurveVarRef())
            {
                var densVar = GetVariable("Dens", customModuleSet, 3f);
                densVar.DrawGUI();
            }

            var cmplx = GetVariable("Complx", customModuleSet, true); cmplx.HideFlag = true;
            cmplx.DisplayName = "Complex Mode";
            cmplx.Tooltip = "Enabling animation sampling algorithm which costs more CPU but can prevent jittering when using framerate curve";
            cmplx.DrawGUI();

            if (_lastMainSet != null)
                if (_lastMainSet.SettingsForClip != null)
                {
                    var lRect = GUILayoutUtility.GetLastRect();

                    float labelOFf = 153;
                    lRect.position += new Vector2(labelOFf, lRect.height + 3);
                    lRect.height = 7;
                    lRect.width -= labelOFf + 9;

                    GUI.color = Color.white * 0.4f;
                    if (pix != null) GUI.DrawTexture(lRect, pix);
                    //GUI.Box(lRect, GUIContent.none, EditorStyles.helpBox);
                    GUI.color = Color.white;

                    // Debug predicted keyframe positions


                    float FPS = _FPS;
                    AnimationCurve FPSCurve = _FPSCurve;
                    float clipLen = _lastMainSet.SettingsForClip.length;

                    float step = 1f / FPS;
                    float defaultMarkersCount = Mathf.Round(clipLen * 1 / FPS);
                    float progr = 0f;

                    int maxMarkers = step == 0 ? 100 : (Mathf.RoundToInt(clipLen / step) * 4 + 2);
                    int markers = 0;
                    var fpsCurve = _FPSCurve;

                    GUI.color = new Color(0.2f, 1f, 0.4f, 1f);

                    if (fpsCurve != null)
                        while (markers < maxMarkers)
                        {
                            progr += step;

                            Rect markerRect = new Rect(0, 0, 2, 6);
                            Vector3 nPos = new Vector2(lRect.x, lRect.y);


                            if (_FPSCurve.Evaluate(progr) < 1f)
                            {
                                markerRect.height += 1 + (1f - _FPSCurve.Evaluate(progr) * 4);
                            }
                            else if (_FPSCurve.Evaluate(progr) > 1f)
                            {
                                markerRect.width += 1 + 3 * _FPSCurve.Evaluate(progr);
                            }

                            nPos.x += lRect.width * progr;

                            markerRect.position = nPos;

                            //GUI.DrawTexture(markerRect, pix);
                            GUI.Box(markerRect, GUIContent.none, EditorStyles.helpBox);
                            markers += 1;

                            if (progr > 1f) break;
                        }

                    GUI.color = new Color(1f, 1f, 0.4f, 1f);
                    Rect timeRect = new Rect(0, 0, 2, 6);
                    timeRect.position = new Vector2(lRect.x + (_lastProgress) * lRect.width, lRect.y);

                    if (pix == null)
                    {
                        pix = new Texture2D(1, 1);
                        pix.SetPixel(0, 0, Color.white);
                        pix.Apply();
                    }

                    GUI.DrawTexture(timeRect, pix);
                    //GUI.Box(timeRect, GUIContent.none, FGUI_Resources.HeaderBoxStyleH);

                    GUI.color = Color.white;
                }

            base.InspectorGUI_HeaderFoldown(customModuleSet);
        }

        static Texture2D pix = null;


        public override void InspectorGUI_ModuleBody(float optionalBlendGhost, ADClipSettings_Main _anim_MainSet, AnimationDesignerSave s, ADClipSettings_CustomModules cModule, ADClipSettings_CustomModules.CustomModuleSet set)
        {
            var curveVar = GetFPSCurveVar();
            curveVar.DisplayName = "Framerate Over Time";
            curveVarRef = curveVar;

            var fpsVar = GetVariable("FPS", set, DefaultTargetFPS);
            fpsVar.DisplayName = "Base Framerate";
            fpsVar.FloatSwitch = ADVariable.EVarFloatingSwitch.Int;
            fpsVar.SetRangeHelperValue(new Vector2(1f, 60f));

            var fpsMaxVar = GetVariable("MaxFPS", set, DefaultTargetFPS * 2);
            fpsMaxVar.DisplayName = "Max Framerate";
            fpsMaxVar.FloatSwitch = ADVariable.EVarFloatingSwitch.Int;
            fpsMaxVar.SetRangeHelperValue(new Vector2(5f, 60f));
            fpsMaxVar.GUISpacing = (new Vector2(0, 5));

            var densVar = GetVariable("Dens", set, 10f);
            densVar.DisplayName = "Sample Density";
            densVar.HideFlag = true;
            densVar.FloatSwitch = ADVariable.EVarFloatingSwitch.Float;
            densVar.SetRangeHelperValue(new Vector2(1f, 80f));

            var pVar = GetVariable("Main", set, true); pVar.DisplayName = "Apply To Main";
            pVar = GetVariable("Morphs", set, true); pVar.DisplayName = "Apply To Morphs";
            pVar = GetVariable("Time", set, false); pVar.DisplayName = "Apply To Time";

            base.InspectorGUI_ModuleBody(optionalBlendGhost, _anim_MainSet, s, cModule, set);

            if (_edebug != "") EditorGUILayout.LabelField(_edebug, EditorStyles.centeredGreyMiniLabel);
        }

        AnimationCurve _FPSCurve
        {
            get
            {
                return GetFPSCurveVar().GetCurve();
            }
        }

        ADVariable GetFPSCurveVar()
        {
            var v = GetVariable("FPSCurve", null, AnimationCurve.EaseInOut(0f, 0f, 1f, 0f));
            if (v == null) return null;

            if (v.RangeHelperValue.w == 0f)
            {
                Vector4 v4 = v.RangeHelperValue;
                if (v4.w == 0f) v4.w = 1f;
                v.SetRangeHelperValue(v4);
            }

            return v;
        }

        int _MaxFPS { get { return GetIntVariable("MaxFPS"); } set { if (_lastMainSet == null) return; GetVariable("MaxFPS").IntV = value; } }
        int _FPS { get { return GetIntVariable("FPS"); } set { if (_lastMainSet == null) return; GetVariable("FPS").IntV = value; } }
        bool _Main { get { return GetVariable("Main").GetBoolValue(); } }
        bool _Morphs { get { return GetVariable("Morphs").GetBoolValue(); } }
        bool _Time { get { return GetVariable("Time").GetBoolValue(); } }
        float _Density { get { return GetVariable("Dens", null, 10f).GetFloatValue(); } }

        #endregion



    }
}
