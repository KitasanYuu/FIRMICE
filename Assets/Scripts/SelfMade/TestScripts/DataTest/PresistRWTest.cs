using UnityEngine;
using System.IO;
using YDataPresisting;

public static class PresistRWTest
{
    public static void SavePlayerData(PlayerData data, string fileName)
    {
        string path = GetSaveFilePath(fileName);
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(path, json);
    }

    public static PlayerData LoadPlayerData(string fileName)
    {
        string path = GetSaveFilePath(fileName);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<PlayerData>(json);
        }
        else
        {
            Debug.LogWarning("Player data file not found.");
            return null;
        }
    }

    private static string GetSaveFilePath(string fileName)
    {
        return Application.persistentDataPath + "/" + fileName + ".json";
    }
}