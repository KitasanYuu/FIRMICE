using UnityEditor;
using UnityEngine;

namespace ActiveRange
{

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
            // 获取物体的世界坐标
            Vector3 worldPosition = transform.position;
            worldPosition.y = 0.0f;

            // 绘制在xz平面上的圆
            DrawCircleOnXZPlane(worldPosition, detectionRadius, 360);
        }

        private void DrawCircleOnXZPlane(Vector3 center, float radius, int segments)
        {
            Handles.color = new Color(1f, 0f, 1f, 0.4f);

            Vector3 axis = Vector3.up;  // 指定轴向为y轴，即绘制在xz平面上
            Handles.DrawWireDisc(center, axis, radius);
        }
#endif
    }
}
