using CustomInspector.Extensions;
using System;
using UnityEditor;
using UnityEngine;

namespace CustomInspector.Editor
{   
    [CustomPropertyDrawer(typeof(HideFieldAttribute))]
    public class HideFieldAttributeDrawer : PropertyDrawer
    {
        static bool isGlobalDisabled = false;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!isGlobalDisabled)
            {
                if (property.IsArrayElement()) //is element in a list
                {
                    position.height = DrawProperties.errorHeight;
                    EditorGUI.HelpBox(position, "Use [HideInInspector] to hide the whole list instead of just the elements", MessageType.Warning); //Pack lists in classes and show classes parallel
                    return;
                }
                else
                {
                    // nothing
                }
            }
            else
            {
                DrawProperties.PropertyField(position, label, property);
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!isGlobalDisabled)
            {
                if (property.IsArrayElement())
                    return DrawProperties.errorHeight;
                else
                    return -EditorGUIUtility.standardVerticalSpacing;
            }
            else
            {
                return DrawProperties.GetPropertyHeight(label, property);
            }
        }

        public class GlobalDisable : IDisposable
        {
            public GlobalDisable() => isGlobalDisabled = true;
            public void Dispose() => isGlobalDisabled = false;
        }
    }
}

