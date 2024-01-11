using CustomInspector.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CustomInspector.Editor
{
    [CustomPropertyDrawer(typeof(UnitAttribute))]
    public class UnitAttributeDrawer : PropertyDrawer
    {
        const float spacing = 4;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            UnitAttribute u = (UnitAttribute)attribute;
            GUIContent unit = new(u.unitName, "used Unit");
            float width = GetWidth(unit.text);
            position.width -= (width + spacing);
            position.width = Mathf.Max(position.width, EditorGUIUtility.labelWidth + EditorGUIUtility.fieldWidth);

            //Draw Property
            DrawProperties.PropertyField(position, label, property);
            //Draw Unit
            Rect uRect = new()
            {
                x = position.x + position.width + spacing,
                y = position.y,
                width = width,
                height = position.height,
            };
            using (new NewIndentLevel(0))
            {
                EditorGUI.LabelField(uRect, unit);
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return DrawProperties.GetPropertyHeight(label, property);
        }
        //Savings for performance
        static readonly Dictionary<string, float> namesWidth = new();
        float GetWidth(string name)
        {
            if(!namesWidth.TryGetValue(name, out float width))
            {
                width = GUI.skin.label.CalcSize(new GUIContent(name)).x;
                namesWidth.Add(name, width);
            }
            return width;
        }
    }
}
