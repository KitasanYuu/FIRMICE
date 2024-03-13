using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public class CSLineCounter : EditorWindow
{
    private string folderPath = "";
    private Vector2 scrollPosition;
    private int totalLines = 0;
    private int totalFiles = 0;
    private string[] allFiles;

    [MenuItem("YuuTools/Statistic/CSLinesCount")]
    public static void ShowWindow()
    {
        GetWindow(typeof(CSLineCounter));
    }

    private void OnGUI()
    {
        GUILayout.Label("Drag and drop folder here:", EditorStyles.boldLabel);
        folderPath = EditorGUILayout.TextField("Folder Path", folderPath);

        Event evt = Event.current;
        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Drag & Drop Folder Here");

        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!dropArea.Contains(evt.mousePosition))
                    return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (string draggedObject in DragAndDrop.paths)
                    {
                        folderPath = draggedObject;
                    }
                }
                break;
        }

        GUILayout.Space(20);

        if (GUILayout.Button("Count Lines"))
        {
            CountAndDisplayLines();
        }

        GUILayout.Space(20);

        // 显示总行数和总文件数
        GUILayout.Label($"Total Files: {totalFiles}");
        GUILayout.Label($"Total Lines: {totalLines}");

        // 使用滚动视图显示所有脚本
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        if (allFiles != null)
        {
            string currentGroup = null;

            foreach (string file in allFiles)
            {
                string fileName = Path.GetFileName(file);
                string fileDirectory = Path.GetDirectoryName(file);

                // 获取脚本文件自身的父文件夹
                string parentFolder = Path.GetFileName(fileDirectory);

                if (parentFolder != currentGroup)
                {
                    currentGroup = parentFolder;
                    GUILayout.Label($"------------------- Group: {currentGroup} -------------------", EditorStyles.boldLabel);
                }

                int fileLineCount = GetLineCount(file);
                string fileInfo = $"{fileName} ({fileLineCount} lines)";
                GUILayout.BeginHorizontal();
                GUILayout.Label(fileInfo);
                GUILayout.EndHorizontal();
            }
        }
        GUILayout.EndScrollView();
    }

    private int GetLineCount(string filePath)
    {
        try
        {
            string[] lines = File.ReadAllLines(filePath);
            return lines.Length;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error reading file '{filePath}': {ex.Message}");
            return 0;
        }
    }

    private void CountAndDisplayLines()
    {
        totalLines = 0;
        totalFiles = 0;

        try
        {
            // 获取所有文件和文件夹
            string[] files = Directory.GetFiles(folderPath, "*.cs", SearchOption.AllDirectories);
            allFiles = files;
            totalFiles = files.Length;

            // 统计所有 .cs 文件行数
            foreach (string file in files)
            {
                totalLines += GetLineCount(file);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error: {ex.Message}");
        }
    }
}
