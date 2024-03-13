using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuBackGroundSwitcher : MonoBehaviour
{
    public string BackGroundSeceneName;

    void Start()
    {
        LoadSceneAdditive(BackGroundSeceneName);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LoadSceneAdditive(string SceneToLoad)
    {
        // 使用SceneManager加载指定名称的场景，并以附加模式加载
        SceneManager.LoadScene(SceneToLoad, LoadSceneMode.Additive);
    }
}
