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
                if (!EditorPrefs.HasKey("LSS.Installed"))
                {
                    EditorPrefs.SetInt("LSS.Installed", 1);
                    EditorUtility.DisplayDialog("Hello there!", "Thank you for purchasing Loading Screen Studio.\n\n" +
                                                "If you need help, feel free to contact us through our support channels or Discord.", "Got it!");
                }

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