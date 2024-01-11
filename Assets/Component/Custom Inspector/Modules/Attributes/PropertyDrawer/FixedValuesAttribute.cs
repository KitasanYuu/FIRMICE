using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace CustomInspector
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    [Conditional("UNITY_EDITOR")]
    public class FixedValuesAttribute : PropertyAttribute
    {
        public object[] values;
        public FixedValuesAttribute(params object[] values)
        {
            this.values = values;
        }
    }
}
