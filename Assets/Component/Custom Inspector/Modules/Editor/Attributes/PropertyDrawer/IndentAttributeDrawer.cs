using CustomInspector.Extensions;
using UnityEditor;
using UnityEngine;

namespace CustomInspector.Editor
{
    [CustomPropertyDrawer(typeof(IndentAttribute))]
    public class IndentAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            IndentAttribute ia = (IndentAttribute)attribute;
            using (new EditorGUI.IndentLevelScope(ia.additionalIndentLevel))
            {
                DrawProperties.PropertyField(position, label, property, true);
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return DrawProperties.GetPropertyHeight(label, property);
        }
    }
}