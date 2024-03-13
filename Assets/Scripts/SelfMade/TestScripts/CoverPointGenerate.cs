using CustomInspector;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CoverPointGenerate : MonoBehaviour
{
    [SerializeField]
    private Vector2 PointNumberXZ = new Vector2(0, 0);
    [SerializeField]
    private Vector3 RectangleOffset = new Vector3(0,0,0);
    private Vector3 RectangleSize;

    public List<Vector3> generatedPoints = new List<Vector3>();
    public List<Vector3> CoverCornorList = new List<Vector3>();
    public List<Vector3> desiredPoints = new List<Vector3>();

    [Space2(20)]
    public bool GizmosDebug = false;

    void OnValidate()
    {

        RectangleSize = new Vector3(transform.localScale.x +RectangleOffset.x, 0f + RectangleOffset.y, transform.localScale.z + RectangleOffset.z);
        GeneratePoints();
    }

    void GeneratePoints()
    {
        generatedPoints.Clear();
        CoverCornorList.Clear();
        desiredPoints.Clear();

        float halfWidth = RectangleSize.x * 0.5f;
        float halfLength = RectangleSize.z * 0.5f;
        float halfHeight = RectangleSize.y * 0.5f;

        // 计算长方形的四个角
        Vector3 topLeft = new Vector3(transform.position.x - halfWidth, 0f + halfHeight, transform.position.z + halfLength);
        Vector3 topRight = new Vector3(transform.position.x + halfWidth, 0f + halfHeight, transform.position.z + halfLength);
        Vector3 bottomLeft = new Vector3(transform.position.x - halfWidth, 0f + halfHeight, transform.position.z - halfLength);
        Vector3 bottomRight = new Vector3(transform.position.x + halfWidth, 0f + halfHeight, transform.position.z - halfLength);

        // 在相邻角之间生成点
        for (int i = 0; i < PointNumberXZ.x; i++)
        {
            float t = i / (float)(PointNumberXZ.x - 1);
            Vector3 point = Vector3.Lerp(topLeft, topRight, t);
            AddUniquePoint(point);

            point = Vector3.Lerp(bottomRight, bottomLeft, t);
            AddUniquePoint(point);
        }

        // 在相邻角之间生成点
        for (int i = 0; i < PointNumberXZ.y; i++)
        {
            float t = i / (float)(PointNumberXZ.y - 1);
            Vector3 point = Vector3.Lerp(topRight, bottomRight, t);
            AddUniquePoint(point);

            point = Vector3.Lerp(bottomLeft, topLeft, t);
            AddUniquePoint(point);
        }


        foreach (Vector3 coverPoint in CoverCornorList)
        {
            Vector3 closest1 = Vector3.zero;
            Vector3 closest2 = Vector3.zero;
            float minDistance1 = float.MaxValue;
            float minDistance2 = float.MaxValue;

            foreach (Vector3 generatedPoint in generatedPoints)
            {
                if (!CoverCornorList.Contains(generatedPoint))
                {
                    float distance = Vector3.Distance(coverPoint, generatedPoint);

                    if (distance < minDistance1)
                    {
                        minDistance2 = minDistance1;
                        minDistance1 = distance;
                        closest2 = closest1;
                        closest1 = generatedPoint;
                    }
                    else if (distance < minDistance2)
                    {
                        minDistance2 = distance;
                        closest2 = generatedPoint;
                    }
                }
            }

            // 确保两个最近点的射线角度垂直
            Vector3 dir1 = closest1 - coverPoint;
            Vector3 dir2 = closest2 - coverPoint;

            if (!AreRayDirectionsPerpendicular(dir1, dir2))
            {
                // 如果射线方向不垂直，保留距离最短的点
                //Debug.LogWarning("Warning: Ray directions are not perpendicular. Retaining the closest point.");

                // 重新选择最近点
                closest1 = FindNextClosestPoint(coverPoint, generatedPoints, closest1);

                // 重新计算射线方向
                dir1 = closest1 - coverPoint;

                // 如果还是不满足条件，继续处理或放弃这组数据
            }

            // 添加之前检查是否已存在于列表中
            if (!desiredPoints.Contains(closest1))
            {
                desiredPoints.Add(closest1);
            }

            if (!desiredPoints.Contains(closest2))
            {
                desiredPoints.Add(closest2);
            }
        }
    }

    Vector3 FindNextClosestPoint(Vector3 coverPoint, List<Vector3> generatedPoints, Vector3 currentClosest)
    {
        Vector3 newClosest = currentClosest;
        float minDistance = Vector3.Distance(coverPoint, currentClosest);

        foreach (Vector3 point in generatedPoints)
        {
            if (!CoverCornorList.Contains(point) && point != currentClosest)
            {
                float distance = Vector3.Distance(coverPoint, point);

                if (AreRayDirectionsPerpendicular(point - coverPoint, currentClosest - coverPoint) && distance < minDistance)
                {
                    newClosest = point;
                    minDistance = distance;
                }
            }
        }

        return newClosest;
    }

    bool AreRayDirectionsPerpendicular(Vector3 dir1, Vector3 dir2)
    {
        return Mathf.Approximately(Vector3.Dot(dir1.normalized, dir2.normalized), 0f);
    }


    void AddUniquePoint(Vector3 point)
    {
        if (!generatedPoints.Contains(point))
        {
            generatedPoints.Add(point);
        }
        else
        {
            // 如果点已存在，将其添加到CoverCornorList
            if (!CoverCornorList.Contains(point))
            {
                CoverCornorList.Add(point);
            }
        }
    }

    public void DebugGizmosSet(bool newboolean)
    {
        GizmosDebug = newboolean;
    }

#if UNITY_EDITOR

    void OnDrawGizmos()
    {
        if (GizmosDebug)
        {
            foreach (Vector3 point in generatedPoints)
            {
                Gizmos.color = Color.gray;
                Gizmos.DrawSphere(point, 0.1f);
            }
            foreach (Vector3 point in CoverCornorList)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(point, 0.1f);
            }

            foreach (Vector3 point in desiredPoints)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(point, 0.1f);
            }
        }
    }
#endif
}
