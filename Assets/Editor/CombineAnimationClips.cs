using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class MergeAnimationClipsInFolder : EditorWindow
{
    private string folderPath;
    private AnimationClip mergedClip;

    [MenuItem("YuuTools/Animation/Merge Animation Clips In Folder")]
    static void Init()
    {
        GetWindow<MergeAnimationClipsInFolder>();
    }

    void OnGUI()
    {
        GUILayout.Label("Merge Animation Clips In Folder", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical();

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Folder Path:");

        EditorGUILayout.BeginHorizontal();
        folderPath = EditorGUILayout.TextField(folderPath);
        if (GUILayout.Button("Browse", GUILayout.Width(80)))
        {
            folderPath = EditorUtility.OpenFolderPanel("Select Folder", folderPath, "");
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Merged Animation Clip:");

        EditorGUILayout.BeginHorizontal();
        mergedClip = EditorGUILayout.ObjectField(mergedClip, typeof(AnimationClip), false) as AnimationClip;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(20);

        if (GUILayout.Button("Merge Animation Clips In Folder"))
        {
            MergeClipsInFolder();
        }

        EditorGUILayout.EndVertical();
    }

    private void MergeClipsInFolder()
    {
        if (string.IsNullOrEmpty(folderPath))
        {
            EditorUtility.DisplayDialog("Error", "Please specify a folder path.", "OK");
            return;
        }

        if (mergedClip == null)
        {
            EditorUtility.DisplayDialog("Error", "Please specify a target Animation Clip to merge into.", "OK");
            return;
        }

        string[] animationFiles = Directory.GetFiles(folderPath, "*.anim");

        if (animationFiles.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "No Animation Clips found in the specified folder.", "OK");
            return;
        }

        foreach (string animationFile in animationFiles)
        {
            string relativePath = Path.Combine("Assets", animationFile.Substring(Application.dataPath.Length + 1));
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(relativePath);
            if (clip != null)
            {
                MergeAnimationClip(clip, mergedClip);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success", "Animation Clips in folder merged into '" + mergedClip.name + "'.", "OK");
    }

    private void MergeAnimationClip(AnimationClip sourceClip, AnimationClip targetClip)
    {
        EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(sourceClip);

        foreach (EditorCurveBinding curveBinding in curveBindings)
        {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(sourceClip, curveBinding);
            AnimationUtility.SetEditorCurve(targetClip, curveBinding, curve);
        }
    }
}
