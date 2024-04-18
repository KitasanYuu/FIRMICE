using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

public class ButtonListener2Object : MonoBehaviour
{
    // 在Unity编辑器中显示函数选择下拉列表
    public GameObject targetObject;
    public MonoBehaviour targetScript;
    public string functionName;

    public GameObject currentPanel;
    public GameObject targetPanel;

    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        // 检查是否已选择了目标脚本
        if (targetScript == null)
        {
            Debug.LogError("Target script is not selected!");
            return;
        }

        // 使用反射调用选择的函数
        targetScript.GetType().GetMethod(functionName)?.Invoke(targetScript, new object[] { currentPanel, targetPanel });
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(ButtonListener2Object))]
public class ButtonListener2ObjectEditor : Editor
{
    private SerializedProperty targetObjectProperty;
    private SerializedProperty targetScriptProperty;
    private SerializedProperty functionNameProperty;
    private List<string> methodNames = new List<string>();
    private List<string[]> methodParameters = new List<string[]>();

    private void OnEnable()
    {
        targetObjectProperty = serializedObject.FindProperty("targetObject");
        targetScriptProperty = serializedObject.FindProperty("targetScript");
        functionNameProperty = serializedObject.FindProperty("functionName");

    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 显示物体字段
        EditorGUILayout.PropertyField(targetObjectProperty, new GUIContent("Target Object"));

        if (targetObjectProperty.objectReferenceValue != null)
        {
            GameObject targetObject = (GameObject)targetObjectProperty.objectReferenceValue;

            // 获取物体上的脚本列表
            MonoBehaviour[] scripts = targetObject.GetComponents<MonoBehaviour>();
            string[] scriptNames = new string[scripts.Length];
            for (int i = 0; i < scripts.Length; i++)
            {
                scriptNames[i] = scripts[i].GetType().Name;
            }

            // 显示脚本选择下拉列表
            int selectedScriptIndex = EditorGUILayout.Popup("Script", GetSelectedIndex(targetScriptProperty.objectReferenceValue != null ? targetScriptProperty.objectReferenceValue.name : ""), scriptNames);

            if (selectedScriptIndex >= 0)
            {
                targetScriptProperty.objectReferenceValue = scripts[selectedScriptIndex];
                functionNameProperty.stringValue = "";
                RefreshMethodNames();

                // 强制刷新编辑器界面
                EditorUtility.SetDirty(target);
                serializedObject.ApplyModifiedProperties();
                Repaint();
            }

            SerializedProperty selectedScript = serializedObject.FindProperty("targetScript");
            EditorGUILayout.PropertyField(selectedScript);

            RefreshMethodNames();

            int selectedMethodIndex = EditorGUILayout.Popup("Function Name", GetSelectedIndex(functionNameProperty.stringValue), methodNames.ToArray());
            functionNameProperty.stringValue = methodNames[selectedMethodIndex];

            serializedObject.ApplyModifiedProperties();

            if (!string.IsNullOrEmpty(functionNameProperty.stringValue))
            {
                SerializedProperty currentPanelProperty = serializedObject.FindProperty("currentPanel");
                SerializedProperty targetPanelProperty = serializedObject.FindProperty("targetPanel");

                EditorGUILayout.PropertyField(currentPanelProperty);
                EditorGUILayout.PropertyField(targetPanelProperty);

                // 显示函数的参数
                if (selectedMethodIndex >= 0 && selectedMethodIndex < methodNames.Count)
                {
                    string[] parameters = methodParameters[selectedMethodIndex];
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        EditorGUILayout.LabelField(parameters[i]);
                    }
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Please select a target object.", MessageType.Info);
        }

        serializedObject.ApplyModifiedProperties();
    }


    private void RefreshMethodNames()
    {
        methodNames.Clear();
        methodParameters.Clear();

        ButtonListener2Object buttonClickListener = (ButtonListener2Object)target;
        MonoBehaviour targetScript = (MonoBehaviour)targetScriptProperty.objectReferenceValue;

        if (targetScript != null)
        {
            var methods = targetScript.GetType().GetMethods();

            foreach (var method in methods)
            {
                if (!method.IsSpecialName && method.ReturnType == typeof(void))
                {
                    var parameters = method.GetParameters();

                    if (parameters.Length <= 2 && ParametersAreSupported(parameters))
                    {
                        methodNames.Add(method.Name);

                        // 获取函数的参数列表
                        List<string> parameterNames = new List<string>();
                        foreach (var parameter in parameters)
                        {
                            parameterNames.Add(parameter.ParameterType.ToString() + " " + parameter.Name);
                        }
                        methodParameters.Add(parameterNames.ToArray());
                    }
                }
            }
        }
    }


    private bool ParametersAreSupported(ParameterInfo[] parameters)
    {
        foreach (var parameter in parameters)
        {
            if (parameter.ParameterType != typeof(GameObject))
            {
                return false;
            }
        }
        return true;
    }

    private int GetSelectedIndex(string selectedOption)
    {
        return methodNames.IndexOf(selectedOption);
    }
}
#endif
