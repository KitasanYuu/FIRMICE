using UnityEngine;
using CustomInspector;
using VInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TestField
{
    [RequireComponent(typeof(AlertLine))]
    public class AlertLogic : MonoBehaviour
    {
        // 警戒度
        [ReadOnly] public float CurrentAlertness;
        // 是否被发现
        [ReadOnly] public bool TargetExposed;
        // 用于清除目标暴露状态的标志
        public bool TempClearExposeStatus;

        [Space2(10)]

        [Foldout("ReceivedParameters")]
        // 警戒目标
        [ReadOnly] public GameObject AlertTarget;
        [ReadOnly] public bool RecoverProceeding;
        // 是否发现目标
        [ReadOnly] public bool TargetFound;
        // 是否发现目标但在警戒范围之外
        [ReadOnly] public bool FoundButOutRange;
        // 是否瞄准目标
        [ReadOnly] public bool TargetAiming;
        // 是否开火
        [ReadOnly] public bool TargetFire;
        // 目标的移动状态
        [ReadOnly] public int TargetMoveStatus;

        [Space2(10)]

        [Foldout("Alertness Settings")]
        // 初始警戒度
        [SerializeField] private float InitAlertness = 0;
        // 最大警戒度
        [SerializeField] private float MaxAlertness;
        // 警戒度增加速率
        [Range(0, 100), SerializeField] private float AlertnessIncreaseRate;
        // 蹲伏状态时的警戒度增加速率
        [Range(0, 100), SerializeField] private float CrouchIncreaseRate;
        // 冲刺状态时的警戒度增加速率
        [Range(0, 100), SerializeField] private float SprintIncreaseRate;
        [Space2(10)]
        // 警戒度减少速率
        [Range(0, 100), SerializeField] private float AlertnessDecreaseRate;
        [Space2(10)]
        // 瞬间发现区域
        public float DeathZone;

        // 引用AlertLine和BroadCasterInfoContainer脚本
        private AlertLine alertLine;
        private HumanoidMoveLogic HmoveLogic;
        private BroadCasterInfoContainer BCIC;

        private void Awake()
        {
            // 初始化引用脚本和组件
            ComponentInit();
        }

        void Start()
        {
            // 订阅事件
            EventSubscribe();
        }

        // 每帧更新
        void Update()
        {
            // 临时清除目标暴露状态
            TempClear();
            // 增加警戒度
            AlertnessIncrease();
            // 判断是否被发现
            Expose();
            // 警戒度恢复
            AlertnessRecover();
        }

        // 用于临时清除目标暴露状态
        private void TempClear()
        {
            if (TempClearExposeStatus || Input.GetKey(KeyCode.Backspace))
            {
                TargetExposed = false;
                TempClearExposeStatus = false;
            }
        }

        // 根据目标状态增加警戒度
        private void AlertnessIncrease()
        {
            if (HmoveLogic != null)
            {
                RecoverProceeding = HmoveLogic.RecoverStart;
                if (AlertTarget != null && !RecoverProceeding)
                {
                    if (TargetFound)
                    {
                        // 根据目标移动状态增加警戒度
                        if (TargetMoveStatus == -1)
                            CurrentAlertness = Mathf.Min(CurrentAlertness + CrouchIncreaseRate * Time.deltaTime, MaxAlertness);
                        else if (TargetMoveStatus == 0 || TargetMoveStatus == 1)
                            CurrentAlertness = Mathf.Min(CurrentAlertness + AlertnessIncreaseRate * Time.deltaTime, MaxAlertness);
                        else if (TargetMoveStatus == 2)
                            CurrentAlertness = Mathf.Min(CurrentAlertness + SprintIncreaseRate * Time.deltaTime, MaxAlertness);
                    }
                    else if (FoundButOutRange)
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
            RecoverProceeding = HmoveLogic.RecoverStart;
            if (AlertTarget != null && !RecoverProceeding)
            {
                // 目标在警戒范围内且开火时，暴露状态为true
                if (alertLine.DistanceToTarget <= alertLine.DetectionRadius && TargetFire)
                {
                    TargetExposed = true;
                }

                // 目标在瞬间发现区域内且被发现时，暴露状态为true
                if (alertLine.DistanceToTarget <= DeathZone && TargetFound)
                {
                    TargetExposed = true;
                }

                // 警戒度达到99.0时，暴露状态为true
                if (CurrentAlertness >= 99.0f)
                {
                    TargetExposed = true;
                }
            }
            else if (AlertTarget == null)
            {
                // 没有目标时，暴露状态为false，同时警戒度逐渐回归初始值
                TargetExposed = false;
                if (CurrentAlertness != 0)
                {
                    CurrentAlertness = Mathf.Lerp(CurrentAlertness, InitAlertness, Time.deltaTime * 10f);
                    if (CurrentAlertness < 1)
                        CurrentAlertness = 0;
                }
            }
        }

        // 警戒度在未被发现的情况下慢慢清零
        private void AlertnessRecover()
        {
            if (HmoveLogic.RecoverStart || !TargetFound && !FoundButOutRange)
            {
                CurrentAlertness = Mathf.Max(CurrentAlertness - AlertnessDecreaseRate * Time.deltaTime, InitAlertness);
            }
        }

        //接收的广播事件
        private void OnTargetFound(bool newValue)
        {
            TargetFound = newValue;
        }

        private void OnFoundButOutRange(bool newValue)
        {
            FoundButOutRange = newValue;
        }

        private void OnTargetReceivedChanged(GameObject newTarget)
        {
            AlertTarget = newTarget;
        }

        private void OnTargetMovingStatusChanged(int newValue)
        {
            TargetMoveStatus = newValue;
        }

        private void OnFire(bool newValue)
        {
            TargetFire = newValue;
        }

        private void OnAiming(bool newValue)
        {
            TargetAiming = newValue;
        }

        // 初始化引用脚本和组件
        private void ComponentInit()
        {
            alertLine = GetComponent<AlertLine>();
            HmoveLogic = GetComponent<HumanoidMoveLogic>();
            BCIC = GetComponent<BroadCasterInfoContainer>();
        }

        // 订阅事件
        private void EventSubscribe()
        {
            BCIC.AlertTargetReceivedChanged += OnTargetReceivedChanged;
            BCIC.TargetMovingStatusChanged += OnTargetMovingStatusChanged;
            BCIC.Fire += OnFire;
            BCIC.Aiming += OnAiming;
            alertLine.TargetFound += OnTargetFound;
            alertLine.FoundButOutRange += OnFoundButOutRange;
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
                alertLine.TargetFound -= OnTargetFound;
                alertLine.FoundButOutRange -= OnFoundButOutRange;
            }
        }

#if UNITY_EDITOR
        // 在Scene视图中绘制警戒区域
        private void OnDrawGizmos()
        {
            // 获取物体的世界坐标
            Vector3 worldPosition = transform.position;
            worldPosition.y = 0.0f;

            // 绘制在xz平面上的圆
            DrawCircleOnXZPlane(worldPosition, DeathZone, 360);
        }

        // 在Scene视图中绘制xz平面上的圆
        private void DrawCircleOnXZPlane(Vector3 center, float radius, int segments)
        {
            Handles.color = Color.white;

            Vector3 axis = Vector3.up;  // 指定轴向为y轴，即绘制在xz平面上
            Handles.DrawWireDisc(center, axis, radius);
        }
#endif
    }
}
