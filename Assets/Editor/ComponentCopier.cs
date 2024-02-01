using UnityEditor;
using UnityEngine;

public class ComponentCopier : EditorWindow
{
    private GameObject sourceObject;
    private GameObject targetObject;

    private Vector2 scrollPositionSource;
    private Vector2 scrollPositionTarget;

    private bool[] selectedScripts;
    private bool isConfirmationNeeded = false;

    [MenuItem("YuuTools/ComponentEditor/Component Copier")]
    public static void ShowWindow()
    {
        GetWindow<ComponentCopier>("Component Copier");
    }


    private void OnGUI()
    {
        EditorGUILayout.BeginVertical(); // 主垂直布局

        EditorGUILayout.BeginHorizontal(); // 第一行水平布局
        GUILayout.Label("Component Copier", EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();

        // Copy Components Button
        GUI.enabled = (sourceObject != null && targetObject != null);
        // Confirm Button
        GUI.enabled = true;
        if (GUILayout.Button("Copy Components"))
        {
            // 弹出二次确认窗口
            bool confirm = EditorUtility.DisplayDialog("Confirm Copy", "注意，该项操作不可逆，确认要执行组件复制吗？", "Yes", "No");
            if (confirm)
            {
                CopyComponents(sourceObject, targetObject);
            }
        }

        EditorGUILayout.BeginHorizontal(); // 第二行水平布局

        // Source Object Column
        EditorGUILayout.BeginVertical(GUILayout.Width(position.width / 2 - 10));
        GUILayout.Label("Source Object", EditorStyles.boldLabel);
        sourceObject = EditorGUILayout.ObjectField(sourceObject, typeof(GameObject), true) as GameObject;

        // Display components on the source object
        scrollPositionSource = EditorGUILayout.BeginScrollView(scrollPositionSource, GUILayout.Height(position.height - 160));
        if (sourceObject != null)
        {
            DisplayComponents(sourceObject, true);
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndVertical();

        // Target Object Column
        EditorGUILayout.BeginVertical(GUILayout.Width(position.width / 2 - 10));
        GUILayout.Label("Target Object", EditorStyles.boldLabel);
        targetObject = EditorGUILayout.ObjectField(targetObject, typeof(GameObject), true) as GameObject;

        // Display components on the target object
        scrollPositionTarget = EditorGUILayout.BeginScrollView(scrollPositionTarget, GUILayout.Height(position.height - 160));
        if (targetObject != null)
        {
            DisplayComponents(targetObject, false);
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal(); // 结束第二行水平布局

        EditorGUILayout.EndVertical(); // 结束主垂直布局
    }

    private void DisplayComponents(GameObject obj, bool isSource)
    {
        Component[] components = obj.GetComponents<Component>();
        if (isSource)
        {
            if (selectedScripts == null || selectedScripts.Length != components.Length)
            {
                selectedScripts = new bool[components.Length];
            }
        }

        for (int i = 0; i < components.Length; i++)
        {
            Component component = components[i];

            if (component != null)
            {
                EditorGUILayout.BeginHorizontal();
                if (isSource)
                {
                    bool selected = EditorGUILayout.Toggle(selectedScripts[i], GUILayout.Width(20));
                    if (selected != selectedScripts[i])
                    {
                        selectedScripts[i] = selected;
                        Repaint();
                    }
                }
                else
                {
                    GUILayout.Space(20);
                }

                EditorGUI.indentLevel++;
                EditorGUILayout.ObjectField(component.GetType().Name, component, typeof(Component), false);
                EditorGUI.indentLevel--;

                EditorGUILayout.EndHorizontal();
            }
        }
    }

    private void CopyComponents(GameObject source, GameObject target)
    {
        // 获取源对象上所有的组件
        Component[] componentsToCopy = source.GetComponents<Component>();

        // 遍历源对象上的组件
        for (int i = 0; i < componentsToCopy.Length; i++)
        {
            Component component = componentsToCopy[i];

            // 如果组件不是Transform，并且选中了对应的脚本（selectedScripts[i]为真）
            if (!(component is Transform) && selectedScripts[i])
            {
                // 使用UnityEditor的工具复制和粘贴组件
                UnityEditorInternal.ComponentUtility.CopyComponent(component);
                UnityEditorInternal.ComponentUtility.PasteComponentAsNew(target);
            }
        }

        Debug.Log("Components copied successfully from " + source.name + " to " + target.name);
    }

}
