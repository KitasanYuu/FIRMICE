using CustomInspector.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static CustomInspector.Extensions.DrawProperties.MultiColumnList.Column;

namespace CustomInspector.Editor
{
    [CustomPropertyDrawer(typeof(SetAttribute))]
    [CustomPropertyDrawer(typeof(SerializableSet<>))]
    [CustomPropertyDrawer(typeof(SerializableSortedSet<>))]
    public class SetDrawer : PropertyDrawer
    {
        const string valuesListPath = "values.list";
        const float errorHeight = 60;
        //Remove has to happen after dict was drawn. otherwise tries to draw removed things
        List<Action> removesAfterDraw = new();
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //Check type
            if (!ValidType())
            {
                DrawProperties.DrawPropertyWithMessage(position, label, property,
                    "SerializableDictionaryAttribute only valid on SerializableDictionary's", MessageType.Error);
                return;
            }

            //draw dict
            SerializedProperty values = property.FindPropertyRelative(valuesListPath);
            if (values == null) // values could not be found, probably because key was not serializable
            {
                position.height = errorHeight;
                Type valuesType = new DirtyValue(property).FindRelative(valuesListPath).Type;
                EditorGUI.HelpBox(position, $"Sets element-type is non-serializable: {valuesType.GetGenericArguments()[0]}." +
                                            $"\nMaybe add [System.Serializable] if its your custom class", MessageType.Error);
                return;
            }

            DrawProperties.ListDrawer.DrawHeader(position, label, values);
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            if (!values.isExpanded)
                return;

            using (new EditorGUI.IndentLevelScope(1))
            {
                position = EditorGUI.IndentedRect(position);

                using (new NewIndentLevel(0))
                {
                    //Draw Dictionary
                    var dictColumns = GetSetColumns(property: property, values: values);
                    float listHeight = DrawProperties.MultiColumnList.DrawList(position, columns: dictColumns);
                    foreach (var rem in removesAfterDraw)
                    {
                        rem.Invoke();
                    }
                    removesAfterDraw.Clear();


                    position.y += listHeight + EditorGUIUtility.standardVerticalSpacing;

                    //Edit
                    position.height = EditorGUIUtility.singleLineHeight;
                    property.isExpanded = EditorGUI.Foldout(position, property.isExpanded,
                                    new GUIContent("Edit", "add or remove elements"));

                    if (!property.isExpanded)
                        return;
                    position.y += position.height + EditorGUIUtility.standardVerticalSpacing;

                    using (new EditorGUI.IndentLevelScope(1))
                    {
                        position = EditorGUI.IndentedRect(position);

                        using (new NewIndentLevel(0))
                        {
                            //Edit options
                            var editColumns = GetEditColumns(property, values.arraySize);
                            DrawProperties.MultiColumnList.DrawList(position, columns: editColumns);
                        }
                    }
                }
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            //Check type
            if (!ValidType())
                return DrawProperties.GetPropertyWithMessageHeight(label, property);


            SerializedProperty values = property.FindPropertyRelative(valuesListPath);
            if (values == null) // could not be found, probably because value was not serializable
            {
                return errorHeight;
            }

            float totalHeight = EditorGUIUtility.singleLineHeight;

            if (!values.isExpanded)
                return totalHeight;

            var dictColumns = GetSetColumns(property: property, values: values);
            totalHeight += EditorGUIUtility.standardVerticalSpacing
                            + DrawProperties.MultiColumnList.GetHeight(columns: dictColumns)
                            + EditorGUIUtility.standardVerticalSpacing
                            + EditorGUIUtility.singleLineHeight; //list and the edit

            if (!property.isExpanded)
                return totalHeight;

            var editColumns = GetEditColumns(property, values.arraySize);
            totalHeight += DrawProperties.MultiColumnList.GetHeight(columns: editColumns)
                            + EditorGUIUtility.standardVerticalSpacing;

            return totalHeight;
        }
        bool ValidType()
        {
            return fieldInfo.FieldType.IsGenericType &&
            (fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(SerializableSet<>)
               || fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(SerializableSortedSet<>));
        }
        DrawProperties.MultiColumnList.Column[] GetSetColumns(SerializedProperty property, SerializedProperty values)
        {
            //create list body
            DrawProperties.MultiColumnList.Column indices;
            DrawProperties.MultiColumnList.Column valuesColumn;
            DrawProperties.MultiColumnList.Column deleteButtons;

            if (values.arraySize > 0)
            {
                indices = new(false, Enumerable.Range(0, values.arraySize)
                                     .Select(_ => $"Element: {_}")
                                     .Select(_ => new Entry(EditorGUIUtility.singleLineHeight, (position) => EditorGUI.LabelField(position, _))),
                                     fixedWidth: GUI.skin.label.CalcSize(new GUIContent($"Element: {values.arraySize}")).x + 5);

                valuesColumn = new(true, Enumerable.Range(0, values.arraySize)
                                     .Select(_ => values.GetArrayElementAtIndex(_))
                                     .Select(_ => new Entry(DrawProperties.GetPropertyHeight(_), (position) => DrawProperties.PropertyFieldWithoutLabel(position, _, true))));

                deleteButtons = new(false, Enumerable.Range(0, values.arraySize)
                                     .Select(_ => new Entry(EditorGUIUtility.singleLineHeight, (position) =>
                                     {
                                         removesAfterDraw.Add(() =>
                                         {
                                             if (GUI.Button(position, new GUIContent("-", "Remove this keyValuePair")))
                                             {
                                                 property.serializedObject.ApplyModifiedProperties();
                                                 property.CallMethodInside("RemoveAt", new object[] { _ });
                                                 if (!property.ApplyModifiedField(true))
                                                 {
                                                     Debug.LogWarning("Remove could not be saved");
                                                 }
                                             }
                                         });
                                     })),
                                     fixedWidth: EditorGUIUtility.singleLineHeight);
            }
            else
            {
                indices = new(false, Enumerable.Repeat(new Entry(0, (position) => { }), 1));
                valuesColumn = new(false, Enumerable.Repeat(new Entry(EditorGUIUtility.singleLineHeight, (position) => EditorGUI.LabelField(position, "(empty)")), 1));
                deleteButtons = new(false, Enumerable.Repeat(new Entry(0, (position) => { }), 1));
            }

            //insert headers to columns
            GUIContent valuesHeader = new($"value: {PropertyConversions.GetIListElementType(DirtyValue.GetType(values)).Name}");

            indices.entrys = Enumerable.Repeat(new Entry(0, (position) => { }), 1)
                                    .Concat(indices.entrys);
            valuesColumn.entrys = Enumerable.Repeat(new Entry(EditorGUIUtility.singleLineHeight, (position) => EditorGUI.LabelField(position, valuesHeader)), 1)
                                    .Concat(valuesColumn.entrys);
            deleteButtons.entrys = Enumerable.Repeat(new Entry(0, (position) => { }), 1)
                                    .Concat(deleteButtons.entrys);

            return new DrawProperties.MultiColumnList.Column[] { indices, valuesColumn, deleteButtons };
        }
        DrawProperties.MultiColumnList.Column[] GetEditColumns(SerializedProperty property, int keysArraySize)
        {
            SerializedProperty editor_input = property.FindPropertyRelative("editor_input");

            DrawProperties.MultiColumnList.Column buttons = new(false, new List<Entry>()
            {
                new Entry(EditorGUIUtility.singleLineHeight, (position) =>
                {
                    if (GUI.Button(position, new GUIContent("TryAdd", "Adds given key/value pair to the dictionary")))
                    {
                        property.serializedObject.ApplyModifiedProperties();
                        object value = editor_input.GetValue();
                        if(value != null)
                        {
                            bool wasAdded = (bool)property.CallMethodInside("TryAdd", new object[] { value });
                            if (wasAdded)
                            {
                                if (!property.ApplyModifiedField(true))
                                {
                                    Debug.LogWarning("TryAdd could not be saved");
                                }
                                EditorGUIUtility.keyboardControl = 0;
                            }
                            else
                                Debug.Log("Dictionary already contains key");
                        }
                        else
                            Debug.Log("Cannot add 'null' to Dictionary");
                    }
                }),
                new Entry(EditorGUIUtility.singleLineHeight, (position) =>
                {
                    if (GUI.Button(position, new GUIContent("Remove", "Removes entry with given key from the dictionary")))
                    {
                        property.serializedObject.ApplyModifiedProperties();
                        bool wasRemoved = (bool)property.CallMethodInside("Remove", new object[] { editor_input.GetValue() });
                        if (wasRemoved)
                        {
                            if (!property.ApplyModifiedField(true))
                            {
                                Debug.LogWarning("Remove could not be saved");
                            }
                        }
                    }
                }),
                new Entry(EditorGUIUtility.singleLineHeight, (position) =>
                {
                    using (new EditorGUI.DisabledScope(keysArraySize <= 0))
                    {
                        if (GUI.Button(position, new GUIContent("Clear", "Removes all key/value pairs from the dictionary")))
                        {
                            property.serializedObject.ApplyModifiedProperties();
                            property.CallMethodInside("Clear", new object[] { });
                            if (!property.ApplyModifiedField(true))
                            {
                                Debug.LogWarning("Clear could not be saved");
                            }
                        }
                    }
                }),
            }, 80);

            DrawProperties.MultiColumnList.Column buttonValues
                = new(false, Enumerable.Repeat(new Entry(DrawProperties.GetPropertyHeight(editor_input),
                      (position) => DrawProperties.PropertyFieldWithoutLabel(position, editor_input)), 1)
                      .Concat(Enumerable.Repeat(new Entry(0, (position) => { }), 2)));

            return new DrawProperties.MultiColumnList.Column[] { buttons, buttonValues };
        }
    }
}