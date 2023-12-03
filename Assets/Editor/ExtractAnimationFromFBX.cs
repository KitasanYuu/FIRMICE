using UnityEngine;
using UnityEditor;

public class ExtractAnimationFromFBX : EditorWindow
{
    private Object fbxFileObject; // ���ڴ洢��קѡ����ļ�����

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

        // ��FBX�ļ�����ģ��
        GameObject modelObject = (GameObject)AssetDatabase.LoadAssetAtPath<GameObject>(fbxFilePath);
        if (modelObject == null)
        {
            Debug.LogError("Failed to load FBX file at path: " + fbxFilePath);
            return;
        }

        // ����һ���µĶ���Ƭ��
        AnimationClip editableClip = new AnimationClip();
        editableClip.name = "EditableAnimationClip"; // �����¶���Ƭ�ε�����

        // ��ȡģ�͵�����AnimationClip
        AnimationClip[] animationClips = AnimationUtility.GetAnimationClips(modelObject);
        Debug.Log("Number of animation clips found: " + animationClips.Length);

        if (animationClips.Length == 0)
        {
            Debug.LogError("No animation clips found in the FBX file.");
            return;
        }

        // ѡ���һ������Ƭ�Σ���������Ϣ���Ƶ��µĶ���Ƭ����
        AnimationClip originalClip = animationClips[0];
        AnimationClipSettings clipSettings = AnimationUtility.GetAnimationClipSettings(originalClip);
        AnimationUtility.SetAnimationClipSettings(editableClip, clipSettings);

        // ��ԭʼ����Ƭ�ε���Ϣ���Ƶ��µĶ���Ƭ����
        EditorUtility.CopySerialized(originalClip, editableClip);

        // �����µĶ���Ƭ��Ϊ.asset�ļ�
        string path = "Assets/NewEditableAnimationClip.anim";
        AssetDatabase.CreateAsset(editableClip, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("�����ɱ༭�Ķ���Ƭ�Σ�" + path);
    }
}
