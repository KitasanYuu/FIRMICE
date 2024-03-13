using CustomInspector;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ResourceReader
{
    public string GainPath(string ObjectType, string ObjectName)
    {
        GlobalObjectPathSetting GOPS = Resources.Load<GlobalObjectPathSetting>("GlobalSettings/GlobalObjectPathSetting");
        System.Reflection.FieldInfo field = GOPS.GetType().GetField(ObjectType);
        FolderPath folderPath = (FolderPath)field.GetValue(GOPS);
        string resourcesFolderPath = "Assets/Resources/";
        string FolderPath = folderPath.path;

        // 使用 Path.Combine() 来组合路径
        string FullPath = Path.Combine(FolderPath, ObjectName);
        FullPath = FullPath.Replace("\\", "/");
        // 为了得到相对路径，去除资源路径前缀
        string relativePath = FullPath.Replace(resourcesFolderPath, "");

        //Debug.Log(relativePath);
        return relativePath;
    }

    public TextAsset GetCSVFile(string CSVName)
    {
        TextAsset csv = null;
        string CsvResourcePath = GainPath("DataPath", CSVName);
        csv = Resources.Load<TextAsset>(CsvResourcePath);
        return csv;
    }

    public GameObject GetGameObject(string ObjectType ,string ObjectName)
    {
        GameObject ReturnGameObject = null;
        string ObjectResourcePath = GainPath(ObjectType, ObjectName);
        ReturnGameObject = Resources.Load<GameObject>(ObjectResourcePath);
        return ReturnGameObject;
    }

}
