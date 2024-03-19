using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace FIMSpace.FEditor
{
    /// <summary>
    /// FM: Class with basic tools for working in Unity Editor level
    /// </summary>
    public static partial class FEditor_MenuAddOptions
    {

        [MenuItem("CONTEXT/Collider/Generate NavMesh Obstacle")]
        private static void GenerateNavMeshObstacle(MenuCommand menuCommand)
        {
            Collider targetComponent = (Collider)menuCommand.context;

            if (targetComponent)
            {
                NavMeshObstacle obstacle = targetComponent.gameObject.GetComponent<NavMeshObstacle>();
                if (obstacle == null) obstacle = targetComponent.gameObject.AddComponent<NavMeshObstacle>();
                obstacle.center = targetComponent.bounds.center;
                obstacle.size = targetComponent.bounds.size;
                obstacle.carving = true;

                EditorUtility.SetDirty(targetComponent.gameObject);
            }
        }


        [MenuItem("CONTEXT/Transform/Fit child objects to bottom origin")]
        private static void ChildBottomOrigin(MenuCommand menuCommand)
        {
            Transform t = (Transform)menuCommand.context;
            FitToBottom(t);
        }

        [MenuItem("CONTEXT/Transform/Hide Transform In The Inspector View (Use Components Hider to unhide)")]
        private static void HideTransformInInspector(MenuCommand menuCommand)
        {
            Transform t = (Transform)menuCommand.context;

            if (t)
            {
                t.hideFlags = HideFlags.HideInInspector;
                EditorUtility.SetDirty(t);
            }
        }

        private static void FitToBottom(Transform t)
        {
            if (t.childCount > 0)
            {
                float lowestY = float.MaxValue;
                Renderer rr = null;

                for (int i = 0; i < t.childCount; i++)
                {
                    Renderer r = t.GetChild(i).GetComponent<Renderer>();

                    if (r.bounds.min.y < lowestY)
                    {
                        lowestY = r.bounds.min.y;
                        rr = r;
                    }
                }

                if (rr)
                {
                    Vector3 offset = new Vector3(0, t.position.y - rr.bounds.min.y, 0);
                    for (int i = 0; i < t.childCount; i++)
                    {
                        t.GetChild(i).position += offset;
                    }
                }

                EditorUtility.SetDirty(t.gameObject);
            }
        }

        [MenuItem("CONTEXT/Transform/Generate parent + Fit objects to bottom")]
        private static void GenerateParentAndFit(MenuCommand menuCommand)
        {
            Transform t = (Transform)menuCommand.context;
            int sibl = t.GetSiblingIndex();
            GameObject parent = new GameObject(t.name);
            parent.transform.SetParent(t.parent);
            parent.transform.position = t.position;
            parent.transform.rotation = t.rotation;
            parent.transform.localScale = t.localScale;
            t.SetParent(parent.transform);
            FitToBottom(parent.transform);
            EditorUtility.SetDirty(t.gameObject);
            parent.transform.SetSiblingIndex(sibl);
            if (Selection.activeGameObject == t.gameObject) Selection.activeGameObject = parent;
        }

        [MenuItem("CONTEXT/AudioReverbZone/Fit To Collider")]
        private static void AudioReverbZoneFit(MenuCommand menuCommand)
        {
            AudioReverbZone targetComponent = (AudioReverbZone)menuCommand.context;

            if (targetComponent)
            {
                Collider c = targetComponent.gameObject.GetComponent<Collider>();

                if (c)
                {
                    targetComponent.minDistance = Vector3.Distance(c.bounds.min, c.bounds.max) * 0.45f;
                    targetComponent.maxDistance = targetComponent.minDistance * 1.35f;
                }

                EditorUtility.SetDirty(targetComponent.gameObject);
            }
        }


        [MenuItem("CONTEXT/ReflectionProbe/Fit To Collider")]
        private static void ReflectionProbeFit(MenuCommand menuCommand)
        {
            ReflectionProbe targetComponent = (ReflectionProbe)menuCommand.context;

            if (targetComponent)
            {
                Collider c = targetComponent.gameObject.GetComponent<Collider>();
                BoxCollider bc = c as BoxCollider;

                if (c)
                {
                    if (bc)
                    {
                        targetComponent.center = bc.center;
                        targetComponent.size = bc.size;
                    }
                    else
                    {
                        targetComponent.center = c.bounds.center;
                        targetComponent.size = c.bounds.size;
                    }
                }

                EditorUtility.SetDirty(targetComponent.gameObject);
            }
        }

        [MenuItem("GameObject/Add Separator", false, 0)]
        static void AddSeparatorObject()
        {
            GameObject go = new GameObject();
            go.name = "-------------------";
            go.gameObject.SetActive(false);
            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
        }

        [MenuItem("CONTEXT/MeshFilter/Save Mesh As Asset")]
        private static void SaveFilterMeshAsAsset(MenuCommand menuCommand)
        {
            MeshFilter targetComponent = (MeshFilter)menuCommand.context;

            if (targetComponent == null) return;
            if (targetComponent.sharedMesh == null) return;

            Mesh newMesh = GameObject.Instantiate(targetComponent.sharedMesh) as Mesh;

            string nameFormatted = targetComponent.sharedMesh.name.Replace(":", "-");
            nameFormatted = nameFormatted.Replace("=", "_");

            string path = EditorUtility.SaveFilePanel("Select Directory", Application.dataPath, nameFormatted, "");
            if (path == "") return;

            if (path.StartsWith(Application.dataPath))
            {
                path = "Assets" + path.Substring(Application.dataPath.Length);
            }

            AssetDatabase.CreateAsset(newMesh, path + ".asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var obj = AssetDatabase.LoadAssetAtPath(path + ".asset", typeof(Mesh));
            if (obj) EditorGUIUtility.PingObject(obj);
        }

    }
}
