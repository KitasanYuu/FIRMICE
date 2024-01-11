using UnityEditor;
using UnityEngine;
using CustomInspector.Extensions;
using System;
using System.Linq;
using System.Collections.Generic;

namespace CustomInspector.Editor
{

    [CustomPropertyDrawer(typeof(ShowPropertyAttribute))]
    public class ShowPropertyAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ShowPropertyAttribute sm = (ShowPropertyAttribute)attribute;

            var owner = property.GetOwnerAsFinder();
            SerializedProperty prop = owner.FindPropertyRelative(sm.getPropertyPath);

            if (prop == null)
            {
                DrawProperties.DrawPropertyWithMessage(position, label, property, $"Property {sm.getPropertyPath} on {owner.Name} not found", MessageType.Error);
                return;
            }

            Rect propRect;
            using (new HideFieldAttributeDrawer.GlobalDisable())
            {
                //Draw
                GUIContent propLabel = new(sm.label, sm.tooltip);
                propRect = new(position)
                {
                    height = EditorGUI.GetPropertyHeight(prop, propLabel)
                };
                EditorGUI.BeginChangeCheck();
                DrawProperties.PropertyField(propRect, propLabel, prop);
                if (EditorGUI.EndChangeCheck())
                {
                    prop.serializedObject.ApplyModifiedProperties();
                }
            }

            //other
            propRect.y += propRect.height + EditorGUIUtility.standardVerticalSpacing;
            propRect.height = position.height - propRect.height - EditorGUIUtility.standardVerticalSpacing;
            DrawProperties.PropertyField(propRect, label, property);
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ShowPropertyAttribute sm = (ShowPropertyAttribute)attribute;

            SerializedProperty prop = property.GetOwnerAsFinder().FindPropertyRelative(sm.getPropertyPath);

            if (prop == null)
                return DrawProperties.GetPropertyWithMessageHeight(label, property);

            float showedPropHeight;
            using (new HideFieldAttributeDrawer.GlobalDisable())
            {
                showedPropHeight = EditorGUI.GetPropertyHeight(prop, label);
            }
            return showedPropHeight + EditorGUIUtility.standardVerticalSpacing + DrawProperties.GetPropertyHeight(label, property);
        }
    }
}