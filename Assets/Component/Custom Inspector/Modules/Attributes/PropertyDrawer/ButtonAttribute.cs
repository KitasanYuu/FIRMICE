using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CustomInspector
{


    /// <summary>
    /// Adds in the inspector to a button, that calls methods.
    /// Put name in like: [SerializeField, InspectorButton(nameof(myMethod))] bool someNameInInspector;
    /// or for methods within other fields: [SerializeField, InspectorButton("someField.myMethod")] bool someNameInInspector;
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    [Conditional("UNITY_EDITOR")]
    public class ButtonAttribute : PropertyAttribute
    {
        public readonly string methodPath;
        public readonly bool usePropertyAsParameter;

        public string label = null;
        public string tooltip = null;
        public Size size = Size.medium;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="methodPath">The name of the method to execute</param>
        /// <param name="usePropertyAsParameter">creates an input field of type of the property. You can invoke the method with it</param>
        public ButtonAttribute(string methodPath, bool usePropertyAsParameter = false)
        {
            if (!usePropertyAsParameter) //then its compatible
                order = -10;
            if (string.IsNullOrEmpty(methodPath))
                Debug.LogWarning($"{nameof(ButtonAttribute)}: needs a valid string to be passed as {nameof(methodPath)} parameter");
            this.methodPath = methodPath;
            this.usePropertyAsParameter = usePropertyAsParameter;
        }
    }
}