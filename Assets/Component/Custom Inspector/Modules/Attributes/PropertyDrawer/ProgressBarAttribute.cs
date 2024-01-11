using System;
using System.Diagnostics;
using UnityEngine;


namespace CustomInspector
{
    /// <summary>
    /// Displays a progress bar instead of your number. Progressbar is full of you reached given max
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public class ProgressBarAttribute : PropertyAttribute
    {
        public Size size;

        public readonly string minGetter = null;
        public readonly float min;
        public readonly string maxGetter = null;
        public readonly float max;

        public bool isReadOnly = false;

        public ProgressBarAttribute(float max)
        {
            this.max = max;
        }
        [Obsolete("Use syntax ProgressBar(float max, size = Size.medium) instead of ProgressBar(float max, Size.medium)")]
        public ProgressBarAttribute(float max, Size size = Size.medium)
        {
            this.max = max;
            this.size = size;
        }
        public ProgressBarAttribute(float min, float max)
        {
            this.min = min;
            this.max = max;
        }
        public ProgressBarAttribute(string minGetter, float max)
        {
            this.minGetter = minGetter;
            this.max = max;
        }
        public ProgressBarAttribute(float min, string maxGetter)
        {
            this.min = min;
            this.maxGetter = maxGetter;
        }
        public ProgressBarAttribute(string minGetter, string maxGetter)
        {
            this.minGetter = minGetter;
            this.maxGetter = maxGetter;
        }
    }
}
