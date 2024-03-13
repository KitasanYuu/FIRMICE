using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.HighDefinition;
using System.Collections.Generic;
using UnityEngine.Rendering;
using System.Linq;

public class SetRenderingLayerMaskEditorHD : EditorWindow
{
    private List<Renderer> renderers = new List<Renderer>();
    private Vector2 scrollPosition;
    private Dictionary<Renderer, bool> rendererSelections = new Dictionary<Renderer, bool>();
    private int layerMaskValue = 0; // Used with MaskField for selecting light layers
    private string[] layerNames; // Light Layer names from HDRP settings

    [MenuItem("YuuTools/Render/LightLayerSetter")]
    private static void ShowWindow()
    {
        var window = GetWindow<SetRenderingLayerMaskEditorHD>("Set Light Layers HD");
        window.InitializeLayerNames();
        window.Show();
    }

    private void InitializeLayerNames()
    {
        var currentPipelineAsset = GraphicsSettings.currentRenderPipeline as HDRenderPipelineAsset;
        if (currentPipelineAsset != null)
        {
            layerNames = currentPipelineAsset.currentPlatformRenderPipelineSettings.renderingLayerNames;
        }
        else
        {
            layerNames = new[] { "Layer names not available" };
        }
    }

    void OnGUI()
    {
        GUILayout.Label("Drag GameObjects here to adjust their Light Layers", EditorStyles.boldLabel);

        // Drag and drop area
        Rect dropArea = GUILayoutUtility.GetRect(0f, 50f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Drag GameObjects Here");
        HandleDragAndDrop(dropArea);

        // Buttons for select all / deselect all
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Select All"))
        {
            SetAllRendererSelections(true);
        }
        if (GUILayout.Button("Deselect All"))
        {
            SetAllRendererSelections(false);
        }
        EditorGUILayout.EndHorizontal();

        // Renderer list with toggles
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        foreach (var renderer in renderers)
        {
            EditorGUILayout.BeginHorizontal();
            rendererSelections[renderer] = EditorGUILayout.Toggle(rendererSelections[renderer], GUILayout.Width(20));
            EditorGUILayout.ObjectField(renderer.gameObject.name, renderer, typeof(Renderer), true);
            EditorGUILayout.LabelField(GetLightLayerNames(renderer.renderingLayerMask));
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        // Light Layers selection
        layerMaskValue = EditorGUILayout.MaskField("Light Layers", layerMaskValue, layerNames);
        if (GUILayout.Button("Apply Selected Light Layers"))
        {
            ApplySelectedLightLayers();
        }
    }

    private void HandleDragAndDrop(Rect dropArea)
    {
        Event evt = Event.current;
        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!dropArea.Contains(evt.mousePosition))
                    return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    AddDraggedRenderers(DragAndDrop.objectReferences);
                }
                break;
        }
    }

    private void AddDraggedRenderers(Object[] draggedObjects)
    {
        foreach (var obj in draggedObjects)
        {
            if (obj is GameObject go)
            {
                foreach (var renderer in go.GetComponentsInChildren<Renderer>(true))
                {
                    if (!renderers.Contains(renderer))
                    {
                        renderers.Add(renderer);
                        rendererSelections[renderer] = true; // Default to selected
                    }
                }
            }
        }
    }

    private void SetAllRendererSelections(bool select)
    {
        foreach (var key in rendererSelections.Keys.ToList())
        {
            rendererSelections[key] = select;
        }
    }

    private void ApplySelectedLightLayers()
    {
        foreach (var renderer in renderers)
        {
            if (rendererSelections.TryGetValue(renderer, out bool selected) && selected)
            {
                renderer.renderingLayerMask = (uint)layerMaskValue;
                EditorUtility.SetDirty(renderer);
            }
        }
    }

    private string GetLightLayerNames(uint layerMask)
    {
        List<string> names = new List<string>();
        for (int i = 0; i < layerNames.Length; i++)
        {
            if ((layerMask & (1 << i)) != 0)
            {
                names.Add(layerNames[i]);
            }
        }
        return string.Join(", ", names);
    }
}
