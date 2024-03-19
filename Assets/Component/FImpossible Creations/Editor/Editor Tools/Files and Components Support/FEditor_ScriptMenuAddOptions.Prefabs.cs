#if UNITY_2019_4_OR_NEWER
using System.IO;
using UnityEditor;
using UnityEditor.Hardware;
using UnityEngine;

namespace FIMSpace.FEditor
{
    /// <summary>
    /// FM: Class with basic tools for working in Unity Editor level
    /// </summary>
    public static partial class FEditor_MenuAddOptions
    {

        [MenuItem("Assets/Utilities/Create Prefab and Add Collider", true)]
        private static bool CreatePrefabOutOfModelAssetCollCheck(MenuCommand menuCommand)
        { return IsAnyPrefabable(Selection.objects); }

        [MenuItem("Assets/Utilities/Create Prefab", true)]
        private static bool CreatePrefabOutOfModelAssetCheck(MenuCommand menuCommand)
        { return IsAnyPrefabable(Selection.objects); }


        [MenuItem("Assets/Utilities/Create Prefab and Add Collider")]
        private static void CreatePrefabOutOfModelAssetColl(MenuCommand menuCommand)
        {
            if (Selection.objects.Length == 0) return;

            for (int i = 0; i < Selection.objects.Length; i++)
            {
                Object ob = Selection.objects[i];
                var type = PrefabUtility.GetPrefabAssetType(ob);
                if (type == PrefabAssetType.NotAPrefab || type == PrefabAssetType.MissingAsset) continue;

                string directory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(ob));
                GameObject toSave = GeneratePrePrefabObject(ob);

                if (toSave == null) return;

                MeshFilter f = toSave.GetComponentInChildren<MeshFilter>();
                if (f == null) f = FTransformMethods.FindComponentInAllChildren<MeshFilter>(toSave.transform);

                if (f)
                    f.gameObject.AddComponent<BoxCollider>();
                else
                    toSave.AddComponent<BoxCollider>();

                directory = Path.Combine(directory, toSave.name + ".prefab");
                PrefabUtility.SaveAsPrefabAsset(toSave, directory);

                if (toSave) GameObject.DestroyImmediate(toSave);
            }
        }


        [MenuItem("Assets/Utilities/Create Prefab")]
        private static void CreatePrefabOutOfModelAsset(MenuCommand menuCommand)
        {
            if (Selection.objects.Length == 0) return;

            for (int i = 0; i < Selection.objects.Length; i++)
            {
                Object ob = Selection.objects[i];
                var type = PrefabUtility.GetPrefabAssetType(ob);
                if (type == PrefabAssetType.NotAPrefab || type == PrefabAssetType.MissingAsset) continue;

                string directory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(ob));

                GameObject toSave = GeneratePrePrefabObject(ob);

                directory = Path.Combine(directory, toSave.name + ".prefab");
                PrefabUtility.SaveAsPrefabAsset(toSave, directory);

                if (toSave) GameObject.DestroyImmediate(toSave);
            }
        }

        [MenuItem("Assets/Utilities/Create Material with this as Default Texture", true)]
        private static bool CreateMaterialWithTexCheck(MenuCommand menuCommand)
        { return IsAnyTexture(Selection.objects); }

        [MenuItem("Assets/Utilities/Create Material with this as Default Texture", false)]
        private static void CreateMaterialWithTex(MenuCommand menuCommand)
        {
            if (!IsAnyTexture(Selection.objects)) return;
            if (Selection.objects.Length == 0) return;

            Shader defSh = Shader.Find("Standard");

            if (defSh is null)
            {
                UnityEngine.Debug.Log("No Default Shader!");
                return;
            }

            for (int i = 0; i < Selection.objects.Length; i++)
            {
                Object ob = Selection.objects[i];

                TextureImporter texImp = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(ob));
                if (texImp is null) continue;

                string directory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(ob));

                Material newMat = new Material(defSh);
                newMat.SetTexture("_MainTex", (Texture2D)ob);
                newMat.SetFloat("_Glossiness", 0f);

                newMat.name = ClearMaterialTypeNames(ob.name);

                directory = Path.Combine(directory, newMat.name + ".mat");
                AssetDatabase.CreateAsset(newMat, directory);

            }
        }

        [MenuItem("Assets/Utilities/Sub-Assets/Destroy Sub Asset", true)]
        private static bool DestroySubAssetCheck(MenuCommand menuCommand)
        { return AssetDatabase.IsSubAsset(Selection.objects[0]); }

        [MenuItem("Assets/Utilities/Sub-Assets/Destroy Sub Asset", false)]
        private static void DestroySubAsset(MenuCommand menuCommand)
        {
            if (Selection.objects.Length == 0) return;
            if (AssetDatabase.IsSubAsset(Selection.objects[0]) == false) return;
            GameObject.DestroyImmediate(Selection.objects[0], true);
        }

        [MenuItem("Assets/Utilities/Sub-Assets/Unhide All Sub Assets", false)]
        private static void UnhideSubAssets(MenuCommand menuCommand)
        {
            if (Selection.objects.Length == 0) return;
            var allAtPath = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(Selection.objects[0]));

            for (int i = 0; i < allAtPath.Length; i++)
            {
                if (allAtPath[i].hideFlags != HideFlags.HideInHierarchy) continue;
                //if (AssetDatabase.IsSubAsset(allAtPath[i]) == false) continue;
                allAtPath[i].hideFlags = HideFlags.None;
                EditorUtility.SetDirty(allAtPath[i]);
            }

            EditorUtility.SetDirty(Selection.objects[0]);
            AssetDatabase.SaveAssets();
        }


        [MenuItem("Assets/Utilities/Sub-Assets/Hide All Sub Assets", false)]
        private static void HideSubAssets(MenuCommand menuCommand)
        {
            if (Selection.objects.Length == 0) return;
            var allAtPath = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(Selection.objects[0]));

            for (int i = 0; i < allAtPath.Length; i++)
            {
                if (AssetDatabase.IsSubAsset(allAtPath[i]) == false) continue;
                allAtPath[i].hideFlags = HideFlags.HideInHierarchy;
                EditorUtility.SetDirty(allAtPath[i]);
            }

            EditorUtility.SetDirty(Selection.objects[0]);
            AssetDatabase.SaveAssets();
        }


        private static string ClearMaterialTypeNames(string name)
        {
            name = name.Replace("Albedo", "");
            name = name.Replace("ALBEDO", "");
            name = name.Replace("Texture", "Material");
            name = name.Replace("TEXTURE", "MATERIAL");
            name = name.Replace("Diffuse", "");
            name = name.Replace("Normal", "");
            name = name.Replace("TEX", "MAT");
            name = name.Replace("Tex", "Mat");
            name = name.Replace("tex", "mat");
            return name;
        }
    }
}
#endif
