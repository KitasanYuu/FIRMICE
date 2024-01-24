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
    }
}
