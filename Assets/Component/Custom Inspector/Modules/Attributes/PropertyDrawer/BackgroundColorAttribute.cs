using System;
using System.Diagnostics;
using UnityEngine;


namespace CustomInspector
{
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public class BackgroundColorAttribute : PropertyAttribute
    {
        public readonly FixedColor color;
        public readonly float borderSize;

        /// <summary>
        /// Draw a background behind the property
        /// </summary>
        /// <param name="color"></param>
        /// <param name="borderSize">Shrinks property this many pixels in x</param>
        public BackgroundColorAttribute(FixedColor color = FixedColor.DarkGray, float borderSize = 5)
        {
            order = -10;
            this.color = color;
            this.borderSize = Mathf.Max(0, borderSize);
        }
    }
}