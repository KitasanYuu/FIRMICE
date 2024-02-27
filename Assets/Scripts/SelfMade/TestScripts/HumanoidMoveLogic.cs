using CustomInspector;
using Pathfinding;
using System.Collections.Generic;
using UnityEngine;
using VInspector;

namespace TestField
{
    public class HumanoidMoveLogic : AIMove
    {
        [Tab("MoveLogic")]
        [HorizontalLine("MoveSettings", 2, FixedColor.Gray)]
        public float NormalSpeed;
        public float SprintSpeed;
        public float AimSpeed;
        public float stoppingDistance;

        [HorizontalLine("DifferentiatingSettings", 2, FixedColor.Gray)]
        [Tooltip("指定该AI的有效射程")]
        public float ValidShootRange;
        [Tooltip("用来指定物体在距离检测时发射的射线忽略对象层级，建议选择可能碰撞的AI层级")]
        public LayerMask RayIgnoreLayer;
        [Tooltip("决定了在保持距离方法中与目标保持的特定距离")]
        public float KeepDistanceToTarget;
        [Tooltip("决定了每隔多少时间重新寻找一次安全点位")]
        public float ReScanDelay = 5f;

        [Foldout("ParameterCheck")]
        [ReadOnly, SerializeField]
        private bool TargetExpose;
        [ReadOnly, SerializeField]
        private GameObject Target;
        [ReadOnly]
        public GameObject CurrentCoverSelected;
        [ReadOnly]
        public float ToTargetDistance;
        [SerializeField, ReadOnly]
        private bool InBattle;
        [SerializeField, ReadOnly, Tooltip("在移动时的面向，true代表朝向移动方向，false代表朝向Target")]
        private bool FacetoMoveDir = true;
        [SerializeField, ReadOnly]
        private bool StartMoving;
        [SerializeField, ReadOnly]
        private bool IsMoving;
        [SerializeField, ReadOnly]
        private bool HasExcuted;
        [SerializeField, ReadOnly]
        private bool NeedKeepDistance = false;
        [SerializeField, ReadOnly]
        private bool isApproachThreadRecursing = false;
        [SerializeField, ReadOnly]
        private bool isDistanceKeeperRecursing = false;
        [SerializeField, ReadOnly]
        private Vector3 TargetPosition;
        [SerializeField, ReadOnly]
        private Vector3 SelfPosition;
        [SerializeField, ReadOnly]
        private Quaternion TargetRotation;

        //声明引用脚本组件
        private AlertCore AC;
        private AIBrain AIB;
        private BroadCasterInfoContainer BCIC;
        private Seeker seeker;
        private CoverUtility coverUtility = new CoverUtility();
        private AIFunction aif = new AIFunction();

        private bool hasGeneratedPoint;
        private List<GameObject> HalfCoverList = new List<GameObject>();
        private List<GameObject> FullCoverList = new List<GameObject>();
        private List<GameObject> OccupiedCoverList = new List<GameObject>();
        private List<GameObject> FreeCoverList = new List<GameObject>();
        private List<GameObject> CoverList = new List<GameObject>();


        private GameObject PreviousCoverSelected;
        private Vector3 SafePoint;
        private bool FirstEnterBattle = true;
        private bool NoCoverNear;
        private bool hasRotationed = true;
        private Vector3 InitPosition;
        private Quaternion InitRotation;

        //随便写的两个占位函数方便管理
        private bool facetotarget =false;
        private bool facetoforward = true;
        private int normalspeed = 0;
        private int aimspeed = 1;
        private int sprintspeed = 2;

        //
        private bool RecoverComplete;

        private void Awake()
        {
            InitPosition = transform.position;
            InitRotation = transform.rotation;
            ComponentInit();
            MergeCoverLists(); // 在 Awake 中进行 CoverList 合并的初始化
        }

        private void Start()
        {
            EventSubscribe();
        }

        private void Update()
        {
            IsFaceToTarget();
            InBattleLogic();
            Moving();

            //if (Target != null)
            //{
            //    Debug.LogError(Target.transform.position);
            //}
        }

        private void InBattleLogic()
        {
            OutBattleParameterUpdate();
            if (InBattle)
            {
                BattleStart();
                TargetOutRange();
                FaceToTarget(Target);
                DistanceKeeper(Target);
                CoverOccupied();
            }
            PositionRecover();
        }

        private void WonderingLogic()
        {

        }

        //战时移动的主控
        private void BattleStart()
        {
            if (InBattle)
            {
                //如果刚刚进入战斗则首先寻找最近的掩体的安全点
                if (FirstEnterBattle)
                {
                    CurrentCoverSelected = coverUtility.FindNearestCover(gameObject, FreeCoverList);
                    Vector3 InitSafePoint = coverUtility.FindNearestCoverPoint(gameObject, Target, FreeCoverList, true, CurrentCoverSelected);
                    CCalcuRouteMove(InitSafePoint, sprintspeed, facetoforward);
                    NiBuXuTiQianBian = true;
                    Debug.LogWarning("BattleInit");
                    FirstEnterBattle = false;
                    RecoverComplete = false;
                }

                if (!IsMoving && !FirstEnterBattle && !NiBuXuTiQianBian)
                {
                    FacetoMoveDir = false;
                }

            }
        }

        //当目标超出有效射程时会在射程内寻找一个最远的掩体并且移动过去
        private void TargetOutRange()
        {
            bool inValidShootPosition = aif.ValidShootPosition(gameObject, Target, ValidShootRange);
            if (InBattle && !inValidShootPosition)
            {
                if (!IsMoving)
                    hasGeneratedPoint = false;
                // 只有在还没有生成过点时才重新生成
                if (!hasGeneratedPoint)
                {
                    CurrentCoverSelected = coverUtility.FindFarthestCover(gameObject, Target, FreeCoverList, ValidShootRange);
                    //Debug.Log(CurrentCoverSelected);
                    Vector3 ShotinRangePoint = coverUtility.FindFarthestCoverPointInRange(gameObject, Target, FreeCoverList, ValidShootRange, true, CurrentCoverSelected, true);
                    //Debug.Log(ShotinRangePoint);
                    FacetoMoveDir = true;
                    CCalcuRouteMove(ShotinRangePoint, sprintspeed, facetoforward);
                    Debug.LogWarning("TargetOutRange");

                    // 设置标志位，表示已经生成过点了
                    hasGeneratedPoint = true;
                }
            }
            else if (InBattle && inValidShootPosition)
            {
                // 如果目标在有效射击位置，则重置标志位，以便下次需要时重新生成点
                hasGeneratedPoint = false;
            }
        }


        //从激活后每隔固定时间(ReScanDelay)刷新一个最近掩体的安全点位移动过去
        private void PositionAdjust()
        {
            bool isDirectToTarget = aif.IsDirectToTarget(gameObject, Target, RayIgnoreLayer);
            if (Target != null && !IsMoving && InBattle && !NoCoverNear)
            {
                if (isDirectToTarget)
                {
                    CurrentCoverSelected = coverUtility.FindNearestCover(gameObject, FreeCoverList);
                    Vector3 RegeneratedPoint = coverUtility.FindNearestCoverPoint(gameObject, Target, FreeCoverList, true, CurrentCoverSelected);
                    CCalcuRouteMove(RegeneratedPoint, aimspeed, facetotarget, Target);
                    Debug.LogWarning("PositionAdjust");
                }
                else if (!isDirectToTarget)
                {
                    CurrentCoverSelected = coverUtility.FindNearestCover(gameObject, FreeCoverList);
                    Identity ID = CurrentCoverSelected?.GetComponent<Identity>();
                    if (ID.Covertype == "FullCover")
                    {
                        ApproachingTarget(true);
                        Debug.LogError("PositionAdjustUsing");
                    }
                }
            }
        }

        #region Update每帧调用的检测,行动
        //每帧更新是否进入战斗状态
        private void EnterBattleParameterUpdate()
        {
            if (TargetExpose && !HasExcuted)
            {
                InBattle = true;
                StartPositionAdjust(true);
                HasExcuted = true;
            }
        }

        private void OutBattleParameterUpdate()
        {
            if (Target == null || BCIC.NeedBackToOrigin)
            {
                CurrentCoverSelected = null;
                HasExcuted = false;
                InBattle = false;
                AC.CurrentAlertness = 0;
                AC.O_ExposeStatusSet(false);
                FacetoMoveDir = true;
                FirstEnterBattle = true;
                NeedKeepDistance = false;
                hasRotationed = false;
                RecoverStart = true;
                SetSelfFaceInfo(facetoforward);
            }
        }

        //声明目前生成并占用的掩体，防止别的AI也将点生成在这个掩体上，造成挤兑
        private void CoverOccupied()
        {
            if (InBattle)
            {
                Identity ID = CurrentCoverSelected?.GetComponent<Identity>();
                if (ID != null)
                {
                    ID.SetOccupiedUseage(true, gameObject);
                }
                // 如果上一个选中的掩体不等于当前选中的掩体，执行某些操作
                if (PreviousCoverSelected != CurrentCoverSelected && PreviousCoverSelected != null)
                {
                    Identity id = PreviousCoverSelected?.GetComponent<Identity>();
                    if (id != null)
                    {
                        id.SetOccupiedUseage(false, null);
                    }
                }
                PreviousCoverSelected = CurrentCoverSelected;
            }
        }

        //用于目标脱离后的归位
        private void PositionRecover()
        {
            if (!InBattle)
            {
                if (RecoverStart)
                {
                    if (!RecoverComplete)
                    {
                        //RoutePoint.Clear();
                        CCalcuRouteMove(InitPosition, sprintspeed, facetoforward);
                        RecoverComplete = true;
                    }

                    foreach (GameObject OccupiedCover in OccupiedCoverList)
                    {
                        Identity ID = OccupiedCover.GetComponent<Identity>();
                        if (ID != null)
                            ID.SetOccupiedUseage(false, null);
                    }
                    float DistanceToOrigin = Vector3.Distance(transform.position, InitPosition);
                    if (DistanceToOrigin < 1 && DistanceToOrigin >= 0)
                    {
                        RecoverStart = false;
                    }
                }

                if (!IsMoving && !hasRotationed)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, InitRotation, Time.deltaTime * 5f);
                    if (transform.rotation == InitRotation)
                        hasRotationed = true;
                }
            }

        }

        //Update控制AStar移动启动，AStar到达目标点后会自动终止
        private void Moving()
        {
            if (StartMoving)
            {
                //Debug.Log("StartMoving");
                AStarMoving();
                StartMoving = !HasReachedPoint;
            }
            IsMoving = !HasReachedPoint;
        }
        #endregion 

        #region 用来控制自身与目标保持距离的递归函数
        //用来在无法直接面对目标时在预设保持距离的范围内生成一个可以直接面对目标的点
        private void ApproachingTarget(bool NoCoverNear = false)
        {
            if (!isApproachThreadRecursing)
            {
                ApproachingTargetRecursive(NoCoverNear, KeepDistanceToTarget);
            }
        }

        private void ApproachingTargetRecursive(bool NoCoverNear, float currentKeepDistance)
        {
            if (NoCoverNear && Target != null)
            {
                // 获取自身位置和目标位置
                Vector3 selfPosition = transform.position;
                Vector3 targetPosition = Target.transform.position;

                // 计算自身到目标的方向向量
                Vector3 directionToTarget = (targetPosition - selfPosition).normalized;

                // 计算生成点的位置，确保点到自身的距离不超过目标到自身的距离
                float distanceToTarget = Vector3.Distance(selfPosition, targetPosition);
                float currentDistance = Mathf.Min(distanceToTarget, currentKeepDistance);
                Vector3 generatedPoint = selfPosition + directionToTarget * currentDistance;

                // 确保生成的点与目标之间的距离为 currentKeepDistance
                float distanceToTargetAfterGeneration = Vector3.Distance(generatedPoint, targetPosition);
                if (distanceToTargetAfterGeneration != currentKeepDistance)
                {
                    // 如果生成的点与目标之间的距离不为 currentKeepDistance，可以根据具体需求调整生成的点的位置
                    generatedPoint = targetPosition - directionToTarget * currentKeepDistance;


                    // 发射射线检测是否击中非目标物体
                    RaycastHit hit;
                    if (Physics.Raycast(generatedPoint, directionToTarget, out hit, currentKeepDistance))
                    {
                        isApproachThreadRecursing = true;
                        // 如果击中非目标物体，递减 currentKeepDistance 并重新生成点
                        currentKeepDistance = Mathf.Max(0, currentKeepDistance - 0.5f);
                        ApproachingTargetRecursive(true, currentKeepDistance); // 递归调用
                        return;
                    }
                }
                // 将生成的点往目标方向再靠近一个单位
                generatedPoint += directionToTarget;
                NCalcuRouteMove(generatedPoint, sprintspeed, facetoforward);
                Debug.LogWarning("ApproachingTarget");
                isApproachThreadRecursing = false;
                NeedKeepDistance = true;
            }
        }

        //用来在与目标之间没有掩体时调用，控制自身与目标的距离保持在预设距离内
        private void DistanceKeeper(GameObject Target)
        {
            ToTargetDistance = Vector3.Distance(TargetPosition, transform.position);
            if (Target != null && !isDistanceKeeperRecursing && !IsMoving && (NeedKeepDistance || NoCoverNear))
            {
                DistanceKeeperRecursive(Target, KeepDistanceToTarget, RayIgnoreLayer);
            }
        }

        private void DistanceKeeperRecursive(GameObject Target, float currentDistance, LayerMask ignoreLayer)
        {
            // 创建一个新的位置向量，忽略Y轴
            Vector3 selfPositionXZ = new Vector3(transform.position.x, 0f, transform.position.z);

            // 获取当前物体的位置
            Vector3 SelfPosition = transform.position;
            Vector3 TargetPosition = Target.transform.position;
            Vector3 origin = Target.transform.position;
            // 获取指向当前物体的方向（从B指向A）
            Vector3 direction = SelfPosition - TargetPosition;

            origin.y += 0.1f;

            // 创建射线，起点是B，方向是从B指向A
            Ray ray = new Ray(origin, direction);

            // 可以在场景中可视化射线，方便调试
            Debug.DrawRay(origin, direction, Color.yellow);

            // 进行射线检测，忽略指定层级的物体
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, currentDistance, ~ignoreLayer))
            {
                isDistanceKeeperRecursing = true;
                // 如果射线击中了物体，可以在这里处理相应的逻辑
                Debug.Log("射线击中了：" + hit.collider.gameObject.name);

                // 递减 currentDistance，并递归调用 DistanceKeeperRecursive
                float newDistance = Mathf.Max(0f, currentDistance - 1f);
                DistanceKeeperRecursive(Target, newDistance, ignoreLayer);
                return;
            }

            // 射线不再击中物体的情况下，获取射线的终点
            Vector3 endPoint = ray.GetPoint(currentDistance);

            //Debug.Log(gameObject.name + " " + Vector3.Distance(selfPositionXZ, endPoint));

            // 如果目标物体在XZ平面上与当前物体的距离小于阈值，直接返回，不再执行后续逻辑
            if (Vector3.Distance(selfPositionXZ, endPoint) <= 0.6f)
                return;

            // 在这里处理不再击中物体的情况，可以根据需要返回 endPoint 或执行其他逻辑
            NCalcuRouteMove(endPoint, aimspeed, facetotarget, Target);
            Debug.LogWarning("DistanceKeeper");
            NeedKeepDistance = false;
            isDistanceKeeperRecursing = false;
        }
        #endregion

        #region 计算路线移动的函数
        //传入目标点自动计算路径并开始移动
        private void CCalcuRouteMove(Vector3 newPoint, int movemode, bool facetomovedir = true, GameObject target = null)
        {
            if (newPoint != Vector3.zero)
            {
                NoCoverNear = false;
                SetMoveParameter(movemode, facetomovedir, target);
                SeekerCalcu(newPoint);
                StartMoving = true;
            }
            else if (newPoint == Vector3.zero)
            {
                NoCoverNear = true;
                ApproachingTarget(NoCoverNear);
                Debug.LogError("CCMoveUsing");
            }
        }

        //传入目标点自动计算路径并开始移动(自动保持距离用)
        private void NCalcuRouteMove(Vector3 newPoint, int movemode, bool facetomovedir = true, GameObject target = null)
        {
            if (newPoint != Vector3.zero && InBattle)
            {
                SetMoveParameter(movemode, facetomovedir, target);
                SeekerCalcu(newPoint);
                FacetoMoveDir = false;
                StartMoving = true;
            }
        }
        #endregion

        #region 用来判断自身状态
        //使自身朝向目标
        private void FaceToTarget(GameObject Target)
        {
            TargetPosition = Vector3.zero;
            TargetRotation = Quaternion.identity;
            SelfPosition = Vector3.zero;
            if (Target != null && !FacetoMoveDir)
            {
                SetSelfFaceInfo(facetotarget);
                TargetPosition = Target.transform.position;
                TargetPosition.y = 0;
                SelfPosition = transform.position;
                SelfPosition.y = 0;

                // 计算目标朝向
                TargetRotation = Quaternion.LookRotation((TargetPosition - SelfPosition).normalized);

                // 使用Slerp插值来平滑地转向目标朝向
                transform.rotation = Quaternion.Slerp(transform.rotation, TargetRotation, 5 * Time.deltaTime);
            }
            else
            {
                SetSelfFaceInfo(facetoforward);
            }

        }
        #endregion

        #region 需要传入物体的Function
        //在进入战斗后自动触发，触发后每隔设定的时间会执行一次位置矫正
        private void StartPositionAdjust(bool newbool)
        {
            // 取消之前可能存在的重复调用
            CancelInvoke("PositionAdjust");

            if (newbool)
            {
                InvokeRepeating("PositionAdjust", 5.0f, ReScanDelay);
            }
        }

        #endregion

        #region 组件初始化，订阅管理
        private void ComponentInit()
        {
            BCIC = GetComponent<BroadCasterInfoContainer>();
            AC = GetComponent<AlertCore>();
            AIB = GetComponent<AIBrain>();
            seeker = GetComponent<Seeker>();
            SetSeekerComponent(seeker);
            SetMoveBasicParameter(NormalSpeed, AimSpeed, SprintSpeed, stoppingDistance);
        }

        private void EventSubscribe()
        {
            BCIC.FullcoverChanged += OnFullCoverChanged;
            BCIC.HalfcoverChanged += OnHalfCoverChanged;
            BCIC.OccupiedcoverChanged += OnOccupiedCoverChanged;
            BCIC.FreecoverChanged += OnFreeCoverChanged;
            AIB.AttackTargetChanged += TargetReceived;
            AC.ExposeStatusChanged += OnTargetExposeStatusChanged;

        }
        private void OnDestroy()
        {
            if (BCIC != null)
            {
                BCIC.FullcoverChanged -= OnFullCoverChanged;
                BCIC.HalfcoverChanged -= OnHalfCoverChanged;
                BCIC.OccupiedcoverChanged -= OnOccupiedCoverChanged;
                BCIC.FreecoverChanged -= OnFreeCoverChanged;
                AIB.AttackTargetChanged -= TargetReceived;
                AC.ExposeStatusChanged -= OnTargetExposeStatusChanged;
            }
        }
        #endregion

        #region 广播接收函数流程
        private void TargetReceived(GameObject newvalue)
        {
            Target = newvalue;
        }

        private void OnFullCoverChanged(List<GameObject> newList)
        {
            FullCoverList = newList;
            MergeCoverLists(); // 当 FullCoverList 发生变化时，重新合并 CoverList
        }

        private void OnHalfCoverChanged(List<GameObject> newList)
        {
            HalfCoverList = newList;
            MergeCoverLists(); // 当 HalfCoverList 发生变化时，重新合并 CoverList
        }
        private void OnOccupiedCoverChanged(List<GameObject> newList)
        {
            OccupiedCoverList = newList;
        }
        private void OnFreeCoverChanged(List<GameObject> newList)
        {
            FreeCoverList = newList;
        }

        private void OnTargetExposeStatusChanged(bool ExposeStatus)
        {
            TargetExpose = ExposeStatus;
            EnterBattleParameterUpdate();
        }

        private void MergeCoverLists()
        {
            // 合并 FullCoverList 和 HalfCoverList 到 CoverList
            FreeCoverList = new List<GameObject>(FullCoverList);
            FreeCoverList.AddRange(HalfCoverList);
        }


        #endregion

        #region Gizmos绘制
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(SafePoint, 0.5f);
        }
#endif
        #endregion
    }
}