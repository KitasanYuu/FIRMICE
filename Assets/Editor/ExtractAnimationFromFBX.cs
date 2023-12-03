using UnityEngine;
using UnityEditor;

public class ExtractAnimationFromFBX : EditorWindow
{
    private Object fbxFileObject; // 用于存储拖拽选择的文件对象

    [MenuItem("Tools/Extract Animation")]
    static void Init()
    {
        ExtractAnimationFromFBX window = (ExtractAnimationFromFBX)EditorWindow.GetWindow(typeof(ExtractAnimationFromFBX));
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Extract Animation", EditorStyles.boldLabel);
        GUILayout.Space(10);

        Event evt = Event.current;
        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Drag & Drop FBX File Here");

        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!dropArea.Contains(evt.mousePosition))
                    break;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        if (draggedObject is GameObject)
                        {
                            fbxFileObject = draggedObject;
                            break;
                        }
                    }
                }
                Event.current.Use();
                break;
        }

        fbxFileObject = EditorGUILayout.ObjectField("Selected FBX File", fbxFileObject, typeof(Object), false);

        if (GUILayout.Button("Extract") && fbxFileObject != null)
        {
            ExtractAnimationClip();
        }
    }

    void ExtractAnimationClip()
    {
        string fbxFilePath = AssetDatabase.GetAssetPath(fbxFileObject);

        // 从FBX文件加载模型
        GameObject modelObject = (GameObject)AssetDatabase.LoadAssetAtPath<GameObject>(fbxFilePath);
        if (modelObject == null)
        {
            Debug.LogError("Failed to load FBX file at path: " + fbxFilePath);
            return;
        }

        // 创建一个新的动画片段
        AnimationClip editableClip = new AnimationClip();
        editableClip.name = "EditableAnimationClip"; // 设置新动画片段的名称

        // 获取模型的所有AnimationClip
        AnimationClip[] animationClips = AnimationUtility.GetAnimationClips(modelObject);
        Debug.Log("Number of animation clips found: " + animationClips.Length);

        if (animationClips.Length == 0)
        {
            Debug.LogError("No animation clips found in the FBX file.");
            return;
        }

        // 选择第一个动画片段，并将其信息复制到新的动画片段中
        AnimationClip originalClip = animationClips[0];
        AnimationClipSettings clipSettings = AnimationUtility.GetAnimationClipSettings(originalClip);
        AnimationUtility.SetAnimationClipSettings(editableClip, clipSettings);

        // 将原始动画片段的信息复制到新的动画片段中
        EditorUtility.CopySerialized(originalClip, editableClip);

        // 保存新的动画片段为.asset文件
        string path = "Assets/NewEditableAnimationClip.anim";
        AssetDatabase.CreateAsset(editableClip, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("创建可编辑的动画片段：" + path);
    }
}
