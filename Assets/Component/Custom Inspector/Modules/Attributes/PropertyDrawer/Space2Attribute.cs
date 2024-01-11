using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace CustomInspector
{
    /// <summary>
    /// A version of unitys [Space]-attribute but as PropertyAttrbute instead of DecoratorDrawer
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    [Conditional("UNITY_EDITOR")]
    public class Space2Attribute : PropertyAttribute
    {
        public readonly float pixels;
        private Space2Attribute()
        {
            order = -10;
        }
        public Space2Attribute(float pixel) : this()
        {
            this.pixels = Mathf.Max(pixel, 0);
        }
    }
}
