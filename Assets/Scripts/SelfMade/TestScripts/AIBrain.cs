using CustomInspector;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace TestField
{
    public class AIBrain : MonoBehaviour
    {
        public List<GameObject> AttackTarget;

        private BroadCasterInfoContainer BCIC;

        void Start()
        {
            ComponentInit();
            EventSubscribe();
        }

        void Update()
        {

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