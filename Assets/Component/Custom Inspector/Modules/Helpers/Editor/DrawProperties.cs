using CustomInspector.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CustomInspector.Extensions
{
    public static class DrawProperties
    {
        public static void PropertyField(Rect position, GUIContent label, SerializedProperty property, bool includeChildren = true)
        {
            //Check if GUIContent broke
            Debug.Assert(property != null);
            label = PropertyValues.RepairLabel(label, property);
            EditorGUI.PropertyField(position: position, label: label, property: property, includeChildren: includeChildren);
        }
        /// <summary>
        /// No label, but generics get theirself toString() in the foldout
        /// </summary>
        public static void PropertyFieldWithoutLabel(Rect position, SerializedProperty property, bool includeChildren = true)
        {
            if (property.propertyType != SerializedPropertyType.Generic)
            {
                PropertyField(position: position, label: GUIContent.none, property: property, includeChildren: includeChildren);
            }
            else
            {
                if (includeChildren)
                {
                    PropertyField(position: position, label: new GUIContent(property.GetValue().ToString(), property.tooltip), property: property, includeChildren: includeChildren);
                }
                else
                {
                    property.isExpanded = false;
                    EditorGUI.LabelField(position: position, label: new GUIContent(property.GetValue().ToString(), property.tooltip));
                }
            }
        }
        public static void PropertyFieldWithFoldout(Rect position, GUIContent label, SerializedProperty property, bool includeChildren = true)
        {
            label = PropertyValues.RepairLabel(label, property);
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
            EditorGUI.PropertyField(position, property, new GUIContent(" "), includeChildren);
        }
        public static float GetPropertyHeight(GUIContent label, SerializedProperty property, bool includeChildren = true)
            => EditorGUI.GetPropertyHeight(property, label, includeChildren);
        public static float GetPropertyHeight(SerializedProperty property, bool includeChildren = true)
            => EditorGUI.GetPropertyHeight(property, includeChildren);
        public static float GetPropertyHeight(SerializedPropertyType type, GUIContent label)
        {
            //!widemode is broken on EditorGUI.GetPropertyHeight
            if (EditorGUIUtility.wideMode)
                return EditorGUI.GetPropertyHeight(type, label);
            else
            {
                return type switch
                {
                    SerializedPropertyType.Vector2Int or
                    SerializedPropertyType.Vector2 or
                    SerializedPropertyType.Vector3Int or
                    SerializedPropertyType.Vector3 or
                    SerializedPropertyType.Vector4 => 2 * EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing,

                    _ => EditorGUI.GetPropertyHeight(type, label),
                };
            }
        }

        /// <summary>
        /// Generic show label in the inspector and returns user input
        /// </summary>
        public static System.Object DrawField(Rect position, object value, Type fieldType, bool disabled = false)
            => DrawField(position, GUIContent.none, value, fieldType, disabled);
        public static System.Object DrawField<T>(Rect position, GUIContent label, T value, Type fieldType, bool disabled = false)
        {
            UnityEngine.Debug.Assert(value is not SerializedProperty, "Your drawing the serialization instead of the actual object. Use DrawProperties.PropertyField instead");


            if (value is null)
            {
                return EditorGUI.ObjectField(position, label, obj: null, objType: fieldType, true);
            }

            //we have to top align the label for wide-mode=true
            Rect labelRect = new(position)
            {
                height = EditorGUIUtility.singleLineHeight,
            };
            EditorGUI.LabelField(labelRect, label);

            GUIContent g = new(" ");
            using (new EditorGUI.DisabledScope(disabled))
            {
                switch (value)
                {
                    case int i:
                        return EditorGUI.IntField(position, g, i);
                    case bool b:
                        return EditorGUI.Toggle(position, g, b);
                    case float f:
                        return EditorGUI.FloatField(position, g, f);
                    case string s:
                        return EditorGUI.TextField(position, g, s);
                    case Color c:
                        return EditorGUI.ColorField(position, g, c);
                    case Enum e:
                        return EditorGUI.EnumFlagsField(position, g, e);
                    case Vector2Int v2i:
                        return EditorGUI.Vector2IntField(position, g, v2i);
                    case Vector2 v2:
                        return EditorGUI.Vector2Field(position, g, v2);
                    case Vector3Int v3i:
                        return EditorGUI.Vector3IntField(position, g, v3i);
                    case Vector3 v3:
                        return EditorGUI.Vector3Field(position, g, v3);
                    case Vector4 v4:
                        return EditorGUI.Vector4Field(position, g, v4);
                    case RectInt ri:
                        return EditorGUI.RectIntField(position, g, ri);
                    case Rect r:
                        return EditorGUI.RectField(position, g, r);
                    case AnimationCurve ac:
                        return EditorGUI.CurveField(position, g, ac);
                    case BoundsInt bi:
                        return EditorGUI.BoundsIntField(position, g, bi);
                    case Bounds b:
                        return EditorGUI.BoundsField(position, g, b);
                    case Quaternion q:
                        return EditorGUI.Vector4Field(position, g, new Vector4(q.x, q.y, q.z, q.w)).ToQuaternion();
                    case Object o:
                        return EditorGUI.ObjectField(position, label: g, obj: o, objType: fieldType, true);
                    default:
                        EditorGUI.LabelField(position, g, new GUIContent(value.ToString(), label.tooltip));
                        return value;
                };
            }
        }
        public static void DrawLabelSettings(Rect position, SerializedProperty property, GUIContent label, InternalLabelStyle style)
        {
            switch (style)
            {
                case InternalLabelStyle.NoLabel:
                    DrawProperties.PropertyFieldWithoutLabel(position, property, true);
                    break;

                case InternalLabelStyle.EmptyLabel:
                    DrawProperties.PropertyField(position, new GUIContent(" "), property, true);
                    break;

                case InternalLabelStyle.NoSpacing:
                    if (property.propertyType == SerializedPropertyType.Generic)
                        DrawProperties.PropertyField(position, label, property, true);
                    else
                    {
                        float labelWidth = GUI.skin.label.CalcSize(label).x + 5; //some additional distance
                        using (new LabelWidthScope(labelWidth))
                        {
                            DrawProperties.PropertyField(position, label, property, true);
                        }
                    }
                    break;

                case InternalLabelStyle.FullSpacing:
                    DrawProperties.PropertyField(position, label, property, true);
                    break;
                default:
                    throw new NotImplementedException(style.ToString());
            }
        }
        /// <summary> Additional spacing to before the message. Bigger than errorEndSpacing so that you know where the error belongs to </summary>
        public const float errorStartSpacing = 5;
        /// <summary> spacing between message and the field it belongs too.</summary>
        public const float errorEndSpacing = 2;
        public const float errorHeight = 40;
        /// <summary> Draws the field with position, but inserts an error message before </summary>
        public static void DrawPropertyWithMessage(Rect position, GUIContent label, SerializedProperty property, string errorMessage, MessageType type, bool includeChildren = true, bool disabled = false)
        {
            position.y += errorStartSpacing;
            Rect rect = EditorGUI.IndentedRect(position);
            rect.height = errorHeight;
            using (new NewIndentLevel(0))
            {
                EditorGUI.HelpBox(rect, errorMessage, type);
            }

            position.y += rect.height + errorEndSpacing;
            position.height = DrawProperties.GetPropertyHeight(label, property);

            if (disabled)
                DrawProperties.DisabledPropertyField(position, label, property, includeChildren);
            else
                DrawProperties.PropertyField(position, label: label, property: property, includeChildren: includeChildren);
        }
        public static float GetPropertyWithMessageHeight(GUIContent label, SerializedProperty property, bool includeChildren = true)
        {
            return DrawProperties.errorHeight + DrawProperties.errorStartSpacing
                   + GetPropertyHeight(label, property, includeChildren) + errorEndSpacing;
        }
        /// <summary> Disables the field, but the label stays white </summary>
        public static void DisabledPropertyField(Rect position, GUIContent label, SerializedProperty property, bool includeChildren = true)
        {
            Rect labelRect = new(position)
            {
                width = EditorGUIUtility.labelWidth,
                height = EditorGUIUtility.singleLineHeight,
            };
            label = PropertyValues.RepairLabel(label, property); //bug fix
            EditorGUI.LabelField(labelRect, label);
            using (new EditorGUI.DisabledScope(true))
                DrawProperties.PropertyField(position: position, label: new GUIContent(" "), property: property, includeChildren: includeChildren);
        }
        /// <summary>
        /// Draws a helpbox with specific height
        /// </summary>
        /// <param name="position"></param>
        /// <param name="errorMessage"></param>
        /// <param name="type"></param>
        public static void DrawMessageField(Rect position, string errorMessage, MessageType type)
        {
            Rect rect = EditorGUI.IndentedRect(position);
            using (new NewIndentLevel(0))
            {
                EditorGUI.HelpBox(rect, errorMessage, type);
            }
        }

        /// <summary> Draws a border around given rect </summary>
        /// <param name="extendOutside">If the thickness makes the border bigger(true) or the space in the middle smaller</param>
        public static void DrawBorder(Rect position, bool extendOutside, float thickness = 1) => DrawBorder(position, new Color(0.5f, 0.5f, 0.5f, 1f), extendOutside, thickness);
        /// <summary> Draws a border around given rect </summary>
        public static void DrawBorder(Rect position, Color color, bool extendOutside, float thickness = 1)
        {
            if (extendOutside)
            {
                //Up down
                Rect up = new(position.x - thickness, position.y - thickness,
                                    position.width + 2 * thickness, thickness);
                EditorGUI.DrawRect(up, color);
                up.y += position.height + thickness;
                EditorGUI.DrawRect(up, color);

                //right left
                Rect left = new(position.x - thickness, position.y - thickness,
                                    thickness, position.height + 2 * thickness);
                EditorGUI.DrawRect(left, color);
                left.x += position.width + thickness;
                EditorGUI.DrawRect(left, color);
            }
            else
            {
                //Up down
                Rect up = new(position.x, position.y,
                                    position.width, thickness);
                EditorGUI.DrawRect(up, color);
                up.y += position.height - thickness;
                EditorGUI.DrawRect(up, color);

                //right left
                Rect left = new(position.x, position.y,
                                    thickness, position.height);
                EditorGUI.DrawRect(left, color);
                left.x += position.width - thickness;
                EditorGUI.DrawRect(left, color);
            }
        }

        public static class MultiColumnList
        {
            /// <summary> label percentage of labelWidth+fieldWidth </summary>
            public const float fieldRatio = .35f;
            /// <summary> horizontal distance between to columns </summary>
            public const float columnsSpace = 10;
            /// <returns>List height</returns>
            public static float DrawList(Rect position, params Column[] columns)
            {
                position = EditorGUI.IndentedRect(position);

                float usedWidth = 0;
                int columnsWidthDynamicWidth = columns.Length;
                foreach (Column c in columns)
                {
                    if(c.fixedWidth.HasValue)
                    {
                        usedWidth += c.fixedWidth.Value;
                        columnsWidthDynamicWidth--;
                    }
                }
                if (columnsWidthDynamicWidth <= 0)
                    throw new ArgumentException("There has to be minimum 1 column with dynamic with (fixedWidth not set)"); // for design and to not divide by zero

                float dynamicWidth = Mathf.Max(0, (position.width - usedWidth - (columns.Length - 1) * columnsSpace) / columnsWidthDynamicWidth);
                Rect part = new(position);

                float listStartHeight = part.y;

                using (new NewIndentLevel(0))
                {
                    IEnumerator<Column.Entry>[] entries = columns.Select(_ => _.entrys.GetEnumerator()).ToArray();
                    int entriesAmount = columns[0].entrys.Count();

                    for (int row = 0; row < entriesAmount; row++)
                    {
                        float maxHeight = 0;
                        part.x = position.x;
                        for (int column = 0; column < entries.Length; column++)
                        {
                            //Get Entry
                            Column c = columns[column];
                            if (!entries[column].MoveNext())
                                Debug.LogError($"Columns do not have the same number of rows.\nRow missing for column {column}");
                            Column.Entry entry = entries[column].Current;
                            if (entry.height > maxHeight)
                                maxHeight = entry.height;

                            //Define Rect size
                            part.height = entry.height;
                            part.width = c.fixedWidth ?? dynamicWidth;
                            //Draw
                            using (new LabelWidthScope(part.width * fieldRatio))
                            {
                                using (new EditorGUI.DisabledScope(c.isDisabled))
                                {
                                    entry.drawer.Invoke(part);
                                }
                            }
                            //prepare rect position for next
                            part.x += part.width + columnsSpace;
                        }
                        part.y += maxHeight + EditorGUIUtility.standardVerticalSpacing;
                    }

                    //some test
                    Debug.Assert(entries.All(_ => !_.MoveNext()), "Columns do not have the same number of rows");
                }

                return part.y - listStartHeight - EditorGUIUtility.standardVerticalSpacing;
            }
            public static float GetHeight(params Column[] columns)
            {
                IEnumerator<Column.Entry>[] entries = columns.Select(_ => _.entrys.GetEnumerator()).ToArray();
                int entriesAmount = columns[0].entrys.Count();
                float totalHeight = 0;
                for (int row = 0; row < entriesAmount; row++)
                {
                    float maxHeight = 0;
                    for (int column = 0; column < entries.Length; column++)
                    {
                        //Get Entry
                        Column c = columns[column];
                        if (!entries[column].MoveNext())
                            Debug.LogError($"Columns do not have the same number of rows.\nRow missing for column {column}");
                        Column.Entry entry = entries[column].Current;
                        if (entry.height > maxHeight)
                            maxHeight = entry.height;
                    }
                    totalHeight += maxHeight + EditorGUIUtility.standardVerticalSpacing;
                }
                return totalHeight - EditorGUIUtility.standardVerticalSpacing;
            }
            public class Column
            {
                public bool isDisabled;
                public IEnumerable<Entry> entrys;
                public float? fixedWidth; //horizontal width of column

                public Column(bool isDisabled, IEnumerable<Entry> entrys, float? fixedWidth = null)
                {
                    this.isDisabled = isDisabled;
                    this.entrys = entrys;
                    this.fixedWidth = fixedWidth;
                }

                public class Entry
                {
                    public float height;
                    public Action<Rect> drawer;

                    public Entry(float height, Action<Rect> drawer)
                    {
                        this.height = height;
                        this.drawer = drawer;
                    }
                }
            }
        }
        public static class ListDrawer
        {
            public static float HeaderHeight => EditorGUIUtility.singleLineHeight;
            [System.Diagnostics.Conditional("UNITY_EDITOR")]
            public static void DrawHeader(Rect position, GUIContent label, SerializedProperty property)
            {
                TestIfList(property);

                position.height = HeaderHeight;
                label.text += $" (Count: {property.arraySize})";
                property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
            }
            /// <summary>
            /// Draws a list under a foldout
            /// </summary>
            [System.Diagnostics.Conditional("UNITY_EDITOR")]
            public static void DrawList(Rect position, GUIContent label, SerializedProperty property)
                => DrawList(false, position, label, property);
            /// <summary>
            /// Draws a list under a foldout
            /// </summary>
            [System.Diagnostics.Conditional("UNITY_EDITOR")]
            public static void DrawList(bool isReadOnly, Rect position, GUIContent label, SerializedProperty property)
            {
                TestIfList(property);

                position = EditorGUI.IndentedRect(position);

                using (new NewIndentLevel(0))
                {
                    DrawHeader(position, label, property);
                    position.y += HeaderHeight + EditorGUIUtility.standardVerticalSpacing;

                    if (property.isExpanded)
                        using (new EditorGUI.IndentLevelScope(1))
                        {
                            DrawBody(isReadOnly: isReadOnly, position: position, property: property);
                        }

                }
            }
            /// <summary>
            /// Draws the list readonly if condition true, without top label or foldout
            /// </summary>
            [System.Diagnostics.Conditional("UNITY_EDITOR")]
            public static void DrawBody(Rect position, SerializedProperty property, bool withLabels = true, bool includeChildren = true)
                => DrawBody(isReadOnly: false, position, property, withLabels, includeChildren);
            public static void DrawBody(bool isReadOnly, Rect position, SerializedProperty property, bool withLabels = true, bool includeChildren = true)
            {
                TestIfList(property);

                if (property.arraySize > 0)
                {
                    if (withLabels)
                    {
                        for (int i = 0; i < property.arraySize; i++)
                        {
                            SerializedProperty prop = property.GetArrayElementAtIndex(i);
                            GUIContent label = new($"Element: {i}");
                            position.height = DrawProperties.GetPropertyHeight(label, prop, includeChildren);
                            EditorGUI.LabelField(position, label);

                            if (prop.propertyType != SerializedPropertyType.Generic)
                            {
                                using (new EditorGUI.DisabledScope(isReadOnly))
                                {
                                    DrawProperties.PropertyField(position, new GUIContent(" "), prop, includeChildren);
                                }
                            }
                            else
                                EditorGUI.LabelField(position, " ", prop.GetValue()?.ToString() ?? "(null)");

                            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                        }
                    }
                    else // no labels
                    {
                        using (new EditorGUI.DisabledScope(isReadOnly))
                        {
                            for (int i = 0; i < property.arraySize; i++)
                            {
                                SerializedProperty prop = property.GetArrayElementAtIndex(i);
                                position.height = DrawProperties.GetPropertyHeight(prop, includeChildren);
                                if (prop.propertyType != SerializedPropertyType.Generic)
                                    DrawProperties.PropertyField(position, GUIContent.none, prop, includeChildren);
                                else
                                    EditorGUI.LabelField(position, prop.GetValue()?.ToString() ?? "(null)");
                                position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                            }
                        }
                    }
                }
                else // arraysize 0
                {
                    GUIContent content = new("(empty)");
                    //center align
                    float width = GUI.skin.label.CalcSize(content).x + 20;
                    Rect infoRect = new(position)
                    {
                        x = position.x + (position.width - width) / 2f,
                        width = width,
                        height = EditorGUIUtility.singleLineHeight,
                    };
                    EditorGUI.LabelField(infoRect, content);
                }
            }

            public static float GetBodyHeight(SerializedProperty list)
            {
                TestIfList(list);
                return Math.Max(list.arraySize, 1)
                        * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            }

            [System.Diagnostics.Conditional("UNITY_EDITOR")]
            static void TestIfList(SerializedProperty property)
            {
                if (property == null)
                    throw new NullReferenceException("SerializedProperty cannot be null");

                if (!property.isArray)
                    throw new Exceptions.WrongTypeException($"Given SerializedProperty({property.propertyPath}) is not a list/array." +
                    $"\nUse DrawList<T>(SerializedProperty property, List<T> list) instead to pass custom list");
            }
            public static class ReadOnly
            {
                /// <summary>
                /// Draws a readonly list with EditorGUILayout in the inspector
                /// </summary>
                [System.Diagnostics.Conditional("UNITY_EDITOR")]
                public static void DrawList(Rect position, GUIContent label, SerializedProperty property)
                    => ListDrawer.DrawList(isReadOnly: true, position: position, label: label, property: property);

                [System.Diagnostics.Conditional("UNITY_EDITOR")]
                public static void DrawList<T>(Rect position, SerializedProperty owner, IEnumerable<T> list, bool withLabels = true)
                {
                    if (owner == null)
                        throw new NullReferenceException("SerializedProperty cannot be null");

                    owner.isExpanded = EditorGUILayout.Foldout(owner.isExpanded, new GUIContent(owner.name, owner.tooltip));
                    if (owner.isExpanded)
                    {
                        using (new EditorGUI.IndentLevelScope(1))
                        {
                            position = EditorGUI.IndentedRect(position);
                        }
                        DrawBody(position, list, withLabels);
                    }
                }

                /// <summary>
                /// Draws a readonly list without top label or foldout
                /// </summary>
                [System.Diagnostics.Conditional("UNITY_EDITOR")]
                public static void DrawBody(Rect position, SerializedProperty property, bool withLabels = true, bool includeChildren = true)
                    => ListDrawer.DrawBody(isReadOnly: true, position: position, property: property, withLabels: withLabels, includeChildren: includeChildren);

                public static void DrawBody<T>(Rect position, IEnumerable<T> list, bool withLabels = true)
                {
                    var e = list.GetEnumerator();
                    position.height = EditorGUIUtility.singleLineHeight;
                    if (!e.MoveNext())
                    {
                        EditorGUI.LabelField(position, "(empty)");
                    }
                    else
                    {
                        if (withLabels)
                        {
                            int i = 0;
                            do
                            {
                                EditorGUI.LabelField(position, new GUIContent($"Element: {i++}"),
                                                               new GUIContent($"{e.Current}"));
                                position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                            }
                            while (e.MoveNext());
                        }
                        else
                        {
                            do
                            {
                                EditorGUI.LabelField(position, new GUIContent($"{e.Current}"));
                                position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                            }
                            while (e.MoveNext());
                        }
                    }
                }
            }
        }
    }
}