using BattleShoot;
using CustomInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TestField
{
    public class E_AIBrain : MonoBehaviour
    {
        public GameObject CurrentTarget;
        [ReadOnly]
        public bool TargetExposed;
        [ReadOnly]
        public bool CanFire;
        private bool PreviousCanFireStatus;
        private GameObject PreviousTarget;
        public List<GameObject> AttackTarget;
        public bool focusonplayer;

        private GameObject AlertTarget;

        private AIFunction aif = new AIFunction();
        private BroadCasterInfoContainer BCIC;
        private AIMove AiM;
        private AlertCore AC;
        private ShootController shootcontroller;

        public Action<GameObject> AttackTargetChanged;
        public Action<bool> AbleToFireChanged;
        void Start()
        {
            ParameterInit();
            ComponentInit();
            EventSubscribe();
        }

        void Update()
        {
            Debug.Log(PreviousCanFireStatus);

            if (AiM.inBattle)
            {
                TargetSelect();
                ShootCondition();
            }

        }

        private void TargetSelect()
        {
            if (focusonplayer)
            {
                CurrentTarget = AlertTarget;
            }
            else
            {
                CurrentTarget = aif.CurrentSelectedAttackTarget(gameObject, AttackTarget);
            }

            if (CurrentTarget != PreviousTarget)
            {
                shootcontroller.SetCurrentAttackTarget(CurrentTarget);
                OnAttackTargetChanged(CurrentTarget);
                PreviousTarget = CurrentTarget;

                //Debug.Log(CurrentTarget);
            }
        }

        private void ShootCondition()
        {
            if(AiM != null)
            {
                if (AiM.FacetoTarget && TargetExposed)
                {
                    CanFire = true;
                }
                else
                {
                    CanFire = false;
                }

                if(PreviousCanFireStatus != CanFire)
                {
                    OnFireConditionChanged(CanFire);
                    shootcontroller.SetAssetFireComfirm(CanFire);
                    PreviousCanFireStatus = CanFire;
                }
            }
        }

        #region 接收广播值

        private void OnAttackTargetListChanged(List<GameObject> newList)
        {
            AttackTarget = newList;
        }

        private void OnAlertTargetChanged(GameObject newGameObject)
        {
            AlertTarget = newGameObject;
        }

        private void OnTargetExposeStatusChanged(bool ExposeStatus)
        {
            TargetExposed = ExposeStatus;
            //shootcontroller.AimStatusSet(TargetExposed);
        }

        #endregion

        #region 广播

        protected virtual void OnAttackTargetChanged(GameObject TargetObject)
        {
            AttackTargetChanged?.Invoke(TargetObject);
        }

        protected virtual void OnFireConditionChanged(bool CanFire)
        {
            AbleToFireChanged?.Invoke(CanFire);
        }

        #endregion

        #region 组件参数初始化&订阅管理

        private void ParameterInit()
        {
            PreviousCanFireStatus = false;
        }

        private void ComponentInit()
        {
            shootcontroller = GetComponent<ShootController>();
            BCIC = GetComponent<BroadCasterInfoContainer>();
            AC = GetComponent<AlertCore>();
            AiM = aif.GainSelfMoveScriptType(gameObject) as AIMove;
        }

        private void EventSubscribe()
        {
            AC.ExposeStatusChanged += OnTargetExposeStatusChanged;
            BCIC.AlertTargetReceivedChanged += OnAlertTargetChanged;
            BCIC.AttackTargetListChanged += OnAttackTargetListChanged;
        }

        private void OnDestroy()
        {
            if (BCIC != null)
            {
                AC.ExposeStatusChanged -= OnTargetExposeStatusChanged;
                BCIC.AlertTargetReceivedChanged -= OnAlertTargetChanged;
                BCIC.AttackTargetListChanged -= OnAttackTargetListChanged;
            }
        }
        #endregion
    }
}