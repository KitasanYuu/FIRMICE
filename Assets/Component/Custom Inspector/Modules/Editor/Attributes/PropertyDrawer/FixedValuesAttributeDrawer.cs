using CustomInspector.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CustomInspector.Editor
{
    [CustomPropertyDrawer(typeof(FixedValuesAttribute))]
    public class FixedValuesAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            FixedValuesAttribute f = (FixedValuesAttribute)attribute;
            if(f.values == null || f.values.Length <= 0)
            {
                DrawProperties.DrawPropertyWithMessage(position, label, property, errorMessage: "FixedValuesAttribute: No values given to choose from", MessageType.Error);
                return;
            }

            var values = f.values.ToList();

            if(values.Any(_ => _.GetType() != fieldInfo.FieldType))
            {
                DrawProperties.DrawPropertyWithMessage(position, label, property, errorMessage: $"FixedValuesAttribute: Not all given values are type of {fieldInfo.FieldType.Name}", MessageType.Error);
                return;
            }

            int index = values.IndexOf(property.GetValue());
            if (index == -1)
            {
                property.SetValue(values[0]);
                index = 0;
            }

            EditorGUI.BeginChangeCheck();
            index = EditorGUI.Popup(position, label, index, values.Select(_ => new GUIContent(_.ToString())).ToArray());
            if(EditorGUI.EndChangeCheck())
            {
                property.SetValue(values[index]);
                property.serializedObject.ApplyModifiedProperties();
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            FixedValuesAttribute f = (FixedValuesAttribute)attribute;
            var values = f.values?.ToList();
            if (values == null || values.Count <= 0
                || values.Any(_ => _.GetType() != fieldInfo.FieldType))
            {
                return DrawProperties.GetPropertyWithMessageHeight(label, property);
            }

            return EditorGUIUtility.singleLineHeight;
        }
    }
}
