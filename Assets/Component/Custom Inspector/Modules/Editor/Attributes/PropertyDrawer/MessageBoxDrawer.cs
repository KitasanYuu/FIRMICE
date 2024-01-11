using UnityEngine;
using UnityEditor;
using CustomInspector.Extensions;

namespace CustomInspector.Editor
{
    /// <summary>
    /// Draws the field name and behind a custom errorMessage
    /// </summary>
    [CustomPropertyDrawer(typeof(MessageBoxAttribute))]
    public class MessageBoxDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            MessageBoxAttribute hv = (MessageBoxAttribute)attribute;
            DrawProperties.DrawPropertyWithMessage(position, label, property, hv.content, MessageBoxConvert.ToUnityMessageType(hv.type));
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return DrawProperties.GetPropertyWithMessageHeight(label, property);
        }
    }
}