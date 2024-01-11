using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace CustomInspector
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    [Conditional("UNITY_EDITOR")]
    public class ShowAssetReferenceAttribute : PropertyAttribute
    {
        public readonly string fileName = null;

        public ShowAssetReferenceAttribute(string fileName = null)
        {
            this.fileName = fileName;
        }
    }
}
