//using FIMSpace.FEditor;
//using FIMSpace.Generating;
//using System;
//using UnityEditor;
//using UnityEngine;

//namespace FIMSpace.AnimationTools
//{
//    public partial class AnimationDesignerWindow : EditorWindow
//    {

//        #region GUI Related

//        int _sel_spring_index = -1;
//        [NonSerialized] ADClipSettings_Springs editedSpringSet;

//        void DrawSpringsTab()
//        {

//            if (isReady == false) { EditorGUILayout.HelpBox("First prepare Armature", MessageType.Info); return; }
//            if (TargetClip == null) { EditorGUILayout.HelpBox("No AnimationClip to work on!", MessageType.Info); return; }

//            GUILayout.Space(3);
//            ADClipSettings_Springs springSet = S.GetSetupForClip(S.SpringSetupsForClips, TargetClip, _toSet_SetSwitchToHash);
//            editedSpringSet = springSet;


//            GUILayout.Space(3);
//            EditorGUILayout.BeginHorizontal();
//            DrawTargetClipField("Bone Springs For:", true);
//            EditorGUILayout.EndHorizontal();

//            GUILayout.Space(4);
//            AnimationDesignerWindow.GUIDrawFloatPercentage(ref springSet.AllSpringsBlend, new GUIContent("All Springs Blend"));
//            FGUI_Inspector.DrawUILine(0.3f, 0.5f, 1, 14, 0.975f);

//            #region Adding Spring


//            #endregion

//            EditorGUILayout.BeginHorizontal();
//            GUIContent c = new GUIContent(" +  Add Spring for a Bone  + ");
//            var rect = GUILayoutUtility.GetRect(c, EditorStyles.miniButton);
//            rect.height = 24;

//            if (springSet.Springs.Count == 0) GUI.backgroundColor = Color.green;

//            #region Bone Add Button 

//            if (GUI.Button(rect, c))
//            {
//                rect.width = Mathf.Min(350, rect.width);

//                _SelectorHelperId = "Spring";

//                if (latestAnimator.IsHuman())
//                    ShowHumanoidBonesSelector("Choose Your Character Model Bone", latestAnimator.GetAnimator(), rect, S.GetNonHumanoidBonesList, false, false, false, "Humanoid Bones/", "Other Bones/");
//                else
//                    ShowBonesSelector("Choose Your Character Model Bone", S.GetAllArmatureBonesList, rect);
//            }

            
//            #endregion

//            EditorGUILayout.EndHorizontal();

//            GUILayout.Space(8);

//            if (springSet.Springs.Count == 0) GUI.backgroundColor = preGuiC;

//            GUILayout.Space(5);

//            if (springSet.Springs.Count == 0)
//            {
//                EditorGUILayout.HelpBox("No Bones Springs in '" + TargetClip.name + "' animation clip yet!", MessageType.Info);
//            }
//            else
//            {

//                #region Refresh Indexes

//                for (int i = 0; i < springSet.Springs.Count; i++)
//                {
//                    springSet.Springs[i].Index = i;
//                }

//                #endregion


//                #region Springificators Selector and Spring GUI

//                DrawSelectorGUI(springSet.Springs, ref _sel_spring_index, 18, position.width - 22);

//                if (_sel_spring_index >= springSet.Springs.Count) _sel_spring_index = springSet.Springs.Count - 1;

//                if (_sel_spring_index > -1)
//                {
//                    var spring = springSet.Springs[_sel_spring_index];
//                    GUILayout.Space(5);

//                    EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);

//                    spring.DrawHeaderGUI(springSet.Springs, !sectionFocusMode);

//                    if (spring.Foldown)
//                    {
//                        #region Changing Bone

//                        if (Searchable.IsSetted)
//                        {
//                            if (_SelectorHelperId == "SpringChange")
//                            {
//                                spring.Transform = Searchable.Get<Transform>();
//                                _SelectorHelperId = "";
//                            }
//                        }

//                        EditorGUILayout.BeginHorizontal();
//                        EditorGUILayout.ObjectField("Spring for:", spring.T, typeof(Transform), true);

//                        if (DropdownButton("Change Bone for the Spring"))
//                        {
//                            _SelectorHelperId = "SpringChange";
                            
//                            if (latestAnimator.IsHuman())
//                                ShowHumanoidBonesSelector("Choose Your Character Model Bone", latestAnimator.GetAnimator(), rect, S.GetNonHumanoidBonesList, false, false, false, "Humanoid Bones/", "Other Bones/");
//                            else
//                                ShowBonesSelector("Choose Your Character Model Bone", S.GetAllArmatureBonesList, rect);
//                        }

//                        EditorGUILayout.EndHorizontal();

//                        #endregion

//                    }

//                    spring.DrawTopGUI();


//                    #region Curve progress bar

//                    float ghostMul = 0f;

//                    if (drawModsGizmos)
//                    {
//                        Rect r = GUILayoutUtility.GetLastRect();
//                        r.position += new Vector2(152, 0);
//                        r.width -= 152;
//                        float startWidth = r.width - 60;
//                        r.width = r.width * animationProgress;

//                        float alph = 1f;
//                        if (animationProgress < 0.1f) alph = (animationProgress / 0.1f) * 0.4f;
//                        else if (animationProgress < 0.9f) alph = 0.4f; else alph = ((1 - animationProgress) / 0.1f) * 0.4f;
//                        GUI.color = new Color(0.4f, 1f, 0.4f, alph);
//                        GUI.Box(r, GUIContent.none);

//                        Rect sr = new Rect(r);
//                        sr.width = startWidth;
//                        sr.position -= new Vector2(0, EditorGUIUtility.singleLineHeight + 2);
//                        GUI.color = new Color(1f, 1f, 1f, 0.07f);

//                        ghostMul = spring.Blend * spring.BlendEvaluation.Evaluate(animationProgress);
//                        GUI.HorizontalSlider(sr, ghostMul, 0f, 1f);

//                        GUI.color = preGuiC;
//                    }

//                    #endregion


//                    spring.DrawParamsGUI(ghostMul);


//                    GUILayout.Space(4);
//                    EditorGUILayout.EndVertical();

//                    GUILayout.Space(5);
//                }
//                else
//                {
//                    GUILayout.Space(5);
//                    EditorGUILayout.HelpBox("No Spring Selected", MessageType.Info);
//                    GUILayout.Space(5);
//                }


//                #endregion

//            }

//            EditorGUILayout.EndVertical();

//            Springificators_DrawTooltipField();

//            EditorGUILayout.BeginVertical(); // To Avoid error for ending vertical

//            // Proceeding Removing
//            for (int i = springSet.Springs.Count - 1; i >= 0; i--)
//                if (springSet.Springs[i].RemoveMe)
//                    springSet.Springs.RemoveAt(i);

//        }

//        #endregion


//        #region Gizmos Related

//        void _Gizmos_SpringsCategory()
//        {

//            if (_sel_spring_index != -1)
//                if (editedSpringSet != null)
//                    if (editedSpringSet.Springs.ContainsIndex(_sel_spring_index, true))
//                    {
//                        Handles.color = new Color(0.4f, 0.8f + timeSin01 * 0.2f, 0.4f - timeSin01 * 0.1f, 0.8f + timeSin01 * 0.2f);

//                        editedSpringSet.Springs[_sel_spring_index].DrawGizmos(1f + timeSin01 * 0.5f);
//                    }

//        }

//        #endregion


//        #region Update Loop Related

//        void _Update_SpringsCategory()
//        {

//            Springificators_UpdateTooltips();

//        }

//        #endregion


//        #region Tip Field


//        float _tip_springs_alpha = 0f;
//        string _tip_springs = "";
//        float _tip_springs_elapsed = -4f;
//        int _tip_springs_index = 0;

//        void Springificators_DrawTooltipField()
//        {
//            if (_tip_springs_alpha > 0f)
//            {
//                GUI.color = new Color(1f, 1f, 1f, _tip_springs_alpha);
//                EditorGUILayout.LabelField(_tip_springs, EditorStyles.centeredGreyMiniLabel, GUILayout.Height(28));
//                GUI.color = preGuiC;
//            }
//            else
//                EditorGUILayout.LabelField(" ", EditorStyles.centeredGreyMiniLabel);
//        }


//        void Springificators_UpdateTooltips()
//        {
//            _tip_springs_elapsed += dt;

//            if (_tip_springs == "") Tooltip_CheckSpringificatorsText();

//            if (_tip_springs_elapsed > 0f)
//            {
//                if (_tip_springs_elapsed < 8f)
//                {
//                    _tip_springs_alpha = Mathf.Lerp(_tip_springs_alpha, 1f, dt * 3f);
//                }
//                else
//                {
//                    _tip_springs_alpha = Mathf.Lerp(_tip_springs_alpha, -0.05f, dt * 6f);
//                }

//                if (_tip_springs_elapsed > 16f)
//                {
//                    _tip_springs_elapsed = -4f;
//                    _tip_springs_alpha = 0f;
//                    Tooltip_CheckSpringificatorsText();
//                }
//            }
//        }

//        void Tooltip_CheckSpringificatorsText()
//        {
//            if (_tip_springs_index == 0) _tip_springs = "You can try using springs for hit reactions";
//            else if (_tip_springs_index == 1) _tip_springs = "Spring is very useful for animating pelvis bone with IK";

//            _tip_springs_index += 1;
//            if (_tip_springs_index == 2) _tip_springs_index = 0;
//        }

//        #endregion


//    }

//}