using UnityEngine;
using UnityEditor;
using CustomInspector.Extensions;
using System;
using System.Linq;
using System.Collections.Generic;

namespace CustomInspector.Editor
{
    [CustomPropertyDrawer(typeof(ButtonAttribute))]
    public class ButtonAttributeDrawer : PropertyDrawer
    {
        const float maxInputWidth = 100;
        const float horizontalSpacing = 5;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ButtonAttribute ib = (ButtonAttribute)attribute;
            

            if (ib.usePropertyAsParameter) //use field as input
            {
                DirtyValue dv = new(property);
                Type[] pTypes = new Type[] { dv.Type };

                InvokableMethod method;
                try
                {
                    method = PropertyValues.GetMethodOnOwner(property, ib.methodPath, pTypes);
                }
                catch(Exception e)
                {
                    DrawProperties.DrawPropertyWithMessage(position, label, property, e.Message, MessageType.Error);
                    return;
                }

                (GUIContent buttonLabel, Vector2 labelSize) = GetButtonLabel();

                float savedLabelWidth = EditorGUIUtility.labelWidth;
                if (string.IsNullOrEmpty(label.text))
                    label.text = "param";
                GUIContent inputLabel = new(PropertyConversions.NameFormat(label.text), label.tooltip + "\n-This will be passed in the function as paramter");
                EditorGUIUtility.labelWidth = Mathf.Min( GUI.skin.label.CalcSize(inputLabel).x, position.width/4f);

                Rect buttonRect = new(x: position.x, y: position.y,
                                    width: Math.Min(position.width - EditorGUIUtility.fieldWidth - horizontalSpacing, labelSize.x), //zumindest platz für ein field
                                    height: labelSize.y);

                position.y += Math.Max((buttonRect.height - EditorGUIUtility.singleLineHeight) / 2, 0);
                float inputSpace = position.width - buttonRect.width - horizontalSpacing;
                float input_width = Math.Min(inputSpace, maxInputWidth);
                input_width = Math.Max(input_width, inputSpace - (GUI.skin.label.CalcSize(inputLabel).x + 20));
                Rect inputRect = new(x: position.x + position.width - input_width,
                                            y: position.y,
                                            width: input_width,
                                            height: EditorGUIUtility.singleLineHeight);

                float inputLabel_width = position.width - buttonRect.width - horizontalSpacing - inputRect.width;
                if(inputLabel_width > 0)
                {
                    Rect inputLabelRect = new(x: buttonRect.x + buttonRect.width + horizontalSpacing,
                                            y: position.y,
                                            width: inputLabel_width,
                                            height: EditorGUIUtility.singleLineHeight);

                    using (new NewIndentLevel(0))
                    {
                        EditorGUI.LabelField(inputLabelRect, inputLabel);
                    }
                }
                
                using (new NewIndentLevel(0))
                {
                    EditorGUI.BeginChangeCheck();
                    DrawProperties.PropertyFieldWithoutLabel(inputRect, property);
                    if(EditorGUI.EndChangeCheck())
                        property.serializedObject.ApplyModifiedProperties();
                }
                
                if (GUI.Button(buttonRect, buttonLabel))
                {
                    object inputValue = property.GetValue();
                    Debug.Assert(inputValue.GetType() == dv.Type, $"Mismatched type: {inputValue.GetType()} not same type as {dv.Type}");
                    var input = new object[] { inputValue };

                    if (Selection.count <= 1)
                    {
                        property.serializedObject.ApplyModifiedProperties();
                        try
                        {
                            method.Invoke(parameters: input);
                        }
                        catch(Exception e) { Debug.LogException(e); }
                        property.serializedObject.ApplyModifiedFields(true);
                    }
                    else //multiediting
                    {
                        var serializedObjects = property.serializedObject.targetObjects.Select(_ => new SerializedObject(_)).ToList();

                        foreach (var so in serializedObjects)
                        {
                            so.ApplyModifiedProperties();
                        }
                        foreach (var so in serializedObjects)
                        {
                            InvokableMethod m = InvokableMethod.GetMethod(obj: new DirtyValue(so.targetObject, property.propertyPath.PrePath(false)),
                                                                         methodPath: ib.methodPath, pTypes);
                            try
                            {
                                m.Invoke();
                            }
                            catch (Exception e)
                            {
                                Debug.LogException(e);
                            }
                        }
                        foreach (var so in serializedObjects)
                        {
                            so.ApplyModifiedFields(true);
                            so.Dispose();
                        }
                    }
                }

                EditorGUIUtility.labelWidth = savedLabelWidth;
            }
            else //just the default: only the button
            {
                InvokableMethod method;
                try
                {
                    method = PropertyValues.GetMethodOnOwner(property, ib.methodPath);
                }
                catch (Exception e)
                {
                    DrawProperties.DrawPropertyWithMessage(position, label, property, e.Message, MessageType.Error);
                    return;
                }

                (GUIContent buttonLabel, Vector2 labelSize) = GetButtonLabel();
                Rect buttonRect = new(position.x + (position.width - labelSize.x) / 2, position.y,
                    labelSize.x, labelSize.y);

                if (GUI.Button(buttonRect, buttonLabel))
                {
                    //Debug.Log("pressed button");
                    if (Selection.count <= 1)
                    {
                        property.serializedObject.ApplyModifiedProperties();
                        try
                        {
                            method.Invoke();
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                        }
                        property.serializedObject.ApplyModifiedFields(true);
                    }
                    else //multiediting
                    {
                        List<SerializedObject> serializedObjects = property.serializedObject.targetObjects.Select(_ => new SerializedObject(_)).ToList();

                        foreach (var so in serializedObjects)
                        {
                            so.ApplyModifiedProperties();
                        }
                        foreach (var so in serializedObjects)
                        {
                            InvokableMethod m = InvokableMethod.GetMethod(obj: new DirtyValue(so.targetObject, property.propertyPath.PrePath(false)),
                                                                         methodPath: ib.methodPath);
                            try
                            {
                                m.Invoke();
                            }
                            catch (Exception e)
                            {
                                Debug.LogException(e);
                            }
                        }
                        foreach (var so in serializedObjects)
                        {
                            so.ApplyModifiedFields(true);
                            so.Dispose();
                        }
                    }
                        
                }
                    
                position.y += buttonRect.height + EditorGUIUtility.standardVerticalSpacing;

                //And the field below
                position.height = DrawProperties.GetPropertyHeight(label, property);
                GUIContent propLabel = new(property.name, property.tooltip);

                DrawProperties.PropertyField(position, propLabel, property, true);
            }



            (GUIContent buttonLabel, Vector2 size) GetButtonLabel()
            {
                GUIContent buttonLabel;
                if (ib.label == null) //we testet earlier, that path is valid
                {
                    try
                    {
                        buttonLabel = new(PropertyConversions.NameFormat(PropertyConversions.PathEnd(ib.methodPath)), ib.tooltip);
                    }
                    catch
                    {
                        Debug.LogError($"{nameof(ButtonAttribute)}: MethodPath could not be formatted"); //should be testet earlier
                        buttonLabel = new GUIContent("error: see console");
                    }
                }
                else
                    buttonLabel = new(ib.label, ib.tooltip);
                float buttonWidth = StylesConvert.ToButtonWidth(position.width, buttonLabel, ib.size);
                float buttonHeight = StylesConvert.ToButtonHeight(ib.size);

                return (buttonLabel, new Vector2 (buttonWidth, buttonHeight));
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ButtonAttribute ib = (ButtonAttribute)attribute;

            try
            {
                if (ib.usePropertyAsParameter)
                {
                    DirtyValue dv = new(property);
                    Type[] pTypes = new Type[] { dv.Type };
                    var method = PropertyValues.GetMethodOnOwner(property, ib.methodPath, pTypes);
                }
                else
                {
                    var method = PropertyValues.GetMethodOnOwner(property, ib.methodPath);
                }
            }
            catch
            {
                return DrawProperties.GetPropertyWithMessageHeight(label, property);
            }

            if (ib.usePropertyAsParameter)
            {
                float buttonheight = StylesConvert.ToButtonHeight(ib.size);
                float upperSpacing = Math.Max((buttonheight - EditorGUIUtility.singleLineHeight) / 2f, 0);
                float propHeight = DrawProperties.GetPropertyHeight(label, property)
                                    + upperSpacing * 2; // *2 because some space below
                return Math.Max(buttonheight, propHeight);
            }
            else
                return StylesConvert.ToButtonHeight(ib.size)
                + EditorGUIUtility.standardVerticalSpacing + DrawProperties.GetPropertyHeight(label, property);
        }
    }
}