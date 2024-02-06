using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;

public class CSVReader
{
    private Dictionary<string, List<Dictionary<string, object>>> data;
    private string csvFolderPath;

    public CSVReader()
    {
        data = new Dictionary<string, List<Dictionary<string, object>>>();
        // 默认的CSV文件夹路径，您可以根据需要修改
        csvFolderPath = Path.Combine(Application.dataPath, "Data", "Table");
    }

public void LoadCSV(string csvFileName)
{
    string csvFilePath = Path.Combine(csvFolderPath, csvFileName);
    if (!File.Exists(csvFilePath))
    {
        Debug.LogError("CSV file not found: " + csvFilePath);
        return;
    }

    string[] lines = File.ReadAllLines(csvFilePath);
    string[] headers = lines[0].Trim().Split(','); // 使用第一行作为变量名
    string[] types = lines[1].Trim().Split(','); // 使用第二行作为变量类型

    List<Dictionary<string, object>> dataList = new List<Dictionary<string, object>>();

    for (int i = 2; i < lines.Length; i++) // 从第三行开始读取数据
    {
        string[] fields = lines[i].Trim().Split(',');
        Dictionary<string, object> entry = new Dictionary<string, object>();
        for (int j = 0; j < headers.Length; j++)
        {
            string valueString = fields[j].Trim();
            object value = ParseValue(types[j], valueString); // 解析值，根据每列的变量类型进行转换
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
        case "string":
        default:
            return valueString;
    }
}


    public Dictionary<string, object> GetDataByID(string csvFileName, string id)
    {
        if (!data.ContainsKey(csvFileName))
        {
            LoadCSV(csvFileName + ".csv");
        }

        if (data.ContainsKey(csvFileName))
        {
            foreach (var entry in data[csvFileName])
            {
                // 这里假设 ID 是字符串类型
                if (entry.ContainsKey("ID") && entry["ID"].ToString() == id)
                    return entry;
            }
        }
        return null;
    }

}
