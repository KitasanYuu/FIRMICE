﻿#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace lilToon
{
    internal class lilToonAssetPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            var id = Shader.PropertyToID("_lilToonVersion");
            foreach(var path in importedAssets)
            {
                if(!path.EndsWith(".mat")) continue;
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                lilStartup.MigrateMaterial(material, id);
            }
        }
    }
}
#endif
