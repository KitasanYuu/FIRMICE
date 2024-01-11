using CustomInspector.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CustomInspector.Editor
{
    [CustomPropertyDrawer(typeof(SerializableDateTimeAttribute))]
    public class SerializableDateTimeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializableDateTimeAttribute attr = (SerializableDateTimeAttribute)attribute;

            //draw default
            if (attr.format == SerializableDateTime.InspectorFormat.Default
                || attr.format == SerializableDateTime.InspectorFormat.AddTextInput)
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
            if (attr.format == SerializableDateTime.InspectorFormat.TextInput)
            {
                EditorGUI.LabelField(position, label);
            }

            //Text input
            if (attr.format == SerializableDateTime.InspectorFormat.TextInput
                || attr.format == SerializableDateTime.InspectorFormat.AddTextInput)
            {
                Rect fRect = EditorGUI.IndentedRect(position);
                using (new NewIndentLevel(0))
                {
                    fRect.x += EditorGUIUtility.labelWidth;
                    fRect.width -= EditorGUIUtility.labelWidth;
                    fRect.height = EditorGUIUtility.singleLineHeight;

                    DateTime dt = new MyDateTime(property);

                    EditorGUI.BeginChangeCheck();
                    string res = EditorGUI.DelayedTextField(fRect, GUIContent.none, dt.ToString());
                    if (EditorGUI.EndChangeCheck())
                    {
                        MyDateTime @new = new MyDateTime(DateTime.Parse(res));
                        @new.ApplyOnProp(property);
                    }
                }
            }

            //DateEnums
            if (attr.format == SerializableDateTime.InspectorFormat.DateEnums)
            {
                Rect space = EditorGUI.IndentedRect(position);
                using (new NewIndentLevel(0))
                {
                    //Label
                    Rect labelRect = new(space);
                    labelRect.width = EditorGUIUtility.labelWidth;
                    EditorGUI.LabelField(labelRect, label);
                    //Enums
                    float enumsWidth = space.width - labelRect.width;
                    Rect enumRect = new(space);
                    enumRect.width -= labelRect.width;
                    enumRect.width = (enumRect.width - 2 * EditorGUIUtility.standardVerticalSpacing) / 3f;
                    if(enumRect.width < 40)
                        enumRect.width = 40;
                    
                    enumRect.x += labelRect.width + 2 * (enumRect.width + EditorGUIUtility.standardVerticalSpacing); //go to end (because year first)

                    //year
                    EditorGUI.BeginChangeCheck();
                    SerializedProperty yearProp = property.FindPropertyRelative("year");
                    yearProp.intValue = Math.Clamp(EditorGUI.IntField(enumRect, GUIContent.none, yearProp.intValue), 1, 9999);

                    enumRect.x -= enumRect.width + EditorGUIUtility.standardVerticalSpacing;

                    //Month
                    SerializedProperty monthProp = property.FindPropertyRelative("month");
                    monthProp.intValue = 1 + EditorGUI.Popup(enumRect, monthProp.intValue - 1, System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.MonthNames
                                                  .Select(x => new GUIContent(x)).ToArray());

                    enumRect.x -= enumRect.width + EditorGUIUtility.standardVerticalSpacing;

                    //Day
                    SerializedProperty dayProp = property.FindPropertyRelative("day");
                    if (EditorGUI.EndChangeCheck())
                    {
                        dayProp.intValue = Mathf.Clamp(dayProp.intValue, 1, DateTime.DaysInMonth(yearProp.intValue, monthProp.intValue));
                    }
                    int daysPossible = DateTime.DaysInMonth(yearProp.intValue, monthProp.intValue);
                    dayProp.intValue = EditorGUI.Popup(enumRect, dayProp.intValue, Enumerable.Range(1, daysPossible + 1)
                                                  .Select(x => new GUIContent(x.ToString())).ToArray());
                }
                property.serializedObject.ApplyModifiedProperties();
            }
        }
        readonly struct MyDateTime
        {
            public readonly int year;
            public readonly int month;
            public readonly int day;
            public readonly int hour;
            public readonly int minute;
            public readonly int second;
            public readonly DateTimeKind kind;

            public MyDateTime(SerializedProperty property)
            {
                year = property.FindPropertyRelative("year").intValue;
                month = property.FindPropertyRelative("month").intValue;
                day = property.FindPropertyRelative("day").intValue;
                hour = property.FindPropertyRelative("hour").intValue;
                minute = property.FindPropertyRelative("minute").intValue;
                second = property.FindPropertyRelative("second").intValue;
                kind = (DateTimeKind)property.FindPropertyRelative("kind").enumValueIndex;
            }
            public MyDateTime(DateTime dateTime)
            {
                year = dateTime.Year;
                month = dateTime.Month;
                day = dateTime.Day;
                hour = dateTime.Hour;
                minute = dateTime.Minute;
                second = dateTime.Second;
                kind = dateTime.Kind;
            }
            public void ApplyOnProp(SerializedProperty property)
            {
                SerializedProperty yearP = property.FindPropertyRelative("year");
                yearP.intValue = year;
                SerializedProperty monthP = property.FindPropertyRelative("month");
                monthP.intValue = month;
                SerializedProperty dayP = property.FindPropertyRelative("day");
                dayP.intValue = day;
                SerializedProperty hourP = property.FindPropertyRelative("hour");
                hourP.intValue = hour;
                SerializedProperty minuteP = property.FindPropertyRelative("minute");
                minuteP.intValue = minute;
                SerializedProperty secondP = property.FindPropertyRelative("second");
                secondP.intValue = second;
                SerializedProperty kindP = property.FindPropertyRelative("kind");
                kindP.enumValueIndex = (int)kind;
            }
            public static implicit operator DateTime(MyDateTime dateTime)
                => new DateTime(dateTime.year, dateTime.month, dateTime.day, dateTime.hour, dateTime.minute, dateTime.second, dateTime.kind);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializableDateTimeAttribute attr = (SerializableDateTimeAttribute)attribute;
            return attr.format switch
            {
                SerializableDateTime.InspectorFormat.Default
                or SerializableDateTime.InspectorFormat.AddTextInput => EditorGUI.GetPropertyHeight(property, label),

                SerializableDateTime.InspectorFormat.TextInput
                or SerializableDateTime.InspectorFormat.DateEnums    => EditorGUIUtility.singleLineHeight,
                _ => throw new NotImplementedException(attr.format.ToString()),
            };
        }
    }
}
