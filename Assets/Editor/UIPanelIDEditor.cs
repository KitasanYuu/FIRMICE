using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(UIPanelID))]
public class UIPanelIDEditor : Editor
{
    private SerializedProperty uiIDProperty;

    private void OnEnable()
    {
        uiIDProperty = serializedObject.FindProperty("UIID");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(uiIDProperty, true);

        if (GUILayout.Button("Add New UIIdentity"))
        {
            UIPanelID uiPanelID = (UIPanelID)target;
            uiPanelID.UIID.Add(new UIIdentity());
            EditorUtility.SetDirty(uiPanelID); // 标记目标对象已被修改
        }

        serializedObject.ApplyModifiedProperties();
    }
}

[CustomEditor(typeof(UIIdentity))]
public class UIIdentityEditor : Editor
{
    private SerializedProperty panelIDProperty;
    private SerializedProperty subIdentityProperty;

    private void OnEnable()
    {
        panelIDProperty = serializedObject.FindProperty("PanelID");
        subIdentityProperty = serializedObject.FindProperty("SubIdentity");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(panelIDProperty);
        EditorGUILayout.PropertyField(subIdentityProperty, true);

        if (GUILayout.Button("Add New SubSelectIdentity"))
        {
            // 首先将 targetObject 转换为 UIPanelID 对象
            UIPanelID uiPanelID = target as UIPanelID;
            if (uiPanelID != null)
            {
                // 获取 UIPanelID 对象中的 UIIdentity 列表
                List<UIIdentity> uiIdentities = uiPanelID.UIID;
                if (uiIdentities != null && uiIdentities.Count > 0)
                {
                    // 获取列表中的第一个 UIIdentity 对象
                    UIIdentity uiIdentity = uiIdentities[0]; // 这里根据你的实际需求来获取 UIIdentity 对象
                    if (uiIdentity != null)
                    {
                        // 向 UIIdentity 对象的 SubIdentity 列表中添加新的 SubSelectIdentity 对象
                        uiIdentity.SubIdentity.Add(new SubSelectIdentity());
                        EditorUtility.SetDirty(uiPanelID); // 标记目标对象已被修改
                    }
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

}
