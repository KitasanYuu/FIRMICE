#pragma warning disable 0618
using UnityEngine;
using System.Collections.Generic;

namespace SceneTools
{
    public class SceneComponentSwitch : MonoBehaviour
    {
        public bool enableScenes = true;
        public bool disableScenes = false;
        [SerializeField] private GameObject target;

        public List<string> scenesToEnable = new List<string>();
        public List<string> scenesToDisable = new List<string>();

        private SceneNameCapture sceneNameCapture;

        private bool isExitingScene = false; // 新增标志，表示是否正在退出场景
        private bool initialTargetState; // 新增变量，用于记录 target 的初始状态

        private void Start()
        {
            // 获取 SceneNameCapture 组件
            sceneNameCapture = FindObjectOfType<SceneNameCapture>();

            if (sceneNameCapture == null)
            {
                Debug.LogError("SceneNameCapture not found!");
            }
            else
            {
                // 订阅 OnSceneLoaded 事件
                sceneNameCapture.OnSceneLoaded += OnSceneLoaded;

                // 手动调用一次 OnSceneLoaded 方法，以确保第一个场景的正确处理
                OnSceneLoaded(sceneNameCapture.GetCurrentSceneName());

                // 记录 target 的初始状态
                initialTargetState = target.activeSelf;
            }
        }

        private void OnDestroy()
        {
            if (sceneNameCapture != null)
            {
                // 在脚本销毁时取消订阅事件
                sceneNameCapture.OnSceneLoaded -= OnSceneLoaded;
            }
        }

        // OnSceneLoaded 方法将被添加到 OnSceneLoaded 事件的委托中
        private void OnSceneLoaded(string currentSceneName)
        {
            // 如果正在退出场景，则将 target 的状态恢复到默认状态
            if (isExitingScene)
            {
                target.SetActive(initialTargetState); // 恢复到初始状态
                isExitingScene = false;
            }

            // 根据场景名判断是否需要启用或禁用
            if (enableScenes && scenesToEnable.Contains(currentSceneName))
            {
                Debug.Log("Enabling scene: " + currentSceneName);
                target.SetActive(true);
                isExitingScene = true; // 设置退出场景标志为 true
            }

            if (disableScenes && scenesToDisable.Contains(currentSceneName))
            {
                Debug.Log("Disabling scene: " + currentSceneName);
                target.SetActive(false);
                isExitingScene = true; // 设置退出场景标志为 true
            }

        }

        private void Update()
        {
            Debug.LogWarning(target.active);
        }
    }
}
