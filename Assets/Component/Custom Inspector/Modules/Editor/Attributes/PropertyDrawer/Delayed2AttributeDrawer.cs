using CustomInspector.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CustomInspector.Editor
{
    [CustomPropertyDrawer(typeof(Delayed2Attribute))]
    public class Delayed2AttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position = EditorGUI.IndentedRect(position);

            using (new NewIndentLevel(0))
            {
                label = PropertyValues.RepairLabel(label, property);
                var propertyType = property.propertyType;
                if (propertyType == SerializedPropertyType.Integer)
                {
                    EditorGUI.DelayedIntField(position, property, label);
                }
                else if (propertyType == SerializedPropertyType.Float)
                {
                    EditorGUI.DelayedFloatField(position, property, label);
                }
                else if (propertyType == SerializedPropertyType.String)
                {
                    EditorGUI.DelayedTextField(position, property, label);
                }
                else if (propertyType == SerializedPropertyType.Vector2Int
                        || propertyType == SerializedPropertyType.Vector2
                        || propertyType == SerializedPropertyType.Vector3Int
                        || propertyType == SerializedPropertyType.Vector3)
                {
                    float labelWidth = EditorGUIUtility.labelWidth;
                    {
                        if (propertyType == SerializedPropertyType.Vector2Int)
                        {
                            Rect[] bodys = DivideByAndSetLabelWidth(DrawLabelGetBody(), 2);
                            var v2i = property.vector2IntValue;
                            v2i.x = EditorGUI.DelayedIntField(bodys[0], "X", v2i.x);
                            v2i.y = EditorGUI.DelayedIntField(bodys[1], "Y", v2i.y);
                            property.vector2IntValue = v2i;
                        }
                        else if (propertyType == SerializedPropertyType.Vector2)
                        {
                            Rect[] bodys = DivideByAndSetLabelWidth(DrawLabelGetBody(), 2);
                            var v2 = property.vector2Value;
                            v2.x = EditorGUI.DelayedFloatField(bodys[0], "X", v2.x);
                            v2.y = EditorGUI.DelayedFloatField(bodys[1], "Y", v2.y);
                            property.vector2Value = v2;
                        }
                        else if (propertyType == SerializedPropertyType.Vector3Int)
                        {
                            Rect[] bodys = DivideByAndSetLabelWidth(DrawLabelGetBody(), 3);
                            var v3i = property.vector3IntValue;
                            v3i.x = EditorGUI.DelayedIntField(bodys[0], "X", v3i.x);
                            v3i.y = EditorGUI.DelayedIntField(bodys[1], "Y", v3i.y);
                            v3i.z = EditorGUI.DelayedIntField(bodys[2], "Z", v3i.z);
                            property.vector3IntValue = v3i;
                        }
                        else if (propertyType == SerializedPropertyType.Vector3)
                        {
                            Rect[] bodys = DivideByAndSetLabelWidth(DrawLabelGetBody(), 3);
                            var v3 = property.vector3Value;
                            v3.x = EditorGUI.DelayedFloatField(bodys[0], "X", v3.x);
                            v3.y = EditorGUI.DelayedFloatField(bodys[1], "Y", v3.y);
                            v3.z = EditorGUI.DelayedFloatField(bodys[2], "Z", v3.z);
                            property.vector3Value = v3;
                        }
                        else
                            throw new Exception("Vector type not handled");
                            

                        Rect DrawLabelGetBody()
                        {
                            Rect rect = new(position)
                            {
                                width = EditorGUIUtility.labelWidth - position.x,
                                height = EditorGUIUtility.singleLineHeight,
                            };
                            EditorGUI.LabelField(rect, label);

                            if (EditorGUIUtility.wideMode) //same line
                            {
                                rect.x += rect.width + 6;
                                rect.width = position.width - (rect.width + 6);
                            }
                            else //next line
                            {
                                rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                                rect.width = position.width;
                                using (new NewIndentLevel(1))
                                {
                                    rect = EditorGUI.IndentedRect(rect);
                                }
                            }
                            
                            return rect;
                        }
                        Rect[] DivideByAndSetLabelWidth(Rect r, int amount)
                        {
                            EditorGUIUtility.labelWidth = 14;

                            Debug.Assert(amount > 0);
                            Rect result = new()
                            {
                                x = r.x,
                                y = r.y,
                                width = (r.width - (amount - 1) * 5) / amount,
                                height = r.height
                            };
                            return Enumerable.Range(0, amount)
                                .Select<int, Rect>(i => { Rect r = new(result); r.x += i * (r.width + 5); return r; })
                                .ToArray();
                        }
                    }
                    EditorGUIUtility.labelWidth = labelWidth;
                }
                else
                {
                    DrawProperties.DrawPropertyWithMessage(position, label, property, $"Delayed2 (used on {propertyType}) is only available on numbers and strings", MessageType.Error);
                }
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Float:
                case SerializedPropertyType.String:
                    return EditorGUIUtility.singleLineHeight;

                case SerializedPropertyType.Vector2Int:
                case SerializedPropertyType.Vector2:
                case SerializedPropertyType.Vector3Int:
                case SerializedPropertyType.Vector3:
                {
                    if(EditorGUIUtility.wideMode)
                        return EditorGUIUtility.singleLineHeight;
                    else
                        return 2 * EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }

                default:
                    return DrawProperties.GetPropertyWithMessageHeight(label, property);
            };
        }
    }
}
