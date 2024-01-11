using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CustomInspector
{
    /// <summary>
    /// Sets a maximum value for inputs in the inspector in form of a number or a reference (given by path)
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public class MaxAttribute : PropertyAttribute
    {
        /// <summary>
        /// The maximum allowed value.
        /// </summary>
        public readonly float max;
        public readonly string maxPath = null;
        private MaxAttribute()
        {
            //Has to be before the built-in MinAttribute
            order = -10;
        }
        /// <summary>
        /// Attribute used to make a float or int variable in a script be restricted to a specific maximum  value.
        /// </summary>
        /// <param name="max">The maximum  allowed value.</param>
        public MaxAttribute(float max)
        {
            this.max = max;
        }
        public MaxAttribute(string maxPath) : this()
        {
            if (maxPath == null)
                Debug.LogWarning($"No {nameof(maxPath)} given to Min2Attribute to retrieve value from");
            this.maxPath = maxPath;
        }
        public bool DependsOnOtherProperty() => maxPath != null;
    }
}