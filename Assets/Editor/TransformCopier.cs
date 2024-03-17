using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class TransformCopier : EditorWindow
{
    GameObject sourceObject;
    GameObject targetObject;

    [MenuItem("YuuTools/Animation/Transform Copier")]
    public static void ShowWindow()
    {
        GetWindow<TransformCopier>("Transform Copier");
    }

    void OnGUI()
    {
        GUILayout.Label("Copy Transform Data", EditorStyles.boldLabel);

        sourceObject = (GameObject)EditorGUILayout.ObjectField("Source Object", sourceObject, typeof(GameObject), true);
        targetObject = (GameObject)EditorGUILayout.ObjectField("Target Object", targetObject, typeof(GameObject), true);

        if (GUILayout.Button("Copy Transforms"))
        {
            if (sourceObject == null || targetObject == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select both source and target objects.", "OK");
            }
            else
            {
                CopyTransformData(sourceObject.transform, targetObject.transform);
                EditorUtility.DisplayDialog("Completed", "Transform data has been copied.", "OK");
            }
        }
    }

    void CopyTransformData(Transform source, Transform target)
    {
        // Use a Dictionary to map the full path (name hierarchy) to the Transform for quick lookups.
        var sourceTransforms = new Dictionary<string, Transform>();
        PopulateTransformDictionary(source, string.Empty, sourceTransforms);

        ApplyTransformData(target, string.Empty, sourceTransforms);
    }

    void PopulateTransformDictionary(Transform current, string path, Dictionary<string, Transform> dict)
    {
        // Construct the path for this transform
        string currentPath = string.IsNullOrEmpty(path) ? current.name : path + "/" + current.name;
        dict[currentPath] = current;

        foreach (Transform child in current)
        {
            PopulateTransformDictionary(child, currentPath, dict);
        }
    }

    void ApplyTransformData(Transform target, string path, Dictionary<string, Transform> sourceTransforms)
    {
        string currentPath = string.IsNullOrEmpty(path) ? target.name : path + "/" + target.name;
        if (sourceTransforms.TryGetValue(currentPath, out Transform sourceTransform))
        {
            target.position = sourceTransform.position;
            target.rotation = sourceTransform.rotation;
            target.localScale = sourceTransform.localScale;
        }

        foreach (Transform child in target)
        {
            ApplyTransformData(child, currentPath, sourceTransforms);
        }
    }
}
