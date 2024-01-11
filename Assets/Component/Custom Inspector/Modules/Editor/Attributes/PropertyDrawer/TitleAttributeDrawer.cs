using CustomInspector.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CustomInspector.Editor
{
    [CustomPropertyDrawer(typeof(TitleAttribute))]
    public class TitleAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            TitleAttribute t = (TitleAttribute)attribute;

            GUIContent title = new(t.content, t.tooltip);

            //spacing
            float upperSpacing = Mathf.Max(t.upperSpacing, 0);
            position.y += upperSpacing;
            position.height -= upperSpacing;

            //style
            GUIStyle titleStyle = new(EditorStyles.boldLabel);
            titleStyle.fontSize = t.fontSize;

            //Draw title

            Rect r = new(position)
            {
                height = titleStyle.CalcSize(title).y,
            };

            EditorGUI.LabelField(r, title, titleStyle);

            //Draw propertyfield
            position.y += r.height + EditorGUIUtility.standardVerticalSpacing;
            position.height -= r.height + EditorGUIUtility.standardVerticalSpacing;
            DrawProperties.PropertyField(position, label, property);
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            TitleAttribute t = (TitleAttribute)attribute;

            //style
            GUIStyle titleStyle = new(EditorStyles.boldLabel);
            titleStyle.fontSize = t.fontSize;

            //Title
            float labelHeight = titleStyle.CalcSize(new GUIContent(t.content, t.tooltip)).y;

            return Mathf.Max(t.upperSpacing, 0)
                    + labelHeight + EditorGUIUtility.standardVerticalSpacing
                    + EditorGUI.GetPropertyHeight(property, label);
        }
    }
}
