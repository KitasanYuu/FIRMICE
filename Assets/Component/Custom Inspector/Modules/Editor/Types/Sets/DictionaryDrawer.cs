using CustomInspector.Extensions;
using CustomInspector.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using static CustomInspector.Extensions.DrawProperties.MultiColumnList.Column;

namespace CustomInspector.Editor
{
    [CustomPropertyDrawer(typeof(DictionaryAttribute))]
    [CustomPropertyDrawer(typeof(SerializableDictionary<,>))]
    [CustomPropertyDrawer(typeof(SerializableSortedDictionary<,>))]
    [CustomPropertyDrawer(typeof(ReorderableDictionary<,>))]
    public class DictionaryDrawer : PropertyDrawer
    {
        //const float errorHeight = 60;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            try
            {
                PropInfo info = GetInfo(property);

                if (info.errorMessage != null)
                {
                    DrawProperties.DrawPropertyWithMessage(position, label, property, info.errorMessage, MessageType.Error);
                    return;
                }
                else
                {
                    info.gui(position, label, property);
                    return;
                }
            }
            catch(ExitGUIException e)
            {
                throw e;
            }
            catch(Exception e)
            {
                Debug.LogException(e);
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            try
            {
                PropInfo info = GetInfo(property);

                if (info.errorMessage != null)
                {
                    return DrawProperties.GetPropertyWithMessageHeight(label, property);
                }
                else
                {
                    return info.height(property);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return 0;
            }
}

        static Dictionary<PropertyIdentifier, PropInfo> infos = new();
        PropInfo GetInfo(SerializedProperty property)
        {
            PropertyIdentifier id = new(property);
            if (!infos.TryGetValue(id, out PropInfo info))
            {
                info = new PropInfo(property, attribute as DictionaryAttribute, fieldInfo);
            }
            return info;
        }

        class PropInfo
        {
            public readonly string errorMessage = null;

            public readonly float keyWidth;

            //only for nonreorderable: Remove has to happen after dict was drawn. otherwise tries to draw removed things
            public readonly List<Action> removesAfterDraw = new();
            public readonly Action<Rect, GUIContent, SerializedProperty> gui;
            public readonly Func<SerializedProperty, float> height;

            public PropInfo(SerializedProperty property, DictionaryAttribute attribute, FieldInfo fieldInfo)
            {
                if (!fieldInfo.FieldType.IsGenericType || fieldInfo.FieldType.GetGenericArguments().Length != 2)
                {
                    errorMessage = "[Dictionary]-attribute is only valid on Custominspector dictionaries.";
                    return;
                }
                var args = fieldInfo.FieldType.GetGenericArguments();


                keyWidth = attribute?.keySize ?? DictionaryAttribute.defaultKeySize;
                if (keyWidth <= 0)
                    Debug.LogWarning($"Dictionary on {property.serializedObject.targetObject}->{property.propertyPath}:\nKeysize is set to zero");

                if (fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(SerializableDictionary<,>)
                    || fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(SerializableSortedDictionary<,>))
                {
                    string keysListPath = "keys.values.list";
                    string valuesListPath = "values.list";

                    SerializedProperty keys_instantiate = property.FindPropertyRelative(keysListPath);
                    SerializedProperty values_instantiate = property.FindPropertyRelative(valuesListPath);

                    if (keys_instantiate == null)
                    {
                        errorMessage = $"Dictionary: Type '{args[0]}' is not serializable."
                         + $"\nMaybe add [System.Serializable] if its your custom class";
                        return;
                    }
                    if (values_instantiate == null)
                    {
                        errorMessage = $"Dictionary: Type '{args[1]}' is not serializable"
                        + $"\nMaybe add [System.Serializable] if its your custom class";
                        return;
                    }

                    if(keys_instantiate.arraySize != values_instantiate.arraySize)
                    {
                        int amount;
                        if(keys_instantiate.arraySize > values_instantiate.arraySize) //more keys
                        {
                            amount = keys_instantiate.arraySize - values_instantiate.arraySize;
                            for (int i = keys_instantiate.arraySize - 1; i >= keys_instantiate.arraySize; i--)
                            {
                                keys_instantiate.DeleteArrayElementAtIndex(i);
                            }
                        }
                        else //more values
                        {
                            amount = values_instantiate.arraySize - keys_instantiate.arraySize;
                            for (int i = values_instantiate.arraySize - 1; i >= keys_instantiate.arraySize; i--)
                            {
                                values_instantiate.DeleteArrayElementAtIndex(i);
                            }
                        }

                        Debug.LogError($"InternalDictionaryError: keys do not match values." +
                            $"\n{amount} elements deleted");
                    }

                    gui = (position, label, property) =>
                    {
                        SerializedProperty keys = property.FindPropertyRelative(keysListPath);
                        SerializedProperty values = property.FindPropertyRelative(valuesListPath);

                        DrawProperties.ListDrawer.DrawHeader(position, label, keys);
                        position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        if (!keys.isExpanded)
                            return;

                        using (new EditorGUI.IndentLevelScope(1))
                        {
                            position = EditorGUI.IndentedRect(position);

                            using (new NewIndentLevel(0))
                            {
                                //Draw Dictionary
                                var dictColumns = NonReorderable.GetDictionaryColumns(property: property, keys: keys, values: values, position.width, this);
                                float listHeight = DrawProperties.MultiColumnList.DrawList(position, columns: dictColumns);
                                foreach (var rem in removesAfterDraw)
                                {
                                    rem.Invoke();
                                }
                                removesAfterDraw.Clear();


                                position.y += listHeight + EditorGUIUtility.standardVerticalSpacing;

                                //Edit
                                position.height = EditorGUIUtility.singleLineHeight;
                                values.isExpanded = EditorGUI.Foldout(position, values.isExpanded,
                                                new GUIContent("Edit", "add or remove elements"));

                                if (!values.isExpanded)
                                    return;
                                position.y += position.height + EditorGUIUtility.standardVerticalSpacing;

                                using (new EditorGUI.IndentLevelScope(1))
                                {
                                    position = EditorGUI.IndentedRect(position);

                                    using (new NewIndentLevel(0))
                                    {
                                        //Edit options
                                        var editColumns = NonReorderable.GetEditColumns(property, position.width, keys.arraySize, this);
                                        DrawProperties.MultiColumnList.DrawList(position, columns: editColumns);
                                    }
                                }
                            }
                        }
                    };

                    height = (property) =>
                    {
                        SerializedProperty keys = property.FindPropertyRelative(keysListPath);
                        SerializedProperty values = property.FindPropertyRelative(valuesListPath);

                        float totalHeight = EditorGUIUtility.singleLineHeight;

                        if (!keys.isExpanded)
                            return totalHeight;

                        var dictColumns = NonReorderable.GetDictionaryColumns(property: property, keys: keys, values: values, 10, this);
                        totalHeight += EditorGUIUtility.standardVerticalSpacing
                                        + DrawProperties.MultiColumnList.GetHeight(columns: dictColumns)
                                        + EditorGUIUtility.standardVerticalSpacing
                                        + EditorGUIUtility.singleLineHeight; //list and the edit

                        if (!values.isExpanded)
                            return totalHeight;

                        var editColumns = NonReorderable.GetEditColumns(property, 10, keys.arraySize, this);
                        totalHeight += DrawProperties.MultiColumnList.GetHeight(columns: editColumns)
                                        + EditorGUIUtility.standardVerticalSpacing;

                        return totalHeight;
                    };
                }
                else if (fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(ReorderableDictionary<,>))
                {
                    string path = "keyValuePairs";
                    SerializedProperty keyValuePairs_instantiate = property.FindPropertyRelative(path);
                    if(keyValuePairs_instantiate == null)
                    {
                        errorMessage = $"Dictionary: Argument types are not serializable."
                         + $"\nMaybe add [System.Serializable] if its your custom class";
                        return;
                    }
                    gui = (position, label, prop) =>
                    {
                        SerializedProperty keyValuePairs = prop.FindPropertyRelative(path);
                        Reorderable.MarkInvalidPairs(keyValuePairs);
                        KeyValuePairDrawer.keyWidth = keyWidth;
                        DrawProperties.PropertyField(position, label, keyValuePairs);
                    };
                    height = (prop) =>
                    {
                        SerializedProperty keyValuePairs = prop.FindPropertyRelative(path);
                        return DrawProperties.GetPropertyHeight(keyValuePairs);
                    };


                }
                else
                {
                    errorMessage = "[Dictionary]-attribute is only valid on Custominspector dictionaries.";
                    return;
                }
            }
            static class Reorderable
            {
                public static void MarkInvalidPairs(SerializedProperty keyValuePairs)
                {
                    Debug.Assert(keyValuePairs.isArray, "pairs not found");
                    List<SerializedProperty> pairs = keyValuePairs.GetAllPropertys(false).ToList(); //.Select(_ => _.FindPropertyRelative("key").GetValue()).ToList();
                    List<object> keys = pairs.Select(_ => _.FindPropertyRelative("key").GetValue()).ToList();
                    for (int i = 0; i < pairs.Count; i++)
                    {
                        object key = keys[i];
                        SerializedProperty isValid = pairs[i].FindPropertyRelative("isValid");
                        if (key.IsUnityNull())
                        {
                            isValid.boolValue = false;
                            continue;
                        }
                        else
                        {
                            isValid.boolValue = keys.Take(i).All(o => !key.Equals(o));
                            continue;
                        }
                    }
                }
            }
            static class NonReorderable
            {
                public static DrawProperties.MultiColumnList.Column[] GetDictionaryColumns(SerializedProperty property, SerializedProperty keys, SerializedProperty values, float positionWidth, PropInfo info)
                {
                    if (property == null)
                        throw new ArgumentException("Cannot draw null property");
                    if (keys == null)
                        throw new NullReferenceException("No keys available to draw.");
                    if (values == null)
                        throw new NullReferenceException("No values available to draw.");

                    //create list body
                    DrawProperties.MultiColumnList.Column indices;
                    DrawProperties.MultiColumnList.Column keysColumn;
                    DrawProperties.MultiColumnList.Column valuesColumn;
                    DrawProperties.MultiColumnList.Column deleteButtons;

                    if (keys.arraySize > 0)
                    {
                        float indicesWidth = GUI.skin.label.CalcSize(new GUIContent($"Element: {keys.arraySize}")).x + 5;
                        float deleteButtonsWidth = EditorGUIUtility.singleLineHeight;

                        indices = new(false, Enumerable.Range(0, keys.arraySize)
                                             .Select(_ => $"Element: {_}")
                                             .Select(_ => new Entry(EditorGUIUtility.singleLineHeight, (position) => EditorGUI.LabelField(position, _))),
                                             fixedWidth: indicesWidth);

                        keysColumn = new(true, Enumerable.Range(0, keys.arraySize)
                                             .Select(_ => keys.GetArrayElementAtIndex(_))
                                             .Select(_ => new Entry(DrawProperties.GetPropertyHeight(_), (position) => DrawProperties.PropertyFieldWithoutLabel(position, _, true)))
                                             , fixedWidth: (positionWidth - indicesWidth - deleteButtonsWidth - DrawProperties.MultiColumnList.columnsSpace * 2) * info.keyWidth);

                        valuesColumn = new(false, Enumerable.Range(0, values.arraySize)
                                             .Select(_ => values.GetArrayElementAtIndex(_))
                                             .Select(_ => new Entry(DrawProperties.GetPropertyHeight(_), (position) => DrawProperties.PropertyFieldWithoutLabel(position, _, true)))
                                             );

                        deleteButtons = new(false, Enumerable.Range(0, keys.arraySize)
                                             .Select(_ => new Entry(EditorGUIUtility.singleLineHeight, (position) =>
                                             {
                                                 info.removesAfterDraw.Add(() =>
                                                 {
                                                     if (GUI.Button(position, new GUIContent("-", "Remove this keyValuePair")))
                                                     {
                                                         property.CallMethodInside("RemoveAt", new object[] { _ });
                                                         if (!property.ApplyModifiedField(true))
                                                         {
                                                             Debug.LogWarning("Remove could not be saved");
                                                         }
                                                     }
                                                 });
                                             })),
                                             fixedWidth: deleteButtonsWidth);
                    }
                    else
                    {
                        indices = new(false, Enumerable.Repeat(new Entry(0, (position) => { }), 1));
                        keysColumn = new(false, Enumerable.Repeat(new Entry(EditorGUIUtility.singleLineHeight, (position) => EditorGUI.LabelField(position, "(empty)")), 1));
                        valuesColumn = new(false, Enumerable.Repeat(new Entry(EditorGUIUtility.singleLineHeight, (position) => EditorGUI.LabelField(position, "(empty)")), 1));
                        deleteButtons = new(false, Enumerable.Repeat(new Entry(0, (position) => { }), 1));
                    }

                    //insert headers to columns
                    GUIContent keysHeader = new($"key: {PropertyConversions.GetIListElementType(DirtyValue.GetType(keys)).Name}");
                    GUIContent valuesHeader = new($"value: {PropertyConversions.GetIListElementType(DirtyValue.GetType(values)).Name}");

                    indices.entrys = Enumerable.Repeat(new Entry(0, (position) => { }), 1)
                                            .Concat(indices.entrys);
                    keysColumn.entrys = Enumerable.Repeat(new Entry(EditorGUIUtility.singleLineHeight, (position) => EditorGUI.LabelField(position, keysHeader)), 1)
                                            .Concat(keysColumn.entrys);
                    valuesColumn.entrys = Enumerable.Repeat(new Entry(EditorGUIUtility.singleLineHeight, (position) => EditorGUI.LabelField(position, valuesHeader)), 1)
                                            .Concat(valuesColumn.entrys);
                    deleteButtons.entrys = Enumerable.Repeat(new Entry(0, (position) => { }), 1)
                                            .Concat(deleteButtons.entrys);

                    return new DrawProperties.MultiColumnList.Column[] { indices, keysColumn, valuesColumn, deleteButtons };
                }
                public static DrawProperties.MultiColumnList.Column[] GetEditColumns(SerializedProperty property, float positionWidth, int keysArraySize, PropInfo info)
                {
                    SerializedProperty editor_keyInput = property.FindPropertyRelative("editor_keyInput");
                    SerializedProperty editor_valueInput = property.FindPropertyRelative("editor_valueInput");

                    float buttonsWidth = 80;
                    DrawProperties.MultiColumnList.Column buttons = new(false, new List<Entry>()
                    {
                        new Entry(EditorGUIUtility.singleLineHeight, (position) =>
                        {
                            if (GUI.Button(position, new GUIContent("TryAdd", "Adds given key/value pair to the dictionary")))
                            {
                                property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                                bool wasAdded = (bool)property.CallMethodInside("TryAdd", new object[] { editor_keyInput.GetValue(), editor_valueInput.GetValue() });
                                if (wasAdded)
                                {
                                    if (!property.ApplyModifiedField(true))
                                    {
                                        Debug.LogWarning("TryAdd could not be saved");
                                    }
                                    EditorGUIUtility.keyboardControl = 0;
                                }
                                else Debug.Log("Dictionary already contains key");
                            }
                        }),
                        new Entry(EditorGUIUtility.singleLineHeight, (position) =>
                        {
                            if (GUI.Button(position, new GUIContent("Remove", "Removes entry with given key from the dictionary")))
                            {
                                property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                                bool wasRemoved = (bool)property.CallMethodInside("Remove", new object[] { editor_keyInput.GetValue() });
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
                                    property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                                    property.CallMethodInside("Clear", new object[] { });
                                    if (!property.ApplyModifiedField(true))
                                    {
                                        Debug.LogWarning("Clear could not be saved");
                                    }
                                }
                            }
                        }),
                    }, buttonsWidth);

                    DrawProperties.MultiColumnList.Column buttonKeys
                        = new(false, Enumerable.Repeat(new Entry(DrawProperties.GetPropertyHeight(editor_keyInput),
                              (position) => DrawProperties.PropertyFieldWithoutLabel(position, editor_keyInput)), 2)
                              .Concat(Enumerable.Repeat(new Entry(0, (position) => { }), 1))
                              , fixedWidth: (positionWidth - buttonsWidth - DrawProperties.MultiColumnList.columnsSpace * 2) * info.keyWidth);

                    DrawProperties.MultiColumnList.Column buttonValues
                        = new(false, Enumerable.Repeat(new Entry(DrawProperties.GetPropertyHeight(editor_valueInput),
                              (position) => DrawProperties.PropertyFieldWithoutLabel(position, editor_valueInput)), 1)
                              .Concat(Enumerable.Repeat(new Entry(0, (position) => { }), 2)));

                    return new DrawProperties.MultiColumnList.Column[] { buttons, buttonKeys, buttonValues };
                }
            }
        }
    }

    [CustomPropertyDrawer(typeof(ReorderableDictionary<,>.SerializableKeyValuePair))]
    public class KeyValuePairDrawer : PropertyDrawer
    {
        public static float keyWidth = .4f;
        const float dividerWidth = 10;

        static GUIContent notValidIcon;
        static GUIStyle topRight = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.UpperRight };
        GUIContent GetNotValidIcon()
        {
            if (notValidIcon == null)
            {
                notValidIcon = EditorGUIUtility.IconContent(InspectorIcon.Error.ToInternalIconName());
                notValidIcon.tooltip = "This key is a duplicate and wont be added to the dictinoary!";
            }
            return notValidIcon;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty key = property.FindPropertyRelative("key");
            SerializedProperty value = property.FindPropertyRelative("value");
            SerializedProperty isValid = property.FindPropertyRelative("isValid");

            Rect ind = EditorGUI.IndentedRect(position);
            using (new NewIndentLevel(0))
            {
                Color prev = GUI.color;
                if (!isValid.boolValue) GUI.color = new Color(1f, .6f, .6f);

                Rect r = new(ind)
                {
                    width = ind.width * keyWidth - dividerWidth / 2f,
                };

                using (new LabelWidthScope(r.width * keyWidth))
                {
                    DrawProperties.PropertyFieldWithoutLabel(r, key);
                }

                if (!isValid.boolValue)
                {
                    //Draw warning
                    EditorGUI.LabelField(r, GetNotValidIcon(), topRight);
                }

                r.x += r.width + dividerWidth;
                r.width = ind.width - (r.width + dividerWidth);

                using (new LabelWidthScope(r.width * keyWidth))
                {
                    DrawProperties.PropertyFieldWithoutLabel(r, value);
                }
                GUI.color = prev;
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty key = property.FindPropertyRelative("key");
            SerializedProperty value = property.FindPropertyRelative("value");

            return Mathf.Max(DrawProperties.GetPropertyHeight(label, key), DrawProperties.GetPropertyHeight(label, value));
        }
    }
}