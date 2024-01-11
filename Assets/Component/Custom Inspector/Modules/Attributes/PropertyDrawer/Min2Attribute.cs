using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CustomInspector
{
    /// <summary>
    /// An extension to the unitys built-in [Min]-attribute that also accepts references in form of relative paths
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public class Min2Attribute : PropertyAttribute
    {
        /// <summary>
        /// The minimum allowed value.
        /// </summary>
        public readonly float min;
        public readonly string minPath = null;

        public Min2Attribute()
        {
            order = -10;
        }

        /// <summary>
        /// Attribute used to make a float or int variable in a script be restricted to a specific minimum value.
        /// </summary>
        /// <param name="min">The minimum  allowed value.</param>
        public Min2Attribute(float min) : this()
        {
            this.min = min;
        }
        public Min2Attribute(string minPath) : this()
        {
            if (minPath == null)
                Debug.LogWarning($"No {nameof(minPath)} given to Min2Attribute to retrieve value from");
            this.minPath = minPath;
        }
        public bool DependsOnOtherProperty() => minPath != null;
    }
}