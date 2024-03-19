//using FIMSpace.FEditor;
//using FIMSpace.FTools;
//using System;
//using System.Collections.Generic;
//using UnityEditor;
//using UnityEngine;


//namespace FIMSpace.AnimationTools
//{
//    public partial class AnimationDesignerSave : ScriptableObject
//    {
//        public List<ADClipSettings_CustomModule> Springs = new List<ADClipSettings_CustomModule>();

//    }

//    /// <summary>
//    /// Hips setups for single AnimationClip
//    /// </summary>
//    [System.Serializable]
//    public partial class ADClipSettings_CustomModule : IADSettings
//    {
//        public AnimationClip settingsForClip;
//        public List<HipsSpringSet> Springs = new List<HipsSpringSet>();

//        public float AllSpringsBlend = 1f;

//        public ADClipSettings_CustomModule() { }

//        /// <summary> Used for identifying different variants when modifying the same AnimationClip multiple times </summary>
//        [SerializeField, HideInInspector] private string setId = "";
//        [SerializeField, HideInInspector] private int setIdHash = 0;
//        public void AssignID(string name) { setId = name; setIdHash = setId.GetHashCode(); }
//        public string SetID { get { return setId; } }
//        public int SetIDHash { get { return setIdHash; } }
//        public AnimationClip SettingsForClip { get { return settingsForClip; } }
//        public void OnConstructed(AnimationClip clip, int hash) { settingsForClip = clip; setIdHash = hash; }


//        public void RefreshWithSetup(AnimationDesignerSave save) { }


//        internal void CheckInitialization(AnimationDesignerSave s)
//        {
//            for (int i = 0; i < Springs.Count; i++)
//            {
//                Springs[i].CheckInitialization(s);
//            }
//        }



//        #region Backup

//        //internal static void PasteValuesTo(ADClipSettings_Modificators from, ADClipSettings_Modificators to)
//        //{
//        //    for (int i = 0; i < from.BonesModificators.Count; i++)
//        //        if (from.BonesModificators[i].Index == to.BonesModificators[i].Index)
//        //            ModificatorSet.PasteValuesTo(from.BonesModificators[i], to.BonesModificators[i]);
//        //}

//        #endregion


//        [System.Serializable]
//        public partial class HipsSpringSet : INameAndIndex
//        {

//            #region Processing References

//            //[NonSerialized] ADBoneID Parent;
//            public Transform Transform = null;
//            public Transform T { get { return Transform; } }


//            public Vector3 pos { get { return T.position; } }
//            public Quaternion rot { get { return T.rotation; } }



//            #endregion

//            public FElasticTransform MotionMuscle { get; private set; }

//            internal void CheckInitialization(AnimationDesignerSave s)
//            {
//                RefreshTransformReference(s);
//                //throw new NotImplementedException();

//                if (T == null) return;
//                if (MotionMuscle == null || MotionMuscle.transform == null)
//                {
//                    MotionMuscle = new FElasticTransform();
//                    MotionMuscle.Initialize(T);
//                }
//            }


//            public void RefreshTransformReference(AnimationDesignerSave save)
//            {
//                if (T == null)
//                {
//                    if (string.IsNullOrEmpty(BoneName)) return;
//                    Transform = save.GetBoneByName(BoneName);
//                }
//                //throw new NotImplementedException();
//            }





//            public enum EModification
//            {
//                AdditiveRotationOrPosition,
//                OverrideRotationOrPosition,
//                ElasticRotation
//            }

//            public EModification Type = EModification.AdditiveRotationOrPosition;


//            public enum EOrder
//            {
//                BeforeElasticness,
//                AfterElasticness,
//            }

//            public EOrder UpdateOrder = EOrder.BeforeElasticness;


//            public string ModName;
//            public string BoneName;
//            public string GetName { get { return ModName; } }

//            public int Index;

//            public int GetIndex { get { return Index; } }

//            public float GUIAlpha { get { if (Enabled == false) return 0.1f; if (Blend < 0.5f) return 0.2f + Blend; return 1f; } }

//            [NonSerialized] public bool RemoveMe = false;

//            public HipsSpringSet(Transform t, AnimationDesignerSave save)
//            {
//                BoneName = t.name;
//                ModName = t.name + " Modificator";
//                Transform = t;

//                Enabled = true;
//                Blend = 1f;
//                BlendEvaluation = AnimationCurve.EaseInOut(0f, 1f, 1f, 1f);
//            }


//            #region Gizmos Related

//            internal void DrawBoxGizmo(float size)
//            {
//                if (T == null) return;
//                Handles.CubeHandleCap(0, T.position, T.rotation, size, EventType.Repaint);
//            }

//            internal void DrawBoneGizmo(Transform child, float boneFatness)
//            {
//                if (child == null) return;
//                FGUI_Handles.DrawBoneHandle(T.position, child.position, boneFatness, true);
//            }

//            #endregion



//            // Settings

//            public bool Enabled = false;
//            public float Blend = 1f;
//            public AnimationCurve BlendEvaluation;


//            public float RotationBlend = 1f;
//            public float PositionBlend = 1f;


//            // Offsets
//            public Vector3 RotationValue = Vector3.zero;
//            public AnimationCurve RotationEvaluate = AnimationCurve.EaseInOut(0f, 1f, 1f, 1f);
//            public Vector3 PositionValue = Vector3.zero;
//            public AnimationCurve PositionEvaluate = AnimationCurve.EaseInOut(0f, 1f, 1f, 1f);


//            // Elastic
//            public float RotationsRapidity = 0.7f;
//            public float RotationsDamping = 0.25f;
//            public float RotationsSwinginess = 0.6f;
//            public float RotationsBoost = 0f;


//            public static HipsSpringSet CopyingFrom = null;
//            public bool Foldown = false;

//            public static void PasteValuesTo(HipsSpringSet from, HipsSpringSet to)
//            {
//                to.BlendEvaluation = AnimationDesignerWindow.CopyCurve(from.BlendEvaluation);
//                to.Blend = from.Blend;
//                to.Enabled = from.Enabled;
//                to.Type = from.Type;

//                to.RotationBlend = from.RotationBlend;
//                to.PositionBlend = from.PositionBlend;

//                to.RotationValue = from.RotationValue;
//                to.PositionValue = from.PositionValue;

//                to.RotationsRapidity = from.RotationsRapidity;
//                to.RotationsDamping = from.RotationsDamping;
//                to.RotationsSwinginess = from.RotationsSwinginess;
//            }


//            #region GUI Related

//            internal void DrawHeaderGUI(List<HipsSpringSet> modsList, bool advanced)
//            {
//                if (T != null) BoneName = T.name;

//                Color preBg = GUI.backgroundColor;

//                EditorGUILayout.BeginHorizontal();

//                Enabled = EditorGUILayout.Toggle(Enabled, GUILayout.Width(24));

//                if (GUILayout.Button(FGUI_Resources.GetFoldSimbol(Foldown, true), EditorStyles.label, GUILayout.Width(22), GUILayout.Height(19)))
//                {
//                    Foldown = !Foldown;
//                }

//                ModName = EditorGUILayout.TextField(ModName);
//                GUILayout.Space(4);


//                #region Left - Right Arrow Keys

//                if (advanced)
//                    if (modsList != null)
//                    {
//                        if (Index > 0)
//                            if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowLeft, "Moving modificator to be executed before other modificators"), FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(19)))
//                            {
//                                modsList[Index] = modsList[Index - 1];
//                                modsList[Index - 1] = this;
//                            }

//                        if (Index < modsList.Count - 1)
//                            if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowRight, "Moving modificator to be executed after other modificators"), FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(19)))
//                            {
//                                modsList[Index] = modsList[Index + 1];
//                                modsList[Index + 1] = this;
//                            }
//                    }

//                #endregion


//                #region Copy Paste Buttons

//                if (CopyingFrom != null)
//                {
//                    if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("Clipboard").image, "Paste copied values"), FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(19)))
//                    {
//                        PasteValuesTo(CopyingFrom, this);
//                    }
//                }

//                if (CopyingFrom == this) GUI.backgroundColor = new Color(0.6f, 1f, 0.6f, 1f);
//                if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("TreeEditor.Duplicate").image, "Copy spring parameters values below to paste them into other spring"), FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(19)))
//                {
//                    CopyingFrom = this;
//                }
//                if (CopyingFrom == this) GUI.backgroundColor = preBg;

//                #endregion

//                //T.position += new Vector3(Mathf.Infinity, Mathf.Infinity, -Mathf.Infinity * 10f);

//                GUILayout.Space(4);
//                GUI.backgroundColor = new Color(1f, 0.5f, 0.5f, 1f);
//                if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Remove), FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(19)))
//                {
//                    RemoveMe = true;
//                }
//                GUI.backgroundColor = preBg;

//                EditorGUILayout.EndHorizontal();

//                GUILayout.Space(5);
//            }

//            internal void DrawGizmos(float v)
//            {
//                throw new NotImplementedException();
//            }

//            internal void DrawTopGUI()
//            {
//                if (!Enabled) GUI.enabled = false;

//                if (!Foldown)
//                {
//                    EditorGUILayout.BeginHorizontal();
//                    AnimationDesignerWindow.GUIDrawFloatPercentage(ref Blend, new GUIContent("Modification Blend  "));
//                    AnimationDesignerWindow.DrawCurve(ref BlendEvaluation, "", 120);
//                    EditorGUILayout.EndHorizontal();
//                }
//                else
//                {
//                    AnimationDesignerWindow.GUIDrawFloatPercentage(ref Blend, new GUIContent("Modification Blend  "));
//                    AnimationDesignerWindow.DrawCurve(ref BlendEvaluation, "Blend Along Clip Time:");
//                }

//                GUILayout.Space(4);

//                GUI.enabled = true;
//            }

//            internal void DrawParamsGUI(float optionalBlendGhost)
//            {
//                if (!Enabled) GUI.enabled = false;
//                Color preC = GUI.color;

//                GUI.color = new Color(0f, 0f, 0f, 0.5f);
//                EditorGUILayout.BeginVertical(FGUI_Resources.ViewBoxStyle);
//                EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxBlankStyle);
//                GUI.color = preC;


//                //FGUI_Inspector.DrawUILine(0.5f, 0.3f, 1, 8, 0.975f);
//                Type = (EModification)EditorGUILayout.EnumPopup("Type:", Type);
//                FGUI_Inspector.DrawUILine(0.5f, 0.3f, 1, 9, 0.975f);

//                GUILayout.Space(6);

//                if (Type == EModification.AdditiveRotationOrPosition || Type == EModification.OverrideRotationOrPosition)
//                {
//                    #region Rotation / Position Mod

//                    EditorGUILayout.BeginHorizontal();
//                    RotationValue = EditorGUILayout.Vector3Field(" Angles:", RotationValue);
//                    AnimationDesignerWindow.DrawCurve(ref RotationEvaluate, "", 60);
//                    EditorGUILayout.EndHorizontal();

//                    if (Foldown) AnimationDesignerWindow.GUIDrawFloatPercentage(ref RotationBlend, new GUIContent("Angles Blend"));

//                    GUILayout.Space(6);

//                    EditorGUILayout.BeginHorizontal();
//                    PositionValue = EditorGUILayout.Vector3Field(" Position Offset:", PositionValue);
//                    AnimationDesignerWindow.DrawCurve(ref PositionEvaluate, "", 60);
//                    EditorGUILayout.EndHorizontal();

//                    if (Foldown) AnimationDesignerWindow.GUIDrawFloatPercentage(ref PositionBlend, new GUIContent("Offset Blend"));

//                    #endregion
//                }
//                else if (Type == EModification.ElasticRotation)
//                {
//                    #region Elastic Rotation

//                    RotationsBoost = EditorGUILayout.Slider(new GUIContent("   Boost", FGUI_Resources.Tex_Rotation, "Multiplying elasticness effect to see effect results more clearly"), RotationsBoost, 0f, 1f);
//                    GUILayout.Space(3);
//                    RotationsRapidity = EditorGUILayout.Slider("Rapidity", RotationsRapidity, 0f, 1f);
//                    RotationsDamping = EditorGUILayout.Slider("Damping", RotationsDamping, 0f, 1f);
//                    RotationsSwinginess = EditorGUILayout.Slider("Swinginess", RotationsSwinginess, 0f, 1f);

//                    #endregion
//                }

//                #region Value ghost

//                //if (TestBlend > 0f)
//                //    if (optionalBlendGhost > 0f)
//                //    {
//                //        Rect r = GUILayoutUtility.GetLastRect();
//                //        r.position += new Vector2(188, 0);
//                //        r.width -= 188 + 56;

//                //        GUI.color = new Color(1f, 1f, 1f, 0.055f);
//                //        GUI.HorizontalSlider(r, TestBlend * optionalBlendGhost, 0f, 1f);
//                //        GUI.color = preC;
//                //    }

//                #endregion

//                EditorGUILayout.EndVertical();
//                GUI.enabled = true;
//                EditorGUILayout.EndVertical();
//            }


//            #endregion


//        }
//    }
//}