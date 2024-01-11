using System;
using System.Diagnostics;
using UnityEngine;


namespace CustomInspector
{
    /// <summary>
    /// Changes a vector2 to a range with a minLimit and maxLimit
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public class AsRangeAttribute : PropertyAttribute
    {
        public readonly float minLimit;
        public readonly float maxLimit;

        public AsRangeAttribute(float minLimit, float maxLimit)
        {
            this.minLimit = minLimit;
            this.maxLimit = maxLimit;
        }
    }
}