#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FTools
{
    public abstract class FContainerBase : ScriptableObject
    {
        public List<AssetReference> ContainedAssets = new List<AssetReference>();

        public virtual bool Contains(Object obj)
        {
            for (int i = ContainedAssets.Count - 1; i >= 0; i--)
            {
                if (ContainedAssets[i].Reference == null) { ContainedAssets.RemoveAt(i); continue; }
                if (ContainedAssets[i].Reference == obj) return true;
            }

            return false;
        }

        public virtual void Remove(Object obj)
        {
            for (int i = ContainedAssets.Count - 1; i >= 0; i--)
            {
                if (ContainedAssets[i].Reference == null) { ContainedAssets.RemoveAt(i); continue; }
                if (ContainedAssets[i].Reference == obj) { ContainedAssets.RemoveAt(i); return; }
            }
        }

        public virtual void RemoveAndDestroy(Object obj)
        {
            if (obj == null) return;

            Remove(obj);

#if UNITY_EDITOR
            AssetDatabase.RemoveObjectFromAsset(obj);
            if (obj) DestroyImmediate(obj, true);
#endif
        }

        public virtual void CopyAsset(Object obj, string extension = ".asset")
        {
            if (obj == null) return;

#if UNITY_EDITOR
            string path = AssetDatabase.GetAssetPath(this);
            AssetDatabase.CreateAsset(Instantiate(obj), System.IO.Path.GetDirectoryName(path) + "/" + obj.name + " (copy)" + extension);
#endif
        }


        public virtual void Add(Object obj)
        {
            if (obj == null) return;

            AssetReference a = new AssetReference();
            a.Reference = obj;
#if UNITY_EDITOR
            a.OriginalExtension = System.IO.Path.GetExtension(AssetDatabase.GetAssetPath(obj));
#endif
            ContainedAssets.Add(a);
        }

        [System.Serializable]
        public class AssetReference
        {
            public Object Reference;
            public string OriginalExtension = "";
        }

        public AssetReference GetReferenceTo(Object asset)
        {
            if (asset == null) return null;

            for (int i = ContainedAssets.Count - 1; i >= 0; i--)
            {
                if (ContainedAssets[i].Reference == null) { ContainedAssets.RemoveAt(i); continue; }
                if (ContainedAssets[i].Reference == asset) return ContainedAssets[i];
            }

            return null;
        }

        public virtual void AddAsset(Object obj)
        {
            if (obj == null) return;

            if (Contains(obj))
            {
                UnpackSingleAsset(obj);
                return;
            }

            if (!Contains(obj)) Add(obj);
            AddAssetTo(this, obj);
        }

        public virtual void UnpackSingleAsset(Object asset)
        {
            if (asset == null) return;
            UnpackSingleAsset(this, asset);
        }

        public virtual void UnpackAll()
        {
            UnpackAll(this);
        }

        public void _SetDirty()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }


        #region Static Editor Utility Methods


        public static void AddAssetTo(ScriptableObject container, Object asset)
        {
#if UNITY_EDITOR

            string prePath = AssetDatabase.GetAssetPath(asset);
            AssetDatabase.RemoveObjectFromAsset(asset);
            AssetDatabase.AddObjectToAsset(asset, AssetDatabase.GetAssetPath(container));
            AssetDatabase.SaveAssets();
            AssetDatabase.DeleteAsset(prePath);
#endif
        }


        public static void UnpackAll(FContainerBase container)
        {

#if UNITY_EDITOR
            string path = AssetDatabase.GetAssetPath(container);
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);

            for (int a = 0; a < assets.Length; a++)
            {
                UnityEngine.Object asset = assets[a];
                if (asset == container) continue;

                var refr = container.GetReferenceTo(asset);
                if (refr == null) return;

                AssetDatabase.RemoveObjectFromAsset(asset);
                AssetDatabase.CreateAsset(asset, System.IO.Path.GetDirectoryName(path) + "/" + asset.name + refr.OriginalExtension);

            }

            container.ContainedAssets.Clear();

            AssetDatabase.ImportAsset(path);
#endif
        }


        public static void UnpackSingleAsset(FContainerBase container, Object tgt)
        {

#if UNITY_EDITOR

            string path = AssetDatabase.GetAssetPath(container);
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);

            for (int a = 0; a < assets.Length; a++)
            {
                UnityEngine.Object asset = assets[a];
                if (asset != tgt) continue;

                var refr = container.GetReferenceTo(tgt);
                if (refr == null) return;

                AssetDatabase.RemoveObjectFromAsset(tgt);

                AssetDatabase.CreateAsset(tgt, System.IO.Path.GetDirectoryName(path) + "/" + asset.name + refr.OriginalExtension);
                break;

            }

            container.Remove(tgt);

            AssetDatabase.ImportAsset(path);
            AssetDatabase.SaveAssets();
#endif
        }


        #endregion

    }

    #region Inspector window draw

#if UNITY_EDITOR
    public class FContainerBaseEditor : Editor
    {
        public FContainerBase bGet { get { if (_bget == null) _bget = (FContainerBase)target; return _bget; } }
        private FContainerBase _bget;

        protected virtual string MainDragAndDropText { get { return "  Drag & Drop your Assets here"; } }
        protected virtual string HeaderInfo { get { return "Simple file in which you can keep multiple asset files to keep projects directory cleaner"; } }
        protected virtual Color DragAndDropBoxColor { get { return new Color(0.2f, 1f, 0.4f, 0.35f); } }
        protected virtual bool DrawDragAndDropToCopy { get { return false; } }
        protected virtual bool DrawDragAndDropToRemove { get { return false; } }

        public override void OnInspectorGUI()
        {
            if (HeaderInfo.Length > 1)
            {
                EditorGUILayout.HelpBox(HeaderInfo, UnityEditor.MessageType.Info);
            }

            serializedObject.Update();

            GUILayout.Space(4f);
            DrawPropertiesExcluding(serializedObject, "m_Script");

            serializedObject.ApplyModifiedProperties();
            GUILayout.Space(10f);
            DrawDragAndDropBox();
            GUILayout.Space(10f);

            GUIBody();


            if (DrawDragAndDropToCopy || DrawDragAndDropToRemove)
            {
                GUILayout.Space(10f);

                if (DrawDragAndDropToCopy && DrawDragAndDropToRemove)
                {
                    EditorGUILayout.BeginHorizontal();

                    bGet.CopyAsset(DragAndDropBox(new Color(0.4f, 0.5f, 1f), bGet, 32, "  Drag here to Copy"));
                    bGet.RemoveAndDestroy(DragAndDropBox(new Color(1f, 0.4f, 0.4f), bGet, 32, "  Drag here to REMOVE"));

                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    if (DrawDragAndDropToCopy) bGet.CopyAsset(DragAndDropBox(new Color(0.4f, 0.5f, 1f), bGet, 32, "  Drag & Drop here to Copy"));
                    else bGet.RemoveAndDestroy(DragAndDropBox(new Color(1f, 0.4f, 0.4f), bGet, 32, "  Drag & Drop here to REMOVE"));
                }

                GUILayout.Space(10f);
            }

            GUIFooter();
        }

        protected virtual void GUIBody()
        {

        }

        protected virtual void GUIFooter()
        {
            GUILayout.Space(16f);
            if (GUILayout.Button("Unpack all files outside", GUILayout.Height(20)))
            {
                bGet.UnpackAll();
            }
        }

        protected virtual void DrawDragAndDropBox()
        {
            GUILayout.Label("Drop file from inside this pack to release it to project directory", EditorStyles.centeredGreyMiniLabel);

            Object dragged = DragAndDropBox(DragAndDropBoxColor, bGet, 44, MainDragAndDropText);

            if (dragged)
            {
                bGet.AddAsset(dragged);
            }
        }


        public static Object DragAndDropBox(Color col, Object toDirty, float height = 44f, string ddText = "  Drag & Drop your Assets here")
        {
            Object ob = null;

            Color preCol = GUI.color;
            GUI.color = col;
            GUILayout.Space(2);
            GUILayout.BeginVertical(FGUI_Resources.HeaderBoxStyleH);
            GUI.color = new Color(1f, 1f, 1f, 0.7f);

            var drop = GUILayoutUtility.GetRect(0f, height, new GUILayoutOption[1] { GUILayout.ExpandWidth(true) });

            GUI.Box(drop, new GUIContent(ddText, FGUI_Resources.Tex_Drag), new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter });
            Event dropEvent = Event.current;
            GUI.color = preCol;

            switch (dropEvent.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:

                    if (!drop.Contains(dropEvent.mousePosition)) break;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (dropEvent.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (var dragged in DragAndDrop.objectReferences)
                        {
                            Object draggedObject = dragged as Object;

                            if (draggedObject)
                            {
                                ob = draggedObject;
                                EditorUtility.SetDirty(toDirty);
                            }
                        }

                    }

                    Event.current.Use();
                    break;
            }

            GUILayout.EndVertical();
            GUILayout.Space(2);

            GUI.color = preCol;

            return ob;
        }


    }
#endif

    #endregion

}