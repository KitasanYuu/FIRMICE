using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CustomInspector.Helpers
{
    public static class InternalEditorStylesConvert
    {
        public static GUIContent IconNameToGUIContent(string iconName)
        {
            Texture2D iconTexture = (Texture2D)typeof(EditorGUIUtility)
                            .GetMethod("LoadIcon", BindingFlags.NonPublic | BindingFlags.Static)
                            .Invoke(null, new object[] { iconName });

            if (iconTexture != null)
                return new GUIContent() { image = iconTexture };
            else
                return new GUIContent(iconName);
        }
    }
}
