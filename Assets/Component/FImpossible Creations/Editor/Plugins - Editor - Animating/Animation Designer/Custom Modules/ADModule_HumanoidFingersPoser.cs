using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.AnimationTools.CustomModules
{
    public class ADModule_HumanoidFingersPoser : ADHumanoidMuclesModuleBase
    {
        public override string ModuleTitleName
        { get { return "Humanoid/Fingers Poser"; } }

        public override void HumanoidChanges(Animator mecanim, float animationProgress, float deltaTime, AnimationDesignerSave s, ADClipSettings_Main anim_MainSet, ADClipSettings_CustomModules customModules, ADClipSettings_CustomModules.CustomModuleSet set)
        {
            var vR = GetVariable("Right", set, true);
            bool right = vR.GetBoolValue();

            var vL = GetVariable("Left", set, false);
            bool left = vL.GetBoolValue();

            //var vAddit = GetVariable("Additive", set, true);
            //bool additive = vAddit.GetBoolValue();

            var vTh = GetVariable("Thumb", set, Vector3.zero);
            var vInd = GetVariable("Index", set, Vector2.zero);
            var vMiddl = GetVariable("Middle", set, Vector2.zero);
            var vRing = GetVariable("Ring", set, Vector2.zero);
            var vPinky = GetVariable("Pinky", set, Vector2.zero);

            float blend = set.Blend * set.BlendEvaluation.Evaluate(animationProgress);

            // GUISpacing x == 1: additive   y == blend

            if (right) // Right hand
            {
                float thumbCurl = vTh.GetVector3Value().z;
                FingerAdjust(vTh, animationProgress,    MID_RightThumb1Stretched, vTh.GetVector2Value(), vTh.GUISpacing.x == 0, blend * vTh.GetVector4Value().w, (1f - thumbCurl), (1f - thumbCurl * 0.5f));
                FingerAdjust(vInd, animationProgress,   MID_RightIndex1Stretched, vInd.GetVector2Value(), vInd.GUISpacing.x == 0, blend * vInd.GetVector4Value().w, blend);
                FingerAdjust(vMiddl, animationProgress, MID_RightMiddle1Stretched, vMiddl.GetVector2Value(), vMiddl.GUISpacing.x == 0, blend * vMiddl.GetVector4Value().w, blend);
                FingerAdjust(vRing, animationProgress,  MID_RightRing1Stretched, vRing.GetVector2Value(), vRing.GUISpacing.x == 0, blend * vRing.GetVector4Value().w, blend);
                FingerAdjust(vPinky, animationProgress, MID_RightLittle1Stretched, vPinky.GetVector2Value(), vPinky.GUISpacing.x == 0, blend * vPinky.GetVector4Value().w, blend);
            }

            if (left)
            {

                float thumbCurl = vTh.GetVector3Value().z;
                FingerAdjust(vTh, animationProgress,        MID_LeftThumb1Stretched, vTh.GetVector2Value(), vTh.GUISpacing.x == 0, blend * vTh.GetVector4Value().w, (1f - thumbCurl * 0.7f), (1f - thumbCurl * 0.4f));
                FingerAdjust(vInd, animationProgress,       MID_LeftIndex1Stretched, vInd.GetVector2Value(), vInd.GUISpacing.x == 0, blend * vInd.GetVector4Value().w, blend);
                FingerAdjust(vMiddl, animationProgress,     MID_LeftMiddle1Stretched, vMiddl.GetVector2Value(), vMiddl.GUISpacing.x == 0, blend * vMiddl.GetVector4Value().w, blend);
                FingerAdjust(vRing, animationProgress,      MID_LeftRing1Stretched, vRing.GetVector2Value(), vRing.GUISpacing.x == 0, blend * vRing.GetVector4Value().w, blend);
                FingerAdjust(vPinky, animationProgress,     MID_LeftLittle1Stretched, vPinky.GetVector2Value(), vPinky.GUISpacing.x == 0, blend * vPinky.GetVector4Value().w, blend);
            }
        }

        private void FingerAdjust(ADVariable advar, float animProgr, int startI, Vector2 factors, bool additive, float blend, float blendStretch1 = 1f, float blendStretch2 = 1f)
        {
            float weight = factors.x;
            int i;

            blend *= advar.GetBlendEvaluation(animProgr);

            if (additive)
            {
                for (i = startI; i < startI + 4; i++)
                {
                    if (i == startI + 1) continue;

                    float fBlend = blend;
                    if (i == startI) fBlend *= blendStretch1;
                    if (i == startI + 2) fBlend *= blendStretch2;

                    // Using .x
                    muscles[i] = Mathf.LerpUnclamped(muscles[i], 1f, weight * fBlend);
                }

                i = startI + 1; // Using .y
                muscles[i] = Mathf.LerpUnclamped(muscles[i], 1f, factors.y * blend);
            }
            else
            {
                for (i = startI; i < startI + 4; i++)
                {
                    if (i == startI + 1) continue;

                    float fBlend = blend;
                    if (i == startI) fBlend *= blendStretch1;
                    if (i == startI + 2) fBlend *= blendStretch2;

                    // Using .x
                    muscles[i] = Mathf.LerpUnclamped(muscles[i], factors.x, fBlend);
                }

                i = startI + 1; // Using .y
                muscles[i] = Mathf.LerpUnclamped(muscles[i], factors.y, blend);
            }
        }



        public enum EDisplayMode
        {
            Thumb, Index, Middle, Ring, Pinky
        }

        public EDisplayMode Display = EDisplayMode.Thumb;



        #region Editor GUI Related Code

        static GUIContent _gca = null;
        public override void InspectorGUI_ModuleBody(float animProgr, ADClipSettings_Main _anim_MainSet, AnimationDesignerSave s, ADClipSettings_CustomModules cModule, ADClipSettings_CustomModules.CustomModuleSet set)
        {
            if (GUI_CheckForHumanoidAnimator(s, _anim_MainSet) == false) return;

            if (_gca == null) _gca = new GUIContent("A", "Additive / Override Pose Switch");

            var vR = GetVariable("Right", set, true);
            vR.DisplayName = "Right Hand";
            vR.Tooltip = "Enable to use Right Hand, disable to use Left Hand";



            EditorGUILayout.BeginHorizontal();
            vR.DrawGUI();

            var vL = GetVariable("Left", set, false);
            vL.DisplayName = "Left Hand";
            vL.Tooltip = "Enable to use Left Hand";
            vL.DrawGUI();

            Vector4 defaultVal = new Vector4(0f, 0f, 0f, 1f);
            var vTh = GetVariable("Thumb", set, defaultVal);
            vTh.Tooltip = "X is Stretch  Y is Spread  Z is Curl";
            GUILayout.FlexibleSpace();

            if (vTh.GUISpacing.x == 0)
            {
                if (GUILayout.Button("Set All Override"))
                {
                    GetVariable("Thumb", set, defaultVal).GUISpacing.x = 1;
                    GetVariable("Index", set, defaultVal).GUISpacing.x = 1;
                    GetVariable("Middle", set, defaultVal).GUISpacing.x = 1;
                    GetVariable("Ring", set, defaultVal).GUISpacing.x = 1;
                    GetVariable("Pinky", set, defaultVal).GUISpacing.x = 1;
                }
            }
            else
            {
                if (GUILayout.Button("Set All Additive"))
                {
                    GetVariable("Thumb", set, defaultVal).GUISpacing.x = 0;
                    GetVariable("Index", set, defaultVal).GUISpacing.x = 0;
                    GetVariable("Middle", set, defaultVal).GUISpacing.x = 0;
                    GetVariable("Ring", set, defaultVal).GUISpacing.x = 0;
                    GetVariable("Pinky", set, defaultVal).GUISpacing.x = 0;
                }
            }

            EditorGUILayout.EndHorizontal();

            FGUI_Inspector.DrawUILineCommon(8);


            EditorGUILayout.BeginHorizontal();

            if (Display == EDisplayMode.Thumb) GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Thumb")) { Display = EDisplayMode.Thumb; }
            if (Display == EDisplayMode.Index) GUI.backgroundColor = Color.green; else GUI.backgroundColor = Color.white;
            if (GUILayout.Button("Index")) { Display = EDisplayMode.Index; }
            if (Display == EDisplayMode.Middle) GUI.backgroundColor = Color.green; else GUI.backgroundColor = Color.white;
            if (GUILayout.Button("Middle")) { Display = EDisplayMode.Middle; }
            if (Display == EDisplayMode.Ring) GUI.backgroundColor = Color.green; else GUI.backgroundColor = Color.white;
            if (GUILayout.Button("Ring")) { Display = EDisplayMode.Ring; }
            if (Display == EDisplayMode.Pinky) GUI.backgroundColor = Color.green; else GUI.backgroundColor = Color.white;
            if (GUILayout.Button("Pinky")) { Display = EDisplayMode.Pinky; }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(6);

            // GUISpacing x == 0: additive   y == blend


            DisplayFinderGUI(vTh, EDisplayMode.Thumb, animProgr, true);

            var vInd = GetVariable("Index", set, defaultVal);
            vInd.Tooltip = "X is Stretch  Y is Spread";
            DisplayFinderGUI(vInd, EDisplayMode.Index, animProgr, false);

            var vMiddl = GetVariable("Middle", set, defaultVal);
            vMiddl.Tooltip = "X is Stretch  Y is Spread";
            DisplayFinderGUI(vMiddl, EDisplayMode.Middle, animProgr, false);

            var vRing = GetVariable("Ring", set, defaultVal);
            vRing.Tooltip = "X is Stretch  Y is Spread";
            DisplayFinderGUI(vRing, EDisplayMode.Ring, animProgr, false);

            var vPinky = GetVariable("Pinky", set, defaultVal);
            vPinky.Tooltip = "X is Stretch  Y is Spread";
            DisplayFinderGUI(vPinky, EDisplayMode.Pinky, animProgr, false);

            //base.InspectorGUI_ModuleBody(optionalBlendGhost, _anim_MainSet, s, cModule, set);
        }


        void DisplayFinderGUI(ADVariable vTh, EDisplayMode disp, float animProgr, bool v3p = true)
        {
            if (Display == disp)
            {
                EditorGUILayout.BeginHorizontal();
                Vector4 v4 = vTh.GetVector4Value();
                EditorGUIUtility.labelWidth = 100;

                v4.w = EditorGUILayout.Slider(disp.ToString() + " Blend:", v4.w, 0f, 1f); EditorGUIUtility.labelWidth = 0;

                vTh.DrawBlendCurveGUI(44, animProgr);
                GUILayout.Space(8);

                if (vTh.GUISpacing.x == 0f) GUI.backgroundColor = Color.green;
                if (GUILayout.Button(_gca, GUILayout.Width(20))) { if (vTh.GUISpacing.x == 1) vTh.GUISpacing.x = 0; else vTh.GUISpacing.x = 1f; }
                GUI.backgroundColor = Color.white;


                EditorGUILayout.EndHorizontal();
                GUILayout.Space(4);

                if (v3p)
                {
                    v4.x = EditorGUILayout.Slider(disp.ToString() + " Stretch:", v4.x, -1f, 1f);
                    v4.y = EditorGUILayout.Slider(disp.ToString() + " Spread:", v4.y, -1f, 1f);
                    v4.z = EditorGUILayout.Slider(disp.ToString() + " Curl:", v4.z, -1f, 1f);
                }
                else
                {
                    v4.x = EditorGUILayout.Slider(disp.ToString() + " Stretch:", v4.x, -1f, 1f);
                    v4.y = EditorGUILayout.Slider(disp.ToString() + " Spread:", v4.y, -1f, 1f);
                }

                vTh.SetValue(v4);
                EditorGUIUtility.labelWidth = 0;
            }
        }

        #endregion Editor GUI Related Code
    }
}