#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
namespace cowsins {

[CustomEditor(typeof(CrosshairShape))]
public class CrosshairShapeEditor : Editor
{
    override public void OnInspectorGUI()
    {
        serializedObject.Update();
        var myScript = target as CrosshairShape;

        DrawDefaultInspector();

        EditorGUILayout.Space(10f);

        if (GUILayout.Button("Save Settings as a Preset")) CowsinsUtilities.SavePreset(myScript, myScript.presetName);

        EditorGUILayout.Space(5f);

        if (GUILayout.Button("Apply Current Preset")) 
        {
            if (myScript.currentPreset != null)CowsinsUtilities.ApplyPreset(myScript.currentPreset,myScript);
                else Debug.LogError("Can´t apply a non existing preset. Please, assign your desired preset to 'currentPreset'. "); 
        }


        serializedObject.ApplyModifiedProperties();

    }
}
}
#endif