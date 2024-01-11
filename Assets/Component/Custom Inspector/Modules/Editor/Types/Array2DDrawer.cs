using CustomInspector.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CustomInspector.Editor
{
    [CustomPropertyDrawer(typeof(Array2D<>))]
    [CustomPropertyDrawer(typeof(Array2DAttribute))]
    public class Array2DDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if(property == null)
                return;


            //Check type
            if (!fieldInfo.FieldType.IsGenericType
                || fieldInfo.FieldType.GetGenericTypeDefinition() != typeof(Array2D<>))
            {
                DrawProperties.DrawPropertyWithMessage(position, label, property,
                                "Array2DAttribute only valid on Array2D", MessageType.Error);
                return;
            }

            //foldout
            position.height = EditorGUIUtility.singleLineHeight;
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
            if (!property.isExpanded)
                return;
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            EditorGUI.indentLevel++;
            position = EditorGUI.IndentedRect(position);
            EditorGUI.indentLevel--;

            using (new NewIndentLevel(0))
            {

                //Get infos
                SerializedProperty rowAmount = property.FindPropertyRelative("rowAmount");
                SerializedProperty columnAmount = property.FindPropertyRelative("columnAmount");
                Debug.Assert(rowAmount != null, "Member not found on " + property.name);
                Debug.Assert(columnAmount != null, "Member not found on " + property.name);

                //Get rows
                SerializedProperty rows = property.FindPropertyRelative("rows"); //This is an array
                Debug.Assert(rows != null, "Rows-member not found");
                Debug.Assert(rows.isArray, "Expected rows to be an array");

                //draw size options
                int spacing = 15;
                Rect rowColumDefine = new(position)
                {
                    width = (position.width - spacing) / 2,
                    height = EditorGUIUtility.singleLineHeight,
                };
                using (new LabelWidthScope(EditorGUIUtility.labelWidth / 2f))
                {
                    EditorGUI.BeginChangeCheck();
                    int res = EditorGUI.IntField(rowColumDefine, new GUIContent("rows"), rowAmount.intValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        res = Math.Max(res, 1);
                        rowAmount.intValue = res;
                        rows.arraySize = res;
                    }

                    rowColumDefine.x += rowColumDefine.width + spacing;
                    EditorGUI.BeginChangeCheck();
                    res = EditorGUI.IntField(rowColumDefine, new GUIContent("columns"), columnAmount.intValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        res = Math.Max(res, 1);
                        columnAmount.intValue = res;
                        for (int i = 0; i < rowAmount.intValue; i++)
                        {
                            rows.GetArrayElementAtIndex(i).FindPropertyRelative("elements").arraySize = res;
                        }
                    }
                }

                int rowAmountValue = rowAmount.intValue;
                int columnAmountValue = columnAmount.intValue;

                //Draw
                float width = position.width;
                float columnWidth = (width - (EditorGUIUtility.standardVerticalSpacing * (columnAmountValue - 1))) / columnAmountValue;
                float rowHeight = DrawProperties.GetPropertyHeight(property.propertyType, GUIContent.none);
                Rect rect = new()
                {
                    x = position.x,
                    y = position.y + rowColumDefine.height + EditorGUIUtility.standardVerticalSpacing,
                    width = columnWidth,
                    height = rowHeight,
                };

                if (rows.arraySize != rowAmountValue)
                    rows.arraySize = rowAmountValue;

                for (int rowIndex = 0; rowIndex < rowAmountValue; rowIndex++)
                {
                    SerializedProperty row = rows.GetArrayElementAtIndex(rowIndex)
                                                     .FindPropertyRelative("elements");
                    if (row.arraySize != columnAmountValue)
                        row.arraySize = columnAmountValue;

                    rect.x = position.x;
                    for (int columnIndex = 0; columnIndex < columnAmountValue; columnIndex++)
                    {
                        SerializedProperty element = row.GetArrayElementAtIndex(columnIndex);
                        EditorGUI.PropertyField(rect, element, GUIContent.none, includeChildren: false);
                        rect.x += columnWidth + EditorGUIUtility.standardVerticalSpacing;
                    }

                    rect.y += rowHeight + EditorGUIUtility.standardVerticalSpacing;
                }
            }

            property.serializedObject.ApplyModifiedProperties();
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if(property == null)
                return 0f;

            //Check type
            if (!fieldInfo.FieldType.IsGenericType
                || fieldInfo.FieldType.GetGenericTypeDefinition() != typeof(Array2D<>))
            {
                return DrawProperties.GetPropertyWithMessageHeight(label, property);
            }

            //foldout
            if (!property.isExpanded)
                return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            //Calc height
            SerializedProperty rowAmount = property.FindPropertyRelative("rowAmount");
            Debug.Assert(rowAmount != null);

            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing //foldout
                + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing //row and column amounts
                //table size
                + rowAmount.intValue * (DrawProperties.GetPropertyHeight(property.propertyType, GUIContent.none)
                                + EditorGUIUtility.standardVerticalSpacing);
        }
    }
}
