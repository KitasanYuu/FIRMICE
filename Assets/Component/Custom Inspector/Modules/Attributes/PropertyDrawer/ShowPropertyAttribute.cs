using System;
using System.Diagnostics;
using UnityEngine;


namespace CustomInspector
{
    /// <summary>
    /// Displays the given Property again
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    [Conditional("UNITY_EDITOR")]
    public class ShowPropertyAttribute : PropertyAttribute
    {
        public readonly string getPropertyPath;
        /// <summary>
        /// Displayed name in the inspector
        /// </summary>
        public string label = null;
        /// <summary>
        /// Tooltip on field
        /// </summary>
        public string tooltip = null;

        
        /// <param name="getPropertyPath">The name of the property to display</param>
        public ShowPropertyAttribute(string getPropertyPath)
        {
            order = -10;
            this.getPropertyPath = getPropertyPath;
        }
    }
}