using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace CustomInspector
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    [Conditional("UNITY_EDITOR")]
    public class TitleAttribute : PropertyAttribute
    {
        public readonly string content;
        public readonly bool underlined;

        public int upperSpacing = 12;
        public string tooltip = null;
        public byte fontSize = 12;

        public TitleAttribute(string content, bool underlined = false)
        {
            order = -10;

            this.content = content;
            this.underlined = underlined;
        }
    }
}
