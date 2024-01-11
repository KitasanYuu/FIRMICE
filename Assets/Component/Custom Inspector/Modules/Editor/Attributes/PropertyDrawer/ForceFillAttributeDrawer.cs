using CustomInspector.Extensions;
using CustomInspector.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;


namespace CustomInspector.Editor
{
    [CustomPropertyDrawer(typeof(ForceFillAttribute))]
    public class ForceFillAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ForceFillAttribute ffa = (ForceFillAttribute)attribute;
            var info = GetInfo(property, ffa);

            //general errors
            if (info.errorMessage != null)
            {
                DrawProperties.DrawPropertyWithMessage(position, label, property, info.errorMessage, MessageType.Error);
                return;
            }

            //If we should even test
            if (ffa.onlyTestInPlayMode && !Application.isPlaying)
            {
                DrawProperties.PropertyField(position, label, property);
                return;
            }

            //if filled
            object value = property.GetValue();
            if (info.invalids.Contains(value))
            {
                string errorMessage = $"ForceFill: Value of '{ToString(value)}' on '{property.name}' is not valid.";
                if(info.invalids.Length > 1)
                    errorMessage += $"\nForbidden Values are: { string.Join(", ", info.invalids.Select(_ => ToString(_)))}";

                DrawProperties.DrawPropertyWithMessage(position, label, property,
                            errorMessage, MessageType.Error);

                static string ToString(object o)
                {
                    if (o == null)
                        return "null";
                    string res = o.ToString();
                    if (res == "")
                        return "empty";
                    return res;
                }
                return;
            }
            else
            {
                DrawProperties.PropertyField(position, label, property);
                return;
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ForceFillAttribute ffa = (ForceFillAttribute)attribute;
            var info = GetInfo(property, ffa);

            //general errors
            if(info.errorMessage != null)
            {
                return DrawProperties.GetPropertyWithMessageHeight(label, property);
            }

            //If even test
            if(ffa.onlyTestInPlayMode && !Application.isPlaying)
                return DrawProperties.GetPropertyHeight(label, property);

            //if filled
            object value = property.GetValue();
            if (info.invalids.Contains(value))
                return DrawProperties.GetPropertyWithMessageHeight(label, property);
            else
                return DrawProperties.GetPropertyHeight(label, property);

        }

        static readonly Dictionary<PropertyIdentifier, PropInfo> savedInfo = new();
        PropInfo GetInfo(SerializedProperty property, ForceFillAttribute attribute)
        {
            PropertyIdentifier id = new(property);
            if(!savedInfo.TryGetValue(id, out PropInfo info))
            {
                info = new(property, fieldInfo, attribute);
                savedInfo.Add(id, info);
            }
            return info;
        }
        class PropInfo
        {
            public readonly string errorMessage;
            public readonly object[] invalids;

            public PropInfo(SerializedProperty property, FieldInfo fieldInfo, ForceFillAttribute attribute)
            {
                //if no given invalids, we take default value
                if ((attribute.notAllowed?.Length ?? 0) < 1)
                {
                    Type propSystemType = property.propertyType.ToSystemType();
                    if (propSystemType == null)
                    {
                        errorMessage = "Property is null";
                        this.invalids = new object[] { null };
                    }
                    else if (propSystemType == typeof(string)) //unity auto converts null-string to empty-string
                        this.invalids = new object[] { "" };
                    else if (propSystemType == typeof(char)) //Activator.CreateInstance invalid on char
                        this.invalids = new object[] { '\0' };
                    else if (propSystemType == typeof(Enum)) //Activator.CreateInstance invalid on enums
                        this.invalids = new object[] { Enum.ToObject(fieldInfo.FieldType, 0) };
                    else if (propSystemType.IsValueType)
                    {
                        try
                        {
                            this.invalids = new object[] { Activator.CreateInstance(propSystemType) };
                        }
                        catch
                        {
                            this.invalids = new object[] { null };
                            Debug.LogWarning($"{nameof(ForceFillAttribute)}: needs a paramter (invalid value) for value type {propSystemType} because it has no default constructor");
                        }
                    }
                    else
                        this.invalids = new object[] { null };
                    errorMessage = null;
                }
                else
                {
                    List<object> invalids = new();
                    //add given invalids
                    foreach (var item in attribute.notAllowed)
                    {
                        if(property.propertyType == SerializedPropertyType.String //unity auto converts null-string to empty-string
                            && item == null)
                        {
                            invalids.Add("");
                            continue;
                        }

                        try
                        {
                            invalids.Add(property.ParseString(item));
                        }
                        catch
                        {
                            errorMessage = $"ForceFill: Failed to parse \"{item}\" as \"{property.propertyType}\"";
                        }
                    }
                    this.invalids = invalids.ToArray();
                    errorMessage = null;
                }
            }
        }
    }
}