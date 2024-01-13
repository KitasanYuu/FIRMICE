#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace Michsky.UI.Heat
{
    [CustomEditor(typeof(LocalizationLanguage))]
    [System.Serializable]
    public class LocalizationLanguageEditor : Editor
    {
        private LocalizationLanguage lTarget;
        private GUISkin customSkin;
        private LocalizationLanguage.TableList tempTable;

        private void OnEnable()
        {
            lTarget = (LocalizationLanguage)target;

            if (EditorGUIUtility.isProSkin == true) { customSkin = HeatUIEditorHandler.GetDarkEditor(customSkin); }
            else { customSkin = HeatUIEditorHandler.GetLightEditor(customSkin); }
        }

        public override void OnInspectorGUI()
        {
            if (customSkin == null)
            {
                EditorGUILayout.HelpBox("Editor variables are missing. You can manually fix this by deleting " +
                    "Heat UI > Resources folder and then re-import the package. \n\nIf you're still seeing this " +
                    "dialog even after the re-import, contact me with this ID: " + UIManager.buildID, MessageType.Error);
                return;
            }

            // Info Header
            HeatUIEditorHandler.DrawHeader(customSkin, "Content Header", 8);

            var localizationSettings = serializedObject.FindProperty("localizationSettings");
            var languageID = serializedObject.FindProperty("languageID");
            var languageName = serializedObject.FindProperty("languageName");
            var localizedName = serializedObject.FindProperty("localizedName");
            var tableList = serializedObject.FindProperty("tableList");

            GUI.enabled = false;
            HeatUIEditorHandler.DrawProperty(languageID, customSkin, "Language ID");
            HeatUIEditorHandler.DrawProperty(languageName, customSkin, "Language Name");
            HeatUIEditorHandler.DrawProperty(localizedName, customSkin, "Localized Name");
            GUI.enabled = true;

            // Settings Header
            HeatUIEditorHandler.DrawHeader(customSkin, "Options Header", 14);
            HeatUIEditorHandler.DrawPropertyCW(localizationSettings, customSkin, "Localization Settings", 130);

            if (localizationSettings != null && GUILayout.Button("Show Localization Settings", customSkin.button)) 
            { 
                Selection.activeObject = localizationSettings.objectReferenceValue; 
            }

            // Content Header
            HeatUIEditorHandler.DrawHeader(customSkin, "Tables Header", 14);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Import Table", customSkin.button)) { Import(); return; }
            if (GUILayout.Button("Export Table(s)", customSkin.button)) { Export(); return; }
            GUILayout.EndHorizontal();
            GUI.enabled = false;
            EditorGUILayout.PropertyField(tableList, new GUIContent("Table List (Debug Only)"), true);

            serializedObject.ApplyModifiedProperties();
        }

        void Import()
        {
            string path = EditorUtility.OpenFilePanel("Select a file to import", "", "");

            if (path.Length != 0)
            {
                string tempKey = null;
                bool checkForValue = false;
                bool processNewEntry = false;

                LocalizationTable targetTable = null;
                List<LocalizationLanguage.TableContent> keysToBeAdded = new List<LocalizationLanguage.TableContent>();

                foreach (string option in File.ReadLines(path))
                {
                    if (option.Contains("[LanguageID] "))
                    {
                        string tempLangID = option.Replace("[LanguageID] ", "");
                        checkForValue = false;

                        if (tempLangID != lTarget.languageID)
                        {
                            Debug.LogError("The language ID does not match with the language asset.");
                            break;
                        }
                    }

                    if (option.Contains("[TableID] "))
                    {
                        string tempTableID = option.Replace("[TableID] ", "");
                        checkForValue = false;

                        for (int i = 0; i < lTarget.tableList.Count; i++)
                        {
                            if (lTarget.tableList[i].table.tableID == tempTableID)
                            {
                                targetTable = lTarget.tableList[i].table;
                                tempTable = lTarget.tableList[i];
                                break;
                            }
                        }
                    }

                    else if (option.Contains("[StringKey] "))
                    {
                        tempKey = option.Replace("[StringKey] ", "");
                        checkForValue = false;
                        processNewEntry = false;
                    }

                    else if (option.Contains("[Value] "))
                    {
                        if (tempTable == null)
                        {
                            Debug.LogError("Can't find the given table ID.");
                            break;
                        }

                        processNewEntry = true;

                        for (int i = 0; i < tempTable.tableContent.Count; i++)
                        {
                            if (tempTable.tableContent[i].key == tempKey)
                            {
                                processNewEntry = false;
                                string tempValue = option.Replace("[Value] ", "");
                                tempTable.tableContent[i].value = tempValue;
                                continue;
                            }
                        }

                        if (processNewEntry)
                        {
                            LocalizationLanguage.TableContent tempEntry = new LocalizationLanguage.TableContent();
                            tempEntry.key = tempKey;
                            tempEntry.value = option.Replace("[Value] ", "");
                            keysToBeAdded.Add(tempEntry);
                        }

                        checkForValue = true;
                    }

                    else if (checkForValue == true && !option.Contains("[Value] ") && !string.IsNullOrEmpty(option))
                    {
                        if (tempTable == null)
                        {
                            Debug.LogError("Can't find the given table ID.");
                            break;
                        }

                        if (processNewEntry) { keysToBeAdded[keysToBeAdded.Count - 1].value = keysToBeAdded[keysToBeAdded.Count - 1].value + "\n" + option; }
                        else
                        {
                            for (int i = 0; i < tempTable.tableContent.Count; i++)
                            {
                                if (tempTable.tableContent[i].key == tempKey)
                                {
                                    tempTable.tableContent[i].value = tempTable.tableContent[i].value + "\n" + option;
                                    break;
                                }
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(tempKey) && tempTable != null)
                {
                    for (int i = 0; i < keysToBeAdded.Count; i++)
                    {
                        LocalizationTable.TableContent newTableEntry = new LocalizationTable.TableContent();
                        newTableEntry.key = keysToBeAdded[i].key;
                        targetTable.tableContent.Add(newTableEntry);

                        for (int x = 0; x < lTarget.localizationSettings.languages.Count; x++)
                        {
                            foreach (LocalizationLanguage.TableList list in lTarget.localizationSettings.languages[x].localizationLanguage.tableList)
                            {
                                if (list.table.tableID == tempTable.table.tableID)
                                {
                                    list.tableContent.Add(keysToBeAdded[i]);
                                    break;
                                }
                            }

                            EditorUtility.SetDirty(lTarget.localizationSettings.languages[x].localizationLanguage);
                        }
                    }

                    Debug.Log(keysToBeAdded.Count.ToString() + " new localization entries has been added.");
                    Debug.Log(tempTable.table.tableID + " (table) has been successfully imported to " + lTarget.languageID);

                    EditorUtility.SetDirty(lTarget);
                }
            }
        }

        void Export()
        {
            for (int i = 0; i < lTarget.tableList.Count; i++)
            {
                string path = EditorUtility.SaveFilePanel("Export: " + lTarget.tableList[i].table.tableID, "", "Exported_" + lTarget.tableList[i].table.tableID + "(" + lTarget.languageID + ")", "txt");

                if (path.Length != 0)
                {
                    TextWriter tw = new StreamWriter(path, false);
                    tw.WriteLine("[LanguageID] " + lTarget.languageID);
                    tw.WriteLine("[TableID] " + lTarget.tableList[i].table.tableID);
                    tw.WriteLine("\n------------------------------");

                    for (int x = 0; x < lTarget.tableList[i].table.tableContent.Count; x++)
                    {
                        tw.WriteLine("\n[StringKey] " + lTarget.tableList[i].table.tableContent[x].key);
                        tw.Write("[Value] " + lTarget.tableList[i].tableContent[x].value + "\n");
                    }

                    tw.Close();
                    Debug.Log(lTarget.tableList[i].table.tableID + " has been exported to: " + path);
                }
            }
        }
    }
}
#endif