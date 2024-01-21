using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CoverPointGenerate : MonoBehaviour
{

    [SerializeField]
    private Vector3 RectangleSize = new Vector3(5f, 0f, 3f);

    [SerializeField]
    private int numberOfPoints = 10;  // 点的数量

    private List<Vector3> generatedPoints = new List<Vector3>();

    void OnValidate()
    {
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
        for (int i = 0; i < numberOfPoints; i++)
        {
            float t = i / (float)(numberOfPoints - 1);
            Vector3 point = Vector3.Lerp(topLeft, topRight, t);
            generatedPoints.Add(point);

            point = Vector3.Lerp(topRight, bottomRight, t);
            generatedPoints.Add(point);

            point = Vector3.Lerp(bottomRight, bottomLeft, t);
            generatedPoints.Add(point);

            point = Vector3.Lerp(bottomLeft, topLeft, t);
            generatedPoints.Add(point);
        }

        // 确保生成的点数量不超过预设值
        int excessPoints = generatedPoints.Count - numberOfPoints * 4;
        for (int i = 0; i < excessPoints; i++)
        {
            generatedPoints.RemoveAt(generatedPoints.Count - 1);
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
