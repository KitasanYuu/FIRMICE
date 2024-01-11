using CustomInspector.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CustomInspector.Editor
{
    [CustomPropertyDrawer(typeof(InspectorIconAttribute))]
    public class InspectorIconAttributeDrawer : PropertyDrawer
    {
        const float horizontalSpace = 20;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            InspectorIconAttribute ia = (InspectorIconAttribute)attribute;
            
            GUIContent c = GetIcon(ia.icon);

            position = EditorGUI.IndentedRect(position);
            using (new NewIndentLevel(0))
            {
                Rect iconRect = new(position)
                {
                    width = horizontalSpace,
                    height = EditorGUIUtility.singleLineHeight,
                };
                position.width -= horizontalSpace;

                using (new LabelWidthScope())
                {
                    if (ia.appendAtEnd == false)
                    {
                        position.x += horizontalSpace;
                        EditorGUIUtility.labelWidth -= horizontalSpace;
                    }
                    else
                    {
                        iconRect.x = position.x + position.width;
                    }

                    EditorGUI.LabelField(iconRect, c);

                    DrawProperties.PropertyField(position, label, property);
                }
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return DrawProperties.GetPropertyHeight(label, property);
        }
        //Gets saved for performance
        readonly static Dictionary<InspectorIcon, GUIContent> icons = new();
        GUIContent GetIcon(InspectorIcon icon)
        {
            if(!icons.TryGetValue(icon, out GUIContent res))
            {
                string iconName = icon.ToInternalIconName();
                res = EditorGUIUtility.IconContent(iconName);
                icons.Add(icon, res);
            }
            return res;
        }
    }
}
