using FIMSpace.FEditor;
using FIMSpace.Generating;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.AnimationTools
{
    public partial class AnimationDesignerWindow : EditorWindow
    {

        #region GUI Related


        [SerializeField] int _sel_mod_index = -1;

        public Vector3 angle;
        [NonSerialized] ADClipSettings_Modificators editedModSet;
        bool drawAllModsEval = false;



        //bool _ModificatorLimbMode = false;
        void DrawModificatorsTab()
        {
            if (isReady == false) { EditorGUILayout.HelpBox("First prepare Armature", MessageType.Info); return; }
            if (TargetClip == null) { EditorGUILayout.HelpBox("No AnimationClip to work on!", MessageType.Info); return; }

            //FGUI_Inspector.DrawUILine(0.3f, 0.5f, 1, 8, 0.975f);
            //EditorGUILayout.LabelField(new GUIContent("Single Bones Modifications", "Design additional motion to selected bone transforms"), FGUI_Resources.HeaderStyleBig);
            //GUILayout.Space(6);

            GUILayout.Space(3);
            ADClipSettings_Modificators modSet = S.GetSetupForClip(S.ModificatorsSetupsForClips, TargetClip, _toSet_SetSwitchToHash);
            editedModSet = modSet;
            

            GUILayout.Space(3);
            EditorGUILayout.BeginHorizontal();
            if (DrawTargetClipField(FGUI_Resources.GetFoldSimbol(drawAllModsEval, true) + "  Bone Modifiers For:", true)) drawAllModsEval = !drawAllModsEval;
            //DrawTargetClipField("Bone Modificators For:", true);


            #region Copy Paste Buttons

            Color preBG = GUI.backgroundColor;
            if (ADClipSettings_Modificators.CopyingFrom != null)
            {
                if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("Clipboard").image, "Paste copied values"), FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(19)))
                {
                    ADClipSettings_Modificators.PasteValuesTo(ADClipSettings_Modificators.CopyingFrom, modSet);
                }
            }

            if (ADClipSettings_Modificators.CopyingFrom == modSet) GUI.backgroundColor = new Color(0.6f, 1f, 0.6f, 1f);
            if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("TreeEditor.Duplicate").image, "Copy Elasticity parameters values below to paste them into other limb"), FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(19)))
            {
                ADClipSettings_Modificators.CopyingFrom = modSet;
            }
            if (ADClipSettings_Modificators.CopyingFrom == modSet) GUI.backgroundColor = preBG;

            #endregion


            EditorGUILayout.EndHorizontal();


            if (drawAllModsEval)
                AnimationDesignerWindow.DrawCurve(ref _anim_MainSet.ModsEvaluation, "All Mods Blend:");


            GUILayout.Space(4);
            AnimationDesignerWindow.GUIDrawFloatPercentage(ref modSet.AllModificatorsBlend, new GUIContent("All Modifiers Blend:  "));
            FGUI_Inspector.DrawUILine(0.3f, 0.5f, 1, 14, 0.975f);

            #region Adding Modificator

            // Adding new modificator

            if (Searchable.IsSetted)
            {
                if (_SelectorHelperId != "")
                    if (_SelectorHelperId == "Mod")
                    {
                        Transform chT = Searchable.Get<Transform>();

                        if (chT)
                        {
                            modSet.BonesModificators.Add(new ADClipSettings_Modificators.ModificatorSet(chT, S));
                        }

                        _SelectorHelperId = "";
                        _sel_mod_index = modSet.BonesModificators.Count - 1;
                    }
            }

            #endregion

            EditorGUILayout.BeginHorizontal();
            GUIContent c = new GUIContent(" +  Add Job for a Bone  + ");
            var rect = GUILayoutUtility.GetRect(c, EditorStyles.miniButton);
            rect.height = 24;

            if (modSet.BonesModificators.Count == 0) GUI.backgroundColor = Color.green;

            #region Bone Add Button 

            if (GUI.Button(rect, c))
            {
                rect.width = Mathf.Min(350, rect.width);

                _SelectorHelperId = "Mod";

                if (latestAnimator.IsHuman())
                {
                    ShowHumanoidBonesSelector("Choose Your Character Model Bone", latestAnimator.GetAnimator(), rect, S.GetNonHumanoidBonesList, false, false, false, "Humanoid Bones/", "Other Bones/");
                }
                else
                {
                    List<Transform> bnList = S.GetAllArmatureBonesList;
                    if (S.SkelRootBone != null) if (bnList.Contains(S.SkelRootBone) == false) bnList.Add(S.SkelRootBone);
                    ShowBonesSelector("Choose Your Character Model Bone", bnList, rect, false, S);
                }
            }


            #endregion

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(8);

            if (modSet.BonesModificators.Count == 0) GUI.backgroundColor = preGuiC;

            GUILayout.Space(5);

            if (modSet.BonesModificators.Count == 0)
            {
                EditorGUILayout.HelpBox("No Bones Modifiers in '" + TargetClip.name + "' animation clip yet!", MessageType.Info);
            }
            else
            {

                #region Refresh Indexes

                for (int i = 0; i < modSet.BonesModificators.Count; i++)
                {
                    modSet.BonesModificators[i].Index = i;
                }

                #endregion


                #region Modificators Selector and Mod GUI


                StartUndoCheckFor(this, ": Modificator Limbs");
                DrawSelectorGUI(modSet.BonesModificators, ref _sel_mod_index, 18, position.width - 22);
                EndUndoCheckFor(this);

                if (_sel_mod_index >= modSet.BonesModificators.Count) _sel_mod_index = modSet.BonesModificators.Count - 1;


                if (_sel_mod_index > -1)
                {
                    var mod = modSet.BonesModificators[_sel_mod_index];
                    GUILayout.Space(5);

                    EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);

                    StartUndoCheck(" :Modificator");

                    mod.DrawHeaderGUI(modSet.BonesModificators, !sectionFocusMode, ref _sel_mod_index);

                    if (modSet.BonesModificators.ContainsIndex(_sel_mod_index))
                    {

                        if (mod.Foldown)
                        {
                            #region Changing Bone

                            if (Searchable.IsSetted)
                            {
                                if (_SelectorHelperId == "ModChange")
                                {
                                    mod.Transform = Searchable.Get<Transform>();
                                    _SelectorHelperId = "";
                                }
                            }

                            EditorGUILayout.BeginHorizontal();
                            Transform preT = mod.T;
                            Transform newT = (Transform)EditorGUILayout.ObjectField("Modify:", mod.T, typeof(Transform), true);
                            if (preT != newT) { mod.SetBoneTransform(newT); }

                            if (DropdownButton("Change Bone to be modified by Modificator"))
                            {
                                _SelectorHelperId = "ModChange";

                                if (latestAnimator.IsHuman())
                                    ShowHumanoidBonesSelector("Choose Your Character Model Bone", latestAnimator.GetAnimator(), rect, S.GetNonHumanoidBonesList, false, false, false, "Humanoid Bones/", "Other Bones/");
                                else
                                    ShowBonesSelector("Choose Your Character Model Bone", S.GetAllArmatureBonesList, rect);
                            }

                            EditorGUILayout.EndHorizontal();

                            #endregion

                        }

                        mod.DrawTopGUI(animationProgressForEval, _anim_MainSet, _sel_mod_index);
                        mod.DrawParamsGUI(animationProgressForEval, S);
                    }

                    EndUndoCheck();

                    GUILayout.Space(4);
                    EditorGUILayout.EndVertical();

                    GUILayout.Space(5);
                }
                else
                {
                    GUILayout.Space(5);
                    EditorGUILayout.HelpBox("No Modificator Selected", MessageType.Info);
                    GUILayout.Space(5);
                }


                #endregion

            }




            if (sectionFocusMode)
            {
                if (Get)
                {
                    GUILayout.Space(3);
                    EditorGUILayout.BeginHorizontal();
                    DrawPlaybackStopButton();
                    DrawPlaybackButton();
                    GUILayout.Space(5);
                    DrawPlaybackTimeSlider();
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(5);
                }
            }

            EditorGUILayout.EndVertical();

            Modificators_DrawTooltipField();

            EditorGUILayout.BeginVertical(); // To Avoid error for ending vertical

            // Proceeding Removing
            for (int i = modSet.BonesModificators.Count - 1; i >= 0; i--)
                if (modSet.BonesModificators[i].RemoveMe)
                    modSet.BonesModificators.RemoveAt(i);

        }

        #endregion


        #region Gizmos Related

        void _Gizmos_ModsCategory()
        {

            if (_sel_mod_index != -1)
                if (editedModSet != null)
                    if (editedModSet.BonesModificators.ContainsIndex(_sel_mod_index, true))
                    {
                        Handles.color = new Color(0.4f, 0.4f, 0.8f + timeSin01 * 0.2f, 0.8f + timeSin01 * 0.2f);

                        var mod = editedModSet.BonesModificators[_sel_mod_index];
                        mod.DrawGizmos(1f + timeSin01 * 0.5f);


                        // Local position gizmo handle
                        if (mod.Type == ADClipSettings_Modificators.ModificatorSet.EModification.LookAtPosition)
                        {
                            if (mod.alignTo == null)
                            {
                                Vector3 prePos = latestAnimator.transform.TransformPoint(mod.PositionValue);
                                Vector3 newPos = FEditor_TransformHandles.PositionHandle(prePos, Quaternion.identity, 0.1f);
                                if (Vector3.Distance(prePos, newPos) > 0.001f)
                                {
                                    mod.PositionValue = latestAnimator.transform.InverseTransformPoint(newPos);
                                }

                                if (mod.Transform) Handles.DrawDottedLine(mod.Transform.position, prePos, 2f);

                            }
                            else
                            {
                                if (mod.Transform) Handles.DrawDottedLine(mod.Transform.position, mod.alignTo.position, 2f);
                            }
                        }
                        else if (mod.Type == ADClipSettings_Modificators.ModificatorSet.EModification.AdditiveRotation)
                        {

                            if (mod.Transform)
                            {

                                if (Event.current.type == EventType.MouseDown)
                                {
                                    if (_modAdditiveRotStartAngles == null)
                                    {
                                        _modAdditiveRotStartAngles = mod.RotationValue;
                                    }
                                }

                                Quaternion preRot = Quaternion.Euler(mod.RotationValue);// (_modAdditiveRotStartAngles == null ? mod.Transform.rotation : _modAdditiveRotStartAngles.Value) * Quaternion.Euler(mod.RotationValue);

                                float boneLength = 0.1f; // Helper scale for rotation handle
                                if (mod.Transform.parent) boneLength = Vector3.Distance(mod.Transform.position, mod.Transform.parent.position) * 1.0f;

                                bool wasUp = false;
                                if (Event.current.type == EventType.MouseUp) wasUp = true;

                                Color preC = Handles.color;
                                Handles.color = new Color(preC.r, preC.g, preC.b, preC.a * 0.7f);
                                Quaternion newRot = FEditor_TransformHandles.RotationHandle(preRot, mod.Transform.position, Mathf.Max(0.125f, boneLength));
                                Handles.color = new Color(preC.r, preC.g, preC.b, preC.a * 0.7f);

                                if (_modAdditiveRotStartAngles != null)
                                {
                                    float diff = Quaternion.Angle(Quaternion.Euler(_modAdditiveRotStartAngles.Value), newRot);

                                    if (diff > 0.5f)
                                    {
                                        mod.RotationValue = FEngineering.WrapVector(newRot.eulerAngles);
                                        //mod.RotationValue = (FEngineering.QToLocal(_modAdditiveRotStartAngles.Value, newRot).eulerAngles);
                                        //mod.RotationValue = FEngineering.WrapVector( AnimationGenerateUtils.EnsureQuaternionContinuity(_modAdditiveRotStartModRot, FEngineering.QToLocal(_modAdditiveRotStartAngles.Value, newRot)).eulerAngles);

                                        if (Event.current.type == EventType.Used)
                                            if (wasUp)
                                            {
                                                _modAdditiveRotStartAngles = null;
                                            }
                                    }
                                }
                            }

                        }

                    }


        }

        Vector3? _modAdditiveRotStartAngles = null;

        #endregion


        #region Update Loop Related

        void _Update_ModsCategory()
        {

            Modificators_UpdateTooltips();

        }

        #endregion


        #region Tip Field


        float _tip_mods_alpha = 0f;
        string _tip_mods = "";
        float _tip_mods_elapsed = -4f;
        int _tip_mods_index = 0;

        void Modificators_DrawTooltipField()
        {
            if (_tip_mods_alpha > 0f)
            {
                GUI.color = new Color(1f, 1f, 1f, _tip_mods_alpha);
                EditorGUILayout.LabelField(_tip_mods, EditorStyles.centeredGreyMiniLabel, GUILayout.Height(30));
                GUI.color = preGuiC;
            }
            else
                EditorGUILayout.LabelField(" ", EditorStyles.centeredGreyMiniLabel, GUILayout.Height(30));
        }


        void Modificators_UpdateTooltips()
        {
            _tip_mods_elapsed += dt;

            if (_tip_mods == "") Tooltip_CheckModificatorsText();

            if (_tip_mods_elapsed > 0f)
            {
                if (_tip_mods_elapsed < 8f)
                {
                    _tip_mods_alpha = Mathf.Lerp(_tip_mods_alpha, 1f, dt * 3f);
                }
                else
                {
                    _tip_mods_alpha = Mathf.Lerp(_tip_mods_alpha, -0.05f, dt * 6f);
                }

                if (_tip_mods_elapsed > 16f)
                {
                    _tip_mods_elapsed = -4f;
                    _tip_mods_alpha = 0f;
                    Tooltip_CheckModificatorsText();
                }
            }
        }

        void Tooltip_CheckModificatorsText()
        {
            if (_tip_mods_index == 0) _tip_mods = "You can add multiple modifiers for a single bone";
            else if (_tip_mods_index == 1) _tip_mods = "Bone modifiers are working only on single bones - no limbs";
            else if (_tip_mods_index == 2) _tip_mods = "You can add modificator to root bone to generate root motion";
            else if (_tip_mods_index == 3) _tip_mods = "Click RIGHT Mouse Button on the Category Button for Focus Mode";
            else if (_tip_mods_index == 4) _tip_mods = "If you encounter trouble with looping pose,\nuse curve to fade out effects on clip start and clip end";
            else if (_tip_mods_index == 5) _tip_mods = "Rotation Angles Axis reaction depends on character setup\nChange X/Y/Z to high values to learn how your model reacts";

            _tip_mods_index += 1;
            if (_tip_mods_index == 6) _tip_mods_index = 0;
        }

        #endregion


    }

}