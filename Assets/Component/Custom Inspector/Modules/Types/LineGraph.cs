using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CustomInspector
{

    [System.Serializable]
    public class LineGraph
    {

        public LineGraph() { }
        public LineGraph(Vector2[] points) : base()
        {
            Points = points;
        }


#if UNITY_EDITOR
        [MessageBox("LineGraph is missing the [LineGraph]-attribute to be displayed")]
        [SerializeField, HideField] bool _;
#endif

        [SerializeField, HideInInspector, Delayed2] Vector2[] points = new Vector2[] { Vector2.zero };
        /// <summary>
        /// Vector2's sorted by ascending x-value: Please also provide it sorted ascending
        /// </summary>
        public Vector2[] Points
        {
            get => points;
            set
            {
                if (value == null)
                    throw new ArgumentException($"Points provided to {nameof(LineGraph)} must not be null");
                else if(value.Length <= 0)
                    throw new ArgumentException($"Points provided to {nameof(LineGraph)} must not be empty");

                points = value;
                SortPoints();
            }
        }
        private void SortPoints()
        {
            //checks if list is sorted, and if not: sorts it new
            //the general order should not be changed, because steps can exist
            for (int i = 0; i < points.Length - 1; i++)
            {
                if (points[i].x > points[i + 1].x)
                {
                    points = points.OrderBy(p => p.x).ToArray();
                    break;
                }
            }
        }
        /// <returns>y value of the line-graph</returns>
        public float GetYValue(float xValue)
        {
            if (points.Length <= 0)
                throw new ArgumentNullException("Line-graph has to points provided. Please add them in the inspector or per script.");

            //before first point
            if(xValue <= points[0].x)
                return points[0].y;

            //between points
            for (int i = 1; i < points.Length; i++)
            {
                if (xValue < points[i].x)
                    return Interpolation(points[i - 1], points[i]);
            }

            //after last
            return points[^1].y;


            float Interpolation(Vector2 pointA, Vector2 pointB) //assuming pointA.x is smaller than pointB.x
            {
                float progress = (xValue - pointA.x) / (pointB.x - pointA.x);
                return Mathf.LerpUnclamped(pointA.y, pointB.y, progress);
            }
        }
    }
    /// <summary>
    /// Only valid for LineGraph! Used to fix overriding of other attributes
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class LineGraphAttribute : PropertyAttribute
    {
        public const float defaultGraphHeight = 150;
        /// <summary>
        /// Display-height in the inspector in pixel
        /// </summary>
        public float graphHeight = defaultGraphHeight;

        public LineGraphAttribute()
        {
            if (graphHeight <= 0)
            {
                Debug.LogWarning($"{nameof(LineGraph)}: {nameof(graphHeight)} cannot be zero or negative");
                graphHeight = 100;
            }
        }
    }
}
