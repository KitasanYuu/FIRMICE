using System;
using System.Diagnostics;
using UnityEngine;

namespace CustomInspector
{
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public class LayerAttribute : PropertyAttribute
    {
        public readonly string requiredName = null;
        public LayerAttribute() { }
        public LayerAttribute(string requiredName)
        {
            this.requiredName = requiredName;
        }
    }
}