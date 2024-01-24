using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class AnimationPathListEditor : EditorWindow
{
    private AnimationClip selectedClip;
    private Vector2 scrollPosition;

    [MenuItem("YuuTools/Animation/AnimPath/Animation Path List Editor")]
    static void OpenWindow()
    {
        GetWindow<AnimationPathListEditor>("Animation Path List Editor");
    }

    private void OnGUI()
    {
        GUILayout.Label("Select an Animation Clip:", EditorStyles.boldLabel);

        selectedClip = EditorGUILayout.ObjectField(selectedClip, typeof(AnimationClip), false) as AnimationClip;

        if (selectedClip != null)
        {
            GUILayout.Label("Object Paths in Animation Clip:", EditorStyles.boldLabel);

            // 获取所有的曲线数据
            EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(selectedClip);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach (EditorCurveBinding curveBinding in curveBindings)
            {
                // 获取自定义路径
                string customPath = GetCustomPath(curveBinding);

                // 显示路径 
                EditorGUILayout.LabelField("Path: " + customPath, EditorStyles.miniBoldLabel);
            }

            EditorGUILayout.EndScrollView();
        }
    }

    private string GetCustomPath(EditorCurveBinding binding)
    {
        return "CustomPath/" + binding.path;
    }
}