using CustomInspector.Extensions;
using UnityEditor;
using UnityEngine;


namespace CustomInspector.Editor
{
    [CustomPropertyDrawer(typeof(LayerAttribute))]
    class LayerAttributeDrawer : PropertyDrawer
    {
        const float fixButtonWidth = 40;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            LayerAttribute l = (LayerAttribute)attribute;

            Rect rect;

            if(!string.IsNullOrEmpty(l.requiredName))
            {
                int req = LayerMask.NameToLayer(l.requiredName);

                if(req == -1)
                {
                    //wrong layer name entered
                    DrawProperties.DrawPropertyWithMessage(position, label, property,
                        $"LayerName {l.requiredName} not found.\nTyping-Error or Layer was removed.", MessageType.Error);

                    return;
                }
                else if(req == property.intValue)
                {
                    rect = position;
                }
                else
                {
                    Rect errorRect = new(position)
                    {
                        y = position.y + DrawProperties.errorStartSpacing,
                        width = position.width - fixButtonWidth,
                        height = DrawProperties.errorHeight,
                    };
                    DrawProperties.DrawMessageField(errorRect, $"{property.name}'s value does not match the code's: {l.requiredName}", MessageType.Warning);

                    Rect buttonRect = new(errorRect)
                    {
                        x = errorRect.x + errorRect.width,
                        width = fixButtonWidth,
                    };
                    if(GUI.Button(buttonRect, new GUIContent("Fix", $"set Layer to {l.requiredName}")))
                    {
                        property.intValue = LayerMask.NameToLayer(l.requiredName);
                        property.serializedObject.ApplyModifiedProperties();
                    }

                    rect = new(position)
                    {
                        y = errorRect.y + errorRect.height,
                        height = EditorGUIUtility.singleLineHeight,
                    };
                }
            }
            else
            {
                rect = position;
            }

            EditorGUI.BeginChangeCheck();
            int res = EditorGUI.LayerField(rect, label, property.intValue);
            if (EditorGUI.EndChangeCheck())
            {
                property.intValue = res;
                property.serializedObject.ApplyModifiedProperties();
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            LayerAttribute l = (LayerAttribute)attribute;
            if (!string.IsNullOrEmpty(l.requiredName))
            {
                int req = LayerMask.NameToLayer(l.requiredName);
                if (req == -1 || req != property.intValue)
                    return DrawProperties.errorStartSpacing + DrawProperties.errorHeight + EditorGUIUtility.singleLineHeight;
                else
                    return EditorGUIUtility.singleLineHeight;
            }
            else
                return EditorGUIUtility.singleLineHeight;
        }
    }
}