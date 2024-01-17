using UnityEngine;
using CustomInspector;

namespace TargetFinding
{
    public class ObjectSeeker : MonoBehaviour
    {
        public Vector3 DetecCenter;
        public Vector3 DetecRange;
        public LayerMask targetLayer;
        [Tag]public string targetTag = "Player";
        //[HideInInspector]
        [ReadOnly]public GameObject targetToFollow; // 用于存储找到的目标物体

        private void Update()
        {
            DetectAndSetTarget();
        }

        private void DetectAndSetTarget()
        {
            if (targetToFollow != null)
            {
                // 已经有目标，无需再检测，直接返回
                enabled = false;
                return;
            }

            // 自定义盒子的中心位置
            Vector3 boxCenter = transform.position + DetecCenter; // 例如，在当前位置上偏移1个单位的X轴
                                                                  // 自定义X、Y、Z轴上的半径
            Vector3 halfExtents = DetecRange;

            // 在当前物体位置以半扩展尺寸为halfExtents的盒子范围内进行检测
            Collider[] colliders = Physics.OverlapBox(boxCenter, halfExtents, Quaternion.identity, targetLayer);

            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag(targetTag))
                {
                    targetToFollow = collider.gameObject;
                    return; // 一旦找到目标，立即返回
                }
            }
        }

        public void StartSeek(bool StartStatus)
        {
            enabled = StartStatus;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;

            // 在Scene视图中绘制盒子，以显示检测范围
            Vector3 halfExtents = DetecRange; // 根据实际设置的半径来定义
            Vector3 boxCenter = transform.position + DetecCenter;
            Gizmos.DrawWireCube(boxCenter, halfExtents * 2); // 注意：* 2是因为Gizmos.DrawWireCube使用的是半扩展尺寸
        }
#endif
    }
}
