using CustomInspector.Extensions;
using System;
using System.Diagnostics;
using UnityEngine;


namespace CustomInspector
{
    /// <summary>
    /// Prints an error if value is null.
    /// You can add not allowed values like "1" or "forbiddenString"
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public class ForceFillAttribute : PropertyAttribute
    {
        /// <summary>
        /// The height in the inspector of the errormessage
        /// </summary>
        public const float errorSize = 35;

        public readonly string[] notAllowed = null;

        /// <summary>
        /// This message will appear, if field value is not correct (instead of the default message: "Value of {current_value} is not valid)".
        /// </summary>
        public string errorMessage = null;
        /// <summary>
        /// Will only test field in play mode
        /// </summary>
        public bool onlyTestInPlayMode = false;

        public ForceFillAttribute()
        {
            order = -10;
        }
        /// <summary>
        /// Define additional forbidden values. 'Null' is always forbidden
        /// </summary>
        public ForceFillAttribute(params string[] notAllowed) : this()
        {
            this.notAllowed = notAllowed;
        }
    }
    /// <summary>
    /// A helper class to test in the editor, if all fields in the inspector are filled
    /// </summary>
    public static class ForceFill
    {
        /// <summary>
        /// Prints an Error, if any field in Class has Attribute [ForceFill] and is null (to check if everything got filled in the inspector)
        /// <para>It prints found fields' names and the transforms Hierarchies of the Monobehaviour</para>
        /// <para>This function is conditional: It wont cut performance in build</para>
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void CheckForceFilled(this MonoBehaviour @object)
            => @object.CheckFilled(attributeType: typeof(ForceFillAttribute));

        /// <summary>
        /// Prints an Error, if any field in Class has Attribute [ForceFill] and is null (to check if everything got filled in the inspector)
        /// <para>It prints found fields' names and the given owners Hierarchies of the Monobehaviour</para>
        /// <para>This function is conditional: It wont cut performance in build</para>
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void CheckForceFilled(this object @object, Transform owner)
            => @object.CheckFilled(owner, typeof(ForceFillAttribute));
    }
}