using UnityEngine;
using UnityEngine.SceneManagement;

public class InitPlane : MonoBehaviour
{
    [SerializeField]
    private GameObject targetToDisable; // 要设置为false的目标

    private void Start()
    {
        // 添加场景加载完成后的事件监听器
        SceneManager.sceneLoaded += OnSceneLoaded;

    }


    // 当场景加载完成后执行的方法
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 在这里设置目标为false
        if (targetToDisable != null)
        {
            targetToDisable.SetActive(false);
        }

        // 移除事件监听，如果不再需要
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
