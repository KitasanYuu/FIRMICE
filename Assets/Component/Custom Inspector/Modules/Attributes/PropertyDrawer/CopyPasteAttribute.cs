using System;
using System.Diagnostics;
using UnityEngine;


namespace CustomInspector
{
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public class CopyPasteAttribute : PropertyAttribute
    {
        public readonly bool previewClipboard;

        public CopyPasteAttribute(bool previewClipboard = true)
        {
            order = -10;
            this.previewClipboard = previewClipboard;
        }
    }
}