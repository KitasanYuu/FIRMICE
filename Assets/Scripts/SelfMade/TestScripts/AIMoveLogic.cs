using CustomInspector;
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
        public LayerMask CoverLayer;
        private bool TargetExpose;
        private List<GameObject> HalfCoverList = new List<GameObject>();
        private List<GameObject> FullCoverList = new List<GameObject>();
        private List<GameObject> CoverList = new List<GameObject>();
        private GameObject Target;

        private Vector3 SafePoint;


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
            TargetDistanceDetect();
            if (Input.GetKeyDown(KeyCode.R))
            {
                FindSafeLocation();
                BMP?.SeekerCalcu(SafePoint);

            }

        }

        private void FindSafeLocation()
        {
            if (Target == null || CoverList == null)
            {
                // 进行错误处理，或者直接返回
                Debug.LogError("Target or CoverList is null.");
                return;
            }
            //调用FindNearestCoverPointOnRoute回调路径上距离最短的掩体位置
            Vector3 safeLocation = coverUtility.FindNearestCoverPointOnRoute(gameObject, Target, CoverList);
            SafePoint = safeLocation;
        }

        private void TargetDistanceDetect()
        {
            if (Target != null)
            {


            }
        }

        bool VaildShootingPosition()
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
        bool IsDirectToTarget(GameObject Target, LayerMask CoverLayer)
        {
            if (Target != null)
            {
                // 获取自身位置
                Vector3 selfPosition = transform.position;

                // 获取目标位置
                Vector3 targetPosition = Target.transform.position;

                // 获取从自身到目标的方向向量
                Vector3 directionToTarget = targetPosition - selfPosition;

                // 发射射线检测是否击中目标层
                RaycastHit hitInfo;
                bool hit = Physics.Raycast(selfPosition, directionToTarget.normalized, out hitInfo, directionToTarget.magnitude, CoverLayer);

                // 如果击中目标层，返回 false，否则返回 true
                return !hit;
            }

            // 处理 Target 为 null 的情况
            return false;
        }

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

        private void OnDestroy()
        {
            if (BCIC != null)
            {
                BCIC.FullcoverChanged -= OnFullCoverChanged;
                BCIC.HalfcoverChanged -= OnHalfCoverChanged;
                BCIC.TargetReceivedChanged -= TargetReceived;
            }
        }

        private void ParameterUpdate()
        {
            TargetExpose = AL.TargetExposed;
            if (TargetExpose)
                InBattle = true;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(SafePoint, 0.5f);
        }
#endif
    }
}