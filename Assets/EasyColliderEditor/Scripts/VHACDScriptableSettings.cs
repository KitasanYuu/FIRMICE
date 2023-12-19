#if (UNITY_EDITOR && !UNITY_EDITOR_LINUX)
namespace ECE
{
  using System.Collections;
  using System.Collections.Generic;
  using UnityEngine;
  using UnityEditor;
  public class VHACDScriptableSettings : ScriptableObject
  {
    [SerializeField] VHACDParameters _vhacdParameters;

    public VHACDParameters GetParameters() { return _vhacdParameters; }
    public void SetParameters(VHACDParameters parameters)
    {
      VHACDParameters copy = new VHACDParameters(parameters);
      _vhacdParameters = copy;
    }

    /// <summary>
    /// Saves the current parameters after popping up a save file panel asking where to save.
    /// </summary>
    /// <param name="vhacdParamsToSave"></param>
    public static void Save(VHACDParameters vhacdParamsToSave)
    {
      string firstAssetPath = FindFirstSOAssetPath();
      string path = EditorUtility.SaveFilePanel("Select where to save the current VHACD Settings.", firstAssetPath, "VHACDSettings", "asset");
      if (path.Contains(Application.dataPath))
      {
        path = path.Replace(Application.dataPath, "Assets");
        if (!string.IsNullOrEmpty(path))
        {
          VHACDScriptableSettings settingstoSave = ScriptableObject.CreateInstance<VHACDScriptableSettings>();
          settingstoSave.SetParameters(vhacdParamsToSave);
          AssetDatabase.CreateAsset(settingstoSave, path);
          AssetDatabase.SaveAssets();
        }
      }
    }

    /// <summary>
    /// Pops open a file panel and loads
    /// </summary>
    /// <returns></returns>
    public static VHACDScriptableSettings Load()
    {
      string firstAssetPath = FindFirstSOAssetPath(); // probably just better to load wherever it finds a vhacd settings object.
      string path = EditorUtility.OpenFilePanel("Select VHACD Settings to Load", firstAssetPath, "asset");
      path = path.Replace(Application.dataPath, "Assets");
      VHACDScriptableSettings settings = AssetDatabase.LoadAssetAtPath<VHACDScriptableSettings>(path);
      return settings;
    }

    public static string FindFirstSOAssetPath()
    {

      string[] ecp = AssetDatabase.FindAssets("t:VHACDScriptableSettings");
      string assetPath = "";
      if (ecp.Length > 0)
      {
        assetPath = AssetDatabase.GUIDToAssetPath(ecp[0]);
      }
      if (!string.IsNullOrEmpty(assetPath))
      {
        int index = assetPath.LastIndexOf("/");
        if (index >= 0)
        {
          assetPath = assetPath.Remove(index, assetPath.Length - index);
        }
      }
      return assetPath;
    }
  }
}
#endif