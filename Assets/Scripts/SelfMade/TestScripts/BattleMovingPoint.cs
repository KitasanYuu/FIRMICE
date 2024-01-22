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
            Debug.Log(safeLocation);

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
            GameObject nearestCover = FindNearestCover(coverObjects);

            if (nearestCover != null)
            {
                // 在掩体周围生成一个点
                Vector3 randomPointAroundCover = RandomPointBetween(nearestCover, nearestCover.transform.position, target.transform.position, 1f);
                return randomPointAroundCover;
            }
            else
            {
                Debug.LogError("No nearest cover found.");
                return Vector3.zero;
            }
        }

        private GameObject FindNearestCover(List<GameObject> coverObjects)
        {
            GameObject nearestCover = null;
            float minDistance = float.MaxValue;
            Vector3 currentPosition = transform.position;

            foreach (GameObject coverObject in coverObjects)
            {
                float distance = Vector3.Distance(currentPosition, coverObject.transform.position);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestCover = coverObject;
                }
            }

            return nearestCover;
        }

        private Vector3 RandomPointBetween(GameObject nearestCover, Vector3 pointA, Vector3 pointB, float distance)
        {
            CoverPointGenerate CPG = nearestCover.GetComponent<CoverPointGenerate>();
            List<Vector3> validPoints = CPG.generatedPoints;
            List<Vector3> suitablePoints = new List<Vector3>();

            foreach (Vector3 coverPoint in validPoints)
            {
                // 发射射线，检测是否可以直接到达目标
                RaycastHit hit;
                if (Physics.Raycast(coverPoint, pointB - coverPoint, out hit))
                {
                    // 判断是否被 nearestCover 遮挡
                    if (hit.collider.gameObject == nearestCover)
                    {
                        Debug.Log("Found a suitable cover point at: " + coverPoint);
                        suitablePoints.Add(coverPoint);
                    }
                }
            }

            if (suitablePoints.Count > 0)
            {
                // 从符合条件的点中随机选择一个返回
                Vector3 randomPoint = suitablePoints[Random.Range(0, suitablePoints.Count)];
                return randomPoint;
            }
            else
            {
                // 如果没有找到符合条件的点，可以在这里处理
                Debug.Log("No suitable cover point found.");
                return Vector3.zero; // 或者返回其他值，表示未找到符合条件的点
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
