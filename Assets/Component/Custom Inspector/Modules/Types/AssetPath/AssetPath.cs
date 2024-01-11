using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


namespace CustomInspector
{
    /// <summary>
    /// Only valid for FolderPath or FilePath! Used to fix overriding of other attributes
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class AssetPathAttribute : PropertyAttribute { }

    /// <summary>
    /// Used for any Object whether its a folder or a file
    /// </summary>
    [System.Serializable]
    public abstract class AssetPath
    {
#if UNITY_EDITOR
        [MessageBox("You are overriding the default PropertyDrawer of AssetPath. Use the [AssetPath] attribute to fix overriding", MessageBoxType.Error)]
#endif
        [SerializeField]
        string path;
        protected const string invalidString = "<invalid>";

        /// <summary> All types accepted by this path </summary>
        public readonly Type RequiredType = typeof(Object);
        public string Name => GetFileName();

#if UNITY_EDITOR
#pragma warning disable CS0414
        [SerializeField, HideInInspector]
        private Object assetReference = null;
#pragma warning restore CS0414
#endif
        protected AssetPath(string defaultPath = invalidString)
        {
            this.path = defaultPath;
        }
        protected AssetPath(Type fileType)
        {
            this.path = invalidString;
            this.RequiredType = fileType;
        }
        protected AssetPath(string defaultPath, Type fileType)
        {
            this.path = defaultPath;
            this.RequiredType = fileType;
        }

        /// <summary>
        /// The relative path starting with 'Assets/'
        /// </summary>
        /// <exception cref="System.NullReferenceException">If no valid path was entered</exception>
        public string GetPath()
        {
            if (HasPath())
                return path;
            else
                throw new System.NullReferenceException("No valid path entered. Fill it in the Inspector!");
        }
        /// <summary>
        /// string behind the last slash of the path
        /// </summary>
        /// <exception cref="System.NullReferenceException">If no valid path was entered</exception>
        public string GetFileName()
        {
            string path = GetPath();

            Debug.Assert(path[^1] != '/' && path[^1] != '\\', "Path has no name");
            for (int i = path.Length - 2; i >= "Assets".Length; i--)
            {
                if (path[i] == '/' || path[i] == '\\')
                {
                    return path[(i + 1)..];
                }
            }
            throw new ArgumentException("Path has to start with 'Assets/'");
        }
        public Type AssetType
        {
            get
            {
#if UNITY_EDITOR
                if (HasPath())
                    return AssetDatabase.LoadAssetAtPath<Object>(path).GetType();
                else
                    return null;
#else
                throw new NotSupportedException("path is not available in build");
#endif
            }
        }
        /// <summary>
        /// path to the game data folder on the target device + relative path
        /// </summary>
        public string GetAbsolutePath()
        {
            string relativePath = GetPath()[6..]; //Remove the 'Assets'
            return Application.dataPath + relativePath;
        }

        /// <summary>
        /// Change the current path
        /// </summary>
        /// <returns>True, if current path is valid</returns>
        public bool SetPath(string path)
        {
            this.path = path;
            return HasPath();
        }

        /// <summary>
        /// If a path is filled
        /// </summary>
        /// <returns></returns>
        public bool HasPath()
            => IsValidPath(path, RequiredType);

        public bool HasEnding(string ending)
        {
            string path = GetPath();

            if (ending == "*")
                return true;

            if(ending.Length > path.Length)
                return false;
            int ind = path.LastIndexOf('.');
            if (ind == -1)
                return ending == "";
            else
                return path[(ind + 1)..] == ending;
        }


        public static bool IsValidPath(string path, Type fileType)
        {
#if UNITY_EDITOR
            if(path == invalidString)
                return false;

            if(fileType == typeof(Folder))
            {
                return AssetDatabase.IsValidFolder(path);
            }
            else
            {
                return AssetDatabase.LoadAssetAtPath<Object>(path) != null && !AssetDatabase.IsValidFolder(path);
            }
#else
            throw new NotSupportedException("path is not available in build");
#endif
        }

        public override string ToString()
        {
            if (HasPath())
            {
                return $"AssetPath({path})";
            }
            else
            {
                return "AssetPath(empty)";
            }
        }
        public static string AbsoluteToRelativePath(string absolute)
        {
            int startIndex = Application.dataPath.Length - "Assets".Length;
            return absolute[startIndex..];
        }
    }
}
