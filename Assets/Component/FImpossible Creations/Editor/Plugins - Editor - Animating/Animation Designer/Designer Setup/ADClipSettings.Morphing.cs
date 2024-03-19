using FIMSpace.FEditor;
using FIMSpace.FTools;
using FIMSpace.Generating;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace FIMSpace.AnimationTools
{
    [System.Serializable]
    public class ADClipSettings_Morphing : IADSettings
    {
        public AnimationClip settingsForClip;
        public List<MorphingSet> Morphs = new List<MorphingSet>();

        public ADClipSettings_Morphing() { }

        /// <summary> Used for identifying different variants when modifying the same AnimationClip multiple times </summary>
        [SerializeField, HideInInspector] private string setId = "";
        [SerializeField, HideInInspector] private int setIdHash = 0;
        public void AssignID(string name) { setId = name; setIdHash = setId.GetHashCode(); }
        public string SetID { get { return setId; } }
        public int SetIDHash { get { return setIdHash; } }
        public AnimationClip SettingsForClip { get { return settingsForClip; } }

        public void OnConstructed(AnimationClip clip, int hash) { settingsForClip = clip; setIdHash = hash; }


        public void RefreshWithSetup(ADClipSettings_Main main)
        {
            for (int i = 0; i < Morphs.Count; i++)
            {
                var morph = Morphs[i];
                morph.Refresh(main);
            }
        }


        public ADClipSettings_Morphing Copy(ADClipSettings_Morphing to, AnimationDesignerSave save, bool noCopy)
        {
            ADClipSettings_Morphing cpy = to;
            if (noCopy == false) cpy = (ADClipSettings_Morphing)MemberwiseClone();

            cpy.Morphs = new List<MorphingSet>();
            for (int i = 0; i < Morphs.Count; i++)
            {
                MorphingSet nSet = new MorphingSet(Morphs[i].Enabled, Morphs[i].MorphName, i, Morphs[i].Blend);
                cpy.Morphs.Add(nSet);
                MorphingSet.PasteValuesTo(Morphs[i], nSet);
            }

            cpy.setId = to.setId;
            cpy.setIdHash = to.setIdHash;

            return cpy;
        }


        internal static void PasteValuesTo(ADClipSettings_Morphing from, ADClipSettings_Morphing to)
        {
            for (int i = 0; i < from.Morphs.Count; i++)
                if (from.Morphs[i].Index == to.Morphs[i].Index)
                    MorphingSet.PasteValuesTo(from.Morphs[i], to.Morphs[i]);
        }


        [System.Serializable]
        public class MorphingSet : INameAndIndex
        {

            #region Display and Helper Variables

            public int Index;
            public string MorphName;

            public static MorphingSet CopyingFrom = null;

            public bool Foldown = true;
            [NonSerialized] public bool RemoveMe = false;
            public string GetName { get { return MorphName; } }
            public int GetIndex { get { return Index; } }
            public float GUIAlpha { get { if (Enabled == false) return 0.1f; if (Blend < 0.5f) return 0.2f + Blend; return 1f; } }


            #endregion


            public bool Enabled = false;
            public float Blend = 1f;
            public AnimationCurve BlendEvaluation = AnimationCurve.EaseInOut(0f, 1f, 1f, 1f);

            public AnimationClip MorphWithClip;
            public float TimeStretchMultiplier = 1;
            public float CycleOffset = 0f;

            public bool UseClipTimeModify = true;
            public AnimationCurve TimeEvaluation = AnimationCurve.Linear(0f, 0f, 1f, 1f);

            [NonSerialized] public Vector3 MorphRootPosition = Vector3.zero;
            public AnimationCurve RootMotionBlend = AnimationCurve.Linear(0f, 0f, 1f, 0f);

            public enum EOrder
            {
                InheritElasticity,
                OverrideModsAndIK,
                BeforeIK
            }

            public EOrder UpdateOrder = EOrder.InheritElasticity;


            public MorphingSet()
            {
                Enabled = true;
                Index = -1;
                MorphName = "New Morph";
                Blend = 1f;
                MorphRootPosition = Vector3.zero;
            }

            public MorphingSet(bool enabled = false, string id = "", int index = -1, float blend = 1f)
            {
                Enabled = enabled;
                Index = index;
                MorphName = id;
                Blend = blend;
                MorphRootPosition = Vector3.zero;
                //BlendEvaluation = AnimationDesignerWindow.GetExampleCurve(0f, 1f, 0.3f, 1f, 0.3f);
            }



            #region Morphing limb settings

            private void SampleMorphAnimation(AnimationDesignerSave save)
            {
                //float morphClipTime = AnimationDesignerWindow.Get.GetMainClipAnimationSampleTime(null);
                float morphClipTimeOrigin = (AnimationDesignerWindow.Get.GetAnimationProgressFromSampleTime(null, null));
                float morphClipTime = morphClipTimeOrigin;
                if ( morphClipTime > 1.0001f) morphClipTime = morphClipTimeOrigin % 1f;

                morphClipTime *= TimeStretchMultiplier;
                morphClipTime += CycleOffset;
                if (morphClipTime < 0f) morphClipTime = 1f - (-morphClipTime);
                morphClipTime = (morphClipTime * MorphWithClip.length); // % MorphWithClip.length;
                if (morphClipTime > ( MorphWithClip.length + 0.001f)) morphClipTime = morphClipTime % MorphWithClip.length;

                float maxClipTime = MorphWithClip.length;

                if (UseClipTimeModify)
                    if (MorphWithClip != null)
                    {
                        if (_latestMain != null)
                        {
                            if (_latestMain.ClipSampleTimeCurve != null)
                                if (_latestMain.ClipSampleTimeCurve.keys.Length > 0)
                                {
                                    morphClipTime = Mathf.LerpUnclamped(0f, morphClipTime, _latestMain.ClipSampleTimeCurve.Evaluate(morphClipTime / MorphWithClip.length));
                                }
                        }
                        else
                        {
                            UnityEngine.Debug.Log("[AD Error] Main Clip Setup is null - it shouldn't happen");
                        }
                    }

                morphClipTime = AnimationDesignerWindow.Get.EnsureMorphClipTime(morphClipTime, MorphWithClip, this);


                if (TimeEvaluation != null) if (TimeEvaluation.keys.Length > 1)
                    {
                        bool isDefault = false;

                        if (TimeEvaluation.keys[0].time == 0f && TimeEvaluation.keys[0].value == 0f)
                            if (TimeEvaluation.keys[1].time == 1f && TimeEvaluation.keys[1].value == 1f)
                                if (TimeEvaluation.keys[0].inTangent == 1f && TimeEvaluation.keys[0].outTangent == 1f)
                                    if (TimeEvaluation.keys[1].inTangent == 1f && TimeEvaluation.keys[1].outTangent == 1f)
                                    {
                                        isDefault = true;
                                    }

                        if (!isDefault)
                            morphClipTime = TimeEvaluation.Evaluate(morphClipTime / maxClipTime) * maxClipTime;
                    }

                MorphWithClip.SampleAnimation(save.LatestAnimator.gameObject, morphClipTime);
                AnimationDesignerWindow.Get.UpdateHumanoidIKPreview(MorphWithClip, morphClipTime);
                if (save.LatestAnimator) MorphRootPosition = save.LatestAnimator.position;
            }

            void ValidateData(AnimationDesignerSave save)
            {
                if (MorphingLimbSets == null)
                {
                    MorphingLimbSets = new List<MorphLimbSettings>();
                    RefreshLimbs(save);
                }
            }

            public void CaptureMorph(AnimationDesignerSave save, ADClipSettings_Main main)
            {
                if (MorphWithClip == null) return;
                ValidateData(save);

                Vector3 rootPosSave = Vector3.zero;

                if (main.ResetRootPosition)
                {
                    rootPosSave = save.LatestAnimator.InverseTransformPoint(save.Armature.PelvisBoneReference.TempTransform.position);
                    rootPosSave.z = 0f;
                }

                SampleMorphAnimation(save);

                if (main.ResetRootPosition)
                {
                    save.Armature.PelvisBoneReference.TempTransform.position = save.LatestAnimator.TransformPoint(rootPosSave);
                }

                for (int l = 0; l < MorphingLimbSets.Count; l++)
                {
                    MorphLimbSettings morphLimb = MorphingLimbSets[l];
                    if (morphLimb.LimbEnabled == false) continue;

                    if (l == 0)
                        morphLimb.CaptureLimbState(null, save); // pelvis
                    else
                        morphLimb.CaptureLimbState(save.Limbs[morphLimb.LimbIndex], null);
                }
            }


            public void ApplyMorph(float animProgr, AnimationDesignerSave save)
            {
                if (MorphWithClip == null) return;
                if (MorphingLimbSets.Count == 0) return;

                float blendEval = BlendEvaluation.Evaluate(animProgr) * Blend;

                // Apply pelvis
                MorphLimbSettings morphLimb = MorphingLimbSets[0];
                if (morphLimb.LimbEnabled)
                {
                    morphLimb.ApplyCapturedLimbState(null, blendEval, save);
                }

                for (int l = 1; l < MorphingLimbSets.Count; l++)
                {
                    morphLimb = MorphingLimbSets[l];
                    if (morphLimb.LimbEnabled == false) continue;

                    morphLimb.ApplyCapturedLimbState(save.Limbs[morphLimb.LimbIndex], blendEval, null);
                }

                if (RootMotionBlend != null && RootMotionBlend.length > 0 && save.LatestAnimator != null)
                {
                    if (save.LatestAnimator.UsingRootMotion())
                    {
                        float blend = RootMotionBlend.Evaluate(animProgr) * blendEval;
                        save.LatestAnimator.transform.position = Vector3.Lerp(save.LatestAnimator.transform.position, MorphRootPosition, blend);
                    }
                }
                //AnimationDesignerWindow.Get.UpdateHumanoidIKPreview(nutrue);
            }


            [System.Serializable]
            public class MorphLimbSettings
            {
                public bool LimbEnabled = false;
                public float LimbBlend = 1f;

                public float PositionBlend = 1f;
                public float RotationBlend = 1f;

                public int LimbIndex = -1;

                public void Refresh()
                {
                    Captured.Clear();
                }

                public void PasteValuesTo(MorphLimbSettings to)
                {
                    to.LimbEnabled = LimbEnabled;
                    to.LimbBlend = LimbBlend;
                    to.LimbIndex = LimbIndex;

                    to.PositionBlend = PositionBlend;
                    to.RotationBlend = RotationBlend;
                }

                public void CaptureLimbState(ADArmatureLimb limb, AnimationDesignerSave save)
                {
                    Captured.Clear();

                    if (limb == null) // Pelvis
                    {
                        Captured.Add(new MorphCapture(save));
                        return;
                    }

                    for (int l = 0; l < limb.Bones.Count; l++)
                    {
                        Captured.Add(new MorphCapture(limb.Bones[l]));
                    }
                }

                public void ApplyCapturedLimbState(ADArmatureLimb limb, float parentBlend, AnimationDesignerSave save)
                {
                    if (PositionBlend == 0f) PositionBlend = 1f;
                    if (RotationBlend == 0f) RotationBlend = 1f;

                    if (limb == null)
                    {
                        float blendVal = parentBlend * LimbBlend;
                        save.ReferencePelvis.localPosition = Vector3.LerpUnclamped(save.ReferencePelvis.localPosition, Captured[0].keyLocalPos, blendVal * PositionBlend);
                        save.ReferencePelvis.localRotation = Quaternion.SlerpUnclamped(save.ReferencePelvis.localRotation, Captured[0].keyLocalRot, blendVal * RotationBlend);
                        return;
                    }

                    if (Captured.Count != limb.Bones.Count)
                    {
                    }
                    else
                        for (int l = 0; l < Captured.Count; l++)
                        {
                            var key = Captured[l];
                            var bone = limb.Bones[l];

                            float blendVal = parentBlend * LimbBlend * limb.AnimationBlend;
                            bone.T.localPosition = Vector3.LerpUnclamped(bone.T.localPosition, key.keyLocalPos, blendVal * PositionBlend);
                            bone.T.localRotation = Quaternion.SlerpUnclamped(bone.T.localRotation, key.keyLocalRot, blendVal * RotationBlend);
                        }
                }

                public struct MorphCapture
                {
                    public Vector3 keyLocalPos;
                    public Quaternion keyLocalRot;

                    public MorphCapture(AnimationDesignerSave save) : this()
                    {
                        // Capture pelvis
                        keyLocalPos = save.ReferencePelvis.localPosition;
                        keyLocalRot = save.ReferencePelvis.localRotation;
                    }

                    public MorphCapture(ADBoneID aDBoneID) : this()
                    {
                        keyLocalPos = aDBoneID.T.localPosition;
                        keyLocalRot = aDBoneID.T.localRotation;
                    }
                }

                public List<MorphCapture> Captured = new List<MorphCapture>();
            }

            /// <summary> LimbSets[0] is reserved for pelvis! </summary>
            [SerializeField] List<MorphLimbSettings> MorphingLimbSets = new List<MorphLimbSettings>();
            void RefreshLimbs(AnimationDesignerSave save)
            {
                if (MorphingLimbSets.Count != save.Limbs.Count + 1)
                {
                    FGenerators.AdjustCount(MorphingLimbSets, save.Limbs.Count + 1, false);
                }

                for (int l = 0; l < MorphingLimbSets.Count; l++)
                {
                    MorphingLimbSets[l].LimbIndex = l - 1;
                }
            }

            #endregion


            #region Copy Paste


            public static void PasteValuesTo(MorphingSet from, MorphingSet to)
            {
                Keyframe[] evalKeys = new Keyframe[from.BlendEvaluation.keys.Length];

                from.BlendEvaluation.keys.CopyTo(evalKeys, 0);
                to.BlendEvaluation = new AnimationCurve(evalKeys);
                to.Blend = from.Blend;
                to.Enabled = from.Enabled;

                to.UpdateOrder = from.UpdateOrder;
                to.TimeStretchMultiplier = from.TimeStretchMultiplier;
                to.CycleOffset = from.CycleOffset;
                to.UseClipTimeModify = from.UseClipTimeModify;

                if (to.MorphWithClip == null) to.MorphWithClip = from.MorphWithClip;

                if (to.MorphingLimbSets.Count == 0)
                    if (AnimationDesignerWindow.Get)
                        if (AnimationDesignerWindow.Get.S) to.RefreshLimbs(AnimationDesignerWindow.Get.S);

                if (from.MorphingLimbSets.Count == to.MorphingLimbSets.Count)
                    for (int l = 0; l < from.MorphingLimbSets.Count; l++)
                    {
                        from.MorphingLimbSets[l].PasteValuesTo(to.MorphingLimbSets[l]);
                    }
            }


            #endregion


            #region GUI Related


            private ADClipSettings_Main _latestMain = null;
            public void Refresh(ADClipSettings_Main main)
            {
                _latestMain = main;
            }

            internal void DrawTopGUI(float animProgr, ADClipSettings_Main main, int i)
            {
                _latestMain = main;

                #region Backup Code

                //EditorGUILayout.BeginHorizontal();

                //Enabled = EditorGUILayout.Toggle(Enabled, GUILayout.Width(24));

                //if (!string.IsNullOrEmpty(MorphName))
                //{
                //    EditorGUILayout.LabelField(MorphName + " : Settings", FGUI_Resources.HeaderStyle);
                //}

                #region Copy Paste Buttons

                //if (CopyingFrom != null)
                //{
                //    if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("Clipboard").image, "Paste copied values"), FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(19)))
                //    {
                //        PasteValuesTo(CopyingFrom, this);
                //    }
                //}

                //if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("TreeEditor.Duplicate").image, "Copy morphing parameters values below to paste them into other limb"), FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(19)))
                //{
                //    CopyingFrom = this;
                //}

                #endregion

                //EditorGUILayout.EndHorizontal();

                #endregion

                if (!Enabled) GUI.enabled = false;

                if (!Foldown)
                {
                    EditorGUILayout.BeginHorizontal();
                    AnimationDesignerWindow.GUIDrawFloatPercentage(ref Blend, new GUIContent("Morph Blend  "));
                    AnimationDesignerWindow.DrawCurve(ref BlendEvaluation, "", 120);
                    EditorGUILayout.EndHorizontal();

                    Rect r = AnimationDesignerWindow.DrawCurveProgressOnR(animProgr, 160, 40);
                    AnimationDesignerWindow.DrawSliderProgress(Blend * BlendEvaluation.Evaluate(animProgr), 116, 177, r);
                    GUILayout.Space(5);
                }

                GUI.enabled = true;
            }


            internal void DrawHeaderGUI(List<MorphingSet> morphsList, bool advanced, ref int selector)
            {
                Color preBg = GUI.backgroundColor;

                EditorGUILayout.BeginHorizontal();

                Enabled = EditorGUILayout.Toggle(Enabled, GUILayout.Width(24));

                if (GUILayout.Button(FGUI_Resources.GetFoldSimbol(Foldown, true), EditorStyles.label, GUILayout.Width(22), GUILayout.Height(19)))
                {
                    Foldown = !Foldown;
                }

                MorphName = EditorGUILayout.TextField(MorphName);
                GUILayout.Space(4);


                #region Left - Right Arrow Keys

                if (morphsList != null)
                {
                    if (Index > 0)
                        if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowLeft, "Moving morph to be executed before other morphs"), FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(19)))
                        {
                            morphsList[Index] = morphsList[Index - 1];
                            morphsList[Index - 1] = this;
                            selector -= 1;
                        }

                    if (Index < morphsList.Count - 1)
                        if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowRight, "Moving morph to be executed after other morphs"), FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(19)))
                        {
                            morphsList[Index] = morphsList[Index + 1];
                            morphsList[Index + 1] = this;
                            selector += 1;
                        }
                }

                #endregion


                #region Copy Paste Buttons

                if (CopyingFrom != null)
                {
                    if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("Clipboard").image, "Paste copied values"), FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(19)))
                    {
                        PasteValuesTo(CopyingFrom, this);
                    }
                }

                if (CopyingFrom == this) GUI.backgroundColor = new Color(0.6f, 1f, 0.6f, 1f);
                if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("TreeEditor.Duplicate").image, "Copy modificator parameters values below to paste them into other modificator"), FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(19)))
                {
                    CopyingFrom = this;
                }
                if (CopyingFrom == this) GUI.backgroundColor = preBg;

                #endregion

                //T.position += new Vector3(Mathf.Infinity, Mathf.Infinity, -Mathf.Infinity * 10f);

                GUILayout.Space(4);
                GUI.backgroundColor = new Color(1f, 0.5f, 0.5f, 1f);
                if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Remove), FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(19)))
                {
                    RemoveMe = true;
                }
                GUI.backgroundColor = preBg;

                EditorGUILayout.EndHorizontal();

                GUILayout.Space(5);
            }


            internal void DrawParamsGUI(float animProgr, AnimationDesignerSave save)
            {
                if (Foldown)
                {
                    ValidateData(save);
                    RefreshLimbs(save);
                    MorphLimbSettings morphLimb;

                    GUILayout.Space(4);
                    EditorGUILayout.LabelField(" Prepare Morphing Mask ", FGUI_Resources.HeaderStyle);
                    GUILayout.Space(7);


                    for (int i = 1; i < MorphingLimbSets.Count; i++)
                    {
                        morphLimb = MorphingLimbSets[i];
                        GUILayout.BeginHorizontal();
                        morphLimb.LimbEnabled = EditorGUILayout.Toggle("", morphLimb.LimbEnabled, GUILayout.Width(16));
                        if (morphLimb.LimbEnabled == false) GUI.enabled = false;
                        GUILayout.Label(save.Limbs[morphLimb.LimbIndex].LimbName, GUILayout.Width(110));
                        AnimationDesignerWindow.GUIDrawFloatPercentage(ref morphLimb.LimbBlend, new GUIContent("Blend"));
                        if (morphLimb.LimbEnabled == false) GUI.enabled = true;
                        GUILayout.EndHorizontal();
                    }

                    GUILayout.Space(3);
                    // first limbSets[0] is pelvis
                    GUILayout.BeginHorizontal(); morphLimb = MorphingLimbSets[0];
                    morphLimb.LimbEnabled = EditorGUILayout.Toggle("", morphLimb.LimbEnabled, GUILayout.Width(16));
                    if (morphLimb.LimbEnabled == false) GUI.enabled = false;
                    GUILayout.Label("Pelvis", GUILayout.Width(110));
                    AnimationDesignerWindow.GUIDrawFloatPercentage(ref morphLimb.LimbBlend, new GUIContent("Blend"));
                    if (morphLimb.LimbEnabled == false) GUI.enabled = true;
                    GUILayout.EndHorizontal();

                    if (save.LatestAnimator)
                    {
                        if (save.LatestAnimator.UsingRootMotion())
                        {
                            GUILayout.Space(4);
                            AnimationDesignerWindow.DrawCurve(ref RootMotionBlend, "Root Motion Blend: ", 0, 0f, 0f, 1f, 1f, 0f, 1f, 1f, 1f, "Control root motion blend over play progress.");
                        }
                    }

                    GUILayout.Space(14);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(14);
                    if (GUILayout.Button(" Close Limb Selector ", FGUI_Resources.ButtonStyle, GUILayout.Height(17))) Foldown = !Foldown;

                    if (GUILayout.Button("Switch All", FGUI_Resources.ButtonStyle, GUILayout.Width(70), GUILayout.Height(17)))
                    {
                        for (int i = 0; i < MorphingLimbSets.Count; i++)
                        {
                            morphLimb = MorphingLimbSets[i];
                            morphLimb.LimbEnabled = !morphLimb.LimbEnabled;
                            EditorUtility.SetDirty(save);
                        }
                    }

                    GUILayout.Space(14);
                    GUILayout.EndHorizontal();

                    GUILayout.Space(4);

                    EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);
                    EditorGUIUtility.labelWidth = 200;
                    UseClipTimeModify = EditorGUILayout.Toggle("Clip Time Modify Vulnerable: ", UseClipTimeModify);
                    EditorGUIUtility.labelWidth = 140;
                    AnimationDesignerWindow.DrawCurve(ref TimeEvaluation, "Time Flow: ", 0, 0f, 0f, 1f, 1f, 0f, 1f, 1f, 1f, "Easily control time flow for the played morph clip.");
                    GUILayout.Space(5);
                    EditorGUIUtility.labelWidth = 0;
                    EditorGUILayout.EndVertical();

                    return;
                }

                GUILayout.Space(5);

                if (!Enabled) GUI.enabled = false;
                Color preC = GUI.color;

                GUILayout.BeginHorizontal();
                EditorGUIUtility.labelWidth = 84;
                MorphWithClip = (AnimationClip)EditorGUILayout.ObjectField("Morph With:", MorphWithClip, typeof(AnimationClip), false);
                EditorGUIUtility.labelWidth = 0;
                GUILayout.Space(4);
                EditorGUIUtility.labelWidth = 24;
                CycleOffset = EditorGUILayout.FloatField(new GUIContent(EditorGUIUtility.IconContent("Animation.FilterBySelection").image, "Clip progress cycle offset"), CycleOffset, GUILayout.Width(52));
                if (CycleOffset < -1f) CycleOffset = -1f; else if (CycleOffset > 1f) CycleOffset = 1f;
                GUILayout.Space(4);
                TimeStretchMultiplier = EditorGUILayout.FloatField(new GUIContent(EditorGUIUtility.IconContent("UnityEditor.AnimationWindow").image, "Time stretch multiplier value for animation clip"), TimeStretchMultiplier, GUILayout.Width(58));
                GUILayout.EndHorizontal();
                EditorGUIUtility.labelWidth = 0;


                // Selected clips infos
                if (_latestMain != null) if (_latestMain.SettingsForClip != null) if (MorphWithClip != null)
                        {
                            AnimationClip mainCl = _latestMain.SettingsForClip;
                            AnimationClip morphCl = MorphWithClip;

                            bool wrong = false;
                            if (mainCl.humanMotion != morphCl.humanMotion)
                            {
                                wrong = true;
                                if (mainCl.humanMotion)
                                    EditorGUILayout.HelpBox("Selected morph animation clip is not humanoid clip!", MessageType.Error);
                                else
                                    EditorGUILayout.HelpBox("Selected morph animation clip is humanoid clip!", MessageType.Error);
                            }

                            if (mainCl.length <= 0f)
                            {
                                wrong = true;
                                EditorGUILayout.HelpBox("Clip duration must be greater than zero!", MessageType.Error);
                            }

                            if (!wrong)
                            {
                                string info = "Main Clip Length: " + System.Math.Round(mainCl.length, 1) + "    Morph Clip Length: " + System.Math.Round(morphCl.length, 1);
                                info += "\nMorph Time Strech Factor  =  x" + System.Math.Round((morphCl.length / mainCl.length) * TimeStretchMultiplier, 2);
                                EditorGUILayout.LabelField(info, EditorStyles.centeredGreyMiniLabel, GUILayout.Height(30));
                            }

                        }


                GUILayout.Space(4);

                GUILayout.BeginHorizontal();
                UpdateOrder = (EOrder)EditorGUILayout.EnumPopup("Morph Update Order:", UpdateOrder);
                if (GUILayout.Button("Mask", FGUI_Resources.ButtonStyle, GUILayout.Height(17), GUILayout.Width(50))) { Foldown = !Foldown; }
                GUILayout.EndHorizontal();

                GUILayout.Space(4);

                //AnimationDesignerWindow.GUIDrawFloatPercentage(ref TestBlend, new GUIContent("  TestBlend", FGUI_Resources.Tex_Rotation));

                GUI.enabled = true;
            }


            #endregion


        }

    }
}