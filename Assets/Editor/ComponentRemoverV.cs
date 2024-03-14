using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MonoBehaviour), true, isFallback = true)]
public class CustomComponentRemover : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Remove This Script Safely"))
        {
            RemoveScriptSafely();
        }
    }

    void RemoveScriptSafely()
    {
        if (EditorUtility.DisplayDialog("Remove Script",
            "Are you sure you want to remove this script?", "Remove", "Cancel"))
        {
            MonoBehaviour monoBehaviour = (MonoBehaviour)target;
            DestroyImmediate(monoBehaviour, true);
        }
    }

    [MenuItem("CONTEXT/MonoBehaviour/Remove This Script Safely")]
    private static void RemoveScriptSafely(MenuCommand command)
    {
        MonoBehaviour targetScript = (MonoBehaviour)command.context;
        if (EditorUtility.DisplayDialog("Remove Script",
            $"Are you sure you want to remove {targetScript.GetType().Name}?", "Remove", "Cancel"))
        {
            DestroyImmediate(targetScript, true);
        }
    }
}
