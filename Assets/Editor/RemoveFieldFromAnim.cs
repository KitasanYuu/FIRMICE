using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class RemoveFieldFromAnim : EditorWindow
{
    private string fieldToRemove = "";
    private HashSet<string> inputFilePaths = new HashSet<string>();

    [MenuItem("YuuTools/Animation/AnimPath/Remove Field From Anim")]
    private static void ShowWindow()
    {
        GetWindow<RemoveFieldFromAnim>("Remove Path Field");
    }

    private void OnGUI()
    {
        GUILayout.Label("Field to Remove", EditorStyles.boldLabel);
        fieldToRemove = EditorGUILayout.TextField(fieldToRemove);

        // 检测是否有动画剪辑被拖拽到窗口
        Event evt = Event.current;
        Rect dropArea = GUILayoutUtility.GetRect(0f, 50f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Drop Animation Clips Here");

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

                    foreach (var draggedObject in DragAndDrop.objectReferences)
                    {
                        if (draggedObject is AnimationClip)
                        {
                            string clipPath = AssetDatabase.GetAssetPath(draggedObject);
                            inputFilePaths.Add(clipPath);
                        }
                    }
                }
                Event.current.Use();
                break;
        }

        if (inputFilePaths.Count > 0)
        {
            GUILayout.Space(10);
            GUILayout.Label("Files to Process:", EditorStyles.boldLabel);
            foreach (string filePath in inputFilePaths)
            {
                GUILayout.Label(filePath);
            }
            GUILayout.Space(10);
        }

        if (inputFilePaths.Count > 0 && GUILayout.Button("Remove Field"))
        {
            RemoveFieldFromAnimFiles();
        }
    }

    private void RemoveFieldFromAnimFiles()
    {
        if (string.IsNullOrEmpty(fieldToRemove) || inputFilePaths.Count == 0)
        {
            Debug.LogError("Field to Remove and Input File Paths cannot be empty!");
            return;
        }

        foreach (string inputFilePath in inputFilePaths)
        {
            // 生成一个临时文件路径
            string tempFilePath = inputFilePath + ".temp";

            using (StreamReader reader = new StreamReader(inputFilePath))
            using (StreamWriter writer = new StreamWriter(tempFilePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("    path:"))
                    {
                        // 使用字符串的 Replace 方法删除指定字段
                        string modifiedLine = line.Replace(fieldToRemove, "");
                        writer.WriteLine(modifiedLine);
                    }
                    else
                    {
                        writer.WriteLine(line);
                    }
                }
            }

            // 关闭文件后，将临时文件内容覆盖原文件
            File.Replace(tempFilePath, inputFilePath, null);

            Debug.Log($"处理完成！{inputFilePath}");
        }

        // 清空处理完的文件列表
        inputFilePaths.Clear();
    }
}
