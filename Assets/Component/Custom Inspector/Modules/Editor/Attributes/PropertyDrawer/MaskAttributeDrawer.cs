using CustomInspector.Extensions;
using System;
using UnityEditor;
using UnityEngine;


namespace CustomInspector.Editor
{
    [CustomPropertyDrawer(typeof(MaskAttribute))]
    public class MaskAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = EditorGUIUtility.singleLineHeight;

            if(property.propertyType == SerializedPropertyType.Integer)
            {
                MaskAttribute m = (MaskAttribute)attribute;

                Rect labelRect = new(position)
                {
                    width = EditorGUIUtility.labelWidth,
                };
                EditorGUI.LabelField(labelRect, label);

                Rect toggleRect = new(position)
                {
                    x = labelRect.x + labelRect.width,
                    width = EditorGUIUtility.singleLineHeight,
                };

                int value = property.intValue;
                using (new NewIndentLevel(0))
                {
                    EditorGUI.BeginChangeCheck();
                    Debug.Assert(m.bitNames == null || m.bitsAmount == m.bitNames.Length, "BitNames have different length than bitAmount");
                    for (int i = 0; i < m.bitsAmount; i++)
                    {
                        if (m.bitNames != null)
                        {
                            GUIContent bitLabelGuiC = new(m.bitNames[i], $"bit {i}");
                            Rect bitLabelR = new(toggleRect)
                            {
                                width = GUI.skin.label.CalcSize(bitLabelGuiC).x,
                            };
                            EditorGUI.LabelField(bitLabelR, bitLabelGuiC);
                            toggleRect.x += bitLabelR.width + EditorGUIUtility.standardVerticalSpacing;
                        }
                        bool res = EditorGUI.Toggle(toggleRect, (value & (1 << i)) != 0);

                        if (res)
                        {
                            value |= 1 << i;
                        }
                        else
                        {
                            value &= ~(1 << i);
                        }

                        toggleRect.x += toggleRect.width + EditorGUIUtility.standardVerticalSpacing;
                        //if out of view
                        if (toggleRect.x > position.x + position.width)
                            break;
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.intValue = value;
                        property.serializedObject.ApplyModifiedProperties();
                    }
                }
            }
            else if(property.propertyType == SerializedPropertyType.Enum)
            {
                EditorGUI.BeginChangeCheck();
                Enum res = EditorGUI.EnumFlagsField(position, label, (Enum)Enum.ToObject(fieldInfo.FieldType, property.intValue));
                if (EditorGUI.EndChangeCheck())
                {
                    property.intValue = (int)Convert.ChangeType(res, typeof(int));
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
            else
            {
                EditorGUI.HelpBox(position, "MaskAttribute only supports integers and enums", MessageType.Error);
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}