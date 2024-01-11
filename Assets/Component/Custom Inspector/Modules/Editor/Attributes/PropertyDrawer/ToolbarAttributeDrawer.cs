using UnityEditor;
using UnityEngine;


namespace CustomInspector.Editor
{
    
    [CustomPropertyDrawer(typeof(ToolbarAttribute))]
    public class ToolbarAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ToolbarAttribute ta = (ToolbarAttribute)attribute;
            

            if (property.propertyType == SerializedPropertyType.Boolean)
            {
                float width = Mathf.Min(position.width, 2 * ta.maxWidth);
                Rect rect = new(position.x + (position.width - width) / 2,
                            position.y + ta.space,
                            width, ta.height);

                EditorGUI.BeginChangeCheck();
                bool res = GUI.Toolbar(rect, property.boolValue ? 0 : 1, new string[] { property.name, "None" }) == 0;
                if (EditorGUI.EndChangeCheck())
                    property.boolValue = res;
            }
            else if (property.propertyType == SerializedPropertyType.Enum)
            {
                string[] names = property.enumNames;
                float width = Mathf.Min(position.width, names.Length * ta.maxWidth);
                Rect rect = new(position.x + (position.width - width) / 2,
                             position.y + ta.space,
                             width, ta.height);

                EditorGUI.BeginChangeCheck();
                int res = GUI.Toolbar(rect, property.enumValueIndex, names);
                if(EditorGUI.EndChangeCheck())
                {
                    property.enumValueIndex = res;
                }
            }
            else
            {
                EditorGUI.HelpBox(position, $"({property.propertyType} not supported)", MessageType.Error);
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ToolbarAttribute ta = (ToolbarAttribute)attribute;
            return ta.height + 2 * ta.space;
        }
    }
}