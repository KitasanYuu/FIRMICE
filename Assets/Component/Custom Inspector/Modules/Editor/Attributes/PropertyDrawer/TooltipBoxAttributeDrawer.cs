using CustomInspector.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CustomInspector.Editor
{
    [CustomPropertyDrawer(typeof(TooltipBoxAttribute))]
    public class TooltipBoxAttributeDrawer : PropertyDrawer
    {
        const float spacing = 0; //horizontal spacing between property and the infobox

        static GUIContent infoIcon = null; //cached for performance
        static float infoWidth;
        static void SetInfos()
        {
            infoIcon = EditorGUIUtility.IconContent(StylesConvert.ToInternalIconName(InspectorIcon.Info));
            infoWidth = GUI.skin.label.CalcSize(infoIcon).x;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            TooltipBoxAttribute t = (TooltipBoxAttribute)attribute;

            if (infoIcon == null)
                SetInfos();

            GUIContent infoContent = new(infoIcon)
            {
                tooltip = t.content,
            };
            
            position.width -= (infoWidth + spacing);
            position.width = Mathf.Max(position.width, EditorGUIUtility.labelWidth + EditorGUIUtility.fieldWidth);

            //Draw Property
            DrawProperties.PropertyField(position, label, property);
            //Draw Unit
            Rect uRect = new()
            {
                x = position.x + position.width + spacing,
                y = position.y,
                width = infoWidth,
                height = Mathf.Min(position.height, EditorGUIUtility.singleLineHeight),
            };
            using (new NewIndentLevel(0))
            {
                EditorGUI.LabelField(uRect, infoContent);
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return DrawProperties.GetPropertyHeight(label, property);
        }
    }
}
