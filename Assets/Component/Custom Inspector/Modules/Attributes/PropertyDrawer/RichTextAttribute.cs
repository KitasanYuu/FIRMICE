using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace CustomInspector
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    [Conditional("UNITY_EDITOR")]
    public class RichTextAttribute : PropertyAttribute
    {
        public readonly bool allowMultipleLines;
        public readonly int maxLines;

        public RichTextAttribute()
        {
            order = 0;
        }
        /// <summary> </summary>
        /// <param name="allowMultipleLines">If text field should be expanded on every newLine</param>
        /// <param name="maxLines">Even if "allowMultipleLines" is true, "maxLines" defines a maximum amount of lines.</param>
        public RichTextAttribute(bool allowMultipleLines, int maxLines = 15) : this()
        {
            this.allowMultipleLines = allowMultipleLines;
            this.maxLines = Math.Max(maxLines, 1);
        }
    }
}
