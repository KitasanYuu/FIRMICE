using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;

public class TransformCapturer : EditorWindow
{
    private GameObject targetObject;
    private List<TransformSnapshot> snapshots = new List<TransformSnapshot>();
    private string saveDirectoryPath = "Assets/Snapshots/";

    [MenuItem("YuuTools/Animation/TransformCapturer")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(TransformCapturer));
    }

    private void OnGUI()
    {
        GUILayout.Label("Record Transform Snapshot", EditorStyles.boldLabel);

        targetObject = EditorGUILayout.ObjectField("Target Object", targetObject, typeof(GameObject), true) as GameObject;

        if (GUILayout.Button("Record Snapshot"))
        {
            RecordSnapshot();
        }

        GUILayout.Label("Available Snapshots:");
        foreach (var snapshot in snapshots)
        {
            if (GUILayout.Button(snapshot.name))
            {
                ApplySnapshot(snapshot);
            }
        }

        //if (GUILayout.Button("Save Snapshots"))
        //{
        //    SaveSnapshots();
        //}

        if (GUILayout.Button("Load Snapshots"))
        {
            LoadSnapshots();
        }
    }

    private void RecordSnapshot()
    {
        bool needRecord = true;
        if (targetObject == null)
        {
            Debug.LogWarning("Target object is not assigned.");
            return;
        }

        foreach (var existingSnapshot in snapshots)
        {
            if (existingSnapshot.targetObject == targetObject && TransformDataMatches(existingSnapshot, targetObject))
            {
                Debug.Log("A matching snapshot already exists for the target object. No new snapshot was saved.");
                needRecord = false;
                return;
            }
        }

        if (needRecord)
        {
            var snapshot = new TransformSnapshot(targetObject, targetObject.name);
            snapshots.Add(snapshot);
            Debug.Log("Snapshot recorded.");
            SaveSnapshotToFile(snapshot);
        }
    }

    private bool TransformDataMatches(TransformSnapshot snapshot, GameObject target)
    {
        // 获取当前目标对象及其所有子对象的Transform组件
        Transform[] currentTransforms = target.GetComponentsInChildren<Transform>(true);

        // 检查快照中记录的变换数据数量是否与当前目标对象的变换组件数量相同
        if (snapshot.transformData.Count != currentTransforms.Length) return false;

        for (int i = 0; i < currentTransforms.Length; i++)
        {
            var currentTransformData = snapshot.transformData[i];
            var currentTransform = currentTransforms[i];

            // 比较位置、旋转和缩放数据
            if (currentTransform.localPosition != currentTransformData.localPosition ||
                currentTransform.localRotation != currentTransformData.localRotation ||
                currentTransform.localScale != currentTransformData.localScale)
            {
                return false; // 一旦发现任何不匹配，则立即返回false
            }
        }

        return true; // 所有变换数据完全匹配
    }


    private void ApplySnapshot(TransformSnapshot snapshot)
    {
        if (targetObject == snapshot.targetObject)
        {
            snapshot.Apply();
            Debug.Log("Snapshot applied: " + snapshot.name);
        }
        else
        {
            Debug.Log(Selection.activeGameObject);
            Debug.Log(snapshot.targetObject);
            Debug.LogWarning("Cannot apply snapshot. Please select the correct GameObject.");
        }
    }


    private void OnInspectorUpdate()
    {
        Repaint();
    }

    private string GenerateFileName(GameObject gameObject)
    {
        string fileName = gameObject.name + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".json";
        return Path.Combine(saveDirectoryPath, fileName);
    }

    private void SaveSnapshots()
    {
        foreach (var snapshot in snapshots)
        {
            SaveSnapshotToFile(snapshot);
        }
    }

    private void SaveSnapshotToFile(TransformSnapshot snapshot)
    {
        if (!Directory.Exists(saveDirectoryPath))
        {
            Directory.CreateDirectory(saveDirectoryPath);
        }

        string fileName = GenerateFileName(snapshot.targetObject);
        SnapshotData snapshotData = new SnapshotData();
        snapshotData.snapshots = new List<TransformSnapshotData>();
        snapshotData.snapshots.Add(new TransformSnapshotData(snapshot));

        string json = JsonUtility.ToJson(snapshotData, true);
        File.WriteAllText(fileName, json);
        Debug.Log("Snapshot saved to file: " + fileName);
    }

    private void LoadSnapshots()
    {
        snapshots.Clear(); // 清除旧的快照列表

        string[] snapshotFiles = Directory.GetFiles(saveDirectoryPath, "*.json");

        foreach (var snapshotFile in snapshotFiles)
        {
            string json = File.ReadAllText(snapshotFile);
            SnapshotData snapshotData = JsonUtility.FromJson<SnapshotData>(json);

            foreach (var snapshotInfo in snapshotData.snapshots)
            {
                int instanceID = snapshotInfo.targetObject.GetInstanceID(); // 假设这是JSON中对应字段的路径
                int TargetObjcectID = targetObject?.GetInstanceID() ?? -1;

                if (instanceID == TargetObjcectID)
                {
                    var snapshot = new TransformSnapshot(targetObject, snapshotInfo.name);
                    snapshot.LoadFromSnapshotData(snapshotInfo);
                    snapshots.Add(snapshot);
                }
                else
                {
                    Debug.LogWarning($"Cannot find GameObject with InstanceID: {instanceID}");
                }
            }
        }
    }

    private GameObject FindGameObjectByInstanceID(int instanceID)
    {
        foreach (var go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
        {
            if (go.GetInstanceID() == instanceID)
            {
                return go;
            }
        }
        return null; // 如果没有找到匹配的InstanceID，则返回null
    }


    private void OnEnable()
    {
        // Automatically load snapshots when the window is opened
        LoadSnapshots();
    }

    //private void OnHierarchyChange()
    //{
    //    // Automatically save a snapshot when a new object is dragged into the window
    //    if (targetObject != null)
    //    {
    //        RecordSnapshot();
    //    }
    //}
}

[System.Serializable]
public class TransformSnapshot
{
    public string name;
    public GameObject targetObject;
    public List<TransformData> transformData = new List<TransformData>();

    [System.Serializable]
    public class TransformData
    {
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;
    }

    public TransformSnapshot(GameObject target, string objectName)
    {
        targetObject = target;
        RecordSnapshot();
        name = "Snapshot_" + objectName + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
    }


    public TransformSnapshot(GameObject target)
    {
        targetObject = target;
        RecordSnapshot();
        name = "Snapshot_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
    }

    private void RecordSnapshot()
    {
        if (targetObject != null)
        {
            Transform[] transforms = targetObject.GetComponentsInChildren<Transform>();

            foreach (var transform in transforms)
            {
                var data = new TransformData
                {
                    localPosition = transform.localPosition,
                    localRotation = transform.localRotation,
                    localScale = transform.localScale
                };
                transformData.Add(data);
            }
        }
    }

    public void Apply()
    {
        if (targetObject != null && transformData != null)
        {
            Transform[] transforms = targetObject.GetComponentsInChildren<Transform>();

            for (int i = 0; i < Mathf.Min(transforms.Length, transformData.Count); i++)
            {
                transforms[i].localPosition = transformData[i].localPosition;
                transforms[i].localRotation = transformData[i].localRotation;
                transforms[i].localScale = transformData[i].localScale;
            }
        }
    }

    public void LoadFromSnapshotData(TransformSnapshotData snapshotData)
    {
        name = snapshotData.name;
        targetObject = snapshotData.targetObject;
        transformData = snapshotData.transformData;
    }
}

[System.Serializable]
public class TransformSnapshotData
{
    public string name;
    public GameObject targetObject;
    public List<TransformSnapshot.TransformData> transformData;

    public TransformSnapshotData(TransformSnapshot snapshot)
    {
        name = snapshot.name;
        targetObject = snapshot.targetObject;
        transformData = snapshot.transformData;
    }
}

[System.Serializable]
public class SnapshotData
{
    public List<TransformSnapshotData> snapshots;
}