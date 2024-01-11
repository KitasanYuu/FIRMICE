using UnityEngine;
using UnityEditor;
using CustomInspector.Extensions;
using System;


namespace CustomInspector.Editor
{
    /// <summary>
    /// Shows the value as a label. e.g. for an info text
    /// </summary>
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ReadOnlyAttribute readOnlyAttr = (ReadOnlyAttribute)attribute;

            float savedLabelWidth = EditorGUIUtility.labelWidth;

            //position.height = DrawProperties.GetPropertyHeight(property, label);

            switch (readOnlyAttr.disableStyle)
            {
                case DisableStyle.GreyedOut:
                    using (new EditorGUI.DisabledScope(true))
                    {
                        DrawProperties.DrawLabelSettings(position, property, label, readOnlyAttr.labelStyle.ToInteralStyle());
                    }
                    break;

                case DisableStyle.OnlyText:
                    string valueString;
                    try
                    {
                        valueString = property.GetValue().ToString();
                    }
                    catch (NotSupportedException e)
                    {
                        valueString = e.Message;
                    }

                    switch (readOnlyAttr.labelStyle)
                    {
                        case LabelStyle.NoLabel:
                            EditorGUI.LabelField(position, valueString);
                            break;

                        case LabelStyle.EmptyLabel:
                            EditorGUI.LabelField(position, new GUIContent(" ", property.tooltip), new GUIContent(valueString, property.tooltip));
                            break;

                        case LabelStyle.NoSpacing:
                            float labelWidth = GUI.skin.label.CalcSize(label).x + 5; //some additional distance
                            EditorGUI.LabelField(position, label);
                            Rect propRect = new(position)
                            {
                                x = position.x + labelWidth,
                                width = position.width - labelWidth,
                            };
                            EditorGUI.LabelField(propRect, valueString);
                            break;

                        case LabelStyle.FullSpacing:
                            EditorGUI.LabelField(position, label, new GUIContent(valueString, property.tooltip));
                            break;
                        default:
                            throw new NotImplementedException(readOnlyAttr.labelStyle.ToString());
                    }
                    break;

                default:
                    throw new NotImplementedException(readOnlyAttr.disableStyle.ToString());
            }

            EditorGUIUtility.labelWidth = savedLabelWidth;
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return DrawProperties.GetPropertyHeight(label, property);
        }
    }
}