using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using CustomInspector;

namespace TestField
{
    public class DetectionScript : MonoBehaviour
    {
        public bool foundTarget;
        [ReadOnly]public GameObject targetContainer;
        public LayerMask IgnoreLayer; // 障碍物层

        [Header("Customize Detection Area")]
        public bool syncRadiusWithRayLength = false; // 是否同步扇形的边长为射线长度
        public float detectionAngle = 45f; // 扇形的角度
        public float detectionRadius = 10.0f; // 扇形的半径
        public float detectionRotation = 0f; // 扇形的旋转角度
        public float rayLength = 10.0f; // 射线的长度

        public List<GameObject> targets = new List<GameObject>();

        private Vector3 detectionDirection = Vector3.forward; // 扇形的方向
        private TargetContainer targetcontainer;

        // 在编辑器中即时更新
        private void OnValidate()
        {
            // 确保数值大于等于0
            detectionAngle = Mathf.Max(detectionAngle, 0f);
            detectionRadius = Mathf.Max(detectionRadius, 0f);
            detectionRotation = Mathf.Max(detectionRotation, 0f);
            rayLength = Mathf.Max(rayLength, 0f);

            if (syncRadiusWithRayLength)
            {
                detectionRadius = rayLength;
            }
            else
            {
                rayLength = Mathf.Min(rayLength, detectionRadius);
            }
        }

        private void Start()
        {
            if (targetContainer != null)
            {
                targetcontainer = targetContainer.GetComponent<TargetContainer>();
                targets = targetcontainer.targets;
            }
        }

        private void Update()
        {
            DetectTargets();
        }

        private void DetectTargets()
        {
            foundTarget = false;

            // 获取当前位置
            Vector3 origin = transform.position;

            // 遍历目标列表
            foreach (GameObject targetObject in targets)
            {
                // 获取目标的位置
                Vector3 targetPosition = targetObject.transform.position;

                // 计算朝向目标的标准化方向向量
                Vector3 direction = (targetPosition - origin).normalized;
                Vector3 RayDirection = (targetPosition - origin).normalized;

                // 进行旋转
                direction = Quaternion.Euler(0f, detectionRotation, 0f) * direction;

                // 计算射线的长度（使用自定义的射线长度）
                float currentRayLength = Mathf.Min(rayLength, detectionRadius);

                // 检测是否在扇形区域内
                float angle = Vector3.Angle(detectionDirection, direction);

                //Debug.Log(angle);

                if (angle <= detectionAngle * 0.5f)
                {
                    // 发射射线到目标
                    Ray ray = new Ray(origin, RayDirection);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit, currentRayLength, ~IgnoreLayer))
                    {
                        // 获取命中目标的名称
                        string hitTargetName = hit.collider.gameObject.name;

                        // 检查命中目标是否为 targetContainer 或者在列表中
                        if (hit.collider.gameObject == targetContainer || targets.Contains(hit.collider.gameObject))
                        {
                            // 这里执行侦测到目标的逻辑
                            foundTarget = true;
                            break; // 如果至少有一条射线命中目标，就跳出循环
                        }
                        else
                        {
                            // 这里执行未预期目标的逻辑
                        }
                    }
                }
            }

            // 如果至少有一条射线命中了目标，执行相应的逻辑
            if (foundTarget)
            {
                Debug.Log("At least one target detected!");
                // 在这里执行相应的逻辑
            }
        }

#if UNITY_EDITOR
        // 在Scene视图中显示Gizmos
        private void OnDrawGizmos()
        {
            // 获取当前位置
            Vector3 origin = transform.position;

            // 计算扇形的两个边缘点
            Quaternion leftRotation = Quaternion.AngleAxis(-detectionAngle * 0.5f, Vector3.up);
            Quaternion rightRotation = Quaternion.AngleAxis(detectionAngle * 0.5f, Vector3.up);
            Vector3 leftDirection = leftRotation * detectionDirection;
            Vector3 rightDirection = rightRotation * detectionDirection;

            // 进行旋转
            leftDirection = Quaternion.Euler(0f, -detectionRotation, 0f) * leftDirection;
            rightDirection = Quaternion.Euler(0f, -detectionRotation, 0f) * rightDirection;

            // 遍历目标列表
            foreach (GameObject targetObject in targets)
            {
                // 获取目标的位置
                Vector3 targetPosition = targetObject.transform.position;

                // 计算朝向目标的标准化方向向量
                Vector3 direction = (targetPosition - origin).normalized;
                Vector3 RayDirection = (targetPosition - origin).normalized;

                // 进行旋转
                direction = Quaternion.Euler(0f, detectionRotation, 0f) * direction;

                // 计算射线的长度（使用自定义的射线长度）
                float currentRayLength = Mathf.Min(rayLength, detectionRadius);

                // 检测是否在扇形区域内
                float angle = Vector3.Angle(detectionDirection, direction);
                if (angle <= detectionAngle * 0.5f)
                {
                    // 在扇形区域内，绘制蓝色射线
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(origin, origin + RayDirection * currentRayLength);
                }
                else
                {
                    // 不在扇形区域内，绘制黄色射线
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(origin, origin + RayDirection * currentRayLength);
                }
            }

            // 在Scene视图上绘制扇形
            DrawSector(origin, leftDirection, rightDirection, detectionRadius, detectionAngle);
        }

        // 绘制扇形的方法
        private void DrawSector(Vector3 origin, Vector3 leftDirection, Vector3 rightDirection, float radius, float angle)
        {
            Handles.color = Color.red;

            // 使用Handles.DrawWireArc来绘制扇形的圆弧
            Handles.DrawWireArc(origin, Vector3.up, leftDirection, angle, radius);

            // 绘制扇形两侧的连线
            Vector3 leftEdge = origin + leftDirection * radius;
            Vector3 rightEdge = origin + rightDirection * radius;
            Handles.DrawLine(origin, leftEdge);
            Handles.DrawLine(origin, rightEdge);
        }
#endif
    }
}
