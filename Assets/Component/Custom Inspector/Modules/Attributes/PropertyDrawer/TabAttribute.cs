using CustomInspector.Extensions;
using System;
using System.Diagnostics;
using UnityEngine;

namespace CustomInspector
{
    /// <summary>
    /// Hide and show groups of fields based on a toolbar-selection
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public class TabAttribute : PropertyAttribute
    {
        public readonly string groupName = null;

        public FixedColor backgroundColor = FixedColor.DarkGray;

        public TabAttribute(string groupName)
        {
            order = -10;
            this.groupName = groupName;
        }
        public TabAttribute(InspectorIcon icon)
        {
            groupName = icon.ToInternalIconName();
        }
    }
}