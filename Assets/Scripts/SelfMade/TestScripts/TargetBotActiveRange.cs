using UnityEngine;

public class TargetBotActiveRange : MonoBehaviour
{
    public float detectionRadius = 5f; // 检测半径
    public LayerMask targetLayer; // 目标层级
    public int TargetStatus;

    private bool detectedObject = false; // 布尔变量用于跟踪是否检测到了物体

    private void Update()
    {
        SetStatus();
    }

    // 在需要时检查 detectedObject 变量的状态
    private void SetStatus()
    {
        // 获取脚本所在物体的位置
        Vector3 center = transform.position;

        // 检测范围内的物体
        Collider[] colliders = Physics.OverlapSphere(center, detectionRadius, targetLayer);

        if (colliders.Length > 0)
        {
            // 在这里处理检测到的物体
            detectedObject = true;
            // Debug.Log("检测到物体：" + colliders[0].gameObject.name);
        }
        else
        {
            detectedObject = false; // 如果没有检测到物体，将变量设置为false
        }

        if (detectedObject)
        {
            // 处理检测到物体的情况
            TargetStatus = 1;
        }
        else
        {
            // 处理没有检测到物体的情况
            TargetStatus = 0;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 1f, 0.4f); // 设置为紫色，您可以根据需要调整颜色

        // 获取物体的世界坐标
        Vector3 center = transform.position;

        // 忽略Y轴
        center.y = 0f;

        // 绘制外圆
        Gizmos.DrawWireSphere(center, detectionRadius);

        // 绘制内圆（如果需要）
        float innerRadius = detectionRadius; // 用内圆的半径替换 yourInnerRadius
        Gizmos.DrawWireSphere(center, innerRadius);
    }
#endif
}
