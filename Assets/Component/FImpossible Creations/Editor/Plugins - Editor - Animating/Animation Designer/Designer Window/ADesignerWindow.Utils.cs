using FIMSpace.FEditor;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace FIMSpace.AnimationTools
{
    public partial class AnimationDesignerWindow : EditorWindow
    {

        #region Helper utils


        bool _utilTip_humanoidExportInfoTipDispl = false;
        bool _utilTip_displayArmSetupInfo = false;
        float _utilTip_humanoidExportInfoTipElapsed = 0f;

        void UtilsUpdate()
        {
            if (_utilTip_humanoidExportInfoTipDispl) _utilTip_humanoidExportInfoTipElapsed += dt;
        }


        #endregion


        #region GUI Utils



        /// <summary> Use code like: AddEditorEvent(() => { your code });</summary>
        public static void AddEditorEvent(System.Action ac)
        {
            EditorEvents.Add(ac);
        }

        static List<System.Action> EditorEvents = new List<System.Action>();
        public static void UseEditorEvents()
        {
            try
            {
                for (int i = 0; i < EditorEvents.Count; i++)
                {
                    if (EditorEvents[i] != null) EditorEvents[i].Invoke();
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.Log("[Animation Designer Editor Events] Some error occured when executing editor event!");
                UnityEngine.Debug.LogException(e);
            }

            EditorEvents.Clear();
        }


        public void AddDampReferencesEvent(bool preDamp = false)
        {
            preDamp = false;
            if (preDamp) if (ProjectFileSave) ProjectFileSave.DampSessionSkeletonReferences();

            AddEditorEvent(() =>
            {
                if (preDamp == false) if (ProjectFileSave) ProjectFileSave.DampSessionSkeletonReferences();
                _switchingReferences = true;
                ProjectFileSave = null;
                _latest_DisplaySave = null;
                _latest_ProjectFileSave = null;
                _latest_SceneObj = null;
                latestAnimator = null;
                _validated_anim = null;
                _toSet_triedLoadFromDrafts = null;
                _serializationChanges = true;
            });
        }


        void GizmosSwitchButton()
        {
            Texture gizm = FGUI_Resources.Tex_Gizmos;
            if (drawGizmos == false) gizm = FGUI_Resources.Tex_GizmosOff;
            if (GUILayout.Button(new GUIContent(gizm, "Switching displaying skeleton gizmos on the scene"), FGUI_Resources.ButtonStyle, GUILayout.Width(24), GUILayout.Height(20))) drawGizmos = !drawGizmos;
        }


        void FocusModeSwitchButton()
        {
            Texture gizm = FGUI_Resources.Tex_AB;

            int wid = 24;
            if (sectionFocusMode)
            {
                GUI.backgroundColor = new Color(0.2f, 1f, 0.2f, 1f);
                GUI.color = new Color(0.8f, 1f, 0.8f, 1f);
                wid = 50;
            }

            if (GUILayout.Button(new GUIContent(gizm, "Hiding some gui panels to focus on the design process"), FGUI_Resources.ButtonStyle, GUILayout.Width(wid), GUILayout.Height(20))) sectionFocusMode = !sectionFocusMode;
            GUI.color = preGuiC;
            GUI.backgroundColor = preBG;
        }


        void GizmosModsButton()
        {
            Texture gizm = FGUI_Resources.Tex_Bone;
            if (drawModsGizmos == false) GUI.color = preGuiC * 0.7f;
            if (GUILayout.Button(new GUIContent(gizm, "Switching displaying bone modificators gizmos on the scene and GUI"), FGUI_Resources.ButtonStyle, GUILayout.Width(24), GUILayout.Height(20))) drawModsGizmos = !drawModsGizmos;
            GUI.color = preGuiC;
        }


        public static void DrawSelectorGUI<T>(List<T> elements, ref int selection, float viewPaddingWidth = 18, float? maxWidth = null, int? deselectIndex = -1, bool addOption = false) where T : INameAndIndex, new()
        {
            Color preC = GUI.backgroundColor;

            float cWidth = viewPaddingWidth;
            float width = maxWidth == null ? EditorGUIUtility.currentViewWidth : maxWidth.Value;

            GUILayout.BeginHorizontal();
            int column = 0;
            bool drawnAdd = false;

            for (int l = 0; l < elements.Count; l++)
            {
                var toDraw = elements[l];

                GUIContent c = new GUIContent(toDraw.GetName);
                Vector2 guiSize = EditorStyles.miniButton.CalcSize(c);

                float maxWdth = width;
                if (addOption && column == 0) maxWdth -= 22;

                if (cWidth + guiSize.x + 8 > width)
                {
                    if (addOption && column == 0)
                    {
                        drawnAdd = true;
                        GUILayout.FlexibleSpace();
                        GUI.backgroundColor = new Color(0.7f, 1f, 0.8f, 1f);
                        if (GUILayout.Button("+", GUILayout.Width(20))) { elements.Add(new T()); }
                        GUI.backgroundColor = preC;
                    }

                    cWidth = viewPaddingWidth + guiSize.x + 8;
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    column += 1;
                }
                else
                {
                    cWidth += guiSize.x + 8;
                }

                if (selection == toDraw.GetIndex) GUI.backgroundColor = new Color(0f, 1f, 0f, toDraw.GUIAlpha);
                else GUI.backgroundColor = new Color(1f, 1f, 1f, toDraw.GUIAlpha);

                if (GUILayout.Button(c, GUILayout.Width(guiSize.x + 8)))
                {
                    if (selection == toDraw.GetIndex)
                    {
                        if (deselectIndex != null) selection = deselectIndex.Value;
                    }
                    else
                    {
                        EditorGUI.FocusTextInControl("");
                        selection = toDraw.GetIndex;
                    }
                }
            }

            if (addOption && column == 0 && !drawnAdd)
            {
                GUILayout.FlexibleSpace();
                GUI.backgroundColor = new Color(0.7f, 1f, 0.8f, 1f);

                if (GUILayout.Button("+", GUILayout.Width(20)))
                {
                    elements.Add(new T());
                    selection = elements.Count - 1;
                }

                GUI.backgroundColor = preC;
            }

            GUILayout.EndHorizontal();

            GUI.backgroundColor = preC;
        }

        static bool _expandCurves = false;
        static GUIContent _curvGC = null;
        public static void DrawCurve(ref AnimationCurve curve, string label = "", int width = 0, float startTime = 0f, float startValue = 0f, float endTime = 1f, float endValue = 1f, float r = 0f, float g = 1f, float b = 1f, float a = 1f, string tooltip = "")
        {
            if (curve == null) curve = new AnimationCurve();

            if (_curvGC == null) _curvGC = new GUIContent();
            _curvGC.text = label;
            _curvGC.tooltip = tooltip;

            //bool undo = false;
            //if (Get) if (Get.EnableExperimentalUndo) undo = true;
            //if (undo) Get.StartUndoCheckFor(curve, ": Curve");

            #region Loop curve with dhift+right mouse button

            if (Get)
            {
                if (Event.current != null)
                    if (Event.current.type == EventType.MouseDown)
                    {

                        if (Event.current.button == 0)
                        {
                            if (Event.current.alt)
                            {
                                Rect clickR = GUILayoutUtility.GetLastRect();
                                clickR.position += new Vector2(180, 0); //GUI.Box(clickR, new GUIContent("AAAAA"), Get._style_horScroll);

                                if (clickR.Contains(Event.current.mousePosition))
                                {
                                    Keyframe key = new Keyframe();
                                    float cTime = Get.animationProgress;
                                    key.value = curve.Evaluate(cTime);
                                    key.time = cTime;
                                    curve.AddKey(key);
                                    Event.current.Use();
                                }
                            }
                            else
                            if (Event.current.shift)
                            {
                                Rect clickR = GUILayoutUtility.GetLastRect();
                                clickR.position += new Vector2(180, 0); //GUI.Box(clickR, new GUIContent("AAAAA"), Get._style_horScroll);

                                if (clickR.Contains(Event.current.mousePosition))
                                {
                                    AnimationGenerateUtils.LoopCurve(ref curve, Event.current.button == 1, endTime);
                                    Event.current.Use();
                                }
                            }
                            else if (Event.current.control)
                            {
                                Rect clickR = GUILayoutUtility.GetLastRect();
                                clickR.position += new Vector2(180, 0); //GUI.Box(clickR, new GUIContent("AAAAA"), Get._style_horScroll);

                                if (clickR.Contains(Event.current.mousePosition))
                                {

                                    if (curve.keys.Length > 10)
                                        curve = AnimationGenerateUtils.ReduceKeyframes(curve, 0.08f);
                                    if (curve.keys.Length > 5)
                                        curve = AnimationGenerateUtils.ReduceKeyframes(curve, 0.05f);
                                    else
                                        curve = AnimationGenerateUtils.ReduceKeyframes(curve, 0.01f);

                                    Event.current.Use();
                                }
                            }
                        }
                        else if (Event.current.button == 1)
                        {
                            if (Event.current.shift || Event.current.control) // Move keys
                            {
                                Rect clickR = GUILayoutUtility.GetLastRect();
                                clickR.position += new Vector2(180, 0); //GUI.Box(clickR, new GUIContent("AAAAA"), Get._style_horScroll);

                                if (clickR.Contains(Event.current.mousePosition))
                                {
                                    for (int i = 0; i < curve.keys.Length; i++)
                                    {
                                        var k = curve.keys[i];
                                        k.time += Event.current.shift ? 0.05f : -0.05f;
                                        if (k.time > 1f) k.time -= 1f;
                                        curve.MoveKey(i, k);
                                    }
                                }
                            }
                        }
                    }

            }

            #endregion


            if (width < 1)
            {
                if (string.IsNullOrEmpty(label))
                    curve = EditorGUILayout.CurveField(curve, new Color(r, g, b, a), new Rect(startTime, startValue, endTime - startTime, endValue - startValue));
                else
                    curve = EditorGUILayout.CurveField(_curvGC, curve, new Color(r, g, b, a), new Rect(startTime, startValue, endTime - startTime, endValue - startValue));
            }
            else
            {
                if (string.IsNullOrEmpty(label))
                    curve = EditorGUILayout.CurveField(curve, new Color(r, g, b, a), new Rect(startTime, startValue, endTime - startTime, endValue - startValue), GUILayout.Width(width));
                else
                    curve = EditorGUILayout.CurveField(_curvGC, curve, new Color(r, g, b, a), new Rect(startTime, startValue, endTime - startTime, endValue - startValue), GUILayout.Width(width));
            }

            if (_expandCurves)
            {

            }

            if (Get)
            {
                Rect clickR2 = GUILayoutUtility.GetLastRect();
                clickR2.position += new Vector2(clickR2.width, 0);
                //clickR2.position += new Vector2(16, 0);
                clickR2.width = 10;
                Get.CurveOptionsButton(ref curve, clickR2, startTime, endTime);
            }

        }

        void CurveOptionsButton(ref AnimationCurve curve, Rect r, float startTime, float endTime)
        {

            if (_latestModCurveHash != -1)
                if (_latestModCurveToApply != null)
                    if (curve.GetHashCode() == _latestModCurveHash)
                    {
                        curve.keys = _latestModCurveToApply.keys;
                        _latestModCurveHash = -1;
                    }


            if (GUI.Button(r, FGUI_Resources.GUIC_More, EditorStyles.label))
            {
                AnimationCurve cu = CopyCurve(curve);
                int hash = curve.GetHashCode();

                GenericMenu menu = new GenericMenu();

                menu.AddItem(new GUIContent("Add Key in current animation time (ALT+LMB)"), false, () =>
                {
                    Keyframe key = new Keyframe();
                    float cTime = Get.animationProgress;
                    key.value = cu.Evaluate(cTime);
                    key.time = cTime;
                    cu.AddKey(key);
                    _latestModCurveToApply = cu;
                    _latestModCurveHash = hash;
                });

                //menu.AddItem(new GUIContent("Reverse Curve"), false, () =>
                //{
                //    AnimationGenerateUtils.LoopCurve(ref cu, Event.current.button == 1, endTime);
                //    _latestModCurveToApply = cu;
                //});

                menu.AddItem(new GUIContent("Loop Curve (SHIFT+LMB)"), false, () =>
                {
                    AnimationGenerateUtils.LoopCurve(ref cu, true, endTime);
                    _latestModCurveToApply = cu;
                    _latestModCurveHash = hash;
                });


                menu.AddItem(new GUIContent(""), false, () => { });

                menu.AddItem(new GUIContent("Reset To value 0"), false, () =>
                {
                    for (int k = 0; k < cu.keys.Length; k++) cu.MoveKey(k, new Keyframe(cu.keys[k].time, 0f));
                    _latestModCurveToApply = cu;
                    _latestModCurveHash = hash;
                });

                menu.AddItem(new GUIContent("Reset To value 1"), false, () =>
                {
                    for (int k = 0; k < cu.keys.Length; k++) cu.MoveKey(k, new Keyframe(cu.keys[k].time, 1f));
                    _latestModCurveToApply = cu;
                    _latestModCurveHash = hash;
                });

                menu.AddItem(new GUIContent("Set linear from 0 to 1"), false, () =>
                {
                    cu = AnimationCurve.Linear(0f, 0f, 1f, 1f);
                    _latestModCurveToApply = cu;
                    _latestModCurveHash = hash;
                });

                menu.AddItem(new GUIContent(""), false, () => { });


                menu.AddItem(new GUIContent("Reverse Time"), false, () =>
                {
                    AnimationCurve helpC = CopyCurve(cu);

                    int ind = 0;
                    for (int i = helpC.keys.Length - 1; i >= 0; i--)
                    {
                        var cKey = helpC.keys[i];
                        cu.MoveKey(ind, new Keyframe(cKey.time, cKey.value));
                        ind += 1;
                    }

                    _latestModCurveToApply = cu;
                    _latestModCurveHash = hash;
                });


                menu.AddItem(new GUIContent("Iverse Values"), false, () =>
                {
                    AnimationCurve helpC = CopyCurve(cu);

                    for (int i = 0; i < helpC.keys.Length; i++)
                    {
                        var cKey = helpC.keys[i];
                        cu.MoveKey(i, new Keyframe(cKey.time, 1f - cKey.value));
                    }

                    _latestModCurveToApply = cu;
                    _latestModCurveHash = hash;
                });


                menu.AddItem(new GUIContent(""), false, () => { });


                menu.AddItem(new GUIContent("Reduce Keys (Simplify : CTRL+LMB)"), false, () =>
                {
                    if (cu.keys.Length > 10)
                        cu = AnimationGenerateUtils.ReduceKeyframes(cu, 0.08f);
                    if (cu.keys.Length > 5)
                        cu = AnimationGenerateUtils.ReduceKeyframes(cu, 0.05f);
                    else
                        cu = AnimationGenerateUtils.ReduceKeyframes(cu, 0.01f);

                    _latestModCurveToApply = cu;
                    _latestModCurveHash = hash;
                });


                menu.AddItem(new GUIContent(""), false, () => { });

                menu.AddItem(new GUIContent("Copy Curve"), false, () =>
                {
                    _latestModCurveCopy = CopyCurve(cu);
                });

                if (_latestModCurveCopy != null)
                {
                    menu.AddItem(new GUIContent("Paste Curve"), false, () =>
                    {
                        _latestModCurveToApply = CopyCurve(_latestModCurveCopy);
                        _latestModCurveHash = hash;
                        //_latestModCurveCopy = null;
                    });
                }


                menu.ShowAsContext();
            }
        }

        static int _latestModCurveHash = -1;
        static AnimationCurve _latestModCurveCopy = null;
        static AnimationCurve _latestModCurveToApply = null;

        public static Rect DrawCurveProgressOnR(float progr, float rOffset = 150f, float widthCorrByFieldAndPadding = 60f, Rect? rect = null, float yOff = 0f, float linesUp = 0)
        {
            return DrawCurveProgress(progr, Get.position.width - rOffset, widthCorrByFieldAndPadding, rect, yOff, linesUp);
        }

        public static Rect DrawCurveProgress(float progr, float xOffByLabel = 150f, float widthCorrByFieldAndPadding = 60f, Rect? rect = null, float yOff = 0f, float linesUp = 0)
        {
            if (!drawGUIGosting) return new Rect();

            Color preC = GUI.color;

            Rect r = rect == null ? GUILayoutUtility.GetLastRect() : rect.Value;
            Rect initRect = r;

            r.position += new Vector2(xOffByLabel, 0);
            r.width -= xOffByLabel;
            float startWidth = r.width - widthCorrByFieldAndPadding;
            r.width = r.width * progr;
            r.position -= new Vector2(0, EditorGUIUtility.singleLineHeight * linesUp + yOff);

            float alph = 1f;
            if (progr < 0.04f) alph = (progr / 0.04f) * 0.4f;
            else if (progr < 0.96f) alph = 0.4f; else alph = ((1 - progr) / 0.04f) * 0.4f;
            GUI.color = new Color(0.4f, 1f, 0.4f, alph);
            GUI.Box(r, GUIContent.none);

            GUI.color = preC;
            return initRect;
        }


        public static Rect DrawSliderProgress(float progr, float xOffByLabel = 150f, float widthCorrByFieldAndPadding = 58f, Rect? rect = null, float yOff = 0f, float linesUp = 0)
        {
            if (!drawGUIGosting) return new Rect();

            Color preC = GUI.color;

            Rect r = rect == null ? GUILayoutUtility.GetLastRect() : rect.Value;
            Rect initRect = r;

            r.position += new Vector2(xOffByLabel, 0);
            r.width -= xOffByLabel + widthCorrByFieldAndPadding;
            r.position -= new Vector2(0, EditorGUIUtility.singleLineHeight * linesUp + yOff);
            GUI.color = new Color(1f, 1f, 1f, 0.15f);

            GUI.HorizontalSlider(r, progr, 0f, 1f);
            GUI.color = preC;

            return initRect;
        }


        public static void DrawNullTweakGUI(ADArmatureLimb limb, int limbIndex)
        {
            if (Get == null) return;

            if (limb.HaveNullBones())
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox("Your limb have undefined bones! It will not animate!", MessageType.Error);
                if (GUILayout.Button(" Go Tweak It ", GUILayout.Height(36))) { AddEditorEvent(() => { Get.Category = ECategory.Setup; Get._sel_SetupDisplayLimb = limbIndex; }); }
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(3);
            }
        }

        public static void LogCurve(AnimationCurve c)
        {
            if (c == null) return;

            string report = "Curve.Length = " + c.length;

            for (int i = 0; i < c.length; i++)
            {
                var key = c.keys[i];
                report += "\n";
                report += "[" + i + "] time: " + key.time + "  value: " + key.value;
                report += "  inTan: " + key.inTangent + "  outTan: " + key.outTangent;
                report += "  inWght: " + key.inWeight + "  outWght: " + key.outWeight;
                report += "  mode: " + key.weightedMode;
            }

            UnityEngine.Debug.Log(report);
        } 


        #endregion


        #region Classes Utils

        public static float GetAmplified02Range(float enterValue, float amplifyAfter1UpTo2 = 5f)
        {
            if (enterValue <= 1f) return enterValue;
            float a = (1f - enterValue);
            return enterValue + a * amplifyAfter1UpTo2;
        }

        public static AnimationCurve GetExampleCurve(float startTime = 0f, float endTime = 1f, float startValue = 0f, float midValues = 0.75f, float endValue = 0f)
        {
            AnimationCurve c = new AnimationCurve();

            c.AddKey(startTime, startValue);

            c.AddKey(Mathf.Lerp(startTime, endTime, 0.25f), midValues);
            c.AddKey(Mathf.Lerp(startTime, endTime, 0.75f), midValues * 1f);

            c.AddKey(endTime, endValue);

            c.SmoothTangents(0, 0.5f);
            c.SmoothTangents(1, 0.5f);
            c.SmoothTangents(2, 0.5f);
            c.SmoothTangents(3, 0.5f);

            return c;
        }




        internal static AnimationCurve CopyCurve(AnimationCurve blendEvaluation)
        {
            if (blendEvaluation == null) return null;
            Keyframe[] evalKeys = new Keyframe[blendEvaluation.keys.Length];
            blendEvaluation.keys.CopyTo(evalKeys, 0);
            return new AnimationCurve(evalKeys);
        }


        #endregion


        #region Animator Get Clips Utils


        public static List<AnimationClip> GetAllClipsFrom(Animator anim)
        {
            List<AnimationClip> ClipDatas = new List<AnimationClip>();
            if (anim == null) return ClipDatas;

            AnimatorController animContr = (AnimatorController)anim.runtimeAnimatorController;
            if (animContr == null) return ClipDatas;

            for (int l = 0; l < animContr.layers.Length; l++)
            {
                ChildAnimatorState[] animatorStates = animContr.layers[l].stateMachine.states;

                for (int i = 0; i < animatorStates.Length; i++)
                {
                    AnimatorState animState = animatorStates[i].state;
                    if (animState.motion == null) continue;

                    BlendTree bt = animState.motion as BlendTree;

                    if (bt == null)
                    {
                        AnimationClip clip = GetAnimationClip(anim.runtimeAnimatorController, animState.motion.name);
                        if (clip == null) continue;
                        if (ClipDatas.Contains(clip) == false) ClipDatas.Add(clip);
                    }
                    else
                    {
                        var bClips = GetAllClipsFromBlendTree(anim, bt);
                        for (int b = 0; b < bClips.Count; b++)
                        {
                            if (bClips[b] == null) continue;
                            if (ClipDatas.Contains(bClips[b]) == false) ClipDatas.Add(bClips[b]);
                        }
                    }

                }


            }

            return ClipDatas;
        }

        public static AnimationClip GetAnimationClip(RuntimeAnimatorController animator, string name)
        {
            if (animator == null) return null;
            foreach (var clip in animator.animationClips) if (clip.name == name) return clip;
            return null;
        }


        /// <summary>
        /// Get recursive clips from all sub blend trees
        /// </summary>
        public static List<AnimationClip> GetAllClipsFromBlendTree(Animator anim, BlendTree blendTree)
        {
            List<AnimationClip> clips = new List<AnimationClip>();
            if (anim == null) return clips;
            if (anim.runtimeAnimatorController == null) return clips;
            if (blendTree == null) return clips;

            for (int i = 0; i < blendTree.children.Length; i++)
            {
                if (blendTree.children[i].motion == null) continue;

                BlendTree chBlendTree = blendTree.children[i].motion as BlendTree;
                if (chBlendTree != null)
                {
                    List<AnimationClip> chClips = GetAllClipsFromBlendTree(anim, chBlendTree);
                    foreach (var clp in chClips) clips.Add(clp);
                }
                else
                {
                    clips.Add(GetAnimationClip(anim.runtimeAnimatorController, blendTree.children[i].motion.name));
                }
            }

            return clips;
        }


        public static string SaveClipPopup(bool rightMouseButton = false)
        {
            string lastPath = "";

            if (!string.IsNullOrWhiteSpace(staticExportDirectory))
            {
                if (AssetDatabase.IsValidFolder(staticExportDirectory)) lastPath = staticExportDirectory;
            }

            if (string.IsNullOrWhiteSpace(lastPath)) if (Get.TargetClip) lastPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(Get.TargetClip));
            if (string.IsNullOrWhiteSpace(lastPath)) lastPath = Application.dataPath;

            int ver = 0;
            if (Get._anim_MainSet != null)
                if (Get._anim_MainSet.SetIDHash != 0)
                {
                    if (Get._anim_MainSet != Get.S.MainSetupsForClips[0])
                    {
                        int v = 0;
                        for (int i = 0; i < Get.S.MainSetupsForClips.Count; i++)
                        {
                            if (Get.S.MainSetupsForClips[i].settingsForClip == Get._anim_MainSet.settingsForClip)
                            {
                                v += 1;
                                if (Get.S.MainSetupsForClips[i] == Get._anim_MainSet) { ver = v; break; }
                            }
                        }
                    }
                }

            string verStr = "";
            if (_forceExportGeneric) verStr = _exportLegacy ? " - Legacy" : " - Generic";
            if (ver > 0) verStr += " V" + (ver);

            string clipName = Get._anim_MainSet.AlternativeUsePrefix ? "Anim - " : "" + Get._anim_MainSet.AlternativeName;
            if (string.IsNullOrWhiteSpace(Get._anim_MainSet.AlternativeName)) clipName = Get.TargetClip.name + " - Modified" + verStr;

            string filename = "";

            if (rightMouseButton)
            {
                filename = Path.Combine(lastPath, clipName + ".anim");
            }
            else
                filename = EditorUtility.SaveFilePanelInProject("Choose path to save animation clip file", clipName, "anim", "New Animation Clip Name", lastPath);

            //AnimationClip nClip = null;

            if (!string.IsNullOrEmpty(filename))
            {
                //filename = Path.GetFileNameWithoutExtension(filename);

                if (!string.IsNullOrEmpty(filename))
                {
                    try
                    {
                        if (filename != "")
                        {
                            return filename;
                        }
                    }
                    catch (System.Exception)
                    {
                        Debug.LogError("Something went wrong when creating animation clip file in your project.");
                    }

                }
            }

            return "";
        }


        #endregion


        #region Storage Utils

        Transform _validated_anim = null;

        /// <summary>
        /// Making sure if skeleton and limbs are prepared correctly
        /// </summary>
        private void ValidateSkeleton()
        {
            if (isReady == false) return;
            if (Ar.BonesSetup.Count == 0) return;

            if (_validate_lastClipSwitch != TargetClip) OnClipSwitch();

            if (latestAnimator != _validated_anim)
            {
                _validate_lastClipSwitch = null;

                Ar.LatestAnimator = latestAnimator;

                var armatureBones = S.GetAllArmatureBonesList;
                bool forceReload = false;

                #region Check for limbs reload

                for (int l = 0; l < Limbs.Count; l++)
                {
                    for (int b = 0; b < Limbs[l].Bones.Count; b++)
                    {
                        var bone = Limbs[l].Bones[b];
                        if (bone.T != null)
                        {
                            if (armatureBones.Contains(bone.T) == false) forceReload = true;
                        }

                        if (forceReload) break;
                    }

                    if (forceReload) break;
                }

                #endregion

                if (forceReload)
                {
                    for (int l = 0; l < Limbs.Count; l++) Limbs[l].RefresTransformReferences(Ar);
                    S._SetDirty();
                }

                _validated_anim = latestAnimator;
            }
        }


        private AnimationClip _validate_lastClipSwitch = null;
        private AnimationClipSettings _lastClipSettings = null;
        private bool _lastClipLooping = false;
        public void OnClipSwitch()
        {
            if (TargetClip == null) return;

            //_toSet_AdditionalDesignerSetSwitchTo = "";
            _toSet_SetSwitchToHash = 0;

            var main = S.GetSetupForClip(S.MainSetupsForClips, TargetClip, _toSet_SetSwitchToHash);
            S.GetSetupForClip(S.ElasticnessSetupsForClips, TargetClip, _toSet_SetSwitchToHash).RefreshSetsWith(S);
            var modSet = S.GetSetupForClip(S.ModificatorsSetupsForClips, TargetClip, _toSet_SetSwitchToHash);
            modSet.RefreshMods(S, main);
            modSet.RefreshTransformReferences(S);

            var ik = S.GetSetupForClip(S.IKSetupsForClips, TargetClip, _toSet_SetSwitchToHash);
            for (int c = 0; c < ik.LimbIKSetups.Count; c++) ik.LimbIKSetups[c].RefreshMod(S, main);

            _validate_lastClipSwitch = TargetClip;

            if (TargetClip != null)
            {
                _lastClipSettings = AnimationUtility.GetAnimationClipSettings(TargetClip);
                _lastClipLooping = TargetClip.isLooping;

                if (_lastClipSettings != null)
                {
                    if (_lastClipSettings.loopBlend) _lastClipLooping = true;
                    if (_lastClipSettings.loopTime) _lastClipLooping = true;
                    if (TargetClip.legacy && TargetClip.wrapMode == WrapMode.Loop) _lastClipLooping = true;
                }
            }
        }


        private List<TemporarySave> Temps = new List<TemporarySave>();

        public class TemporarySave
        {
            public Transform Anim;
            public AnimationDesignerSave Save;

            public TemporarySave(Transform anim, AnimationDesignerSave save)
            {
                Anim = anim;
                Save = save;
            }
        }

        public AnimationDesignerSave GetTempSaveFor(Transform anim)
        {
            for (int i = Temps.Count - 1; i >= 0; i--)
            {
                if (Temps[i] == null || Temps[i].Anim == null)
                    Temps.RemoveAt(i);
                else
                    if (Temps[i].Save == null)
                    Temps[i].Save = CreateInstance<AnimationDesignerSave>();
            }

            for (int i = 0; i < Temps.Count; i++)
                if (Temps[i].Anim == anim)
                    return Temps[i].Save;

            TemporarySave t = new TemporarySave(anim, CreateInstance<AnimationDesignerSave>());
            Temps.Add(t);
            return t.Save;
        }

        #endregion


        #region Auto Detection Algorithms


        public void AutoDetection_Limbs()
        {
            if (latestAnimator == null) return;

            int newLimbs = 0;

            bool rmb = false;
            if (Event.current != null) if (Event.current.button == 1) rmb = true;

            ForceTPose();

            Transform rootT = latestAnimator;
            //if (!rmb) { if (S.Armature != null) if (S.Armature.RootBoneReference != null) if (S.Armature.RootBoneReference.TempTransform != null) rootT = S.Armature.RootBoneReference.TempTransform; }
            Transform pelvisT = null;
            if (S.Armature != null) if (S.Armature.PelvisBoneReference != null) pelvisT = S.Armature.PelvisBoneReference.TempTransform;

            SkeletonRecognize.SkeletonInfo info = new SkeletonRecognize.SkeletonInfo(rootT, rmb ? S.GetAllArmatureBonesList : null, pelvisT);

            if (info.Arms >= 2)
            {
                for (int i = 0; i < info.ProbablyLeftArms.Count; i++)
                {
                    ADArmatureLimb lArm = new ADArmatureLimb();
                    for (int b = 0; b < info.ProbablyLeftArms[i].Count; b++)
                        lArm.Bones.Add(new ADBoneID(info.ProbablyLeftArms[i][b], latestAnimator.transform));
                    lArm.LimbType = ADArmatureLimb.ELimbType.Arm;
                    lArm.LimbName = "Left Arm";
                    if (lArm.Bones.Count > 0) { Limbs.Add(lArm); newLimbs += 1; }
                }

                for (int i = 0; i < info.ProbablyRightArms.Count; i++)
                {
                    ADArmatureLimb rArm = new ADArmatureLimb();
                    for (int b = 0; b < info.ProbablyRightArms[i].Count; b++)
                        rArm.Bones.Add(new ADBoneID(info.ProbablyRightArms[i][b], latestAnimator.transform));
                    rArm.LimbName = "Right Arm";
                    rArm.LimbType = ADArmatureLimb.ELimbType.Arm;
                    if (rArm.Bones.Count > 0) { Limbs.Add(rArm); newLimbs += 1; }
                }
            }


            if (info.SpineChainLength >= 3)
            {
                ADArmatureLimb spn = new ADArmatureLimb();

                for (int b = 0; b < info.ProbablySpineChainShort.Count; b++)
                {
                    if (info.ProbablySpineChainShort[b] == latestAnimator.transform) continue;
                    if (latestAnimator.transform.childCount > 0) if (info.ProbablySpineChainShort[b] == latestAnimator.transform.GetChild(0)) continue;
                    spn.Bones.Add(new ADBoneID(info.ProbablySpineChainShort[b], latestAnimator.transform));
                }

                spn.LimbType = ADArmatureLimb.ELimbType.Spine;
                spn.LimbName = "Spine";
                spn.ExecuteFirst = true;
                spn.CheckComponentsBlendingInitialization(true);

                if (spn.Bones.Count > 0) { Limbs.Add(spn); newLimbs += 1; }
            }

            if (info.Legs >= 2)
            {
                for (int i = 0; i < info.ProbablyLeftLegs.Count; i++)
                {
                    ADArmatureLimb lLeg = new ADArmatureLimb();
                    for (int b = 0; b < info.ProbablyLeftLegs[i].Count; b++)
                        lLeg.Bones.Add(new ADBoneID(info.ProbablyLeftLegs[i][b], latestAnimator.transform));
                    lLeg.LimbType = ADArmatureLimb.ELimbType.Leg;
                    lLeg.LimbName = "Left Leg";
                    if (lLeg.Bones.Count > 0) { Limbs.Add(lLeg); newLimbs += 1; }
                }

                for (int i = 0; i < info.ProbablyRightLegs.Count; i++)
                {
                    ADArmatureLimb rLeg = new ADArmatureLimb();
                    for (int b = 0; b < info.ProbablyRightLegs[i].Count; b++)
                        rLeg.Bones.Add(new ADBoneID(info.ProbablyRightLegs[i][b], latestAnimator.transform));
                    rLeg.LimbType = ADArmatureLimb.ELimbType.Leg;
                    rLeg.LimbName = "Right Leg";
                    if (rLeg.Bones.Count > 0) { Limbs.Add(rLeg); newLimbs += 1; }
                }
            }

            if (newLimbs > 0)
            {
                for (int i = 0; i < Limbs.Count; i++)
                {
                    Limbs[i].Index = i;
                    Limbs[i].RefresTransformReferences(Ar);
                }


                EditorUtility.DisplayDialog("Setted Up Limbs", "Algorithm found " + info.Legs + " legs and " + info.Arms + " arms in the skeleton and setted up " + Limbs.Count + " limbs, please review them if everything is correct!\n\nAlgorithm recognized model as: " + info.WhatIsIt, "Ok");
            }
            else
                EditorUtility.DisplayDialog("Couldn't detect bones", "Algorithm was uneable to find limbs in the current skeleton.", "Ok");


            if (debugLogs)
            {
                UnityEngine.Debug.Log("--- AUTO LIMBS DETECTION REPORT BELOW ---\n\n" + info.GetLog());
            }
        }


        #endregion


    }
}