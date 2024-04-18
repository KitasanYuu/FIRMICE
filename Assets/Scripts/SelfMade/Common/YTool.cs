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

    }
}
