using Kinemation.Recoilly.Runtime;
using UnityEditor;
using UnityEngine;

namespace Kinemation.Recoilly.Editor
{
    [CustomEditor(typeof(CoreAnimComponent))]
    public class LookComponentEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            var look = (CoreAnimComponent) target;

            if (GUILayout.Button("Setup bones"))
            {
                look.SetupBones();
            }
        }
    }
}
