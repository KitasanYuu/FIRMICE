using UnityEngine;
using System.Diagnostics;
using System;

namespace CustomInspector
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    [Conditional("UNITY_EDITOR")]
    public class DecimalsAttribute : PropertyAttribute
    {
        public readonly int amount;
        public DecimalsAttribute(int amount)
        {
            order = -10;
            this.amount = amount;
        }
    }
}