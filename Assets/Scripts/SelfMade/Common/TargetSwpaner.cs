using UnityEngine;
using UnityEngine.SceneManagement;

public class TargetSwpaner : MonoBehaviour
{
    public GameObject prefabToGenerate; // 预制体
    public string respawnObjectName = "Reswpan"; // Reswpan物体的名称


    private void Awake()
    {
        // 订阅场景加载完成事件
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // 取消订阅场景加载完成事件以防止内存泄漏
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 在新场景加载完成后生成预制体
        if (prefabToGenerate != null)
        {
            // 查找名为"Reswpan"的物体
            GameObject respawnObject = GameObject.Find(respawnObjectName);

            // 如果找到了"Reswpan"物体，获取其位置并生成预制体
            if (respawnObject != null)
            {
                Vector3 spawnPosition = respawnObject.transform.position;
                Instantiate(prefabToGenerate, spawnPosition, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning("Could not find object with the name " + respawnObjectName);
            }
        }
    }
}
