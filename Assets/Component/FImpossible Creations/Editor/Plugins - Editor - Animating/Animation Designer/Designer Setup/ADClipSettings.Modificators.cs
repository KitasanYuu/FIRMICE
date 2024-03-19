using FIMSpace.FEditor;
using FIMSpace.Generating;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace FIMSpace.AnimationTools
{
    public partial class AnimationDesignerSave : ScriptableObject
    {
        public List<ADClipSettings_Modificators> Modificators = new List<ADClipSettings_Modificators>();
    }

    /// <summary>
    /// Modificators setup for single AnimationClip
    /// </summary>
    [System.Serializable]
    public partial class ADClipSettings_Modificators : IADSettings
    {
        public AnimationClip settingsForClip;
        public List<ModificatorSet> BonesModificators = new List<ModificatorSet>();

        public float AllModificatorsBlend = 1f;

        public ADClipSettings_Modificators() { }


        /// <summary> Used for identifying different variants when modifying the same AnimationClip multiple times </summary>
        [SerializeField, HideInInspector] private string setId = "";
        [SerializeField, HideInInspector] private int setIdHash = 0;
        public void AssignID(string name) { setId = name; setIdHash = setId.GetHashCode(); }
        public string SetID { get { return setId; } }
        public int SetIDHash { get { return setIdHash; } }
        public AnimationClip SettingsForClip { get { return settingsForClip; } }
        public void OnConstructed(AnimationClip clip, int hash) { settingsForClip = clip; setIdHash = hash; }


        public static ADClipSettings_Modificators CopyingFrom = null;
        public ADClipSettings_Modificators Copy()
        {
            ADClipSettings_Modificators cpy = (ADClipSettings_Modificators)MemberwiseClone();
            return cpy;
        }


        public static void PasteValuesTo(ADClipSettings_Modificators from, ADClipSettings_Modificators to)
        {
            for (int i = 0; i < from.BonesModificators.Count; i++)
            {
                var copying = from.BonesModificators[i];
                var newMod = new ModificatorSet(copying.Transform, AnimationDesignerWindow.Get.S);
                ModificatorSet.PasteValuesTo(copying, newMod);
                to.BonesModificators.Add(newMod);
            }

            EditorUtility.SetDirty(AnimationDesignerWindow.Get.S);
        }


        public void RefreshMods(AnimationDesignerSave save, ADClipSettings_Main main)
        {
            for (int i = 0; i < BonesModificators.Count; i++)
            {
                BonesModificators[i].RefreshMod(save, main);
            }
        }



        public ADClipSettings_Modificators Copy(ADClipSettings_Modificators to, AnimationDesignerSave save, bool noCopy)
        {
            ADClipSettings_Modificators cpy = to;
            if (noCopy == false) cpy = (ADClipSettings_Modificators)MemberwiseClone();

            cpy.AllModificatorsBlend = AllModificatorsBlend;
            cpy.BonesModificators = new List<ModificatorSet>();
            for (int i = 0; i < BonesModificators.Count; i++)
            {
                ModificatorSet nSet = new ModificatorSet(BonesModificators[i].T, save);
                cpy.BonesModificators.Add(nSet);
                nSet.ModName = BonesModificators[i].ModName;
                ModificatorSet.PasteValuesTo(BonesModificators[i], nSet);
            }

            cpy.setId = to.setId;
            cpy.setIdHash = to.setIdHash;

            return cpy;
        }


        [System.Serializable]
        public partial class ModificatorSet : INameAndIndex
        {

            #region Processing References

            //[NonSerialized] ADBoneID Parent;
            public Transform Transform = null;
            public Transform T { get { return Transform; } }


            public Vector3 pos { get { return T.position; } }
            public Quaternion rot { get { return T.rotation; } }



            #endregion


            public enum EModification
            {
                AdditiveRotation,
                OverrideRotation,
                AdditivePosition,
                OverridePosition,
                ElasticRotation,
                LookAtPosition
            }


            public EModification Type = EModification.AdditiveRotation;


            public enum EOrder
            {
                InheritElasticity,
                AffectIK,
                Last_Override,
                BeforeEverything
            }


            public EOrder UpdateOrder = EOrder.InheritElasticity;


            public string ModName;
            public string BoneName;
            public string GetName { get { return ModName; } }

            public int Index;

            public bool FixedAxisRotation = false;
            public bool ChildRotIndependent = false;

            public int GetIndex { get { return Index; } }

            public float GUIAlpha { get { if (Enabled == false) return 0.1f; if (Blend < 0.5f) return 0.2f + Blend; return 1f; } }

            [NonSerialized] public bool RemoveMe = false;

            public ModificatorSet()
            {
                ModName = "Unknown Mod";
            }

            public ModificatorSet(Transform t, AnimationDesignerSave save)
            {
                if (t == null)
                {
                    UnityEngine.Debug.Log("[Animation Designer] Null Transform!");
                    return;
                }

                SetBoneTransform(t);

                if (t.name.Length > 9)
                {
                    ModName = t.name.Substring(t.name.Length - 7, 7) + " Mod";
                }
                else
                    ModName = t.name + " Mod";


                Enabled = true;
                Blend = 1f;
                BlendEvaluation = AnimationCurve.EaseInOut(0f, 1f, 1f, 1f);

                #region Finding humanoid bone name

                if (save.LatestAnimator != null)
                {
                    Animator a = save.LatestAnimator.GetAnimator();

                    if (a)
                        if (a.isHuman)
                        {
                            for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
                            {
                                HumanBodyBones b = (HumanBodyBones)i;
                                if (a.GetBoneTransform(b) == t)
                                {
                                    ModName = b.ToString() + " Mod";
                                    return;
                                }
                            }
                        }
                }

                #endregion

            }


            #region Refresh

            public void SetBoneTransform(Transform t)
            {
                Transform = t;
                BoneName = t.name;
            }

            public void RefreshMod(AnimationDesignerSave save, ADClipSettings_Main main)
            {

                if (alignToBoneName != "")
                {
                    if (alignTo == null || alignTo.name != alignToBoneName)
                    {
                        alignTo = save.GetBoneByName(alignToBoneName);
                    }
                }

            }


            #endregion


            #region Gizmos Related

            internal void DrawBoxGizmo(float size)
            {
                if (T == null) return;
                Handles.CubeHandleCap(0, T.position, T.rotation, size, EventType.Repaint);
            }

            internal void DrawBoneGizmo(Transform child, float boneFatness)
            {
                if (child == null) return;
                FGUI_Handles.DrawBoneHandle(T.position, child.position, boneFatness, true);
            }

            #endregion


            #region Main Settings

            public bool Enabled = false;
            public float Blend = 1f;
            public AnimationCurve BlendEvaluation;

            public float RotationBlend = 1f;
            public float PositionBlend = 1f;

            public bool RootMotionTransformSpace = false;

            #endregion


            #region Offsets

            public Vector3 RotationValue = Vector3.zero;
            public AnimationCurve RotationEvaluate = AnimationCurve.EaseInOut(0f, 1f, 1f, 1f);
            public Vector3 PositionValue = Vector3.zero;
            public AnimationCurve PositionEvaluate = AnimationCurve.EaseInOut(0f, 1f, 1f, 1f);

            public Vector3 RotationValue2 = Vector3.zero;
            public AnimationCurve RotationEvaluate2 = AnimationCurve.EaseInOut(0f, 0.5f, 1f, 0.5f);

            #endregion


            #region Elastic Params

            public float RotationsRapidity = 0.7f;
            public float RotationsDamping = 0.25f;
            public float RotationsSwinginess = 0.6f;
            public float RotationsBoost = 0f;

            #endregion


            #region Align

            string selectorHelperId = "";

            public string alignToBoneName = "";
            public Transform alignTo = null;
            public float AlignToBlend = 1f;

            #endregion

            public int ModeSwitcher = 0;

            public static ModificatorSet CopyingFrom = null;
            public bool Foldown = false;

            public static void PasteValuesTo(ModificatorSet from, ModificatorSet to)
            {
                to.BlendEvaluation = AnimationDesignerWindow.CopyCurve(from.BlendEvaluation);
                to.PositionEvaluate = AnimationDesignerWindow.CopyCurve(from.PositionEvaluate);
                to.RotationEvaluate = AnimationDesignerWindow.CopyCurve(from.RotationEvaluate);
                to.Blend = from.Blend;
                to.Enabled = from.Enabled;
                to.Type = from.Type;
                to.UpdateOrder = from.UpdateOrder;

                to.RotationBlend = from.RotationBlend;
                to.PositionBlend = from.PositionBlend;

                to.RotationValue = from.RotationValue;
                to.PositionValue = from.PositionValue;

                to.RotationsRapidity = from.RotationsRapidity;
                to.RotationsDamping = from.RotationsDamping;
                to.RotationsSwinginess = from.RotationsSwinginess;

                to.RotationEvaluate2 = AnimationDesignerWindow.CopyCurve(from.RotationEvaluate2);
                to.RotationValue2 = from.RotationValue2;
                to.FixedAxisRotation = from.FixedAxisRotation;
                to.ChildRotIndependent = from.ChildRotIndependent;
            }


            #region GUI Related

            internal void DrawHeaderGUI(List<ModificatorSet> modsList, bool advanced, ref int selector)
            {
                if (T != null) BoneName = T.name;

                Color preBg = GUI.backgroundColor;

                EditorGUILayout.BeginHorizontal();

                Enabled = EditorGUILayout.Toggle(Enabled, GUILayout.Width(24));

                if (GUILayout.Button(FGUI_Resources.GetFoldSimbol(Foldown, true), EditorStyles.label, GUILayout.Width(22), GUILayout.Height(19)))
                {
                    Foldown = !Foldown;
                }

                ModName = EditorGUILayout.TextField(ModName);
                GUILayout.Space(4);


                #region Left - Right Arrow Keys

                //if (advanced)
                if (modsList != null)
                {
                    if (Index > 0)
                        if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowLeft, "Moving modificator to be executed before other modifiers"), FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(19)))
                        {
                            modsList[Index] = modsList[Index - 1];
                            modsList[Index - 1] = this;
                            selector -= 1;
                        }

                    if (Index < modsList.Count - 1)
                        if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowRight, "Moving modificator to be executed after other modifiers"), FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(19)))
                        {
                            modsList[Index] = modsList[Index + 1];
                            modsList[Index + 1] = this;
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

            internal void DrawTopGUI(float animProgr, ADClipSettings_Main main, int i)
            {
                if (!Enabled) GUI.enabled = false;

                if (!Foldown)
                {
                    EditorGUILayout.BeginHorizontal();
                    AnimationDesignerWindow.GUIDrawFloatPercentage(ref Blend, new GUIContent("Modifier Blend  "));
                    AnimationDesignerWindow.DrawCurve(ref BlendEvaluation, "", 120);
                    EditorGUILayout.EndHorizontal();

                    Rect r = AnimationDesignerWindow.DrawCurveProgressOnR(animProgr, 160, 40);
                    AnimationDesignerWindow.DrawSliderProgress(Blend * BlendEvaluation.Evaluate(animProgr), 116, 177, r);
                }
                else
                {
                    AnimationDesignerWindow.GUIDrawFloatPercentage(ref Blend, new GUIContent("Modifier Blend  "));
                    AnimationDesignerWindow.DrawSliderProgress(Blend * BlendEvaluation.Evaluate(animProgr), 120, 55);
                    AnimationDesignerWindow.DrawCurve(ref BlendEvaluation, "Blend Along Clip Time:");
                    AnimationDesignerWindow.DrawCurveProgress(animProgr);
                }

                GUILayout.Space(4);

                if (main != null)
                {
                    if (T != null)
                    {
                        if (T == main.RootMotionTransform)
                        {
                            if (UpdateOrder != EOrder.BeforeEverything)
                                EditorGUILayout.HelpBox("You're modifying Root Motion Transform! You should change update order to 'Before Everything' to avoid Elasticity Glitches", MessageType.None);
                        }
                    }
                }

                if (i > 0)
                    if (Type == EModification.OverridePosition || Type == EModification.OverrideRotation)
                    {
                        EditorGUILayout.HelpBox("Make sure that overriding modifiers are on the left of modificators list to not erase effect of other modificators!", MessageType.None);
                    }

                GUI.enabled = true;
            }

            internal void DrawParamsGUI(float animProgr, AnimationDesignerSave save)
            {
                if (!Enabled) GUI.enabled = false;
                Color preC = GUI.color;

                GUI.color = new Color(0f, 0f, 0f, 0.5f);
                EditorGUILayout.BeginVertical(FGUI_Resources.ViewBoxStyle);
                EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxBlankStyle);
                GUI.color = preC;

                EditorGUIUtility.labelWidth = 64;

                //FGUI_Inspector.DrawUILine(0.5f, 0.3f, 1, 8, 0.975f);
                EditorGUILayout.BeginHorizontal();
                Type = (EModification)EditorGUILayout.EnumPopup("Type:", Type);
                GUILayout.Space(8);
                UpdateOrder = (EOrder)EditorGUILayout.EnumPopup(UpdateOrder, GUILayout.Width(50));
                EditorGUILayout.EndHorizontal();


                FGUI_Inspector.DrawUILine(0.5f, 0.3f, 1, 9, 0.975f);
                GUILayout.Space(6);

                if (Type == EModification.AdditiveRotation || Type == EModification.OverrideRotation)
                {
                    #region Rotation  Mod GUI

                    EditorGUILayout.BeginHorizontal();
                    RotationValue = EditorGUILayout.Vector3Field(" Angles:", RotationValue);

                    if (FixedAxisRotation) GUI.backgroundColor = Color.green;
                    if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Rotation, "Turn ON/OFF Fixed Axis Rotation ! Whole model rotation 0,0,0 is required !"), GUILayout.Width(24), GUILayout.Height(19)))
                    {
                        FixedAxisRotation = !FixedAxisRotation;
                    }
                    GUI.backgroundColor = preC;


                    AnimationDesignerWindow.DrawCurve(ref RotationEvaluate, "", 60, 0f, -1f, 1f, 1f);
                    EditorGUILayout.EndHorizontal();

                    if (FixedAxisRotation)
                    {
                        if (AnimationDesignerWindow.Get)
                            if (AnimationDesignerWindow.Get.AnimatorTransform)
                                if (AnimationDesignerWindow.Get.AnimatorTransform.rotation != Quaternion.identity)
                                {
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.HelpBox("Modified character is not rotated in 0,0,0 - it will make fixed axis rotation to export wrong rotations.", MessageType.Warning);
                                    if (GUILayout.Button("Fix")) { AnimationDesignerWindow.Get.AnimatorTransform.rotation = Quaternion.identity; }
                                    EditorGUILayout.EndHorizontal();
                                }
                    }

                    AnimationDesignerWindow.DrawCurveProgressOnR(animProgr, 114f, 60f);


                    if (Foldown)
                    {
                        EditorGUILayout.BeginHorizontal();
                        RotationValue2 = EditorGUILayout.Vector3Field(" Rotate Child:", RotationValue2);
                        AnimationDesignerWindow.DrawCurve(ref RotationEvaluate2, "", 60, 0f, -1f, 1f, 1f);
                        EditorGUILayout.EndHorizontal();
                        AnimationDesignerWindow.DrawCurveProgressOnR(animProgr, 114f, 60f);
                        GUILayout.Space(6);

                        AnimationDesignerWindow.GUIDrawFloatPercentage(ref RotationBlend, new GUIContent("Angles Blend"));

                        if (Type == EModification.OverrideRotation)
                        {
                            if (GUILayout.Button("Get Animator Rotation")) RotationValue = (Transform.localRotation).eulerAngles;
                        }
                    }

                    #endregion
                }
                else if (Type == EModification.AdditivePosition || Type == EModification.OverridePosition)
                {
                    #region Position Mod GUI

                    GUILayout.Space(6);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUIUtility.labelWidth = 94;

                    PositionValue = EditorGUILayout.Vector3Field(" Position Offset:", PositionValue);
                    AnimationDesignerWindow.DrawCurve(ref PositionEvaluate, "", 60, 0f, -1f, 1f, 1f);
                    //AnimationDesignerWindow.DrawCurveProgress(optionalBlendGhost)

                    EditorGUILayout.EndHorizontal();
                    AnimationDesignerWindow.DrawCurveProgressOnR(animProgr, 114f, 60f);

                    if (Foldown)
                    {
                        AnimationDesignerWindow.GUIDrawFloatPercentage(ref PositionBlend, new GUIContent("Offset Blend"));
                        //RootMotionTransformSpace = EditorGUILayout.Toggle("Root Motion Space", RootMotionTransformSpace);
                    }

                    #endregion
                }
                else if (Type == EModification.ElasticRotation)
                {
                    #region Elastic Rotation GUI

                    EditorGUIUtility.labelWidth = 84;

                    RotationsBoost = EditorGUILayout.Slider(new GUIContent("   Boost", FGUI_Resources.Tex_Rotation, "Multiplying Elasticity effect to see effect results more clearly"), RotationsBoost, 0f, 1f);
                    GUILayout.Space(3);
                    RotationsRapidity = EditorGUILayout.Slider("Rapidity", RotationsRapidity, 0f, 1f);
                    RotationsDamping = EditorGUILayout.Slider("Damping", RotationsDamping, 0f, 1f);
                    RotationsSwinginess = EditorGUILayout.Slider("Swinginess", RotationsSwinginess, 0f, 1f);

                    #endregion
                }
                else if (Type == EModification.LookAtPosition)
                {
                    #region Look At Position GUI


                    EditorGUIUtility.labelWidth = 130;
                    PositionValue = EditorGUILayout.Vector3Field(" Local Look Position:", PositionValue);
                    GUILayout.Space(6);

                    string currMode = ModeSwitcher == 0 ? "Inherit Animation Mode" : "Override Animation Mode";

                    if (GUILayout.Button(currMode, EditorStyles.layerMaskField))
                    {
                        GenericMenu menu = new GenericMenu();

                        menu.AddItem(new GUIContent("Inherit Animation"), ModeSwitcher == 0, () => { ModeSwitcher = 0; });
                        menu.AddItem(new GUIContent("Override Animation"), ModeSwitcher == 1, () => { ModeSwitcher = 1; });

                        menu.ShowAsContext();
                    }


                    #region Align Fields



                    GUILayout.Space(6);
                    EditorGUILayout.BeginHorizontal();
                    GUI.color = new Color(1f, 1f, 1f, 0.7f);
                    EditorGUIUtility.labelWidth = 218;
                    alignTo = (Transform)EditorGUILayout.ObjectField("Look Towards Transform (Optional):", alignTo, typeof(Transform), true);
                    EditorGUIUtility.labelWidth = 0;
                    GUI.color = preC;
                    GUILayout.Space(6);

                    if (Searchable.IsSetted)
                        if (selectorHelperId != "")
                            if (selectorHelperId == "algn" + GetIndex)
                            {
                                object g = Searchable.Get();

                                if (g == null) alignTo = null; else alignTo = (Transform)g;

                                if (alignTo)
                                    alignToBoneName = alignTo.name;
                                else
                                    alignToBoneName = "";

                                selectorHelperId = "";
                            }


                    if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_DownFold), EditorStyles.label, GUILayout.Width(20), GUILayout.Height(18)))
                    {
                        selectorHelperId = "algn" + GetIndex;
                        AnimationDesignerWindow.ShowBonesSelector("Choose Your Character Model Bone", save.GetAllArmatureBonesList, AnimationDesignerWindow.GetMenuDropdownRect(), true);
                    }

                    EditorGUILayout.EndHorizontal();
                    if (alignTo != null) AnimationDesignerWindow.GUIDrawFloatPercentage(ref AlignToBlend, new GUIContent("Align To Blend:"));

                    #endregion


                    #endregion
                }

                #region Value ghost

                //if (TestBlend > 0f)
                //    if (optionalBlendGhost > 0f)
                //    {
                //        Rect r = GUILayoutUtility.GetLastRect();
                //        r.position += new Vector2(188, 0);
                //        r.width -= 188 + 56;

                //        GUI.color = new Color(1f, 1f, 1f, 0.055f);
                //        GUI.HorizontalSlider(r, TestBlend * optionalBlendGhost, 0f, 1f);
                //        GUI.color = preC;
                //    }

                #endregion

                EditorGUILayout.EndVertical();
                GUI.enabled = true;
                EditorGUILayout.EndVertical();
                EditorGUIUtility.labelWidth = 0;
            }

            internal ModificatorSet Copy()
            {
                ModificatorSet cpy = (ModificatorSet)MemberwiseClone();
                return cpy;
            }


            #endregion


        }

    }
}