using System;
using UnityEditor;

namespace CustomInspector.Extensions
{
    public class LabelWidthScope : IDisposable
    {
        float oldLabelWidth;

        public LabelWidthScope()
        {
            oldLabelWidth = EditorGUIUtility.labelWidth;
        }
        public LabelWidthScope(float newLabelWidth)
        {
            oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = newLabelWidth;
        }
        public void Dispose()
        {
            EditorGUIUtility.labelWidth = oldLabelWidth;
        }
    }
}