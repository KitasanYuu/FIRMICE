using CustomInspector.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace CustomInspector
{
    public enum InspectorButtonState { isPressed, isSelected, notSelected, }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false)]
    [Conditional("UNITY_EDITOR")]
    public class AsButtonAttribute : PropertyAttribute
    {
        public readonly bool staysPressed;

        public Size size = Size.medium;
        public FixedColor selectedColor = FixedColor.PressedBlue;
        public string label = null;
        public string tooltip = "";

        public AsButtonAttribute(bool staysPressed = true)
        {
            this.staysPressed = staysPressed;
        }
    }
}
