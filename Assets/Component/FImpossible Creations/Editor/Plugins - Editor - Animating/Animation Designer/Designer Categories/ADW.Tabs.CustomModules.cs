using FIMSpace.FEditor;
using FIMSpace.Generating;
using System;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.AnimationTools
{
    public partial class AnimationDesignerWindow : EditorWindow
    {

        #region GUI Related

        int _sel_cmodule_index = -1;
        [NonSerialized] ADClipSettings_CustomModules editedCModuleSet;

        void DrawCustomModulesTab()
        {

            if (isReady == false) { EditorGUILayout.HelpBox("First prepare Armature", MessageType.Info); return; }
            if (TargetClip == null) { EditorGUILayout.HelpBox("No AnimationClip to work on!", MessageType.Info); return; }

            GUILayout.Space(3);
            ADClipSettings_CustomModules cModule = S.GetSetupForClip(S.CustomModuleSetupsForClips, TargetClip, _toSet_SetSwitchToHash);
            editedCModuleSet = cModule;


            #region Top GUI View 

            GUILayout.Space(3);
            EditorGUILayout.BeginHorizontal();

            _anim_MainSet.TurnOnModules = EditorGUILayout.Toggle(_anim_MainSet.TurnOnModules, GUILayout.Width(24));
            if (_anim_MainSet.TurnOnModules == false) GUI.enabled = false;

            DrawTargetClipField("Custom Modules For:", true);
            EditorGUILayout.EndHorizontal();

            //GUILayout.Space(4);
            //AnimationDesignerWindow.GUIDrawFloatPercentage(ref cModule.AllModulesBlend, new GUIContent("All Modules Blend"), 6);
            FGUI_Inspector.DrawUILine(0.3f, 0.5f, 1, 7, 0.975f);

            if (cModule.CustomModules.Count == 0) GUI.backgroundColor = preGuiC;


            #endregion


            if (cModule.CustomModules.Count == 0)
            {
                EditorGUILayout.HelpBox("No Selected Modules for '" + TargetClip.name + "' animation clip yet!", MessageType.Info);
            }
            else
            {
                #region Refresh Indexes

                for (int i = 0; i < cModule.CustomModules.Count; i++)
                {
                    cModule.CustomModules[i].Index = i;
                }

                #endregion

                DrawSelectorGUI(cModule.CustomModules, ref _sel_cmodule_index, 18, position.width - 22, -1, true);

                if (_sel_cmodule_index >= cModule.CustomModules.Count) _sel_cmodule_index = cModule.CustomModules.Count - 1;

                // Selecting module
                if (_sel_cmodule_index > -1)
                {
                    var moduleIns = cModule.CustomModules[_sel_cmodule_index];
                    GUILayout.Space(5);

                    EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);

                    if (moduleIns.ModuleReference)
                        moduleIns.ModuleReference.baseSerializedObject.Update();

                    EditorGUI.BeginChangeCheck();

                    moduleIns.DrawHeaderGUI(cModule.CustomModules, !sectionFocusMode);

                    moduleIns.DrawTopGUI(animationProgress);

                    moduleIns.DrawParamsGUI(animationProgress, _anim_MainSet, S, cModule);

                    if (EditorGUI.EndChangeCheck())
                    {
                        S._SetDirty();

                        if (moduleIns.ModuleReference)
                            moduleIns.ModuleReference.baseSerializedObject.ApplyModifiedProperties();
                    }

                    GUILayout.Space(4);
                    EditorGUILayout.EndVertical();

                    GUILayout.Space(5);
                }
                else
                {
                    GUILayout.Space(5);
                    EditorGUILayout.HelpBox("No Module Selected", MessageType.Info);
                    GUILayout.Space(5);
                }

            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(); // To Avoid error for ending vertical

            // Proceeding Removing
            for (int i = cModule.CustomModules.Count - 1; i >= 0; i--)
                if (cModule.CustomModules[i].RemoveMe)
                    cModule.CustomModules.RemoveAt(i);


            GUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();
            GUIContent c = new GUIContent(" +  Add Module Processor  + ");
            var rect = GUILayoutUtility.GetRect(c, EditorStyles.miniButton);
            rect.height = 22;


            #region Module Add Button 

            if (cModule.CustomModules.Count == 0) GUI.backgroundColor = Color.green;

            if (GUI.Button(rect, c))
            {
                rect.width = Mathf.Min(350, rect.width);
                cModule.AddNewModule(S);

                if (_sel_cmodule_index == -1)
                {
                    if (cModule.CustomModules.Count >= 1) _sel_cmodule_index = cModule.CustomModules.Count - 1;
                }
                else
                {
                    _sel_cmodule_index = cModule.CustomModules.Count - 1;
                    if (_sel_cmodule_index >= cModule.CustomModules.Count) _sel_cmodule_index = -1;
                }
            }

            GUI.backgroundColor = preBG;

            #endregion

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(9);
            CustomModules_DrawTooltipField();

        }

        #endregion


        #region Gizmos Related

        void _Gizmos_ModulesCategory()
        {
            if (_sel_cmodule_index != -1)
                if (editedCModuleSet != null)
                    if (editedCModuleSet.CustomModules.ContainsIndex(_sel_cmodule_index, true))
                    {
                        Handles.color = new Color(0.4f, 0.8f + timeSin01 * 0.2f, 0.4f - timeSin01 * 0.1f, 0.8f + timeSin01 * 0.2f);
                        editedCModuleSet.CustomModules[_sel_cmodule_index].DrawSceneHandles(1f + timeSin01 * 0.5f, animationProgress);
                    }

        }

        #endregion


        #region Update Loop Related

        void _Update_ModulesCategory()
        {

            CustomModules_UpdateTooltips();

        }

        #endregion


        #region Tip Field


        float _tip_cmoduls_alpha = 0f;
        string _tip_cmoduls = "";
        float _cmoduls_elapsed = -4f;
        int _tip_cmoduls_index = 0;

        void CustomModules_DrawTooltipField()
        {
            if (_tip_cmoduls_alpha > 0f)
            {
                GUI.color = new Color(1f, 1f, 1f, _tip_cmoduls_alpha);
                EditorGUILayout.LabelField(_tip_cmoduls, EditorStyles.centeredGreyMiniLabel, GUILayout.Height(28));
                GUI.color = preGuiC;
            }
            else
                EditorGUILayout.LabelField(" ", EditorStyles.centeredGreyMiniLabel);
        }


        void CustomModules_UpdateTooltips()
        {
            _cmoduls_elapsed += dt;

            if (_tip_cmoduls == "") Tooltip_CheckCustomModulesText();

            if (_cmoduls_elapsed > 0f)
            {
                if (_cmoduls_elapsed < 8f)
                {
                    _tip_cmoduls_alpha = Mathf.Lerp(_tip_cmoduls_alpha, 1f, dt * 3f);
                }
                else
                {
                    _tip_cmoduls_alpha = Mathf.Lerp(_tip_cmoduls_alpha, -0.05f, dt * 6f);
                }

                if (_cmoduls_elapsed > 16f)
                {
                    _cmoduls_elapsed = -4f;
                    _tip_cmoduls_alpha = 0f;
                    Tooltip_CheckCustomModulesText();
                }
            }
        }

        void Tooltip_CheckCustomModulesText()
        {
            if (_tip_cmoduls_index == 0) _tip_cmoduls = "Try usign Custom coded Modules for extra\nanimating features or create your own.";
            else if (_tip_cmoduls_index == 1) _tip_cmoduls = "Modules like strafer creator can change\nwalk forward animation into strafe animation";

            _tip_cmoduls_index += 1;
            if (_tip_cmoduls_index == 2) _tip_cmoduls_index = 0;
        }

        #endregion

    }
}