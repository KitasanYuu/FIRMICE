using System;
using System.Diagnostics;
using UnityEngine;

namespace CustomInspector
{
    /// <summary>
    /// Calls a method with given name and returns the message if it returns false
    /// <para>method description: bool MethodName(valueType value) {...}</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    [Conditional("UNITY_EDITOR")]
    public class ValidateAttribute : PropertyAttribute
    {
        public readonly string methodPath;
        public readonly string errorMessage;
        public MessageBoxType errorType;

        public ValidateAttribute(string methodPath, string errorMessage = "Value is not valid", MessageBoxType errorType = MessageBoxType.Error)
        {
            order = -10;
            this.methodPath = methodPath;
            this.errorMessage = errorMessage;
            this.errorType = errorType;
        }
    }
}