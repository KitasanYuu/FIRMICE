using System;
using System.Diagnostics;
using UnityEngine;


namespace CustomInspector
{
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public class GUIColorAttribute : PropertyAttribute
    {
        public readonly string colorString = null;
        public readonly FixedColor? fixedColor = null;
        public readonly bool colorWholeUI;

        public GUIColorAttribute(string color = "(0.9, 0.0, 0, 1)", bool colorWholeUI = false)
        {
            order = -10;
            this.colorString = color;
            this.colorWholeUI = colorWholeUI;
        }
        public GUIColorAttribute(FixedColor fixedColor, bool colorWholeUI = false)
        {
            order = -10;
            this.fixedColor = fixedColor;
            this.colorWholeUI = colorWholeUI;
        }
    }
}
