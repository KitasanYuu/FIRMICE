using CustomInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TestField
{
    [RequireComponent(typeof(BattleAreaManager))]
    public class AreaBoarCaster : MonoBehaviour
    {
        [SerializeField, ReadOnly] private GameObject ObjectFound;
        [SerializeField, ReadOnly] private BattleAreaManager BAM;
        [SerializeField, ReadOnly] private List<GameObject> Partner = new List<GameObject>();
        [SerializeField, ReadOnly] private List<GameObject> BCReceiver = new List<GameObject>();
        private List<GameObject> BroadCastReceiver = new List<GameObject>();
        public List<GameObject> SBroadCastReceiver = new List<GameObject>();
        [SerializeField, ReadOnly] private List<GameObject> FullCoverList = new List<GameObject>();
        [SerializeField, ReadOnly] private List<GameObject> HalfCoverList = new List<GameObject>();
        [SerializeField, ReadOnly] private List<GameObject> OccupiedCoverList = new List<GameObject>();
        [SerializeField, ReadOnly] private List<GameObject> FreeCoverList = new List<GameObject>();

        // Start is called before the first frame update
        void Start()
        {
            BAM = GetComponent<BattleAreaManager>();


            // 订阅事件，当BroadCastReceiver变化时调用 HandleBroadCastReceiverChanged 方法
            BAM.BroadCastReceiverChanged += HandleBroadCastReceiverChanged;
            // 订阅事件
            BAM.TargetFoundChanged += OnTargetFoundChanged;
            BAM.PartnerChanged += OnPartnerChanged;
            BAM.FullCoverChanged += OnFullCoverChanged;
            BAM.HalfCoverChanged += OnHalfCoverChanged;
            BAM.SBroadCastReceiverChanged += OnSBroadCastReceivedChanged;
            BAM.OccupiedCoverChanged += OnOccupiedCoverChanged;
            BAM.FreeCoverChanged += OnFreeCoverChanged;
        }
        // Update is called once per frame
        void Update()
        {

        }


        private void HandleBroadCastReceiverChanged(List<GameObject> newReceiverList)
        {
            BCReceiver = newReceiverList;
            SendOtherReceiverINFO();
        }

        private void OnTargetFoundChanged(GameObject newTarget)
        {
            ObjectFound = BAM.TargetFound;
            BroadCastReceiver = BAM.BroadCastReceiver;
            // 在 TargetFound 变化时，向 BroadCastReceiver 中的具有 AlertLine 脚本的对象发送信息
            SendAlertToObjectWithAlertLine();
        }

        private void OnPartnerChanged(List<GameObject> newList)
        {
            Partner = newList;
            SendPartnerINFO();
        }

        private void OnHalfCoverChanged(List<GameObject> newList)
        {
            HalfCoverList = newList;
            foreach (GameObject receiverObject in BCReceiver)
            {
                BroadCasterInfoContainer BCIC = receiverObject.GetComponent<BroadCasterInfoContainer>();
                if (BCIC != null)
                {
                    // 直接调用 AlertLine 脚本中的 TargetBoardCast 方法，将 ObjectFound 传递给它
                    BCIC.HalfCoverChanged(HalfCoverList);
                }
            }

        }

        private void OnFullCoverChanged(List<GameObject> newList)
        {
            FullCoverList = newList;
            foreach (GameObject receiverObject in BCReceiver)
            {
                BroadCasterInfoContainer BCIC = receiverObject.GetComponent<BroadCasterInfoContainer>();
                if (BCIC != null)
                {
                    // 直接调用 AlertLine 脚本中的 TargetBoardCast 方法，将 ObjectFound 传递给它
                    BCIC.FullCoverChanged(FullCoverList);
                }
            }
        }

        private void OnOccupiedCoverChanged(List<GameObject> newList)
        {
            OccupiedCoverList = newList;
            foreach (GameObject receiverObject in BCReceiver)
            {
                BroadCasterInfoContainer BCIC = receiverObject.GetComponent<BroadCasterInfoContainer>();
                if (BCIC != null)
                {
                    BCIC.OccupiedCoverChanged(OccupiedCoverList);
                }
            }
        }

        private void OnFreeCoverChanged(List<GameObject> newList)
        {
            FreeCoverList = newList;
            foreach (GameObject receiverObject in BCReceiver)
            {
                BroadCasterInfoContainer BCIC = receiverObject.GetComponent<BroadCasterInfoContainer>();
                if (BCIC != null)
                {
                    BCIC.FreeCoverChanged(FreeCoverList);
                }
            }
        }

        private void OnSBroadCastReceivedChanged(List<GameObject> newList)
        {
            SBroadCastReceiver = newList;

            // 找到两个列表的交集，即相同的物体
            List<GameObject> commonObjects = BroadCastReceiver.Intersect(SBroadCastReceiver).ToList();

            // 遍历相同的物体，并获取每个物体上的一个组件
            foreach (GameObject obj in commonObjects)
            {
                BroadCasterInfoContainer BCIC = obj.GetComponent<BroadCasterInfoContainer>();

                if (BCIC != null)
                {
                    BCIC.NeedBackToOrigin = false;
                    BCIC.TargetBoardCast(ObjectFound);
                }
            }

            // 找到两个列表的并集，即所有不同的物体
            List<GameObject> onlyInCurrent = BroadCastReceiver.Except(SBroadCastReceiver).ToList();
            //Debug.Log(onlyInCurrent.Count);

            // 遍历所有不同的物体，并获取每个物体上的一个组件
            foreach (GameObject obj in onlyInCurrent)
            {
                BroadCasterInfoContainer BCIC = obj.GetComponent<BroadCasterInfoContainer>();

                if (BCIC != null)
                {
                    BCIC.NeedBackToOrigin = true;
                    BCIC.TargetBoardCast(null);
                }
            }
        }

        private void SendPartnerINFO()
        {
            foreach (GameObject receiverObject in BCReceiver)
            {
                BroadCasterInfoContainer BCIC = receiverObject.GetComponent<BroadCasterInfoContainer>();
                if (BCIC != null)
                {
                    BCIC.PartnerChanged(Partner);
                }
            }
        }

        private void SendAlertToObjectWithAlertLine()
        {
            foreach (GameObject receiverObject in SBroadCastReceiver)
            {
                BroadCasterInfoContainer BCIC = receiverObject.GetComponent<BroadCasterInfoContainer>();
                if (BCIC != null)
                {
                    // 直接调用 AlertLine 脚本中的 TargetBoardCast 方法，将 ObjectFound 传递给它
                    BCIC.TargetBoardCast(ObjectFound);
                }
            }
        }

        private void SendOtherReceiverINFO()
        {
            foreach (GameObject receiverObject in BCReceiver)
            {
                List<GameObject> filteredList = BCReceiver.Where(x => x != receiverObject).ToList();

                BroadCasterInfoContainer BCIC = receiverObject.GetComponent<BroadCasterInfoContainer>();
                if (BCIC != null)
                {
                    BCIC.OtherReceiverINFOChanged(filteredList);
                }
            }
        }


        // 在脚本销毁时取消订阅事件，以防止潜在的内存泄漏
        private void OnDestroy()
        {
            BAM.TargetFoundChanged -= OnTargetFoundChanged;
            BAM.PartnerChanged -= OnPartnerChanged;
            BAM.BroadCastReceiverChanged -= HandleBroadCastReceiverChanged;
            BAM.HalfCoverChanged -= OnHalfCoverChanged;
            BAM.FullCoverChanged -= OnFullCoverChanged;
            BAM.SBroadCastReceiverChanged -= OnSBroadCastReceivedChanged;
            BAM.OccupiedCoverChanged -= OnOccupiedCoverChanged;
            BAM.FreeCoverChanged -= OnFreeCoverChanged;
        }
    }
}