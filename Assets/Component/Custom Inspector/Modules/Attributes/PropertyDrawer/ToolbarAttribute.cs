using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CustomInspector
{
    /// <summary>
    /// Draws radio-buttons
    /// Valid for enums and booleans.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public class ToolbarAttribute : PropertyAttribute
    {
        public readonly float height;
        public readonly float space;
        public readonly float maxWidth;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="height">Size of 17 is same size as a normal text</param>
        /// <param name="space">Empty lines before and after the toolbar</param>
        /// <param name="maxWidth">Max width an element of the toolbar takes</param>
        public ToolbarAttribute(float height = 30, float space = 10, float maxWidth = 150)
        {
            if (height < 5)
            {
                Debug.LogWarning("ToolbarAttribute: size has to be greater 5");
                height = 5;
            }
            if (space < 0)
            {
                Debug.LogWarning("ToolbarAttribute: space has to be greater or zero");
                space = 0;
            }
            if (maxWidth < 25)
            {
                Debug.LogWarning("ToolbarAttribute: maxWidth has to be greater than 25");
                maxWidth = 100;
            }
            this.height = height;
            this.space = space;
            this.maxWidth = maxWidth;
        }
    }
}