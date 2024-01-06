#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace Michsky.LSS
{
    [CustomEditor(typeof(LSS_Manager))]
    [System.Serializable]
    public class LSS_ManagerEditor : Editor
    {
        private LSS_Manager lssmTarget;
        private GUISkin customSkin;
        private string mainScene;
        List<string> lsList = new List<string>();

        private void OnEnable()
        {
            lssmTarget = (LSS_Manager)target;
            lssmTarget.loadingScreens = Resources.LoadAll("Loading Screens", typeof(GameObject));

            foreach (var t in lssmTarget.loadingScreens) { lsList.Add(t.name); }

            if (EditorGUIUtility.isProSkin == true) { customSkin = LSS_EditorHandler.GetDarkEditor(customSkin); }
            else { customSkin = LSS_EditorHandler.GetLightEditor(customSkin); }
        }

        public override void OnInspectorGUI()
        {
            var loadingMode = serializedObject.FindProperty("loadingMode");
            var presetName = serializedObject.FindProperty("presetName");

            var enableTrigger = serializedObject.FindProperty("enableTrigger");
            var onTriggerExit = serializedObject.FindProperty("onTriggerExit");
            var loadWithTag = serializedObject.FindProperty("loadWithTag");
            var startLoadingAtStart = serializedObject.FindProperty("startLoadingAtStart");
            var sceneName = serializedObject.FindProperty("sceneName");
            var objectTag = serializedObject.FindProperty("objectTag");
            var selectedLoadingIndex = serializedObject.FindProperty("selectedLoadingIndex");
            var selectedTagIndex = serializedObject.FindProperty("selectedTagIndex");

            var audioFadeDuration = serializedObject.FindProperty("audioFadeDuration");
            var audioSources = serializedObject.FindProperty("audioSources");

            var onLoadingStart = serializedObject.FindProperty("onLoadingStart");

            var dontDestroyOnLoad = serializedObject.FindProperty("dontDestroyOnLoad");

            var loadedScenes = serializedObject.FindProperty("loadedScenes");
            var lockSelection = serializedObject.FindProperty("lockSelection");

            LSS_EditorHandler.DrawHeader(customSkin, "Resources Header", 6);
            LSS_EditorHandler.DrawProperty(loadingMode, customSkin, "Loading Mode", "Set the preferred loading mode. See docs for more information.");

            if (lsList.Count == 1 || lsList.Count >= 1)
            {
                if (selectedLoadingIndex.intValue > lsList.Count) { lssmTarget.selectedLoadingIndex = 0; }

                if (lockSelection.boolValue == true)
                {
                    GUI.enabled = false;
                    LSS_EditorHandler.DrawProperty(presetName, customSkin, "Selected Preset", "Select a loading screen preset.");
                    GUI.enabled = true;
                }

                else
                {
                    GUILayout.BeginHorizontal(EditorStyles.helpBox);
                    EditorGUILayout.LabelField(new GUIContent("Selected Preset"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                    selectedLoadingIndex.intValue = EditorGUILayout.Popup(selectedLoadingIndex.intValue, lsList.ToArray());
                    presetName.stringValue = lssmTarget.loadingScreens.GetValue(selectedLoadingIndex.intValue).ToString().Replace(" (UnityEngine.GameObject)", "").Trim();
                    GUILayout.EndHorizontal();
                }

                if (lssmTarget.loadingMode == LSS_Manager.LoadingMode.Additive && Application.isPlaying)
                {
                    GUI.enabled = false;
                    mainScene = SceneManager.GetActiveScene().name;
                    GUILayout.BeginHorizontal(EditorStyles.helpBox);
                    EditorGUILayout.LabelField(new GUIContent("Main Scene"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                    EditorGUILayout.TextField(mainScene);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUI.indentLevel = 1;
                    EditorGUILayout.PropertyField(loadedScenes, new GUIContent("Loaded Scenes"), true);
                    loadedScenes.isExpanded = true;
                    GUILayout.EndHorizontal();
                    GUI.enabled = true;
                }

                if (GUILayout.Button("Show Selected Screen", customSkin.button))
                    Selection.activeObject = Resources.Load("Loading Screens/" + lsList[lssmTarget.selectedLoadingIndex]);

                LSS_EditorHandler.DrawHeader(customSkin, "Settings Header", 10);
                lockSelection.boolValue = LSS_EditorHandler.DrawToggle(lockSelection.boolValue, customSkin, "Lock Preset Selection", "Lock the preset selection to avoid losing the order when adding a new preset.");

                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Space(-3);
                startLoadingAtStart.boolValue = LSS_EditorHandler.DrawTogglePlain(startLoadingAtStart.boolValue, customSkin, "Start Loading At Start", "Start loading process immediately after the component is initialized.");
                GUILayout.Space(3);

                if (startLoadingAtStart.boolValue == true)
                {
                    LSS_EditorHandler.DrawPropertyCW(sceneName, customSkin, "Load Scene", "Scene to be loaded.", 80);
                    GUI.enabled = false;
                }

                GUILayout.EndVertical();

                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Space(-3);
                enableTrigger.boolValue = LSS_EditorHandler.DrawTogglePlain(enableTrigger.boolValue, customSkin, "Load Using Trigger", "Use a collider/trigger to do the loading process.");
                GUILayout.Space(3);

                if (enableTrigger.boolValue == true)
                {
                    LSS_EditorHandler.DrawPropertyCW(sceneName, customSkin, "Load Scene", "Scene to be loaded.", 80);
                    onTriggerExit.boolValue = LSS_EditorHandler.DrawToggle(onTriggerExit.boolValue, customSkin, "On Trigger Exit", "Use OnTriggerExit instead of OnTriggerEnter.");

                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.Space(-3);
                    loadWithTag.boolValue = LSS_EditorHandler.DrawTogglePlain(loadWithTag.boolValue, customSkin, "Load With Tag", "Only use the loading process with the tags specified.");
                    GUILayout.Space(3);

                    if (lssmTarget.loadWithTag == true)
                    {
                        GUILayout.BeginHorizontal(EditorStyles.helpBox);

                        EditorGUILayout.LabelField(new GUIContent("Object Tag"), customSkin.FindStyle("Text"), GUILayout.Width(80));
                        selectedTagIndex.intValue = EditorGUILayout.Popup(selectedTagIndex.intValue, UnityEditorInternal.InternalEditorUtility.tags);
                        objectTag.stringValue = UnityEditorInternal.InternalEditorUtility.tags[selectedTagIndex.intValue].ToString();

                        GUILayout.EndHorizontal();
                    }

                    GUILayout.EndVertical();
                }

                if (enableTrigger.boolValue == true && lssmTarget.GetComponent<Collider>() == null)
                {
                    EditorGUILayout.HelpBox("You need to add a collider to use this feature. You can either add a collider manually or hit the button below.", MessageType.Warning);
                    if (GUILayout.Button("+ Create Collider"))
                    {
                        lssmTarget.gameObject.AddComponent<BoxCollider>();
                        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                    }
                }

                GUILayout.EndVertical();
                GUI.enabled = true;

                LSS_EditorHandler.DrawHeader(customSkin, "Smooth Audio Header", 10);
                LSS_EditorHandler.DrawPropertyCW(audioFadeDuration, customSkin, "Audio Fade Duration", "Set the smooth audio fade duration in seconds.", 134);
                GUILayout.BeginHorizontal();
                EditorGUI.indentLevel = 1;
                EditorGUILayout.PropertyField(audioSources, new GUIContent("Fade Audio Sources"), true);
                EditorGUI.indentLevel = 0;
                GUILayout.EndHorizontal();

                LSS_EditorHandler.DrawHeader(customSkin, "Events Header", 10);
                EditorGUILayout.PropertyField(onLoadingStart, new GUIContent("On Loading Start"), true);
                GUILayout.BeginHorizontal();
                EditorGUI.indentLevel = 1;
                EditorGUILayout.PropertyField(dontDestroyOnLoad, new GUIContent("Don't Destroy On Load"), true);
                EditorGUI.indentLevel = 0;
                GUILayout.EndHorizontal();
            }

            else
                    EditorGUILayout.HelpBox("There is no loading screen prefab in the Resoures > Loading Screens folder." +
                        "You need to create at least a single prefab to use the loading screen system.", MessageType.Warning);

            serializedObject.ApplyModifiedProperties();
            if (Application.isPlaying == false) { Repaint(); }
        }
    }
}
#endif