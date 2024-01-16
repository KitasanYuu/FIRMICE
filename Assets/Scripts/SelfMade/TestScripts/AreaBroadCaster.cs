using System.Collections.Generic;
using UnityEngine;

namespace TestField
{
    [RequireComponent(typeof(TargetSearchArea))]
    public class AreaBoarCaster : MonoBehaviour
    {
        private GameObject ObjectFound;
        private List<GameObject> BroadCastReceiver = new List<GameObject>();
        private TargetSearchArea targetsearcharea;

        // Start is called before the first frame update
        void Start()
        {
            targetsearcharea = GetComponent<TargetSearchArea>();

            // 订阅事件
            targetsearcharea.TargetFoundChanged += OnTargetFoundChanged;
        }

        // Update is called once per frame
        void Update()
        {
            
        }


        private void OnTargetFoundChanged(GameObject newTarget)
        {

            Debug.Log("1");
            ObjectFound = targetsearcharea.TargetFound;
            BroadCastReceiver = targetsearcharea.BroadCastReceiver;
            // 在 TargetFound 变化时，向 BroadCastReceiver 中的具有 AlertLine 脚本的对象发送信息
            SendAlertToObjectWithAlertLine();
        }

        private void SendAlertToObjectWithAlertLine()
        {
            Debug.Log("2");
            foreach (GameObject receiverObject in BroadCastReceiver)
            {
                Debug.Log("3");

                AlertLine alertLine = receiverObject.GetComponent<AlertLine>();
                if (alertLine != null)
                {
                    // 直接调用 AlertLine 脚本中的 TargetBoardCast 方法，将 ObjectFound 传递给它
                    alertLine.TargetBoardCast(ObjectFound);
                }
            }
        }

        // 在脚本销毁时取消订阅事件，以防止潜在的内存泄漏
        private void OnDestroy()
        {
            targetsearcharea.TargetFoundChanged -= OnTargetFoundChanged;
        }
    }
}
