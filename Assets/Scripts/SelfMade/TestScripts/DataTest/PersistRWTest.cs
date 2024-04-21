using UnityEngine;
using System.IO;
using System.Security.Cryptography;
using YDataPersistence;
using System;
using System.Text;
using DataManager;
using YuuTool;
using System.Collections.Generic;

public static class PersistRWTest
{

    public static void SavePlayerData(PersistDataClass data, string fileName)
    {
        string path = GetSaveFilePath(fileName);
        string json = JsonUtility.ToJson(data);

        if (ResourceReader.GetEncryptionOption().UsingAes)
        {
            // Read encryption key from ScriptableObject
            string encryptionKey = ResourceReader.GetEncryptionOption().AesKey;
            json = Encrypt(json, encryptionKey);
        }

        File.WriteAllText(path, json);
    }

    public static PersistDataClass LoadPlayerData(string fileName)
    {
        string path = GetSaveFilePath(fileName);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);

            if (ResourceReader.GetEncryptionOption().UsingAes)
            {
                // Read encryption key from ScriptableObject
                string encryptionKey = ResourceReader.GetEncryptionOption().AesKey;
                json = Decrypt(json, encryptionKey);
            }

            return JsonUtility.FromJson<PersistDataClass>(json);
        }
        else
        {
            Debug.LogWarning("Player data file not found.");
            return null;
        }
    }

    public static void DestorySave(string VerifyCode)
    {
        string path = GetSaveFilePath();
        if (Directory.Exists(path))
        {
            string[] filePaths = Directory.GetFiles(path, "*.Ysave");

            foreach (string filePath in filePaths)
            {
                string json = File.ReadAllText(filePath);
                PersistDataClass data = JsonUtility.FromJson<PersistDataClass>(json);
                if(data.cryptographicCheckCode == VerifyCode)
                {
                    File.Delete(filePath);
                }
            }
        }
    }

    public static List<PersistDataClass> GetAllSave()
    {
        string path = GetSaveFilePath();
        if (Directory.Exists(path))
        {
            string[] filePaths = Directory.GetFiles(path, "*.Ysave");
            List<PersistDataClass> saveDataList = new List<PersistDataClass>();

            foreach (string filePath in filePaths)
            {
                string json = File.ReadAllText(filePath);
                PersistDataClass data = JsonUtility.FromJson<PersistDataClass>(json);
                saveDataList.Add(data);
            }
            return saveDataList;
        }
        else
        {
            Debug.LogWarning("Player data file not found.");
            return null;
        }
    }

    private static string GetSaveFilePath(string fileName = null)
    {
        string folderPath = YTool.CreateFolder("Saver");


        if(fileName != null)
        {
            string _fileName = fileName + ".YSave";

            return Path.Combine(folderPath, _fileName);
        }
        else
        {
            return folderPath;
        }

    }

    // AES encryption method
    private static string Encrypt(string plainText, string key)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Encoding.UTF8.GetBytes(key);
            aesAlg.Mode = CipherMode.ECB; // Use ECB mode for simplicity (not recommended for large data)
            aesAlg.Padding = PaddingMode.PKCS7;

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                }
                return Convert.ToBase64String(msEncrypt.ToArray());
            }
        }
    }

    // AES decryption method
    private static string Decrypt(string cipherText, string key)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Encoding.UTF8.GetBytes(key);
            aesAlg.Mode = CipherMode.ECB;
            aesAlg.Padding = PaddingMode.PKCS7;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }
    }
}
