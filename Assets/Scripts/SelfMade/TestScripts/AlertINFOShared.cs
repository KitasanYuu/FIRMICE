using CustomInspector;
using System.Collections.Generic;
using TestField;
using UnityEditor;
using UnityEngine;

namespace TestField
{
    [RequireComponent(typeof(AlertLogic))]
    public class AlertINFOShared : MonoBehaviour
    {
        public bool ReceiveSharedINFO;
        public float SharedDistance;
        [ReadOnly, SerializeField] private List<GameObject> OtherReceiver = new List<GameObject>();


        private BroadCasterInfoContainer BCIC;
        private AlertLogic AL;

        private void Awake()
        {
            ComponentInit();
        }

        private void Start()
        {

            EventSubscribe();
        }

        // Update is called once per frame
        private void Update()
        {
            ShareTargetComfirm();
        }

        private void ShareTargetComfirm()
        {
            // 在Update中发射射线，设置足够大的射线长度
            float maxDistance = Mathf.Infinity; // 设置射线的最大长度为无穷大

            // 遍历列表中的每个目标物体
            foreach (GameObject target in OtherReceiver)
            {
                // 构造射线，从当前物体位置到目标物体位置
                Ray ray = new Ray(transform.position, target.transform.position - transform.position);
                RaycastHit hit;

                // 检测射线是否击中任何物体
                if (Physics.Raycast(ray, out hit, maxDistance))
                {
                    // 获取被击中的物体
                    GameObject hitObject = hit.collider.gameObject;

                    // 检查被击中的物体是否是目标物体或者目标物体的子物体
                    if (hitObject == target || hitObject.transform.IsChildOf(target.transform))
                    {
                        // 计算两者之间的距离，只考虑X和Z轴
                        float distance = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                                          new Vector3(target.transform.position.x, 0, target.transform.position.z));

                        // 如果距离小于等于 SharedDistance，则执行某些操作
                        if (distance <= SharedDistance)
                        {
                            AlertINFOShared AIS = target.GetComponent<AlertINFOShared>();
                            AlertLogic alertlogic = target.GetComponent<AlertLogic>();
                            if (alertlogic != null && AL.TargetExposed && AIS.ReceiveSharedINFO)
                            {
                                alertlogic.TargetExposed = true;
                            }

                        }
                    }
                }
            }
        }

        private void ComponentInit()
        {
            BCIC = GetComponent<BroadCasterInfoContainer>();
            AL = GetComponent<AlertLogic>();
        }


        private void OnOtherReceiverChanged(List<GameObject> newList)
        {
            OtherReceiver = newList;
        }

        // 订阅事件
        private void EventSubscribe()
        {
            BCIC.OtherReceiverChanged += OnOtherReceiverChanged;
        }

        private void OnDestroy()
        {
            if (BCIC != null)
            {
                BCIC.OtherReceiverChanged -= OnOtherReceiverChanged;
            }
        }

        // 在Scene视图中绘制射线
        private void OnDrawGizmos()
        {
            // 获取物体的世界坐标
            Vector3 worldPosition = transform.position;
            worldPosition.y = 0.0f;

            // 绘制在xz平面上的圆
            DrawCircleOnXZPlane(worldPosition, SharedDistance, 360);

            // 遍历列表中的每个目标物体
            foreach (GameObject target in OtherReceiver)
            {
                AlertINFOShared AIS = target.GetComponent<AlertINFOShared>();
                // 计算两者之间的距离，只考虑X和Z轴
                float distance = Vector3.Distance(new Vector3(transform.position.x, 0.0f, transform.position.z),
                                                  new Vector3(target.transform.position.x, 0.0f, target.transform.position.z));

                if (AIS.ReceiveSharedINFO)
                {
                    if (distance <= SharedDistance)
                    {
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawLine(transform.position, target.transform.position);
                    }
                    else
                    {
                        Gizmos.color = Color.grey;
                        Gizmos.DrawLine(transform.position, target.transform.position);
                    }
                }
                else
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawLine(transform.position, target.transform.position);
                }
            }
        }
        private void DrawCircleOnXZPlane(Vector3 center, float radius, int segments)
        {
            Handles.color = Color.white;

            Vector3 axis = Vector3.up;  // 指定轴向为y轴，即绘制在xz平面上
            Handles.DrawWireDisc(center, axis, radius);
        }

    }
}
