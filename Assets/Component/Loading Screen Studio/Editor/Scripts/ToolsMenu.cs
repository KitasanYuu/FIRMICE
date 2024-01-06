#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Security.Policy;

namespace Michsky.LSS
{
    public class ToolsMenu : Editor
    {
        static void MakeSceneDirty(GameObject source, string sourceName)
        {
            if (Application.isPlaying == false)
            {
                Undo.RegisterCreatedObjectUndo(source, sourceName);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }

        static void UpdateCustomEditorPath()
        {
            string darkPath = AssetDatabase.GetAssetPath(Resources.Load("LSSEditor-Dark"));
            string lightPath = AssetDatabase.GetAssetPath(Resources.Load("LSSEditor-Light"));

            EditorPrefs.SetString("LSS.CustomEditorDark", darkPath);
            EditorPrefs.SetString("LSS.CustomEditorLight", lightPath);
        }

        static void CreateObject(string resourcePath)
        {
            try
            {
                UpdateCustomEditorPath();
                GameObject clone = Instantiate(Resources.Load<GameObject>(resourcePath), Vector3.zero, Quaternion.identity) as GameObject;

                if (Selection.activeGameObject != null)
                {
                    clone.transform.SetParent(Selection.activeGameObject.transform, false);
                }

                clone.name = clone.name.Replace("(Clone)", "").Trim();
                MakeSceneDirty(clone, clone.name);
                Selection.activeObject = clone;
            }

            catch { EditorUtility.DisplayDialog("Loading Screen Studio", "Cannot create the LSS manager due to missing asset file(s).", "Dismiss"); }
        }

        [MenuItem("Tools/Loading Screen Studio/Create LSS Manager", false, 0)]
        static void CreateLSSM()
        {
            CreateObject("LSS Manager");
        }

        [MenuItem("Tools/Loading Screen Studio/Show Default Loading Screen", false, 0)]
        static void ShowCurrentLoadingScreen()
        {
            string folderPath = AssetDatabase.GetAssetPath(Resources.Load("LSS Manager")).Replace("LSS Manager.prefab", "").Trim();
            string[] guids = AssetDatabase.FindAssets("t:Object", new[] { folderPath + "Loading Screens" });

            if (guids.Length > 0)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                Object asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));
                Selection.activeObject = asset;
            }

            else
            {
                EditorUtility.DisplayDialog("Loading Screen Studio", "There's no loading screen available under: " + folderPath + "Loading Screens.", "Dismiss");
            }
        }

        [MenuItem("Tools/Loading Screen Studio/Documentation", false, 12)]
        static void OpenDocs()
        {
            Application.OpenURL("https://docs.michsky.com/docs/lss/");
        }
    }
}
#endif