using CustomInspector.Extensions;
using CustomInspector.Helpers;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace CustomInspector.Editor
{
    [CustomPropertyDrawer(typeof(HookAttribute))]
    public class HookAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            PropInfo info = GetInfo(property);

            if (info.errorMessage != null)
            {
                DrawProperties.DrawPropertyWithMessage(position, label, property, info.errorMessage, MessageType.Error);
                return;
            }

            
            object oldValue = property.GetValue();

            EditorGUI.BeginChangeCheck();
            DrawProperties.PropertyField(position, label, property);
            if (EditorGUI.EndChangeCheck())
            {
                object newValue = property.GetValue();

                HookAttribute a = (HookAttribute)attribute;
                if (a.useHookOnly)
                {
                    //Revert change on property
                    property.SetValue(oldValue);
                }
                //property to instantiation
                property.serializedObject.ApplyModifiedProperties();
                //method on instantiation
                info.hookMethod(property, oldValue, newValue);
                //instantiation to property
                property.serializedObject.ApplyModifiedFields(true);
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            PropInfo info = GetInfo(property);

            if(info.errorMessage != null)
                return DrawProperties.GetPropertyWithMessageHeight(label, property);
            else
                return DrawProperties.GetPropertyHeight(label, property);
        }

        static readonly Dictionary<PropertyIdentifier, PropInfo> savedInfos = new();
        PropInfo GetInfo(SerializedProperty property)
        {
            PropertyIdentifier id = new(property);
            if(!savedInfos.TryGetValue(id, out PropInfo info))
            {
                info = new(property, fieldInfo.FieldType, (HookAttribute)attribute);
                savedInfos.Add(id, info);
            }
            return info;
        }
        class PropInfo
        {
            public readonly string errorMessage;
            public readonly bool methodHasParameters;
            /// <summary> A method that executes on with property, oldValue & newValue </summary>
            public readonly Action<SerializedProperty, object, object> hookMethod;

            public PropInfo(SerializedProperty property, Type propertyType, HookAttribute attribute)
            {
                DirtyValue owner = DirtyValue.GetOwner(property);

                InvokableMethod method;
                try
                {
                    try
                    {
                        method = property.GetMethodOnOwner(attribute.methodPath);
                        methodHasParameters = false;
                        errorMessage = null;
                    }
                    catch
                    {
                        method = property.GetMethodOnOwner(attribute.methodPath, new Type[] { propertyType, propertyType });
                        methodHasParameters = true;
                        errorMessage = null;
                    }
                }
                catch (MissingMethodException e)
                {
                    errorMessage = e.Message + " or without parameters";
                    return;
                }
                catch (Exception e)
                {
                    errorMessage = e.Message;
                    return;
                }

                if (!methodHasParameters && attribute.useHookOnly)
                {
                    errorMessage = $"HookAttribute: New inputs are not applied, because you set 'useHookOnly', " +
                            $"but your method on '{attribute.methodPath}' did not define the parameters {propertyType} oldValue, {propertyType} newValue";
                    return;
                }

                Func<bool> ifExecute = attribute.target switch
                {
                    ExecutionTarget.Always => () => true,
                    ExecutionTarget.IsPlaying => () => Application.isPlaying,
                    ExecutionTarget.IsNotPlaying => () => !Application.isPlaying,
                    _ => throw new NotImplementedException(attribute.target.ToString()),
                };


                if (methodHasParameters)
                {
                    hookMethod = (p, o, n) =>
                    {
                        if(ifExecute())
                            p.GetMethodOnOwner(attribute.methodPath, new Type[] { propertyType, propertyType }).Invoke(o, n);
                    };
                }
                else
                {
                    hookMethod = (p, o, n) =>
                    {
                        if (ifExecute())
                            p.GetMethodOnOwner(attribute.methodPath).Invoke();
                    };
                }
            }
        }
    }
}
