using FIMSpace.FEditor;
using FIMSpace.Generating;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.AnimationTools
{
    public partial class AnimationDesignerWindow : EditorWindow
    {
        public static bool debugLogs = false;
        bool debugTabFoldout = false;
        public enum ECategory { Setup, Elasticity, Modifiers, IK, Morphing, Custom }
        public ECategory Category = ECategory.Setup;

        #region Fitting Save Check

        AnimationDesignerSave _foundFittingSave = null;

        bool FittingSaveValidated
        {
            get
            {
                if (_foundFittingSave) if (latestAnimator)
                    {
                        Avatar av = latestAnimator.GetAvatar();
                        if (av != null) if (_foundFittingSave.TargetAvatar == av) if (_foundFittingSave != S) return true;
                    }

                return false;
            }
        }

        #endregion

        GUIContent _undoTex = null;

        void DisplaySaveHeaderTab()
        {
            if (latestAnimator == null) return;

            if (latestFilesInDraft == 0)
            {
                CheckDraftsFoldersForFileCount();
                if (S == null || S.Armature.BonesSetup.Count == 0) return;
            }

            GUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();

            #region Experimental Undo Switch

            if (_undoTex == null || _undoTex.image == null)
            {
                _undoTex = new GUIContent(Resources.Load<Texture2D>("Fimp/Misc Icons/FUndo"), "Enable experimental Ctrl+Z undo function for Animation Designer Window.\n(As for now Undo is not working with Animation Curves!)");
            }

            Color preBg = GUI.backgroundColor;
            GUI.backgroundColor = EnableExperimentalUndo ? Color.white : Color.gray * 0.6f;
            if (GUILayout.Button(_undoTex, FGUI_Resources.ButtonStyle, GUILayout.Height(18), GUILayout.Width(24))) { EnableExperimentalUndo = !EnableExperimentalUndo; }
            GUI.backgroundColor = preBg;

            #endregion

            #region Found fitting save - refresh button

            if (FittingSaveValidated)
            {
                if (_foundFittingSave != S)
                {
                    if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Refresh, "Designer Save file using same animator Avatar like current animator was found - click to choose it without need for searching for it"), EditorStyles.label, GUILayout.Height(16), GUILayout.Width(18)))
                    {

                        AddEditorEvent(() =>
                        {
                            ProjectFileSave = _foundFittingSave;
                            if (_foundFittingSave) if (_foundFittingSave.LatestCorrect) TargetClip = _foundFittingSave.LatestCorrect;
                        });
                    }

                    GUILayout.Space(4);
                }
            }

            #endregion


            ProjectFileSave = (AnimationDesignerSave)EditorGUILayout.ObjectField(ProjectFileSave, typeof(AnimationDesignerSave), false);

            GUILayout.Space(4);


            #region Button to display menu of draft setup files

            if (BaseDirectory)
            {

                if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_DownFold, "Display quick selection menu for AnimationDesigner Saves contained in the target directory"), EditorStyles.label, GUILayout.Width(16), GUILayout.Height(16)))
                {
                    CheckDraftsFoldersForFileCount(true);

                    string path = AssetDatabase.GetAssetPath(BaseDirectory);
                    var files = System.IO.Directory.GetFiles(path, "*.asset");
                    if (files != null)
                    {
                        GenericMenu draftsMenu = new GenericMenu();
                        draftsMenu.AddItem(new GUIContent("None"), ProjectFileSave == null, () => { _toSet_ProjectFileSave_Clear = true; });

                        for (int i = 0; i < files.Length; i++)
                        {
                            AnimationDesignerSave fs = AssetDatabase.LoadAssetAtPath<AnimationDesignerSave>(files[i]);

                            if (fs)
                            {
                                draftsMenu.AddItem(new GUIContent(fs.name), ProjectFileSave == fs, () => { _toSet_ProjectFileSave = fs; });
                            }
                        }

                        draftsMenu.ShowAsContext();
                    }
                }

                GUILayout.Space(3);
            }


            #endregion


            if (ProjectFileSave != null)
            {
                //if (GUILayout.Button(new GUIContent(FGUI_Resources.TexTargetingIcon, "Select AnimationAdjusterSave File"), FGUI_Resources.ButtonStyle, GUILayout.Width(20), GUILayout.Height(19))) Selection.activeObject = ProjectFileSave;
                if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Rename, "Opens popup for renaming AnimationDesignerSave filename"), EditorStyles.label, GUILayout.Width(20), GUILayout.Height(19))) FGenerators.RenamePopup(ProjectFileSave);
                GUILayout.Space(4);
            }

            if (DisplaySave)
            {
                if (ProjectFileSave == null) if (isReady) GUI.backgroundColor = Color.green;

                if (GUILayout.Button(new GUIContent("New Save", _help_animDesSave), GUILayout.Width(71)))
                {
                    string path = "";

                    if (BaseDirectory != null)
                    {
                        string testName = "Save";
                        if (latestAnimator != null) testName = latestAnimator.gameObject.name;
                        path = AssetDatabase.GetAssetPath(BaseDirectory);
                        var files = Directory.GetFiles(path, "*.asset");
                        path += "/AnimDesigner_" + testName + (files.Length + 1) + ".asset";
                    }

                    var scrInstance = Instantiate(DisplaySave);

                    if (string.IsNullOrEmpty(path))
                        path = FGenerators.GenerateScriptablePath(scrInstance, "ADSave_");

                    if (!string.IsNullOrEmpty(path))
                    {
                        UnityEditor.AssetDatabase.CreateAsset(scrInstance, path);
                        AssetDatabase.SaveAssets();
                        ProjectFileSave = scrInstance;
                    }
                }

                GUI.backgroundColor = preBG;
            }

            if (ProjectFileSave)
            {

                #region Preset File Check

                if (wasChecked != ProjectFileSave)
                {
                    so_currentSetup = null;
                    isInDefaultDirectory = false;

                    wasChecked = ProjectFileSave;
                    if (BaseDirectory)
                    {
                        string qPath = AssetDatabase.GetAssetPath(BaseDirectory);
                        string sPath = AssetDatabase.GetAssetPath(ProjectFileSave);
                        qPath = Path.GetFileName(qPath);
                        sPath = Path.GetFileName(Path.GetDirectoryName(sPath));
                        if (sPath.Contains(qPath)) isInDefaultDirectory = true;
                    }
                }

                #endregion


                if (isInDefaultDirectory)
                {
                    if (GUILayout.Button(new GUIContent(" Move", _FolderDir, "Move ADesigner Save file in project directory"), GUILayout.Height(20), GUILayout.Width(74)))
                    {
                        string path = FGenerators.GetPathPopup("Move ADesigner Save file to new directory in project", "AnimationDesignerSave");
                        if (!string.IsNullOrEmpty(path))
                        {
                            UnityEditor.AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(ProjectFileSave), path);
                            AssetDatabase.SaveAssets();
                        }

                        wasChecked = null;
                    }
                }
                else
                {
                    if (BaseDirectory != null)
                        if (GUILayout.Button(new GUIContent(" Back", _FolderDir, "Move ADesigner Save file in project to default Setups directory"), GUILayout.Height(20), GUILayout.Width(56)))
                        {
                            string path = AssetDatabase.GetAssetPath(BaseDirectory);

                            if (!string.IsNullOrEmpty(path))
                            {
                                path += "/" + ProjectFileSave.name + ".asset";
                                UnityEditor.AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(ProjectFileSave), path);
                                AssetDatabase.SaveAssets();
                            }

                            wasChecked = null;
                        }
                }

            }
            else
            {
                //wasChecked = null;
            }

            if (_helpC_animDesInfo == null || _helpC_animDesInfo.text == "") _helpC_animDesInfo = new GUIContent(FGUI_Resources.Tex_Info, _help_animDesSave);
            if (GUILayout.Button(_helpC_animDesInfo, EditorStyles.label, GUILayout.Width(17))) EditorUtility.DisplayDialog("'New Save' button info", _help_animDesSaveLong, "Ok");

            GUILayout.Space(2);

            //s
            EditorGUILayout.EndHorizontal();

            if (S.Limbs.Count > 1)
            {
                if (ProjectFileSave == null)
                {
                    if (S.MainSetupsForClips.Count > 1)
                    {
                        if (S.ModificatorsSetupsForClips.Count > 0)
                        {
                            if (S.ModificatorsSetupsForClips[0].BonesModificators.Count > 1)
                            {
                                GUI.color = new Color(1f, 0.5f, 0.3f);
                                EditorGUILayout.HelpBox("Don't forget to save your work in the save file!", MessageType.Warning);
                                GUI.color = preGuiC;
                                return;
                            }
                        }

                        GUI.color = new Color(1f, 0.8f, 0.6f);
                        EditorGUILayout.HelpBox("Don't forget to save your work in the save file!", MessageType.Info);
                        GUI.color = preGuiC;
                    }
                    else
                    {
                        bool enableds = false;
                        if (S.ElasticnessSetupsForClips != null)
                        {
                            if (S.ElasticnessSetupsForClips.Count > 0)
                                if (S.ElasticnessSetupsForClips[0].LimbsSets != null)
                                    if (S.ElasticnessSetupsForClips[0].LimbsSets.Count > 0)
                                    {
                                        for (int i = 0; i < S.ElasticnessSetupsForClips[0].LimbsSets.Count; i++)
                                        {
                                            if (S.ElasticnessSetupsForClips[0].LimbsSets[i].Enabled)
                                            {
                                                enableds = true;
                                                break;
                                            }
                                        }
                                    }
                        }

                        if (enableds)
                        {
                            GUI.color = new Color(1f, 0.85f, 0.7f);
                            EditorGUILayout.HelpBox("Don't forget to save your work in the save file!", MessageType.Info);
                            GUI.color = preGuiC;
                        }
                        else
                            EditorGUILayout.HelpBox("Don't forget to save your work in the save file!", MessageType.None);
                    }
                }
            }

        }


        public void DrawPlaybackStopButton()
        {
            if (GUILayout.Button(new GUIContent(Tex_Stop), EditorStyles.label, GUILayout.Width(22), GUILayout.Height(20))) { playPreview = false; animationElapsed = 0f; SampleCurrentAnimation(); }
        }

        public void DrawPlaybackButton()
        {
            if (playPreview) GUI.color = Color.gray;
            if (GUILayout.Button(new GUIContent(Tex_Play), EditorStyles.label, GUILayout.Width(22), GUILayout.Height(20))) { playPreview = !playPreview; }
            GUI.color = preGuiC;
        }

        bool updateDesigner = true;
        void DrawPlaybackTab()
        {
            if (TargetClip == null) GUI.backgroundColor = new Color(1f, 1f, 0.5f, 1f);
            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);

            GUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = preBG;

            updateDesigner = EditorGUILayout.Toggle(updateDesigner, GUILayout.Width(18));

            if (Searchable.IsSetted) if (_SelectorHelperId == "prjClp") { TargetClip = Searchable.Get<AnimationClip>(); _SelectorHelperId = "prjClp"; }

            if (TargetClip == null) GUI.color = new Color(1f, 1f, 0.5f, 1f);
            TargetClip = (AnimationClip)EditorGUILayout.ObjectField("Work With Clip:", TargetClip, typeof(AnimationClip), true);
            GUI.color = preGuiC;


            #region Button to display Clips Selector

            if (latestAnimator)
            {
                DrawAlreadyEditedClipsFoldDownButton();
                DrawAnimationClipsFoldDownButton();
                GUILayout.Space(3);
                DrawProjectAnimationClipsFoldDownButton();
                GUILayout.Space(3);
                DrawAdditionalClipSetupButton();
            }

            #endregion

            EditorGUILayout.EndHorizontal();


            if (_anim_MainSet != null)
                if (_anim_MainSet.SetIDHash != 0)
                {
                    if (_anim_MainSet != S.MainSetupsForClips[0])
                    {
                        GUI.color = new Color(1f, 1f, 1f, preGuiC.a * 0.8f);
                        EditorGUILayout.LabelField(new GUIContent("Displaying additional version of clip", FGUI_Resources.Tex_Add), EditorStyles.centeredGreyMiniLabel, GUILayout.Height(14));
                        GUI.color = preGuiC;
                    }
                }


            if (TargetClip)
                if (latestAnimator != null)
                {
                    if (latestAnimator.IsHuman())
                    {
                        if (TargetClip.isHumanMotion == false)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.HelpBox("Selected Clip is not Humanoid Animation Clip!", MessageType.Warning);
                            if (S.LatestCorrect) if (S.LatestCorrect.isHumanMotion) if (GUILayout.Button("Restore Previous")) { TargetClip = S.LatestCorrect; }
                            EditorGUILayout.EndHorizontal();
                        }
                        else
                        {
                            S.LatestCorrect = TargetClip;
                        }
                    }
                    else
                    {
                        if (TargetClip.isHumanMotion)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.HelpBox("Selected Clip is Humanoid Animation Clip! It should be Generic!", MessageType.Warning);
                            if (S.LatestCorrect) if (S.LatestCorrect.isHumanMotion == false) if (GUILayout.Button("Restore Previous")) { TargetClip = S.LatestCorrect; }
                            EditorGUILayout.EndHorizontal();
                        }
                        else
                        {
                            S.LatestCorrect = TargetClip;
                        }
                    }
                }


            if (TargetClip != null)
            {
                GUILayout.Space(3);

                if (debugTabFoldout == false) DrawPlaybackPanel();
            }


            if (TargetClip == null)
            {
                GUILayout.Space(4);
                EditorGUILayout.HelpBox("Choose some animation clip to start working on it", MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }

        public void DrawPlaybackPanel()
        {
            EditorGUILayout.BeginHorizontal();
            DrawPlaybackStopButton();
            DrawPlaybackButton();
            GUILayout.Space(6);
            DrawPlaybackSpeedSlider();
            EditorGUILayout.EndHorizontal();
            DrawPlaybackTimeSlider();
        }

        public void DrawPlaybackTimeSlider()
        {
            float progr = animationElapsed / _play_mod_Length;

            EditorGUI.BeginChangeCheck();

            //EditorGUILayout.BeginHorizontal();
            GUI.color = new Color(1f, 0.75f, 0.75f, 1f);
            GUI.backgroundColor = new Color(1f, 0.75f, 0.75f, 1f);
            EditorGUILayout.BeginHorizontal();
            progr = GUILayout.HorizontalSlider(progr, 0f, 1f);
            GUILayout.Space(12);
            EditorGUILayout.LabelField(System.Math.Round(animationElapsed, 2).ToString(), GUILayout.Width(32));
            EditorGUILayout.LabelField("sec", GUILayout.Width(22));
            EditorGUILayout.EndHorizontal();
            GUI.backgroundColor = preBG;
            GUI.color = preGuiC;
            if (EditorGUI.EndChangeCheck())
            {
                //UnityEngine.Debug.Log("progr = " + progr + " clipLen = " + TargetClip.length + " animEl = " + animationElapsed);
                animationElapsed = progr * _play_mod_Length;
                SampleCurrentAnimation();
            }
        }

        public void SetAnimationProgressManually(float progr)
        {
            progr = Mathf.Clamp01(progr);
            animationElapsed = progr * _play_mod_Length;
        }

        public void DrawPlaybackSpeedSlider()
        {
            EditorGUIUtility.labelWidth = 120;
            EditorGUIUtility.fieldWidth = 30;
            playbackSpeed = EditorGUILayout.Slider("Playback Speed: ", playbackSpeed, 0.1f, 2f);
            EditorGUIUtility.fieldWidth = 0;
            EditorGUIUtility.labelWidth = 0;
        }

        void DrawAlreadyEditedClipsFoldDownButton()
        {
            if (ProjectFileSave != null)
            {
                //UnityEngine.Debug.Log("ProjectFileSave " + ProjectFileSave + " clipscount " + ProjectFileSave.MainSetupsForClips.Count);
                if (ProjectFileSave.MainSetupsForClips.Count > 1)
                {
                    GUILayout.Space(-5);
                    GUI.color = new Color(1f, 1f, 1f, preGuiC.a * 0.6f);
                    if (DropdownButton("Display quick selection menu for animation clips which was edited with the AnimationDesignerSave file", true))
                    {
                        List<AnimationClip> clips = ProjectFileSave.GetAllEditedClipsReferences();

                        GenericMenu clipsMenu = new GenericMenu();

                        clipsMenu.AddItem(new GUIContent("-- Already Edited Clips --"), false, () => { });
                        clipsMenu.AddItem(new GUIContent(""), false, () => { });
                        clipsMenu.AddItem(new GUIContent(""), false, () => { });

                        for (int i = 0; i < clips.Count; i++)
                        {
                            var clp = clips[i];
                            clipsMenu.AddItem(new GUIContent(clp.name), TargetClip == clp, () => { TargetClip = clp; });
                        }

                        if (TargetClip != null)
                        {
                            clipsMenu.AddItem(new GUIContent(""), false, () => { });
                            clipsMenu.AddItem(new GUIContent(""), false, () => { });
                            clipsMenu.AddItem(new GUIContent(""), false, () => { });
                            AnimationClip toRemove = TargetClip;
                            clipsMenu.AddItem(new GUIContent("REMOVE Designer Save Data for '" + TargetClip.name + "'"), false, () => { TargetClip = null; ProjectFileSave.RemoveSaveDataForClip(toRemove); });
                        }

                        clipsMenu.ShowAsContext();
                    }
                    GUI.color = preGuiC;
                }
            }
        }

        void DrawAnimationClipsFoldDownButton()
        {
            if (currentMecanim == null) return;

            if (DropdownButton("Display quick selection menu for animation clips aready in animator"))
            {
                var clips = GetAllClipsFrom(currentMecanim);

                GenericMenu clipsMenu = new GenericMenu();
                clipsMenu.AddItem(new GUIContent("-- Clips In the Animator Controller --"), false, () => { });
                clipsMenu.AddItem(new GUIContent(""), false, () => { });
                clipsMenu.AddItem(new GUIContent(""), false, () => { });

                clipsMenu.AddItem(new GUIContent("None"), TargetClip == null, () => { TargetClip = null; });

                for (int i = 0; i < clips.Count; i++)
                {
                    var clp = clips[i];
                    clipsMenu.AddItem(new GUIContent(clp.name), TargetClip == clp, () => { TargetClip = clp; });
                }

                clipsMenu.ShowAsContext();
            }
        }

        int latestProjAnimsGuidsCount = 0;
        List<AnimationClip> latestProjAnimClipAssets = new List<AnimationClip>();

        void DrawProjectAnimationClipsFoldDownButton()
        {
            if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_SearchDirectory, "Clicking it first time will trigger project scan for Animation Clips!\n\nDisplay quick selection menu for animation clips found in project (trying to display only ones which will work with the current armature setup)"), EditorStyles.label, GUILayout.Width(28), GUILayout.Height(18)))
            {
                List<AnimationClip> clips = new List<AnimationClip>();
                List<string> clipNames = new List<string>();

                try
                {
                    string animTypeName = "fitting";
                    bool humano = false;

                    if (latestAnimator != null)
                    {
                        if (latestAnimator.IsHuman())
                        {
                            animTypeName = "Humanoid";
                            humano = true;
                        }
                        else
                        {
                            if (currentLegacy) animTypeName = "Legacy";
                            else if (currentMecanim) animTypeName = "Generic";
                        }
                    }

                    string title = "Searching project for " + animTypeName + " animation clips...";
                    string info = "Searching the next time will be faster...";

                    string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(AnimationClip).ToString().Replace("UnityEngine.", "")));
                    bool search = true;

                    if (latestProjAnimClipAssets.Count > 0)
                    {
                        if (latestProjAnimsGuidsCount == guids.Length) search = false;
                    }

                    if (search)
                    {
                        latestProjAnimClipAssets.Clear();
                        latestProjAnimsGuidsCount = guids.Length;

                        for (int i = 0; i < guids.Length; i++)
                        {
                            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                            AnimationClip asset = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
                            if (asset != null) latestProjAnimClipAssets.Add(asset);

                            if (i == 0 || i % 50 == 0)
                            {
                                if (EditorUtility.DisplayCancelableProgressBar(title, info, (float)i / (float)guids.Length))
                                { EditorUtility.ClearProgressBar(); latestProjAnimsGuidsCount = 0; return; }
                            }
                        }
                    }

                    // Choosing right animation clips
                    info = "Choosing right animation clips...";

                    for (int i = 0; i < latestProjAnimClipAssets.Count; i++)
                    {
                        var clip = latestProjAnimClipAssets[i];
                        bool add = false;

                        if (S.LatestAnimator.IsHuman())
                        {
                            if (clip.isHumanMotion && clip.humanMotion) add = true;
                        }
                        else
                        {
                            if (currentMecanim)
                            {
                                if (!clip.isHumanMotion && !clip.humanMotion && clip.legacy == false) add = true;
                            }
                            else if (currentLegacy)
                            {
                                if (!clip.isHumanMotion && !clip.humanMotion && clip.legacy) add = true;
                            }
                        }

                        if (add)
                            if (clips.Contains(clip) == false)
                            {
                                clips.Add(clip);

                                var mainAsset = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(clip));
                                string clipPath = System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(clip)).ToLower();

                                string nme = clip.name;
                                string nmeToLower = nme.ToLower();
                                string foldName = "";

                                string category = "";

                                if (nmeToLower.Contains("idle")) category = "Idle";
                                else if (nmeToLower.Contains("walk")) category = "Walk";
                                else if (nmeToLower.Contains("run")) category = "Run";
                                else if (nmeToLower.Contains("jog")) category = "Jog";
                                else if (nmeToLower.Contains("sprint")) category = "Sprint";
                                else if (nmeToLower.Contains("attack")) category = "Attack";
                                else if (nmeToLower.Contains("hit")) category = "Hit";
                                else if (nmeToLower.Contains("damage")) category = "Damage";
                                else if (nmeToLower.Contains("jump")) category = "Jump";
                                else if (nmeToLower.Contains("skill")) category = "Skill";
                                else if (nmeToLower.Contains("punch")) category = "Punch";
                                else if (nmeToLower.Contains("dash")) category = "Dash";

                                if (!humano)
                                {
                                    if (!string.IsNullOrWhiteSpace(nme))
                                    {
                                        if (clip.name.Contains("|"))
                                        {
                                            string preName = clip.name.Substring(0, clip.name.IndexOf('|'));
                                            category = ":" + preName;
                                        }
                                    }
                                }

                                if (mainAsset != null)
                                    if (mainAsset.name.Length > 6)
                                        nme += "  (" + mainAsset.name + ")";

                                if (category == "")
                                {
                                    foldName = "Others/" + clip.name;
                                    //if (clip.name.Length > 4) foldName = "Others/" + clip.name.Substring(0, Mathf.Min(clip.name.Length, 6)).ToLower();
                                }
                                else
                                {
                                    foldName = category;
                                }

                                //if (mainAsset != null) nme += " (" + mainAsset.name + ")";

                                if (clip.name != clipPath) clipNames.Add(foldName + "/" + nme);
                                else
                                {
                                    if (clip.name.Length > 4)
                                    {
                                        clipNames.Add(foldName + "/" + nme);
                                    }
                                    else
                                    {
                                        clipNames.Add(nme);
                                    }
                                }
                            }

                        if (i % 10 == 0) EditorUtility.DisplayProgressBar(title, info, (float)i / (float)guids.Length);
                    }

                }
                catch (Exception)
                {
                    EditorUtility.ClearProgressBar();
                }

                EditorUtility.ClearProgressBar();

                if (clips.Count > 0)
                {

                    _SelectorHelperId = "prjClp";
                    AnimationDesignerWindow.ShowElementsSelector(latestAnimator.IsHuman() ? "Choose Project Humanoid Animation Clip" : "Choose Project Animation Clip", clips, clipNames, AnimationDesignerWindow.GetMenuDropdownRect(), true);

                }

            }
        }

        public static AnimationClip _CopyFrom_Clip;
        public static AnimationDesignerSave _CopyFrom_Setup;
        public static int _CopyFrom_VersHash = 0;

        void DrawAdditionalClipSetupButton()
        {
            if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Add, "If you want to create more animation clips variants out of this one source animation clip or select ones already added"), EditorStyles.label, GUILayout.Width(20), GUILayout.Height(18)))
            {
                var allVersions = S.GetAllDesignerSetsFor(TargetClip);

                if (Event.current != null)
                {

                    if (allVersions.Count == 1 || Event.current.button == 0)
                    {

                        GenericMenu draftsMenu = new GenericMenu();
                        draftsMenu.AddItem(new GUIContent("+ Create Additional Designer Set for the Clip +"), false, () =>
                        {
                            _toSet_SetSwitchToHash = (allVersions.Count + 1).ToString().GetHashCode();
                            //_toSet_AdditionalDesignerSetSwitchTo = TargetClip.name + " - Version " + (allVersions.Count + 1).ToString();
                        });

                        if (_CopyFrom_Clip == null || _CopyFrom_Clip != TargetClip)
                        {
                            draftsMenu.AddItem(new GUIContent("Copy all setup of this AnimationClip"), false, () =>
                            {
                                _CopyFrom_Clip = TargetClip;
                                _CopyFrom_Setup = S;
                                _CopyFrom_VersHash = _anim_MainSet.SetIDHash;
                            });
                        }

                        if (_CopyFrom_Clip != null && _CopyFrom_Clip != TargetClip)
                        {
                            draftsMenu.AddItem(new GUIContent("+ Paste all setup from " + _CopyFrom_Clip.name), false, () =>
                            {
                                //copyy(_CopyFrom_Setup, _CopyFrom_Clip, _CopyFrom_VersHash, S, TargetClip, _anim_MainSet.SetIDHash);
                                //AnimationDesignerSave.CopyAllSettingsFromTo(_CopyFrom_Setup, _CopyFrom_Clip, _CopyFrom_VersHash, S, TargetClip, _anim_MainSet.SetIDHash);

                                var fromS = _CopyFrom_Setup;
                                var fromC = _CopyFrom_Clip;
                                var fromH = _CopyFrom_VersHash;

                                AddEditorEvent(() =>
                                {
                                    fromS.CopySettingsFromTo(fromC, fromH, _anim_MainSet.SetIDHash, S, TargetClip);
                                });

                                _CopyFrom_Clip = null;
                                _CopyFrom_Setup = null;
                                _CopyFrom_VersHash = 0;
                            });
                        }

                        draftsMenu.AddItem(GUIContent.none, false, () => { });
                        draftsMenu.AddItem(GUIContent.none, false, () => { });

                        for (int i = 0; i < allVersions.Count; i++)
                        {
                            ADClipSettings_Main set = allVersions[i];
                            string display = "Version " + i;
                            if (!string.IsNullOrEmpty(set.AlternativeName)) display = set.AlternativeName;
                            if (set.SetIDHash == 0 || allVersions.Count == 1) display = "Main Animation Design";
                            draftsMenu.AddItem(new GUIContent(display), _anim_MainSet.SetIDHash == set.SetIDHash, () => { if (set.SetIDHash != _toSet_SetSwitchToHash) _toSet_SetSwitchToHash = set.SetIDHash; });
                        }

                        draftsMenu.AddItem(GUIContent.none, false, () => { });

                        draftsMenu.AddItem(new GUIContent("Rename current selected clip version"), false, () =>
                        {
                            string newName = _anim_MainSet.AlternativeName;
                            if (string.IsNullOrEmpty(newName)) newName = "Clip Name";
                            newName = FGenerators.RenamePopup(null, newName);
                            if (!string.IsNullOrEmpty(newName)) { _anim_MainSet.AlternativeName = newName; _anim_MainSet.AlternativeUsePrefix = true; }
                        });

                        draftsMenu.AddItem(new GUIContent("Rename current selected clip version (No Prefix)"), false, () =>
                        {
                            string newName = _anim_MainSet.AlternativeName;
                            if (string.IsNullOrEmpty(newName)) newName = "Clip Name";
                            newName = FGenerators.RenamePopup(null, newName);
                            if (!string.IsNullOrEmpty(newName)) { _anim_MainSet.AlternativeName = newName; _anim_MainSet.AlternativeUsePrefix = false; }
                        });


                        if (_anim_MainSet != allVersions[0])
                        {
                            draftsMenu.AddSeparator("");
                            draftsMenu.AddSeparator("");
                            draftsMenu.AddItem(new GUIContent("X REMOVE selected clip version"), false, () =>
                            {
                                var toRemoveMain = _anim_MainSet;
                                var toRemoveEl = _anim_elSet;
                                var toRemoveMod = _anim_modSet;
                                var toRemoveIK = _anim_ikSet;
                                var toRemoveMorphs = _anim_morphSet;
                                var toRemoveModule = _anim_cModuleSet;

                                AddEditorEvent(() =>
                                {
                                    S.MainSetupsForClips.Remove(toRemoveMain);
                                    S.ElasticnessSetupsForClips.Remove(toRemoveEl);
                                    S.ModificatorsSetupsForClips.Remove(toRemoveMod);
                                    S.IKSetupsForClips.Remove(toRemoveIK);
                                    S.MorphingSetupsForClips.Remove(toRemoveMorphs);
                                    S.CustomModuleSetupsForClips.Remove(toRemoveModule);
                                });

                                _toSet_SetSwitchToHash = 0;
                            });
                        }

                        draftsMenu.ShowAsContext();
                    }
                    else // Copy Button
                    {

                        GenericMenu draftsMenu = new GenericMenu();
                        draftsMenu.AddItem(new GUIContent("< Choose Clip Set To Copy Settings From >"), false, () => { });
                        draftsMenu.AddItem(GUIContent.none, false, () => { });

                        for (int i = 0; i < allVersions.Count; i++)
                        {
                            var set = allVersions[i];
                            if (_anim_MainSet.SetIDHash == set.SetIDHash) continue;

                            string display = "Version " + i;
                            if (!string.IsNullOrEmpty(set.AlternativeName)) display = set.AlternativeName;
                            if (set.SetIDHash == 0 || i == 0 || allVersions.Count == 1) display = "Main Animation Design";

                            int fromId = set.SetIDHash;
                            int toId = _anim_MainSet.SetIDHash;
                            AnimationClip clp = TargetClip;

                            draftsMenu.AddItem(new GUIContent("Copy from '" + display + "'"), _anim_MainSet.SetIDHash == set.SetIDHash, () =>
                            {
                                AddEditorEvent(() =>
                                {
                                    S.CopySettingsFromTo(clp, fromId, toId, S, null);
                                });
                            });
                        }

                        draftsMenu.ShowAsContext();

                    }
                }
            }
        }

        bool DropdownButton(string tooltip, bool dropRight = false)
        {
            Texture2D foldTex = dropRight ? FGUI_Resources.Tex_RightFold : FGUI_Resources.Tex_DownFold;
            if (GUILayout.Button(new GUIContent(foldTex, tooltip), EditorStyles.label, GUILayout.Width(16), GUILayout.Height(16))) return true;
            return false;
        }


        void DrawCategoriesTab()
        {

            GUILayout.Space(4);
            DrawCategorySelector(ref Category);

            GUILayout.Space(3);
            GUI.color = new Color(0.7f, 1f, 0.7f, preGuiC.a);
            GUI.backgroundColor = GUI.color;
            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);
            GUI.backgroundColor = preBG;
            GUI.color = preGuiC;

            if ((debugTabFoldout || updateDesigner == false) && Category != ECategory.Setup)
            {
                GUILayout.Space(4);

                if (debugTabFoldout)
                    EditorGUILayout.HelpBox("Now Focusing on Model Setup Pose", MessageType.Info);
                else
                    EditorGUILayout.HelpBox("Disabled all effects to compare animations", MessageType.Info);

                GUILayout.Space(4);
            }
            else
            {
                if (Category != ECategory.Setup && _anim_MainSet == null)
                {
                    EditorGUILayout.HelpBox("Lacking main setup data for animation clip!", MessageType.Error);
                }
                else
                {
                    switch (Category)
                    {
                        case ECategory.Setup: DrawSetupTab(); break;
                        case ECategory.IK: DrawIKTab(); break;
                        case ECategory.Modifiers: DrawModificatorsTab(); break;
                        //case ECategory.Springs: DrawSpringsTab(); break;
                        case ECategory.Elasticity: DrawElasticnessTab(); break;
                        case ECategory.Morphing: DrawMorphingTab(); break;
                        case ECategory.Custom: DrawCustomModulesTab(); break;
                    }
                }
            }

            EditorGUILayout.EndVertical();

        }


        #region Section Buttons

        public static Texture2D Tex_Play { get { if (__texPlay != null) return __texPlay; __texPlay = Resources.Load<Texture2D>("AnimationDesigner/Play"); return __texPlay; } }
        private static Texture2D __texPlay = null;
        public static Texture2D Tex_Stop { get { if (__texStp != null) return __texStp; __texStp = Resources.Load<Texture2D>("AnimationDesigner/Stop"); return __texStp; } }
        private static Texture2D __texStp = null;
        public static Texture2D Tex_WaitIMG { get { if (__texWaitImage != null) return __texWaitImage; __texWaitImage = Resources.Load<Texture2D>("AnimationDesigner/WaitImg"); return __texWaitImage; } }
        private static Texture2D __texWaitImage = null;

        public static Texture2D Tex_Arm { get { if (__texArm != null) return __texArm; __texArm = Resources.Load<Texture2D>("AnimationDesigner/Arm"); return __texArm; } }
        private static Texture2D __texArm = null;
        public static Texture2D Tex_Leg { get { if (_texLeg != null) return _texLeg; _texLeg = Resources.Load<Texture2D>("AnimationDesigner/Leg"); return _texLeg; } }
        private static Texture2D _texLeg = null;
        public static Texture2D Tex_Elastic { get { if (__texElast != null) return __texElast; __texElast = Resources.Load<Texture2D>("AnimationDesigner/Elastic"); return __texElast; } }
        private static Texture2D __texElast = null;
        public static Texture2D Tex_Hips { get { if (_texHips != null) return _texHips; _texHips = Resources.Load<Texture2D>("AnimationDesigner/Hips"); return _texHips; } }
        private static Texture2D _texHips = null;
        public static Texture2D Tex_IK { get { if (__texIK != null) return __texIK; __texIK = Resources.Load<Texture2D>("AnimationDesigner/IK"); return __texIK; } }
        private static Texture2D __texIK = null;
        public static Texture2D Tex_Chain { get { if (__texChain != null) return __texChain; __texChain = Resources.Load<Texture2D>("AnimationDesigner/Chain"); return __texChain; } }
        private static Texture2D __texChain = null;
        public static Texture2D Tex_AD { get { if (__texad != null) return __texad; __texad = Resources.Load<Texture2D>("AnimationDesigner/SPR_ADes32"); return __texad; } }
        private static Texture2D __texad = null;
        public static Texture2D Tex_Blank { get { if (__texBlank != null) return __texBlank; __texBlank = new Texture2D(1, 1); return __texBlank; } }
        private static Texture2D __texBlank = null;

        public static Texture2D Tex_Magnet { get { if (__texmagnt != null) return __texmagnt; __texmagnt = Resources.Load<Texture2D>("AnimationDesigner/SPR_Magnet"); return __texmagnt; } }
        private static Texture2D __texmagnt = null;

        public static Texture2D Tex_Pixel { get { if (__texpixl != null) return __texpixl; __texpixl = new Texture2D(1, 1); __texpixl.SetPixel(0, 0, Color.white); __texpixl.Apply(false, true); return __texpixl; } }
        private static Texture2D __texpixl = null;

        public static Texture2D Tex_CModules { get { if (__texCMods != null) return __texCMods; __texCMods = Resources.Load<Texture2D>("AnimationDesigner/ADModuleIcon"); return __texCMods; } }
        private static Texture2D __texCMods = null;

        private void DrawCategorySelector(ref ECategory categoryVar)
        {
            EditorGUILayout.BeginHorizontal();

            int height = 30;

            StartUndoCheckFor(this, ": Category");
            DrawSectionSelButton(ref Category, ECategory.Setup, FGUI_Resources.Tex_GearSetup, height);
            DrawSectionSelButton(ref Category, ECategory.Elasticity, Tex_Elastic, height);
            DrawSectionSelButton(ref Category, ECategory.Modifiers, FGUI_Resources.Tex_Limits, height);
            DrawSectionSelButton(ref Category, ECategory.IK, Tex_IK, height);
            //DrawSectionSelButton(ref Category, ECategory.Springs, Tex_Hips, height);
            DrawSectionSelButton(ref Category, ECategory.Morphing, FGUI_Resources.TexBehaviourIcon, height);
            DrawSectionSelButton(ref Category, ECategory.Custom, Tex_CModules, height, 32);
            GUI.color = preGuiC;
            EndUndoCheckFor(this);

            EditorGUILayout.EndHorizontal();


            #region Selection Helper Guide

            if ((int)Category < 5)
            {
                float animVal = 0f;
                if (!sectionFocusMode) animVal = timeSin01;

                Rect preR = GUILayoutUtility.GetLastRect();
                float startWdth = preR.width - 32f;
                float startPx = preR.position.x;
                float elWdth = (startWdth / 5f); // 5 but Custom modules button is shorter
                preR.position += new Vector2(0, preR.height + 1);
                preR.height = 3;
                preR.width = elWdth * 0.5f;
                float barWdth = preR.width - 32f; // 32 is custom modules button width
                preR.position = new Vector2(startPx * 1.0f + elWdth * (float)((int)Category) + elWdth * 0.5f - barWdth * 0.5f - 16f, preR.position.y);

                GUI.color = new Color(0.3f, 0.85f, 0.3f, 0.4f - animVal * 0.2f);
                GUI.DrawTexture(preR, Tex_Blank);
                GUI.color = preGuiC;
            }

            #endregion

        }

        void DrawSectionSelButton(ref ECategory cat, ECategory target, Texture icon, int height, int overrideWidth = 0)
        {
            if (cat == target) GUI.backgroundColor = Color.green;

            GUILayoutOption opt2 = null;
            if (overrideWidth > 0) opt2 = GUILayout.Width(overrideWidth);
            else opt2 = GUILayout.MinWidth(32);

            if (GUILayout.Button(new GUIContent(icon, target.ToString()), FGUI_Resources.ButtonStyle, GUILayout.Height(height), opt2))
            {
                if (target == ECategory.Setup)
                {
                    S._Tips_RootAndHipsMakeSureCounter += 1;
                }

                if (Event.current.button == 1) sectionFocusMode = !sectionFocusMode;
                cat = target;
            }

            GUI.backgroundColor = preBG;
        }

        #endregion


        [NonSerialized] GUIContent gc_SaveAsClip = null;
        [NonSerialized] GUIContent gc_OverrClip = null;

        /// <summary> Call S.GatherBones(); </summary>
        bool _Trigger_SoftPrepareArmature = false;
        /// <summary> Call S.GetArmature(); repaintRequest = true; S._SetDirty(); </summary>
        bool _Trigger_PrepareArmature = false;
        public static bool _exportLegacy = false;
        public static bool _forceExportGeneric = false;
        bool _foldout_rareExport = false;

        void DrawBakingTab()
        {

            if (S == null) return;
            if (S.Armature == null) return;
            if (sectionFocusMode && Category == ECategory.Setup) return;

            try
            {
                GUILayout.Space(4);

                EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);

                GUILayout.Space(4);

                if (S.Armature.BonesSetup.Count == 0 || S.Armature.BonesSetup[0].TempTransform == null)
                {
                    GUI.backgroundColor = Color.yellow;
                    if (GUILayout.Button("Prepare Armature before Saving Clips")) { _Trigger_SoftPrepareArmature = true; }
                    GUI.backgroundColor = preBG;
                    GUI.enabled = false;
                }
                else
                {
                    float wd = position.width - 92;
                    _forceExportGeneric = false;

                    if (currentMecanim)
                    {
                        if (gc_SaveAsClip == null) gc_SaveAsClip = new GUIContent("  Save As New Animation Clip", EditorGUIUtility.IconContent("AnimationClip Icon").image);

                        EditorGUILayout.BeginHorizontal();

                        if (wd < 140) wd = 140;
                        else
                        {
                            GUILayout.Space(10);
                        }

                        if (GUILayout.Button(gc_SaveAsClip, GUILayout.Height(22), GUILayout.Width(wd - 70)))
                        {
                            string newClip = SaveClipPopup(Event.current.button == 1);
                            if (string.IsNullOrEmpty(newClip) == false) { _exportLegacy = false; AddEditorEvent(() => { ExportToFile(newClip); }); }
                        }

                        if (GUILayout.Button(new GUIContent("  Save+", gc_SaveAsClip.image, "Save and remember directory it's saved in for the next saves"), GUILayout.Width(55), GUILayout.Height(22)))
                        {
                            string newClip = SaveClipPopup(Event.current.button == 1);
                            if (string.IsNullOrEmpty(newClip) == false) { _exportLegacy = false; AddEditorEvent(() => { ExportToFile(newClip, true); }); }
                        }

                        DrawBakeDebugSwitchButton();

                        EditorGUILayout.EndHorizontal();

                    }
                    else if (currentLegacy)
                    {
                        EditorGUILayout.BeginHorizontal();
                        if (wd < 140) wd = 140;
                        else
                        {
                            GUILayout.Space(10);
                        }

                        if (GUILayout.Button(new GUIContent("  Save As Legacy Animation Clip", EditorGUIUtility.IconContent("Animation Icon").image), GUILayout.Height(22), GUILayout.Width(wd)))
                        {
                            string newClip = SaveClipPopup(Event.current.button == 1);
                            if (string.IsNullOrEmpty(newClip) == false) { _exportLegacy = true; AddEditorEvent(() => { ExportToFile(newClip); }); }
                        }

                        DrawBakeDebugSwitchButton();

                        EditorGUILayout.EndHorizontal();
                    }


                }

                //if (sectionFocusMode == false)

                if (TargetClip)
                    if (LatestSaved != null)
                        if (isReady)
                            if (TargetClip.name.StartsWith(LatestSaved.name.Substring(0, Mathf.Min(LatestSaved.name.Length - 1, 4))))
                            {
                                GUILayout.Space(3);
                                EditorGUILayout.BeginHorizontal();

                                GUI.enabled = false;
                                EditorGUIUtility.labelWidth = 110;
                                EditorGUILayout.ObjectField("Latest Generated: ", LatestSaved, typeof(AnimationClip), true);
                                EditorGUIUtility.labelWidth = 0;
                                GUI.enabled = true;

                                if (gc_OverrClip == null) gc_OverrClip = new GUIContent(" Overwrite", EditorGUIUtility.IconContent("AnimationClip Icon").image);
                                if (GUILayout.Button(gc_OverrClip, GUILayout.Height(19), GUILayout.Width(80)))
                                {
                                    string newClip = AssetDatabase.GetAssetPath(LatestSaved);
                                    if (string.IsNullOrEmpty(newClip) == false) { _exportLegacy = false; AddEditorEvent(() => { ExportToFile(newClip); }); }
                                }

                                EditorGUILayout.EndHorizontal();
                            }

                GUI.enabled = true;


                if (sectionFocusMode == false)
                {
                    GUILayout.Space(4);

                    _utilTip_humanoidExportInfoTipDispl = false;
                    if (_utilTip_humanoidExportInfoTipElapsed < 10f)
                    {
                        if (TargetClip != null) if (TargetClip.isHumanMotion)
                            {
                                _utilTip_humanoidExportInfoTipDispl = true;
                                EditorGUILayout.HelpBox("Exported Humanoid Animation may look slightly different than scene animation because of Unity's humanoid rig retargeting, but in most cases it should be not noticable.", MessageType.None);
                            }
                    }


                    if (S)
                    {
                        EditorGUILayout.BeginHorizontal();

                        StartUndoCheck(": Export Settings");

                        EditorGUIUtility.labelWidth = 118;
                        EditorGUIUtility.fieldWidth = 46;
                        if (S.Export_OptimizeCurves <= 0.001f || S.Export_OptimizeCurves > 0.12f) GUI.color = new Color(1f, 1f, 0.2f, 1f);
                        S.Export_OptimizeCurves = EditorGUILayout.Slider("Optimize New File: ", S.Export_OptimizeCurves * 3.33f, 0f, 0.3f, GUILayout.Width(240)) / 3.33f;
                        GUI.color = preGuiC;
                        EditorGUIUtility.fieldWidth = 0;
                        EditorGUIUtility.labelWidth = 0;


                        #region Quality info

                        //if (sectionFocusMode == false)
                        {

                            if (S.Export_OptimizeCurves <= 0f)
                            {
                                EditorGUILayout.HelpBox("No Optimization - Giant Filesize", MessageType.None);
                            }
                            else
                            if (S.Export_OptimizeCurves < 0.02f)
                            {
                                EditorGUILayout.HelpBox("High Precision - Medium Filesize", MessageType.None);
                            }
                            else if (S.Export_OptimizeCurves > 0.065f)
                            {
                                EditorGUILayout.HelpBox("No Precision - Sliding Foots - Very Low Filesize - BAD FOR IDLE ANIMATIONS", MessageType.None);
                            }
                            else
                            {
                                EditorGUILayout.HelpBox("Small Precision - Small Filesize", MessageType.None);
                            }

                        }

                        #endregion



                        EditorGUILayout.EndHorizontal();
                        GUILayout.Space(2);

                        if (_anim_MainSet != null)
                        {
                            if (TargetClip != null)
                                if (_lastClipLooping)
                                {
                                    EditorGUILayout.BeginHorizontal();
                                    _anim_MainSet.Export_LoopAdditionalKeys = EditorGUILayout.IntSlider(new GUIContent("Loop Addional Frames:", "Trying to smooth additional frames from end of the clip towards first frame pose. In some models it will solve looping perfectly, in some it can produce some noticable body-rotation swap."), _anim_MainSet.Export_LoopAdditionalKeys, 0, 8);
                                    Anim_MainSet.Export_WrapLoopBakeMode = (ADBoneReference.EWrapBakeAlgrithmType)EditorGUILayout.EnumPopup(GUIContent.none, Anim_MainSet.Export_WrapLoopBakeMode, GUILayout.Width(39));
                                    EditorGUILayout.EndHorizontal();
                                }
                        }

                        EndUndoCheck();
                    }

                }



                GUILayout.Space(2);

                if (sectionFocusMode == false)
                {

                    EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);
                    GUILayout.Space(2);
                    if (GUILayout.Button(new GUIContent(FGUI_Resources.GetFoldSimbol(additionalBakeSettingsFoldout, true) + "   Additional Export Settings"), EditorStyles.boldLabel, GUILayout.Width(170)))
                    {
                        additionalBakeSettingsFoldout = !additionalBakeSettingsFoldout;
                    }

                    if (additionalBakeSettingsFoldout)
                    {

                        StartUndoCheck(": Export Settings");


                        GUILayout.Space(2);
                        EditorGUIUtility.labelWidth = 240;
                        S.Export_SetAllOriginalBake = EditorGUILayout.Toggle(new GUIContent("Set Root Motion Original + Baked", "Automatically switching all animation clip settings to be baked + original motion.\n(the settings of the animation clip you see in the inspector window when animation clip is selected)"), S.Export_SetAllOriginalBake);

                        if (latestAnimator.IsHuman())
                        {
                            //includeHeadKeyframes = EditorGUILayout.Toggle(new GUIContent("Include Jaw/Eyes Keyframes", "Including keyframes for eye/jaw bones with same state as in original clip. Not including will result in lower filesize."), includeHeadKeyframes);
                            //includeFingersKeyframes = EditorGUILayout.Toggle(new GUIContent("Include Fingers Keyframes", "Include fingers keyframes, not including will result in fingers in mid-fist pose and lower filesize."), includeFingersKeyframes);
                        }

                        S.Export_CopyEvents = EditorGUILayout.Toggle(new GUIContent("Copy Events", "Coppying animation clip events from original clip to new generated one"), S.Export_CopyEvents);
                        S.Export_CopyCurves = EditorGUILayout.Toggle(new GUIContent("Copy Curves", "Copy undefined curves from the original animation clip, it can be blendshapes curves or some others"), S.Export_CopyCurves);

                        GUILayout.Space(4);
                        GUIDrawFloatPercentage(ref S.Export_HipsAndLegsPrecisionBoost, new GUIContent("Hips and Legs Precision Boost", "Setting this value to full, will disable compression on the leg and hip bones to make legs stay still on ground - it will result in bigger filesize.\n\nThis option is choosing limb bones with foot IK selected (don't need to be enabled) for compression multiply."));
                        GUILayout.Space(4);
                        //GUIDrawFloatPercentage(ref S.Export_AdaptBakeFramerate, new GUIContent("Adapt Baking Framerate", "This parameter puts effect on the elasticness limbs. Setting it up to 100% will result in motion adapted to 60fps, it means that animation clip motion should look the same like in editor scene view, otherwise elasticness simulation can get more stiff when working with source animation clip framerate."));
                        //GUILayout.Space(4);


                        if (_anim_MainSet != null)
                        {
                            _anim_MainSet.Export_ForceRootMotion = EditorGUILayout.Toggle(new GUIContent("Force Original with RootMotion", "Some models don't contains root motion curves but unity applies it internally which makes detecting it problematically, in such case this toggle should be enabled"), _anim_MainSet.Export_ForceRootMotion);
                            _anim_MainSet.Export_LoopClip = (ADClipSettings_Main.ELoopClipDetection)EditorGUILayout.EnumPopup("Export with Loop:", _anim_MainSet.Export_LoopClip);
                        }

                        GUILayout.Space(4);
                        FGUI_Inspector.FoldHeaderStart(ref _foldout_rareExport, "Rare Export Settings", FGUI_Resources.BGInBoxStyle, null, 20);
                        if (_foldout_rareExport)
                        {
                            if (latestAnimator.IsHuman() == false)
                                S.Export_IncludeRootMotionInKeyAnimation = EditorGUILayout.Toggle(new GUIContent("RootMotion+Key", "Enable it if your rig requires both root motion curves and keyframe animation in the same time to work correctly with root motion (rare case)"), S.Export_IncludeRootMotionInKeyAnimation);

                            S.Export_BakeRootIndividually = EditorGUILayout.Toggle(new GUIContent("Bake Root Individually", "(Whole Setup Variable) Required if root bone is containing root motion curves and individual keyframe position/rotation animation. If it's not required, enabling this feature can cause problems in exported animation!"), S.Export_BakeRootIndividually);

                            _anim_MainSet.Export_RootMotionTangents = (ADClipSettings_Main.ERootMotionCurveAdjust)EditorGUILayout.EnumPopup("Root Motion Tangents:", _anim_MainSet.Export_RootMotionTangents);
                            //EditorGUILayout.CurveField("DebugCRV:", S.Export_DebugCurve);
                        }
                        GUILayout.EndVertical();
                        GUILayout.Space(4);

                        EditorGUIUtility.labelWidth = 0;



                        if (currentMecanim)
                        {
                            if (currentMecanim.isHuman)
                            {
                                GUILayout.Space(6);

                                EditorGUILayout.BeginHorizontal();
                                if (GUILayout.Button(new GUIContent("  Save As Generic", EditorGUIUtility.IconContent("Animator Icon").image), GUILayout.Height(22), GUILayout.Width(150)))
                                {
                                    _forceExportGeneric = true;
                                    _exportLegacy = false;
                                    string newClip = SaveClipPopup();
                                    if (string.IsNullOrEmpty(newClip) == false) { AddEditorEvent(() => { ExportToFile(newClip); }); }
                                }

                                if (GUILayout.Button(new GUIContent("  Save As Legacy", EditorGUIUtility.IconContent("Animation Icon").image), GUILayout.Height(22), GUILayout.Width(150)))
                                {
                                    _forceExportGeneric = true;
                                    _exportLegacy = true;
                                    string newClip = SaveClipPopup();
                                    if (string.IsNullOrEmpty(newClip) == false) { AddEditorEvent(() => { ExportToFile(newClip); }); }
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                        }

                        EndUndoCheck();
                    }

                    EditorGUILayout.EndVertical();
                }

                //if (GUILayout.Button("Export FBX")){}

                EditorGUILayout.EndVertical();
                GUILayout.Space(3);

            }
            catch (Exception exc)
            {
                UnityEngine.Debug.Log("exc " + exc.HResult);
                Debug.LogException(exc);
            }
        }

        static string staticExportDirectory = "";

        void DrawBakeDebugSwitchButton()
        {

            if (!debugLogs) GUI.color = new Color(0.7f, 0.7f, 0.7f, 0.8f);
            if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Debug, "Turn on/off export logs"), FGUI_Resources.ButtonStyle, GUILayout.Height(19), GUILayout.Width(22)))
            {
                debugLogs = !debugLogs;
            }
            GUI.color = preGuiC;


            if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ExportIcon, "Set constant target export directory for saving clip."), GUILayout.Width(25), GUILayout.Height(21)))
            {
                staticExportDirectory = EditorUtility.SaveFolderPanel("Choose target directory in the project for saving animation clips.", Application.dataPath, "");
                staticExportDirectory = "Assets" + staticExportDirectory.Replace(Application.dataPath, "");
                if (staticExportDirectory == "Assets") staticExportDirectory = "";
                UnityEngine.Debug.Log("static = " + staticExportDirectory);
                //UnityEngine.Debug.Log("dire " + staticExportDirectory + " Is Valid? " + AssetDatabase.IsValidFolder(staticExportDirectory));
                //UnityEngine.Debug.Log("clipPath " + AssetDatabase.GetAssetPath(Get.TargetClip));
            }
        }

        bool DrawTargetClipField(string title, bool withFoldDown = false)
        {
            bool clicked = false;
            GUILayout.Space(4);
            GUI.color = new Color(1f, 1f, 1f, 0.6f);
            EditorGUILayout.ObjectField(title, TargetClip, typeof(AnimationClip), true);
            GUI.color = preGuiC;

            var r = GUILayoutUtility.GetLastRect();
            if (GUI.Button(r, GUIContent.none, GUIStyle.none)) { clicked = true; }

            if (withFoldDown)
            {
                DrawAlreadyEditedClipsFoldDownButton();
                DrawAnimationClipsFoldDownButton();
            }

            GUILayout.Space(4);
            return clicked;
        }

        GameObject tPosePF = null;
        private GUIContent _helpC_animDesInfo = null;
        private readonly string _help_animDesSave = "Save your work in file, to get back to it any time - all settings for Elasticness, Modificators, IK will be saved for all animation clips you edited.\n\nAnimator Designer File - for each of animation designed character, DON'T create separate save files for each animation clip!";
        private readonly string _help_animDesSaveLong = "Generating new AnimationDesignerSave file in default directory.\n\nSave File stores your armature setup, limbs setup and parameters setted for animation clips you tried using with Animation Designer.\n\nYou can select your model and get back to your animation designs any time.\n\nIf you don't save designer file, setup will be lost after closing Unity Editor.\n(You don't have to save in case of doing just few small tweaks)";
        Color _c_prep = new Color(0.4f, 0.8f, 1f, 1f);

        void DrawPrepareFoundButtonIfSaveDetected()
        {
            if (FittingSaveValidated)
            {
                GUI.backgroundColor = Color.green;

                if (GUILayout.Button(new GUIContent("     Found save file of current model!\n   Switch to it? - Back To Design", Tex_AD), GUILayout.Height(36)))
                {
                    AddEditorEvent(() =>
                    {
                        ProjectFileSave = _foundFittingSave;
                        if (_foundFittingSave) if (_foundFittingSave.LatestCorrect) TargetClip = _foundFittingSave.LatestCorrect;
                    });
                }

                GUI.backgroundColor = preBG;
                GUILayout.Space(6);
            }
        }



        void DrawRootBoneField()
        {
            EditorGUILayout.BeginHorizontal();
            Transform skelRoot = (Transform)EditorGUILayout.ObjectField(new GUIContent("Skeleton Root"), S.Armature.RootBoneReference.TempTransform, typeof(Transform), true);
            if (skelRoot != S.SkelRootBone) { S.Armature.SetRootBoneRef(skelRoot); S._SetDirty(); }

            GUI.color = preGuiC;
            if (S.SkelRootBone == latestAnimator) GUILayout.Label(new GUIContent(FGUI_Resources.Tex_Warning, "Skeleton root is same as animator transform, it probably will produce glitches!"), GUILayout.Width(16));

            EditorGUILayout.EndHorizontal();
        }



        void DrawBaseToolsTab()
        {
            if (S.Armature.BonesSetup.Count == 0)
            {
                DrawPrepareFoundButtonIfSaveDetected();

                GUI.backgroundColor = _c_prep;

                string armDesTitle = "";
                if (!FittingSaveValidated) armDesTitle = "  Prepare Armature - Start New Design"; else armDesTitle = "  Prepare Armature - Continue Design";

                if (GUILayout.Button(new GUIContent(armDesTitle, FGUI_Resources.Tex_Bone), GUILayout.Height(26))) { _Trigger_PrepareArmature = true; }
                GUI.backgroundColor = preBG;
                //GUI.backgroundColor = Color.yellow;
                //if (GUILayout.Button("Prepare Armature")) { S.GetArmature(); repaintRequest = true; S._SetDirty(); }
            }
            else
            {
                if (S.Armature.BonesSetup[0].TempTransform == null)
                {
                    GUI.backgroundColor = Color.green;
                    DrawRefreshArmatureStartDesignButton(); // if (GUILayout.Button(new GUIContent("     Refresh Armature Bone References\n   Start Design", Tex_AD), GUILayout.Height(40))) { S.GatherBones(); repaintRequest = true; }
                    GUI.backgroundColor = preBG;

                    //DrawRootBoneField();
                    //for (int i = 0; i < S.Armature.BonesSetup.Count; i++) EditorGUILayout.LabelField("[" + i + "] " + S.Armature.BonesSetup[i].BoneName);
                }
            }

            if (debugTabFoldout)
            {
                GUILayout.Space(-4);
                GUI.color = Color.green * 1.2f;
                EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyleH);
                if (debugTabFoldout) GUI.color = preGuiC;
            }
            else
            {
                return;
            }

            //if (GUILayout.Button(new GUIContent(FGUI_Resources.GetFoldSimbol(debugTabFoldout, true) + "   Base Tools"), EditorStyles.boldLabel, GUILayout.Width(120)))
            //{
            //    debugTabFoldout = !debugTabFoldout;
            //}

            if (debugTabFoldout == false)
            {
                foldoutArmature = false;
                //GUILayout.Space(8);
                //RefreshArmatureButton(20);

                //FocusModeSwitchButton();
                //GizmosModsButton();
                //GizmosSwitchButton();
                //EditorGUILayout.EndHorizontal();

            }
            else
            {
                //EditorGUILayout.BeginHorizontal();


                #region Main Buttons


                GUILayout.Space(6);

                if (S.Armature.BonesSetup.Count == 0)
                {
                    //GUI.backgroundColor = preBG;
                    //GizmosModsButton();
                    //GizmosSwitchButton();
                    //EditorGUILayout.EndHorizontal();
                }
                else
                {
                    //GUILayout.FlexibleSpace();
                    //FocusModeSwitchButton();
                    //GizmosModsButton();
                    //GizmosSwitchButton();
                    //EditorGUILayout.EndHorizontal();

                    int bHeight = 22;

                    EditorGUILayout.BeginHorizontal();

                    GUI.color = Color.white * 0.8f;
                    if (GUILayout.Button(new GUIContent("  Armature", FGUI_Resources.Tex_Refresh), GUILayout.Height(bHeight))) { S.GetArmature(true); repaintRequest = true; }
                    GUI.color = preGuiC;

                    if (S.Armature.BonesSetup[0].TempTransform == null) GUI.backgroundColor = Color.green;
                    else GUI.color = Color.white * 0.8f;

                    GUILayout.Space(6);
                    if (GUILayout.Button(new GUIContent("   Refresh Armature Bone References", FGUI_Resources.Tex_Refresh), GUILayout.Height(bHeight))) { S.GatherBones(); repaintRequest = true; }
                    GUI.backgroundColor = preBG;
                    GUI.color = preGuiC;
                    EditorGUILayout.EndHorizontal();


                    GUILayout.Space(8);
                    EditorGUILayout.LabelField("T-Pose is used by IK to work correctly", EditorStyles.centeredGreyMiniLabel);
                    GUILayout.Space(5);


                    EditorGUILayout.BeginHorizontal();
                    if (S.TPose.BonesCoords.Count == 0)
                    {
                        if (GUILayout.Button("Prepare T-Pose", GUILayout.Height(bHeight)))
                        {
                            S.CaptureTPose(); repaintRequest = true;
                        }
                    }
                    else
                    {
                        if (GUILayout.Button(new GUIContent("  T-Pose", FGUI_Resources.Tex_Refresh), GUILayout.Height(bHeight)))
                        {
                            if (EditorUtility.DisplayDialog("Overwrite T-Pose", "Are you sure that character is setted in T-Pose? (Previous T-Pose setup will be lost)", "Yes, now it's correct!", "No!"))
                            {
                                S.CaptureTPose(); repaintRequest = true;
                            }
                        }

                        GUILayout.Space(6);

                        if (GUILayout.Button("Check model T-Pose", GUILayout.Height(bHeight)))
                        {
                            S.RestoreTPose(); repaintRequest = true;
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(7);


                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent("Recovering Lost T-Pose:", "If you setted wrong T-Pose by mistake, you can try setting bones coordinates like prefab or model file"));

                    if (latestAnimator != null)
                    {
                        string pth = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(latestAnimator.gameObject);

                        if (string.IsNullOrEmpty(pth))
                        {
                            if (S)
                            {
                                UnityEngine.Object av = S.TargetAvatar;
                                if (!av) av = latestAnimator.GetAvatar();
                                if (av) { pth = AssetDatabase.GetAssetPath(av); }
                            }
                        }

                        if (string.IsNullOrEmpty(pth) == false)
                        {
                            if (GUILayout.Button(new GUIContent(FGUI_Resources.TexTargetingIcon, "Trying to find model with T-Pose in project browser (using right mouse button will try automatically set new T-Pose)"), FGUI_Resources.ButtonStyle, GUILayout.Width(19), GUILayout.Height(17)))
                            {
                                GameObject aObj = AssetDatabase.LoadAssetAtPath<GameObject>(pth);
                                if (aObj) EditorGUIUtility.PingObject(aObj);

                                if (aObj)
                                    if (Event.current.button == 1)
                                        tPosePF = aObj;
                            }
                        }
                        else
                        {

                        }
                    }


                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();

                    tPosePF = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Read From Model File: ", "If you setted wrong T-Pose by mistake, you can try setting bones coordinates like model file or prefab file"), tPosePF, typeof(GameObject), true);

                    if (tPosePF)
                    {
                        bool done = S.TrySettingPoseFromObject(tPosePF);

                        if (done)
                        {
                            repaintRequest = true;
                            S.CaptureTPose();
                        }

                        if (done)
                        {
                            EditorUtility.DisplayDialog("Restoring T-Pose", "Resoring T-Pose Done and Setted!", "Ok");
                        }
                        else
                            EditorUtility.DisplayDialog("Restoring T-Pose", "Couldn't restore T-Pose out of " + tPosePF.name + " object. Bone names or bones structure is different or there is no Animator component!", "Ok");

                        tPosePF = null;
                    }

                    EditorGUILayout.EndHorizontal();


                    if (Ar != null)
                    {
                        if (!Ar.Humanoid)
                        {
                            GUILayout.Space(6);

                            AnimationClip armatureVerifyWith = null;
                            EditorGUIUtility.labelWidth = 310;
                            armatureVerifyWith = (AnimationClip)EditorGUILayout.ObjectField(new GUIContent("Verify Armature Using Animation Clip Of The Model: ", "You can verify model-baking bindings, using one of the animation clips for this model."), armatureVerifyWith, typeof(AnimationClip), true);
                            EditorGUIUtility.labelWidth = 0;

                            if (armatureVerifyWith != null)
                            {
                                Ar.VerifyArmatureWithAnimationClip(armatureVerifyWith, true, S);
                            }
                        }

                        if (Ar.Humanoid) Ar.UseRootBoneForAvatar = EditorGUILayout.Toggle(new GUIContent("Use Root Bone For Avatar: ", "Some humanoid rigs may require using other root bone than animator transform for correct export.\n(Using 'Skeleton Root' field)"), Ar.UseRootBoneForAvatar);
                    }

                    GUILayout.Space(8);

                    if (GUILayout.Button("Armature Report: " + S.Armature.BonesSetup.Count + " Bones", EditorStyles.centeredGreyMiniLabel))
                    {
                        foldoutArmature = !foldoutArmature;
                    }

                    if (foldoutArmature)
                    {
                        GUIContent guiC_remove = new GUIContent(FGUI_Resources.Tex_Remove, "Remove bone from the armature list. !Be sure you know what you're doing!");
                        GUILayoutOption[] gC_opt = new GUILayoutOption[] { GUILayout.Width(24), GUILayout.Height(18) };

                        EditorGUILayout.BeginVertical(FGUI_Resources.ViewBoxStyle);
                        int toRemove = -1;
                        for (int i = 0; i < S.Armature.BonesSetup.Count; i++)
                        {
                            Transform preT = S.Armature.BonesSetup[i].TempTransform;

                            EditorGUILayout.BeginHorizontal();

                            S.Armature.BonesSetup[i].TempTransform = (Transform)EditorGUILayout.ObjectField(S.Armature.BonesSetup[i].TempTransform, typeof(Transform), true);

                            if (preT != S.Armature.BonesSetup[i].TempTransform)
                            {
                                S.Armature.BonesSetup[i] = new ADBoneReference(S.Armature.BonesSetup[i].TempTransform, i, latestAnimator);
                            }

                            //S.Armature.BonesSetup[i].CompressionFactor = GUILayout.HorizontalSlider(S.Armature.BonesSetup[i].CompressionFactor, 0f,1f, GUILayout.Width(100));

                            if (S.Armature.BonesSetup[i].HumanoidBoneDefined == false)
                            {
                                EditorGUI.BeginChangeCheck();
                                GUILayout.Space(4);
                                EditorGUIUtility.labelWidth = 45;
                                S.Armature.BonesSetup[i].BakeBone = EditorGUILayout.Toggle("Bake:", S.Armature.BonesSetup[i].BakeBone, GUILayout.Width(68));
                                EditorGUIUtility.labelWidth = 0;

                                if (S.Armature.BonesSetup[i].BakeBone)
                                {
                                    EditorGUIUtility.labelWidth = 82;
                                    S.Armature.BonesSetup[i].BakePosition = EditorGUILayout.Toggle("Bake Position:", S.Armature.BonesSetup[i].BakePosition, GUILayout.Width(100));
                                }

                                if (EditorGUI.EndChangeCheck())
                                {
                                    EditorUtility.SetDirty(S);
                                    so_currentSetup.ApplyModifiedProperties();//
                                }
                                EditorGUIUtility.labelWidth = 0;
                            }

                            if (GUILayout.Button(guiC_remove, FGUI_Resources.ButtonStyle, gC_opt))
                            {
                                toRemove = i;
                            }

                            EditorGUILayout.EndHorizontal();
                        }

                        if (toRemove != -1)
                        {
                            AddEditorEvent(() => { S.Armature.BonesSetup.RemoveAt(toRemove); });
                        }

                        GUILayout.Space(8);
                        if (GUILayout.Button(" + Add Custom Bone to the Armature + ")) { S.Armature.BonesSetup.Add(new ADBoneReference(null, -1, latestAnimator)); }
                        GUILayout.Space(2);

                        EditorGUILayout.EndVertical();
                    }

                    GUILayout.Space(5);

                }

                #endregion


            }

            if (debugTabFoldout == false) GUILayout.Space(-2);

            EditorGUILayout.EndVertical();

        }

        bool foldoutArmature = false;
        void RefreshArmatureButton(int height)
        {
            if (S.Armature.BonesSetup.Count == 0)
            {
                DrawPrepareFoundButtonIfSaveDetected();

                GUI.backgroundColor = _c_prep;
                if (GUILayout.Button(new GUIContent("  Prepare Armature", FGUI_Resources.Tex_Bone), GUILayout.Height(24))) { _Trigger_PrepareArmature = true; }
                GUI.backgroundColor = preBG;
            }
            else
            {
                if (S.Armature.BonesSetup[0].TempTransform == null) GUI.backgroundColor = Color.green;
                DrawRefreshArmatureStartDesignButton(); // if (GUILayout.Button(new GUIContent("  Refresh Armature Bone References", FGUI_Resources.Tex_Refresh), GUILayout.Height(height))) { S.GatherBones(); repaintRequest = true; }
                GUI.backgroundColor = preBG;
            }
        }

        void DrawRefreshArmatureStartDesignButton()
        {

            if (GUILayout.Button(new GUIContent("     Refresh Armature Bone References\n   Continue Design", Tex_AD), GUILayout.Height(40))) { S.GatherBones(); _serializationChanges = true; repaintRequest = true; }

        }

        void RefreshArmatureSaveAndSkeletonSetup()
        {
            _serializationChanges = true;
            _reloadingAlpha = 0f;

            if (S.Armature.BonesSetup.Count == 0)
                S.GetArmature();
            else
                S.GatherBones();

            S._SetDirty();
            repaintRequest = true;
        }

    }

}