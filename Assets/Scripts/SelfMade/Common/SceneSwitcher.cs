using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    [SerializeField]
    private Button switchButton; // UI Button组件


    void Start()
    {
        // 加载起始场景
        //SceneManager.LoadScene(SceneName);
    }

    // 用于接收场景名称的方法
    //public void SwitchToScene(string sceneName)
    //{
    //    // 切换到指定的场景
    //    SceneManager.LoadScene(sceneName);
    //}

    public void StartTransition(string transitionSceneName)
    {
        // 异步加载过渡场景
        SceneManager.LoadSceneAsync(transitionSceneName);
    }

    // 这个方法可以在过渡场景中的某个动画事件中调用，表示过渡动画结束
    public void EndTransition(string sceneName)
    {
        // 异步加载目标场景
        SceneManager.LoadSceneAsync(sceneName);
    }
}
