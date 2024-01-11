using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace CustomInspector
{
    /// <summary>
    /// Only valid for DynamicSlider! Used to fix overriding of other attributes
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class DynamicSliderAttribute : PropertyAttribute { }

    /// <summary>
    /// If min and max are allowed to change
    /// </summary>
    public enum FixedSide
    {
        None,
        FixedMin,
        FixedMax
    }

    /// <summary>
    /// Draws in the inspector a slider (like with range attribute)
    /// But the sliders min and max value can be changed
    /// </summary>
    [System.Serializable]
    public class DynamicSlider
    {
        [MessageBox("You are overriding the default PropertyDrawer of DynamicSlider. Use the [DynamicSlider] attribute to fix overriding", MessageBoxType.Error)]

        [SerializeField]
        public float value;

#if UNITY_EDITOR
        /// <summary>
        /// The current min and max of the slider
        /// </summary>
        [SerializeField]
        float min, max;

        /// <summary>
        /// Initial values of min and max from the constructor
        /// </summary>
        readonly float defaultMin, defaultMax;

        /// <summary>
        /// The side that cannot be changed in the inspector
        /// </summary>
        readonly FixedSide fixedSide;
#endif

        public DynamicSlider(float value, float min, float max, FixedSide fixedSide = FixedSide.None)
        {
            this.value = value;
#if UNITY_EDITOR
            this.min = defaultMin = min;
            this.max = defaultMax = max;
            if (defaultMin == defaultMax)
                Debug.LogWarning($"Your defaultMin and defaultMax are both the same value({defaultMin})");
            this.fixedSide = fixedSide;
#endif
        }

        public override string ToString()
        {
            return value.ToString();
        }

        //to float
        public static implicit operator float(DynamicSlider dynamicSlider)
        {
            return dynamicSlider.value;
        }
        //sign
        public static float operator +(DynamicSlider d) => d;
        public static float operator -(DynamicSlider d) => -d.value;

        //With self
        public static float operator +(DynamicSlider a, DynamicSlider b)
            => a.value + b.value;
        public static float operator -(DynamicSlider a, DynamicSlider b)
            => a.value - b.value;
        public static float operator *(DynamicSlider a, DynamicSlider b)
            => a.value * b.value;
        public static float operator /(DynamicSlider a, DynamicSlider b)
            => a.value / b.value;
        //With float
        public static float operator +(float f, DynamicSlider d)
            => f + d.value;
        public static float operator +(DynamicSlider d, float f)
            => d.value + f;
        public static float operator -(float f, DynamicSlider d)
            => f - d.value;
        public static float operator -(DynamicSlider d, float f)
            => d.value - f;
        public static float operator *(float f, DynamicSlider d)
            => f * d.value;
        public static float operator *(DynamicSlider d, float f)
            => d.value * f;
        public static float operator /(float f, DynamicSlider d)
            => f / d.value;
        public static float operator /(DynamicSlider d, float f)
            => d.value / f;
    }
}