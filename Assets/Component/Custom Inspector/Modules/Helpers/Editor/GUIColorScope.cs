using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CustomInspector.Helpers.Editor
{
    public class BackgroundColorScope : IDisposable
    {
        readonly Color prevColor;
        public BackgroundColorScope()
        {
            prevColor = GUI.backgroundColor;
        }
        public BackgroundColorScope(Color newColor)
        {
            prevColor = GUI.backgroundColor;
            GUI.backgroundColor = newColor;
        }

        public void Dispose()
        {
            GUI.backgroundColor = prevColor;
        }
    }
    public class GUIColorScope : IDisposable
    {
        readonly Color prevColor;
        public GUIColorScope()
        {
            prevColor = GUI.color;
        }
        public GUIColorScope(Color newColor)
        {
            prevColor = GUI.color;
            GUI.color = newColor;
        }

        public void Dispose()
        {
            GUI.color = prevColor;
        }
    }
}
