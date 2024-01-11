using System;
using System.Diagnostics;
using UnityEngine;

namespace CustomInspector
{
    /// <summary>
    /// Defines how multiple boolean conditions are evaluated in ShowIfAttribute and ShowIfNotAttribute
    /// </summary>
    public enum BoolOperator
    {
        /// <summary> Check if all given bool values are true </summary>
        And,
        /// <summary> Check if all one of given bool values is true </summary>
        Or,
    }
    /// <summary>
    /// Defines how object values are translated to boolean values in ShowIfAttribute and ShowIfNotAttribute
    /// </summary>
    public enum ComparisonOp
    {
        /// <summary> Check if all given field values are equal </summary>
        Equals,
        /// <summary> Check if all given field values are not null </summary>
        NotNull,
        /// <summary> Check if all given field values are null </summary>
        Null,
    }
    /// <summary>
    /// Some common conditions for the use in ShowIfAttribute and ShowIfNotAttribute
    /// </summary>
    public enum StaticConditions
    {
        /// <summary> If you are currently playing </summary>
        IsPlaying,
        /// <summary> If you are currently not playing </summary>
        IsNotPlaying,
        True,
        False,
        /// <summary> If current object (interpreted as Monobehaviour) is enabled </summary>
        IsEnabled,
        /// <summary> If current object (interpreted as Monobehaviour) is enabled and its gameobject is active </summary>
        IsActiveAndEnabled,
    }

    /// <summary>
    /// Looks, whether the bool/method with given name(s) is/are true - Otherwise it will be greyed out or hidden.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public class ShowIfAttribute : PropertyAttribute
    {
        public const float indentation = 15;

        public readonly BoolOperator? op;
        public readonly ComparisonOp? comOp;

        /// <summary> null-path's get ignored </summary>
        public readonly string[] conditionPaths;
        public DisabledStyle style = DisabledStyle.Invisible;
        public int indent = 1;
        public bool Invert { get; protected set; } = false;

        private ShowIfAttribute()
        {
            order = -10;
        }
        protected ShowIfAttribute(params string[] conditionPaths)
        : this()
        {
            this.conditionPaths = conditionPaths;
        }
        /// <summary>
        /// Looks, whether the bool/method with given name/path is true
        /// </summary>
        /// <param name="conditionPath">Name/path of an bool/method in scope</param>
        public ShowIfAttribute(string conditionPath)
        : this(new string[1] { conditionPath })
        { }
        public ShowIfAttribute(StaticConditions condition)
        : this(condition.ToString())
        { }
        /// <summary>
        /// Looks, whether the bools/methods with given names/paths are true
        /// </summary>
        /// <param name="conditionPath">Name/path of an bool/method in scope</param>
        public ShowIfAttribute(BoolOperator op, params string[] conditionPaths)
        : this(conditionPaths)
        {
            this.op = op;
        }
        public ShowIfAttribute(BoolOperator op, StaticConditions cond1, string cond2 = null, string cond3 = null, string cond4 = null)
        : this(op, cond1.ToString(), cond2, cond3, cond4) { }
        public ShowIfAttribute(BoolOperator op, string cond1, StaticConditions cond2, string cond3 = null, string cond4 = null)
        : this(op, cond1, cond2.ToString(), cond3, cond4) { }
        public ShowIfAttribute(BoolOperator op, string cond1, string cond2, StaticConditions cond3, string cond4 = null)
        : this(op, cond1, cond2, cond3.ToString(), cond4) { }
        /// <summary>
        /// Checks values on given paths/names
        /// <param name="conditionPath">Name/path of an bool/method in scope</param>
        public ShowIfAttribute(ComparisonOp com, params string[] conditionPaths)
        : this(conditionPaths)
        {
            this.comOp = com;
        }
    }
    [Conditional("UNITY_EDITOR")]
    public class ShowIfNotAttribute : ShowIfAttribute
    {
        /// <summary>
        /// Looks, whether the bool/method with given name/path is false
        /// </summary>
        /// <param name="conditionPath">Name/path of an bool/method in scope</param>
        public ShowIfNotAttribute(string conditionPath)
        : base(conditionPath)
        {
            this.Invert = true;
        }
        public ShowIfNotAttribute(StaticConditions condition)
        : base(condition)
        {
            this.Invert = true;
        }
        /// <summary>
        /// Looks, whether the bools/methods with given names/paths are false
        /// </summary>
        /// <param name="conditionPath">Name/path of an bool/method in scope</param>
        public ShowIfNotAttribute(BoolOperator op, params string[] conditionPaths)
        : base(op, conditionPaths)
        {
            this.Invert = true;
        }
        public ShowIfNotAttribute(BoolOperator op, StaticConditions cond1, string cond2 = null, string cond3 = null, string cond4 = null)
        : base(op, cond1.ToString(), cond2, cond3, cond4)
        {
            this.Invert = true;
        }
        public ShowIfNotAttribute(BoolOperator op, string cond1, StaticConditions cond2, string cond3 = null, string cond4 = null)
        : base(op, cond1, cond2.ToString(), cond3, cond4)
        {
            this.Invert = true;
        }
        public ShowIfNotAttribute(BoolOperator op, string cond1, string cond2, StaticConditions cond3, string cond4 = null)
        : base(op, cond1, cond2, cond3.ToString(), cond4)
        {
            this.Invert = true;
        }
        /// <summary>
        /// Checks values on given paths/names
        /// <param name="conditionPath">Name/path of an bool/method in scope</param>
        public ShowIfNotAttribute(ComparisonOp com, params string[] conditionPaths)
        : base(com, conditionPaths)
        {
            this.Invert = true;
        }
    }
}