using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace cowsins {
[CreateAssetMenu(fileName = "NewProceduralShot", menuName = "COWSINS/New Procedural Shot Effect", order = 2)]
public class ProceduralShot_SO : ScriptableObject
{
    [Range(0,2)]public float playSpeed; 

    [System.Serializable]
    public struct Translation
    {
        [Tooltip("Translation on the X axis. The movement follows this Animation Curves from 0 to 1")]public AnimationCurve xTranslation;

        [Tooltip("Translation on the Y axis. The movement follows this Animation Curves from 0 to 1")] public AnimationCurve yTranslation;

        [Tooltip("Translation on the Z axis. The movement follows this Animation Curves from 0 to 1")] public AnimationCurve zTranslation;
    }
    [System.Serializable]
    public struct Rotation
    {
        [Tooltip("Rotation on the X axis. The movement follows this Animation Curves from 0 to 1")] public AnimationCurve xRotation;

        [Tooltip("Rotation on the Y axis. The movement follows this Animation Curves from 0 to 1")] public AnimationCurve yRotation;

        [Tooltip("Rotation on the Z axis. The movement follows this Animation Curves from 0 to 1")] public AnimationCurve zRotation;
    }

    public Translation translation;

    [Min(0)]public Vector3 translationDistance;

    [Range(0, 1)] public float aimingTranslationMultiplier; 

    public Rotation rotation;

    [Min(0)] public Vector3 rotationDistance;

    [Range(0, 1)] public float aimingRotationMultiplier;
}


#if UNITY_EDITOR
[System.Serializable]
[CustomEditor(typeof(ProceduralShot_SO))]
public class ProceduralShot_SOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        Weapon_SO myScript = target as Weapon_SO;

        Texture2D myTexture = Resources.Load<Texture2D>("CustomEditor/proceduralShot_CustomEditor") as Texture2D;
        GUILayout.Label(myTexture); 

        EditorGUILayout.BeginVertical();


        EditorGUILayout.LabelField("VARIABLES", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("playSpeed"));
        EditorGUILayout.Space(10f);

        EditorGUILayout.LabelField("EFFECT CUSTOMIZATION", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("translation"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("translationDistance"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("aimingTranslationMultiplier"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("rotation"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationDistance")); 
        EditorGUILayout.PropertyField(serializedObject.FindProperty("aimingRotationMultiplier"));
        EditorGUILayout.EndVertical(); 
        
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
}