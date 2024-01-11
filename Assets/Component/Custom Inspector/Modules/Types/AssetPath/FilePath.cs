using System;
using UnityEditor;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using System.IO;
#endif

namespace CustomInspector
{
    /// <summary>
    /// Used for 'real' Objects (that are not Folders)
    /// <para>Use the AssetPathTypeAttribute to specify the objectType</para>
    /// </summary>
    [System.Serializable]
    public class FilePath : AssetPath
    {
        public FilePath(string defaultPath = invalidString)
        : base(defaultPath, typeof(Object)) { }
        public FilePath(string defaultPath, Type fileType)
        : base(defaultPath, fileType) { }
        public FilePath(Type fileType, string defaultPath = invalidString)
        : base(defaultPath, fileType) { }


        public T LoadAsset<T>() where T : Object
        {
#if UNITY_EDITOR
            return (T)AssetDatabase.LoadAssetAtPath(GetPath(), RequiredType);
#else
            throw new NotSupportedException("Loading assets in build is not allowed");
#endif
        }

        /// <summary>
        /// Replace current asset on path with new one
        /// </summary>
        /// <param name="asset">the new asset</param>
        /// <exception cref="System.NullReferenceException">If the path or asset is null</exception>
        public void OverrideAsset(Object asset)
        {
#if UNITY_EDITOR
            //check
            if (asset is null)
                throw new System.NullReferenceException("asset is null");

            string path = GetPath();
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(asset, path);

#else
            throw new NotSupportedException("Creating assets in build is not allowed");
#endif
        }
        /// <returns>the content of the file as text</returns>
        public string ReadAllText()
        {
#if UNITY_EDITOR
            return File.ReadAllText(GetAbsolutePath());
#else
            throw new NotSupportedException("Reading assets in build is not allowed");
#endif
        }
        /// <returns>the content of the file as lines of text. Note: The lines do not contain a newline-character</returns>
        public string[] ReadAllLines()
        {
#if UNITY_EDITOR
            return File.ReadAllLines(GetAbsolutePath());
#else
            throw new NotSupportedException("Reading assets in build is not allowed");
#endif
        }
        public void OverrideAllText(string content)
        {
#if UNITY_EDITOR
            File.WriteAllText(GetAbsolutePath(), content);
            AssetDatabase.Refresh();
#else
            throw new NotSupportedException("Loading assets in build is not allowed");
#endif
        }
        public void AppendAllText(string content)
        {
#if UNITY_EDITOR
            File.AppendAllText(GetAbsolutePath(), content);
            AssetDatabase.Refresh();
#else
            throw new NotSupportedException("Loading assets in build is not allowed");
#endif
        }
    }
}
