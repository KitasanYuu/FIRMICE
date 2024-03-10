// SceneNameCapture.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

namespace SceneTools
{
    public class SceneNameCapture : MonoBehaviour
    {
        // 声明一个公共事件
        public event Action<string> OnSceneLoaded;

        // 获取并返回当前场景名
        public string GetCurrentSceneName()
        {
            return SceneManager.GetActiveScene().name;
        }

        // 在脚本启动时订阅 sceneLoaded 事件
        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoadedInternal;

            // 手动调用一次，确保在第一个场景时也执行
            string currentSceneName = GetCurrentSceneName();
            OnSceneLoadedInternal(new Scene(), LoadSceneMode.Single);
        }

        // 在脚本禁用时取消订阅 sceneLoaded 事件
        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoadedInternal;
        }

        // 场景加载完成时的回调函数
        private void OnSceneLoadedInternal(Scene scene, LoadSceneMode mode)
        {
            string currentSceneName = scene.name;

            // 触发公共事件，通知订阅者场景加载完成
            OnSceneLoaded?.Invoke(currentSceneName);

            Debug.Log("Scene Loaded: " + currentSceneName);
        }
    }
}
