using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class RendrSwitcher : EditorWindow
{
    private GameObject rootObject;
    private List<Renderer> renderers = new List<Renderer>();
    private Vector2 scrollPos;

    [MenuItem("YuuTools/Render/RendrSwitcher")]
    public static void ShowWindow()
    {
        GetWindow<RendrSwitcher>("RendrSwitcher");
    }

    void OnGUI()
    {
        GUILayout.Label("Root Object", EditorStyles.boldLabel);
        rootObject = (GameObject)EditorGUILayout.ObjectField(rootObject, typeof(GameObject), true);

        if (rootObject != null)
        {
            if (GUILayout.Button("Find Renderers"))
            {
                FindRenderers();
            }

            if (renderers.Count > 0)
            {
                if (GUILayout.Button("Enable All Renderers"))
                {
                    ToggleRenderers(true);
                }

                if (GUILayout.Button("Disable All Renderers"))
                {
                    ToggleRenderers(false);
                }

                GUILayout.Label("Found Renderers:", EditorStyles.boldLabel);
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandHeight(true));
                foreach (Renderer renderer in renderers)
                {
                    EditorGUILayout.ObjectField(renderer.gameObject.name, renderer, typeof(Renderer), true);
                }
                EditorGUILayout.EndScrollView();
            }
        }
    }

    void FindRenderers()
    {
        renderers.Clear();
        if (rootObject != null)
        {
            Renderer[] foundRenderers = rootObject.GetComponentsInChildren<Renderer>(true);
            renderers.AddRange(foundRenderers);
        }
        Debug.Log($"Found {renderers.Count} Renderers.");
    }

    void ToggleRenderers(bool enable)
    {
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = enable;
        }
        Debug.Log($"{(enable ? "Enabled" : "Disabled")} {renderers.Count} Renderers.");
    }
}
