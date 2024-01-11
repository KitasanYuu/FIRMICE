using System;
using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;

namespace CustomInspector.Extensions
{
    public static class EditProperties
    {
        const BindingFlags defaultBindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        //Set
        /// <summary>
        /// Sets value of Property ASSUMING value has same type as serializedProperty.propertyType
        /// </summary>
        public static void SetValue(this SerializedProperty property, object value)
        {
            if (property == null)
                throw new NullReferenceException("property is null");

            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer: property.intValue = (int)value; break;
                case SerializedPropertyType.Boolean: property.boolValue = (bool)value; break;
                case SerializedPropertyType.Float: //and double value
                    if (value is float f) property.floatValue = f;
                    else property.doubleValue = (double)value; break;
                case SerializedPropertyType.String: property.stringValue = (string)value; break;
                case SerializedPropertyType.Character: property.intValue = Convert.ToInt32((char)value); break;
                case SerializedPropertyType.Color: property.colorValue = (Color)value; break;
                case SerializedPropertyType.ExposedReference: property.exposedReferenceValue = (UnityEngine.Object)value; break;
                case SerializedPropertyType.ObjectReference: property.objectReferenceValue = (UnityEngine.Object)value; break;
                case SerializedPropertyType.Enum: property.enumValueIndex = (int)value; break;
                case SerializedPropertyType.Vector2Int: property.vector2IntValue = (Vector2Int)value; break;
                case SerializedPropertyType.Vector2: property.vector2Value = (Vector2)value; break;
                case SerializedPropertyType.Vector3Int: property.vector3IntValue = (Vector3Int)value; break;
                case SerializedPropertyType.Vector3: property.vector3Value = (Vector3)value; break;
                case SerializedPropertyType.Vector4: property.vector4Value = (Vector4)value; break;
                case SerializedPropertyType.RectInt: property.rectIntValue = (RectInt)value; break;
                case SerializedPropertyType.Rect: property.rectValue = (Rect)value; break;
                case SerializedPropertyType.LayerMask: property.intValue = (int)((LayerMask)value); break;
                case SerializedPropertyType.AnimationCurve: property.animationCurveValue = (AnimationCurve)value; break;
                case SerializedPropertyType.BoundsInt: property.boundsIntValue = (BoundsInt)value; break;
                case SerializedPropertyType.Bounds: property.boundsValue = (Bounds)value; break;
                case SerializedPropertyType.Quaternion: property.quaternionValue = (Quaternion)value; break;
                case SerializedPropertyType.Generic: SetGeneric(value); break;
                default: UnityEngine.Debug.LogError($"{property.propertyType} not supported"); break;
            };

            void SetGeneric(object value)
            {
                if (value != null)
                {
                    if (!property.isArray) // no array
                    {
                        Type type = value.GetType();

                        foreach (var prop in property.GetAllPropertys(true))
                        {
                            FieldInfo field = type.GetFieldWholeInheritance(prop.name, defaultBindingFlags);
                            prop.SetValue(field.GetValue(value));
                        }
                    }
                    else // array
                    {
                        IList newValues;
                        try
                        {
                            newValues = (IList)value;
                        }
                        catch (InvalidCastException)
                        {
                            throw new ArgumentException($"{property.propertyPath} was expecting a list");
                        }
                        //Make same size as new values:


                        property.arraySize = newValues.Count;

                        for (int i = 0; i < property.arraySize; i++)
                        {
                            property.GetArrayElementAtIndex(i).SetValue(newValues[i]);
                        }
                    }
                }
                else // value is null
                {
                    SetGeneric(ForceCreateInstance(DirtyValue.GetType(property)));
                }
            }
        }
        /// <summary>
        /// Creates an object of given type (creates with or without constructor anyway)
        /// </summary>
        /// <returns></returns>
        public static object ForceCreateInstance(Type type)
        {
            try //if has constructor
            {
                return Activator.CreateInstance(type);
            }
            catch (MissingMethodException) //has no constructor
            {
                return FormatterServices.GetUninitializedObject(type);
            }
        }
    }
}