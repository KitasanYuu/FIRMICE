using System;
using System.Collections.Generic;
using UnityEngine;

namespace TestField
{
    public class AIBrain : MonoBehaviour
    {
        public GameObject CurrentTarget;
        private GameObject PreviousTarget;
        public List<GameObject> AttackTarget;
        public bool focusonplayer;

        private GameObject AlertTarget;

        private AIFunction aif = new AIFunction();
        private BroadCasterInfoContainer BCIC;
        private AIMove AiM;
        private AlertCore AC;

        public Action<GameObject> AttackTargetChanged;

        void Start()
        {
            ComponentInit();
            EventSubscribe();
        }

        void Update()
        {
              TargetSelect();
            Debug.Log("AIMFaceToTarget"+AiM.facetoTarget);
        }

        private void TargetSelect()
        {
            if(focusonplayer)
            {
                CurrentTarget = AlertTarget;
            }
            else
            {
                CurrentTarget = aif.CurrentSelectedAttackTarget(gameObject, AttackTarget);
            }

            if (CurrentTarget != PreviousTarget)
            {
                OnAttackTargetChanged(CurrentTarget);
                PreviousTarget = CurrentTarget;

                Debug.Log(CurrentTarget);
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

        #endregion

        #region 广播

        protected virtual void OnAttackTargetChanged(GameObject TargetObject)
        {
            AttackTargetChanged?.Invoke(TargetObject);
        }

        #endregion

        #region 组件初始化&订阅管理
        private void ComponentInit()
        {
            BCIC = GetComponent<BroadCasterInfoContainer>();
            AC = GetComponent<AlertCore>();
            AiM = aif.GainSelfMoveScriptType(gameObject) as AIMove;
        }

        private void EventSubscribe()
        {
            BCIC.AlertTargetReceivedChanged += OnAlertTargetChanged;
            BCIC.AttackTargetListChanged += OnAttackTargetListChanged;
        }

        private void OnDestroy()
        {
            if (BCIC != null)
            {
                BCIC.AlertTargetReceivedChanged -= OnAlertTargetChanged;
                BCIC.AttackTargetListChanged -= OnAttackTargetListChanged;
            }
        }
        #endregion
    }
}