using System;
using System.Diagnostics;
using UnityEngine;

namespace CustomInspector
{
    public enum ExecutionTarget
    {
        /// <summary>
        /// Method gets called on *every* inspector changes
        /// </summary>
        Always,
        /// <summary>
        /// Method only gets called *while playing* on inspector changes
        /// </summary>
        IsPlaying,
        /// <summary>
        /// Method only gets called when *not playing* on inspector changes
        /// </summary>
        IsNotPlaying
    }
    /// <summary>
    /// Calls the given function, when variable got changed in the unity inspector
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    [Conditional("UNITY_EDITOR")]
    public class HookAttribute : PropertyAttribute
    {
        public readonly string methodPath;
        public readonly ExecutionTarget target;

        /// <summary>
        /// Defines wheter changes in the inspector are auto applied to the properties.
        /// If set, ONLY the hook is called with the old and new value.
        /// Properties are not changing if you are not changing manually in the hook given function
        /// </summary>
        public bool useHookOnly = false;

        public HookAttribute(string methodPath, ExecutionTarget target = ExecutionTarget.Always)
        {
            this.methodPath = methodPath;
            this.target = target;
            order = -10;
        }
    }
}