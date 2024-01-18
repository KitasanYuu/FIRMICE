using UnityEngine;
using CustomInspector;
using VInspector;
using static UnityEngine.Rendering.HableCurve;

namespace TestField
{
    [RequireComponent(typeof(AlertLine))]
    public class AlertLogic : MonoBehaviour
    {
        [ReadOnly, SerializeField] private float CurrentAlertness;
        [ReadOnly] public bool TargetExposed;
        public bool TempClearExposeStatus;

        [Space2(10)]

        [Foldout("ReceivedParameters")]
        [ReadOnly] public GameObject AlertTarget;
        [ReadOnly] public bool TargetFound;
        [ReadOnly] public bool FoundButOutRange;
        [ReadOnly] public bool TargetAiming;
        [ReadOnly] public bool TargetFire;
        [ReadOnly] public int TargetMoveStatus;

        [Space2(10)]

        [Foldout("Alertness Settings")]
        [SerializeField] private float InitAlertness=0;
        [SerializeField] private float MaxAlertness;
        [Range(0,100),SerializeField] private float AlertnessIncreaseRate;
        [Range(0, 100), SerializeField] private float CrouchIncreaseRate;
        [Range(0, 100), SerializeField] private float SprintIncreaseRate;
        [Space2(10)]
        [Range(0, 100), SerializeField] private float AlertnessDecreaseRate;
        [Space2(10)]
        public float DeathZone;


        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------



        //申明引用脚本
        private AlertLine alertLine;
        private BroadCasterInfoContainer BCIC;

        private void Awake()
        {
            ComponentInit();
        }

        void Start()
        {
            EventSubscribe();
        }

        // Update is called once per frame
        void Update()
        {
            TempClear();
            CountDownExpose();
            Expose();
            AlertnessRecover();
        }

        private void TempClear()
        {
            if (TempClearExposeStatus || Input.GetKey(KeyCode.Backspace))
            {
                TargetExposed = false;
                TempClearExposeStatus = false;
            }
        }

        private void CountDownExpose()
        {
            if (AlertTarget != null)
            {
                if (TargetFound)
                {

                    if (TargetMoveStatus == -1)
                        CurrentAlertness = Mathf.Min(CurrentAlertness + CrouchIncreaseRate * Time.deltaTime, MaxAlertness);
                    else if (TargetMoveStatus == 0 || TargetMoveStatus == 1)
                        CurrentAlertness = Mathf.Min(CurrentAlertness + AlertnessIncreaseRate * Time.deltaTime, MaxAlertness);
                    else if (TargetMoveStatus == 2)
                        CurrentAlertness = Mathf.Min(CurrentAlertness + SprintIncreaseRate * Time.deltaTime, MaxAlertness);

                }
                else if (FoundButOutRange)
                {
                    if (TargetMoveStatus == -1)
                        CurrentAlertness = Mathf.Min(CurrentAlertness + (AlertnessDecreaseRate/10) * Time.deltaTime, MaxAlertness);
                    else if (TargetMoveStatus == 0)
                        CurrentAlertness = Mathf.Min(CurrentAlertness + (AlertnessIncreaseRate/10) * Time.deltaTime, MaxAlertness);
                    else if (TargetMoveStatus == 1)
                        CurrentAlertness = Mathf.Min(CurrentAlertness + (AlertnessIncreaseRate/5) * Time.deltaTime, MaxAlertness);
                    else if (TargetMoveStatus == 2)
                        CurrentAlertness = Mathf.Min(CurrentAlertness + (SprintIncreaseRate / 4) * Time.deltaTime, MaxAlertness);
                }
            }
        }

        private void Expose()
        {
            if (AlertTarget != null)
            {
                if (alertLine.DistanceToTarget <= alertLine.DetectionRadius && TargetFire)
                {
                    TargetExposed = true;
                }

                if(alertLine.DistanceToTarget <= DeathZone && TargetFound)
                {
                    TargetExposed = true;
                }

                if (CurrentAlertness >= 99.0f)
                {
                    TargetExposed = true;
                }
            }
            else
            {
                TargetExposed =false;
                if(CurrentAlertness!=0)
                {
                    CurrentAlertness = Mathf.Lerp(CurrentAlertness, InitAlertness, Time.deltaTime * 10f);
                    if (CurrentAlertness < 1)
                        CurrentAlertness = 0;
                }

            }

        }

        private void AlertnessRecover()
        {
            if (!TargetFound && !FoundButOutRange)
            {
                CurrentAlertness = Mathf.Max(CurrentAlertness - AlertnessDecreaseRate * Time.deltaTime, InitAlertness);
            }
        }

        private void OnTargetFound(bool newValue)
        {
            // 在 TargetReceived 改变时执行的逻辑
            TargetFound = newValue;
        }

        private void OnFoundButOutRange(bool newValue)
        {
            FoundButOutRange = newValue;
        }

        private void OnTargetReceivedChanged(GameObject newTarget)
        {
            // 在 TargetReceived 改变时执行的逻辑
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

        private void ComponentInit()
        {
            alertLine = GetComponent<AlertLine>();
            BCIC = GetComponent<BroadCasterInfoContainer>();
        }

        // 订阅事件
        private void EventSubscribe()
        {
            BCIC.TargetReceivedChanged += OnTargetReceivedChanged;
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
                BCIC.TargetReceivedChanged -= OnTargetReceivedChanged;
                BCIC.TargetMovingStatusChanged -= OnTargetMovingStatusChanged;
                BCIC.Fire -= OnFire;
                BCIC.Aiming -= OnAiming;
                alertLine.TargetFound -= OnTargetFound;
                alertLine.FoundButOutRange -= OnFoundButOutRange;
            }

        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.white;

            // 获取物体的世界坐标
            Vector3 worldPosition = transform.position;
            worldPosition.y = 0.0f;

            // 绘制在xz平面上的圆
            DrawCircleOnXZPlane(worldPosition, DeathZone, 360);
        }

        void DrawCircleOnXZPlane(Vector3 center, float radius, int segments)
        {
            float angleIncrement = 360.0f / segments;

            Vector3 prevPoint = Vector3.zero;

            for (int i = 0; i <= segments; i++)
            {
                float angle = Mathf.Deg2Rad * (i * angleIncrement);
                float x = center.x + radius * Mathf.Cos(angle);
                float z = center.z + radius * Mathf.Sin(angle);
                Vector3 currentPoint = new Vector3(x, center.y, z);

                if (i > 0)
                {
                    Gizmos.DrawLine(prevPoint, currentPoint);
                }

                prevPoint = currentPoint;
            }
        }
    }
}
