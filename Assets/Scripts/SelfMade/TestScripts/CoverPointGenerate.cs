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

    void OnValidate()
    {

        RectangleSize = new Vector3(transform.localScale.x +RectangleOffset.x, 0f + RectangleOffset.y, transform.localScale.z + RectangleOffset.z);
        GeneratePoints();
    }

    void GeneratePoints()
    {
        generatedPoints.Clear();

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

    }

    void AddUniquePoint(Vector3 point)
    {
        // 添加唯一的点
        if (!generatedPoints.Contains(point))
        {
            generatedPoints.Add(point);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;

        foreach (Vector3 point in generatedPoints)
        {
            Gizmos.DrawSphere(point, 0.1f);
        }
    }
}
