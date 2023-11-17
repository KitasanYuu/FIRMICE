/// <summary>
/// This script belongs to cowsins™ as a part of the cowsins´ FPS Engine. All rights reserved. 
/// </summary>

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using TMPro;
namespace cowsins {
public class GetGameInformation : MonoBehaviour
{
    //FPS
    public bool showFPS;

    public bool showMinimumFrameRate, showMaximumFrameRate;

    [SerializeField, Range(.01f, 1f)] private float fpsRefreshRate;

    [SerializeField] private TextMeshProUGUI fpsObject; 

    [SerializeField] private Color appropriateValueColor, intermediateValueColor, badValueColor;

    private float fpsTimer;

    private float fps,minFps,maxFps;

    private string text = "";

    private void Start()
    {
        if(showFPS)
            fpsTimer = fpsRefreshRate;
        else
            Destroy(fpsObject);

        minFps = float.MaxValue;
    }

    private void Update()
    {
            if (!showFPS) return;

            fpsTimer -= Time.deltaTime;

            if (fpsTimer <= 0)
            {
                text = "";
                fps = 1.0f / Time.deltaTime;

                if (fps < minFps) minFps = fps;
                if (fps > maxFps) maxFps = fps;

                fpsTimer = fpsRefreshRate;
                
                text += "Current FPS: " + GetColoredFPSText(fps) + "\n";

                if (showMinimumFrameRate)
                    text += "Min FPS: " + GetColoredFPSText(minFps) + "\n";

                if (showMaximumFrameRate)
                    text += "Max FPS: " + GetColoredFPSText(maxFps);

                fpsObject.text = text;
            }

        }

        private string GetColoredFPSText(float fps)
        {
            Color fpsColor;

            if (fps < 15f)
            {
                fpsColor = badValueColor;
            }
            else if (fps < 45f)
            {
                fpsColor = intermediateValueColor;
            }
            else
            {
                fpsColor = appropriateValueColor;
            }

            return "<color=#" + ColorUtility.ToHtmlStringRGB(fpsColor) + ">" + fps.ToString("F0") + "</color>";
        }
    }
#if UNITY_EDITOR
[System.Serializable]
[CustomEditor(typeof(GetGameInformation))]
public class GetGameInformatioEditor : Editor
{

    override public void OnInspectorGUI()
    {
        serializedObject.Update();
        GetGameInformation myScript = target as GetGameInformation;

        EditorGUILayout.LabelField("FPS", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showFPS"));
            if(myScript.showFPS)
            {       
                EditorGUILayout.PropertyField(serializedObject.FindProperty("fpsRefreshRate"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("fpsObject"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("showMinimumFrameRate"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("showMaximumFrameRate"));
            }
        EditorGUILayout.Space(10f);
        EditorGUILayout.LabelField("COLOR", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("appropriateValueColor"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("intermediateValueColor"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("badValueColor"));

        serializedObject.ApplyModifiedProperties();

    }
}
#endif
}