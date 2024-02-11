using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using VInspector;
using CustomInspector;

namespace TestField
{
    public class BattleMovingPoint : MonoBehaviour
    {

        public List<Vector3> aimPoint;
        private Vector3 Destination;
 
        public float NormalSpeed = 2f;
        public float SprintSpeed;
        public float AimSpeed;
        public float stoppingDistance = 0.5f;
        public float accelerationDistance = 5.0f; // 你希望加速的距离阈值

        [Foldout("Debug")]
        //MoveMode用于对应三种状态移动速度，0为正常移速，1为瞄准时的速度，2为全速冲刺的速度
        [ReadOnly, SerializeField] private int MoveMode;
        [ReadOnly, SerializeField] private float FacetoForwardDir;
        [ReadOnly, SerializeField] private GameObject Target;


        [HideInInspector]
        public bool HasReachedPoint;
        private Seeker seeker;


        private void Start()
        {
            seeker = GetComponent<Seeker>();
        }

        private void Update()
        {

        }

        public void AStarMoving()
        {
            HasReachedPoint = false;
            //这里用if是因为在抵达目标点一瞬间数组会只有一位数0，此时Vector3 aimPoint[1]会取不到值
            int Count = aimPoint.Count;
            //Debug.LogError(Count);

            if (Count == 1)
            {
                Destination = aimPoint[index: 0];
            }
            else if (Count >= 1)
            {
                Destination = aimPoint[index: 1];
            }
            else if (Count == 0)
            {
                //Debug.LogError("BMP:The List Count is 0!");
                return;
            }
            else
            {
                Debug.LogError("BMP:The List Count Take" + Count);
            }

            float distanceToTarget = CalculateTotalLength(aimPoint);
            float currentSpeed;

            switch (MoveMode)
            {
                case 0:
                    currentSpeed = NormalSpeed;
                    break;
                case 1:
                    currentSpeed = AimSpeed;
                    break;
                case 2:
                    currentSpeed = SprintSpeed;
                    break;
                default:
                    currentSpeed = NormalSpeed;
                    break;
            }
            

            //Debug.LogWarning(distanceToTarget);

            if (distanceToTarget > stoppingDistance)
            {
                if (aimPoint != null && aimPoint.Count != 0)
                {
                    // 计算移动方向
                    Vector3 MoveDir = (aimPoint[1] - transform.position).normalized;
                    //Debug.Log(currentSpeed);

                    //移动角色
                    transform.position = Vector3.MoveTowards(transform.position, Destination, currentSpeed * Time.deltaTime);
                    //Debug.Log(Destination);

                    if (FacetoForwardDir == 0)
                    {
                        // 使物体朝向移动方向
                        if (MoveDir != Vector3.zero)
                        {
                            // 计算目标朝向
                            Quaternion targetRotation = Quaternion.LookRotation(MoveDir);

                            // 使用Slerp插值来平滑地转向目标朝向
                            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5 * Time.deltaTime);
                        }
                    }
                    else if (FacetoForwardDir == 1)
                    {
                        Vector3 TargetPosition = Target.transform.position;
                        TargetPosition.y = 0;
                        Vector3 SelfPosition = transform.position;
                        SelfPosition.y = 0;

                        // 计算目标朝向
                        Quaternion targetRotation = Quaternion.LookRotation((TargetPosition - SelfPosition).normalized);

                        // 使用Slerp插值来平滑地转向目标朝向
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5 * Time.deltaTime);
                    }

                    //如果目标抵达了一个路径节点则将这个节点删去
                    if (Vector3.Distance(aimPoint[1], transform.position) <= 1f)
                    {
                        aimPoint.RemoveAt(1);
                    }
                }
            }
            else
            {
                HasReachedPoint = true;
                currentSpeed = 0;
                aimPoint.Clear();
            }
        }

        public void SetMoveParameter(int movemode, float facetoforwardDir = 0, GameObject target = null)
        {
            MoveMode = movemode;
            FacetoForwardDir = facetoforwardDir;
            Target = target;
        }

        //这个方法用来启动Seeker的路径计算
        public void SeekerCalcu(Vector3 TargetPoint)
        {
            Vector3 TargetPosition = TargetPoint;
            seeker.StartPath(transform.position, TargetPoint);

            seeker.pathCallback += OnPathComplete; //在每次回调成功后把新路径点加在数组后面
        }

        void OnPathComplete(Path path)
        {
            aimPoint = new List<Vector3>(path.vectorPath);
        }

        //一个方法，用来在回调后保存Path数组的值
        float CalculateTotalLength(List<Vector3> points)
        {
            float totalLength = 0f;

            // 遍历点列表，计算相邻点之间的距离并相加
            for (int i = 0; i < points.Count - 1; i++)
            {
                totalLength += Vector3.Distance(points[i], points[i + 1]);
            }

            return totalLength;
        }

    }
}
