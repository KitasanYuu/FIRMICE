using UnityEngine;

namespace CustomInspector
{
    /// <summary>
    /// Only valid for StaticsDrawer! Used to fix overriding of other attributes
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class StaticsDrawerAttribute : PropertyAttribute { }

    /// <summary>
    /// This will display in the inspector all your static values
    /// </summary>
    [System.Serializable]
    public class StaticsDrawer
    {
#if UNITY_EDITOR
        [MessageBox("You are overriding the default PropertyDrawer of StaticsDrawer", MessageBoxType.Error)]
        bool b;
#endif
    }
}
