using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RefreshOnNextFrame : MonoBehaviour
{
    public List<GameObject> GameObjectNeedRefresh = new List<GameObject>();
    // 记录 GameObject 初始状态的字典
    private Dictionary<GameObject, bool> initialObjectStates = new Dictionary<GameObject, bool>();
    // Start is called before the first frame update
    void OnEnable()
    {
        StartCoroutine(RefreshGameObjects());
    }

    IEnumerator RefreshGameObjects()
    {
        // 等待下一帧
        yield return null;

        // 禁用并重新启用游戏对象，确保在下一帧正确更新状态
        foreach (GameObject go in GameObjectNeedRefresh)
        {
            if(go != null)
            {
                // 首次刷新前记录初始状态
                if (!initialObjectStates.ContainsKey(go))
                {
                    initialObjectStates[go] = go.activeSelf;
                }

                // 颠倒状态
                go.SetActive(!initialObjectStates[go]);
                go.SetActive(initialObjectStates[go]);
            }
        }
    }
}
