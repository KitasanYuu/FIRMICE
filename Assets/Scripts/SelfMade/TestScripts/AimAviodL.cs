using UnityEngine;

namespace AimAvoidedL
{
    public class AimAviodL : MonoBehaviour
    {
        public bool isBlockedL = false;
        [SerializeField] public LayerMask AvoidColliderLayerMask;
        private int collisionCount = 0; // 用于记录特定层级的碰撞体数量

        private void Update()
        {
            isBlockedL = collisionCount > 0; // 如果有碰撞体处于指定层级，isBlocked 为 true，否则为 false
        }

        private void OnCollisionEnter(Collision collision)
        {
            if ((AvoidColliderLayerMask & (1 << collision.gameObject.layer)) != 0) // 检查是否处于指定层级
            {
                collisionCount++; // 进入指定层级的碰撞体数量增加
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if ((AvoidColliderLayerMask & (1 << collision.gameObject.layer)) != 0) // 检查是否处于指定层级
            {
                collisionCount--; // 离开指定层级的碰撞体数量减少
                collisionCount = Mathf.Max(0, collisionCount); // 确保数量不会小于零
            }
        }
    }
}
