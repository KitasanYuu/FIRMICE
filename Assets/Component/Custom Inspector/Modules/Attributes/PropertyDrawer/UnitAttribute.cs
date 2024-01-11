using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace CustomInspector
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    [Conditional("UNITY_EDITOR")]
    public class UnitAttribute : PropertyAttribute
    {
        public readonly string unitName;
        public UnitAttribute(string unitName)
        {
            order = -6;
            this.unitName = unitName;
        }
    }
}
