using System.Collections.Generic;
using UnityEngine;

namespace TestField
{
    public class AIBrain : MonoBehaviour
    {
        public GameObject CurrentTarget;
        public List<GameObject> AttackTarget;
        public bool focusonplayer;
        public Dictionary<string, float> directory = new Dictionary<string, float>();

        private GameObject AlertTarget;

        private AIFunction aif = new AIFunction();
        private BroadCasterInfoContainer BCIC;

        void Start()
        {
            ComponentInit();
            EventSubscribe();
        }

        void Update()
        {
            if(Input.GetKeyDown(KeyCode.R))
               TargetSelect();
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
            Debug.Log(CurrentTarget);
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

        #region 组件初始化&订阅管理
        private void ComponentInit()
        {
            BCIC = GetComponent<BroadCasterInfoContainer>();
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