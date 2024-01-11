using CustomInspector.Extensions;
using System;
using UnityEditor;
using UnityEngine;

namespace CustomInspector.Editor
{

    [CustomPropertyDrawer(typeof(MultipleOfAttribute))]
    public class MultipleOfAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            double d_step;
            try
            {
                d_step = GetStep(property);
            }
            catch (Exception e)
            {
                DrawProperties.DrawPropertyWithMessage(position, label, property, e.Message, MessageType.Error);
                return;
            }

            if (property.propertyType == SerializedPropertyType.Integer)
            {
                int step = (int)GetStep(property);
                if (step == d_step) //is int
                {
                    int prevNumber = property.intValue;
                    EditorGUI.BeginChangeCheck();
                    DrawProperties.PropertyField(position, label, property);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (MultipleOf.IsMultiple(property.intValue, step))
                        {
                            // already right
                        }
                        else
                        {
                            if(property.intValue > prevNumber) // was incremented
                                property.intValue = MultipleOf.NearestMultiple(prevNumber + step, step);
                            else
                                property.intValue = MultipleOf.NearestMultiple(prevNumber - step, step);
                        }
                        property.serializedObject.ApplyModifiedProperties();
                    }
                }
                else
                {
                    DrawProperties.DrawPropertyWithMessage(position, label, property, "MultipleOfAttribute: for int fields step must not have decimals", MessageType.Error);
                }
            }
            else if(property.propertyType == SerializedPropertyType.Float)
            {
                float prevNumber = property.floatValue;
                EditorGUI.BeginChangeCheck();
                DrawProperties.PropertyField(position, label, property);
                if (EditorGUI.EndChangeCheck())
                {
                    if (MultipleOf.IsMultiple(property.floatValue, d_step))
                    {
                        // already right
                    }
                    else
                    {
                        if (property.floatValue > prevNumber) // was incremented
                            property.floatValue = (float)MultipleOf.NearestMultiple(prevNumber + d_step, d_step);
                        else
                            property.floatValue = (float)MultipleOf.NearestMultiple(prevNumber - d_step, d_step);
                    }
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
            else
            {
                DrawProperties.DrawPropertyWithMessage(position, label, property,
                                                    $"MultipleOfAttribute only valid on float or int.", MessageType.Error);
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            double step;
            try
            {
                step = GetStep(property);
            }
            catch
            {
                return DrawProperties.GetPropertyWithMessageHeight(label, property);
            }

            if (property.propertyType == SerializedPropertyType.Float
                || (property.propertyType == SerializedPropertyType.Integer && step == (int)step))
            {
                return DrawProperties.GetPropertyHeight(label, property);
            }
            else
            {
                return DrawProperties.GetPropertyWithMessageHeight(label, property);
            }
        }
        double GetStep(SerializedProperty property)
        {
            MultipleOfAttribute m = (MultipleOfAttribute)attribute;
            if(m.stepPath is null)
            {
                return m.step;
            }
            else
            {
                property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                object res = DirtyValue.GetOwner(property).FindRelative(m.stepPath).GetValue();

                try
                {
                    return Convert.ToDouble(res);
                }
                catch (System.FormatException)
                {
                    throw new CustomInspector.Extensions.Exceptions.WrongTypeException($"'{m.stepPath}' is not type of number");
                }
            }
        }
    }
}