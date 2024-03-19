using FIMSpace.FEditor;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace FIMSpace.AnimationTools
{

    // -------------------------------------------------
    //
    //  OPEN ANIMATION DESIGNER WINDOW BY CLICKING
    //  Window/Fimpossible Creations/ Animation Designer Window
    //
    // -------------------------------------------------


    public partial class AnimationDesignerWindow : EditorWindow
    {
        public static AnimationDesignerWindow Get;

        public bool EnableExperimentalUndo = false;

        /// <summary> Assign through script inspector window </summary>
        public UnityEngine.Object BaseDirectory;
        public UnityEngine.Object ModuleSetupsDirectory;
        public UnityEngine.Object CustomModuleScriptFilesDirectory;

        [MenuItem("Window/FImpossible Creations/Animation Designer Window", false, 221)]
        #region Initialize and show window
        public static void Init()
        {
            AnimationDesignerWindow window = (AnimationDesignerWindow)GetWindow(typeof(AnimationDesignerWindow));
            window.titleContent = new GUIContent("ADesigner", Resources.Load<Texture>("AnimationDesigner/SPR_ADes32"), "Animation Designer Window");
            // other Icons: "BuildSettings.Android" "Grid.MoveTool@2x" "Preset.Context@2x" "AnimatorState Icon" "BlendTree Icon" d_BlendTree Icon "d_NavMeshAgent Icon" "Animator Icon"
            window.Show();

            Rect p = window.position;
            if (p.size.x < 340) { p.size = new Vector2(340, p.height); }
            if (p.size.y < 550) p.size = new Vector2(p.size.x, 550);
            window.position = p;

            Get = window;
        }
        #endregion


        #region Open File with double click - open designer window

        [OnOpenAssetAttribute(1)]
        public static bool OpenDesignerScriptableFile(int instanceID, int line)
        {
            UnityEngine.Object obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj is AnimationDesignerSave)
            {
                if (Get == null) Init(); else Get.Focus();

                Get.SetSetup(obj as AnimationDesignerSave);
                Get.Repaint();
                return true;
            }

            return false;
        }



        internal void SetSetup(AnimationDesignerSave setup)
        {
            ProjectFileSave = setup;
        }

        public static bool ContainsProjectFileSave()
        {
            if (Get == null) return false;
            return Get.ProjectFileSave != null;
        }

        #endregion

        #region Saves Related

#if UNITY_2019_4_OR_NEWER
        public static Texture _FolderDir { get { if (__folderdir != null) return __folderdir; __folderdir = EditorGUIUtility.IconContent("d_Folder Icon").image; return __folderdir; } }
        private static Texture __folderdir = null;
#else
        public static Texture _FolderDir { get { if (__folderdir != null) return __folderdir; __folderdir = EditorGUIUtility.IconContent("Folder Icon").image; return __folderdir; } }
        private static Texture __folderdir = null;
#endif

        private SerializedObject so_currentSetup = null;

        private ScriptableObject wasChecked = null;
        private bool isInDefaultDirectory = false;

        protected AnimationDesignerSave ProjectFileSave = null;
        protected AnimationDesignerSave _latest_ProjectFileSave = null;
        protected AnimationDesignerSave DisplaySave = null;
        protected AnimationDesignerSave _latest_DisplaySave = null;
        protected AnimationDesignerSave TempSave = null;

        #endregion

        #region Main Utility

        RuntimeAnimatorController originalAnimatorController = null;
        int latestFilesInDraft = 0;
        DateTime latestDraftsCheckTime = new DateTime();
        float _reloadingAlpha = 0.2f;
        float _hourglassAlpha = 1f;
        public bool IsVisible { get; protected set; }
        void OnBecameVisible() { IsVisible = true; Get = this; }
        void OnBecameInvisible()
        {
            if (currentMecanim) currentMecanim.runtimeAnimatorController = originalAnimatorController;
            if (S) ForceTPose();
            IsVisible = false;
        }
        void CheckDraftsFoldersForFileCount(bool hard = false)
        {
            if (!hard) if (DateTime.Now.Subtract(latestDraftsCheckTime).TotalSeconds < 20) return;
            if (BaseDirectory == null) return;
            string path = AssetDatabase.GetAssetPath(BaseDirectory);
            var files = System.IO.Directory.GetFiles(path, "*.asset");
            if (files != null) latestFilesInDraft = files.Length;
            latestDraftsCheckTime = DateTime.Now;
        }
        private void OnEnable()
        {
            Get = this;
            CheckDraftsFoldersForFileCount();
        }


        protected Color preBG;
        protected Color preGuiC;

        #endregion


        #region Prepare Scene GUI Draw

        bool wasSceneRepaint = false;

        void OnFocus()
        {
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= this.OnSceneGUI;
            SceneView.duringSceneGui += this.OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
            SceneView.onSceneGUIDelegate += this.OnSceneGUI;
#endif
        }



        void OnDestroy()
        {
            if (S)
            {
                if (currentMecanim) currentMecanim.runtimeAnimatorController = originalAnimatorController;
                ForceTPose();
                S.DampSessionSkeletonReferences();
            }

#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= this.OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
#endif
        }

        void OnSceneGUI(SceneView sceneView)
        {
            wasSceneRepaint = true;

            // Forcing GUI repaint when fading animation
            if (_reloadingAlpha < 0.99f)
            {
                Repaint();
            }

            if (SceneView.currentDrawingSceneView == null) return;
            if (SceneView.currentDrawingSceneView.camera == null) return;
            if (!IsVisible) return;
            //if (focusedWindow != Get && (focusedWindow is SceneView == false)) return;
            if (DisplaySave == null) return;

            S.UpdateReferences();

            Handles.BeginGUI();
            DrawScreenGUI();
            Handles.EndGUI();

            Handles.BeginGUI();
            Handles.SetCamera(SceneView.currentDrawingSceneView.camera);
            DrawHandles(SceneView.currentDrawingSceneView.camera);
            Handles.matrix = Matrix4x4.identity;
            Handles.EndGUI();

        }

        #endregion

        internal AnimationClip TargetClip;
        private AnimationClip _latestClip = null;
        internal AnimationClip LatestSaved;

        bool IsReady { get { return (S != null && S.Armature != null && S.Armature.BonesSetup.Count > 0 && S.Armature.BonesSetup[0].TempTransform != null); } }
        bool isReady = false;

        public AnimationDesignerSave S { get { return DisplaySave; } }
        public ADArmatureSetup Ar { get { return S.Armature; } }
        List<ADArmatureLimb> Limbs { get { return DisplaySave.Limbs; } }

        Transform latestAnimator = null;
        Animator currentMecanim = null;
        public Animator GetMecanim { get { return currentMecanim; } }
        Animation currentLegacy = null;

        bool repaintRequest = false;
        bool restoredTPose = true;

        bool sectionFocusMode = false;

        /// <summary> Extra animator character on which we can preview different animation clip in sync with animation designer clip time </summary>
        Transform latestSecondaryAnimator = null;
        AnimationClip latestSecondaryAnimatorClip = null;

        Vector2 scroll = Vector2.zero;
        bool _serializationChanges = false;
        AnimationDesignerSave _toSet_ProjectFileSave = null;
        AnimationDesignerSave _toSet_TriedReloadWith = null;
        Avatar _toSet_triedLoadFromDrafts = null;
        bool _toSet_ProjectFileSave_Clear = false;
        //string _toSet_AdditionalDesignerSetSwitchTo = "";
        public static int _toSet_SetSwitchToHash = 0;
        public static int _last_toSet_SetSwitchToHash = 0;
        //int _toSet_ProjectFileSaveDelay = 0;
        bool _triggerSkeletonRefresh = false;
        bool _switchingReferences = false;
        bool _showTip1 = true;
        GUIStyle _style_horScroll = null;
        GUIStyle _style_vertScroll = null;
        Animation selectionLegacyAnimation = null;
        Animation latestLegacyAnimationComponent = null;

        public Transform AnimatorTransform { get { if (latestAnimator) return latestAnimator.transform; if (latestLegacyAnimationComponent) return latestLegacyAnimationComponent.transform; return null; } }


        private void OnGUI()
        {

            #region Initial GUI Defines

            if (currentMecanim)
            {
                if (currentMecanim.runtimeAnimatorController != null)
                {
                    if (currentMecanim.GetLayerName(0) != "0")
                    {
                        originalAnimatorController = currentMecanim.runtimeAnimatorController;
                    }
                }
            }

            if (isBaking)
            {
                GUILayout.Space(4);
                EditorGUILayout.HelpBox("Animation Baking in progress!", MessageType.Warning);
                GUILayout.Space(10);
                return;
            }

            bool isLayoutEvent = false;
            //bool isRepaintEvent = false;

            if (Event.current != null)
            {
                if (Event.current.type == EventType.Layout) isLayoutEvent = true;
                //else if (Event.current.type == EventType.Repaint) isRepaintEvent = true;
            }

            if (isLayoutEvent)
            {
                UseEditorEvents();
                _serializationChanges = false;
                _switchingReferences = false;
            }

            if (_style_horScroll == null)
            {
                _style_horScroll = GUI.skin.horizontalScrollbar;
                _style_vertScroll = new GUIStyle(GUI.skin.verticalScrollbar);
                _style_vertScroll.fixedWidth = 5;
            }


            #endregion


            #region Scene View Check

            if (SceneView.lastActiveSceneView == null)
            {
                GUILayout.Space(4);
                EditorGUILayout.HelpBox("NO SCENE VIEW! ANIMATION DESIGNER REQUIRES ACTIVE SCENE VIEW!", MessageType.Warning);
                GUILayout.Space(10);
            }

            #endregion


            #region Animation Component Check

            selectionLegacyAnimation = null;

            if (Selection.activeGameObject)
            {
                selectionLegacyAnimation = Selection.activeGameObject.GetComponent<Animation>();
                if (selectionLegacyAnimation != null) latestLegacyAnimationComponent = selectionLegacyAnimation;

                //if (currentAnimation)
                //{
                //    GUILayout.Space(5);
                //    EditorGUILayout.HelpBox("'ANIMATION' component is selected. It's recommended to use 'ANIMATOR' component instead of ANIMATION since ANIMATOR provides much more features for the Animation Designer.\n\nWhen working with GENERIC rig, Animation Designer gives possibility to export animation as Legacy Animation Clip!", MessageType.Info);
                //    GUILayout.Space(5);
                //}
                //else
                //{
                //}
            }
            else
            {
                currentLegacy = null;
            }


            #endregion


            #region Isolated mode check


#if UNITY_2020_1_OR_NEWER

            GameObject targetObj = Selection.activeGameObject;
            if (latestAnimator) targetObj = latestAnimator.gameObject;
            if (targetObj)
            {
                if (UnityEditor.SceneManagement.EditorSceneManager.IsPreviewScene(targetObj.scene))
                {
                    GUILayout.Space(10);
                    EditorGUILayout.HelpBox(" Detected Prefab Isolated Mode!\nAnimation Designer works only on the Scene view!", MessageType.Info);
                    GUILayout.Space(10);
                    return;
                }
            }
#else
#if UNITY_2019_4_OR_NEWER
            if (UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                GUILayout.Space(10);
                EditorGUILayout.HelpBox(" Detected Prefab Isolated Mode!\nAnimation Designer works only on the Scene view!", MessageType.Info);
                GUILayout.Space(10);
                return;
            }
#endif
#endif
            #endregion


            #region Triggering Sheduled Events

            if (isLayoutEvent)
            {
                if (_Trigger_SoftPrepareArmature)
                {
                    S.GatherBones();
                    _Trigger_SoftPrepareArmature = false;
                    _serializationChanges = true;
                }

                if (_Trigger_PrepareArmature)
                {
                    S.GetArmature();
                    repaintRequest = true;
                    S._SetDirty();
                    _Trigger_PrepareArmature = false;
                    _serializationChanges = true;
                }

                //if (so_currentSetup != null)
                //{
                //    SerializedProperty prop = so_currentSetup.GetIterator();
                //    if (prop.NextVisible(true))
                //    {
                //        do
                //        {
                //            EditorGUILayout.PropertyField(so_currentSetup.FindProperty(prop.name), true);
                //        } while (prop.NextVisible(false));
                //    }

                //    so_currentSetup.ApplyModifiedProperties();
                //}
            }

            #endregion



            #region Define data to display


            if (_latestClip != TargetClip)
            {
                _latestClip = TargetClip;
                OnTargetAnimationClipChange();
            }

            if (TargetClip != null) if (currentClipSettings == null) GetTargetClipSettings();

            if (_last_toSet_SetSwitchToHash != _toSet_SetSwitchToHash)
            {
                _last_toSet_SetSwitchToHash = _toSet_SetSwitchToHash;
                OnTargetAnimationClipChange();
            }


            // Assigning Designer Save to view through layout event
            // To avoid annoying Unity events about controls repaint
            if (isLayoutEvent)
                if (_toSet_ProjectFileSave != null || _toSet_ProjectFileSave_Clear)
                {
                    // Can't find a way to fix 'ArgumentException: Getting control 1's position in a group with only 1 controls when doing repaint'
                    // Error when switching between different presets... So many hours wasted, DISAPPOINTEED!

                    if (_toSet_ProjectFileSave_Clear) ProjectFileSave = null; else ProjectFileSave = _toSet_ProjectFileSave;
                    _toSet_ProjectFileSave_Clear = false; _toSet_ProjectFileSave = null;
                    _serializationChanges = true;
                }


            // Defining with object scene selection
            if (Selection.activeGameObject != null)
            {

                // Checking if selected scene object or project file
                // Not supporting working with project assets
                if (AssetDatabase.Contains(Selection.activeGameObject) == false)
                {
                    // Getting animator out of current selected game object
                    Animator cAnim = Selection.activeGameObject.GetComponent<Animator>();
                    Transform animTr = Selection.activeGameObject.transform;

                    if (cAnim || selectionLegacyAnimation)
                    {

                        // When no project file is selected - we set priority to edit just current selected object
                        if (ProjectFileSave == null)
                        {

                            #region Searching drafts for DesignerSave fitting with current animator avatar

                            if (cAnim)
                            {
                                if (_toSet_triedLoadFromDrafts != cAnim.avatar)
                                {
                                    _toSet_triedLoadFromDrafts = cAnim.avatar;

                                    if (BaseDirectory)
                                    {
                                        string path = AssetDatabase.GetAssetPath(BaseDirectory);
                                        var files = System.IO.Directory.GetFiles(path, "*.asset");
                                        if (files != null) for (int i = 0; i < files.Length; i++)
                                            {
                                                AnimationDesignerSave fs = AssetDatabase.LoadAssetAtPath<AnimationDesignerSave>(files[i]);
                                                if (fs) if (fs.TargetAvatar == cAnim.avatar)
                                                    {
                                                        _foundFittingSave = fs;
                                                        //AddEditorEvent(() =>
                                                        //{
                                                        //    ProjectFileSave = fs;
                                                        //    if (fs.LatestCorrect) TargetClip = fs.LatestCorrect;
                                                        //});

                                                        break;
                                                    }
                                            }
                                    }

                                }
                            }

                            #endregion


                            if (ProjectFileSave == null)
                            {
                                TempSave = GetTempSaveFor(animTr);
                                DisplaySave = TempSave;
                                if (_latest_DisplaySave != DisplaySave) _serializationChanges = true;
                                _latest_DisplaySave = DisplaySave;
                                latestAnimator = animTr;
                            }
                        }


                        // Project file set or changed with lines above
                        if (ProjectFileSave != null)
                        {
                            Avatar selAv = animTr.GetAvatar();

                            if (selAv == ProjectFileSave.TargetAvatar) // Selecting Object with same avatar
                            {

                                if (ProjectFileSave.LatestAnimator == animTr)
                                {
                                    if (ProjectFileSave != _toSet_TriedReloadWith)
                                    {
                                        _toSet_TriedReloadWith = ProjectFileSave;
                                        if (S.Armature.BonesSetup.Count == 0 || S.Armature.BonesSetup[0].TempTransform == null) { _triggerSkeletonRefresh = true; }
                                    }
                                }
                                else  // Other Animator but same Avatar
                                {
                                    if (animTr != latestAnimator) // Different latest animator - retarget to new active game object required
                                    {
                                        DrawFocusOnCurrentAnimatorButton();

                                        GUILayout.Space(4);
                                        GUI.backgroundColor = Color.green;
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.HelpBox("Selecting object with different animator! Should Animation Designer Retarget to '" + animTr.name + "' ?", MessageType.None);

                                        if (GUILayout.Button("Switch", GUILayout.Height(28)))
                                        {
                                            AddDampReferencesEvent(true);
                                            FrameTarget(animTr.gameObject);
                                        }

                                        EditorGUILayout.EndHorizontal();
                                        GUI.backgroundColor = preBG;
                                    }
                                    else // Same latest animator
                                    {
                                        DrawFocusOnCurrentAnimatorButton();

                                        GUILayout.Space(4);
                                        GUI.backgroundColor = Color.green;
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.HelpBox("Selecting object with different animator! Should Animation Designer Switch to '" + animTr.name + "' ?", MessageType.None);

                                        if (GUILayout.Button("Switch", GUILayout.Height(28)))
                                        {
                                            AddDampReferencesEvent(true);

                                            AddEditorEvent(() =>
                                           {
                                               TempSave = GetTempSaveFor(animTr);
                                               DisplaySave = TempSave;
                                               _latest_DisplaySave = DisplaySave;
                                               latestAnimator = animTr.transform;
                                               {
                                                   //ProjectFileSave.DampSessionSkeletonReferences();
                                                   //_switchingReferences = true;
                                                   //ProjectFileSave = null;
                                                   //TempSave = GetTempSaveFor(cAnim);
                                                   //DisplaySave = TempSave;
                                                   //_latest_DisplaySave = DisplaySave;
                                                   //latestAnimator = cAnim;
                                                   //_toSet_triedLoadFromDrafts = null;
                                                   //_serializationChanges = true;
                                               }
                                           });

                                            FrameTarget(animTr.gameObject);
                                        }

                                        EditorGUILayout.EndHorizontal();
                                        GUI.backgroundColor = preBG;
                                    }
                                }

                            }
                            else // Different Avatars!
                            {
                                DrawFocusOnCurrentAnimatorButton();

                                GUILayout.Space(4);
                                GUI.backgroundColor = Color.green;
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.HelpBox("Selecting object with different animator! Should Animation Designer Switch to '" + animTr.name + "' ?", MessageType.None);
                                if (GUILayout.Button("Switch", GUILayout.Height(28)))
                                {
                                    ProjectFileSave = null;
                                    TempSave = GetTempSaveFor(animTr);
                                    DisplaySave = TempSave;
                                    _latest_DisplaySave = DisplaySave;
                                    latestAnimator = animTr.transform;
                                    _serializationChanges = true;
                                    FrameTarget(animTr.gameObject);
                                }

                                EditorGUILayout.EndHorizontal();
                                GUI.backgroundColor = preBG;
                            }

                        }
                        else
                        {
                            _toSet_TriedReloadWith = null;
                        }

                    } // If Animator != null   END   -----

                }
                else
                {
                    EditorGUILayout.HelpBox("Selecting Project Asset. Please select Scene Object", MessageType.None);
                }

            }
            else // Not selecting Anything
            {
                DrawFocusOnCurrentAnimatorButton();
            }


            // When project file designer save preset changed
            // we trigger few operations to refresh serialized object
            if (ProjectFileSave != null)
            {
                if (_latest_ProjectFileSave != ProjectFileSave)
                {
                    if (_latest_ProjectFileSave != null) _latest_ProjectFileSave.DampSessionSkeletonReferences();

                    if (isLayoutEvent == false)
                    {
                        DisplaySave = ProjectFileSave;

                        if (latestAnimator) if (DisplaySave.TargetAvatar == null) DisplaySave.TargetAvatar = latestAnimator.GetAvatar();
                        if (_latest_DisplaySave != DisplaySave) _serializationChanges = true;
                        _latest_DisplaySave = DisplaySave;
                        _latest_ProjectFileSave = ProjectFileSave;
                    }
                }
            }

            if (so_currentSetup != null) if (so_currentSetup.targetObject != null) so_currentSetup.Update();


            // Generating serialized object to display properties if required
            if (DisplaySave != null)
            {
                if (so_currentSetup == null || so_currentSetup.targetObject != DisplaySave)
                {
                    so_currentSetup = new SerializedObject(DisplaySave);
                    _serializationChanges = true;
                }
            }


            if (_switchingReferences == false)
                if (_serializationChanges == false)
                    if (S != null) S.RefreshSkeleton(latestAnimator);



            if (latestAnimator)
            {
                bool checkMec = true;
                bool checkLegac = true;

                currentLegacy = null;
                if (currentMecanim) if (currentMecanim.transform == latestAnimator) { checkMec = false; checkLegac = false; }
                if (currentLegacy) if (currentLegacy.transform == latestAnimator) { checkMec = false; checkLegac = false; }

                if (checkMec) currentMecanim = latestAnimator.GetAnimator();
                if (checkLegac) if (currentMecanim == null) currentLegacy = latestAnimator.GetComponent<Animation>();
            }

            #endregion



            #region Initial GUI Colors and Fading Animation

            if (_serializationChanges || _switchingReferences) _reloadingAlpha = 0.001f;

            Color _origGuiC = GUI.color;
            Color _origGuiBC = GUI.backgroundColor;

            preGuiC = new Color(_origGuiC.r, _origGuiC.g, _origGuiC.b, _reloadingAlpha);
            preBG = new Color(_origGuiBC.r, _origGuiBC.g, _origGuiBC.b, _reloadingAlpha);

            _reloadingAlpha += 0.1f;
            if (_reloadingAlpha < 0.94f)
            {
                if (Event.current != null) if (Event.current.type == EventType.MouseDown) Event.current.Use();
            }
            else if (_reloadingAlpha > 1f) _reloadingAlpha = 1f;

            #endregion
            //

            #region Unity Editor Version Related IFDEFS

#if UNITY_2019_4_OR_NEWER
#else
            try
            {
#endif

            #endregion


            EditorGUIUtility.wideMode = true;
            scroll = GUILayout.BeginScrollView(scroll, GUIStyle.none, GUIStyle.none);


            GUILayout.Space(3);
            DisplaySaveHeaderTab();

            if (latestAnimator == null)
            {
                if (!_serializationChanges)
                {
                    EditorGUILayout.HelpBox("Select some scene object with Animator to start working on it", MessageType.Info);

                    #region Tip Image for Inspector Window

                    GUILayout.Space(14);

                    if (_showTip1)
                        if (_Tex_Tip1 != null)
                        {
                            GUI.color = new Color(1f, 1f, 1f, preGuiC.a * 0.7f);
                            float tipWidth = position.width - 18;
                            float tipHeight = tipWidth * ((float)_Tex_Tip1.height / (float)_Tex_Tip1.width);
                            Rect tipRect = EditorGUILayout.GetControlRect();
                            tipRect.position += new Vector2(4, 0);
                            tipRect.width = tipWidth * 0.85f;
                            tipRect.height = tipHeight * 0.85f;
                            GUI.DrawTexture(tipRect, _Tex_Tip1, ScaleMode.ScaleToFit);
                            GUI.color = new Color(1f, 1f, 1f, preGuiC.a * 0.01f);
                            if (GUI.Button(tipRect, GUIContent.none)) { _showTip1 = false; }
                            GUI.color = preGuiC;
                        }

                    #endregion
                }
            }
            else // Else - Displaying Designer Window
            {

                if (_serializationChanges)
                {
                    // For some reason this helpbox wasa causing GUI error
                    //EditorGUILayout.HelpBox("Reloading some data, move cursor here to refresh display", MessageType.Info);
                }
                else // MAIN GUI BLOCK START
                {


                    #region Header Panel with Object Title and Foldout

                    GUILayout.Space(4);
                    EditorGUILayout.BeginVertical(FGUI_Resources.HeaderBoxStyle);

                    EditorGUILayout.BeginHorizontal(GUILayout.Height(24));
                    GUILayout.Space(4);


                    if (sectionFocusMode)
                    {
                        GUILayout.Space(52);
                    }
                    else
                    {
                        if (isReady)
                        {
                            EditorGUILayout.BeginVertical();
                            GUILayout.Space(4);
                            if (GUILayout.Button(new GUIContent(FGUI_Resources.GetFoldSimbol(debugTabFoldout, true)), EditorStyles.boldLabel, GUILayout.Width(20))) debugTabFoldout = !debugTabFoldout;
                            EditorGUILayout.EndVertical();
                        }
                    }

                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(latestAnimator.gameObject.name, FGUI_Resources.HeaderStyleBig)) if (isReady) { debugTabFoldout = !debugTabFoldout; sectionFocusMode = false; }

                    GUILayout.FlexibleSpace();

                    if (isReady)
                    {
                        //if (sectionFocusMode) { GUILayout.Space(2); DrawPlaybackButton(); GUILayout.Space(6); }
                        FocusModeSwitchButton();
                        GizmosModsButton();
                    }

                    GizmosSwitchButton();

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.EndVertical();


                    #endregion


                    if (sectionFocusMode)
                    {
                        debugTabFoldout = false;
                        updateDesigner = true;
                    }


                    // Framing Designer GUI
                    EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxBlankStyle);


                    if (S != null && so_currentSetup != null && so_currentSetup.targetObject != null)
                    {

                        if (_triggerSkeletonRefresh) { RefreshArmatureSaveAndSkeletonSetup(); _triggerSkeletonRefresh = false; }


                        // Initial display operations
                        isReady = IsReady;
                        RefreshSave();
                        ValidateSkeleton();


                        #region Playback tools and Top Refresh Buttons


                        if (sectionFocusMode == false)
                        {

                            if (latestAnimator)
                            {
                                if (latestAnimator.UsingRootMotion())
                                {
                                    GUILayout.Space(-4);
                                    EditorGUILayout.HelpBox("Enabled Root Motion on the Animator can cause changing position of your model on the scene to zero position!", MessageType.None);
                                    GUILayout.Space(-1);
                                }
                            }


                            DrawBaseToolsTab();

                            if (isReady)
                            {
                                GUILayout.Space(12);
                                DrawPlaybackTab();
                                GUILayout.Space(8);
                            }
                        }
                        else
                        {
                            if (latestAnimator.IsHuman())
                            {
                                if (!TargetClip.isHumanMotion) sectionFocusMode = false;
                            }
                            else
                                if (TargetClip.isHumanMotion) sectionFocusMode = false;

                            //GUILayout.Space(4);

                            if (!IsReady) RefreshArmatureButton(26);
                            else
                            {
                                EditorGUILayout.BeginHorizontal();
                                DrawPlaybackStopButton();
                                DrawPlaybackButton();
                                GUILayout.Space(6);
                                DrawPlaybackTimeSlider();
                                EditorGUILayout.EndHorizontal();
                            }
                        }

                        #endregion


                        if (!isReady) // If skeleton/armature is not prepared
                        {
                            GUILayout.Space(8);
                            EditorGUILayout.HelpBox("First Prepare Your Character Model Armature", MessageType.Info);
                            GUILayout.Space(20);


                            #region Main Info Tooltip Switch and View 

                            if (_utilTip_displayArmSetupInfo) GUI.color = new Color(0.6f, 0.6f, 0.6f, 1f);
                            if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Info), FGUI_Resources.ButtonStyle, GUILayout.Width(20), GUILayout.Height(18))) _utilTip_displayArmSetupInfo = !_utilTip_displayArmSetupInfo;

                            GUI.color = preGuiC;

                            if (_utilTip_displayArmSetupInfo)
                                EditorGUILayout.HelpBox("\nModel Armature is set of bone transforms of your character/creature model.\n\nIn addition you can prepare 'Limbs' setups of the Armature, which can be used for some additional and very helpful animation motion.\n\nHumanoid rigs are prepared automatically but you can tweak additional limbs after automatic generating if required.\n\nGeneric Rigs can be setted up automatically but it needs review to check if automatic setup done it properly.\nDo it in 'Setup' category - the button with Gear icon which will appear after doing initial setup.\n ", MessageType.Info);

                            #endregion


                            #region Currently Selected Animator Type Info Buttons


                            if (latestAnimator)
                            {
                                if (latestAnimator.IsHuman())
                                {
                                    GUI.color = new Color(.7f, 1f, 0.7f, 1f);
                                    GUILayout.Space(10);
                                    if (GUILayout.Button("Detected Humanoid Rig", FGUI_Resources.HeaderStyle, GUILayout.Height(20))) RefreshArmatureSaveAndSkeletonSetup();
                                    GUILayout.Space(4);
                                    GUI.color = preGuiC;

                                    if (S.Armature.BonesSetup.Count == 0)
                                    {
                                        if (GUILayout.Button("Algorithm will prepare automatic setup\nfor the Armature!", EditorStyles.centeredGreyMiniLabel, GUILayout.Height(30))) RefreshArmatureSaveAndSkeletonSetup();
                                    }
                                    else
                                    {
                                        if (GUILayout.Button("Click to refresh your\nArmature Setup!", EditorStyles.centeredGreyMiniLabel, GUILayout.Height(30))) RefreshArmatureSaveAndSkeletonSetup();
                                    }
                                }
                                else
                                {
                                    GUILayout.Space(10);

                                    if (currentLegacy)
                                    {
                                        GUI.color = new Color(0.7f, 0.8f, 0.9f, 1f);
                                        if (GUILayout.Button("Detected Legacy Rig", FGUI_Resources.HeaderStyle, GUILayout.Height(20))) RefreshArmatureSaveAndSkeletonSetup();
                                    }
                                    else
                                    {
                                        GUI.color = new Color(1f, 0.7f, 0.5f, 1f);
                                        if (GUILayout.Button("Detected Generic Rig", FGUI_Resources.HeaderStyle, GUILayout.Height(20))) RefreshArmatureSaveAndSkeletonSetup();
                                    }

                                    GUILayout.Space(4);
                                    GUI.color = preGuiC;

                                    if (S.Armature.BonesSetup.Count == 0)
                                    {
                                        if (GUILayout.Button("Algorithm will try setup limbs automatically\nbut please review it manually!", EditorStyles.centeredGreyMiniLabel, GUILayout.Height(30))) RefreshArmatureSaveAndSkeletonSetup();
                                    }
                                    else
                                    {
                                        if (GUILayout.Button("Click to refresh your\nArmature Setup!", EditorStyles.centeredGreyMiniLabel, GUILayout.Height(30))) RefreshArmatureSaveAndSkeletonSetup();
                                    }
                                }
                            }

                            GUI.color = preGuiC;

                            #endregion


                            #region Tutorials Buttons


                            GUILayout.Space(60);

                            GUI.color = new Color(1f, 1f, 1f, 0.75f);
                            GUI.backgroundColor = new Color(1f, 1f, 1f, 0.35f);
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(40);

                            if (GUILayout.Button(new GUIContent("  Having Troubles? Check Tutorials!", FGUI_Resources.Tex_Tutorials), GUILayout.Height(28)))
                            {
                                Application.OpenURL("https://www.youtube.com/watch?v=Q2ruYQNPHGg&list=PL6MURe5By90n1VWe-Ezs9trtl8KeQennl");
                            }

                            GUILayout.Space(40);
                            EditorGUILayout.EndHorizontal();

                            #endregion


                            // Resore Colors
                            GUI.backgroundColor = preBG;
                            GUI.color = preGuiC;

                        }
                        else // Skeleton Ready - View Designer Categories
                        {
                            DrawCategoriesTab();
                        }


                        // Draw Baking Tab if we work on some clip
                        if (TargetClip != null)
                        {
                            GUILayout.Space(5);
                            GUILayout.FlexibleSpace();
                            DrawBakingTab();
                        }


                        so_currentSetup.ApplyModifiedProperties();
                        EditorGUILayout.EndVertical();

                        // Remembering all changes to be saved
                        S._SetDirty();

                    }
                    else // Can't view data
                    {
                        EditorGUILayout.HelpBox("Can't create serialized object for displaying data", MessageType.Info);
                        EditorGUILayout.EndVertical();
                    }


                } // Main GUI Block END

            }

            GUILayout.Space(3);

            GUILayout.EndScrollView();


            #region Draw Hourglass On Top when reloading

            if (_reloadingAlpha < 0.11f) _hourglassAlpha += 0.3f;
            else if (_reloadingAlpha < 0.3f) _hourglassAlpha += 0.2f;
            else _hourglassAlpha -= 0.1f;

            if (_hourglassAlpha > 0.9f) _hourglassAlpha = 0.9f;
            else if (_hourglassAlpha < 0f) _hourglassAlpha = 0f;

            if (_hourglassAlpha > 0.01f)
            {
                GUI.color = new Color(1f, 1f, 1f, _hourglassAlpha);
                Rect rWait = new Rect(position);
                rWait.position = Vector2.zero;
                float xRatio = rWait.width * 0.5f;
                float yRatio = rWait.height * 0.675f;
                rWait.width -= xRatio;
                rWait.height -= yRatio;
                rWait.position += new Vector2(xRatio, yRatio) * 0.5f;
                GUI.DrawTexture(rWait, Tex_WaitIMG, ScaleMode.ScaleToFit);
                GUI.color = preGuiC;
            }

            #endregion


            #region Unity Editor Version Related IFDEFS

#if UNITY_2019_4_OR_NEWER
#else
            }
            catch (Exception e)
            {

                if (e.HResult == -2147024809 || e.HResult == -2146233088) // Ignore harmless unity GUI error
                {
                }
                else
                {
                    UnityEngine.Debug.Log("Info: " + e.HResult + " | ");
                    UnityEngine.Debug.Log("[Animation Designer] Exception when drawing GUI");
                    UnityEngine.Debug.LogException(e);
                }

            }
#endif

            #endregion


            if (repaintRequest && !_serializationChanges)
            {
                SceneView.RepaintAll();
                repaintRequest = false;
            }

        }


        AnimationClipSettings currentClipSettings = null;
        void GetTargetClipSettings()
        {
            if (TargetClip == null) { currentClipSettings = null; return; }
            currentClipSettings = AnimationUtility.GetAnimationClipSettings(TargetClip);
        }

        void OnTargetAnimationClipChange()
        {
            GetTargetClipSettings();
            CheckComponentsInitialization(false);
        }

        void RefreshSave()
        {
            if (_switchingReferences) return;

            S.LatestAnimator = latestAnimator;
            if (S.Armature != null) if (S.Armature.LatestAnimator == null) S.Armature.LatestAnimator = latestAnimator;
        }

        public static void ForceTPose()
        {
            if (Get) Get.S.RestoreTPose();
        }

        public void DrawFocusOnCurrentAnimatorButton()
        {
            if (latestAnimator == null) return;

            GUILayout.Space(4);

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button(new GUIContent("  Focus on current Animator", FGUI_Resources.TexTargetingIcon), GUILayout.Height(26)))
            {
                FrameTarget(latestAnimator.gameObject);
                EditorGUIUtility.PingObject(latestAnimator);
            }
            GUI.backgroundColor = preBG;

        }

        public void FrameTarget(GameObject g)
        {
            Selection.activeGameObject = g;
            SceneView.FrameLastActiveSceneView();
        }
        public static void ForceZeroFramePose()
        {
            if (!Get) return;

            if (Get.TargetClip == null)
            {
                ForceTPose();
                return;
            }

            float preTime = Get.animationElapsed;
            Get.animationElapsed = 0f;
            Get.SampleCurrentAnimation();
            Get.animationElapsed = preTime;
        }
    }
}