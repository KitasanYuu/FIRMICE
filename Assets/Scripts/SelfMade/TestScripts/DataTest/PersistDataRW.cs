using UnityEngine;
using System.IO;
using System.Security.Cryptography;
using YDataPersistence;
using System;
using System.Text;
using DataManager;
using YuuTool;
using System.Collections.Generic;
using System.Linq;

namespace YDataPersistence
{
    public static class PersistDataRW
    {
        public static void SavePlayerData(PersistDataClass data, string fileName)
        {
            // 获取保存文件的路径
            string path = GetSaveFilePath();

            // 如果目录不存在，则创建
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // 检查是否已存在具有相同 cryptographicCheckCode 的数据文件
            string[] existingFiles = Directory.GetFiles(path, "*.ysav");
            foreach (string existingFile in existingFiles)
            {
                string json = File.ReadAllText(existingFile);

                // 如果启用了数据加密选项，则先解密数据
                if (IsEncrypted(json))
                {
                    // 从ScriptableObject中读取加密密钥
                    string encryptionKey = ResourceReader.GetEncryptionOption().AesKey;
                    json = Decrypt(json, encryptionKey);
                }

                PersistDataClass existingData = JsonUtility.FromJson<PersistDataClass>(json);
                if (existingData.cryptographicCheckCode == data.cryptographicCheckCode)
                {
                    if (ResourceReader.GetEncryptionOption().UsingAes)
                    {
                        // 如果找到与要保存的数据具有相同 cryptographicCheckCode 的文件，则覆盖该文件并直接返回
                        File.WriteAllText(existingFile, Encrypt(JsonUtility.ToJson(data), ResourceReader.GetEncryptionOption().AesKey));
                    }
                    else
                    {
                        File.WriteAllText(existingFile, JsonUtility.ToJson(data));
                    }

                    return;
                }
            }

            // 将数据转换为JSON字符串
            string jsonData = JsonUtility.ToJson(data);

            // 如果启用了数据加密选项，则对数据进行加密
            if (ResourceReader.GetEncryptionOption().UsingAes)
            {
                // 从ScriptableObject中读取加密密钥
                string encryptionKey = ResourceReader.GetEncryptionOption().AesKey;
                jsonData = Encrypt(jsonData, encryptionKey);
            }

            string _fileName = fileName;

            if (ResourceReader.GetEncryptionOption().DataNameEncryption)
            {
                _fileName = YTool.SeedGenerateRandomString(fileName, 8);
            }

            // 拼接最终的保存文件路径
            string filePath = Path.Combine(path, _fileName + ".ysav");

            // 将加密后的JSON字符串写入文件
            File.WriteAllText(filePath, jsonData);
        }


        public static PersistDataClass LoadPlayerData(string VerifyCode)
        {
            foreach (PersistDataClass pdc in GetAllSave(true))
            {
                if (pdc.cryptographicCheckCode == VerifyCode)
                {
                    return pdc;
                }
            }

            Debug.LogWarning("Player data file not found.");
            return null;
            //string path = GetSaveFilePath(VerifyCode);
            //if (File.Exists(path))
            //{
            //    string json = File.ReadAllText(path);

            //    if (ResourceReader.GetEncryptionOption().UsingAes)
            //    {
            //        // Read encryption key from ScriptableObject
            //        string encryptionKey = ResourceReader.GetEncryptionOption().AesKey;
            //        json = Decrypt(json, encryptionKey);
            //    }

            //    return JsonUtility.FromJson<PersistDataClass>(json);
            //}
            //else
            //{
            //    Debug.LogWarning("Player data file not found.");
            //    return null;
            //}
        }

        public static void DestroySave(string VerifyCode)
        {
            string path = GetSaveFilePath();
            if (Directory.Exists(path))
            {
                string[] filePaths = Directory.GetFiles(path, "*.ysav");

                foreach (string filePath in filePaths)
                {
                    string json = File.ReadAllText(filePath);

                    // 如果启用了数据加密选项，则先尝试解密数据
                    if (IsEncrypted(json))
                    {
                        // 从ScriptableObject中读取加密密钥
                        string encryptionKey = ResourceReader.GetEncryptionOption().AesKey;
                        json = Decrypt(json, encryptionKey);
                    }

                    PersistDataClass data = JsonUtility.FromJson<PersistDataClass>(json);
                    if (data.cryptographicCheckCode == VerifyCode)
                    {
                        File.Delete(filePath);
                    }
                }
            }
        }


        public static List<PersistDataClass> GetAllSave(bool decrypt = false)
        {
            string path = GetSaveFilePath();
            if (Directory.Exists(path))
            {
                string[] filePaths = Directory.GetFiles(path, "*.ysav");
                List<PersistDataClass> saveDataList = new List<PersistDataClass>();

                foreach (string filePath in filePaths)
                {
                    string json = File.ReadAllText(filePath);
                    bool isEncrypted = IsEncrypted(json);

                    // Check if decryption is needed and if the file is encrypted
                    if (decrypt && isEncrypted)
                    {
                        json = Decrypt(json, ResourceReader.GetEncryptionOption().AesKey); // Assuming AESKey is accessible here
                    }

                    PersistDataClass data = JsonUtility.FromJson<PersistDataClass>(json);
                    saveDataList.Add(data);
                }
                return saveDataList;
            }
            else
            {
                Debug.LogWarning("Player data directory not found.");
                return null;
            }
        }

        public static bool IsEncrypted(string json)
        {
            try
            {
                Decrypt(json, ResourceReader.GetEncryptionOption().AesKey); // 尝试解密文件内容
                return true; // 如果解密成功，则文件已加密
            }
            catch
            {
                return false; // 解密失败，文件未加密
            }
        }



        private static string GetSaveFilePath(string fileName = null)
        {
            string folderPath = YTool.CreateFolder("Saver");


            if (fileName != null)
            {
                string _fileName = fileName + ".ysav";

                return Path.Combine(folderPath, _fileName);
            }
            else
            {
                return folderPath;
            }

        }

        // AES encryption method with CBC mode
        private static string Encrypt(string plainText, string key)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key);
                aesAlg.Mode = CipherMode.CBC; // Use CBC mode for encryption
                aesAlg.Padding = PaddingMode.PKCS7;

                // Generate a random IV (initialization vector)
                aesAlg.IV = GenerateRandomIV();

                byte[] encryptedData;

                // Encrypt the plaintext
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV), CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }
                    encryptedData = msEncrypt.ToArray();
                }

                // Concatenate IV and encrypted data
                byte[] ivAndEncryptedData = aesAlg.IV.Concat(encryptedData).ToArray();

                byte[] rawData = ivAndEncryptedData;
                string base64String = Convert.ToBase64String(rawData);


                string finaldata = ResourceReader.GetEncryptionOption().EncryptionMark + "_kYuu¿SN" + base64String;
                // Return the concatenated IV and encrypted data as Base64 string
                return finaldata;
            }
        }




        // AES decryption method with CBC mode
        private static string Decrypt(string cipherText, string key)
        {
            string[] parts = cipherText.Split("_kYuu¿SN"); // Split the cipherText into parts
            if (parts.Length != 2)
            {
                throw new Exception("Invalid encryption format.");
            }

            string encryptionMark = ResourceReader.GetEncryptionOption().EncryptionMark;
            if (parts[0] != encryptionMark)
            {
                throw new Exception("Invalid encryption mark.");
            }

            byte[] ivAndEncryptedData = Convert.FromBase64String(parts[1]);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key);
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                // Extract IV from the beginning of the byte array
                byte[] iv = ivAndEncryptedData.Take(aesAlg.BlockSize / 8).ToArray();
                aesAlg.IV = iv;

                // Extract encrypted data
                byte[] encryptedData = ivAndEncryptedData.Skip(aesAlg.BlockSize / 8).ToArray();

                // Decrypt the encrypted data
                using (MemoryStream msDecrypt = new MemoryStream(encryptedData))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV), CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }

        // Method to generate a random IV
        private static byte[] GenerateRandomIV()
        {
            byte[] iv = new byte[16]; // AES block size is 128 bits (16 bytes)
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(iv);
            }
            return iv;
        }

    }
}