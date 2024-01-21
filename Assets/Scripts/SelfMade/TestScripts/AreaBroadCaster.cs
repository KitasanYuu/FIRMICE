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
        [SerializeField,ReadOnly]private BattleAreaManager BAM;
        [SerializeField, ReadOnly] private List<GameObject> BCReceiver = new List<GameObject>();
        private List<GameObject> BroadCastReceiver = new List<GameObject>();
        [SerializeField, ReadOnly] private List<GameObject> FullCoverList = new List<GameObject>();
        [SerializeField, ReadOnly] private List<GameObject> HalfCoverList = new List<GameObject>();

        // Start is called before the first frame update
        void Start()
        {
            BAM = GetComponent<BattleAreaManager>();


            // 订阅事件，当BroadCastReceiver变化时调用 HandleBroadCastReceiverChanged 方法
            BAM.BroadCastReceiverChanged += HandleBroadCastReceiverChanged;
            // 订阅事件
            BAM.TargetFoundChanged += OnTargetFoundChanged;
            BAM.FullCoverChanged += OnFullCoverChanged;
            BAM.HalfCoverChanged += OnHalfCoverChanged;
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

        private void OnHalfCoverChanged(List<GameObject> newList)
        {
            HalfCoverList = newList;
            foreach (GameObject receiverObject in BCReceiver)
            {
                BroadCasterInfoContainer broadcasterinfocontainer = receiverObject.GetComponent<BroadCasterInfoContainer>();
                if (broadcasterinfocontainer != null)
                {
                    // 直接调用 AlertLine 脚本中的 TargetBoardCast 方法，将 ObjectFound 传递给它
                    broadcasterinfocontainer.HalfCoverChanged(HalfCoverList);
                }
            }

        }
        private void OnFullCoverChanged(List<GameObject> newList)
        {
            FullCoverList = newList;
            foreach (GameObject receiverObject in BCReceiver)
            {
                BroadCasterInfoContainer broadcasterinfocontainer = receiverObject.GetComponent<BroadCasterInfoContainer>();
                if (broadcasterinfocontainer != null)
                {
                    // 直接调用 AlertLine 脚本中的 TargetBoardCast 方法，将 ObjectFound 传递给它
                    broadcasterinfocontainer.FullCoverChanged(FullCoverList);
                }
            }
        }

        private void SendAlertToObjectWithAlertLine()
        {
            foreach (GameObject receiverObject in BroadCastReceiver)
            {
                BroadCasterInfoContainer broadcasterinfocontainer = receiverObject.GetComponent<BroadCasterInfoContainer>();
                if (broadcasterinfocontainer != null)
                {
                    // 直接调用 AlertLine 脚本中的 TargetBoardCast 方法，将 ObjectFound 传递给它
                    broadcasterinfocontainer.TargetBoardCast(ObjectFound);
                }
            }
        }

        private void SendOtherReceiverINFO()
        {
            foreach (GameObject receiverObject in BCReceiver)
            {
                List<GameObject> filteredList = BCReceiver.Where(x => x != receiverObject).ToList();

                BroadCasterInfoContainer broadcasterInfoContainer = receiverObject.GetComponent<BroadCasterInfoContainer>();
                if (broadcasterInfoContainer != null)
                {
                    broadcasterInfoContainer.OtherReceiverINFOChanged(filteredList);
                }
            }
        }


        // 在脚本销毁时取消订阅事件，以防止潜在的内存泄漏
        private void OnDestroy()
        {
            BAM.TargetFoundChanged -= OnTargetFoundChanged;
            BAM.BroadCastReceiverChanged -= HandleBroadCastReceiverChanged;
            BAM.HalfCoverChanged -= OnHalfCoverChanged;
            BAM.FullCoverChanged -= OnFullCoverChanged;
        }
    }
}