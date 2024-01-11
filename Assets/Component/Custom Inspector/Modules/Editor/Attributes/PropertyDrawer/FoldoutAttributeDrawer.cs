using CustomInspector.Extensions;
using CustomInspector.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CustomInspector.Editor
{
    [CustomPropertyDrawer(typeof(FoldoutAttribute))]
    public class FoldoutAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.Generic) //already has a foldout
            {
                DrawProperties.PropertyField(position, label, property);
                return;
            }
            else if(property.propertyType != SerializedPropertyType.ObjectReference)
            {
                DrawProperties.DrawPropertyWithMessage(position, label, property,
                    $"{nameof(FoldoutAttribute)}'s supported type is only ObjectReference and not {property.propertyType}", MessageType.Error);
                return;
            }

            //Draw current
            Rect holdersRect = new(position)
            {
                height = DrawProperties.GetPropertyHeight(label, property)
            };

            DrawProperties.PropertyFieldWithFoldout(holdersRect, label, property);

            //Draw Members
            using (new EditorGUI.IndentLevelScope(1))
            {
                Rect membersRect = EditorGUI.IndentedRect(position);
                using (new NewIndentLevel(0))
                {
                    membersRect.y = holdersRect.y + holdersRect.height + EditorGUIUtility.standardVerticalSpacing;
                    membersRect.height = position.height - holdersRect.height - EditorGUIUtility.standardVerticalSpacing;

                    if (property.isExpanded)
                    {
                        if (TryGetValue(property, out object value))
                        {
                            if (value is Object o)
                                DrawMembers(membersRect, o);
                            else
                            {
                                property.isExpanded = false;
                                Debug.LogWarning($"Type '{value.GetType()}' cannot be fold out.");
                            }
                        }
                        else
                        {
                            property.isExpanded = false;
                            // Debug.LogWarning($"Please fill '{label.text}' in the inspector first to fold it out.");
                        }
                    }
                }
            }

            void DrawMembers(Rect position, Object displayedObject)
            {
                Debug.Assert(displayedObject != null, "No Object found to draw members on.");
                using (SerializedObject serializedObject = new(displayedObject))
                {
                    List<SerializedProperty> props = serializedObject.GetAllVisiblePropertys(true).ToList();
                    if (props.Count <= 0)
                    {
                        Debug.LogWarning(NoPropsWarning(displayedObject));
                        property.isExpanded = false;
                        return;
                    }
                    EditorGUI.BeginChangeCheck();
                    foreach (SerializedProperty p in props)
                    {
                        position.height = DrawProperties.GetPropertyHeight(new GUIContent(p.name, p.tooltip), p);
                        DrawProperties.PropertyField(position, new GUIContent(p.name, p.tooltip), p);
                        position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                    }
                    if(EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();
                }
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.Generic) //already has a foldout
                return DrawProperties.GetPropertyHeight(label, property);
            else if (property.propertyType != SerializedPropertyType.ObjectReference)
                return DrawProperties.GetPropertyWithMessageHeight(label, property);

            float currentHeight = DrawProperties.GetPropertyHeight(label, property);

            if (property.isExpanded && TryGetValue(property, out object value) && value is Object o)
                currentHeight += GetMembersHeight(o);
            
            return currentHeight;


            float GetMembersHeight(Object displayedObject)
            {
                Debug.Assert(displayedObject != null, "No Object found to search members on.");
                using (SerializedObject serializedObject = new(displayedObject))
                {
                    List<SerializedProperty> props = serializedObject.GetAllVisiblePropertys(true).ToList();
                    if (props.Count <= 0)
                    {
                        Debug.LogWarning(NoPropsWarning(displayedObject));
                        property.isExpanded = false;
                        return 0;
                    }
                    return props.Select(p => DrawProperties.GetPropertyHeight(new GUIContent(p.name, p.tooltip), p))
                                .Sum(x => x + EditorGUIUtility.standardVerticalSpacing);
                }
            }
        }
        static bool TryGetValue(SerializedProperty property, out object value)
        {
            value = property.GetValue();
            return value != null;
        }
        static string NoPropsWarning(Object target) => $"No properties found on {target.name}." +
                                $"\nPlease open the '{target.GetType()}' script and make sure all properties are public and serializable." +
                                "\nPrivates can be serialized with the [SerializeField] attribute.";
    }
}
