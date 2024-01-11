using CustomInspector.Extensions;
using UnityEditor;
using UnityEngine;

namespace CustomInspector.Editor
{
    [CustomPropertyDrawer(typeof(BackgroundColorAttribute))]
    public class BackgroundColorAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            BackgroundColorAttribute bc = (BackgroundColorAttribute)attribute;
            position.height = DrawProperties.GetPropertyHeight(label, property) + 2 * bc.borderSize;
            Rect coloredRect = EditorGUI.IndentedRect(position);
            EditorGUI.DrawRect(coloredRect, bc.color.ToColor());

            Rect shrinked = new(position.x + bc.borderSize, position.y + bc.borderSize,
                                    position.width - 2 * bc.borderSize, position.height - 2 * bc.borderSize);

            var savedLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = (EditorGUIUtility.labelWidth / position.width) * shrinked.width;
            DrawProperties.PropertyField(shrinked, label, property);
            EditorGUIUtility.labelWidth = savedLabelWidth;
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            BackgroundColorAttribute bc = (BackgroundColorAttribute)attribute;
            return DrawProperties.GetPropertyHeight(label, property) + 2 * bc.borderSize + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}