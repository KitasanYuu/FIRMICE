using UnityEngine;
using CustomInspector;

namespace TestField
{
    public class TargetSearchArea : MonoBehaviour
    {
        public string boxIdentifier = "SearchArea";
        [ReadOnly] public GameObject TargetFound;
        // 使用 LayerMask 进行层级检测
        public LayerMask detectionLayerMask;
        public string detectionTag = "DefaultTag";

        public Vector3 boxSize = new Vector3(1f, 1f, 1f);

        // 偏移量
        public Vector3 boxOffset = Vector3.zero;

        void Update()
        {
            Searching();
        }

        private void Searching()
        {
            // 清空 TargetFound
            TargetFound = null;

            // 获取盒子范围，考虑GameObject的位置和偏移量
            Bounds boxBounds = new Bounds(transform.position + boxOffset, boxSize);

            // 打印盒子范围的信息
            Debug.Log($"Box Center: {boxBounds.center}, Extents: {boxBounds.extents}");

            // 检测是否有物体与盒子相交，使用 LayerMask
            Collider[] colliders = Physics.OverlapBox(boxBounds.center, boxBounds.extents, Quaternion.identity, detectionLayerMask);

            // 处理相交的物体
            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag(detectionTag))
                {
                    TargetFound = collider.gameObject;

                    // 找到一个符合条件的物体后退出循环
                    break;
                }
            }
        }

#if UNITY_EDITOR
        // 在Scene视图中绘制Gizmos
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;

            // 绘制框，考虑GameObject的位置和偏移量
            Gizmos.DrawWireCube(transform.position + boxOffset, boxSize);
        }
#endif
    }
}
