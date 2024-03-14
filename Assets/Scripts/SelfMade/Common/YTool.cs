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

    }
}
