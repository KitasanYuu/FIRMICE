using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace TestField
{
    public class CoverUtility
    {

        //用于查找距离自身最近的掩体
        public GameObject FindNearestCover(GameObject selfobject, List<GameObject> coverObjects)
        {
            GameObject nearestCover = null;
            float minDistance = float.MaxValue;
            Vector3 currentPosition = selfobject.transform.position;

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

        //查找距离物体至目标的直线直线距离最短的掩体
        public GameObject FindNearestCoverOnRoute(GameObject selfobject, GameObject target, List<GameObject> coverObjects)
        {
            GameObject nearestCover = null;
            float minDistanceToRoute = float.MaxValue;
            Vector3 linePoint = selfobject.transform.position; // 当前位置为直线上一点
            Vector3 lineVec = target.transform.position - selfobject.transform.position; // 方向向量
            float minExclusionDistance = 3f;

            foreach (GameObject coverObject in coverObjects)
            {

                Vector3 pointVec = coverObject.transform.position - linePoint; // 掩体点到直线上一点的向量
                float coverToTargetDistance = Vector3.Distance(coverObject.transform.position, target.transform.position);

                float distance = Vector3.Cross(lineVec, pointVec).magnitude / lineVec.magnitude;

                float GeneralDistance = coverToTargetDistance + distance;

                if (coverToTargetDistance > minExclusionDistance)
                {
                    Debug.Log(coverObject + " is " + GeneralDistance);

                    // 如果距离小于当前记录的最小距离，更新记录
                    if (GeneralDistance < minDistanceToRoute)
                    {
                        minDistanceToRoute = GeneralDistance;
                        nearestCover = coverObject;
                    }
                }

            }
            return nearestCover;  
        }

        // 查找距离指定目标最远的掩体，并且距离在给定范围内
        public GameObject FindFarthestCover(GameObject target, List<GameObject> coverObjects, float range)
        {
            GameObject farthestCover = null;
            float maxDistance = 0f;  // 初始化最大距离为0

            Vector3 currentPosition = target.transform.position;  // 获取目标位置

            // 遍历所有掩体对象
            foreach (GameObject coverObject in coverObjects)
            {
                // 计算目标位置到掩体对象位置的距离
                float distance = Vector3.Distance(currentPosition, coverObject.transform.position);

                // 检查距离是否在指定范围内
                if (distance <= range)
                {
                    // 如果距离比当前最大距离大，则更新最大距离和最远的掩体对象
                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                        farthestCover = coverObject;
                    }
                }
            }

            return farthestCover;  // 返回范围内最远的掩体对象
        }

        // 查找当前掩体CoverOrigin(CO)往前推进一个掩体O3
        public GameObject FindNextCover(GameObject selfObject, GameObject target, List<GameObject> coverObjects)
        {
            GameObject nearestCover = null;  // 用于存储最近的掩体对象
            GameObject farthestCoverWithinDistanceAroundTarget = null;  // 用于存储在以O2为中心的位置找到的在O2到CO的距离范围内，且CO到O3的距离不超过O1到O2的距离的最远掩体对象
            float minDistance = float.MaxValue;  // 初始化最小距离为正无穷大

            Vector3 currentPosition = selfObject.transform.position;  // 获取自身位置

            // 遍历所有掩体对象
            foreach (GameObject coverObject in coverObjects)
            {
                // 计算自身位置到掩体对象位置的距离
                float distanceToSelf = Vector3.Distance(currentPosition, coverObject.transform.position);

                // 找到离自身最近的掩体
                if (distanceToSelf < minDistance)
                {
                    minDistance = distanceToSelf;
                    nearestCover = coverObject;
                }
            }

            // 计算以O2为中心的位置到自身最近的掩体的距离
            float distanceFromTargetToNearestCover = Vector3.Distance(target.transform.position, nearestCover.transform.position);

            float maxDistanceWithinRange = 0f;  // 初始化在范围内最大距离为0

            // 再次遍历所有掩体对象，找到在以O2为中心的位置找到的在O2到CO的距离范围内，且CO到O3的距离不超过O1到O2的距离的最远掩体
            foreach (GameObject coverObject in coverObjects)
            {
                // 计算以O2为中心的位置到掩体对象位置的距离
                float distanceAroundTarget = Vector3.Distance(target.transform.position, coverObject.transform.position);

                // 计算CO到O3的距离
                float distanceFromNearestToCover = Vector3.Distance(nearestCover.transform.position, coverObject.transform.position);

                // 检查距离，并且排除最近的掩体
                if (coverObject != nearestCover && distanceAroundTarget <= distanceFromTargetToNearestCover && distanceFromNearestToCover <= distanceFromTargetToNearestCover)
                {
                    // 如果距离在范围内并且CO到O3的距离不超过O1到O2的距离，且比当前最大距离大，则更新最大距离和最远的掩体对象
                    if (distanceAroundTarget > maxDistanceWithinRange)
                    {
                        maxDistanceWithinRange = distanceAroundTarget;
                        farthestCoverWithinDistanceAroundTarget = coverObject;
                    }
                }
            }

            return farthestCoverWithinDistanceAroundTarget;  // 返回在以O2为中心的位置找到的在O2到CO的距离范围内，且CO到O3的距离不超过O1到O2的距离的最远掩体对象
        }


        // 获取自身到掩体对象列表中第二远的掩体的距离
        private float GetSecondFarthestDistance(GameObject selfObject, List<GameObject> coverObjects, GameObject nearestCover)
        {
            float maxDistance = 0f;
            foreach (GameObject coverObject in coverObjects)
            {
                if (coverObject != nearestCover)
                {
                    float distance = Vector3.Distance(selfObject.transform.position, coverObject.transform.position);
                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                    }
                }
            }
            return maxDistance;
        }



        //查找掩体周围哪个点是有效的掩体点位（即物体与目标之间的射线上存在选定的掩体）
        public Vector3 RandomPointBetween(GameObject Cover, Vector3 pointB)
        {
            CoverPointGenerate CPG = Cover.GetComponent<CoverPointGenerate>();
            List<Vector3> validPoints = CPG.generatedPoints;
            List<Vector3> suitablePoints = new List<Vector3>();

            foreach (Vector3 coverPoint in validPoints)
            {
                // 发射射线，检测是否可以直接到达目标
                RaycastHit hit;
                if (Physics.Raycast(coverPoint, pointB - coverPoint, out hit))
                {
                    // 判断是否被 nearestCover 遮挡
                    if (hit.collider.gameObject == Cover)
                    {
                        //Debug.Log("Found a suitable cover point at: " + coverPoint);
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

        //随机一个距离物体至目标的直线直线距离最短的掩体上的安全点
        public Vector3 FindNearestCoverPointOnRoute(GameObject selfObject, GameObject target, List<GameObject> coverObjects)
        {
            if (coverObjects.Count == 0)
            {
                Debug.LogError("No cover objects available.");
                return Vector3.zero;
            }

            // 寻找最近的掩体
            //GameObject nearestCover = FindNearestCover(coverObjects);
            GameObject nearestCoverOnRoute = FindNearestCoverOnRoute(selfObject, target, coverObjects);

            if (nearestCoverOnRoute != null)
            {
                // 在掩体周围生成一个点
                Vector3 randomPointAroundCover = RandomPointBetween(nearestCoverOnRoute, target.transform.position);
                return randomPointAroundCover;
            }
            else
            {
                Debug.LogError("No nearest cover found.");
                return Vector3.zero;
            }
        }

        //在最近的掩体上生成一个安全点
        public Vector3 FindNearestCoverPoint(GameObject selfObject, GameObject target, List<GameObject> coverObjects)
        {
            if (coverObjects.Count == 0)
            {
                Debug.LogError("No cover objects available.");
                return Vector3.zero;
            }

            // 寻找最近的掩体
            GameObject nearestCover = FindNearestCover(selfObject, coverObjects);

            if (nearestCover != null)
            {
                // 在掩体周围生成一个点
                Vector3 randomPointAroundCover = RandomPointBetween(nearestCover, target.transform.position);
                return randomPointAroundCover;
            }
            else
            {
                Debug.LogError("No nearest cover found.");
                return Vector3.zero;

            }
        }

        //在有效射程内找一个最远的掩体生成安全点
        public Vector3 FindFarthestCoverPointInRange(GameObject target, List<GameObject> coverObjects,float range)
        {
            if (coverObjects.Count == 0)
            {
                Debug.LogError("No cover objects available.");
                return Vector3.zero;
            }

            // 寻找最近的掩体
            GameObject nearestCover = FindFarthestCover(target, coverObjects,range);

            if (nearestCover != null)
            {
                // 在掩体周围生成一个点
                Vector3 randomPointAroundCover = RandomPointBetween(nearestCover, target.transform.position);
                return randomPointAroundCover;
            }
            else
            {
                Debug.LogError("No nearest cover found.");
                return Vector3.zero;

            }
        }
    }
}
