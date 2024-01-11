using CustomInspector.Extensions;
using UnityEditor;
using UnityEngine;

namespace CustomInspector.Editor
{
    [CustomPropertyDrawer(typeof(ShowIfIsAttribute))]
    [CustomPropertyDrawer(typeof(ShowIfIsNotAttribute))]
    public class ShowIfIsAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ShowIfIsAttribute sa = (ShowIfIsAttribute)attribute;

            property.serializedObject.ApplyModifiedPropertiesWithoutUndo();

            object refValue = DirtyValue.GetOwner(property).FindRelative(sa.fieldPath).GetValue();

            bool show = (refValue.IsUnityNull() && sa.value.IsUnityNull())
                        || (!refValue.IsUnityNull() && refValue.Equals(sa.value));

            if (show ^ sa.Inverted)
            {
                //Show
                position.height = DrawProperties.GetPropertyHeight(label, property);
                using (new EditorGUI.IndentLevelScope(sa.indent))
                {
                    DrawProperties.PropertyField(position, label: label, property: property);
                }
                return;
            }
            else
            {
                if(sa.style == DisabledStyle.Invisible) //Hide
                    return;
                else //if(sa.style == DisabledStyle.Disabled) //show disabled
                {
                    position.height = DrawProperties.GetPropertyHeight(label, property);
                    using (new EditorGUI.IndentLevelScope(sa.indent))
                    {
                        DrawProperties.DisabledPropertyField(position, label: label, property: property);
                    }
                }
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ShowIfIsAttribute sa = (ShowIfIsAttribute)attribute;


            object refValue = DirtyValue.GetOwner(property).FindRelative(sa.fieldPath).GetValue();
            bool show = (refValue == null && sa.value == null)
                        || (refValue != null && refValue.Equals(sa.value));

            if (show ^ sa.Inverted)
            {
                //Show
                return DrawProperties.GetPropertyHeight(label, property);
            }
            else
            {
                if (sa.style == DisabledStyle.Invisible)//Hide
                    return -EditorGUIUtility.standardVerticalSpacing;
                else //if(sa.style == DisabledStyle.Disabled) //show disabled
                {
                    return DrawProperties.GetPropertyHeight(label, property);
                }
            }
        }
    }
}

