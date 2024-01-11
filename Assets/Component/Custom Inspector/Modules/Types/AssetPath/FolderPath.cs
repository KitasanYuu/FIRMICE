using System.Collections.Generic;
using System;
using UnityEditor;
using Object = UnityEngine.Object;
using System.Linq;

#if UNITY_EDITOR
using System.IO;
#endif

namespace CustomInspector
{

    public class Folder { }

    /// <summary>
    /// Used for Folders
    /// </summary>
    [System.Serializable]
    public class FolderPath : AssetPath
    {
        public FolderPath(string defaultPath = invalidString)
        : base(defaultPath, typeof(Folder)) { }


        public void CreateAsset(Object asset, string assetName)
        {
#if UNITY_EDITOR
            //check
            if (asset is null)
                throw new System.NullReferenceException("asset is null");
            if (string.IsNullOrEmpty(assetName))
                throw new System.ArgumentException("assetName empty");
            //Check extension
            {
                int ind = assetName.LastIndexOf('.');
                if (ind == -1)
                    throw new ArgumentException($"{assetName} is missing an extension like '.asset'");
                else if (ind >= assetName.Length - 1)
                    throw new ArgumentException($"extension on {assetName} is empty");
            }

            AssetDatabase.CreateAsset(asset, GetPath() + "/" + assetName);
#else
            throw new NotSupportedException("Creating assets in build is not allowed");
#endif
        }
        public void DeleteAsset(string assetName)
            => DeleteAsset(assetName, typeof(Object));
        public void DeleteAsset(string assetName, Type assetType)
        {
#if UNITY_EDITOR
            //check
            if (string.IsNullOrEmpty(assetName))
                throw new System.ArgumentException("assetName empty");

            string filePath = GetPath() + "/" + assetName;
            if (AssetDatabase.LoadAssetAtPath(filePath, assetType) != null)
                AssetDatabase.DeleteAsset(filePath);
            else
            {
                if (assetType == typeof(Object))
                    throw new ArgumentException($"Asset at {filePath} not found");
                else
                    throw new ArgumentException($"Asset at {filePath} of type {assetType} not found");
            }
#else
            throw new NotSupportedException("Deleting assets in build is not allowed");
#endif
        }
        public T LoadAsset<T>(string assetName) where T : Object
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(assetName))
                throw new System.ArgumentException("assetName empty");

            return AssetDatabase.LoadAssetAtPath<T>(GetPath() + "/" + assetName);
#else
            throw new NotSupportedException("Loading assets in build is not allowed");
#endif
        }
        public Object LoadAsset(string assetName, Type type)
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(assetName))
                throw new System.ArgumentException("assetName empty");

            return AssetDatabase.LoadAssetAtPath(GetPath() + "/" + assetName, type);
#else
            throw new NotSupportedException("Loading assets in build is not allowed");
#endif
        }
        /// <summary>
        /// All subdirectories
        /// </summary>
        public List<FolderPath> GetSubFolders()
        {
#if UNITY_EDITOR
            return AssetDatabase.GetSubFolders(GetPath()).Select(_ => new FolderPath(_)).ToList();
#else
            throw new NotSupportedException("Loading assets in build is not allowed");
#endif
        }
        /// <summary>
        /// All files inside this folder. Note: No folders or files in subfolders will be returned
        /// </summary>
        public List<FilePath> GetFiles(string fileEnding = "*")
        {
#if UNITY_EDITOR
            var paths = Directory.GetFiles(GetAbsolutePath(), "*", SearchOption.TopDirectoryOnly)
                            .Select(_ => AssetPath.AbsoluteToRelativePath(_))
                            .Select(_ => new FilePath(_))
                            .Where(_ => _.HasPath()); //remove the .meta files (and other invisible files)
            if (fileEnding == "*")
                return paths.ToList();
            else
                return paths.Where(_ => _.HasEnding(fileEnding)).ToList();
#else
            throw new NotSupportedException("Loading assets in build is not allowed");
#endif
        }
        /// <summary>
        /// All files inside this folder including in subfolders
        /// </summary>
        public List<FilePath> GetAllFiles(string fileEnding = "*")
        {
#if UNITY_EDITOR
            var paths = AssetDatabase.FindAssets("", new string[] { GetPath() })
                            .Select(_ => new FilePath(AssetDatabase.GUIDToAssetPath(_)))
                            .Where(_ => _.HasPath());
            if(fileEnding == "*")
                return paths.ToList();
            else
                return paths.Where(_ => _.HasEnding(fileEnding)).ToList();
#else
            throw new NotSupportedException("Loading assets in build is not allowed");
#endif
        }
        public void CreateOrReplaceFileAllText(string fileName, string content)
        {
#if UNITY_EDITOR
            string path = GetAbsolutePath() + '/' + fileName;
            File.WriteAllText(GetAbsolutePath() + '/' + fileName, content);
            AssetDatabase.Refresh();
#else
            throw new NotSupportedException("Editing assets in build is not allowed");
#endif
        }
        /// <summary>
        /// Create file using File.WriteAllText
        /// </summary>
        public void CreateFileAllText(string fileName, string content)
        {
#if UNITY_EDITOR
            string path = GetAbsolutePath() + '/' + fileName;
            if (!File.Exists(path))
            {
                File.WriteAllText(GetAbsolutePath() + '/' + fileName, content);
                AssetDatabase.Refresh();
            }
            else
            {
                throw new IOException($"File {fileName} already exists on {path}");
            }
#else
            throw new NotSupportedException("Editing assets in build is not allowed");
#endif
        }
        /// <summary>
        /// Overrides a file using File.WriteAllText
        /// </summary>
        public void OverrideFileAllText(string fileName, string content)
        {
#if UNITY_EDITOR
            string path = GetAbsolutePath() + '/' + fileName;
            if (File.Exists(path))
            {
                File.WriteAllText(GetAbsolutePath() + '/' + fileName, content);
                AssetDatabase.Refresh();
            }
            else
            {
                throw new IOException($"File {fileName} does not exist on {path}");
            }
#else
            throw new NotSupportedException("Editing assets in build is not allowed");
#endif
        }
        /// <summary>
        /// appends text on a file
        /// </summary>
        public void AppendFileAllText(string fileName, string content)
        {
#if UNITY_EDITOR
            string path = GetAbsolutePath() + '/' + fileName;
            if (File.Exists(path))
            {
                File.AppendAllText(GetAbsolutePath() + '/' + fileName, content);
                AssetDatabase.Refresh();
            }
            else
            {
                throw new IOException($"File {fileName} does not exist on {path}");
            }
#else
            throw new NotSupportedException("Editing assets in build is not allowed");
#endif
        }
    }
}
