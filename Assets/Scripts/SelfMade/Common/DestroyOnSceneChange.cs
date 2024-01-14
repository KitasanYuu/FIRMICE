using UnityEngine;
using UnityEngine.SceneManagement;

public class DestroyOnSceneChange : MonoBehaviour
{
    private void Awake()
    {
        // 订阅场景卸载事件
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDestroy()
    {
        // 取消订阅场景卸载事件
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnSceneUnloaded(Scene scene)
    {
        // 在场景卸载时销毁自身所在的游戏对象
        Destroy(gameObject);
    }
}
