using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System;



public class ChangeNamespaceScript : EditorWindow
{
    private string selectedPath; // 新增用于存储选择的路径，可以是文件夹或文件
    private bool isSingleFileSelected;

    private DefaultAsset folderObject; // 通过拖拽选择的文件夹对象
    private string targetNamespace = "YourTargetNamespace"; // 设置目标 namespace
    private List<CSFileInfo> csFiles = new List<CSFileInfo>(); // 存储文件夹内的所有 .cs 文件信息
    private bool selectAll;
    private string[] currentNamespaces;

    [MenuItem("Custom/Change Namespace")]
    public static void ShowWindow()
    {
        ChangeNamespaceScript window = GetWindow<ChangeNamespaceScript>("Change Namespace");
        window.minSize = new Vector2(300, 200);
    }

    private void OnGUI()
    {
        isSingleFileSelected = !string.IsNullOrEmpty(selectedPath) && File.Exists(selectedPath);

        GUILayout.Label("Change Namespace", EditorStyles.boldLabel);

        Event evt = Event.current;
        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Drop Files or Folders Here", EditorStyles.helpBox);

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

                    // 清空之前的路径
                    selectedPath = null;

                    foreach (var draggedObject in DragAndDrop.objectReferences)
                    {
                        if (draggedObject is DefaultAsset)
                        {
                            selectedPath = AssetDatabase.GetAssetPath(draggedObject);
                        }
                        else if (draggedObject is MonoScript)
                        {
                            selectedPath = AssetDatabase.GetAssetPath(draggedObject);
                        }
                    }

                    // 输入发生变化时，清空文件列表
                    csFiles.Clear();
                    ListCSFiles();
                }
                Event.current.Use();
                break;
        }

        targetNamespace = EditorGUILayout.TextField("Target Namespace", targetNamespace);

        GUILayout.Space(10);

        if (GUILayout.Button("List CS Files"))
        {
            ListCSFiles();
        }

        if (csFiles.Count > 0)
        {
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button(selectAll ? "Deselect All" : "Select All"))
            {
                ToggleSelectAllFiles();
            }

            if (GUILayout.Button("Change Namespace"))
            {
                ChangeNamespaceInFiles();
            }

            GUILayout.EndHorizontal();

            DisplayCSFiles();
        }
        else
        {
            GUILayout.Label("No CS files found in the selected folder.");
        }
    }


    // 切换全选和取消选择的方法
    private void ToggleSelectAllFiles()
    {
        selectAll = !selectAll;

        foreach (var fileInfo in csFiles)
        {
            fileInfo.Selected = selectAll;
        }
    }



    private void ListCSFiles()
    {
        csFiles.Clear();

        if (!string.IsNullOrEmpty(selectedPath))
        {
            if (Directory.Exists(selectedPath))
            {
                string folderPath = selectedPath;

                // 检查是否是空文件夹
                if (Directory.GetFiles(folderPath, "*.cs").Length == 0)
                {
                    Debug.LogError("No C# files found in the selected folder.");
                    return;
                }

                RecursiveScan(folderPath);
            }
            else if (File.Exists(selectedPath) && selectedPath.EndsWith(".cs"))
            {
                // 如果选择的是单个文件
                string fileName = Path.GetFileNameWithoutExtension(selectedPath);
                string content = File.ReadAllText(selectedPath);
                string currentNamespace = GetCurrentNamespace(content);

                csFiles.Add(new CSFileInfo(fileName, true, currentNamespace));
            }
            else
            {
                Debug.LogError("Please select a valid folder or a C# file.");
            }
        }
        else
        {
            Debug.LogError("Please select a folder or a C# file.");
        }
    }




    // 递归遍历所有子文件夹内的 .cs 文件
    private void RecursiveScan(string folderPath)
    {
        // 扫描当前文件夹文件
        string[] files = Directory.GetFiles(folderPath, "*.cs");

        foreach (string filePath in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string content = File.ReadAllText(filePath);
            string currentNamespace = GetCurrentNamespace(content);

            csFiles.Add(new CSFileInfo(fileName, true, currentNamespace));
        }

        // 遍历子文件夹 
        foreach (string subFolder in Directory.GetDirectories(folderPath))
        {
            RecursiveScan(subFolder);
        }
    }





    private void DisplayCSFiles()
    {
        GUILayout.Label("CS Files in Selected Folder", EditorStyles.boldLabel);

        // 分离有命名空间和没有命名空间的文件
        var filesWithNamespace = csFiles.Where(fileInfo => !string.IsNullOrEmpty(fileInfo.CurrentNamespace)).ToList();
        var filesWithoutNamespace = csFiles.Where(fileInfo => string.IsNullOrEmpty(fileInfo.CurrentNamespace)).ToList();

        // 对有命名空间的文件按照命名空间分组，数量多的在前面
        var groupedFilesWithNamespace = filesWithNamespace.GroupBy(fileInfo => fileInfo.CurrentNamespace)
                                                          .OrderByDescending(group => group.Count())
                                                          .SelectMany(group => group)
                                                          .ToList();

        // 获取最长的命名空间长度，用于对齐
        int maxNamespaceLength = groupedFilesWithNamespace.Max(fileInfo => fileInfo.CurrentNamespace?.Length ?? 0);

        string currentNamespace = null;

        // 显示有命名空间的文件，按照命名空间排序，数量多的在前面
        for (int i = 0; i < groupedFilesWithNamespace.Count; i++)
        {
            var fileInfo = groupedFilesWithNamespace[i];

            if (fileInfo.CurrentNamespace != currentNamespace)
            {
                if (currentNamespace != null && i > 0)
                {
                    // 如果命名空间不同，且不是第一组，则插入空行和青色横线
                    GUILayout.Space(5);
                    DrawColoredLine(maxNamespaceLength, Color.gray);
                }

                currentNamespace = fileInfo.CurrentNamespace;
            }

            int alignmentOffset = fileInfo.FileName.Length + 18;

            // 开始水平布局
            GUILayout.BeginHorizontal();

            // 文件名和选择框
            fileInfo.Selected = GUILayout.Toggle(fileInfo.Selected || selectAll, $"{fileInfo.FileName}", GUILayout.Width(150));

            // 分隔符
            GUILayout.Label("-", GUILayout.Width(10));

            // 命名空间
            string alignedNamespace = fileInfo.CurrentNamespace.PadRight(Mathf.Max(0, maxNamespaceLength - alignmentOffset) + fileInfo.CurrentNamespace.Length - fileInfo.CurrentNamespace.Replace(" ", "").Length);
            GUILayout.Label($"Current Namespace: {alignedNamespace}");

            // 结束水平布局
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(5);
        DrawColoredLine(maxNamespaceLength, Color.gray);

        // 显示没有明确定义命名空间的文件，直接显示为 "- No Defined Namespace"
        foreach (var fileInfo in filesWithoutNamespace)
        {
            int alignmentOffset = fileInfo.FileName.Length + 18;

            // 开始水平布局
            GUILayout.BeginHorizontal();

            // 文件名和选择框
            fileInfo.Selected = GUILayout.Toggle(fileInfo.Selected || selectAll, $"{fileInfo.FileName}", GUILayout.Width(150));

            // 分隔符
            GUILayout.Label("-", GUILayout.Width(10));

            // 没有明确定义的话，显示 - No Defined Namespace
            GUILayout.Label("No Defined Namespace");

            // 结束水平布局
            GUILayout.EndHorizontal();
        }
    }



    // 用于绘制带颜色的横线
    private static void DrawColoredLine(int length, Color color)
    {
        Rect r = EditorGUILayout.GetControlRect(false, 1);
        r.height = 1;
        EditorGUI.DrawRect(r, color);
    }


    private void ChangeNamespaceInFiles()
    {
        foreach (var fileInfo in csFiles.Where(file => file.Selected))
        {
            string filePath = isSingleFileSelected ? selectedPath : FindCSFilePath(fileInfo.FileName);

            if (string.IsNullOrEmpty(filePath))
            {
                Debug.LogError($"Could not find file path for {fileInfo.FileName}. Skipping.");
                continue;
            }

            Debug.Log($"Processing file: {filePath}");

            string content = File.ReadAllText(filePath);

            // 查找当前 namespace，并替换为目标 
            int startIndex = content.IndexOf("namespace ");

            if (startIndex != -1)
            {
                int endIndex = content.IndexOf("{", startIndex);

                if (endIndex != -1)
                {
                    string currentNamespace = content.Substring(startIndex + 10, endIndex - startIndex - 10).Trim();

                    // 移除命名空间及命名空间后的空格和{
                    content = content.Remove(startIndex, endIndex - startIndex + 1);

                    // 查找末尾的 }
                    int lastBraceIndex = content.LastIndexOf('}');
                    if (lastBraceIndex != -1)
                    {
                        // 移除末尾的 }
                        content = content.Remove(lastBraceIndex, 1);
                    }

                    // 统计移除命名空间后的空行数
                    int emptyLinesAfterRemoval = CountEmptyLinesAfterRemoval(content, startIndex);

                    // 控制在两行空行
                    int desiredEmptyLines = Mathf.Max(2, emptyLinesAfterRemoval);
                    content = AddEmptyLines(content, startIndex, desiredEmptyLines);

                    File.WriteAllText(filePath, content);
                    Debug.Log($"Namespace removed for {fileInfo.FileName}");
                }
                else
                {
                    Debug.LogError($"Could not find '{{' after 'namespace' in file: {filePath}");
                }
            }
            else
            {
                Debug.LogError($"Could not find 'namespace' in file: {filePath}");
            }

            // 处理目标命名空间
            if (!string.Equals(targetNamespace, "null", StringComparison.OrdinalIgnoreCase))
            {
                // 在 public class 上方添加目标 namespace
                int classIndex = content.IndexOf("public class");

                if (classIndex != -1)
                {
                    // 寻找 public class 上方的 [] 并在其上方添加目标 namespace
                    int arrayIndex = content.LastIndexOf('[', classIndex);

                    if (arrayIndex != -1)
                    {
                        // 在 [] 上方添加 namespace
                        content = content.Insert(arrayIndex, $"namespace {targetNamespace}\n{{\n");
                    }
                    else
                    {
                        // 在 public class 上方添加 namespace
                        content = content.Insert(classIndex, $"namespace {targetNamespace}\n{{\n");
                    }
                }
                else
                {
                    // 如果没有找到 public class，直接在文件末尾添加 namespace
                    content = content + $"\nnamespace {targetNamespace}\n{{\n}}";
                }

                // 恢复文件末尾的 }
                content = content.TrimEnd() + "\n}\n";

                File.WriteAllText(filePath, content);
                Debug.Log($"Namespace changed for {fileInfo.FileName}");
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("Namespace change completed.");
    }

    private int CountEmptyLinesAfterRemoval(string content, int startIndex)
    {
        int count = 0;

        for (int i = startIndex; i < content.Length; i++)
        {
            if (content[i] == '\n')
            {
                count++;
            }
            else if (!char.IsWhiteSpace(content[i]))
            {
                break;
            }
        }

        return count;
    }

    private string AddEmptyLines(string content, int startIndex, int desiredEmptyLines)
    {
        int currentEmptyLines = CountEmptyLinesAfterRemoval(content, startIndex);

        if (currentEmptyLines > desiredEmptyLines)
        {
            int linesToRemove = currentEmptyLines - desiredEmptyLines;
            int start = startIndex;

            for (int i = 0; i < linesToRemove; i++)
            {
                while (start < content.Length && content[start] != '\n')
                {
                    start++;
                }

                if (start < content.Length)
                {
                    start++;
                }
            }

            content = content.Remove(startIndex, start - startIndex);
        }
        else if (currentEmptyLines < desiredEmptyLines)
        {
            content = content.Insert(startIndex, new string('\n', desiredEmptyLines - currentEmptyLines));
        }

        return content;
    }



    private string GetCurrentNamespace(string content)
    {
        int startIndex = content.IndexOf("namespace ");

        // 确保找到了 "namespace" 关键字
        if (startIndex != -1)
        {
            int endIndex = content.IndexOf("{", startIndex);

            // 确保找到了 "{" 符号
            if (endIndex != -1)
            {
                // 获取命名空间
                string currentNamespace = content.Substring(startIndex + 10, endIndex - startIndex - 10).Trim();

                // 防止命名空间为空字符串时引发异常
                return string.IsNullOrEmpty(currentNamespace) ? "No Defined Namespace" : currentNamespace;
            }
        }

        return "No Defined Namespace";
    }



    private string FindCSFilePath(string csFileName)
    {
        if (selectedPath != null && File.Exists(selectedPath))
        {
            // 如果有选择的单文件路径,直接返回
            return selectedPath;
        }

        if (!string.IsNullOrEmpty(selectedPath))
        {
            string folderPath = selectedPath;

            if (Directory.Exists(folderPath))
            {
                string[] filePaths = Directory.GetFiles(folderPath, $"{csFileName}.cs", SearchOption.AllDirectories);
                return filePaths.FirstOrDefault();
            }
        }

        return null;
    }

    private class CSFileInfo
    {
        public string FileName { get; }
        public bool Selected { get; set; }
        public string CurrentNamespace { get; }

        public CSFileInfo(string fileName, bool selected, string currentNamespace)
        {
            FileName = fileName;
            Selected = selected;
            CurrentNamespace = currentNamespace;
        }
    }
}




