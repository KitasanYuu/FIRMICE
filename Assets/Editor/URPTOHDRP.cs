using UnityEngine;
using UnityEditor;

public class MaterialConversionEditor : EditorWindow
{
    private Material urpMaterial;

    [MenuItem("YuuTools/Render/Material/Convert URP Material to HDRP")]
    public static void ShowWindow()
    {
        GetWindow<MaterialConversionEditor>("Convert URP to HDRP");
    }

    void OnGUI()
    {
        GUILayout.Label("Convert URP Material to HDRP", EditorStyles.boldLabel);

        urpMaterial = (Material)EditorGUILayout.ObjectField("URP Material", urpMaterial, typeof(Material), false);

        if (GUILayout.Button("Convert"))
        {
            if (urpMaterial != null)
            {
                ConvertMaterialToHDRP(urpMaterial);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Please select a URP Material to convert.", "OK");
            }
        }
    }

    private void ConvertMaterialToHDRP(Material material)
    {
        // Example conversion: Assigning a new HDRP Shader to the material
        // This is a simplistic approach - you'll need to map properties based on your needs
        Shader hdrpShader = Shader.Find("HDRP/Lit"); // Find the HDRP shader you need
        if (hdrpShader != null)
        {
            material.shader = hdrpShader;
            // Example of setting a property, assuming you have a color to transfer
            // material.SetColor("_BaseColor", urpMaterial.GetColor("_BaseColor"));
            Debug.Log("Material converted to HDRP.");
        }
        else
        {
            Debug.LogError("HDRP Lit shader not found.");
        }
    }
}
