using System;
using System.Diagnostics;
using UnityEngine;


namespace CustomInspector
{

    /// <summary>
    /// Draws an ObjectField constrained to given type like some interface
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public class RequireTypeAttribute : PropertyAttribute
    {
        public System.Type requiredType { get; private set; }

        public RequireTypeAttribute(System.Type type)
        {
            this.requiredType = type;
        }
    }
}