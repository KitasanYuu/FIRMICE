#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.UI.Heat
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(LocalizationManager))]
    public class LocalizationManagerEditor : Editor
    {
        private LocalizationManager lmTarget;
        private GUISkin customSkin;

        private void OnEnable()
        {
            lmTarget = (LocalizationManager)target;

            if (EditorGUIUtility.isProSkin == true) { customSkin = HeatUIEditorHandler.GetDarkEditor(customSkin); }
            else { customSkin = HeatUIEditorHandler.GetLightEditor(customSkin); }
        }

        public override void OnInspectorGUI()
        {
            var UIManagerAsset = serializedObject.FindProperty("UIManagerAsset");
            var languageSelector = serializedObject.FindProperty("languageSelector");

            var setLanguageOnAwake = serializedObject.FindProperty("setLanguageOnAwake");
            var updateItemsOnSet = serializedObject.FindProperty("updateItemsOnSet");
            var saveLanguageChanges = serializedObject.FindProperty("saveLanguageChanges");

            HeatUIEditorHandler.DrawHeader(customSkin, "Core Header", 6);
            HeatUIEditorHandler.DrawProperty(UIManagerAsset, customSkin, "UI Manager");
            HeatUIEditorHandler.DrawProperty(languageSelector, customSkin, "Language Selector");

            HeatUIEditorHandler.DrawHeader(customSkin, "Options Header", 10);
            setLanguageOnAwake.boolValue = HeatUIEditorHandler.DrawToggle(setLanguageOnAwake.boolValue, customSkin, "Set Language On Awake");
            updateItemsOnSet.boolValue = HeatUIEditorHandler.DrawToggle(updateItemsOnSet.boolValue, customSkin, "Update Items On Language Set");
            saveLanguageChanges.boolValue = HeatUIEditorHandler.DrawToggle(saveLanguageChanges.boolValue, customSkin, "Save Language Changes");
            LocalizationManager.enableLogs = HeatUIEditorHandler.DrawToggle(LocalizationManager.enableLogs, customSkin, "Enable Logs");

            serializedObject.ApplyModifiedProperties();
            if (Application.isPlaying == false) { Repaint(); }
        }
    }
}
#endif