using System;
using System.Diagnostics;
using UnityEngine;

namespace CustomInspector
{
    /// <summary>
    /// Hides the field in the inspector but not attributes attached to it
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public class HideFieldAttribute : PropertyAttribute
    {
        public HideFieldAttribute() => order = 1;
    }
}

