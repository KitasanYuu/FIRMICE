using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System;
using Debug = UnityEngine.Debug;

namespace CustomInspector
{
    public enum MessageBoxType { None, Info, Warning, Error }
    /// <summary>
    /// Draw a message box in the inspector. You can do it instead of the field or additionally
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    [Conditional("UNITY_EDITOR")]
    public class MessageBoxAttribute : PropertyAttribute
    {
        public readonly string content;
        public readonly MessageBoxType type;

        public readonly float height;
        public MessageBoxAttribute(string content, MessageBoxType type = MessageBoxType.Error, float height = 40)
        {
            order = -10;

            this.content = content;
            this.type = type;
            this.height = height;
        }
    }
#if UNITY_EDITOR
    public static class MessageBoxConvert
    {
        public static MessageType ToUnityMessageType(MessageBoxType type)
        {
            return type switch
            {
                MessageBoxType.None => MessageType.None,
                MessageBoxType.Info => MessageType.Info,
                MessageBoxType.Warning => MessageType.Warning,
                MessageBoxType.Error => MessageType.Error,
                _ => throw new NotImplementedException(type.ToString())
            };
        }
    }
#endif
}