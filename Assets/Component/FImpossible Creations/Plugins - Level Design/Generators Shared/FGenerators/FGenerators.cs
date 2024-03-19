#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using System.Reflection;

namespace FIMSpace.Generating
{
    public static class FGenerators
    {

        public static GameObject InstantiateObject(GameObject obj)
        {
#if UNITY_EDITOR
            GameObject newObj = null;

#if UNITY_2018_4_OR_NEWER
            if (Application.isPlaying == false && (PrefabUtility.GetPrefabAssetType(obj) == PrefabAssetType.Regular || PrefabUtility.GetPrefabAssetType(obj) == PrefabAssetType.Variant))
                newObj = (GameObject)PrefabUtility.InstantiatePrefab(obj);
#else
            if (Application.isPlaying == false && (PrefabUtility.GetPrefabType(obj) == PrefabType.Prefab || PrefabUtility.GetPrefabType(obj) == PrefabType.PrefabInstance))
                newObj = (GameObject)PrefabUtility.InstantiatePrefab(obj);
#endif

            if (newObj == null) newObj = GameObject.Instantiate(obj);


            return newObj;
#else
            return GameObject.Instantiate(obj);
#endif
        }

        public static void ClearGenerated<T>(List<T> generated) where T : UnityEngine.Object
        {
            if (generated == null) return;
            for (int i = 0; i < generated.Count; i++)
            {
                if (generated[i] == null) continue;

                if (generated[i] is Component)
                {
                    Component comp = generated[i] as Component;
                    DestroyObject(comp);
                    continue;
                }

                DestroyObject(generated[i]);
            }

            generated.Clear();
        }

        public static bool CheckIfIsNull(object o)
        {
#if UNITY_2018_1_OR_NEWER
            if (o is null) return true;
#else
            //if (o == null) return true;
            if (object.ReferenceEquals(o, null)) return true;
#endif

            return false;
        }

        /// <summary> Calling CheckIfExist_NOTNULL (just shorter method name) </summary>
        public static bool Exists(object o)
        {
            return CheckIfExist_NOTNULL(o);
        }

        /// <summary> Calling CheckIfExist_NOTNULL (just shorter method name) </summary>
        public static bool NotNull(object o)
        {
            return CheckIfExist_NOTNULL(o);
        }

        /// <summary> Calling CheckIfIsNull (just shorter method name) </summary>
        public static bool IsNull(object o)
        {
            return CheckIfIsNull(o);
        }

        public static bool CheckIfExist_NOTNULL(object o)
        {
            return !CheckIfIsNull(o);
        }


        public static bool IsChildOf(Transform child, Transform rootParent)
        {
            Transform tParent = child;
            while (tParent != null && tParent != rootParent)
            {
                tParent = tParent.parent;
            }

            if (tParent == null) return false; else return true;
        }


        public static void DestroyObject(UnityEngine.Object obj, bool allowDestroyAssets = false)
        {
            if (obj == null) return;

#if UNITY_EDITOR
            if (Application.isPlaying == false)
                GameObject.DestroyImmediate(obj, allowDestroyAssets);
            else
                GameObject.Destroy(obj);
#else
                GameObject.Destroy(obj);
#endif
        }


        /// <summary> Should be used instead of == null on unity classes to be able for call in async methods </summary>
        public static bool RefIsNull(object varMat)
        {
            if (CheckIfIsNull(varMat)) return true;
            if (ReferenceEquals(varMat, null)) return true;
            if (varMat == null) return true;
            if (varMat.Equals(null)) return true;
            return false;
        }


        #region Defined Seed Random Handling

        /// <summary>
        /// Class to use wirth multiple - separated random seed generators
        /// </summary>
        public class DefinedRandom
        {
            public int Seed { get; private set; }
            private System.Random random;

            #region Random Instance

            public DefinedRandom(int seed)
            {
                random = new System.Random(seed);
            }

            public void ReInitializeSeed(int seed)
            {
                random = new System.Random(seed);
            }

            public float GetRandom()
            {
                return FGenerators.GetRandom(random);
            }

            public float GetRandom(float from, float to)
            {
                return FGenerators.GetRandom(from, to, random);
            }

            public float GetRandomPlusMinus(float range)
            {
                return FGenerators.GetRandomPlusMinus(range, random);
            }

            /// <summary> To is exclusive, dedicated for arrays </summary>
            public int GetRandom(int from, int to)
            {
                return FGenerators.GetRandom(from, to, random);
            }

            public int GetRandomInclusive(int from, int to)
            {
                return FGenerators.GetRandomInclusive(from, to, random);
            }

            public int GetRandom(MinMax minMax)
            {
                return FGenerators.GetRandom(minMax, random);
            }

            public bool GetRandomFlip()
            {
                return FGenerators.GetRandomFlip( random);
            }

            #endregion
        }


        static System.Random random = new System.Random();
        public static System.Random GlobalRandomInstance { get { return random; } }
        public static int LatestSeed { get; private set; }

        public static void SetSeed(int seed)
        {
            random = new System.Random(seed);
            LatestSeed = seed;
        }

        public static bool GetRandomFlip()
        {
            return GetRandomFlip(random);
        }

        public static bool GetRandomFlip(System.Random rand)
        {
            return GetRandom(0, 2, rand) == 1;
        }

        public static float GetRandom()
        {
            return GetRandom(random);
        }

        public static int GetRandomInclusive(int from, int to)
        {
            return GetRandomInclusive(from, to, random);
        }

        public static int GetRandomInclusive(int from, int to, System.Random rand)
        {
            return rand.Next(from, to + 1);
        }

        public static float GetRandom(System.Random rand)
        {
            return (float)rand.NextDouble();
        }

        public static Vector2 SwampToBeRising(Vector2 minMax)
        {
            if (minMax.y < minMax.x)
            {
                float swapX = minMax.x;
                minMax.x = minMax.y;
                minMax.y = swapX;
            }

            return minMax;
        }

        /// <summary> Ensuring 'from' is lower value </summary>
        public static float GetRandomSwap(float from, float to, System.Random rand)
        {
            if (from > to)
            {
                float swapTo = from;
                from = to;
                to = swapTo;
            }

            return GetRandom(from, to, rand);
        }

        /// <summary> Ensuring 'from' is lower value </summary>
        public static float GetRandomSwap(float from, float to)
        {
            return GetRandomSwap(from, to, random);
        }

        public static float GetRandomPlusMinus(float plusminus)
        {
            return GetRandomPlusMinus(plusminus, random);
        }

        public static float GetRandomPlusMinus(float plusminus, System.Random rand)
        {
            return GetRandom(-plusminus, plusminus, rand);
        }

        public static float GetRandom(float from, float to)
        {
            return GetRandom(from, to, random);
        }

        public static float GetRandom(float from, float to, System.Random rand)
        {
            return from + (float)rand.NextDouble() * (to - from);
        }

        public static Vector3 GetRandom(Vector3 plusMinusRangesPerAxis)
        {
            return GetRandom(plusMinusRangesPerAxis, random);
        }

        public static Vector3 GetRandom(Vector3 plusMinusRangesPerAxis, System.Random rand)
        {
            Vector3 p = plusMinusRangesPerAxis;
            p.x = GetRandom(-p.x, p.x, rand);
            p.y = GetRandom(-p.y, p.y, rand);
            p.z = GetRandom(-p.z, p.z, rand);
            return p;
        }

        public static int GetRandom(int from, int to)
        {
            return GetRandom(from, to, random);
        }

        public static int GetRandom(int from, int to, System.Random rand)
        {
            return rand.Next(from, to);
        }

        public static int GetRandom(MinMax minMax)
        {
            return GetRandom(minMax, random);
        }

        public static int GetRandom(MinMax minMax, System.Random rand)
        {
            return (int)(minMax.Min + (float)rand.NextDouble() * ((minMax.Max + 1) - minMax.Min));
        }

        #endregion


        #region Search Related


        public static void GetIncrementalTo<T>(List<T> list) where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            if (list == null) return;
            if (list.Count == 0) return;
            T refClip = list[0];
            if (refClip == null) return;

            int indexOf = IndexOfFirstNumber(refClip.name);
            if (indexOf == -1) return;

            string path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(refClip));
            string[] files = Directory.GetFiles(path);

            List<T> clips = new List<T>();

            for (int i = 0; i < files.Length; i++)
            {
                T clip = (T)AssetDatabase.LoadAssetAtPath(files[i], typeof(T));
                if (clip) clips.Add(clip);
            }

            string nameUntilDigit = refClip.name.Substring(0, indexOf);

            List<T> foundClips = new List<T>();
            for (int i = 0; i < clips.Count; i++)
            {
                if (clips[i].name.Contains(nameUntilDigit)) foundClips.Add(clips[i]);
            }

            for (int i = 0; i < foundClips.Count; i++)
            {
                if (!list.Contains(foundClips[i])) list.Add(foundClips[i]);
            }
#endif
        }

        public static void GetSimilarTo<T>(List<T> list) where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            if (list == null) return;
            if (list.Count == 0) return;
            T refClip = list[0];
            if (refClip == null) return;

            string path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(refClip));
            string[] files = Directory.GetFiles(path);

            List<T> clips = new List<T>();

            for (int i = 0; i < files.Length; i++)
            {
                T clip = (T)AssetDatabase.LoadAssetAtPath(files[i], typeof(T));
                if (clip) clips.Add(clip);
            }

            for (int i = 0; i < clips.Count; i++)
            {
                if (!list.Contains(clips[i])) list.Add(clips[i]);
            }
#endif
        }

        public static int IndexOfFirstNumber(string name)
        {
            for (int i = 0; i < name.Length; i++)
            {
                int outer;
                if (int.TryParse(name[i].ToString(), out outer)) return i;
            }

            return -1;
        }


        #endregion


        #region GUI or Editor GUI Related


#if UNITY_EDITOR
        public static void DrawScriptableField<T>(ref T selected, string exampleFilename = "", string title = "Preset:") where T : ScriptableObject
        {
            if (selected == null) return;

            Color bg = GUI.backgroundColor;

            EditorGUILayout.BeginHorizontal();

            EditorGUIUtility.labelWidth = 60;
            selected = (T)EditorGUILayout.ObjectField(title, selected, typeof(T), false);
            if (GUILayout.Button("Create New", GUILayout.Width(94))) selected = (T)GenerateScriptable(GameObject.Instantiate(selected), exampleFilename);

            EditorGUILayout.EndHorizontal();
        }

        public static void DrawObjectList<T>(List<T> toDraw, GUIStyle style, string title, ref bool foldout, bool moveButtons = false, UnityEngine.Object toDirty = null) where T : UnityEngine.Object
        {
            if (toDraw == null) return;

            EditorGUILayout.BeginVertical(style);

            EditorGUILayout.BeginHorizontal();
            string fold = foldout ? " ▼" : " ►";
            if (GUILayout.Button(fold + "  " + title + " (" + toDraw.Count + ")", EditorStyles.label, GUILayout.Width(200))) foldout = !foldout;

            if (foldout)
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("+")) toDraw.Add(null);
            }

            EditorGUILayout.EndHorizontal();

            if (foldout)
            {
                GUILayout.Space(4);

                if (toDraw.Count > 0)
                    for (int i = 0; i < toDraw.Count; i++)
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.BeginHorizontal();

                        GUIContent lbl = new GUIContent("[" + i + "]");
                        float wdth = EditorStyles.label.CalcSize(lbl).x;

                        EditorGUILayout.LabelField(lbl, GUILayout.Width(wdth + 2));

                        toDraw[i] = (T)EditorGUILayout.ObjectField(toDraw[i], typeof(T), false);

                        if (moveButtons)
                        {
                            if (i > 0) if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowUp, "Move this element to be executed before above one"), GUILayout.Width(24))) { T temp = toDraw[i - 1]; toDraw[i - 1] = toDraw[i]; toDraw[i] = temp; }
                            if (i < toDraw.Count - 1) if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowDown, "Move this element to be executed after below one"), GUILayout.Width(24))) { T temp = toDraw[i + 1]; toDraw[i + 1] = toDraw[i]; toDraw[i] = temp; }
                        }

                        if (GUILayout.Button("X", GUILayout.Width(24))) { toDraw.RemoveAt(i); break; }

                        EditorGUILayout.EndHorizontal();
                        if (toDirty != null) if (EditorGUI.EndChangeCheck()) { EditorUtility.SetDirty(toDirty); }
                    }
                else
                {
                    EditorGUILayout.LabelField("No object in list", EditorStyles.centeredGreyMiniLabel);
                }
            }

            EditorGUILayout.EndVertical();
        }


#endif


        public static List<T> CopyList<T>(List<T> cellsInstructions)
        {
            List<T> nList = new List<T>();
            for (int i = 0; i < cellsInstructions.Count; i++) nList.Add(cellsInstructions[i]);
            return nList;
        }

        public static bool IsRightMouseButton()
        {
            if (UnityEngine.Event.current == null) return false;

            if (UnityEngine.Event.current.type == UnityEngine.EventType.Used)
                if (UnityEngine.Event.current.button == 1)
                    return true;

            return false;
        }

        /// <summary> Since remembering in which EditorGUI, EditorGUILayout, or EditorGUIUtility, or  GUILayoutUtility ahhh... in which if these classes you will find the desired variable is so confusing ¯\_(ツ)_/¯ each time when trying finding it, ending in googling for forums post with it </summary>
        public static float InspectorViewWidth()
        {
#if UNITY_EDITOR
            return EditorGUIUtility.currentViewWidth;
#else
            return 0f;
#endif

        }

        /// <summary> Since remembering in which EditorGUI, EditorGUILayout, or EditorGUIUtility, or  GUILayoutUtility ahhh... in which if these classes you will find the desired variable is so confusing ¯\_(ツ)_/¯ each time when trying finding it, ending in googling for unity doc </summary>
        public static Rect GUILastRect()
        {
#if UNITY_EDITOR
            return GUILayoutUtility.GetLastRect();
#else
            return new Rect();
#endif
        }

        /// <summary>
        /// Resetting local position, rotation, scale to zero on 1,1,1 (defaults)
        /// </summary>
        public static void ResetCoords(this Transform t)
        {
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }


        /// <summary>
        /// Working event outside OnGUI
        /// </summary>
        public static Vector2 GetEventMousePosition()
        {
#if UNITY_EDITOR
            var field = typeof(Event).GetField("s_Current", BindingFlags.Static | BindingFlags.NonPublic);
            if (field != null)
            {
                Event current = field.GetValue(null) as Event;
                if (current != null)
                {
                    return EditorWindow.focusedWindow.position.position + current.mousePosition;
                }
            }
#endif
            return Vector2.zero;
        }

#if UNITY_EDITOR
        public static string Editor_GetActiveProjectBrowserFolderPath()
        {
            MethodInfo getActiveFolderPath = typeof(ProjectWindowUtil).GetMethod(
    "GetActiveFolderPath",
    BindingFlags.Static | BindingFlags.NonPublic);

            string folderPath = (string)getActiveFolderPath.Invoke(null, null);
            return folderPath;
        }

#endif


#if UNITY_EDITOR
        public static void DropDownMenu(GenericMenu menu)
        {
            if (menu == null) return;
            if (UnityEngine.Event.current == null) return;
            menu.DropDown(new Rect(UnityEngine.Event.current.mousePosition + Vector2.left * 100, Vector2.zero));
        }
#endif


        #endregion


        #region Scriptable Related


        public static string lastPath = "";

        public static string GetLastPath
        {
            get
            {
#if UNITY_EDITOR
                if (lastPath == "")
                {
                    if (EditorPrefs.HasKey("LastFGenSaveDir"))
                    {
                        lastPath = EditorPrefs.GetString("LastFGenSaveDir");
                        if (!System.IO.File.Exists(lastPath)) lastPath = Application.dataPath;
                    }
                    else lastPath = Application.dataPath;
                }
#endif
                return lastPath;
            }
        }

        public static ScriptableObject GenerateScriptable(ScriptableObject reference, string exampleFilename = "", string playerPrefId = "LastFGenSaveDir")
        {
#if UNITY_EDITOR
            if (lastPath == "")
            {
                if (EditorPrefs.HasKey(playerPrefId))
                {
                    lastPath = EditorPrefs.GetString(playerPrefId);
                    if (!System.IO.File.Exists(lastPath)) lastPath = Application.dataPath;
                }
                else lastPath = Application.dataPath;
            }

            string path = UnityEditor.EditorUtility.SaveFilePanelInProject("Generate Preset File", exampleFilename, "asset", "Enter name of file", lastPath);

            try
            {
                if (path != "")
                {
                    lastPath = System.IO.Path.GetDirectoryName(path);
                    EditorPrefs.SetString(playerPrefId, lastPath);
                    UnityEditor.AssetDatabase.CreateAsset(reference, path);
                }
                else
                    reference = null;
            }
            catch (System.Exception)
            {
                reference = null;
                Debug.LogError("Something went wrong when creating scriptable file in your project.");
            }

#endif
            return reference;
        }

#if UNITY_EDITOR

        public static string RenamePopup(UnityEngine.Object asset, string startNameIfNull = "", bool assetDatabaseRename = true)
        {
            string startName = startNameIfNull;
            if (asset != null) startName = asset.name;
            string filename = EditorUtility.SaveFilePanelInProject("Type new name (no file will be created)", startName, "", "Type new name (no file will be created)");

            if (!string.IsNullOrEmpty(filename))
            {
                filename = System.IO.Path.GetFileNameWithoutExtension(filename);
                if (!string.IsNullOrEmpty(filename))
                {
                    if (asset != null)
                    {
                        if (assetDatabaseRename)
                            AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(asset), filename);
                        else
                            asset.name = filename;

                        asset.name = filename;
                        EditorUtility.SetDirty(asset);
                        AssetDatabase.SaveAssets();
                    }

                    return filename;
                }
            }

            return "";
        }

        public static string GetPathPopup(string windowTitle, string exampleName, string type = "asset", string playerPrefId = "LastFGenSaveDir")
        {
            string path = UnityEditor.EditorUtility.SaveFilePanelInProject(windowTitle, exampleName, type, "Enter name of file", GetLastPath);

            try
            {
                if (path != "")
                {
                    lastPath = System.IO.Path.GetDirectoryName(path);
                    EditorPrefs.SetString(playerPrefId, lastPath);
                    return path;
                }
            }
            catch (System.Exception)
            {
                Debug.LogError("Something went wrong when getting path directory in your project.");
            }

            return path;
        }

        public static string GenerateScriptablePath(ScriptableObject reference, string exampleFilename = "", string playerPrefId = "LastFGenSaveDir")
        {
            if (lastPath == "")
            {
                if (EditorPrefs.HasKey(playerPrefId))
                {
                    lastPath = EditorPrefs.GetString(playerPrefId);
                    if (!System.IO.File.Exists(lastPath)) lastPath = Application.dataPath;
                }
                else lastPath = Application.dataPath;
            }

            string path = UnityEditor.EditorUtility.SaveFilePanelInProject("Generate Preset File", exampleFilename, "asset", "Enter name of file", lastPath);

            try
            {
                if (path != "")
                {
                    lastPath = System.IO.Path.GetDirectoryName(path);
                    EditorPrefs.SetString(playerPrefId, lastPath);
                    return path;
                }
            }
            catch (System.Exception)
            {
                Debug.LogError("Something went wrong when creating scriptable file in your project.");
            }

            return path;
        }
#endif


        public static void DrawScriptableModificatorList<T>(List<T> toDraw, GUIStyle style, string title, ref bool foldout, bool newButton = false, bool moveButtons = false, UnityEngine.Object toDirty = null, string first = "[Base]", string defaultFilename = "New Scriptable File") where T : ScriptableObject
        {
#if UNITY_EDITOR
            if (toDraw == null) return;

            EditorGUILayout.BeginVertical(style);

            EditorGUILayout.BeginHorizontal();
            string fold = foldout ? " ▼" : " ►";
            if (GUILayout.Button(fold + "  " + title + " (" + toDraw.Count + ")", EditorStyles.label, GUILayout.Width(200))) foldout = !foldout;

            if (foldout)
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("+")) toDraw.Add(null);
            }

            EditorGUILayout.EndHorizontal();

            if (foldout)
            {
                GUILayout.Space(4);

                if (toDraw.Count > 0)
                    for (int i = 0; i < toDraw.Count; i++)
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.BeginHorizontal();

                        GUIContent lbl = new GUIContent(i == 0 ? first : "[" + i + "]");
                        float wdth = EditorStyles.label.CalcSize(lbl).x;

                        EditorGUILayout.LabelField(lbl, GUILayout.Width(wdth + 2));

                        toDraw[i] = (T)EditorGUILayout.ObjectField(toDraw[i], typeof(T), false);

                        if (newButton)
                            if (toDraw[i] == null)
                                if (GUILayout.Button("N", GUILayout.Width(24))) toDraw[i] = (T)GenerateScriptable(ScriptableObject.CreateInstance<T>(), defaultFilename);

                        if (moveButtons)
                        {
                            if (i > 0) if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowUp, "Move this element to be executed before above one"), GUILayout.Width(24))) { T temp = toDraw[i - 1]; toDraw[i - 1] = toDraw[i]; toDraw[i] = temp; }
                            if (i < toDraw.Count - 1) if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowDown, "Move this element to be executed after below one"), GUILayout.Width(24))) { T temp = toDraw[i + 1]; toDraw[i + 1] = toDraw[i]; toDraw[i] = temp; }
                        }

                        if (GUILayout.Button("X", GUILayout.Width(24))) { toDraw.RemoveAt(i); break; }

                        EditorGUILayout.EndHorizontal();
                        if (toDirty != null) if (EditorGUI.EndChangeCheck()) { EditorUtility.SetDirty(toDirty); }
                    }
                else
                {
                    EditorGUILayout.LabelField("No object in list", EditorStyles.centeredGreyMiniLabel);
                }
            }

            EditorGUILayout.EndVertical();
#endif
        }


        public static void AddScriptableToSimple(ScriptableObject parent, ScriptableObject toAdd, bool reload = true)
        {
#if UNITY_EDITOR
            try
            {
                AssetDatabase.AddObjectToAsset(toAdd, parent);
                EditorUtility.SetDirty(parent);
                if (reload) AssetDatabase.SaveAssets();
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogException(e);
                throw;
            }
#endif
        }



        public static bool AssetContainsAsset(UnityEngine.Object subAsset, UnityEngine.Object parentAsset)
        {
#if UNITY_EDITOR
            if (parentAsset == null) return false;

            // Container asset is not in asset database then it can't contain any asset
            if (AssetDatabase.Contains(parentAsset) == false) return false;

            // Not sub asset then not contained for sure
            if (AssetDatabase.IsSubAsset(subAsset) == false) return false;

            string path = AssetDatabase.GetAssetPath(parentAsset);
            if (!string.IsNullOrEmpty(path))
            {
                foreach (var asset in AssetDatabase.LoadAllAssetRepresentationsAtPath(path))
                {
                    if (asset == subAsset) return true; // Found subAsset in parentAsset
                }
            }
#endif

            return false; // All assets inside parent asset checked and sub asset was not found
        }


        public static void AddScriptableTo(ScriptableObject toAdd, UnityEngine.Object parentAsset, bool checkIfAlreadyContains = true, bool reload = true)
        {
#if UNITY_EDITOR

            try
            {
                if (parentAsset == null) return;

                if (AssetDatabase.Contains(parentAsset) == false)
                {
                    UnityEngine.Debug.Log("[FGenerators Warning] '" + parentAsset.name + "' is not recognized by AssetDatabase!");
                    return;
                }

                if (AssetDatabase.IsSubAsset(toAdd))
                {
                    UnityEngine.Debug.Log("[FGenerators Warning] '" + toAdd.name + "' is already part of other asset in project!");
                    return;
                }

                if (checkIfAlreadyContains)
                {
                    string path = AssetDatabase.GetAssetPath(parentAsset);
                    if (!string.IsNullOrEmpty(path))
                        foreach (var asset in AssetDatabase.LoadAllAssetRepresentationsAtPath(path))
                        {
                            if (asset == toAdd)
                            {
                                UnityEngine.Debug.Log("[FGenerators Warning] '" + toAdd.name + "' is already part of '" + parentAsset.name + "' asset!");
                                return;
                            }
                        }
                }

                if (AssetDatabase.GetAssetPath(toAdd) == AssetDatabase.GetAssetPath(parentAsset))
                    return;

                AssetDatabase.AddObjectToAsset(toAdd, parentAsset);
                EditorUtility.SetDirty(parentAsset);

                if (reload) AssetDatabase.SaveAssets();

            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogException(e);
                throw;
            }

#endif
        }


        public static bool IsAssetSaved(UnityEngine.Object asset)
        {
#if UNITY_EDITOR
            if (AssetDatabase.Contains(asset) == false)
            {
                AssetDatabase.SaveAssets();
                if (AssetDatabase.Contains(asset) == true) return true;
            }
            else
                return true;
#endif

            return false;
        }


        #endregion


        #region Utilities


        public static void SwapElements<T>(List<T> list, int index1, int index2, bool loop = false)
        {
            if (index1 == index2) return;

            if (loop)
            {
                if (index1 >= list.Count) index1 -= list.Count;
                if (index1 < 0) index1 += list.Count;
                if (index2 >= list.Count) index2 -= list.Count;
                if (index2 < 0) index2 += list.Count;
            }

            T temp = list[index1];
            list[index1] = list[index2];
            list[index2] = temp;
        }

        public static void SwapElements<T>(T[] list, int index1, int index2)
        {
            if (index1 == index2) return;

            T temp = list[index1];
            list[index1] = list[index2];
            list[index2] = temp;
        }

        public static void CheckForNulls<T>(List<T> classes)
        {
            for (int i = classes.Count - 1; i >= 0; i--)
            {
                if (classes[i] == null) classes.RemoveAt(i);
            }
        }


        public static bool IndexInListRange<T>(List<T> list, int index)
        {
            if (list == null) return false;
            if (index < 0) return false;
            if (index >= list.Count) return false;
            return true;
        }

        public static T GetListElementOrNull<T>(this List<T> list, int index) where T : class
        {
            if (list == null) return null;
            if (index < 0) return null;
            if (index >= list.Count) return null;
            if (FGenerators.CheckIfIsNull(list[index])) return null;
            return list[index];
        }

        public static void AdjustCount<T>(List<T> list, int targetCount, bool addNulls = false) where T : class, new()
        {
            if (list.Count == targetCount) return;

            if (list.Count < targetCount)
            {
                if (addNulls)
                {
                    while (list.Count < targetCount) list.Add(null);
                }
                else
                {
                    while (list.Count < targetCount) list.Add(new T());
                }
            }
            else
            {
                while (list.Count > targetCount) list.RemoveAt(list.Count - 1);
            }

            //if (list.Count < targetCount)
            //{
            //    for (int i = 0; i < targetCount - list.Count; i++)
            //    {
            //        if (addNulls)
            //            list.Add(null);
            //        else
            //            list.Add(new T());
            //    }
            //}
            //else
            //{
            //    for (int i = 0; i < list.Count - targetCount; i++)
            //    {
            //        list.RemoveAt(list.Count - 1);
            //    }
            //}
        }


        public static void Shuffle<T>(this IList<T> list)
        {
            Shuffle(list, random);
        }

        public static void Shuffle<T>(this IList<T> list, System.Random random)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }


#if UNITY_EDITOR
        public static bool DrawAllPropsOf(SerializedObject so, bool addIndent = false)
        {
            if (so == null) return false;

            EditorGUI.BeginChangeCheck();

            so.Update();

            if (addIndent) EditorGUI.indentLevel++;
            SerializedProperty sp = so.GetIterator();

            sp.Next(true);
            sp.Next(true);

            do { EditorGUILayout.PropertyField(sp); } while (sp.NextVisible(false));
            if (addIndent) EditorGUI.indentLevel--;

            so.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(so.targetObject);
                return true;
            }

            return false;
        }

        public static void DrawAllPropsOf(SerializedProperty sp, bool addIndent = false)
        {
            if (addIndent) EditorGUI.indentLevel++;
            sp.Next(true);
            do { EditorGUILayout.PropertyField(sp); } while (sp.NextVisible(false));
            if (addIndent) EditorGUI.indentLevel--;
        }

        public static void DrawSomePropsOf(SerializedProperty sp, int upTo, bool addIndent = false)
        {
            int count = 0;
            if (addIndent) EditorGUI.indentLevel++;
            sp.Next(true);
            do { EditorGUILayout.PropertyField(sp); count++; } while (sp.Next(false) && count < upTo);
            if (addIndent) EditorGUI.indentLevel--;
        }


#endif


        #region UI Scale DPI

        private static float _editorUiScaling;
        public static float EditorUIScale { get { return GetEditorUIScale(); } }

        public static float GetEditorUIScale()
        {
#if UNITY_EDITOR
            if (_editorUiScaling > 0.1f) return _editorUiScaling;

            System.Reflection.PropertyInfo p = typeof(GUIUtility).GetProperty("pixelsPerPoint", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            if (p != null)
                _editorUiScaling = (float)p.GetValue(null, null);
            else
                _editorUiScaling = 1f;
            return
                _editorUiScaling;
#else
return 1f;
#endif
        }

        #endregion


        #region Other Editor Related

        public static readonly Color Color_Remove = new Color(1f, 0.825f, 0.825f, 1f);

#if UNITY_EDITOR
        /// <summary> !!! EDITOR ONLY METHOD !!! </summary>
        public static void Editor_IteratorReload(SerializedProperty iterator, bool doNextEnter = true, bool deepCheck = false, bool fullCheck = false)
        {
            if (iterator == null) return;

            var refreshIterator = iterator.Copy();

            if (doNextEnter)
            {
                if (refreshIterator.Next(true) == false) return;
                if (refreshIterator.NextVisible(false) == false) return;
            }

            Rect drawRect = new Rect(-1000, 0, 2, 2);

            if (fullCheck)
            {
                while (refreshIterator.Next(false))
                {
                    EditorGUI.PropertyField(drawRect, refreshIterator, true);

                    var deppIter = refreshIterator.Copy();
                    if (deppIter.Next(true))
                    {
                        while (deppIter.Next(false))
                        {
                            EditorGUI.PropertyField(drawRect, deppIter, true);
                        }
                    }
                }
            }
            else if (deepCheck)
            {
                while (refreshIterator.Next(false))
                {
                    EditorGUI.PropertyField(drawRect, refreshIterator, true);
                }
            }
            else
            {
                while (refreshIterator.NextVisible(false))
                {
                    EditorGUI.PropertyField(drawRect, refreshIterator, true);
                }
            }
        }


#endif

        #endregion


        #endregion

    }

    #region Prefab Helper

    /// <summary>
    /// Class helping drawing prefabs inside inspector window and referencing to them by FGenerators
    /// </summary>
    [System.Serializable]
    public class PrefabReference
    {
        public GameObject CoreGameObject { get { return Prefab; } }
        public Collider CoreCollider { get { return MainCollider; } }

        public GameObject GameObject
        {
            get
            {
                if (tempReplacePrefab != null) return tempReplacePrefab;
                return Prefab;
            }
        }

        public Collider Collider
        {
            get
            {
                if (tempReplaceCollider != null) return tempReplaceCollider;
                return MainCollider;
            }
        }

        [SerializeField] private GameObject Prefab;
        private GameObject tempReplacePrefab;
        [SerializeField] private Collider MainCollider;
        private Collider tempReplaceCollider;

        private int id;
        public int subID;
        private Texture tex;


        #region Prefab Ref Setup


        public Texture Preview
        {
            get
            {
                if (Prefab == null)
                {
                    tex = null;
                    return null;
                }

                if (tex == null || id != Prefab.GetInstanceID())
                {
                    id = Prefab.GetInstanceID();
#if UNITY_EDITOR
                    tex = AssetPreview.GetAssetPreview(Prefab);
#endif
                }

                return tex;
            }
        }


        protected virtual void DrawGUIWithPrefab(Color color, int previewSize = 72, string predicate = "", Action clickCallback = null, Action removeCallback = null, bool drawThumbnail = true, bool drawPrefabField = true)
        {
#if UNITY_EDITOR
            if (drawThumbnail)
            {
                Color bc = GUI.backgroundColor;
                GUI.backgroundColor = color;
                if (GUILayout.Button(new GUIContent(Preview), opt)) if (clickCallback != null) clickCallback.Invoke();
                GUI.backgroundColor = bc;
            }

            if (predicate != "_")
                EditorGUILayout.LabelField(predicate + Prefab.name, EditorStyles.centeredGreyMiniLabel, GUILayout.Width(previewSize));
            EditorGUILayout.BeginHorizontal();
            GameObject prepr = Prefab;

            if (drawPrefabField) Prefab = (GameObject)EditorGUILayout.ObjectField(Prefab, typeof(GameObject), false, removeCallback != null ? opt3 : opt2);

            if (Prefab != prepr) OnPrefabChanges();
            if (removeCallback != null) if (GUILayout.Button("X", GUILayout.Width(20))) { removeCallback.Invoke(); return; }
            EditorGUILayout.EndHorizontal();
#endif
        }



        protected virtual void DrawGUIWithoutPrefab(int previewSize = 72, string predicate = "", Action removeCallback = null, bool drawPrefabField = true)
        {
#if UNITY_EDITOR
            EditorGUILayout.BeginHorizontal();
            if (drawPrefabField) Prefab = (GameObject)EditorGUILayout.ObjectField(Prefab, typeof(GameObject), false, removeCallback != null ? opt3 : opt2);
            if (Prefab) OnPrefabChanges();
            if (removeCallback != null) if (GUILayout.Button("X", GUILayout.Width(20))) { removeCallback.Invoke(); return; }
            EditorGUILayout.EndHorizontal();
#endif
        }



        public virtual void OnPrefabChanges() { }

        public static GUILayoutOption[] opt;
        /// <summary> Height 18</summary>
        public static GUILayoutOption[] opt2;
        /// <summary> Witdh-20  Height 18</summary>
        public static GUILayoutOption[] opt3;
        public static bool StopReloadLayoutOptions = false;



        public static void DrawPrefabField(PrefabReference prefabRef, Color defaultColor, string predicate = "", int previewSize = 72, Action clickCallback = null, Action removeCallback = null, bool drawThumbnail = true, UnityEngine.Object toDiry = null, bool drawPrefabField = true, bool drawAdditionalButtons = true)
        {
#if UNITY_EDITOR
            Color bc = GUI.backgroundColor;

            if (StopReloadLayoutOptions == false)
            {
                opt = new GUILayoutOption[] { GUILayout.Width(previewSize), GUILayout.Height(previewSize) };
                opt2 = new GUILayoutOption[] { GUILayout.Width(previewSize), GUILayout.Height(18) };
                opt3 = new GUILayoutOption[] { GUILayout.Width(previewSize - 20), GUILayout.Height(18) };
            }

            EditorGUILayout.BeginVertical(GUILayout.Width(previewSize));



            EditorGUI.BeginChangeCheck();

            if (prefabRef.Prefab != null)
                prefabRef.DrawGUIWithPrefab(defaultColor, previewSize, predicate, clickCallback, removeCallback, drawThumbnail, drawPrefabField);
            else
                prefabRef.DrawGUIWithoutPrefab(previewSize, predicate, removeCallback, drawPrefabField);


            if (drawAdditionalButtons)
            {

                if (prefabRef != null) if (prefabRef.Prefab != null)
                    {
                        bool prefabIsModel = false;

#if UNITY_EDITOR
#if UNITY_2019_1_OR_NEWER
                        var pfType = PrefabUtility.GetPrefabAssetType(prefabRef.Prefab);
                        if (pfType == PrefabAssetType.Model) prefabIsModel = true;
#endif
#endif

                        bool prefabHasCollider = prefabRef.MainCollider != null;

                        if (prefabIsModel)
                        {

                            if (GUILayout.Button(new GUIContent(" Make", EditorGUIUtility.IconContent("Prefab Icon").image, "Generate prefab out of model file"), GUILayout.Width(64), GUILayout.Height(20)))
                            {
                                string path = AssetDatabase.GetAssetPath(prefabRef.Prefab);
                                GameObject toSave = (GameObject)PrefabUtility.InstantiatePrefab(prefabRef.Prefab);
                                toSave.name = "PR_" + Path.GetFileNameWithoutExtension(path);
                                path = Path.GetDirectoryName(path);
                                //path = path.Substring(0, path.LastIndexOf('/'));
                                path = Path.Combine(path, toSave.name + ".prefab");
#if UNITY_2018_4_OR_NEWER
                                PrefabUtility.SaveAsPrefabAsset(toSave, path);
#else
                            PrefabUtility.CreatePrefab(path, toSave);
#endif
                                if (toSave) GameObject.DestroyImmediate(toSave);
                                prefabRef.Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                            }
                        }
                        else
                        {


                            if (prefabRef.Prefab.transform.rotation != Quaternion.identity)
                            {
                                EditorGUILayout.BeginVertical();
                                EditorGUILayout.HelpBox("Prefab rotation is not 0,0,0!", MessageType.None);
                                if (GUILayout.Button(new GUIContent("FIX", "Setting prefab rotation to 0,0,0 but you can use rotation offset for advanced adjustments for spawning\nbut it's recommended to set 0,0,0 to avoid some unwanted not clear rotations"))) { prefabRef.Prefab.transform.rotation = Quaternion.identity; AssetDatabase.SaveAssets(); }
                                EditorGUILayout.EndVertical();
                            }

                            if (prefabHasCollider == false)
                            {
                                EditorGUILayout.BeginHorizontal();
                                if (GUILayout.Button(new GUIContent("+", EditorGUIUtility.IconContent("BoxCollider Icon").image, "Automatically add box collider to the prefab"), GUILayout.Width(32), GUILayout.Height(21)))
                                {
                                    prefabHasCollider = prefabRef.GetCollider() != null;
                                    if (prefabHasCollider == false)
                                    {
                                        MeshFilter[] f = prefabRef.Prefab.GetComponentsInChildren<MeshFilter>();
                                        float biggsetSize = 0f;
                                        MeshFilter biggest = null;

                                        for (int ff = 0; ff < f.Length; ff++)
                                        {
                                            if (f[ff].sharedMesh == null) continue;
                                            float sz = Vector3.Scale(f[ff].transform.lossyScale, f[ff].sharedMesh.bounds.size).magnitude;
                                            if (sz > biggsetSize)
                                            {
                                                biggsetSize = sz;
                                                biggest = f[ff];
                                            }
                                        }

                                        if (biggest)
                                        {
                                            prefabRef.MainCollider = biggest.gameObject.AddComponent<BoxCollider>();
                                        }
                                        else prefabRef.MainCollider = prefabRef.Prefab.AddComponent<BoxCollider>();
                                    }

                                    AssetDatabase.SaveAssets();

                                }

                                if (GUILayout.Button(new GUIContent("+", EditorGUIUtility.IconContent("MeshCollider Icon").image, "Automatically add mesh collider to the prefab"), GUILayout.Width(32), GUILayout.Height(21)))
                                {
                                    prefabHasCollider = prefabRef.GetCollider() != null;
                                    if (prefabHasCollider == false)
                                    {
                                        MeshFilter[] f = prefabRef.Prefab.GetComponentsInChildren<MeshFilter>();
                                        float biggsetSize = 0f;
                                        MeshFilter biggest = null;

                                        for (int ff = 0; ff < f.Length; ff++)
                                        {
                                            if (f[ff] == null) continue;
                                            if (f[ff].sharedMesh == null) continue;
                                            float sz = Vector3.Scale(f[ff].transform.lossyScale, f[ff].sharedMesh.bounds.size).magnitude;
                                            if (sz > biggsetSize)
                                            {
                                                biggsetSize = sz;
                                                biggest = f[ff];
                                            }
                                        }

                                        if (biggest)
                                        {
                                            MeshCollider cl = biggest.gameObject.AddComponent<MeshCollider>();
                                            cl.sharedMesh = biggest.sharedMesh;
                                            cl.convex = true;
                                            prefabRef.MainCollider = cl;
                                        }
                                    }

                                    AssetDatabase.SaveAssets();
                                }

                                EditorGUILayout.EndHorizontal();
                            }

                        }

                    }

            }

            if (EditorGUI.EndChangeCheck()) if (toDiry != null) EditorUtility.SetDirty(toDiry);

            EditorGUILayout.EndVertical();
#endif
        }


        public static void DrawPrefabsList<T>(List<T> list, ref bool foldout, ref int selected, ref bool thumbnails, Color defaultC, Color selectedC, float viewWidth = 500f, int previewSize = 72, bool searchButtons = false, UnityEngine.Object toDirty = null, bool allowAdding = true) where T : PrefabReference, new()
        {
#if UNITY_EDITOR
            if (list == null) return;

            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxBlankStyle);
            EditorGUILayout.BeginHorizontal();

            string f = FGUI_Resources.GetFoldSimbol(foldout);
            if (GUILayout.Button(f + "  Prefabs (" + list.Count + ")", FGUI_Resources.FoldStyle, GUILayout.Height(20))) foldout = !foldout;

            if (foldout)
            {
                GUILayout.FlexibleSpace();

                if (searchButtons)
                {
                    if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Rename), new GUILayoutOption[] { GUILayout.MaxWidth(24), GUILayout.Height(20) }))
                        thumbnails = !thumbnails;

                    if (list.Count > 0 && list[0] != null && list[0].Prefab != null)
                    {
                        if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_SearchNumeric, "Search for prefabs with the same name but with numbers"), new GUILayoutOption[] { GUILayout.MaxWidth(48), GUILayout.Height(20) }))
                        {
                            List<GameObject> pfs = new List<GameObject>();
                            pfs.Add(list[0].Prefab);
                            FGenerators.GetIncrementalTo(pfs);

                            for (int i = 0; i < pfs.Count; i++)
                            {
                                if (pfs[i] == null) continue;
                                if (!list.Any(p => p.Prefab == pfs[i])) { list.Add(new T() { Prefab = pfs[i] }); if (toDirty != null) EditorUtility.SetDirty(toDirty); }
                            }
                        }

                        if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_SearchDirectory, "Get all prefabs inside it's directory"), new GUILayoutOption[] { GUILayout.MaxWidth(48), GUILayout.Height(20) }))
                        {
                            List<GameObject> pfs = new List<GameObject>();
                            pfs.Add(list[0].Prefab);
                            FGenerators.GetSimilarTo(pfs);

                            for (int i = 0; i < pfs.Count; i++)
                            {
                                if (pfs[i] == null) continue;
                                if (!list.Any(p => p.Prefab == pfs[i])) { list.Add(new T() { Prefab = pfs[i] }); if (toDirty != null) EditorUtility.SetDirty(toDirty); }
                            }
                        }
                    }

                    if (list.Count > 0)
                    {
                        Color prebc = GUI.backgroundColor;
                        GUI.backgroundColor = new Color(1f, 0.635f, 0.635f, 1f);

                        if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Remove, "Remove all prefabs from list"), FGUI_Resources.ButtonStyle, new GUILayoutOption[] { GUILayout.MaxWidth(24), GUILayout.Height(19) }))
                            list.Clear();

                        GUI.backgroundColor = prebc;
                    }

                }

                if (allowAdding)
                    if (GUILayout.Button("+", GUILayout.Width(24))) { list.Add(new T()); if (toDirty != null) EditorUtility.SetDirty(toDirty); }

                EditorGUILayout.EndHorizontal();

                GUILayout.Space(10);

                float currWidth = 0f;
                if (list.Count > 0)
                    EditorGUILayout.BeginHorizontal();
                else
                    EditorGUILayout.BeginHorizontal(GUILayout.Height(28));


                int sel = selected;
                int toRemove = -1;

                for (int i = 0; i < list.Count; i++)
                {
                    if (i == 1) StopReloadLayoutOptions = true;

                    if (allowAdding)
                    {
                        if (selected == i)
                            DrawPrefabField(list[i], selectedC, "[" + i + "] ", previewSize, () => { if (list[i] != null && list[i].Prefab) EditorGUIUtility.PingObject(list[i].Prefab); }, () => { toRemove = i; }, thumbnails, toDirty);
                        else
                            DrawPrefabField(list[i], defaultC, "[" + i + "] ", previewSize, () => { sel = i; }, () => { toRemove = i; }, thumbnails, toDirty);
                    }
                    else
                    {
                        DrawPrefabField(list[i], defaultC, "[" + i + "] ", previewSize, () => { sel = i; }, null, thumbnails);
                    }

                    currWidth += previewSize;
                    if (currWidth + previewSize > viewWidth * 0.9f)
                    {
                        currWidth = 0f;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                    }
                }

                #region Drag & Drop

                if (allowAdding)
                {
                    Color preCol = GUI.color;

                    GUI.color = new Color(0.9f, 0.9f, 0.9f, 0.2f);

                    var drop = GUILayoutUtility.GetRect(0f, 0f, new GUILayoutOption[] { GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true) });

                    GUIStyle d = FGUI_Inspector.Style(new RectOffset(0, 0, 0, 0), new RectOffset(0, 0, 0, 0), new Color(0.2f, 0.7f, 0.3f, 0.5f), Vector4.zero, 0);
                    d = new GUIStyle(d) { alignment = TextAnchor.MiddleCenter };

                    GUI.Box(drop, new GUIContent(FGUI_Resources.Tex_Drag, "Drag & Drop prefabs here"), d);

                    var dropEvent = UnityEngine.Event.current;

                    if (dropEvent != null)
                    {
                        if (dropEvent.type == UnityEngine.EventType.DragPerform || dropEvent.type == UnityEngine.EventType.DragUpdated)
                            if (drop.Contains(dropEvent.mousePosition))
                            {
                                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                                if (dropEvent.type == UnityEngine.EventType.DragPerform)
                                {
                                    DragAndDrop.AcceptDrag();
                                    foreach (var dragged in DragAndDrop.objectReferences)
                                    {
                                        GameObject draggedPrefab = dragged as GameObject;
                                        if (draggedPrefab)
                                        {
                                            T pf = new T();
                                            pf.Prefab = draggedPrefab;
                                            pf.GetMesh(true);
                                            list.Add(pf);

                                            if (toDirty != null) EditorUtility.SetDirty(toDirty);
                                        }
                                    }
                                }

                                UnityEngine.Event.current.Use();
                            }
                    }

                    GUI.color = preCol;
                }

                #endregion



                EditorGUILayout.EndHorizontal();


                if (toRemove != -1)
                {
                    list.RemoveAt(toRemove);
                    if (toDirty != null)
                    {
                        EditorUtility.SetDirty(toDirty);
                        AssetDatabase.SaveAssets();
                    }
                }

                selected = sel;
                if (selected > list.Count) selected = list.Count - 1;
                if (sel < 0) sel = 0;
            }
            else
            {
                GUILayout.FlexibleSpace();

                if (list.Count > 0)
                {
                    Color prebc = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(1f, 0.635f, 0.635f, 1f);

                    if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Remove, "Remove all prefabs from list"), FGUI_Resources.ButtonStyle, new GUILayoutOption[] { GUILayout.MaxWidth(24), GUILayout.Height(19) }))
                        list.Clear();

                    GUI.backgroundColor = prebc;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            StopReloadLayoutOptions = false;
#endif
        }



        [HideInInspector][SerializeField] protected Mesh _refMesh;
        public Mesh GetMesh(bool refresh = false)
        {
            if (Prefab == null) return null;

            if (refresh) _refMesh = null;
            else
                if (_refMesh)
            {
                if (MainCollider == null) GetCollider();
                return _refMesh;
            }

            List<SkinnedMeshRenderer> sk = FTransformMethods.FindComponentsInAllChildren<SkinnedMeshRenderer>(Prefab.transform);
            for (int i = 0; i < sk.Count; i++)
                if (sk[i])
                    if (sk[i].sharedMesh)
                    {
                        _refMesh = sk[i].sharedMesh;
                        if (MainCollider == null) GetCollider();
                        return _refMesh;
                    }

            List<MeshFilter> fs = FTransformMethods.FindComponentsInAllChildren<MeshFilter>(Prefab.transform);
            for (int i = 0; i < fs.Count; i++)
                if (fs[i])
                    if (fs[i].sharedMesh)
                    {
                        _refMesh = fs[i].sharedMesh;
                        if (MainCollider == null) GetCollider();
                        return _refMesh;
                    }

            if (MainCollider == null)
            {
                MainCollider = FTransformMethods.FindComponentInAllChildren<Collider>(Prefab.transform);
            }

            return _refMesh;
        }


        [HideInInspector][SerializeField] protected Collider _refCol;
        public Collider GetCollider()
        {
            if (Prefab == null) return null;
            if (_refCol)
            {
                if (MainCollider == null) MainCollider = _refCol;
                return _refCol;
            }

            List<Collider> sk = FTransformMethods.FindComponentsInAllChildren<Collider>(Prefab.transform);
            for (int i = 0; i < sk.Count; i++)
                if (sk[i])
                {
                    _refCol = sk[i];
                    if (MainCollider == null) MainCollider = _refCol;
                    return _refCol;
                }

            if (_refCol == null) _refCol = Prefab.GetComponent<Collider>();

            if (MainCollider == null) MainCollider = _refCol;

            return _refCol;
        }


        #endregion


        public void SetPrefab(GameObject pf)
        {
            Prefab = pf;
        }

        public void SetCollider(Collider pf)
        {
            MainCollider = pf;
        }

        public void TemporaryReplace(GameObject tempRepl)
        {
            if (tempRepl == null)
            {
                tempReplacePrefab = null;
                tempReplaceCollider = null;
            }
            else
            {
                tempReplacePrefab = tempRepl;
                tempReplaceCollider = tempRepl.GetComponentInChildren<Collider>();
            }
        }


    }

    #endregion


    #region Scene Draw Helper



    #endregion
}
