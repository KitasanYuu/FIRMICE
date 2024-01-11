using CustomInspector.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CustomInspector.Editor
{
    [CustomPropertyDrawer(typeof(TupleAttribute))]
    [CustomPropertyDrawer(typeof(SerializableTuple<,>))]
    [CustomPropertyDrawer(typeof(SerializableTuple<,,>))]
    [CustomPropertyDrawer(typeof(SerializableTuple<,,,>))]
    [CustomPropertyDrawer(typeof(SerializableTuple<,,,,>))]
    [CustomPropertyDrawer(typeof(SerializableTuple<,,,,,>))]
    public class TupleDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!string.IsNullOrEmpty(label.text))
            {
                position.height = EditorGUIUtility.singleLineHeight;
                if (label.text == "Item 1" || label.text == "Item 2") //bug fix
                    label = new(PropertyConversions.NameFormat(property.name), property.tooltip);
                EditorGUI.LabelField(position, label);
                position.y += position.height;
                EditorGUI.indentLevel++;
            }
            position = EditorGUI.IndentedRect(position);
            using (new NewIndentLevel(0))
            {
                foreach (var prop in property.GetAllVisiblePropertys(false))
                {
                    DrawProperties.PropertyFieldWithoutLabel(position, prop);
                }
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var allProps = property.GetAllVisiblePropertys(false);
            if(!string.IsNullOrEmpty(label.text))
                return EditorGUIUtility.singleLineHeight + allProps.Max(_ => DrawProperties.GetPropertyHeight(_));
            else
                return allProps.Max(_ => DrawProperties.GetPropertyHeight(_));
        }
    }
}
