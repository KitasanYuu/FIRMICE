using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ComponentCarrierFinder : EditorWindow
{
    private MonoScript targetScript;
    private List<GameObject> foundObjects = new List<GameObject>();
    private Vector2 scrollPosition;

    [MenuItem("YuuTools/ObjectSeeker/Component Carrier Finder")]
    public static void ShowWindow()
    {
        GetWindow<ComponentCarrierFinder>("Component Carrier Finder");
    }

    private void OnGUI()
    {
        GUILayout.Label("Select a C# Script:", EditorStyles.boldLabel);
        MonoScript newTargetScript = EditorGUILayout.ObjectField(targetScript, typeof(MonoScript), false) as MonoScript;

        if (newTargetScript != targetScript)
        {
            targetScript = newTargetScript;
            FindObjectsWithScript();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Search Objects"))
        {
            FindObjectsWithScript();
        }

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Found Objects:");
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Select All", GUILayout.Width(100)))
        {
            SelectAllObjects();
        }
        EditorGUILayout.EndHorizontal();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        if (foundObjects.Count == 0)
        {
            GUILayout.Label("No objects found with script " + (targetScript != null ? targetScript.name : ""));
        }
        else
        {
            foreach (GameObject obj in foundObjects)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(obj, typeof(GameObject), false);
                GUILayout.Label("Layer: " + LayerMask.LayerToName(obj.layer) + ", Tag: " + obj.tag);
                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.EndScrollView();
    }

    private void FindObjectsWithScript()
    {
        foundObjects.Clear();

        if (targetScript == null)
        {
            return;
        }

        MonoBehaviour[] scriptsInScene = FindObjectsOfType<MonoBehaviour>();
        foreach (MonoBehaviour script in scriptsInScene)
        {
            if (script.GetType() == targetScript.GetClass())
            {
                foundObjects.Add(script.gameObject);
            }
        }
    }

    private void SelectAllObjects()
    {
        Selection.objects = foundObjects.ToArray();
    }
}
