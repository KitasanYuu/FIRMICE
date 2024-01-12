#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.UI.Heat
{
    public class InitHeatUI
    {
        [InitializeOnLoad]
        public class InitOnLoad
        {
            static InitOnLoad()
            {
                if (!EditorPrefs.HasKey("HeatUI.HasCustomEditorData"))
                {
                    string darkPath = AssetDatabase.GetAssetPath(Resources.Load("HeatUIEditor-Dark"));
                    string lightPath = AssetDatabase.GetAssetPath(Resources.Load("HeatUIEditor-Light"));

                    EditorPrefs.SetString("HeatUI.CustomEditorDark", darkPath);
                    EditorPrefs.SetString("HeatUI.CustomEditorLight", lightPath);
                    EditorPrefs.SetInt("HeatUI.HasCustomEditorData", 1);
                }
            }
        }
    }
}
#endif