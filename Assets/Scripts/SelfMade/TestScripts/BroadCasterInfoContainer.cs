using UnityEngine;
using System;
using CustomInspector;

namespace TestField
{
    public class BroadCasterInfoContainer : MonoBehaviour
    {
        // 定义事件委托
        public Action<GameObject> TargetReceivedChanged;

        [HideInInspector]
        public GameObject TargetReceived;

        [ReadOnly]
        public GameObject TargetContainer;

        void Start()
        {
            // 在 Start 方法中进行其他初始化操作
        }

        // Update is called once per frame
        void Update()
        {
            // 在 Update 方法中进行逻辑更新（如果需要的话）
        }

        public void TargetBoardCast(GameObject newTarget)
        {
            // 如果新目标与当前目标不同，触发事件
            if (newTarget != TargetContainer)
            {
                TargetContainer = newTarget;
                TargetReceived = TargetContainer;

                // 触发事件通知其他脚本
                TargetReceivedChanged?.Invoke(TargetReceived);
            }
        }
    }
}
