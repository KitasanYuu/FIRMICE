using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CustomInspector
{
    /// <summary>
    /// separates the current field from the previous ones in the inspector with a Line
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    [Conditional("UNITY_EDITOR")]
    public class HorizontalLineAttribute : PropertyAttribute
    {
        /// <summary>A custom label on the line</summary>
        public string message = null;
        /// <summary>the height of the line (y direction)</summary>
        public float thickness;
        /// <summary>the empty space height before and after the line (y direction)</summary>
        public float spacing;
        /// <summary>Make a gap before and after the line (x direction)</summary>
        public float gapSize;

        /// <summary>the color of the line</summary>
        public readonly FixedColor color;

        private HorizontalLineAttribute() => order = -10;
        /// <summary>
        /// Adds a horizontal line in the inspector above a property
        /// </summary>
        /// <param name="thickness">the height of the line</param>
        /// <param name="color">the color of the line</param>
        /// <param name="spacing">the empty space height before and after the line</param>
        /// <param name="gap">Make a gap before and after the line</param>
        public HorizontalLineAttribute(float thickness = 1, FixedColor color = FixedColor.Black, float spacing = 7, float gapSize = 0)
        : this()
        {

#if UNITY_EDITOR
            if (thickness < 0)
            {
                Debug.LogWarning("HorizontalLineAttribute: thickness must be positive");
                thickness = 0;
            }
            if (spacing < 0)
            {
                Debug.LogWarning("HorizontalLineAttribute: spacing should be positive");
                spacing = 0;
            }
            if (gapSize < 0)
            {
                Debug.LogWarning("HorizontalLineAttribute: gapSize should be positive");
                spacing = 0;
            }
#endif
            this.thickness = thickness;
            this.color = color;
            this.spacing = spacing;
            this.gapSize = gapSize;
        }

        /// <summary>
        /// Adds a horizontal line in the inspector above a property
        /// </summary>
        /// <param name="message">A custom label on the line</param>
        /// <param name="thickness">the height of the line</param>
        /// <param name="color">the color of the line</param>
        /// <param name="spacing">the empty space height before and after the line</param>
        /// <param name="gap">Make a gap before and after the line</param>
        public HorizontalLineAttribute(string message, float thickness = 1, FixedColor color = FixedColor.Cyan, float spacing = 15, float gapSize = 0)
        : this(thickness, color, spacing, gapSize)
        {
            this.message = message;
        }
        public HorizontalLineAttribute(string message, float thickness, float spacing, float gapSize = 0)
        : this(thickness, FixedColor.Cyan, spacing, gapSize)
        {
            this.message = message;
        }
    }
}