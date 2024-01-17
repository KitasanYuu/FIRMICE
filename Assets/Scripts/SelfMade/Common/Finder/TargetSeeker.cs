#pragma warning disable 0618
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using CustomInspector;

namespace TargetFinding
{
    public class TargetSeeker : MonoBehaviour
    {
        public bool searchInactiveObjects = false; // 是否搜索未激活的物体
        public string objectNameToFind = ""; // 要查找的物体的名称
        [Tag]public string objectTagToFind = ""; // 要查找的物体的标签
        public LayerMask objectLayerToFind; // 用于存储要查找的层级
        public List<string> scenesToDeactivateScript; // 存储要禁用脚本的场景名称列表

        [ReadOnly] public GameObject foundObject; // 用于存储找到的物体的引用
        private string currentSceneName; // 用于存储当前场景名称

        private void Start()
        {
            // 获取初始场景的名称
            currentSceneName = SceneManager.GetActiveScene().name;
        }

        private void Update()
        {
            if (!scenesToDeactivateScript.Contains(currentSceneName))
            {
                int conditionsMet = 0;

                if (!string.IsNullOrEmpty(objectTagToFind))
                    conditionsMet++;

                if (objectLayerToFind != 0)
                    conditionsMet++;

                if (!string.IsNullOrEmpty(objectNameToFind))
                    conditionsMet++;

                if (conditionsMet >= 2)
                {
                    ObjectFinding();
                }
                else
                {
                    Debug.LogError($"脚本在物体: {gameObject.name} 上执行搜索时，至少需要输入两项条件才能执行搜索！");
                }
            }
        }

        private void ObjectFinding()
        {
            // 使用LayerMask的value属性来获取所选层级的整数值
            int layerMaskValue = objectLayerToFind.value;

            // 使用新的方法来查找匹配的物体，根据指定的层级和标签
            GameObject[] objectsOfType;
            if (searchInactiveObjects)
            {
                objectsOfType = Resources.FindObjectsOfTypeAll<GameObject>();
            }
            else
            {
                objectsOfType = GameObject.FindObjectsOfType<GameObject>();
            }

            foreach (GameObject obj in objectsOfType)
            {
                // 检查是否匹配指定的标签和层级
                if ((string.IsNullOrEmpty(objectTagToFind) || obj.CompareTag(objectTagToFind)) &&
                    (layerMaskValue == 0 || (layerMaskValue & (1 << obj.layer)) != 0) &&
                    (string.IsNullOrEmpty(objectNameToFind) || obj.name == objectNameToFind))
                {
                    // 找到匹配的物体，将其引用存储在 foundObject 中并退出循环
                    foundObject = obj;
                    break;
                }
            }
        }

        public void SetStatus(bool startStatus)
        {
            enabled = startStatus;
        }

        // 在场景切换后调用
        private void OnSceneChanged(Scene scene, LoadSceneMode mode)
        {
            // 更新当前场景名称
            currentSceneName = scene.name;
        }

        private void OnEnable()
        {
            // 注册场景切换回调
            SceneManager.sceneLoaded += OnSceneChanged;
        }

        private void OnDisable()
        {
            // 取消注册场景切换回调
            SceneManager.sceneLoaded -= OnSceneChanged;
        }
    }
}

