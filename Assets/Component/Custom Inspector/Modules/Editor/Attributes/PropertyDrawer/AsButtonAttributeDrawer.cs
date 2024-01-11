using CustomInspector.Extensions;
using CustomInspector.Helpers.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CustomInspector.Editor
{
    [CustomPropertyDrawer(typeof(AsButtonAttribute))]
    public class AsButtonAttributeDrawer : PropertyDrawer
    {
        bool startedMouseDownOverMe = false;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!IsValidType())
            {
                DrawProperties.DrawPropertyWithMessage(position,
                                                    label,
                                                    property,
                                                    $"[AsButton]-attribute does not support type '{property.propertyType}'." +
                                                    $"\nSupported types: bool, int, string & InspectorButtonState",
                                                    MessageType.Error);
                return;
            }


            AsButtonAttribute b = (AsButtonAttribute)attribute;


            label = PropertyValues.RepairLabel(label, property); // bugfix
            if(!string.IsNullOrEmpty(b.label))
                label.text = b.label;

            Rect buttonRect = GetButtonRect(position, label, b.size);
            if (!string.IsNullOrEmpty(b.tooltip))
            {
                if (string.IsNullOrEmpty(label.tooltip))
                    label.tooltip = b.tooltip;
                else
                    label.tooltip += "\n" + b.tooltip;
            }


            bool isOver = buttonRect.Contains(Event.current.mousePosition);

            if (Event.current.type == EventType.MouseDown)
                startedMouseDownOverMe = isOver;

            bool isPressed = isOver && startedMouseDownOverMe && EditorGUIUtility.hotControl != 0;

            if (b.staysPressed)
            {
                // property.isExpanded <==> isSelected
                Color c = property.isExpanded ? b.selectedColor.ToColor() : Color.white;
                using (new BackgroundColorScope(c))
                {
                    if (isPressed)
                        property.SetValue(IsPressedValue());
                    else
                    {
                        if(property.isExpanded) // is selected
                            property.SetValue(IsSelectedValue());
                        else
                            property.SetValue(NotSelectedValue());
                    }

                    if (GUI.Button(buttonRect, label))
                    {
                        if(!property.isExpanded) //if is not selected
                        {
                            property.isExpanded = true;
                            property.SetValue(IsSelectedValue());
                        }
                        else
                        {
                            property.isExpanded = false;
                            property.SetValue(NotSelectedValue());
                        }
                    }
                }
            }
            else
            {
                GUI.Button(buttonRect, label);

                if (isPressed)
                    property.SetValue(IsPressedValue());
                else
                    property.SetValue(NotSelectedValue());
            }
        }
        object NotSelectedValue()
        {
            if (fieldInfo.FieldType == typeof(bool))
                return false;
            else if (fieldInfo.FieldType == typeof(int))
                return 0;
            else if (fieldInfo.FieldType == typeof(string))
                return "notSelected";
            else if (fieldInfo.FieldType == typeof(InspectorButtonState))
                return InspectorButtonState.notSelected;
            else
                throw new NotImplementedException($"{fieldInfo.FieldType}");
        }
        object IsPressedValue()
        {
            if (fieldInfo.FieldType == typeof(bool))
                return true;
            else if (fieldInfo.FieldType == typeof(int))
                return 1;
            else if (fieldInfo.FieldType == typeof(string))
                return "isPressed";
            else if (fieldInfo.FieldType == typeof(InspectorButtonState))
                return InspectorButtonState.isPressed;
            else
                throw new NotImplementedException($"{fieldInfo.FieldType}");
        }
        object IsSelectedValue()
        {
            if (fieldInfo.FieldType == typeof(bool))
                return true;
            else if (fieldInfo.FieldType == typeof(int))
                return 2;
            else if (fieldInfo.FieldType == typeof(string))
                return "isSelected";
            else if (fieldInfo.FieldType == typeof(InspectorButtonState))
                return InspectorButtonState.isSelected;
            else
                throw new NotImplementedException($"{fieldInfo.FieldType}");
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (IsValidType())
            {
                Size size = ((AsButtonAttribute)attribute).size;
                return GetButtonHeight(size);
            }
            else
            {
                return DrawProperties.GetPropertyWithMessageHeight(label, property);
            }
        }
        bool IsValidType()
        {
            return fieldInfo.FieldType == typeof(bool)
                || fieldInfo.FieldType == typeof(int)
                || fieldInfo.FieldType == typeof(string)
                || fieldInfo.FieldType == typeof(InspectorButtonState);
        }
        static Rect GetButtonRect(Rect position, GUIContent label, Size size)
        {
            float width = StylesConvert.ToButtonWidth(position.width, label, size);
            float height = GetButtonHeight(size);
            return new(position.x + (position.width - width) / 2, position.y,
                    width, height);
        }
        static float GetButtonHeight(Size size)
            => StylesConvert.ToButtonHeight(size);
    }
}
