using CustomInspector.Extensions;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Scripting;

namespace CustomInspector.Editor
{
    [CustomPropertyDrawer(typeof(SelfFillAttribute))]
    public class SelfFillAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //unity fucks up
            label = PropertyValues.RepairLabel(label, property);

            //start

            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                DrawProperties.DrawPropertyWithMessage(position, label, property, $"SelfFill: type {property.propertyType} not supported. Use this attribute on objects (interfaces with RequireType or components)", MessageType.Error, disabled: true);
            }
            else
            {
                label.text += " (auto-filled)";
                string tooltipMessage = "SelfFill: This field will be automatically filled with the first matching component on this gameObject";
                label.tooltip = (string.IsNullOrEmpty(label.tooltip)) ? tooltipMessage : $"{label.tooltip}\n{tooltipMessage}";



                Component component = property.serializedObject.targetObject as Component;
                if (component == null)
                {
                    if(property.serializedObject.targetObject is ScriptableObject)
                        DrawProperties.DrawPropertyWithMessage(position, label, property, $"SelfFillAttribute for ScriptableObjects not supported", MessageType.Error, disabled: true);
                    else
                        DrawProperties.DrawPropertyWithMessage(position, label, property, $"SelfFillAttribute for {property.serializedObject.targetObject.GetType()} not supported", MessageType.Error, disabled: true);
                    return;
                }
                GameObject gob = component.gameObject;


                //Check if empty
                if (property.objectReferenceValue == null)
                {
                    var requiredInterface = fieldInfo.GetCustomAttribute<RequireTypeAttribute>(); // support for 'require custom type' - like interface

                    if (fieldInfo.FieldType == typeof(GameObject))
                    {
                        property.objectReferenceValue = gob;
                        property.serializedObject.ApplyModifiedProperties();
                    }
                    else if(requiredInterface != null)
                    {
                        property.objectReferenceValue = gob.GetComponent(requiredInterface.requiredType);

                        //Check if not found
                        if (property.objectReferenceValue != null)
                            property.serializedObject.ApplyModifiedProperties();
                        else
                        {
                            DrawProperties.DrawPropertyWithMessage(position, label, property, $"SelfFill: no component with interface '{requiredInterface.requiredType}' on this gameObject", MessageType.Error, disabled: true);
                            return;
                        }
                    }
                    else if (fieldInfo.FieldType == typeof(Component) || fieldInfo.FieldType.IsSubclassOf(typeof(Component)))
                    {
                        property.objectReferenceValue = gob.GetComponent(fieldInfo.FieldType);

                        //Check if not found
                        if (property.objectReferenceValue != null)
                            property.serializedObject.ApplyModifiedProperties();
                        else
                        {
                            DrawProperties.DrawPropertyWithMessage(position, label, property, $"SelfFill: no '{fieldInfo.FieldType}' component on this gameObject", MessageType.Error, disabled: true);
                            return;
                        }
                    }
                    else
                    {
                        DrawProperties.DrawPropertyWithMessage(position, label, property, $"SelfFill: type '{fieldInfo.FieldType}' not supported. Use component-types (no Assets) or interfaces (with the [RequireType]-attribute", MessageType.Error, disabled: true);
                        return;
                    }
                }
                else //property.objectReferenceValue != null
                {
                    //Check if valid (invalid fills when for example you copy the script to other objects)
                    if(property.objectReferenceValue is GameObject g)
                    {
                        if (!object.ReferenceEquals(g, gob)) //c.gameObject != gob
                        {
                            if (Application.isPlaying)
                                Debug.LogError($"gameObject reference value on {property.name} deleted. SelfFillAttribute only valid for own gameObject. Location: {property.serializedObject.targetObject}");
                            else
                                Debug.LogWarning($"gameObject reference value on {property.name} discarded. SelfFillAttribute only valid for own gameObject. Location: {property.serializedObject.targetObject}");
                            property.objectReferenceValue = null;
                            property.serializedObject.ApplyModifiedProperties();
                        }
                    }
                    else if(property.objectReferenceValue is Component c)
                    {
                        var requiredInterface = fieldInfo.GetCustomAttribute<RequireTypeAttribute>();

                        //should have right type
                        if(requiredInterface != null && !requiredInterface.requiredType.IsAssignableFrom(c.GetType()))
                        {
                            Debug.LogWarning($"Value on {property.name} had wrong type. {requiredInterface.requiredType} is not assignable from {c.GetType()}.\nValue set to null");
                            property.objectReferenceValue = null;
                            property.serializedObject.ApplyModifiedProperties();
                        }
                        //should be a component on same gameObject
                        else if (!object.ReferenceEquals(c.gameObject, gob)) //c.gameObject != gob
                        {
                            if (Application.isPlaying)
                                Debug.LogError($"objectReferenceValue on {property.name} deleted. SelfFillAttribute only valid for components on same gameObject as the script, holding them. Location: {property.serializedObject.targetObject}");
                            else
                                Debug.LogWarning($"objectReferenceValue on {property.name} discarded. SelfFillAttribute only valid for components on same gameObject as the script, holding them. Location: {property.serializedObject.targetObject}\"");
                            property.objectReferenceValue = null;
                            property.serializedObject.ApplyModifiedProperties();
                        }
                    }
                    else //like an asset
                    {
                        if (!Application.isPlaying)
                            Debug.LogWarning($"Value on {property.name} discarded, because selffillattribute only supports components");
                        else
                            Debug.LogError($"Reference on {property.name} deleted, because selffillattribute only supports components. Location: {property.serializedObject.targetObject}");
                        property.objectReferenceValue = null;
                        property.serializedObject.ApplyModifiedProperties();
                    }
                }
                //Display
                SelfFillAttribute sa = (SelfFillAttribute)attribute;

                if (!sa.hideIfFilled)
                {
                    DrawProperties.DisabledPropertyField(position, label, property);
                }
            }
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if ((property.propertyType != SerializedPropertyType.ObjectReference && fieldInfo.GetCustomAttribute<RequireTypeAttribute>() == null)
                || property.objectReferenceValue == null)
            {
                return DrawProperties.GetPropertyWithMessageHeight(label, property);
            }
            else
            {
                SelfFillAttribute sa = (SelfFillAttribute)attribute;
                if (!sa.hideIfFilled)
                    return DrawProperties.GetPropertyHeight(label, property);
                else
                    return -EditorGUIUtility.standardVerticalSpacing;
            }
        }
    }
}