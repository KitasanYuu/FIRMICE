using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace CustomInspector
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    [Conditional("UNITY_EDITOR")]
    public class TooltipBoxAttribute : PropertyAttribute
    {
        public readonly string content;
        public TooltipBoxAttribute(string content)
        {
            order = -6;
            this.content = content;
        }
    }
}
