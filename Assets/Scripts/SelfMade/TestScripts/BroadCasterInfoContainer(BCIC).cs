using UnityEngine;
using System;
using CustomInspector;
using AvatarMain;
using playershooting;
using NUnit.Framework;
using System.Collections.Generic;

namespace TestField
{
    public class BroadCasterInfoContainer : MonoBehaviour
    {
        // 定义事件委托
        public Action<GameObject> TargetReceivedChanged;
        public Action<List<GameObject>> OtherReceiverChanged;
        public Action<List<GameObject>> HalfcoverChanged;
        public Action<List<GameObject>> FullcoverChanged;
        public Action<List<GameObject>> OccupiedcoverChanged;
        public Action<List<GameObject>> FreecoverChanged;
        public Action<int> TargetMovingStatusChanged;
        public Action<bool> Fire;
        public Action<bool> Aiming;

        [ReadOnly] public bool NeedBackToOrigin = false;
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
        public bool _isAiming;
        [ReadOnly]
        public bool _Fire;

        [ReadOnly] public AvatarController avatarController;
        [ReadOnly] public TPSShootController tpsShootController;
        public List<GameObject> PartnerList = new List<GameObject>();
        public List<GameObject> OtherReceivers = new List<GameObject>();
        public List<GameObject> CoverList = new List<GameObject>();
        public List<GameObject> FullCoverList = new List<GameObject>();
        public List<GameObject> HalfCoverList = new List<GameObject>();
        public List<GameObject> OccupiedCoverList = new List<GameObject>();
        public List<GameObject> FreeCoverList = new List<GameObject>();

        private bool shouldCombineLists = false;

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
                else if (avatarController.Stopping || avatarController.IsJumping)
                    TargetmovingStatus = 0;
                else if (avatarController.IsWalking)
                    TargetmovingStatus = 1;
                else if (avatarController.IsSprinting)
                    TargetmovingStatus = 2;
                else if (_isAiming && TargetmovingStatus == 2)
                    TargetmovingStatus = 1;

            }

            if (TargetmovingStatus != PerviousTargetmovingStatus)
            {
                OnTargetMovingStatusChanged(TargetmovingStatus);
            }

        }

        private void ShootStatus()
        {
            bool PFire = _Fire;
            bool PAim = _isAiming;

            if (tpsShootController != null)
            {
                _isAiming = tpsShootController.isAiming;
                _Fire = tpsShootController.Fire;
            }
            else
            {
                _isAiming = false;
                _Fire = false;
            }

            if (PFire != _Fire)
                OnFire(_Fire);
            if (PAim != _isAiming)
                OnAiming(_isAiming);
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


        private void CombineLists()
        {
            CoverList.Clear();
            HashSet<GameObject> uniqueSet = new HashSet<GameObject>(HalfCoverList);
            uniqueSet.UnionWith(FullCoverList);
            CoverList = new List<GameObject>(uniqueSet);

            // 合并完成后重置 shouldCombineLists 为 false
            shouldCombineLists = false;
        }

        public void PartnerChanged(List<GameObject> newPartnerList)
        {
            PartnerList = newPartnerList;
        }

        public void HalfCoverChanged(List<GameObject> newHalfCoverList)
        {
            HalfCoverList = newHalfCoverList;
            CombineLists();
            OnHalfCoverChanged(newHalfCoverList);
        }

        public void FullCoverChanged(List<GameObject> newFullCoverList)
        {
            FullCoverList = newFullCoverList;
            CombineLists();
            OnFullCoverChanged(newFullCoverList);
        }

        public void OccupiedCoverChanged(List<GameObject> newOccupiedCoverList)
        {
            OccupiedCoverList = newOccupiedCoverList;
            OnOccupiedCoverChanged(newOccupiedCoverList);
        }

        public void FreeCoverChanged(List<GameObject> newFreeCoverList)
        {
            FreeCoverList = newFreeCoverList;
            OnFreeCoverChanged(newFreeCoverList);
        }

        public void OtherReceiverINFOChanged(List<GameObject> newReceiverList)
        {
            OtherReceivers = newReceiverList;
            OnOtherReceiverChanged(newReceiverList);
        }

        protected virtual void OnTargetMovingStatusChanged(int newvalue)
        {
            TargetMovingStatusChanged?.Invoke(newvalue);
        }
        protected virtual void OnFire(bool newvalue)
        {
            Fire?.Invoke(newvalue);
        }

        protected virtual void OnAiming(bool newvalue)
        {
            Aiming?.Invoke(newvalue);
        }

        protected virtual void OnOtherReceiverChanged(List<GameObject> newList)
        {
            OtherReceiverChanged?.Invoke(newList);
        }
        protected virtual void OnFullCoverChanged(List<GameObject> newList)
        {
            FullcoverChanged?.Invoke(newList);
        }
        protected virtual void OnOccupiedCoverChanged(List<GameObject> newList)
        {
            OccupiedcoverChanged?.Invoke(newList);
        }
        protected virtual void OnFreeCoverChanged(List<GameObject> newList)
        {
            FreecoverChanged?.Invoke(newList);
        }
        protected virtual void OnHalfCoverChanged(List<GameObject> newList)
        {
            HalfcoverChanged?.Invoke(newList);
        }
    }
}
