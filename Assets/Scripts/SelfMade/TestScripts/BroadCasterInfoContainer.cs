using UnityEngine;
using System;
using CustomInspector;
using AvatarMain;
using playershooting;

namespace TestField
{
    public class BroadCasterInfoContainer : MonoBehaviour
    {
        // 定义事件委托
        public Action<GameObject> TargetReceivedChanged;
        public Action<int> TargetMovingStatusChanged;

        //事件传进来的参数
        [ReadOnly]
        public GameObject TargetContainer;

        //供外部调用的参数
        [HideInInspector]
        public GameObject TargetReceived;

        //传入Object的移动状态：-1表潜伏，0表停止，1表正常移动，2表奔跑，3表速度超过其他奔跑速度
        [ReadOnly]
        public int TargetmovingStatus;
        [ReadOnly]
        public bool isAiming;
        [ReadOnly]
        public bool Fire;

        [ReadOnly]public AvatarController avatarController;
        [ReadOnly]public TPSShootController tpsShootController;


        void Start()
        {
            // 在 Start 方法中进行其他初始化操作
        }

        // Update is called once per frame
        void Update()
        {
            MovingStatus();
            ShootStatus();
        }

        private void MovingStatus()
        {
            int PerviousTargetmovingStatus = TargetmovingStatus;

            if (avatarController != null)
            {
                if (avatarController.IsCrouching)
                    TargetmovingStatus = -1;
                else if (avatarController.Stopping)
                    TargetmovingStatus = 0;
                else if (avatarController.IsWalking)
                    TargetmovingStatus = 1;
                else if (avatarController.IsSprinting)
                    TargetmovingStatus = 2;
            }

            if (TargetmovingStatus != PerviousTargetmovingStatus)
            {
                OnTargetMovingStatusChanged(TargetmovingStatus);
            }

        }

        private void ShootStatus()
        {
            if (tpsShootController != null)
            {
                isAiming = tpsShootController.isAiming;
                Fire = tpsShootController.Fire;
            }
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

                if (TargetReceived != null)
                {
                    avatarController = TargetReceived.GetComponent<AvatarController>();
                    tpsShootController = TargetReceived?.GetComponent<TPSShootController>();
                }
                else
                {
                    avatarController = null;
                    tpsShootController = null;
                }
            }
        }

        protected virtual void OnTargetMovingStatusChanged(int newvalue)
        {
            TargetMovingStatusChanged?.Invoke(newvalue);
        }

    }
}
