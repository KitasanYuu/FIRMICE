using CustomInspector.Extensions;
using System;
using UnityEditor;
using UnityEngine;


namespace CustomInspector.Editor
{
    //Displays autoproperties e.g. public string s1 { get; private set; }
    [CustomPropertyDrawer(typeof(DisplayAutoPropertyAttribute))]
    public class DisplayAutoPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = EditorGUIUtility.singleLineHeight;


            DirtyValue owner = DirtyValue.GetOwner(property);

            DisplayAutoPropertyAttribute ap = (DisplayAutoPropertyAttribute)attribute;
            (string pre, string name) path = PropertyConversions.DividePath(ap.propertyPath, true);
            string propertyPath = $"{path.pre}<{path.name}>k__BackingField";
            DirtyValue value;
            object val;
            try
            {
                value = owner.FindRelative(propertyPath);
                val = value.GetValue();
            }
            catch (Exception e)
            {
                DrawProperties.DrawPropertyWithMessage(position, label, property, e.Message, MessageType.Error);
                return;
            }

            Rect apRect = new(position)
            {
                height = DrawProperties.GetPropertyHeight(PropertyConversions.ToPropertyType(value.Type), label),
            };

            using (new EditorGUI.DisabledScope(!ap.allowChange || !Application.isPlaying)) //property wont be saved, so you shouldnt think you could do
            {
                EditorGUI.BeginChangeCheck();
                object res = DrawProperties.DrawField(apRect, GetGUIContent(path.name), val, value.Type);
                if (EditorGUI.EndChangeCheck())
                {
                    value.SetValue(res);
                }
            }

            //Draw Property below
            Rect propRect = new(position)
            {
                y = position.y + apRect.height + EditorGUIUtility.standardVerticalSpacing,
                height = DrawProperties.GetPropertyHeight(label, property),
            };
            DrawProperties.PropertyField(propRect, label, property);
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            DirtyValue owner = DirtyValue.GetOwner(property);

            DisplayAutoPropertyAttribute ap = (DisplayAutoPropertyAttribute)attribute;
            (string pre, string name) path = PropertyConversions.DividePath(ap.propertyPath, true);
            string propertyPath = $"{path.pre}<{path.name}>k__BackingField";
            DirtyValue value;
            try
            {
                value = owner.FindRelative(propertyPath);
                value.GetValue();
            }
            catch
            {
                return DrawProperties.GetPropertyWithMessageHeight(label, property);
            }


            SerializedPropertyType apType = PropertyConversions.ToPropertyType(value.Type);
            GUIContent apLabel = GetGUIContent(path.name);
            return DrawProperties.GetPropertyHeight(type: apType, label: apLabel)
                + EditorGUIUtility.standardVerticalSpacing
                + DrawProperties.GetPropertyHeight(label, property);
        }
        GUIContent GetGUIContent(string fieldName)
        {
            DisplayAutoPropertyAttribute ap = (DisplayAutoPropertyAttribute)attribute;

            Debug.Assert(fieldName != null, "Field name not found");
            GUIContent content = ap.label == null ? new(PropertyConversions.NameFormat(fieldName)) : new(ap.label);
            if(ap.tooltip != null)
                content.tooltip = ap.tooltip;

            return content;
        }
    }
}