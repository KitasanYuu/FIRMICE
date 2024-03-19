#if UNITY_2019_4_OR_NEWER
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FEditor
{
    /// <summary>
    /// FM: Class with basic tools for working in Unity Editor level
    /// </summary>
    public static partial class FEditor_MenuAddOptions
    {

#region Prefixes

        private static string GetPrefix(UnityEngine.Object o, string path)
        {
            AssetImporter a = AssetImporter.GetAtPath(path);
            if (a == null) return "";

            if (HaveAnyPrefix(o.name)) return "";

            string targetPrefix = "";
            if (PrefabUtility.IsPartOfAnyPrefab(o))
            {
                PrefabAssetType type = PrefabUtility.GetPrefabAssetType(o);
                if (type == PrefabAssetType.Regular) return "PR_";
                else if (type == PrefabAssetType.Variant) return "PR_V_";
            }

            if (a is TextureImporter) targetPrefix = "TEX_";
            else if (a is AudioImporter) targetPrefix = "AC_";
            else if (a is ModelImporter)
            {
                PrefabAssetType type = PrefabUtility.GetPrefabAssetType(o);
                if (type == PrefabAssetType.Regular || type == PrefabAssetType.Variant)
                    targetPrefix = "PR_";
                else
                    targetPrefix = "";// "MDL_";
            }
            else if (a is ShaderImporter) targetPrefix = "SH_";
            else if (o is Material) targetPrefix = "MAT_";

            if (HavePrefix(o.name, targetPrefix)) return "";

            return targetPrefix;
        }

        private static bool HaveAnyPrefix(string sourceName)
        {
            if (sourceName.Length > 3) if (sourceName[2] == '_') return true;
            if (sourceName.Length > 4) if (sourceName[3] == '_') return true;
            if (sourceName.Length > 5) if (sourceName[4] == '_') return true;
            return false;
        }

        private static bool HavePrefix(string sourceName, string targetPrefix)
        {
            if (sourceName.StartsWith(targetPrefix)) return true;
            return false;
        }

#endregion


#region Helper Prefabs Methods

        static bool IsAnyPrefabable(Object[] list)
        {
            if (list.Length == 0) return false;

            for (int i = 0; i < list.Length; i++)
            {
                Object ob = Selection.objects[i];
                var type = PrefabUtility.GetPrefabAssetType(ob);
                if (type == PrefabAssetType.NotAPrefab || type == PrefabAssetType.MissingAsset) continue;
                return true;
            }

            return false;
        }

        static GameObject GeneratePrePrefabObject(Object ob)
        {
            var type = PrefabUtility.GetPrefabAssetType(ob);
            if (type == PrefabAssetType.NotAPrefab || type == PrefabAssetType.MissingAsset) return null;

            string path = AssetDatabase.GetAssetPath(ob);

            GameObject toSave = (GameObject)PrefabUtility.InstantiatePrefab(ob);

            toSave.name = "PR_" + Path.GetFileNameWithoutExtension(path);

            return toSave;
        }

        static bool IsAnyTexture(Object[] list)
        {
            if (list.Length == 0) return false;

            for (int i = 0; i < list.Length; i++)
            {
                Object ob = Selection.objects[i];
                if (AssetDatabase.Contains(ob) == false) continue;
                if (ob is Texture2D ) return true;
            }

            return false;
        }


        #endregion

    }
}
#endif