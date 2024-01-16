using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using CustomInspector;

namespace TestField
{
    [RequireComponent(typeof(BroadCasterInfoContainer))]
    public class AlertLine : MonoBehaviour
    {
        public bool foundTarget;
        public bool Hitbutoutrange;
        [ReadOnly] public GameObject targetContainer;
        public LayerMask IgnoreLayer; // 障碍物层

        [Header("Customize Detection Area")]
        public Vector3 RayStarpointOffset;
        public bool syncRadiusWithRayLength = false; // 是否同步扇形的边长为射线长度
        public float detectionAngle = 45f; // 扇形的角度
        public float detectionRadius = 10.0f; // 扇形的半径
        public float detectionRotation = 0f; // 扇形的旋转角度
        public float rayLength = 10.0f; // 射线的长度

        public List<GameObject> targetlist = new List<GameObject>();

        private Vector3 detectionDirection = Vector3.forward; // 扇形的方向
        private TargetContainer targetcontainer;

        private BroadCasterInfoContainer broadCasterinfocontainer;

        private bool notarget;
        private bool targetBroadCastFound;

        // 在编辑器中即时更新
        private void OnValidate()
        {
            // 确保数值大于等于0
            detectionAngle = Mathf.Max(detectionAngle, 0f);
            detectionRadius = Mathf.Max(detectionRadius, 0f);
            detectionRotation = Mathf.Max(detectionRotation, 0f);
            rayLength = Mathf.Max(rayLength, 0f);

            if (syncRadiusWithRayLength)
            {
                detectionRadius = rayLength;
            }
            else
            {
                rayLength = Mathf.Min(rayLength, detectionRadius);
            }
        }

        private void Awake()
        {
            // 获取 BroadCasterInfoContainer 组件
            broadCasterinfocontainer = GetComponent<BroadCasterInfoContainer>();
        }

        private void Start()
        {
            // 订阅事件
            broadCasterinfocontainer.TargetReceivedChanged += OnTargetReceivedChanged;

            Startinit();
        }

        private void Update()
        {
            TargetRetake();
            DetectTargets();
        }

        private void Startinit()
        {
            if (targetContainer != null)
            {
                targetcontainer = targetContainer.GetComponent<TargetContainer>();
                targetlist = targetcontainer.targets;
                notarget = false;
            }
            else
            {
                targetcontainer = null;
                targetlist = null;
                notarget = true;
            }
        }

        private void TargetRetake()
        {
            if (targetBroadCastFound)
            {
                Startinit();
                targetBroadCastFound = false;
            }
        }

        private void DetectTargets()
        {
            if (!notarget)
            {
                Hitbutoutrange = false;
                foundTarget = false;

                // 获取当前位置
                Vector3 origin = transform.position;
                // 在起始位置上添加偏移量
                origin += RayStarpointOffset;

                // 遍历目标列表
                foreach (GameObject targetObject in targetlist)
                {
                    // 获取目标的位置
                    Vector3 targetPosition = targetObject.transform.position;

                    // 计算朝向目标的标准化方向向量
                    Vector3 direction = (targetPosition - origin).normalized;
                    Vector3 RayDirection = (targetPosition - origin).normalized;

                    // 进行旋转
                    direction = Quaternion.Euler(0f, detectionRotation, 0f) * direction;

                    // 计算射线的长度（使用自定义的射线长度）
                    float currentRayLength = Mathf.Min(rayLength, detectionRadius);

                    // 检测是否在扇形区域内
                    float angle = Vector3.Angle(detectionDirection, direction);

                    //Debug.Log(angle);


                    // 发射射线到目标
                    Ray ray = new Ray(origin, RayDirection);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit, currentRayLength, ~IgnoreLayer))
                    {
                        // 检查命中目标是否为 targetContainer 或者在列表中
                        if (hit.collider.gameObject == targetContainer || targetlist.Contains(hit.collider.gameObject))
                        {
                            if (angle <= detectionAngle * 0.5f)
                            {
                                // 这里执行侦测到目标的逻辑
                                foundTarget = true;
                                break; // 如果至少有一条射线命中目标，就跳出循环
                            }
                            else
                            {
                                Hitbutoutrange = true;
                                break;
                            }

                        }

                    }
                }

                // 如果至少有一条射线命中了目标，执行相应的逻辑
                if (foundTarget)
                {
                    Debug.Log("At least one target detected!");
                    // 在这里执行相应的逻辑
                }

                if (Hitbutoutrange)
                {
                    Debug.Log("Detected Target But Out the Range");
                }
            }
        }


        private void OnTargetReceivedChanged(GameObject newTarget)
        {
            // 在 TargetReceived 改变时执行的逻辑
            Debug.Log("TargetReceived changed to: " + newTarget);
            targetContainer = newTarget;
            targetBroadCastFound = true;
        }

        // 在脚本销毁时取消订阅事件，以防止潜在的内存泄漏
        private void OnDestroy()
        {
            if (broadCasterinfocontainer != null)
            {
                broadCasterinfocontainer.TargetReceivedChanged -= OnTargetReceivedChanged;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // 获取当前位置
            Vector3 origin = transform.position;
            origin += RayStarpointOffset;

            // 计算扇形的两个边缘点
            Quaternion leftRotation = Quaternion.AngleAxis(-detectionAngle * 0.5f, Vector3.up);
            Quaternion rightRotation = Quaternion.AngleAxis(detectionAngle * 0.5f, Vector3.up);
            Vector3 leftDirection = leftRotation * detectionDirection;
            Vector3 rightDirection = rightRotation * detectionDirection;

            // 在 Scene 视图上绘制蓝色圆，表示 RayStarpointOffset
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(origin, 0.05f);

            // 进行旋转
            leftDirection = Quaternion.Euler(0f, -detectionRotation, 0f) * leftDirection;
            rightDirection = Quaternion.Euler(0f, -detectionRotation, 0f) * rightDirection;

            if (targetContainer != null && targetlist != null)
            {
                // 遍历目标列表
                foreach (GameObject targetObject in targetlist)
                {
                    // 获取目标的位置
                    Vector3 targetPosition = targetObject.transform.position;

                    // 计算朝向目标的标准化方向向量
                    Vector3 direction = (targetPosition - origin).normalized;
                    Vector3 RayDirection = (targetPosition - origin).normalized;

                    // 进行旋转
                    direction = Quaternion.Euler(0f, detectionRotation, 0f) * direction;

                    // 计算射线的长度（使用自定义的射线长度）
                    float currentRayLength = Mathf.Min(rayLength, detectionRadius);

                    // 检测是否在扇形区域内
                    float angle = Vector3.Angle(detectionDirection, direction);

                    // 发射射线到目标
                    Ray ray = new Ray(origin, RayDirection);
                    RaycastHit hit;
                    // 计算目标点与射线的距离
                    float distanceToTarget = Vector3.Distance(targetPosition, origin);
                    // 判定是否在扇形区域内且距离小于等于 detectionRadius
                    if (angle <= detectionAngle * 0.5f && distanceToTarget <= detectionRadius)
                    {
                        if (Physics.Raycast(ray, out hit, float.PositiveInfinity, ~IgnoreLayer))
                        {
                            // 检查命中目标是否为 targetContainer 或者在列表中
                            if (hit.collider.gameObject == targetContainer || targetlist.Contains(hit.collider.gameObject))
                            {
                                // 在扇形区域内检测到目标，绘制黄色射线
                                Gizmos.color = Color.yellow;
                                Gizmos.DrawLine(origin, origin + RayDirection * currentRayLength);

                                // 更新整体 foundTarget 的值
                                foundTarget = true;
                            }
                            else
                            {
                                // 在扇形区域内未检测到目标，绘制蓝色射线
                                Gizmos.color = Color.blue;
                                Gizmos.DrawLine(origin, origin + RayDirection * currentRayLength);
                            }
                        }
                    }
                    else
                    {
                        // 不再使用 currentRayLength 作为射线的长度
                        Ray Gray = new Ray(origin, RayDirection.normalized * detectionRadius);
                        RaycastHit Ghit;

                        if (Physics.Raycast(Gray, out Ghit, float.PositiveInfinity, ~IgnoreLayer))
                        {
                                Gizmos.color = Color.green;
                                Gizmos.DrawLine(origin, origin + RayDirection.normalized * detectionRadius);

                                // 在扇形范围外检测到目标，更新整体 foundTarget 的值
                                foundTarget = true;
                        }

                    }
                }
            }

            // 在Scene视图上绘制扇形
            DrawSector(origin, leftDirection, rightDirection, detectionRadius, detectionAngle);
        }



        // 绘制扇形的方法
        private void DrawSector(Vector3 origin, Vector3 leftDirection, Vector3 rightDirection, float radius, float angle)
            {
            origin -= RayStarpointOffset;
            // 将原点的 Y 轴分量设置为 0
            origin.y = 0f;

                Handles.color = Color.red;

                // 使用 Handles.DrawWireArc 来绘制扇形的圆弧
                Handles.DrawWireArc(origin, Vector3.up, leftDirection, angle, radius);

                // 绘制扇形两侧的连线
                Vector3 leftEdge = origin + leftDirection * radius;
                Vector3 rightEdge = origin + rightDirection * radius;

                // 将两侧的 Y 轴分量设置为 0
                leftEdge.y = 0f;
                rightEdge.y = 0f;

                Handles.DrawLine(origin, leftEdge);
                Handles.DrawLine(origin, rightEdge);
            }


#endif
        }
    }