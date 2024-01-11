using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace CustomInspector
{

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    [Conditional("UNITY_EDITOR")]
    public class InspectorIconAttribute : PropertyAttribute
    {
        public readonly InspectorIcon icon;
        public readonly bool appendAtEnd;
        public InspectorIconAttribute(InspectorIcon icon, bool appendAtEnd = false)
        {
            order = -10;
            this.icon = icon;
            this.appendAtEnd = appendAtEnd;
        }
    }
}
