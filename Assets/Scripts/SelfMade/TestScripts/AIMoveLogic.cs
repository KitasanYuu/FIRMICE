using System.Collections;
using System.Collections.Generic;
using TestField;
using UnityEngine;

namespace TestField
{

    public class AIMoveLogic : MonoBehaviour
    {
        private List<GameObject> HalfCoverList = new List<GameObject>();
        private List<GameObject> FullCoverList = new List<GameObject>();
        private GameObject Target;
        public List<GameObject> CoverList = new List<GameObject>();
        private Vector3 SafePoint;


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
            if (Input.GetKeyDown(KeyCode.R))
            {
                MoveToSafeLocation();
                BMP?.SeekerCalcu(SafePoint);

            }

        }

        private void MoveToSafeLocation()
        {
            if (Target == null || CoverList == null)
            {
                // 进行错误处理，或者直接返回
                Debug.LogError("Target or CoverList is null.");
                return;
            }
            // 执行移动逻辑，使用 CoverList 进行射线检测等
            Vector3 safeLocation = coverUtility.FindNearestCoverPointOnRoute(gameObject, Target, CoverList);
            SafePoint = safeLocation;
            //Debug.Log(safeLocation);
        }

        private void ComponentInit()
        {
            BCIC = GetComponent<BroadCasterInfoContainer>();
            BMP = GetComponent<BattleMovingPoint>();
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

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(SafePoint, 0.5f);
        }
#endif
    }
}