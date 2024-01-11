using UnityEditor;
using UnityEngine;
using CustomInspector.Extensions;
using System;
using System.Linq;
using System.Collections.Generic;

namespace CustomInspector.Editor
{

    [CustomPropertyDrawer(typeof(ShowMethodAttribute))]
    public class ShowMethodAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ShowMethodAttribute sm = (ShowMethodAttribute)attribute;
            //GUIContent proplabel = new(property.name, property.tooltip);

            InvokableMethod getter;
            try
            {
                getter = PropertyValues.GetMethodOnOwner(property, sm.getmethodPath);
            }
            catch(Exception ex)
            {
                DrawProperties.DrawPropertyWithMessage(position, label, property, ex.Message, MessageType.Error);
                return;
            }
            
            //Check
            if (getter.ReturnType == typeof(void))
            {
                DrawProperties.DrawPropertyWithMessage(position, label, property, $"ShowMethod: given method {sm.getmethodPath} doesnt have a return value", MessageType.Error);
                return;
            }

            //Call getter
            property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            object value;
            try
            {
                value = getter.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                DrawProperties.DrawPropertyWithMessage(position, label, property, "error in method. See console for more information", MessageType.Error);
                return;
            }

            //Draw
            string infoMessage = $"The result of {property.serializedObject.targetObject.name}.{property.serializedObject.targetObject.GetType().Name}.{property.propertyPath.PrePath(true)}{sm.getmethodPath}()";
            GUIContent methodLabel = new()
            {
                text = sm.label ?? TryGetNameOuttaGetter(getter.Name),
                tooltip = string.IsNullOrEmpty(sm.tooltip) ? infoMessage : sm.tooltip + "\n" + infoMessage
            };
            Rect methodRect = new(position)
            {
                height = DrawProperties.GetPropertyHeight(PropertyConversions.ToPropertyType(getter.ReturnType), methodLabel)
            };

            DrawProperties.DrawField(methodRect, methodLabel, value, getter.ReturnType, disabled: true);

            //Draw property below
            Rect propRect = new()
            {
                x = position.x,
                y = methodRect.y + methodRect.height + EditorGUIUtility.standardVerticalSpacing,
                height = DrawProperties.GetPropertyHeight(label, property),
                width = position.width,
            };
            DrawProperties.PropertyField(propRect, label, property);
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ShowMethodAttribute sm = (ShowMethodAttribute)attribute;


            InvokableMethod getter;
            try
            {
                getter = PropertyValues.GetMethodOnOwner(property, sm.getmethodPath);
                getter.Invoke();
            }
            catch
            {
                return DrawProperties.GetPropertyWithMessageHeight(label, property);
            }

            if (getter.ReturnType == typeof(void))
                return DrawProperties.GetPropertyWithMessageHeight(label, property);
            else
                return DrawProperties.GetPropertyHeight(PropertyConversions.ToPropertyType(getter.ReturnType), label)
                     + EditorGUIUtility.standardVerticalSpacing
                     + DrawProperties.GetPropertyHeight(label, property);
        }
        public static string TryGetNameOuttaGetter(string getterName)
        {
            if(getterName.Length >= 3
                && (getterName[0] == 'G' || getterName[0] == 'g')
                && (getterName[1] == 'E' || getterName[1] == 'e')
                && (getterName[2] == 'T' || getterName[2] == 't'))
                    getterName = getterName[3..];

            if(getterName.Length > 1)
            {
                List<char> newName = getterName.ToList();
                for (int i = 1; i < newName.Count; i++)
                {
                    if (newName[i] >= 'A' && newName[i] <= 'Z') //uppercase
                    {
                        newName.Insert(i + 1, (char)(newName[i] + 'a' - 'A'));
                        newName[i] = ' ';
                    }
                }
                return String.Join(null, newName.Select(_ => _.ToString()));
            }
            else return getterName;
        }
    }
}