using UnityEngine;
using System.Collections.Generic;

namespace BattleBullet
{
    public class HitCollisionDetection : MonoBehaviour
    {
        private BoxCollider boxCollider;
        public LayerMask targetLayer;
        public bool reverseXAxis = false;
        public bool reverseYAxis = false;
        public float rayLength = 1.0f; // 射线长度
        public Dictionary<CollisionSide, Vector3> directionToVector; // 方向和射线向量对应关系

        public int HitDir;
        public int resetDelayFrames = 3; // 延迟帧数

        private int resetCounter = 0;

        private void Awake()
        {
            boxCollider = GetComponent<BoxCollider>();

            if (boxCollider == null)
            {
                Debug.LogError("没有找到 Box Collider 组件！");
            }

            // 初始化方向和射线向量对应关系
            directionToVector = new Dictionary<CollisionSide, Vector3>
            {
                { CollisionSide.Front, Vector3.forward },
                { CollisionSide.Back, Vector3.back },
                { CollisionSide.Left, Vector3.left },
                { CollisionSide.Right, Vector3.right }
            };
        }

        private void Update()
        {
            if (boxCollider != null)
            {
                bool collisionDetected = false; // 是否检测到碰撞

                // 检测碰撞
                foreach (CollisionSide side in System.Enum.GetValues(typeof(CollisionSide)))
                {
                    if (CheckCollision(side))
                    {
                        // 使用 HitDir 来记录碰撞方向
                        switch (side)
                        {
                            case CollisionSide.Front:
                                HitDir = 1;
                                Debug.Log("前碰撞");
                                break;
                            case CollisionSide.Back:
                                HitDir = -1;
                                Debug.Log("后碰撞");
                                break;
                            case CollisionSide.Left:
                                HitDir = -2;
                                Debug.Log("左碰撞");
                                break;
                            case CollisionSide.Right:
                                HitDir = 2;
                                Debug.Log("右碰撞");
                                break;
                        }

                        collisionDetected = true;

                        // 重置计数器
                        resetCounter = 0;

                        // 可以在这里计算碰撞点坐标并用于生成特效等
                    }
                }

                // 如果没有检测到碰撞，逐渐减小 HitDir 的值
                if (!collisionDetected)
                {
                    resetCounter++;
                    if (resetCounter >= resetDelayFrames)
                    {
                        HitDir = 0; // 在计数器达到指定帧数后才清零
                    }
                }
            }
        }

        private bool CheckCollision(CollisionSide side)
        {
            Vector3 center = boxCollider.bounds.center;
            Vector3 extents = boxCollider.bounds.extents;

            // 根据反转布尔变量决定方向
            float xDirection = reverseXAxis ? -1f : 1f;
            float yDirection = reverseYAxis ? -1f : 1f;

            Vector3 rayDirection = directionToVector[side] * rayLength;

            bool collision = Physics.CheckBox(center + rayDirection * xDirection, extents, Quaternion.identity, targetLayer);
            return collision;
        }

        public enum CollisionSide
        {
            Front,
            Back,
            Left,
            Right
        }
    }
}
