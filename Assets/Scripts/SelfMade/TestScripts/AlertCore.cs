using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using CustomInspector;
using System;
using VInspector;

namespace TestField
{
    [RequireComponent(typeof(BroadCasterInfoContainer))]
    public class AlertCore : MonoBehaviour
    {
        [Tab("AlertRange")]
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        [HorizontalLine("Debug", 2, FixedColor.Gray)]
        public bool EnableGizmozOfAlertRange = false;
        [Tooltip("这个只是用来启用接收端口，让你可以在非运行状态下能够在Gizmos上画出警戒线")]
        public bool TestRay = false;

        [HorizontalLine("ParameterView", 2, FixedColor.Gray)]
        [ReadOnly] public bool foundTarget;
        [ReadOnly] public bool Hitbutoutrange;
        [ReadOnly] public float DistanceToTarget;
        [ReadOnly] public int TargetMovingStatus;
        [CustomInspector.ShowIf(nameof(TestRay), style = DisabledStyle.GreyedOut)] public GameObject targetContainer;
        public List<GameObject> targetlist = new List<GameObject>();

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        [HorizontalLine("SyncRangeSettings", 2, FixedColor.Gray)]
        [Tooltip("指的是目标在不同状态下是否会影响警戒圈的范围")]
        public bool UsingSyncAlertRange;

        [SerializeField, CustomInspector.ShowIf(nameof(UsingSyncAlertRange))]
        private bool SyncRayLength;
        [SerializeField, CustomInspector.ShowIf(BoolOperator.And, nameof(SyncRayLength), nameof(UsingSyncAlertRange))]
        private bool SyncSectorRange;

        //不同情况下的检测范围倍率
        [Range(0, 3), SerializeField, CustomInspector.ShowIf(nameof(UsingSyncAlertRange))] private float CrouchRate = 1.0f;
        [Range(0, 3), SerializeField, CustomInspector.ShowIf(nameof(UsingSyncAlertRange))] private float NormalRate = 1.0f;
        [Range(0, 3), SerializeField, CustomInspector.ShowIf(nameof(UsingSyncAlertRange))] private float SprintRate = 1.0f;

        [SerializeField] private float RangeChangingRate = 10.0f;

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        [HorizontalLine("DetectRaySettings", 2, FixedColor.Gray)]
        public float RayLength = 10.0f; // 射线的长度
        public Vector3 RayStarpointOffset;
        [Space2(10)]
        public LayerMask IgnoreLayer; // 障碍物层

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        [HorizontalLine("DetectRangeSettings", 2, FixedColor.Gray)]
        public bool syncRadiusWithRayLength = false; // 是否同步扇形的边长为射线长度
        [CustomInspector.ShowIf(nameof(MyMethod))]
        public float DetectionRadius = 10.0f; // 扇形的半径
        public bool MyMethod() => syncRadiusWithRayLength == false;

        public float DetectionAngle = 45f; // 扇形的角度
        public float detectionRotation = 0f; // 扇形的旋转角度
        private float TotalRotation;
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        [Tab("AlertLogic")]
        [HorizontalLine("Debug", 2, FixedColor.Gray)]
        public bool EnableGizmosOfAlertLogic = false;
        // 用于清除目标暴露状态的标志
        public bool TempClearExposeStatus;

        [HorizontalLine("ParameterView", 2, FixedColor.Gray)]
        // 警戒度
        [ReadOnly] public float CurrentAlertness;
        // 是否被发现
        [ReadOnly] public bool TargetExposed;
        private bool ExposedStatusContainer;

        [Space2(10)]

        [ReadOnly] public bool RecoverProceeding;
        // 是否瞄准目标
        [ReadOnly] public bool TargetAiming;
        // 是否开火
        [ReadOnly] public bool TargetFire;
        //只是一个占位符，未来可能会适配角色特殊技能
        [Tooltip("只是一个占位符，未来可能会适配角色特殊技能")]
        [ReadOnly] public bool TargetOTA;
        // 目标的移动状态
        [ReadOnly] public int TargetMoveStatus;

        [Space2(10)]

        [HorizontalLine("AlertLogicSettings", 2, FixedColor.Gray)]
        // 初始警戒度
        [Tooltip("警戒度的下限")]
        [SerializeField] private float InitAlertness = 0;
        // 最大警戒度
        [Tooltip("警戒度的上限")]
        [SerializeField] private float MaxAlertness;
        // 警戒度增加速率
        [Tooltip("在正常情况下警戒度增加的速度")]
        [Range(0, 100), SerializeField] private float AlertnessIncreaseRate;
        // 蹲伏状态时的警戒度增加速率
        [Tooltip("在目标潜伏时警戒度增加的速度")]
        [Range(0, 100), SerializeField] private float CrouchIncreaseRate;
        // 冲刺状态时的警戒度增加速率
        [Tooltip("在目标冲刺时警戒度增加的速度")]
        [Range(0, 100), SerializeField] private float SprintIncreaseRate;
        [Space2(10)]
        // 警戒度减少速率
        [Range(0, 100), SerializeField] private float AlertnessDecreaseRate;
        [Space2(10)]
        // 瞬间发现区域
        [Tooltip("在这个范围内会直接被发现，一般不要设的太大，会出现奇怪的问题")]
        public float DeathZone;
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        [Tab("INFOShared")]
        [HorizontalLine("Debug", 2, FixedColor.Gray)]
        public bool EnableGizmosOfINFOShare = false;

        [HorizontalLine("INFOShareSettings", 2, FixedColor.Gray)]
        public bool ReceiveSharedINFO;
        public float SharedDistance;
        [ReadOnly, SerializeField] private List<GameObject> OtherReceiver = new List<GameObject>();

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //申明外部脚本引用
        private TargetContainer targetcontainer;
        private BroadCasterInfoContainer BCIC;
        private AIMove AiM;

        //动态范围的参数
        private float _TRayLength;
        private float _TDetectionRadius;
        private float _TDetectionAngle;
        private float rayLength;
        private float detectionRadius;
        private float detectionAngle;

        private Vector3 detectionDirection = Vector3.forward; // 扇形的方向

        //判断是否达成条件的bool参数
        private bool notarget;
        private bool targetBroadCastFound;

        //广播事件
        public Action<bool> ExposeStatusChanged;

        //调试模式
        private bool EditorMode = true;

        #region 验证参数是否合法
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
        #endregion

        private void Awake()
        {
            ComponentInit();
        }

        private void Start()
        {
            EventSubscribe();
            Startinit();

            EditorMode = false;
        }

        private void Update()
        {
            AlertRange();
            AlertLogic();
            AlertINFOShare();
        }

        #region 不同功能在Update中的调用
        private void AlertRange()
        {
            TargetRetake();
            SyncAlertRange();
            DetectTargets();
        }

        private void AlertLogic()
        {
            // 临时清除目标暴露状态
            //TempClear();
            // 增加警戒度
            AlertnessIncrease();
            // 判断是否被发现
            Expose();
            // 警戒度恢复
            AlertnessRecover();
        }

        private void AlertINFOShare()
        {
            ShareTargetComfirm();
        }
        #endregion

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

            ExposedStatusContainer = !TargetExposed;
        }

        //在BCIC接收到Area广播物体时重新初始化对象
        private void TargetRetake()
        {
            if (targetBroadCastFound)
            {
                Startinit();
                targetBroadCastFound = false;
            }
        }

        #region 原AlertLine实现功能：判定Target是否进入了警戒范围
        //Ray和扇形范围对于Target的不同状态预设处理
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

            if (targetContainer == null)
            {
                _TRayLength = RayLength;
                _TDetectionAngle = DetectionAngle;
                _TDetectionRadius = DetectionRadius;
            }


            rayLength = Mathf.Lerp(rayLength, _TRayLength, Time.deltaTime * RangeChangingRate);
            detectionAngle = Mathf.Lerp(detectionAngle, _TDetectionAngle, Time.deltaTime * RangeChangingRate);
            detectionRadius = Mathf.Lerp(detectionRadius, _TDetectionRadius, Time.deltaTime * RangeChangingRate);

        }

        //判定核心
        private void DetectTargets()
        {
            if (!notarget)
            {
                bool PHBO = Hitbutoutrange;
                bool FT = foundTarget;

                Hitbutoutrange = false;
                foundTarget = false;

                // 获取当前位置
                Vector3 origin = transform.position;
                // 在起始位置上添加偏移量
                origin += RayStarpointOffset;

                DistanceToTarget = Vector3.Distance(origin, targetContainer.transform.position);

                // 遍历目标列表
                foreach (GameObject targetObject in targetlist)
                {
                    // 获取目标的位置
                    Vector3 targetPosition = targetObject.transform.position;

                    // 计算朝向目标的标准化方向向量
                    Vector3 direction = (targetPosition - origin).normalized;
                    Vector3 RayDirection = (targetPosition - origin).normalized;

                    TotalRotation = detectionRotation + transform.rotation.eulerAngles.y;
                    // 进行旋转
                    direction = Quaternion.Euler(0f, TotalRotation, 0f) * direction;

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

                if (FT != foundTarget)
                {
                    //OnTargetFound(foundTarget);
                }

                if (PHBO != Hitbutoutrange)
                    //OnFoundButOutRange(Hitbutoutrange);

                    // 如果至少有一条射线命中了目标，执行相应的逻辑
                    if (foundTarget)
                    {
                        //Debug.Log("At least one target detected!");
                        // 在这里执行相应的逻辑
                    }

                if (Hitbutoutrange)
                {
                    //Debug.Log("Detected Target But Out the Range");
                }
            }
            else
            {
                //Notarget的时候将Distance设置成无限大
                DistanceToTarget = float.PositiveInfinity;
            }
        }

        //private void TempClear()
        //{
        //    if (TempClearExposeStatus || Input.GetKey(KeyCode.Backspace))
        //    {
        //        TargetExposed = false;
        //        TempClearExposeStatus = false;
        //    }
        //}
        #endregion

        #region 原AlertLogic实现功能：根据进入范围的目标状态修正警戒度
        // 根据目标状态增加警戒度
        private void AlertnessIncrease()
        {
            if (AiM != null)
            {
                RecoverProceeding = AiM.RecoverStart;
                 if (targetContainer != null && !RecoverProceeding)
                {
                    if (foundTarget)
                    {
                        // 根据目标移动状态增加警戒度
                        if (TargetMoveStatus == -1)
                            CurrentAlertness = Mathf.Min(CurrentAlertness + CrouchIncreaseRate * Time.deltaTime, MaxAlertness);
                        else if (TargetMoveStatus == 0 || TargetMoveStatus == 1)
                            CurrentAlertness = Mathf.Min(CurrentAlertness + AlertnessIncreaseRate * Time.deltaTime, MaxAlertness);
                        else if (TargetMoveStatus == 2)
                            CurrentAlertness = Mathf.Min(CurrentAlertness + SprintIncreaseRate * Time.deltaTime, MaxAlertness);
                    }
                    else if (Hitbutoutrange)
                    {
                        // 目标在检测范围外但在检测距离内，根据目标移动状态减少警戒度
                        if (TargetMoveStatus == -1)
                            CurrentAlertness = Mathf.Min(CurrentAlertness + (AlertnessDecreaseRate / 10) * Time.deltaTime, MaxAlertness);
                        else if (TargetMoveStatus == 0)
                            CurrentAlertness = Mathf.Min(CurrentAlertness + (AlertnessIncreaseRate / 10) * Time.deltaTime, MaxAlertness);
                        else if (TargetMoveStatus == 1)
                            CurrentAlertness = Mathf.Min(CurrentAlertness + (AlertnessIncreaseRate / 5) * Time.deltaTime, MaxAlertness);
                        else if (TargetMoveStatus == 2)
                            CurrentAlertness = Mathf.Min(CurrentAlertness + (SprintIncreaseRate / 4) * Time.deltaTime, MaxAlertness);
                    }
                }
            }
        }

        // 判断是否暴露
        private void Expose()
        {
            RecoverProceeding = AiM.RecoverStart;
            if (targetContainer != null && !RecoverProceeding)
            {
                // 目标在警戒范围内且开火时，暴露状态为true
                if (DistanceToTarget <= DetectionRadius && TargetFire)
                {
                    TargetExposed = true;
                }

                // 目标在瞬间发现区域内且被发现时，暴露状态为true
                if (DistanceToTarget <= DeathZone && foundTarget)
                {
                    TargetExposed = true;
                }

                // 警戒度达到99.0时，暴露状态为true
                if (CurrentAlertness >= MaxAlertness-1f)
                {
                    TargetExposed = true;
                }
            }
            else if (targetContainer == null)
            {
                // 没有目标时，暴露状态为false，同时警戒度逐渐回归初始值
                TargetExposed = false;
                if (CurrentAlertness != 0)
                {
                    CurrentAlertness = Mathf.Lerp(CurrentAlertness, InitAlertness, Time.deltaTime * 10f);
                    if (CurrentAlertness < InitAlertness + 1)
                        CurrentAlertness = InitAlertness;
                }
            }

            if (TargetExposed != ExposedStatusContainer)
            {
                OnTargetExposeStatusChanged(TargetExposed);
                ExposedStatusContainer = TargetExposed;
            }

        }

        // 警戒度在未被发现的情况下慢慢清零
        private void AlertnessRecover()
        {
            if (AiM.RecoverStart || !foundTarget && !Hitbutoutrange)
            {
                CurrentAlertness = Mathf.Max(CurrentAlertness - AlertnessDecreaseRate * Time.deltaTime, InitAlertness);
            }
        }
        #endregion

        #region 原AlertINFOShared实现功能：在目标暴露的时候向周围其他目标广播目标
        private void ShareTargetComfirm()
        {
            // 在Update中发射射线，设置足够大的射线长度
            float maxDistance = Mathf.Infinity; // 设置射线的最大长度为无穷大

            // 遍历列表中的每个目标物体
            foreach (GameObject target in OtherReceiver)
            {
                Vector3 SelfPosition = new Vector3(transform.position.x, transform.position.y + 0.3f, transform.position.z);
                Vector3 TargetPosition = new Vector3(target.transform.position.x, target.transform.position.y + 0.3f, target.transform.position.z);
                Vector3 DIR = new Vector3((TargetPosition - SelfPosition).x, (TargetPosition - SelfPosition).y + 0.3f, (TargetPosition - SelfPosition).z);

                // 构造射线，从当前物体位置到目标物体位置
                Ray ray = new Ray(SelfPosition, DIR);

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
                            AlertCore AC = target.GetComponent<AlertCore>();
                            if (AC != null)
                            {
                                if (AC != null && TargetExposed && AC.ReceiveSharedINFO)
                                {
                                    AC.O_ExposeStatusSet(true);
                                }
                                //else if (alertlogic != null && !AL.TargetExposed && AIS.ReceiveSharedINFO)
                                //{
                                //    alertlogic.TargetExposed = false;
                                //}
                            }

                        }
                    }
                }
            }
        }

        //外部可以访问并修改ExposeStatus的方法
        public void O_ExposeStatusSet(bool ExposeStatus)
        {
            TargetExposed = ExposeStatus;
            OnTargetExposeStatusChanged(ExposeStatus);
        }

        #endregion

        #region 广播

        protected virtual void OnTargetExposeStatusChanged(bool newExposeStatus)
        {
            ExposeStatusChanged?.Invoke(newExposeStatus);
        }

        #endregion

        #region 组件初始化，订阅管理
        private void ComponentInit()
        {
            BCIC = GetComponent<BroadCasterInfoContainer>();
            AiM = GetComponent<HumanoidMoveLogic>();
        }

        private void OnTargetReceivedChanged(GameObject newTarget)
        {
            // 在 TargetReceived 改变时执行的逻辑
            //Debug.Log("TargetReceived changed to: " + newTarget);
            targetContainer = newTarget;
            targetBroadCastFound = newTarget ? true : false;
            notarget = newTarget ? false : true;
        }

        private void OnTargetMovingStatusChanged(int newValue)
        {
            TargetMovingStatus = newValue;
            //Debug.Log(TargetMovingStatus);
        }

        private void OnFire(bool newValue)
        {
            TargetFire = newValue;
        }

        private void OnAiming(bool newValue)
        {
            TargetAiming = newValue;
        }

        private void OnOtherReceiverChanged(List<GameObject> newList)
        {
            OtherReceiver = newList;
        }

        // 订阅事件
        private void EventSubscribe()
        {
            BCIC.AlertTargetReceivedChanged += OnTargetReceivedChanged;
            BCIC.TargetMovingStatusChanged += OnTargetMovingStatusChanged;
            BCIC.Fire += OnFire;
            BCIC.Aiming += OnAiming;
            BCIC.OtherReceiverChanged += OnOtherReceiverChanged;
        }

        // 在脚本销毁时取消订阅事件，以防止潜在的内存泄漏
        private void OnDestroy()
        {
            if (BCIC != null)
            {
                BCIC.AlertTargetReceivedChanged -= OnTargetReceivedChanged;
                BCIC.TargetMovingStatusChanged -= OnTargetMovingStatusChanged;
                BCIC.Fire -= OnFire;
                BCIC.Aiming -= OnAiming;
                BCIC.OtherReceiverChanged -= OnOtherReceiverChanged;
            }
        }
        #endregion

        #region 绘制可视化部分
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (EnableGizmosOfAlertLogic)
            {
                // 获取物体的世界坐标
                Vector3 worldPosition = transform.position;
                worldPosition.y = 0.0f;

                // 绘制在xz平面上的圆
                DrawCircleOnXZPlane(worldPosition, DeathZone, 360);
            }

            if (EnableGizmozOfAlertRange)
            {
                TotalRotation = detectionRotation + transform.rotation.eulerAngles.y;
                // 正确的获取y轴旋转角度方法

                // 获取当前位置
                Vector3 origin = transform.position;
                origin += RayStarpointOffset;

                // 计算扇形的两个边缘点
                Quaternion leftRotation = Quaternion.AngleAxis(-detectionAngle * 0.5f, Vector3.up);
                Quaternion rightRotation = Quaternion.AngleAxis(detectionAngle * 0.5f, Vector3.up);
                Vector3 leftDirection = leftRotation * detectionDirection;
                Vector3 rightDirection = rightRotation * detectionDirection;
                // 进行旋转
                leftDirection = Quaternion.Euler(0f, TotalRotation, 0f) * leftDirection;
                rightDirection = Quaternion.Euler(0f, TotalRotation, 0f) * rightDirection;

                //调试模式下计算预设点位
                // 计算扇形的两个边缘点
                Quaternion LeftRotation = Quaternion.AngleAxis(-DetectionAngle * 0.5f, Vector3.up);
                Quaternion RightRotation = Quaternion.AngleAxis(DetectionAngle * 0.5f, Vector3.up);
                Vector3 LeftDirection = LeftRotation * detectionDirection;
                Vector3 RightDirection = RightRotation * detectionDirection;
                // 进行旋转
                LeftDirection = Quaternion.Euler(0f, TotalRotation, 0f) * LeftDirection;
                RightDirection = Quaternion.Euler(0f, TotalRotation, 0f) * RightDirection;

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
                                if (distanceToTarget <= currentRayLength && hit.collider.gameObject == targetContainer || targetlist.Contains(hit.collider.gameObject))
                                {
                                    // 在扇形区域内检测到目标，绘制黄色射线
                                    Gizmos.color = Color.yellow;
                                    Gizmos.DrawLine(origin, origin + RayDirection * currentRayLength);
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

            if (EnableGizmosOfINFOShare)
            {
                // 获取物体的世界坐标
                Vector3 worldPosition = transform.position;
                worldPosition.y = 0.0f;

                // 绘制在xz平面上的圆
                DrawShareCircleOnXZPlane(worldPosition, SharedDistance, 360);

                // 遍历列表中的每个目标物体
                foreach (GameObject target in OtherReceiver)
                {
                    AlertCore AC = target.GetComponent<AlertCore>();

                    if (AC != null)
                    {
                        // 计算两者之间的距离，只考虑X和Z轴
                        float distance = Vector3.Distance(new Vector3(transform.position.x, 0.0f, transform.position.z),
                                                          new Vector3(target.transform.position.x, 0.0f, target.transform.position.z));

                        if (AC.ReceiveSharedINFO)
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

                foreach (GameObject target in OtherReceiver)
                {
                    // 构造射线，从当前物体位置到目标物体位置
                    Ray ray = new Ray(transform.position, target.transform.position - transform.position);
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawRay(ray);
                }
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

        private void DrawCircleOnXZPlane(Vector3 center, float radius, int segments)
        {
            Handles.color = Color.white;

            Vector3 axis = Vector3.up;  // 指定轴向为y轴，即绘制在xz平面上
            Handles.DrawWireDisc(center, axis, radius);
        }

        private void DrawShareCircleOnXZPlane(Vector3 center, float radius, int segments)
        {
            Handles.color = Color.cyan;

            Vector3 axis = Vector3.up;  // 指定轴向为y轴，即绘制在xz平面上
            Handles.DrawWireDisc(center, axis, radius);
        }
#endif
        #endregion
    }
}
