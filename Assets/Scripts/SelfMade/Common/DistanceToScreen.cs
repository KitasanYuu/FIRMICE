using CustomInspector;
using UnityEngine;

namespace CameraTools
{

    public class DistanceToScreen : MonoBehaviour
    {
        // 目标物体，通常是玩家角色或摄像机
        public Transform targetObject;
        
        [ReadOnly]public float DistanceToTarget;

        void Update()
        {
            if (targetObject == null)
            {
                Debug.LogWarning("请指定目标物体（玩家角色或摄像机）！");
                return;
            }

            // 获取物体在世界空间中的位置
            Vector3 objectPosition = transform.position;

            // 获取目标物体在世界空间中的位置
            Vector3 targetPosition = targetObject.position;

            // 忽略Y轴坐标
            //objectPosition.y = 0;
            //targetPosition.y = 0;

            // 计算水平平面上的距离
            float distanceToTarget = Vector3.Distance(objectPosition, targetPosition);
            DistanceToTarget = distanceToTarget;

            //// 输出距离信息
            //Debug.Log("物体与目标物体的水平距离：" + distanceToTarget);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, targetObject.position);
        }
    }
}
