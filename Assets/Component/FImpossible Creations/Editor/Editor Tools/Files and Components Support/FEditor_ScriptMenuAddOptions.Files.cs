using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FEditor
{
    /// <summary>
    /// FM: Class with basic tools for working in Unity Editor level
    /// </summary>
    public static partial class FEditor_MenuAddOptions
    {

        [MenuItem("Assets/Utilities/Copy Full Path To Directory")]
        private static void CopyWholePathToDir(MenuCommand menuCommand)
        {
            if (Selection.objects.Length == 0) return;
            string assetPath = AssetDatabase.GetAssetPath(Selection.objects[0]);
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
            GUIUtility.systemCopyBuffer = Path.GetDirectoryName(fullPath);
        }



        [MenuItem("CONTEXT/MonoBehaviour/Go To Script's Directory")]
        private static void GoToBehaviourDirectory(MenuCommand menuCommand)
        {
            if (menuCommand.context is MonoBehaviour)
            {
                MonoBehaviour targetComponent = (MonoBehaviour)menuCommand.context;

                if (targetComponent)
                {
                    MonoScript script = MonoScript.FromMonoBehaviour(targetComponent);
                    if (script) EditorGUIUtility.PingObject(script);
                }
            }
            else if (menuCommand.context is ScriptableObject)
            {
                ScriptableObject targetComponent = (ScriptableObject)menuCommand.context;

                if (targetComponent)
                {
                    MonoScript script = MonoScript.FromScriptableObject(targetComponent);
                    if (script) EditorGUIUtility.PingObject(script);
                }
            }
        }

        [MenuItem("Assets/Utilities/Name iterative selected assets", true)]
        static bool SetFilenamesCheck(MenuCommand menuCommand)
        { return Selection.gameObjects.Length > 0; }

        [MenuItem("Assets/Utilities/Name iterative selected assets", false)]
        private static void SetFilenames(MenuCommand menuCommand)
        {
            if (Selection.gameObjects.Length == 0) return;

            string filename = EditorUtility.SaveFilePanelInProject("Type your target filename (no file will be created)", Selection.gameObjects[0].name, "", "Type your target filename (no file will be created)");
            filename = filename.Replace("Assets/", "");
            if (string.IsNullOrEmpty(filename)) return;

            List<GameObject> toRename = new List<GameObject>();
            GameObject ctx = (GameObject)menuCommand.context;
            if (ctx) toRename.Add(ctx);

            for (int i = 0; i < Selection.gameObjects.Length; i++)
            {
                if (!toRename.Contains(Selection.gameObjects[i])) toRename.Add(Selection.gameObjects[i]);
            }

            int objects = 0;
            for (int i = 0; i < toRename.Count; i++)
            {
                if (toRename[i] == null) continue;
                string assetPath = AssetDatabase.GetAssetPath(toRename[i]);
                if (string.IsNullOrEmpty(assetPath)) continue;

                AssetDatabase.RenameAsset(assetPath, filename + "_" + objects + Path.GetExtension(assetPath));
                objects++;
            }
        }

#if UNITY_2019_4_OR_NEWER
        [MenuItem("Assets/Utilities/Add name prefixes for selection", true)]
        static bool SetPrefixesCheck(MenuCommand menuCommand)
        { return Selection.objects.Length > 0; }

        [MenuItem("Assets/Utilities/Add name prefixes for selection", false)]
        private static void SetPrefixes(MenuCommand menuCommand)
        {
            if (Selection.objects.Length == 0) return;

            for (int i = 0; i < Selection.objects.Length; i++)
            {
                if (Selection.objects[i] == null) continue;

                string assetPath = AssetDatabase.GetAssetPath(Selection.objects[i]);
                if (string.IsNullOrEmpty(assetPath)) continue;

                string prefix = GetPrefix(Selection.objects[i], assetPath);
                if (string.IsNullOrEmpty(prefix)) continue;
                {
                    AssetDatabase.RenameAsset(assetPath, prefix + Selection.objects[i].name);
                }
            }
        }
#endif

    }
}
