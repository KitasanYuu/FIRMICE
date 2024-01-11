using CustomInspector.Extensions;
using System;
using UnityEditor;
using UnityEngine;

namespace CustomInspector.Editor
{
    [CustomPropertyDrawer(typeof(LabelSettingsAttribute))]
    public class LabelSettingsAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            LabelSettingsAttribute la = (LabelSettingsAttribute)attribute;
            if(la.newName != null)
                label.text = la.newName;

            float savedLabelWidth = EditorGUIUtility.labelWidth;
            DrawProperties.DrawLabelSettings(position, property, label, la.style.ToInteralStyle());
            EditorGUIUtility.labelWidth = savedLabelWidth;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return DrawProperties.GetPropertyHeight(label, property);
        }
    }
}