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
        public List<ADClipSettings_CustomModules> CustomModules = new List<ADClipSettings_CustomModules>();

    }

    /// <summary>
    /// Hips setups for single AnimationClip
    /// </summary>
    [System.Serializable]
    public partial class ADClipSettings_CustomModules : IADSettings
    {
        public AnimationClip settingsForClip;
        public List<CustomModuleSet> CustomModules = new List<CustomModuleSet>();

        public bool TurnOnModules = true;
        public float AllModulesBlend = 1f;

        public ADClipSettings_CustomModules() { }

        internal void ResetState()
        {
            for (int i = 0; i < CustomModules.Count; i++)
            {
                var cModl = CustomModules[i];
                cModl.OnResetState();
            }
        }

        /// <summary> Used for identifying different variants when modifying the same AnimationClip multiple times </summary>
        [SerializeField, HideInInspector] private string setId = "";
        [SerializeField, HideInInspector] private int setIdHash = 0;
        public void AssignID(string name) { setId = name; setIdHash = setId.GetHashCode(); }
        public string SetID { get { return setId; } }
        public int SetIDHash { get { return setIdHash; } }
        public AnimationClip SettingsForClip { get { return settingsForClip; } }
        public void OnConstructed(AnimationClip clip, int hash) { settingsForClip = clip; setIdHash = hash; }


        public void RefreshWithSetup(AnimationDesignerSave save) { }


        internal void CheckInitialization(AnimationDesignerSave s)
        {
            for (int i = 0; i < CustomModules.Count; i++)
            {
                CustomModules[i].CheckInitialization(s);
            }
        }

        internal void AddNewModule(AnimationDesignerSave setup)
        {
            CustomModuleSet newSet = new CustomModuleSet();
            if (setup != null) setup._SetDirty();
            CustomModules.Add(newSet);
        }



        #region Backup

        //internal static void PasteValuesTo(ADClipSettings_Modificators from, ADClipSettings_Modificators to)
        //{
        //    for (int i = 0; i < from.BonesModificators.Count; i++)
        //        if (from.BonesModificators[i].Index == to.BonesModificators[i].Index)
        //            ModificatorSet.PasteValuesTo(from.BonesModificators[i], to.BonesModificators[i]);
        //}

        #endregion


        [System.Serializable]
        public partial class CustomModuleSet : INameAndIndex
        {
            public ADCustomModuleBase ModuleReference;

            /// <summary> Can be used to define if some variables needs to be refreshed </summary>
            public string ModuleIDHelper = "";

            AnimationDesignerSave S { get { return AnimationDesignerWindow.Get.S; } }
            ADArmatureSetup Ar { get { return AnimationDesignerWindow.Get.Ar; } }


            public bool SupportingBlending
            {
                get
                {
                    if (ModuleReference == null) return false;
                    return ModuleReference.SupportBlending;
                }
            }

            public List<ADVariable> ModuleVariables = new List<ADVariable>();
            public List<ADTransformMemory> TransformsMemory = new List<ADTransformMemory>();


            internal void CheckInitialization(AnimationDesignerSave s)
            {

            }


            public string ModName;
            public string GetName { get { return ModName; } }

            public int Index;

            public int GetIndex { get { return Index; } }

            public float GUIAlpha { get { if (Enabled == false) return 0.1f; if (Blend < 0.5f) return 0.2f + Blend; return 1f; } }


            public CustomModuleSet()
            {
                ModName = "Module Processor";

                Enabled = true;
                Blend = 1f;
                BlendEvaluation = AnimationCurve.EaseInOut(0f, 1f, 1f, 1f);
            }

            // Settings
            public bool Enabled = false;
            public float Blend = 1f;
            public AnimationCurve BlendEvaluation;

            [NonSerialized] public bool RemoveMe = false;
            /// <summary> Non Serialized false at reload, helper for refresh on project start </summary>
            [NonSerialized] public bool HelpFlag = false;
            public bool Foldown = false;



            #region Module Forwarding Methods

            internal void OnPreUpdateSampling(AnimationDesignerSave s, ADClipSettings_Main anim_MainSet, ref float animProgress, ref float animationProgressClipTime)
            {
                if (ModuleReference == null) return;
                ModuleReference.OnPreUpdateSampling(s, anim_MainSet, this, ref animProgress, ref animationProgressClipTime);
            }

            internal void OnPreUpdateSamplingMorph(AnimationDesignerSave s, AnimationClip clip, ADClipSettings_Morphing.MorphingSet morphSet, ref float clipTime)
            {
                if (ModuleReference == null) return;
                ModuleReference.OnPreUpdateSamplingMorph(s, clip, morphSet, this, ref clipTime);
            }

            internal void OnResetState()
            {
                if (ModuleReference == null) return;
                ModuleReference.OnResetState(this);
            }


            #endregion



            #region GUI Related

            internal void DrawHeaderGUI(List<CustomModuleSet> modsList, bool advanced)
            {
                Color preBg = GUI.backgroundColor;
                EditorGUILayout.BeginHorizontal();

                Enabled = EditorGUILayout.Toggle(Enabled, GUILayout.Width(24));

                if (ModuleReference != null)
                {
                    if (ModuleReference.GUIFoldable)
                        if (GUILayout.Button(FGUI_Resources.GetFoldSimbol(Foldown, true), EditorStyles.label, GUILayout.Width(22), GUILayout.Height(19)))
                        {
                            Foldown = !Foldown;
                        }
                }

                ModName = EditorGUILayout.TextField(ModName);

                EditorGUI.BeginChangeCheck();
                var lastModule = ModuleReference;
                ModuleReference = (ADCustomModuleBase)EditorGUILayout.ObjectField(ModuleReference, typeof(ADCustomModuleBase), true);
                if (EditorGUI.EndChangeCheck())
                {
                    if (ModuleReference)
                    {
                        if (ModuleReference != lastModule)
                        {
                            ModuleVariables.Clear();
                            TransformsMemory.Clear();
                        }

                        ModuleReference.OnSetupChange(this);
                    }

                    EditorUtility.SetDirty(S);
                }

                GUILayout.Space(4);


                #region Left - Right Arrow Keys

                //if (advanced)
                //    if (modsList != null)
                //    {
                //        if (Index > 0)
                //            if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowLeft, "Moving modificator to be executed before other modificators"), FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(19)))
                //            {
                //                modsList[Index] = modsList[Index - 1];
                //                modsList[Index - 1] = this;
                //            }

                //        if (Index < modsList.Count - 1)
                //            if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowRight, "Moving modificator to be executed after other modificators"), FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(19)))
                //            {
                //                modsList[Index] = modsList[Index + 1];
                //                modsList[Index + 1] = this;
                //            }
                //    }

                #endregion

                #region Copy Paste Buttons

                //if (CopyingFrom != null)
                //{
                //    if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("Clipboard").image, "Paste copied values"), FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(19)))
                //    {
                //        PasteValuesTo(CopyingFrom, this);
                //    }
                //}

                //if (CopyingFrom == this) GUI.backgroundColor = new Color(0.6f, 1f, 0.6f, 1f);
                //if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("TreeEditor.Duplicate").image, "Copy spring parameters values below to paste them into other spring"), FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(19)))
                //{
                //    CopyingFrom = this;
                //}
                //if (CopyingFrom == this) GUI.backgroundColor = preBg;

                #endregion


                if (ModuleReference != null)
                {
                    if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Rename, "Opens popup for renaming Custom Module filename"), EditorStyles.label, GUILayout.Width(20), GUILayout.Height(19))) FGenerators.RenamePopup(ModuleReference);

                    if (ModuleReference.SaveDirectory)
                    {
                        //GUI.color = Color.white * 0.89f;
                        //if (GUILayout.Button(new GUIContent("+", "Generate new separated settings file for selected module")))
                        //{
                        //    GenerateInstanceOf(ModuleReference);
                        //}
                        //GUI.color = Color.white;
                    }

                    GUILayout.Space(4);
                }




                // Quick selector foldown

                #region Button to display menu of draft setup files

                UnityEngine.Object modulesDirectory = AnimationDesignerWindow.Get.ModuleSetupsDirectory;
                if (modulesDirectory)
                {
                    if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_DownFold, "Display quick selection menu for Module Instances contained in the target directory"), EditorStyles.label, GUILayout.Width(16), GUILayout.Height(16)))
                    {
                        string path = AssetDatabase.GetAssetPath(modulesDirectory);
                        var files = System.IO.Directory.GetFiles(path, "*.asset");
                        if (files != null)
                        {
                            GenericMenu draftsMenu = new GenericMenu();
                            draftsMenu.AddItem(new GUIContent("None"), ModuleReference == null, () => { ModuleReference = null; });

                            for (int i = 0; i < files.Length; i++)
                            {
                                ADCustomModuleBase cModule = AssetDatabase.LoadAssetAtPath<ADCustomModuleBase>(files[i]);
                                if (cModule == null) continue;

                                string name = cModule.name;
                                name = name.Replace("ADM-", "");
                                name = name.Replace("_", "/");

                                if (cModule) draftsMenu.AddItem(new GUIContent(name), ModuleReference == cModule, () => { ModuleReference = cModule; cModule.OnSetupChange(this); });
                            }

                            draftsMenu.ShowAsContext();
                        }
                    }

                    GUILayout.Space(3);
                }


                #endregion




                if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Expose, "Create setup file of scripted modules."), FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(18)))
                {
                    GenericMenu menu = new GenericMenu();

                    var mods = ADCustomModuleBase.GetCustomModulesTypes();

                    for (int t = 0; t < mods.Count; t++)
                    {
                        Type modType = mods[t];
                        ScriptableObject scr = ScriptableObject.CreateInstance(modType);
                        ADCustomModuleBase cModIns = (ADCustomModuleBase)scr;
                        if (cModIns) menu.AddItem(new GUIContent("New '" + cModIns.ModuleTitleName + "' Setup File"), false, () => { GenerateInstanceOf(cModIns); });
                    }

                    menu.AddItem(GUIContent.none, false, () => { });
                    menu.AddItem(new GUIContent("+ Create Custom Module Script File"), false, () => { GenerateCustomModuleTemplate(); });

                    menu.ShowAsContext();
                }

                GUILayout.Space(2);
                GUI.backgroundColor = new Color(1f, 0.5f, 0.5f, 1f);
                if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Remove), FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(19)))
                {
                    RemoveMe = true;
                }
                GUI.backgroundColor = preBg;

                EditorGUILayout.EndHorizontal();

                GUILayout.Space(5);
            }


            void GenerateCustomModuleTemplate()
            {
                // EditorUtility.DisplayDialog("Info", "Template Not Yet Implemented!", "Ok");
                TextAsset text = Resources.Load("AnimationDesigner/ADCustomModuleTemplate.cs") as TextAsset;

                if (text == null || AnimationDesignerWindow.Get.CustomModuleScriptFilesDirectory == null)
                {
                    EditorUtility.DisplayDialog("Info", "Template File Not Found!", "Ok");
                    return;
                }

                string path = AssetDatabase.GetAssetPath(text);
                string fullPath = Application.dataPath + path.Replace("Assets", "");

                //string path = AssetDatabase.GetAssetPath(AnimationDesignerWindow.Get.CustomModuleScriptFilesDirectory);
                //path = EditorUtility.SaveFilePanelInProject("Generate Animation Designer Custom Module Script File", "CustomModuleScript", "cs", "Enter name of file", path);

                //string fileName = System.IO.Path.GetFileName(path);

                //string fullPath = Application.dataPath + path.Replace("Assets", "");

                //if (string.IsNullOrWhiteSpace(fullPath))
                //{
                //    EditorUtility.DisplayDialog("Info", "Wrong Template Path!", "Ok");
                //    return;
                //}

                if (System.IO.File.Exists(fullPath))
                {
#if UNITY_2019_1_OR_NEWER
                    UnityEditor.ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "ADCustomModuleScript.cs");
#else
                typeof(UnityEditor.ProjectWindowUtil)
                    .GetMethod("CreateScriptAsset", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                    .Invoke(null, new object[] { path, "ADCustomModuleScript.cs" });
#endif
                }
                else
                    Debug.LogError("File under path '" + fullPath + "' doesn't exist, directory probably was moved");
            }


            void GenerateInstanceOf(ADCustomModuleBase mod)
            {
                if (mod == null) return;

                string path;
                path = AssetDatabase.GetAssetPath(mod.SaveDirectory);

                string pathName = mod.ModuleTitleName.Replace("/", " ");

                if (string.IsNullOrWhiteSpace(path) == false)
                {
                    var files = System.IO.Directory.GetFiles(path, "*.asset");
                    path += "/ADM-" + pathName + (files.Length + 1) + ".asset";
                }
                else
                    path = "";

                ADCustomModuleBase scrInstance = ScriptableObject.Instantiate(mod);

                if (string.IsNullOrEmpty(path))
                    path = FGenerators.GenerateScriptablePath(scrInstance, "ADM-" + pathName);

                if (!string.IsNullOrEmpty(path))
                {
                    UnityEditor.AssetDatabase.CreateAsset(scrInstance, path);
                    AssetDatabase.SaveAssets();
                }
                else
                    scrInstance = null;

                if (scrInstance != null)
                    ModuleReference = scrInstance;
            }

            internal void DrawSceneHandles(float v, float progr)
            {
                if (ModuleReference == null) return;
                ModuleReference.SceneView_DrawSceneHandles(this, v, progr);
            }

            internal void DrawTopGUI(float animationProgress)
            {
                if (!Enabled) GUI.enabled = false;

                bool drawBlending = false;

                if (ModuleReference != null)
                {
                    drawBlending = ModuleReference.SupportBlending;
                    if (drawBlending == false && !ModuleReference.GUIFoldable) Foldown = false;
                }

                if (!Foldown)
                {
                    if (drawBlending)
                    {
                        EditorGUILayout.BeginHorizontal();
                        AnimationDesignerWindow.GUIDrawFloatPercentage(ref Blend, new GUIContent("Module Blend:  "));
                        AnimationDesignerWindow.DrawCurve(ref BlendEvaluation, "", 120);
                        EditorGUILayout.EndHorizontal();
                    }
                }
                else
                {
                    if (drawBlending)
                    {
                        AnimationDesignerWindow.GUIDrawFloatPercentage(ref Blend, new GUIContent("Module Blend:  "));
                        AnimationDesignerWindow.DrawCurve(ref BlendEvaluation, "Blend Along Clip Time:");
                    }

                    if (ModuleReference != null) ModuleReference.InspectorGUI_HeaderFoldown(this);
                }

                GUILayout.Space(4);

                if (ModuleReference)
                {
                    ModuleReference.InspectorGUI_Header(animationProgress, this);
                }

                GUI.enabled = true;
            }

            internal void DrawParamsGUI(float progress, ADClipSettings_Main _anim_MainSet, AnimationDesignerSave s, ADClipSettings_CustomModules cModule)
            {
                if (!Enabled) GUI.enabled = false;
                Color preC = GUI.color;

                GUI.color = new Color(0f, 0f, 0f, 0.5f);
                EditorGUILayout.BeginVertical(FGUI_Resources.ViewBoxStyle);
                EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxBlankStyle);
                GUI.color = preC;

                if (ModuleReference == null)
                {
                    EditorGUILayout.HelpBox("  No Module Reference to Process!", MessageType.Info);
                }
                else
                {
                    ModuleReference.InspectorGUI_ModuleBody(progress, _anim_MainSet, s, cModule, this);
                }

                EditorGUILayout.EndVertical();
                GUI.enabled = true;
                EditorGUILayout.EndVertical();
            }


            #endregion


        }

        internal void AfterLateUpdateModules(float deltaTime, float animationProgress, AnimationDesignerSave s, ADClipSettings_Main anim_MainSet)
        {
            for (int i = 0; i < CustomModules.Count; i++)
            {
                if (CustomModules[i].Enabled == false) continue;
                if (CustomModules[i].ModuleReference == null) continue;
                var mod = CustomModules[i].ModuleReference;
                mod.OnLastUpdate(animationProgress, deltaTime, s, anim_MainSet, this, CustomModules[i]);
            }
        }

        internal void LateUpdateModules(float deltaTime, float animationProgress, AnimationDesignerSave s, ADClipSettings_Main anim_MainSet)
        {
            for (int i = 0; i < CustomModules.Count; i++)
            {
                if (CustomModules[i].Enabled == false) continue;
                if (CustomModules[i].ModuleReference == null) continue;
                var mod = CustomModules[i].ModuleReference;
                mod.OnLateUpdate(animationProgress, deltaTime, s, anim_MainSet, this, CustomModules[i]);
            }
        }

        internal void BeforeElasticnessLateUpdateModules(float deltaTime, float animationProgress, AnimationDesignerSave s, ADClipSettings_Main anim_MainSet)
        {
            for (int i = 0; i < CustomModules.Count; i++)
            {
                if (CustomModules[i].Enabled == false) continue;
                if (CustomModules[i].ModuleReference == null) continue;
                var mod = CustomModules[i].ModuleReference;
                mod.OnInheritElasticnessUpdate(animationProgress, deltaTime, s, anim_MainSet, this, CustomModules[i]);
            }
        }

        internal void BeforeIKUpdateModules(float deltaTime, float animationProgress, AnimationDesignerSave s, ADClipSettings_Main anim_MainSet)
        {
            for (int i = 0; i < CustomModules.Count; i++)
            {
                if (CustomModules[i].Enabled == false) continue;
                if (CustomModules[i].ModuleReference == null) continue;
                var mod = CustomModules[i].ModuleReference;
                mod.OnBeforeIKUpdate(animationProgress, deltaTime, s, anim_MainSet, this, CustomModules[i]);
            }
        }

        internal void OnInfluenceIKUpdateModules(float deltaTime, float animationProgress, AnimationDesignerSave s, ADClipSettings_Main anim_MainSet)
        {
            for (int i = 0; i < CustomModules.Count; i++)
            {
                if (CustomModules[i].Enabled == false) continue;
                if (CustomModules[i].ModuleReference == null) continue;
                var mod = CustomModules[i].ModuleReference;
                mod.OnInfluenceIKUpdate(animationProgress, deltaTime, s, anim_MainSet, this, CustomModules[i]);
            }
        }

        internal void OnExportFinalizeModules(AnimationClip originalClip, AnimationClip targetClip, AnimationDesignerSave s, ADClipSettings_Main anim_MainSet, List<AnimationEvent> addingEvents)
        {
            if (CustomModules == null) return;

            for (int i = 0; i < CustomModules.Count; i++)
            {
                if (CustomModules[i] == null) continue;
                if (CustomModules[i].Enabled == false) continue;
                if (CustomModules[i].ModuleReference == null) continue;
                var mod = CustomModules[i].ModuleReference;
                mod.OnExportFinalizing(originalClip, targetClip, s, anim_MainSet, this, CustomModules[i], addingEvents);
            }
        }
    }
}