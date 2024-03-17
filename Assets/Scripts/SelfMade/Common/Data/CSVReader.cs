using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;

namespace DataManager
{
    public class CSVReader
    {
        private Dictionary<string, List<Dictionary<string, object>>> data;

        public CSVReader()
        {
            data = new Dictionary<string, List<Dictionary<string, object>>>();
        }

        public void LoadCSV(string csvFileName)
        {
            ResourceReader RR = new ResourceReader();
            string CsvResourcePath = RR.GainPath("DataPath", csvFileName);
            TextAsset csvFile = RR.GetCSVFile(csvFileName);

            if (csvFile == null)
            {
                Debug.LogError("CSV file not found in Resources: " + CsvResourcePath);
                return;
            }

            string[] lines = csvFile.text.Split('\n');
            string[] headers = lines[0].Trim().Split(',');
            string[] types = lines[1].Trim().Split(',');

            List<Dictionary<string, object>> dataList = new List<Dictionary<string, object>>();

            for (int i = 2; i < lines.Length; i++)
            {
                string[] fields = lines[i].Trim().Split(',');
                if (fields.Length < headers.Length) // 检查字段数量是否少于标题数量
                {
                    Debug.LogWarning(csvFileName+":数据行字段数量少于标题数量，跳过此行: " + "Line:"+i);
                    continue; // 跳过这行
                }

                Dictionary<string, object> entry = new Dictionary<string, object>();
                for (int j = 0; j < headers.Length; j++)
                {
                    string valueString = fields[j].Trim();
                    object value;
                    if (j < types.Length) // 确保类型数组也足够长
                    {
                        value = ParseValue(types[j], valueString);
                    }
                    else
                    {
                        Debug.LogWarning(csvFileName + ":类型定义行字段数量少于标题数量，使用默认类型解析值");
                        value = ParseValue("defaultType", valueString); // 使用一个默认类型处理方法或者直接赋值
                    }
                    entry[headers[j]] = value;
                }
                dataList.Add(entry);
            }


            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(csvFileName);
            data[fileNameWithoutExtension] = dataList;
        }

        private object ParseValue(string type, string valueString)
        {
            // 根据变量类型进行解析，并返回相应的数据类型
            switch (type)
            {
                case "int":
                    return int.Parse(valueString);
                case "float":
                    return float.Parse(valueString);
                case "bool":
                    return bool.Parse(valueString);
                case "Vector2": // 解析 Vector2 类型
                    string[] components = valueString.Trim('(', ')').Split(',');
                    if (components.Length == 2)
                    {
                        float x = float.Parse(components[0]);
                        float y = float.Parse(components[1]);
                        return new Vector2(x, y);
                    }
                    else
                    {
                        //Debug.LogError("Invalid Vector2 format: " + valueString);
                        return Vector2.zero; // 或者返回其他默认值
                    }
                case "Vector3": // 解析 Vector3 类型
                    components = valueString.Trim('(', ')').Split(',');
                    if (components.Length == 3)
                    {
                        float x = float.Parse(components[0]);
                        float y = float.Parse(components[1]);
                        float z = float.Parse(components[2]);
                        return new Vector3(x, y, z);
                    }
                    else
                    {
                        //Debug.LogError("Invalid Vector3 format: " + valueString);
                        return Vector3.zero; // 或者返回其他默认值
                    }
                case "string":
                default:
                    return valueString;
            }
        }

        public Dictionary<string, object> GetDataByID(string csvFileName, string id)
        {
            csvFileName = csvFileName.Trim();
            id = id.Trim();

            if(csvFileName == "role")
            {
                id = RoleTableCidModify(id);
            }

            if (!data.ContainsKey(csvFileName))
            {
                LoadCSV(csvFileName);
            }

            if (data.ContainsKey(csvFileName))
            {
                foreach (var entry in data[csvFileName])
                {
                    // 这里假设 ID 是字符串类型
                    if (entry.ContainsKey("GID") && entry["GID"].ToString() == id)
                        return entry;
                }
            }
            return null;
        }

        public static string RoleTableCidModify(string cid)
        {
            // 检查字符串的最后一个字符是否为数字
            if (cid.Length > 0 && char.IsDigit(cid[cid.Length - 1]))
            {
                // 如果以数字结尾，保留字符串
                Console.WriteLine("字符串以数字结尾，保持不变： " + cid);
                return cid;
            }
            else
            {
                // 如果不以数字结尾，删除最后两个字符（如果长度允许）
                string modifiedString = cid.Length > 2 ? cid.Substring(0, cid.Length - 2) : string.Empty;
                Console.WriteLine("字符串不以数字结尾，修改后的字符串： " + modifiedString);
                return modifiedString;
            }
        }
    }
}
