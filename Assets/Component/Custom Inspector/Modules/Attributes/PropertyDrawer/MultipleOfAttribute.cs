using System;
using System.Diagnostics;
using UnityEngine;

namespace CustomInspector
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    [Conditional("UNITY_EDITOR")]
    public class MultipleOfAttribute : PropertyAttribute
    {
        public readonly string stepPath = null;
        public readonly double step = 1;


        private MultipleOfAttribute()
        {
            order = -10;
        }
        /// <param name="stepPath">The name or path to a number, relative in current class</param>
        public MultipleOfAttribute(string stepPath)
        : this()
        {
            this.stepPath = stepPath;
        }
        public MultipleOfAttribute(double value)
        : this()
        {
            this.step = value;
        }
        public MultipleOfAttribute(float value)
        : this((double)value)
        { }
        public MultipleOfAttribute(int value)
        : this((double)value)
        { }
    }
    public static class MultipleOf
    {
        /// <summary>
        /// If value is multiple of step
        /// </summary>
        public static bool IsMultiple(double value, double step)
        {
            double divided = value / step;
            return divided == (int)divided;
        }
        /// <summary>
        /// If value is multiple of step
        /// </summary>
        public static bool IsMultiple(float value, float step)
            => IsMultiple((double)value, (double)step);
        /// <summary>
        /// Get nearest to value that is multiple of step
        /// </summary>
        public static double NearestMultiple(double value, double step)
        {
            double divided = value / step;
            if (divided >= 0)
                return (int)(divided + .5) * step;
            else
                return (int)(divided - .5) * step;
        }
        /// <summary>
        /// Get nearest to value that is multiple of step
        /// </summary>
        public static float NearestMultiple(float value, float step)
            => (float)NearestMultiple(Convert.ToDouble(value), Convert.ToDouble(step));
        /// <summary>
        /// Get nearest to value that is multiple of step
        /// </summary>
        public static int NearestMultiple(int value, int step)
            => (int)NearestMultiple((double)value, (double)step);
    }
}
