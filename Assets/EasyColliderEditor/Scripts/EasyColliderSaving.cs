#if(UNITY_EDITOR)
using UnityEngine;
using UnityEditor;
using System.IO;
namespace ECE
{
  public static class EasyColliderSaving
  {

    static UnityEngine.Object TryFindPrefabObject(GameObject go)
    {
      UnityEngine.Object foundObject = null;
#if UNITY_2018_2_OR_NEWER
      foundObject = PrefabUtility.GetCorrespondingObjectFromSource(go);
#else
      foundObject = PrefabUtility.GetPrefabParent(go);
      if (foundObject == null)
      {
        foundObject = PrefabUtility.FindPrefabRoot(go);
      }
#endif
      return foundObject;
    }

    static UnityEngine.Object TryFindMeshOrSkinnedMeshObject(GameObject go)
    {
      UnityEngine.Object foundObject = null;
      MeshFilter mf = go.GetComponent<MeshFilter>();
      if (mf != null)
      {
        foundObject = mf.sharedMesh;
      }
      else
      {
        SkinnedMeshRenderer smr = go.GetComponent<SkinnedMeshRenderer>();
        if (smr != null)
        {
          foundObject = smr.sharedMesh;
        }
      }
      return foundObject;
    }

    /// <summary>
    /// Static preferences asset that is currently loaded.
    /// </summary>
    /// <value></value>
    static EasyColliderPreferences ECEPreferences { get { return EasyColliderPreferences.Preferences; } }

    /// <summary>
    /// Gets a valid path to save a convex hull at to feed into save convex hull meshes function.
    /// </summary>
    /// <param name="go">selected gameobject</param>
    /// <param name="ECEPreferences">preferences object</param>
    /// <returns>full save path (ie C:/UnityProjects/ProjectName/Assets/Folder/baseObject)</returns>
    public static string GetValidConvexHullPath(GameObject go)
    {
      //TODO: IMPROVE THIS WHOLE AREA.
      // use default specified path
      // remove invalid characters from file name, just in case (user reported error, thanks!)
      string goName = string.Join("_", go.name.Split(Path.GetInvalidFileNameChars()));
      string path = ECEPreferences.SaveConvexHullPath + goName;
      // get path to go
      if (ECEPreferences.ConvexHullSaveMethod != CONVEX_HULL_SAVE_METHOD.Folder)
      {
        // bandaid for scaled temporary skinned mesh:
        // as the scaled mesh filter is added during setup with the name Scaled Mesh Filter (Temporary)
        if (go.name.Contains("Scaled Mesh Filter"))
        {
          go = go.transform.parent.gameObject; // set the gameobject to the temp's parent (as that will be a part of the prefab if it is one and thus should work.)
        }

        UnityEngine.Object foundObject = null;

        if (ECEPreferences.ConvexHullSaveMethod == CONVEX_HULL_SAVE_METHOD.Prefab)
        {
          foundObject = TryFindPrefabObject(go);
        }
        else if (ECEPreferences.ConvexHullSaveMethod == CONVEX_HULL_SAVE_METHOD.Mesh)
        {
          foundObject = TryFindMeshOrSkinnedMeshObject(go);
        }
        else if (ECEPreferences.ConvexHullSaveMethod == CONVEX_HULL_SAVE_METHOD.PrefabMesh)
        {
          foundObject = TryFindPrefabObject(go);
          if (foundObject == null)
          {
            foundObject = TryFindMeshOrSkinnedMeshObject(go);
          }
        }
        else if (ECEPreferences.ConvexHullSaveMethod == CONVEX_HULL_SAVE_METHOD.MeshPrefab)
        {
          foundObject = TryFindMeshOrSkinnedMeshObject(go);
          if (foundObject == null)
          {
            foundObject = TryFindPrefabObject(go);
          }
        }

        string altPath = AssetDatabase.GetAssetPath(foundObject);
        // but only use the path it if it exists.
        if (altPath != null && altPath != "" && foundObject != null)
        {
          int index = altPath.LastIndexOf("/");
          if (index >= 0)
          {
            string foundObjectName = string.Join("_", foundObject.name.Split(Path.GetInvalidFileNameChars()));
            path = altPath.Remove(index + 1) + foundObjectName;
          }
        }
      }




      bool badPath = false;
      // paths should be in the form of Assets/subfolders/gameobjectname/
      string assetDatabaseFolder = Application.dataPath;
      int adfIndex = path.LastIndexOf("/");
      if (adfIndex >= 0)
      {
        assetDatabaseFolder = path.Remove(adfIndex);
      }
      else { badPath = true; }

      // C:/pathtoprojectfiles/Assets/subfolders/gameobjectname
      // datapath has the /Assets on it, so we removed the "Assets" part as that's already part of the path.
      string fullPath = Application.dataPath.Remove(Application.dataPath.Length - 6) + path;

      // the directory of the full path:" C:/projectfiles/subfolders
      string directory = Application.dataPath;
      int directoryIndex = fullPath.LastIndexOf("/");
      if (directoryIndex >= 0)
      {
        directory = fullPath.Remove(directoryIndex);
      }
      else { badPath = true; }


      if (badPath || !Directory.Exists(directory) || !AssetDatabase.IsValidFolder(assetDatabaseFolder))
      {
        int assetIndex = Application.dataPath.LastIndexOf("Assets");
        string defaultPath = Application.dataPath.Remove(assetIndex, Application.dataPath.Length - assetIndex) + ECEPreferences.SaveConvexHullPath;

        if (!Directory.Exists(defaultPath))
        {
          ECEPreferences.ResetDefaultSavePath();
        }
        directory = defaultPath;
        assetDatabaseFolder = ECEPreferences.SaveConvexHullPath.Remove(ECEPreferences.SaveConvexHullPath.Length - 1, 1);
        fullPath = assetDatabaseFolder + "/" + goName;
      }


      //for debugging malformed paths, somehow bad paths can sometimes be created.
      // bool forceBadPath = true;
      // if the directory specified or found does not exist, fall back to using the location of this script.
      if (badPath || !Directory.Exists(directory) || !AssetDatabase.IsValidFolder(assetDatabaseFolder))
      {
        // save in scripts folder as a final fallback.
        fullPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(ECEPreferences));
        int fullPathIndex = fullPath.LastIndexOf("/");
        // fullPathIndex = forceBadPath ? -1 : 0;
        if (fullPathIndex >= 0)
        {
          fullPath = fullPath.Remove(fullPathIndex);
          // add an additional "Is it possible this folder has been moved" mention into the warning.
          Debug.LogWarning("Easy Collider Editor: Convex Hull save path specified in Easy Collider Editor does not exist. Saving in: " + fullPath + " as a fallback. If the folder has been moved or deleted, update to a different folder in the edit preferences foldout." + "\n\nDebug information: fp:" + fullPath + "\ndir:" + directory + "\nadf:" + assetDatabaseFolder + "\np:" + path);
          fullPath = fullPath + "/" + goName;
        }
        else
        {
          // in the event that we still somehow have a malformed path, we'll save at the default asset root and let the user know.
          Debug.LogWarning("Easy Collider Editor: Unable to save at the normal fallback path. Saving at the root Assets/ project path. Try updating the folder in the edit preferences foldout.\n\nDebug information: fp:" + fullPath + "\ndir:" + directory + "\nadf:" + assetDatabaseFolder + "\np:" + path);
          fullPath = Application.dataPath + "/" + goName;
        }
      }
      return fullPath;
    }

    /// <summary>
    /// goes thorugh the path and finds the first non-existing path that can be used to save.
    /// </summary>
    /// <param name="path">Full path up to save location: ie C:/UnityProjects/ProjectName/Assets/Folder/baseObject</param>
    /// <param name="suffix">Suffix to add to save files ie _Suffix_</param>
    /// <returns>first valid path for AssetDatabase.CreateAsset ie baseObject_Suffix_0</returns>
    private static string GetFirstValidAssetPath(string path, string suffix)
    {

      string validPath = path;
      if (File.Exists(validPath + suffix + "0.asset"))
      {
        int i = 1;
        while (File.Exists(validPath + suffix + i + ".asset"))
        {
          i += 1;
        }
        validPath += suffix + i + ".asset";
      }
      else
      {
        validPath += suffix + "0.asset";
      }
      // have "/Assets/" in the directory earlier than the unity project could still cause issues.
      // Debug.Log(path);
      int index = path.LastIndexOf("/Assets/");
      if (index >= 0)
      {
        validPath = validPath.Remove(0, index);
        validPath = validPath.Remove(0, 1);
      }
      // Debug.Log(validPath);
      return validPath;
    }

    /// <summary>
    /// Creates and saves a mesh asset in the asset database with appropriate path and suffix.
    /// </summary>
    /// <param name="mesh">mesh</param>
    /// <param name="attachTo">gameobject the mesh will be attached to, used to find asset path.</param>
    public static void CreateAndSaveMeshAsset(Mesh mesh, GameObject attachTo)
    {
      if (mesh != null && !DoesMeshAssetExists(mesh))
      {
        string savePath = GetValidConvexHullPath(attachTo);
        if (savePath != "")
        {
          string assetPath = GetFirstValidAssetPath(savePath, ECEPreferences.SaveConvexHullSuffix);
          AssetDatabase.CreateAsset(mesh, assetPath);
          AssetDatabase.SaveAssets();
        }
      }
    }

    /// <summary>
    /// Checks if the asset already exists (needed for rotate and duplicate, as the mesh is the same mesh repeated.)
    /// </summary>
    /// <param name="mesh"></param>
    /// <returns></returns>
    public static bool DoesMeshAssetExists(Mesh mesh)
    {
      string p = AssetDatabase.GetAssetPath(mesh);
      if (p == null || p.Length == 0)
      {
        return false;
      }
      return true;
    }

    /// <summary>
    /// Creates and saves an array of mesh assets in the assetdatabase at the path with the the format "savePath"+"suffix"+[0-n].asset
    /// </summary>
    /// <param name="savePath">Full path up to save location: ie C:/UnityProjects/ProjectName/Assets/Folder/baseObject</param>
    /// <param name="suffix">Suffix to add to save files ie _Suffix_</param>
    public static void CreateAndSaveMeshAssets(Mesh[] meshes, string savePath, string suffix)
    {
      for (int i = 0; i < meshes.Length; i++)
      {
        // get a new valid path for each mesh to save.
        string path = GetFirstValidAssetPath(savePath, suffix);
        try
        {
          AssetDatabase.CreateAsset(meshes[i], path);
        }
        catch (System.Exception error)
        {
          Debug.LogError("Error saving at path:" + path + ". Try changing the save CH path in Easy Collider Editor's preferences to a different folder.\n" + error.ToString());
        }
      }
      AssetDatabase.SaveAssets();
    }

  }
}
#endif
