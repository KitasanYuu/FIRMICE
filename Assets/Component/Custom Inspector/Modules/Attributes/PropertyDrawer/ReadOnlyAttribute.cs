using System;
using System.Diagnostics;
using UnityEngine;



namespace CustomInspector
{
    public enum DisableStyle
    {
        /// <summary>
        /// Makes fields grey, which indicates no possibility to change it
        /// </summary>
        GreyedOut,
        /// <summary>
        /// No input field. Draws the object as a string-text plane in the inspector
        /// </summary>
        OnlyText,
    }

    /// <summary>
    /// Shows a not editable variable in the inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public class ReadOnlyAttribute : PropertyAttribute
    {
        public LabelStyle labelStyle;
        public DisableStyle disableStyle;

        public ReadOnlyAttribute(DisableStyle disableStyle = DisableStyle.GreyedOut, LabelStyle labelStyle = LabelStyle.FullSpacing)
        {
            order = -5;
            this.disableStyle = disableStyle;
            this.labelStyle = labelStyle;
        }
    }
}