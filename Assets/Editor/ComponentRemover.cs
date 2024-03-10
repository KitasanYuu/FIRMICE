using UnityEditor;
using UnityEngine;

public class ComponentRemover : EditorWindow
{
    private GameObject selectedGameObject;
    private bool[] componentSelection;

    [MenuItem("YuuTools/ComponentEditor/Component Remover")]
    public static void ShowWindow()
    {
        GetWindow<ComponentRemover>("Component Remover");
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical(); // 主垂直布局

        EditorGUILayout.BeginHorizontal(); // 第一行水平布局
        GUILayout.Label("Component Remover", EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        // Selected Object Column
        EditorGUILayout.BeginVertical();
        GUILayout.Label("Selected Object", EditorStyles.boldLabel);
        selectedGameObject = EditorGUILayout.ObjectField(selectedGameObject, typeof(GameObject), true) as GameObject;

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();

        if (selectedGameObject != null)
        {
            // Add "Select All" Button
            if (GUILayout.Button("Select All"))
            {
                SelectAllComponents(true);
            }

            // Add "Deselect All" Button
            if (GUILayout.Button("Deselect All"))
            {
                SelectAllComponents(false);
            }
        }

        EditorGUILayout.EndHorizontal(); // 结束第二行水平布局

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();

        // Display components on the selected object
        DisplayComponents();

        if (selectedGameObject != null)
        {
            // Remove Components Button
            GUI.enabled = (selectedGameObject != null);
            if (GUILayout.Button("Remove Selected Components"))
            {
                bool confirm = EditorUtility.DisplayDialog("Confirm Copy", "注意，该项操作不可逆，确认要执行组件删除吗？", "Yes", "No");
                if (confirm)
                {
                    RemoveSelectedComponents();
                }
            }
            GUI.enabled = true;
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical(); // 结束主垂直布局
    }



    private void SelectAllComponents(bool select)
    {
        if (selectedGameObject != null && componentSelection != null)
        {
            for (int i = 0; i < componentSelection.Length; i++)
            {
                componentSelection[i] = select;
            }
        }
    }


    private void DisplayComponents()
    {
        if (selectedGameObject != null)
        {
            GUILayout.Label("Select Components to Remove:", EditorStyles.boldLabel);

            Component[] components = selectedGameObject.GetComponents<Component>();

            if (componentSelection == null || componentSelection.Length != components.Length)
            {
                componentSelection = new bool[components.Length];
            }

            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];

                if (component != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    // Display component selection checkbox
                    componentSelection[i] = EditorGUILayout.ToggleLeft("", componentSelection[i], GUILayout.Width(20));
                    // Display component icon
                    Texture icon = EditorGUIUtility.ObjectContent(component, component.GetType()).image;
                    GUILayout.Label(icon, GUILayout.Width(20), GUILayout.Height(20));

                    // Display component name
                    GUILayout.Label(component.GetType().Name, GUILayout.Width(EditorGUIUtility.labelWidth - 20));

                    // Display component type
                    GUILayout.Label("Type: " + component.GetType().ToString());


                    EditorGUILayout.EndHorizontal();
                }
            }
        }
    }


    private void RemoveSelectedComponents()
    {
        if (selectedGameObject != null && componentSelection != null)
        {
            Component[] components = selectedGameObject.GetComponents<Component>();

            for (int i = components.Length - 1; i >= 0; i--)
            {
                if (componentSelection[i])
                {
                    DestroyImmediate(components[i],true);
                }
            }

            selectedGameObject = null;
            componentSelection = null;
        }
    }
}
