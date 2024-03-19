using FIMSpace.FEditor;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace FIMSpace.AnimationTools
{
    [System.Serializable]
    public class ADClipSettings_Elasticness : IADSettings
    {

        public AnimationClip settingsForClip;
        public ElasticnessSet Main;
        public List<ElasticnessSet> LimbsSets = new List<ElasticnessSet>();

        public ADClipSettings_Elasticness() { }

        /// <summary> Used for identifying different variants when modifying the same AnimationClip multiple times </summary>
        [SerializeField, HideInInspector] private string setId = "";
        [SerializeField, HideInInspector] private int setIdHash = 0;
        public void AssignID(string name) { setId = name; setIdHash = setId.GetHashCode(); }
        public string SetID { get { return setId; } }
        public int SetIDHash { get { return setIdHash; } }
        public AnimationClip SettingsForClip { get { return settingsForClip; } }
        public void OnConstructed(AnimationClip clip, int hash) { settingsForClip = clip; setIdHash = hash; Main = new ElasticnessSet(); }



        public ADClipSettings_Elasticness Copy(ADClipSettings_Elasticness to, bool noCopy)
        {
            ADClipSettings_Elasticness cpy = to;
            if (noCopy == false) cpy = (ADClipSettings_Elasticness)MemberwiseClone();

            cpy.LimbsSets = new List<ElasticnessSet>();
            for (int i = 0; i < LimbsSets.Count; i++)
            {
                ElasticnessSet nSet = LimbsSets[i].Copy();
                cpy.LimbsSets.Add(nSet);
            }
            
            cpy.setId = to.setId;
            cpy.setIdHash = to.setIdHash;
            PasteValuesTo(this, cpy);

            return cpy;
        }


        public void RefreshSetsWith(AnimationDesignerSave save)
        {
            for (int l = 0; l < save.Limbs.Count; l++)
            {
                var limb = save.Limbs[l];

                bool already = false;
                for (int i = 0; i < LimbsSets.Count; i++)
                {
                    if (LimbsSets[i].ID == limb.GetName && LimbsSets[i].Index == limb.GetIndex)
                    {
                        already = true;
                        break;
                    }
                }

                if (!already)
                {
                    ElasticnessSet nSet = new ElasticnessSet(false, limb.GetName, limb.GetIndex);
                    if (SettingsForClip) if (SettingsForClip.hasGenericRootTransform || SettingsForClip.hasRootCurves) nSet.MotionInfluence = 0f;
                    LimbsSets.Add(nSet);
                    save._SetDirty();
                }
            }
        }


        internal static void PasteValuesTo(ADClipSettings_Elasticness from, ADClipSettings_Elasticness to)
        {

            for (int i = 0; i < from.LimbsSets.Count; i++)
            {
                if (!to.LimbsSets.ContainsIndex(i, true)) return;

                if (from.LimbsSets[i].Index == to.LimbsSets[i].Index)
                {
                    ElasticnessSet.PasteValuesTo(from.LimbsSets[i], to.LimbsSets[i]);
                }
            }
        }


        public ElasticnessSet GetElasticnessSettingsForLimb(ADArmatureLimb selectedLimb, AnimationDesignerSave setup)
        {
            for (int i = 0; i < LimbsSets.Count; i++)
            {
                var ls = LimbsSets[i];

                if (ls.Index == selectedLimb.GetIndex)
                {
                    return ls;
                }
            }

            ElasticnessSet set = new ElasticnessSet(false, selectedLimb.GetName, selectedLimb.GetIndex);
            LimbsSets.Add(set);

            if (setup != null) setup._SetDirty();

            return set;
        }


        [System.Serializable]
        public class ElasticnessSet
        {
            public int Index;
            public string ID;

            public bool Enabled = false;
            public float Blend = 1f;
            public AnimationCurve BlendEvaluation = AnimationCurve.EaseInOut(0f, 1f, 1f, 1f);

            public bool RotationsElasticness = true;
            public float RotationsBlend = 0.7f;
            public float RotationsRapidity = 0.7f;
            public float RotationsDamping = 0.25f;
            public float RotationsSwinginess = 0.6f;
            public float RotationInfluence = 1f;
            public bool EulerMode = false;

            public bool OnMovementElasticness = false;
            public float OnMoveBlend = 0f;
            public float MoveRapidity = 0.6f;
            public float MoveDamping = 0.3f;
            public float MoveSmoothing = 0.6f;
            public float MoveMildRotate = 0f;
            public float MoveStretch = 0f;

            public float MotionInfluence = 1f;
            [NonSerialized] public Vector3 TempInfluenceOffset = Vector3.zero;
            [NonSerialized] public Quaternion TempRotInfluenceOffset = Quaternion.identity;

            public static ElasticnessSet CopyingFrom = null;

            public ElasticnessSet Copy()
            {
                ElasticnessSet cpy = (ElasticnessSet)MemberwiseClone();
                return cpy;
            }

            public static void PasteValuesTo(ElasticnessSet from, ElasticnessSet to)
            {
                to.BlendEvaluation = AnimationDesignerWindow.CopyCurve(from.BlendEvaluation);
                to.Blend = from.Blend;
                to.Enabled = from.Enabled;
                to.EulerMode = from.EulerMode;

                to.RotationsElasticness = from.RotationsElasticness;
                to.MotionInfluence = from.MotionInfluence;
                to.RotationsBlend = from.RotationsBlend;
                to.RotationsRapidity = from.RotationsRapidity;
                to.RotationsDamping = from.RotationsDamping;
                to.RotationsSwinginess = from.RotationsSwinginess;

                to.OnMovementElasticness = from.OnMovementElasticness;
                to.OnMoveBlend = from.OnMoveBlend;
                to.MoveRapidity = from.MoveRapidity;
                to.MoveDamping = from.MoveDamping;
                to.MoveSmoothing = from.MoveSmoothing;
                to.MoveMildRotate = from.MoveMildRotate;
            }

            public ElasticnessSet(bool enabled = false, string id = "", int index = -1, float blend = 1f)
            {
                Enabled = enabled;
                Index = index;
                ID = id;
                Blend = blend;
                //BlendEvaluation = AnimationDesignerWindow.GetExampleCurve(0f, 1f, 0.4f, 1f, 0.4f);
            }

            internal void DrawTopGUI(string title = "")
            {
                Color preBg = GUI.backgroundColor;
                EditorGUILayout.BeginHorizontal();

                Enabled = EditorGUILayout.Toggle(Enabled, GUILayout.Width(24));

                if (!string.IsNullOrEmpty(title))
                {
                    EditorGUILayout.LabelField(title + " : Limb Elasticity Settings", FGUI_Resources.HeaderStyle);
                }

                #region Copy Paste Buttons

                if (CopyingFrom != null)
                {
                    if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("Clipboard").image, "Paste copied values"), FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(19)))
                    {
                        PasteValuesTo(CopyingFrom, this);
                    }
                }

                if (CopyingFrom == this) GUI.backgroundColor = new Color(0.6f, 1f, 0.6f, 1f);
                if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("TreeEditor.Duplicate").image, "Copy Elasticity parameters values below to paste them into other limb"), FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(19)))
                {
                    CopyingFrom = this;
                }
                if (CopyingFrom == this) GUI.backgroundColor = preBg;

                #endregion

                EditorGUILayout.EndHorizontal();

                GUILayout.Space(5);

                if (!Enabled) GUI.enabled = false;

                Blend = EditorGUILayout.Slider("Full Blend:", Blend, 0f, 1f);
                AnimationDesignerWindow.DrawCurve(ref BlendEvaluation, "Blend Along Clip Time");


                GUI.enabled = true;
            }

            internal void DrawParamsGUI(float optionalBlendGhost, bool drawRotation = true, bool drawMovement = true, bool drawStretch = true)
            {
                if (!Enabled) GUI.enabled = false;
                Color preC = GUI.color;

                if (drawRotation)
                {
                    AnimationDesignerWindow.GUIDrawFloatPercentage(ref RotationsBlend, new GUIContent("  Rotation Based Elasticity", FGUI_Resources.Tex_Rotation));

                    // Value Ghost
                    if (RotationsBlend > 0f)
                        if (optionalBlendGhost > 0f)
                            AnimationDesignerWindow.DrawSliderProgress(RotationsBlend * optionalBlendGhost, 188, 56);


                    if (RotationsBlend > 0f)
                    {
                        GUILayout.Space(6);
                        EditorGUI.indentLevel++;

                        EulerMode = EditorGUILayout.Toggle("Euler Mode", EulerMode);
                        RotationsRapidity = EditorGUILayout.Slider(new GUIContent("Rapidity", "When increased: muscle reaction will become more sudden"), RotationsRapidity, 0f, 1f);
                        RotationsDamping = EditorGUILayout.Slider(new GUIContent("Damping", "When small, motion will be more bouncy, when increased it will be calm"), RotationsDamping, 0f, 1f);
                        RotationsSwinginess = EditorGUILayout.Slider(new GUIContent( "Swinginess", "When increased it will be more smooth + a bit more subtle"), RotationsSwinginess, 0f, 1f);

                        EditorGUI.indentLevel--;

                        //GUILayout.Space(6);
                        //AnimationDesignerWindow.GUIDrawFloatPercentage(ref RotationInfluence, new GUIContent("Rotation Influence", "If your model rotates on scene with root motion, you can change this value lower to remove object rotation influence on the algorithm"));
                    }


                    FGUI_Inspector.DrawUILine(0.5f, 0.3f, 1, 18, 0.975f);
                }

                if (drawMovement)
                {
                    AnimationDesignerWindow.GUIDrawFloatPercentage(ref OnMoveBlend, new GUIContent("  Position Based Elasticity", FGUI_Resources.Tex_Movement));

                    // Value Ghost
                    if (OnMoveBlend > 0f)
                        if (optionalBlendGhost > 0f)
                            AnimationDesignerWindow.DrawSliderProgress(OnMoveBlend * optionalBlendGhost, 198, 57);


                    if (OnMoveBlend > 0f)
                    {
                        GUILayout.Space(6);
                        EditorGUI.indentLevel++;

                        MoveRapidity = EditorGUILayout.Slider("Rapidity", MoveRapidity, 0f, 1f);
                        MoveDamping = EditorGUILayout.Slider("Damping", MoveDamping, 0f, 1f);
                        MoveSmoothing = EditorGUILayout.Slider("Smoothing", MoveSmoothing, 0f, 1f);
                        MoveMildRotate = EditorGUILayout.Slider("Milding", MoveMildRotate, 0f, 1f);
                        if (drawStretch) MoveStretch = EditorGUILayout.Slider("Stretching", MoveStretch, 0f, 2f);
                        if (MoveStretch > 0f) EditorGUILayout.HelpBox("stretching is changing bone positions, remember to change position baking on this bones", MessageType.None);

                        EditorGUI.indentLevel--;

                        GUILayout.Space(6);
                        AnimationDesignerWindow.GUIDrawFloatPercentage(ref MotionInfluence, new GUIContent("Motion Influence", "If your model moves on scene with root motion, you can change this value lower to remove object movement influence on the algorithm"));

                    }
                }

                GUI.enabled = true;
            }
        }

    }
}