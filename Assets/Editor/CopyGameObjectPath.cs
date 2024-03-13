using UnityEngine;
using UnityEditor;

public class CopyGameObjectPath : Editor
{
    [MenuItem("YuuTools/Copy GameObject Path")]
    static void CopyPath()
    {
        GameObject selectedObject = Selection.activeGameObject;

        if (selectedObject != null)
        {
            Transform current = selectedObject.transform;
            string path = current.name;

            while (current.parent != null)
            {
                current = current.parent;
                path = current.name + "/" + path;
            }

            EditorGUIUtility.systemCopyBuffer = path;
            Debug.Log("Copied GameObject Path: " + path);
        }
        else
        {
            Debug.LogWarning("No GameObject selected.");
        }
    }
}
