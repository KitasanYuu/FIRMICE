using System;
using System.Diagnostics;
using UnityEngine;

namespace CustomInspector
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    [Conditional("UNITY_EDITOR")]
    public class URLAttribute : PropertyAttribute
    {
        public readonly string link;
        public string label = null;
        public string tooltip = "";
        
        public URLAttribute(string link)
        {
            order = -10;
            this.link = link;
        }
    }
}