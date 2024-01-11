using System;
using System.Diagnostics;
using UnityEngine;

namespace CustomInspector
{
    /// <summary>
    /// Change the variable label in the unity inspector
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public class LabelSettingsAttribute : PropertyAttribute
    {
        public readonly LabelStyle style;
        public readonly string newName = null;

        public LabelSettingsAttribute(LabelStyle style)
        {
            order = -5;
            this.style = style;
        }
        public LabelSettingsAttribute(string newName, LabelStyle style = LabelStyle.FullSpacing)
        {
            order = -5;
            this.newName = newName;
            this.style = style;
        }
    }
}