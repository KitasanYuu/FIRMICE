using UnityEngine;
using UnityEditor;
using System.Linq;

public class CopyBlendShapesEditor : EditorWindow
{
    private GameObject sourceObject;
    private GameObject targetObject;

    [MenuItem("YuuTools/Animation/ShapeKey Copier")]
    public static void ShowWindow()
    {
        GetWindow<CopyBlendShapesEditor>("ShapeKey Copier");
    }

    void OnGUI()
    {
        sourceObject = (GameObject)EditorGUILayout.ObjectField("Source Object", sourceObject, typeof(GameObject), true);
        targetObject = (GameObject)EditorGUILayout.ObjectField("Target Object", targetObject, typeof(GameObject), true);

        if (GUILayout.Button("Copy BlendShapes"))
        {
            if (sourceObject != null && targetObject != null)
            {
                CopyBlendShapes(sourceObject, targetObject);
                Debug.Log("BlendShapes copied successfully.");
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Both Source and Target objects must be selected.", "OK");
            }
        }
    }

    private void CopyBlendShapes(GameObject source, GameObject target)
    {
        SkinnedMeshRenderer sourceRenderer = source.GetComponentInChildren<SkinnedMeshRenderer>();
        SkinnedMeshRenderer targetRenderer = target.GetComponentInChildren<SkinnedMeshRenderer>();

        if (sourceRenderer == null || targetRenderer == null)
        {
            Debug.LogError("Both objects must have a SkinnedMeshRenderer component.");
            return;
        }

        Mesh sourceMesh = sourceRenderer.sharedMesh;
        Mesh targetMesh = targetRenderer.sharedMesh;

        if (sourceMesh == null || targetMesh == null)
        {
            Debug.LogError("Both objects must have a mesh.");
            return;
        }

        if (sourceMesh.blendShapeCount != targetMesh.blendShapeCount)
        {
            Debug.LogError("Source and Target objects do not have the same number of BlendShapes.");
            return;
        }

        for (int i = 0; i < sourceMesh.blendShapeCount; i++)
        {
            string blendShapeName = sourceMesh.GetBlendShapeName(i);
            int targetBlendShapeIndex = targetMesh.GetBlendShapeIndex(blendShapeName);
            if (targetBlendShapeIndex != -1)
            {
                float blendShapeWeight = sourceRenderer.GetBlendShapeWeight(i);
                targetRenderer.SetBlendShapeWeight(targetBlendShapeIndex, blendShapeWeight);
            }
        }
    }
}
