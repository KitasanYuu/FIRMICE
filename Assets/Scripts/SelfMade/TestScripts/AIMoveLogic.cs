using CustomInspector;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using TestField;
using UnityEngine;

namespace TestField
{
    public class AIMoveLogic : MonoBehaviour
    {
        [ReadOnly]public float ToTargetDistance;
        public float VaildShootRange;
        public bool InBattle;
        private bool TargetExpose;
        private List<GameObject> HalfCoverList = new List<GameObject>();
        private List<GameObject> FullCoverList = new List<GameObject>();
        private List<GameObject> CoverList = new List<GameObject>();
        private GameObject Target;

        public float ReScanDelay=5f;

        private Vector3 SafePoint;
        private bool FirstEnterBattle=true;

        [Tooltip("在移动时的面向，0代表朝向移动方向，1代表朝向Target")]
        public float Facetoforworddir=0;
        public bool StartMoving;
        public bool IsMoving;
        public bool HasExcuted;

        private AlertLogic AL;
        private BattleMovingPoint BMP;
        private BroadCasterInfoContainer BCIC;
        private CoverUtility coverUtility = new CoverUtility();

        private void Awake()
        {
            ComponentInit();
            MergeCoverLists(); // 在 Awake 中进行 CoverList 合并的初始化
        }

        private void Start()
        {
            EventSubscribe();
        }

        private void Update()
        {
            ParameterUpdate();
            CombatProcessMoving();
            Moving(Facetoforworddir,Target);
            FaceToTarget(Target);


            if (IsDirectToTarget(Target))
            {
                Debug.Log("Direct To Target");
            }
        }

        //战时移动的主控
        private void CombatProcessMoving()
        {
            if (InBattle)
            {
                if (FirstEnterBattle)
                {
                    Vector3 InitSafePoint = coverUtility.FindNearestCoverPoint(gameObject, Target, CoverList);
                    CalcuRouteMove(InitSafePoint);
                    FirstEnterBattle = false;
                }
            }
        }

        private void PositionAdjust()
        {
            if(Target!=null && !IsMoving && InBattle)
            {
                if (IsDirectToTarget(Target))
                {
                    Debug.Log("ReGenered");
                    Vector3 RegeneratedPoint = coverUtility.FindNearestCoverPoint(gameObject,Target, CoverList);
                    CalcuRouteMove(RegeneratedPoint);
                }
            }
        }

        #region Update每帧调用的检测,行动
        //每帧更新是否进入战斗状态
        private void ParameterUpdate()
        {
            TargetExpose = AL.TargetExposed;
            if (TargetExpose && !HasExcuted)
            {
                InBattle = true;
                StartPositionAdjust(true);
                HasExcuted = true;
            }
        }

        //Update控制AStar移动启动，AStar到达目标点后会自动终止

        private void Moving(float FacetoForwordDir = 0,GameObject Target = null)
        {
            if (BMP != null && StartMoving)
            {
                Debug.Log("StartMoving");
                BMP.AStarMoving(FacetoForwordDir,Target);
                StartMoving = !BMP.HasReachedPoint;
            }
            IsMoving = !BMP.HasReachedPoint;
        }
        #endregion

        #region 需要传入物体的Function
        //使自身朝向目标
        private void FaceToTarget(GameObject Target)
        {
            if (Target != null)
            {
                // 计算目标朝向
                Quaternion targetRotation = Quaternion.LookRotation((Target.transform.position - transform.position).normalized);

                // 使用Slerp插值来平滑地转向目标朝向
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5 * Time.deltaTime);
            }

        }

        //传入目标点自动计算路径并开始移动
        private void CalcuRouteMove(Vector3 newPoint)
        {
            BMP?.SeekerCalcu(newPoint);
            StartMoving = true;
        }

        //用于判断目标是否在自身有效射程内，传入目标GameObject
        bool VaildShootingPosition(GameObject Target)
        {
            bool VaildPosition;
            if (Target != null)
            {
                ToTargetDistance = Vector3.Distance(Target.transform.position, gameObject.transform.position);
                //判断目标是否在有效射程内
                if (ToTargetDistance > VaildShootRange)
                {
                    VaildPosition = false;
                }
                else
                {
                    VaildPosition = true;
                }
            }
            else
            {
                VaildPosition = false;
            }
            return VaildPosition;
        }

        //用于判定是否直接面对Target
        bool IsDirectToTarget(GameObject Target)
        {
            if (BCIC == null || Target == null)
            {
                // Handle the case where bcic or Target is null
                return false;
            }

            List<GameObject> CoverList = BCIC.CoverList;

            // 获取自身位置
            Vector3 selfPosition = transform.position;

            // 获取目标位置
            Vector3 targetPosition = Target.transform.position;

            // 获取从自身到目标的方向向量
            Vector3 directionToTarget = targetPosition - selfPosition;

            // 发射射线检测是否击中CoverList
            RaycastHit hitInfo;
            bool hit = Physics.Raycast(selfPosition, directionToTarget.normalized, out hitInfo);

            // 如果击中CoverList，返回 false，否则返回 true
            return !hit || !CoverList.Contains(hitInfo.collider.gameObject);
        }
        #endregion

        #region 组件初始化，订阅管理
        private void ComponentInit()
        {
            BCIC = GetComponent<BroadCasterInfoContainer>();
            BMP = GetComponent<BattleMovingPoint>();
            AL = GetComponent<AlertLogic>();
        }

        private void EventSubscribe()
        {
            BCIC.FullcoverChanged += OnFullCoverChanged;
            BCIC.HalfcoverChanged += OnHalfCoverChanged;
            BCIC.TargetReceivedChanged += TargetReceived;
        }
        private void OnDestroy()
        {
            if (BCIC != null)
            {
                BCIC.FullcoverChanged -= OnFullCoverChanged;
                BCIC.HalfcoverChanged -= OnHalfCoverChanged;
                BCIC.TargetReceivedChanged -= TargetReceived;
            }
        }
        #endregion

        private void StartPositionAdjust(bool newbool)
        {
            // 取消之前可能存在的重复调用
            CancelInvoke("PositionAdjust");

            if (newbool)
            {
                InvokeRepeating("PositionAdjust", 2.0f, ReScanDelay);
            }
        }

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

        private void MergeCoverLists()
        {
            // 合并 FullCoverList 和 HalfCoverList 到 CoverList
            CoverList = new List<GameObject>(FullCoverList);
            CoverList.AddRange(HalfCoverList);
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