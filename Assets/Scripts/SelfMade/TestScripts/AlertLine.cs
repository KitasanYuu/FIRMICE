using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using CustomInspector;

namespace TestField
{
    [RequireComponent(typeof(BroadCasterInfoContainer))]
    public class AlertLine : MonoBehaviour
    {
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        [HorizontalLine("ParameterView", 2, FixedColor.Gray)]
        [ReadOnly]public bool foundTarget;
        [ReadOnly]public bool Hitbutoutrange;
        [ReadOnly]public int TargetMovingStatus;
        [ShowIf(nameof(TestRay),style =DisabledStyle.GreyedOut)] public GameObject targetContainer;
        public List<GameObject> targetlist = new List<GameObject>();
        [Space2(20)]
        public bool TestRay = false;

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        [HorizontalLine("SyncRangeSettings",2,FixedColor.Gray)]
        public bool UsingSyncAlertRange;

        [SerializeField, ShowIf(nameof(UsingSyncAlertRange))]
        private bool SyncRayLength;
        [SerializeField, ShowIf(nameof(SyncRayLength))]
        private bool SyncSectorRange;

        //不同情况下的检测范围倍率
        [Range(0,3),SerializeField, ShowIf(nameof(UsingSyncAlertRange))] private float CrouchRate = 1.0f;
        [Range(0,3),SerializeField, ShowIf(nameof(UsingSyncAlertRange))] private float NormalRate = 1.0f;
        [Range(0,3),SerializeField, ShowIf(nameof(UsingSyncAlertRange))] private float SprintRate = 1.0f;

        [SerializeField] private float RangeChangingRate =10.0f;

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        [HorizontalLine("DetectRaySettings",2,FixedColor.Gray)]
        public float RayLength = 10.0f; // 射线的长度
        public Vector3 RayStarpointOffset;
        [Space2(10)]
        public LayerMask IgnoreLayer; // 障碍物层

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        [HorizontalLine("DetectRangeSettings",2,FixedColor.Gray)]
        public bool syncRadiusWithRayLength = false; // 是否同步扇形的边长为射线长度
        [ShowIf(nameof(MyMethod))]
        public float DetectionRadius = 10.0f; // 扇形的半径
        public bool MyMethod() => syncRadiusWithRayLength == false;

        public float DetectionAngle = 45f; // 扇形的角度
        public float detectionRotation = 0f; // 扇形的旋转角度

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        //动态范围的参数
        private float _TRayLength;
        private float _TDetectionRadius;
        private float _TDetectionAngle;
        private float rayLength;
        private float detectionRadius;
        private float detectionAngle;

        private Vector3 detectionDirection = Vector3.forward; // 扇形的方向
        private TargetContainer targetcontainer;

        private BroadCasterInfoContainer broadCasterinfocontainer;

        private bool notarget;
        private bool targetBroadCastFound;

        //调试模式
        private bool EditorMode = true;

        // 在编辑器中即时更新
        private void OnValidate()
        {
            // 确保数值大于等于0
            DetectionAngle = Mathf.Max(DetectionAngle, 0f);
            DetectionRadius = Mathf.Max(DetectionRadius, 0f);
            detectionRotation = Mathf.Max(detectionRotation, 0f);
            RayLength = Mathf.Max(RayLength, 0f);

            if (syncRadiusWithRayLength)
            {
                DetectionRadius = RayLength;
            }
            else
            {
                RayLength = Mathf.Min(RayLength, DetectionRadius);
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
            broadCasterinfocontainer.TargetMovingStatusChanged += OnTargetMovingStatusChanged;
            ParameterInit();
            Startinit();

            EditorMode = false;
        }

        private void Update()
        {
            TargetRetake();
            SyncAlertRange();
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

        private void SyncAlertRange()
        {
            _TRayLength = RayLength;
            _TDetectionAngle = DetectionAngle;
            _TDetectionRadius = DetectionRadius;

            if (UsingSyncAlertRange)
            {
                if (SyncRayLength)
                {
                    if (TargetMovingStatus == -1)
                    {
                        _TRayLength = RayLength * CrouchRate;
                        if (SyncSectorRange)
                        {
                            _TDetectionRadius = DetectionRadius * CrouchRate;
                            _TDetectionAngle = DetectionAngle * CrouchRate;
                        }
                    }
                    else if (TargetMovingStatus == 0 || TargetMovingStatus == 1)
                    {
                        _TRayLength = RayLength * NormalRate;
                        if (SyncSectorRange)
                        {
                            _TDetectionRadius = DetectionRadius * NormalRate;
                            _TDetectionAngle = DetectionAngle * NormalRate;
                        }
                    }
                    else if (TargetMovingStatus == 2)
                    {
                        _TRayLength = RayLength * SprintRate;
                        if (SyncSectorRange)
                        {
                            _TDetectionRadius = DetectionRadius * SprintRate;
                            _TDetectionAngle = DetectionAngle * SprintRate;
                        }
                    }
                }

            }
            else
            {
                _TRayLength = RayLength;
                _TDetectionAngle = DetectionAngle;
                _TDetectionRadius = DetectionRadius;
            }

            if(targetContainer == null)
            {
                _TRayLength = RayLength;
                _TDetectionAngle = DetectionAngle;
                _TDetectionRadius = DetectionRadius;
            }


            rayLength = Mathf.Lerp(rayLength, _TRayLength, Time.deltaTime * RangeChangingRate);
            detectionAngle = Mathf.Lerp(detectionAngle, _TDetectionAngle, Time.deltaTime * RangeChangingRate);
            detectionRadius = Mathf.Lerp(detectionRadius, _TDetectionRadius, Time.deltaTime * RangeChangingRate);

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

        private void ParameterInit()
        {
            _TRayLength = rayLength;
            _TDetectionRadius = detectionRadius;
            _TDetectionAngle = detectionAngle;
        }


        private void OnTargetReceivedChanged(GameObject newTarget)
        {
            // 在 TargetReceived 改变时执行的逻辑
            Debug.Log("TargetReceived changed to: " + newTarget);
            targetContainer = newTarget;
            targetBroadCastFound = true;
        }

        private void OnTargetMovingStatusChanged(int newValue)
        {
            TargetMovingStatus = newValue;
            Debug.Log(TargetMovingStatus);
        }

        // 在脚本销毁时取消订阅事件，以防止潜在的内存泄漏
        private void OnDestroy()
        {
            if (broadCasterinfocontainer != null)
            {
                broadCasterinfocontainer.TargetReceivedChanged -= OnTargetReceivedChanged;
                broadCasterinfocontainer.TargetMovingStatusChanged -= OnTargetMovingStatusChanged;
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
            // 进行旋转
            leftDirection = Quaternion.Euler(0f, -detectionRotation, 0f) * leftDirection;
            rightDirection = Quaternion.Euler(0f, -detectionRotation, 0f) * rightDirection;

            //调试模式下计算预设点位
            // 计算扇形的两个边缘点
            Quaternion LeftRotation = Quaternion.AngleAxis(-DetectionAngle * 0.5f, Vector3.up);
            Quaternion RightRotation = Quaternion.AngleAxis(DetectionAngle * 0.5f, Vector3.up);
            Vector3 LeftDirection = LeftRotation * detectionDirection;
            Vector3 RightDirection = RightRotation * detectionDirection;
            // 进行旋转
            LeftDirection = Quaternion.Euler(0f, -detectionRotation, 0f) * LeftDirection;
            RightDirection = Quaternion.Euler(0f, -detectionRotation, 0f) * RightDirection;

            // 在 Scene 视图上绘制蓝色圆，表示 RayStarpointOffset
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(origin, 0.05f);



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
                                Gizmos.DrawLine(origin, origin + RayDirection * currentRayLength);

                                // 在扇形范围外检测到目标，更新整体 foundTarget 的值
                                foundTarget = true;
                        }

                    }
                }
            }

            // 运行中在Scene视图上绘制扇形
            DrawSector(origin, leftDirection, rightDirection, detectionRadius, detectionAngle);

            if (EditorMode)
            {
                //调试模式下绘制
                DrawSector(origin, LeftDirection, RightDirection, DetectionRadius, DetectionAngle);
            }

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