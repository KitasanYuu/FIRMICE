using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace CustomInspector
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    [Conditional("UNITY_EDITOR")]
    public class HideScriptReferenceAttribute : Attribute
    {
    }
}
