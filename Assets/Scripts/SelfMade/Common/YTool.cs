using CustomInspector;
using System.IO;
using System.Text;
using UnityEngine;

namespace YuuTool
{
    public static class YTool
    {
        public static Transform FindDeepChild(this Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == childName)
                    return child;

                Transform found = FindDeepChild(child, childName);
                if (found != null)
                    return found;
            }
            return null;
        }
        public static string GenerateRandomString(int length)
        {
            System.Random random = new System.Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            StringBuilder builder = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                builder.Append(chars[random.Next(chars.Length)]);
            }

            return builder.ToString();
        }

        // 扩展方法来检测对象是否为“Missing”
        public static bool IsMissing(this UnityEngine.Object obj)
        {
            // 对象逻辑上等于null但并非真正的null，意味着它是一个被Unity销毁的对象
            return obj == null && !ReferenceEquals(obj, null);
        }

        // 扩展方法来确认对象是否存在且未被销毁
        public static bool IsReallyNotNull(this UnityEngine.Object obj)
        {
            // 对象不等于null且不是“Missing”，意味着它存在且未被销毁
            return obj != null;
        }

        public static string CreateFolder(string folderName ,string tfolderPath = null,bool needClearPath = false)
        {
            string _TfolderPath = tfolderPath == null ? Application.persistentDataPath : tfolderPath;
            string _CfolderPath = Path.Combine(_TfolderPath, folderName);

            // 检查新文件夹是否已存在
            if (!Directory.Exists(_CfolderPath))
            {
                // 创建新文件夹
                Directory.CreateDirectory(_CfolderPath);
            }
            else
            {
                if (needClearPath)
                {
                    ClearFolder(_CfolderPath);
                }
            }

            return _CfolderPath;
        }

        public static void ClearFolder(string folderPath)
        {
            // 获取路径中的所有文件
            string[] files = Directory.GetFiles(folderPath);

            // 删除所有文件
            foreach (string file in files)
            {
                File.Delete(file);
            }
        }

    }
}
