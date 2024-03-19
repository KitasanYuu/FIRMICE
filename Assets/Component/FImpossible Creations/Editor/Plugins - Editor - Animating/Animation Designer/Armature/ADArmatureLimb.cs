using FIMSpace.FEditor;
using FIMSpace.Generating;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.AnimationTools
{
    [System.Serializable]
    public partial class ADArmatureLimb : INameAndIndex
    {
        public int Index = -2;
        public int GetIndex { get { return Index; } }
        public string LimbName = "Limb";
        public string GetName { get { return LimbName; } }

        [NonSerialized] public int AlternateExecutionIndex = -2;
        public int GetExucutionIndex { get { if (AlternateExecutionIndex > -2) return AlternateExecutionIndex; else return Index; } }

        #region Utils

        public float GUIAlpha
        {
            get
            {
                float lowest = 1f;
                if (GizmosBlend < 0.8f) lowest = 0.4f + GizmosBlend * 0.45f;
                if (GizmosBlend < 0.01f) lowest = 0.05f;
                if (AnimationBlend <= 0f) lowest = 0.01f;
                return lowest;
            }
        }

        #endregion

        #region Settings

        public float GizmosBlend = 1f;

        public enum ELimbType { Arm, Leg, Spine, Other }
        public ELimbType LimbType = ELimbType.Other;


        #endregion


        public List<ADBoneID> Bones = new List<ADBoneID>();
        public ADBoneID FirstBone { get { return Bones[0]; } }
        public ADBoneID LastBone { get { return Bones[Bones.Count - 1]; } }

        public bool Calibrate = false;
        public float AnimationBlend = 1f;
        public bool ExecuteFirst = false;

        private float initLimbLength = 0.1f;
        private float initLimbLossyScale = 0.1f;
        private Transform latestRoot;

        /// <summary> Important information for IK processors to Re-Initialize </summary>
        [SerializeField, HideInInspector] private bool _hierarchyChanged = false;
        public bool CheckIfHierarchyChanged() { if (_hierarchyChanged) { _hierarchyChanged = false; return true; } return false; }

        public Transform LatestAnimator { get; private set; }

        public bool RemoveMe { get; private set; } = false;

        internal void RefresTransformReferences(ADArmatureSetup armature)
        {
            for (int b = 0; b < Bones.Count; b++)
            {
                Bones[b].RefreshTransformReference(armature, false);
                //if (Bones[b].T) Bones[b].BoneHelperName = " (" + LimbName + ")";
            }

            if (Bones.Count == 0) return;
            if (Bones[0].T == null) return;

            initLimbLength = 0f;
            initLimbLossyScale = Bones[0].T.lossyScale.x;

            Vector3 lastPos = Bones[0].pos;

            for (int b = 0; b < Bones.Count; b++)
            {
                Bones[b].RefreshTransformReference(armature, false);
                if (Bones[b].T == null) continue;
                initLimbLength += Vector3.Distance(Bones[b].pos, lastPos);
                lastPos = Bones[b].pos;
            }
        }


        #region GUI Related

        int _selectorBoneIndex = -1;

        void AddEmptyBone()
        {
            Bones.Add(new ADBoneID(""));
        }

        void AddBone(Transform bone)
        {
            if (bone == null) { AddEmptyBone(); return; }
            if (latestRoot == null) { AddEmptyBone(); return; }
            ADBoneID lBone = new ADBoneID(bone, latestRoot);
            Bones.Add(lBone);
        }

        void GUI_DrawAddChildToLimb()
        {
            Transform parent = (Transform)EditorGUILayout.ObjectField(new GUIContent( "Append All In:", "Add all child bones, of Drag & Dropped here bone, from the scene hierarchy"), null, typeof(Transform), true, GUILayout.Width(116));
            if (parent) Bones_AddAllChildBonesIn(parent);
        }

        void GUI_DrawAddToLimb()
        {
            Transform parent = (Transform)EditorGUILayout.ObjectField("Add Bone:", null, typeof(Transform), true, GUILayout.Width(116));
            if (parent) if (!ContainsTransform(parent)) AddBone(parent);
        }

        void Bones_AddAllChildBonesIn(Transform parent)
        {
            if (parent == null) return;
            if (!ContainsTransform(parent)) AddBone(parent);

            for (int c = 0; c < parent.childCount; c++)
            {
                var ch = parent.GetChild(c);
                Bones_AddAllChildBonesIn(ch);
            }
        }

        internal bool ContainsTransform(Transform t)
        {
            for (int b = 0; b < Bones.Count; b++)
            {
                if (Bones[b].T == t) return true;
            }

            return false;
        }

        internal void DisplaySetupGUI(AnimationDesignerSave save)
        {
            Color preBG = GUI.backgroundColor;
            Color preG = GUI.color;
            if (LatestAnimator) latestRoot = LatestAnimator.transform;

            if (Bones.Count == 0)
            {
                GUILayout.Space(3);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox("No Bones in Limb!", MessageType.Info);
                GUILayout.Space(8);

                EditorGUILayout.BeginVertical(GUILayout.Width(122));
                EditorGUIUtility.labelWidth = 88;
                GUI_DrawAddChildToLimb();
                GUI_DrawAddToLimb();
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
                GUILayout.Space(3);

                if (GUILayout.Button("+ Add first bone field +"))
                {
                    AddEmptyBone();
                }

                GUILayout.Space(3);

                return;
            }

            GUILayout.Space(5);

            EditorGUIUtility.labelWidth = 150;
            EditorGUILayout.BeginHorizontal();
            LimbName = EditorGUILayout.TextField(new GUIContent("Limb Name:", "This name is just for you, to easier identify limb when focusing on animating different parts of the character body."), LimbName);

            //if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowLeft, "Change Limbs execution order"), GUILayout.Width(24)) ) { save.ChangeLimbsOrder(this, false); EditorUtility.SetDirty(save); AnimationDesignerWindow.Get.CheckComponentsInitialization(true); }
            //if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowRight, "Change Limbs execution order"), GUILayout.Width(24)) ) { save.ChangeLimbsOrder(this, true); EditorUtility.SetDirty(save); AnimationDesignerWindow.Get.CheckComponentsInitialization(true); }

            GUI.backgroundColor = new Color(1f, 0.5f, 0.5f, 1f);
            if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Remove), FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(19))) RemoveMe = true;
            GUI.backgroundColor = preBG;

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(2);

            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 50;
            AnimationBlend = EditorGUILayout.Slider(new GUIContent("Blend:", "Blending whole additional motion applied to limb.\nWhen 0 then limb is not affected at all\nWhen 1 then all limb motion is applied to the baked animation"), AnimationBlend, 0f, 1f);
            //LimbEffectsBlend = EditorGUILayout.Slider(new GUIContent("Effects Blend:", "Blending procedural motion effects applied to the limb like Elasticity"), LimbEffectsBlend, 0f, 1f);

            GUILayout.Space(6);
            EditorGUIUtility.labelWidth = 40;
            ExecuteFirst = EditorGUILayout.Toggle(new GUIContent("First:", "If you want this limb to be executed before other limbs to put different influence - it should be turned on on spine limb but not on the arm limbs to put Elasticity effect of spine on the arms"), ExecuteFirst, GUILayout.Width(58));


            GUILayout.Space(6);
            EditorGUIUtility.labelWidth = 60;
            bool preCal = Calibrate;
            Calibrate = EditorGUILayout.Toggle(new GUIContent("Calibrate:", "If your limb starts to be flipping even during paused animation turn this toggle on"), Calibrate, GUILayout.Width(78));
            if (preCal != Calibrate) if (Calibrate) { AnimationDesignerWindow.ForceTPose(); CheckComponentsBlendingInitialization(true); }
            EditorGUIUtility.labelWidth = 0;
            EditorGUILayout.EndHorizontal();


            GUILayout.Space(4);

            FGUI_Inspector.DrawUILine(0.3f, 0.5f, 1, 8, 0.975f);

            EditorGUILayout.LabelField("Limb Transforms Chain", FGUI_Resources.HeaderStyle);
            GUILayout.Space(4);

            //GUIContent _gb_bl = new GUIContent("Blend:", "Animation Designer Limb Effects Amount Multiplier for single bone transform");


            if (Searchable.IsSetted)
                if (_selectorBoneIndex >= 0)
                {
                    Transform t = Searchable.Get<Transform>();

                    if (Bones.ContainsIndex(_selectorBoneIndex, true))
                    {
                        if (t == null)
                        {
                            Bones[_selectorBoneIndex].ClearTransform();
                            save._SetDirty();
                        }
                        else
                        {
                            Bones[_selectorBoneIndex].AssignTransform(t, latestRoot);
                            _hierarchyChanged = true;
                        }
                    }

                    _selectorBoneIndex = -1;
                }

            int toRemove = -1;

            for (int i = 0; i < Bones.Count; i++)
            {
                var b = Bones[i];

                EditorGUILayout.BeginHorizontal();

                GUILayout.Space(30);

                #region Child Bone Shortcut

                if (i > 0)
                    if (Bones[i - 1] != null)
                        if (Bones[i - 1].T != null)
                            if (Bones[i - 1].T.childCount > 0)
                            {
                                Transform tgt = Bones[i - 1].T.GetChild(0);
                                if (Bones[i].T != tgt)
                                    if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_UpFold, "Assign child of the upper bone to this field"), FGUI_Resources.ButtonStyle, GUILayout.Width(16), GUILayout.Height(16)))
                                    {
                                        _hierarchyChanged = true;
                                        Bones[i].AssignTransform(Bones[i - 1].T.GetChild(0), latestRoot);
                                    }
                            }

                #endregion

                Transform preT = b.T;
                if (preT == null) GUI.backgroundColor = Color.yellow;
                Transform newT = (Transform)EditorGUILayout.ObjectField(b.T, typeof(Transform), true);
                GUI.backgroundColor = preBG;
                if (newT != preT) { b.AssignTransform(newT, latestRoot); _hierarchyChanged = true; }


                #region Child Bone Shortcut

                if (i > 0)
                    if (Bones[i] != null)
                        if (Bones[i].T != null)
                            if (Bones[i].T.childCount > 0)
                            {
                                Transform tgt = Bones[i].T.GetChild(0);
                                if (i < Bones.Count - 1) if (Bones[i + 1].T == tgt) tgt = null;

                                if (tgt != null)
                                {
                                    GUI.color = new Color(.9f, .9f, .9f, .5f);
                                    if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_DownFold, "Assign child of this bone to this field"), FGUI_Resources.ButtonStyle, GUILayout.Width(14), GUILayout.Height(16)))
                                    {
                                        _hierarchyChanged = true;
                                        Bones[i].AssignTransform(Bones[i].T.GetChild(0), latestRoot);
                                    }
                                    GUI.color = preG;
                                }
                            }

                #endregion

                GUILayout.Space(4);


                GUILayout.Space(2);
                if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_DownFold, "Display quick selection menu for available bone transform to choose"), EditorStyles.label, GUILayout.Width(16), GUILayout.Height(16)))
                {
                    _selectorBoneIndex = i;
                    AnimationDesignerWindow.ShowBonesSelector("Choose Your Character Model Bone", save.GetAllArmatureBonesList, AnimationDesignerWindow.GetMenuDropdownRect(), true);
                }

                GUILayout.Space(2);
                if (GUILayout.Button("X", GUILayout.Width(20))) { toRemove = i; }

                GUILayout.Space(30);

                EditorGUILayout.EndHorizontal();

                GUILayout.Space(3);
                GUI.color = new Color(1f, 1f, 1f, 0.5f);
                GUILayout.Label("◊", FGUI_Resources.HeaderStyle);
                GUI.color = preG;
                GUILayout.Space(3);
            }

            if (toRemove > -1)
            {
                if (Bones.ContainsIndex(toRemove, true))
                {
                    Bones.RemoveAt(toRemove);
                }
            }

            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Add Next Bone Field +"))
            {
                AddEmptyBone();
            }

            GUILayout.Space(4);

            EditorGUIUtility.labelWidth = 88;
            GUI_DrawAddChildToLimb();
            EditorGUIUtility.labelWidth = 0;

            EditorGUILayout.EndHorizontal();
        }

        internal bool HaveNullBones()
        {
            for (int i = 0; i < Bones.Count; i++)
                if (Bones[i].T == null) return true;
            return false;
        }

        internal void RefreshLimb(AnimationDesignerSave s)
        {
            LatestAnimator = s.LatestAnimator;
        }

        #endregion


        #region Gizmos Related

        public float GetCurrentLimbLength()
        {
            if (initLimbLossyScale == 0f) return 1f;
            if (Bones.Count == 0) return 1f;
            if (Bones[0].T == null) return 1f;
            return initLimbLength * (Bones[0].T.lossyScale.x / initLimbLossyScale);
        }

        public void DrawGizmos(float boneFatness = 1f)
        {
            if (Bones.Count == 0) return;

            float len = GetCurrentLimbLength();
            Bones[0].DrawBoxGizmo(len * 0.025f);

            if (Bones.Count == 1) return;

            for (int b = 0; b < Bones.Count - 1; b++)
            {
                Bones[b].DrawBoneGizmo(Bones[b + 1].T, boneFatness);
                Bones[b].DrawBoxGizmo(len * 0.01f);
            }

            Bones[Bones.Count - 1].DrawBoxGizmo(len * 0.025f);
            Bones[Bones.Count - 1].DrawBoneGizmo(Bones[Bones.Count - 1].ChildT, boneFatness);
        }

        internal void DampSessionReferences()
        {
            LatestAnimator = null;

            for (int i = 0; i < Bones.Count; i++)
            {
                Bones[i].ClearElasticComponentsAndTransformRef();
            }

            IKArmProcessor = null;
            IKCCDProcessor = null;
            IKLegProcessor = null;
        }


        #endregion

    }
}