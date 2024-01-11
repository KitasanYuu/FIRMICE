#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.LSS
{
    public class InitLSS
    {
        [InitializeOnLoad]
        public class InitOnLoad
        {
            static InitOnLoad()
            {
                if (!EditorPrefs.HasKey("LSS.HasCustomEditorData"))
                {
                    string darkPath = AssetDatabase.GetAssetPath(Resources.Load("LSSEditor-Dark"));
                    string lightPath = AssetDatabase.GetAssetPath(Resources.Load("LSSEditor-Light"));

                    EditorPrefs.SetString("LSSEditor.CustomEditorDark", darkPath);
                    EditorPrefs.SetString("LSSEditor.CustomEditorLight", lightPath);
                    EditorPrefs.SetInt("LSSEditor.HasCustomEditorData", 1);
                }
            }
        }
    }
}
#endif