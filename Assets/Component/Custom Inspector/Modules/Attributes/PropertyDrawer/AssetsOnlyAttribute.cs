using System;
using System.Diagnostics;
using UnityEngine;


namespace CustomInspector
{
    /// <summary>
    /// Put this on a gameobject to forbit to fill sceneObjects
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public class AssetsOnlyAttribute : PropertyAttribute
    {

    }
}