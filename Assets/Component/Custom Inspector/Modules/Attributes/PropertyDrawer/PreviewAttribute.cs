using System;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace CustomInspector
{

    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public class PreviewAttribute : PropertyAttribute
    {
        public readonly float thumbnailSize;

        public PreviewAttribute(Size size = Size.medium)
        {
#if UNITY_EDITOR
            order = -1; //just before the AssetsOnly
            thumbnailSize = size switch
            {
                Size.small => 2 * EditorGUIUtility.singleLineHeight, //this is the absolute minimum size
                Size.medium => 60,
                Size.big => 100,
                Size.max => 150,
                _ => throw new System.NotImplementedException($"{size}")
            };
#endif
        }
    }
}