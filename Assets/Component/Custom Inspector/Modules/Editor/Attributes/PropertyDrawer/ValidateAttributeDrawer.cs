using UnityEditor;
using UnityEngine;
using CustomInspector.Extensions;
using System;
using System.Collections.Generic;

namespace CustomInspector.Editor
{
    [CustomPropertyDrawer(typeof(ValidateAttribute))]
    public class ValidateAttributeDrawer : PropertyDrawer
    {
        Dictionary<SerializedObject, Dictionary<string, SavedValue>> cache = new(); //Dict<propertypath, Tuple<savedValue, isAllowed>>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (IsValid(property, out string message))
                DrawProperties.PropertyField(position, label, property);
            else
                DrawProperties.DrawPropertyWithMessage(position, label, property, $"{property.name}: " + message,
                                MessageBoxConvert.ToUnityMessageType(((ValidateAttribute)attribute).errorType));
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (IsValid(property, out _))
                return DrawProperties.GetPropertyHeight(label, property);
            else
                return DrawProperties.GetPropertyWithMessageHeight(label, property);
        }
        bool IsValid(SerializedProperty property, out string message)
        {
            Dictionary<string, SavedValue> values;
            if (!cache.TryGetValue(property.serializedObject, out values))
            {
                values = new();
                cache.Add(property.serializedObject, values);
            }

            if (values.TryGetValue(property.propertyPath, out SavedValue saved))
            {
                if (saved.value != null && saved.value.Equals(property.GetValue())
                    || property.GetValue() == null)
                {
                    message = saved.message;
                    return saved.isAllowed;
                }
                else
                {
                    property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    bool isValid = CalculateValidness(property, out message);
                    property.serializedObject.ApplyModifiedFields(false); //this function is not made for changes, but why not preventing wierd behaviour

                    saved.isAllowed = isValid;
                    saved.message = message;
                    saved.value = property.GetValue();
                    return isValid;
                }
            }
            else
            {
                property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                bool isValid = CalculateValidness(property, out message);
                property.serializedObject.ApplyModifiedFields(false); //this function is not made for changes, but why not preventing wierd behaviour

                values.Add(property.propertyPath,
                    new SavedValue(property.GetValue(), isValid, message));
                return isValid;
            }


            bool CalculateValidness(SerializedProperty property, out string errorMessage)
            {
                ValidateAttribute va = (ValidateAttribute)attribute;

                string methodPath = va.methodPath;

                Type fieldType = fieldInfo.FieldType.IsArray ? fieldInfo.FieldType.GetElementType()
                                : fieldInfo.FieldType;
                Type[] pTypes = new Type[] { fieldType };
                InvokableMethod method;

                bool hasParams;
                try
                {
                    method = property.GetMethodOnOwner(methodPath);
                    hasParams = false;
                }
                catch (MissingMethodException)
                {
                    try
                    {
                        method = property.GetMethodOnOwner(methodPath, pTypes);
                        hasParams = true;
                    }
                    catch (MissingMethodException e)
                    {
                        errorMessage = e.Message + " or without parameters";
                        return false;
                    }
                    catch (Exception e)
                    {
                        errorMessage = e.Message;
                        return false;
                    }
                }
                catch (Exception e)
                {
                    errorMessage = e.Message;
                    return false;
                }


                if (method.ReturnType == typeof(bool))
                {
                    var @params = hasParams ? new object[] { property.GetValue() } : new object[0];
                    bool res;
                    try
                    {
                        res = (bool)method.Invoke(@params);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        errorMessage = "error in validate-method. See console for more information";
                        return false;
                    }

                    if (res)
                    {
                        errorMessage = null;
                        return true;
                    }
                    else
                    {
                        errorMessage = va.errorMessage;
                        return false;
                    }

                }
                else
                {
                    errorMessage = $"{methodPath}'s return type is not typeof bool";
                    return false;
                }
            }
        }
        class SavedValue
        {
            public object value;
            public bool isAllowed;
            public string message;
            public SavedValue(object value, bool isAllowed, string message)
            {
                this.value = value;
                this.isAllowed = isAllowed;
                this.message = message;
            }
        }
    }
}