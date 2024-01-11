using System;
using System.Diagnostics;
using UnityEngine;


namespace CustomInspector
{

    /// <summary>
    /// Show field, if field (given by path/name) is equal to given value
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    [Conditional("UNITY_EDITOR")]
    public class ShowIfIsAttribute : PropertyAttribute
    {
        public readonly string fieldPath;
        public readonly object value;
        public DisabledStyle style = DisabledStyle.Invisible;
        public int indent = 1;

        public bool Inverted { get; protected set; } = false;

        public ShowIfIsAttribute(string fieldPath, object value)
        {
            order = -10;
            this.fieldPath = fieldPath;
            this.value = value;
        }
    }
    /// <summary>
    /// Show field, if field (given by path/name) is not equal to given value
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    [Conditional("UNITY_EDITOR")]
    public class ShowIfIsNotAttribute : ShowIfIsAttribute
    {
        public ShowIfIsNotAttribute(string fieldPath, object value) : base(fieldPath, value)
        {
            base.Inverted = true;
        }
    }
}