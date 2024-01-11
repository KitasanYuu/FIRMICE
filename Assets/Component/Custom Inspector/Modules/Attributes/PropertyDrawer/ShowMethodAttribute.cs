using System;
using System.Diagnostics;
using UnityEngine;


namespace CustomInspector
{
    /// <summary>
    /// Displays the value of a method in the inspector. (Method will get called on each OnGUI() call)
    /// e.g. if the method 'A' will output 3, then in the inspector will stand A \t 3
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    [Conditional("UNITY_EDITOR")]
    public class ShowMethodAttribute : PropertyAttribute
    {
        public readonly string getmethodPath;
        /// <summary>
        /// Displayed name in the inspector
        /// </summary>
        public string label = null;
        /// <summary>
        /// Tooltip on field
        /// </summary>
        public string tooltip = null;

        
        /// <param name="getmethodPath">The name of the method to display</param>
        public ShowMethodAttribute(string getmethodPath)
        {
            order = -10;
            this.getmethodPath = getmethodPath;
        }
    }
}