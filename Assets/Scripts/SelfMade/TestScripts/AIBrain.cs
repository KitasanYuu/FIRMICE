using System.Collections.Generic;
using UnityEngine;

namespace TestField
{
    public class AIBrain : MonoBehaviour
    {
        public GameObject CurrentTarget;
        public List<GameObject> AttackTarget;
        public Dictionary<string, float> directory = new Dictionary<string, float>();

        private AIFunction aif = new AIFunction();
        private BroadCasterInfoContainer BCIC;

        void Start()
        {
            ComponentInit();
            EventSubscribe();
        }

        void Update()
        {
            if(AttackTarget!= null)
               CurrentTarget =  aif.CurrentSelectedAttackTarget(gameObject,AttackTarget);
        }

        private void OnAttackTargetListChanged(List<GameObject> newList)
        {
            AttackTarget = newList;
        }

        #region 组件初始化&订阅管理
        private void ComponentInit()
        {
            BCIC = GetComponent<BroadCasterInfoContainer>();
        }

        private void EventSubscribe()
        {
            BCIC.AttackTargetListChanged += OnAttackTargetListChanged;
        }

        private void OnDestroy()
        {
            BCIC.AttackTargetListChanged -= OnAttackTargetListChanged;
        }
        #endregion
    }
}