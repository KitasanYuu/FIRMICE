using UnityEditor;
using UnityEngine;
using CustomInspector.Extensions;
using System;

namespace CustomInspector.Editor
{

    [CustomPropertyDrawer(typeof(GetSetAttribute))]
    public class GetSetAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GetSetAttribute sm = (GetSetAttribute)attribute;

            //Get getter
            InvokableMethod getter;
            try
            {
                getter = PropertyValues.GetMethodOnOwner(property, sm.getmethodPath);
            }
            catch (Exception e)
            {
                DrawProperties.DrawPropertyWithMessage(position, label, property, "get-method: " + e.Message, MessageType.Error);
                return;
            }
            //check getters return type
            if (getter.ReturnType == typeof(void))
            {
                string errorMessage = $"Get-Method {sm.getmethodPath} doesnt have a return value";
                DrawProperties.DrawPropertyWithMessage(position, label, property, errorMessage, MessageType.Error);
                return;
            }
            //get setter with getters return type
            InvokableMethod setter;
            try
            {
                setter = PropertyValues.GetMethodOnOwner(property, sm.setmethodPath, new Type[] { getter.ReturnType });
            }
            catch (Exception e)
            {
                DrawProperties.DrawPropertyWithMessage(position, label, property, "set-method: " + e.Message, MessageType.Error);
                return;
            }

            //call getter
            object value;
            property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            try
            {
                value = getter.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                DrawProperties.DrawPropertyWithMessage(position, label, property, "error in get-function. See console for more information", MessageType.Error);
                return;
            }
            //Draw Value
            EditorGUI.BeginChangeCheck();
            Rect getRect = new(position)
            {
                height = DrawProperties.GetPropertyHeight(PropertyConversions.ToPropertyType(getter.ReturnType), label),
            };
            GUIContent getSetLabel;
            string tooltip = sm.tooltip;
            if (sm.label is null)
                getSetLabel = new(ShowMethodAttributeDrawer.TryGetNameOuttaGetter(getter.Name), tooltip);
            else
                getSetLabel = new(sm.label, tooltip);

            var res = DrawProperties.DrawField(position: getRect, label: getSetLabel, value: value, getter.ReturnType);
            if (EditorGUI.EndChangeCheck())
            {
                //call setter
                try
                {
                    setter.Invoke(res);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                property.serializedObject.ApplyModifiedFields(true);
            }

            //Draw Property below
            Rect propRect = new(position)
            {
                y = position.y + getRect.height + EditorGUIUtility.standardVerticalSpacing,
                height = DrawProperties.GetPropertyHeight(label, property),
            };
            EditorGUI.BeginChangeCheck();
            DrawProperties.PropertyField(propRect, label, property);
            if (EditorGUI.EndChangeCheck())
                property.serializedObject.ApplyModifiedProperties();
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            GetSetAttribute sm = (GetSetAttribute)attribute;

            //Get getter
            InvokableMethod getter;
            try
            {
                getter = PropertyValues.GetMethodOnOwner(property, sm.getmethodPath);
            }
            catch
            {
                return DrawProperties.GetPropertyWithMessageHeight(label, property);
            }

            //check getters return type
            if (getter.ReturnType == typeof(void))
                return DrawProperties.GetPropertyWithMessageHeight(label, property);

            //get setter with getters return type
            InvokableMethod setter;
            try
            {
                setter = PropertyValues.GetMethodOnOwner(property, sm.setmethodPath, new Type[] { getter.ReturnType });
            }
            catch
            {
                return DrawProperties.GetPropertyWithMessageHeight(label, property);
            }

            //Draw
            return DrawProperties.GetPropertyHeight(PropertyConversions.ToPropertyType(getter.ReturnType), label) + EditorGUIUtility.standardVerticalSpacing
                        + DrawProperties.GetPropertyHeight(label, property);
        }
    }
}