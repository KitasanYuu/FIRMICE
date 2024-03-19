using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.AnimationTools
{
    public partial class AnimationDesignerWindow : EditorWindow
    {


        #region GUI Related

        [SerializeField] int _sel_morph_index = -1;
        public static ADClipSettings_Morphing MorphSettingsCopyFrom = null;

        void DrawMorphingTab()
        {
            if (isReady == false) { EditorGUILayout.HelpBox("First prepare Armature", MessageType.Info); return; }

            if (TargetClip == null)
            {
                EditorGUILayout.HelpBox("Animation Clip is required", MessageType.Info);
                return;
            }

            if (S.Limbs.Count == 0)
            {
                EditorGUILayout.HelpBox("Morphs requires prepared limbs! Go to setup tab.", MessageType.Info);
                return;
            }

            GUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();

            _anim_MainSet.TurnOnMorphs = EditorGUILayout.Toggle(_anim_MainSet.TurnOnMorphs, GUILayout.Width(24));
            if (_anim_MainSet.TurnOnMorphs == false) GUI.enabled = false;

            DrawTargetClipField("Morphs Configuration For: ", true);

            ADClipSettings_Morphing setup = S.GetSetupForClip(S.MorphingSetupsForClips, TargetClip, _toSet_SetSwitchToHash);

            //if (MorphSettingsCopyFrom != null)
            //{
            //    if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("Clipboard").image, "Paste copied morph setup from other morph"), FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(19)))
            //    {
            //        DisplaySave._SetDirty();
            //        MorphSettingsCopyFrom = null;
            //    }
            //}
            //GUILayout.Space(5);
            //if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("TreeEditor.Duplicate").image, "Copy all morph setup values in order to paste them into other animation clip limbs settings"), FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(19)))
            //{
            //    MorphSettingsCopyFrom = setup;
            //    DisplaySave._SetDirty();
            //}

            EditorGUILayout.EndHorizontal();


            _LimbsRefresh();

            FGUI_Inspector.DrawUILine(0.3f, 0.5f, 1, 8, 0.975f);
            GUILayout.Space(6);



            if (_anim_MainSet.TurnOnMorphs == true)
            {
                if (setup.Morphs.Count == 0)
                {
                    EditorGUILayout.HelpBox("No Morphs! add first morph to see more settings!", MessageType.Info);
                }
                else
                {

                    #region Refresh Indexes

                    for (int i = 0; i < setup.Morphs.Count; i++)
                    {
                        setup.Morphs[i].Index = i;
                    }

                    #endregion

                    StartUndoCheckFor(this, " :Morph Select");
                    DrawSelectorGUI(setup.Morphs, ref _sel_morph_index, 18, position.width - 22);
                    EndUndoCheckFor(this);

                    GUILayout.Space(5);


                    if (_sel_morph_index >= setup.Morphs.Count) _sel_morph_index = setup.Morphs.Count - 1;

                    if (_sel_morph_index > -1)
                    {
                        var mod = setup.Morphs[_sel_morph_index];
                        GUILayout.Space(5);

                        EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);

                        StartUndoCheck(": Morphs");

                        mod.DrawHeaderGUI(setup.Morphs, !sectionFocusMode, ref _sel_morph_index);

                        if (setup.Morphs.ContainsIndex(_sel_morph_index))
                        {

                            if (mod.Foldown)
                            {
                                #region Changing Bone

                                //if (Searchable.IsSetted)
                                //{
                                //    if (_SelectorHelperId == "ModChange")
                                //    {
                                //        mod.Transform = Searchable.Get<Transform>();
                                //        _SelectorHelperId = "";
                                //    }
                                //}

                                //EditorGUILayout.BeginHorizontal();
                                //Transform preT = mod.T;
                                //Transform newT = (Transform)EditorGUILayout.ObjectField("Modify:", mod.T, typeof(Transform), true);
                                //if (preT != newT) { mod.SetBoneTransform(newT); }

                                //if (DropdownButton("Change Bone to be modified by Modificator"))
                                //{
                                //    _SelectorHelperId = "ModChange";

                                //    if (latestAnimator.IsHuman())
                                //        ShowHumanoidBonesSelector("Choose Your Character Model Bone", latestAnimator.GetAnimator(), rect, S.GetNonHumanoidBonesList, false, false, false, "Humanoid Bones/", "Other Bones/");
                                //    else
                                //        ShowBonesSelector("Choose Your Character Model Bone", S.GetAllArmatureBonesList, rect);
                                //}

                                //EditorGUILayout.EndHorizontal();

                                #endregion

                            }

                            mod.DrawTopGUI(animationProgress, _anim_MainSet, _sel_morph_index);
                            mod.DrawParamsGUI(animationProgress, S);
                        }

                        EndUndoCheck();

                        GUILayout.Space(4);
                        EditorGUILayout.EndVertical();

                        GUILayout.Space(5);
                    }
                    else
                    {
                        GUILayout.Space(5);
                        EditorGUILayout.HelpBox("No Morph Selected", MessageType.Info);
                        GUILayout.Space(5);
                    }
                }

            }
            else
            {
                EditorGUILayout.HelpBox("Morphing is turned Off!", MessageType.None);
                GUILayout.Space(5);
            }



            EditorGUILayout.EndVertical();

            if (_anim_MainSet.TurnOnMorphs)
            {
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                GUI.backgroundColor = new Color(0.3f, 1f, 0.3f, 1f);
                if (GUILayout.Button(" +  Add New Morph  + ", FGUI_Resources.ButtonStyle, GUILayout.Height(18)))
                {
                    setup.Morphs.Add(new ADClipSettings_Morphing.MorphingSet(true, "Morph " + setup.Morphs.Count, setup.Morphs.Count));
                }
                GUI.backgroundColor = preBG;
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
            }

            IK_DrawTooltipField();

            EditorGUILayout.BeginVertical(); // To Avoid error for ending vertical

            // Proceeding Removing
            for (int i = setup.Morphs.Count - 1; i >= 0; i--)
                if (setup.Morphs[i].RemoveMe)
                    setup.Morphs.RemoveAt(i);
        }


        #endregion


        #region Gizmos Related

        void _Gizmos_MorphingCategory()
        {

        }

        #endregion


        #region Update Loop Related

        void _Update_MorphingCategory()
        {

        }

        #endregion


    }

}