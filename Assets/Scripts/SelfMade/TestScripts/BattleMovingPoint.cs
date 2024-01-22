using CustomInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TestField
{
    public class BattleMovingPoint : MonoBehaviour
    {
        private List<GameObject> HalfCoverList = new List<GameObject>();
        private List<GameObject> FullCoverList = new List<GameObject>();
        private Vector3 SafePoint;
        private BroadCasterInfoContainer BCIC;
        private GameObject Target;
        public List<GameObject> CoverList;

        CoverUtility coverUtility = new CoverUtility();

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
            Vector3 safeLocation = FindSafeLocation(Target, CoverList);
            SafePoint = safeLocation;
            //Debug.Log(safeLocation);

            // 判断坐标是否可行
            if (IsPositionValid(safeLocation))
            {
                // 在这里可以执行移动逻辑，例如使用NavMeshAgent或者直接修改Transform.position
                // MoveTo(safeLocation);
            }
            else
            {
                // 如果坐标不可行，可以执行一些处理逻辑，例如选择其他目标地点
                Debug.Log("Selected location is not valid. Choosing an alternative location.");

                // 这里可以根据具体情况选择其他目标地点的算法
                // ...

                // 假设简单地选择离 Target 最近的点
                Vector3 alternativeLocation = FindAlternativeLocation();
                // MoveTo(alternativeLocation);
            }
        }

        private Vector3 FindSafeLocation(GameObject target, List<GameObject> coverObjects)
        {
            if (coverObjects.Count == 0)
            {
                Debug.LogError("No cover objects available.");
                return Vector3.zero;
            }

            // 寻找最近的掩体
            //GameObject nearestCover = FindNearestCover(coverObjects);
            GameObject nearestCover = coverUtility.FindNearestCoverOnRoute(gameObject,target, coverObjects);

            if (nearestCover != null)
            {
                // 在掩体周围生成一个点
                Vector3 randomPointAroundCover = coverUtility.RandomPointBetween(nearestCover, target.transform.position);
                return randomPointAroundCover;
            }
            else
            {
                Debug.LogError("No nearest cover found.");
                return Vector3.zero;
            }
        }







        private bool IsPositionValid(Vector3 position)
        {
            // 在这里编写坐标是否可行的判定逻辑，根据实际需求返回true或false
            // 例如，可以检查是否在可行走区域、是否超出边界等等
            // ...

            return true; // 这里是一个简单的示例，始终返回true
        }

        private Vector3 FindAlternativeLocation()
        {
            // 这里可以根据实际需求选择其他目标地点的算法
            // ...

            // 假设简单地选择离 Target 最近的点
            return transform.position + (Target.transform.position - transform.position).normalized * 10f; // 10f是一个简单的距离值
        }

        private void ComponentInit()
        {
            BCIC = GetComponent<BroadCasterInfoContainer>();
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
