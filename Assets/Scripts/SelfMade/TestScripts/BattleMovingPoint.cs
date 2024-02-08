using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

namespace TestField
{
    public class BattleMovingPoint : MonoBehaviour
    {

        public List<Vector3> aimPoint;
        private Vector3 Destination;

        public float FSpeed;
        public float FSprintSpeed;
        public float followSpeed = 2f;
        public float stoppingDistance = 0.5f;
        public float accelerationDistance = 5.0f; // 你希望加速的距离阈值

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

        public void AStarMoving(float FacetoForwordDir = 0, GameObject Target = null)
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

            //Debug.LogWarning(distanceToTarget);

            if (distanceToTarget > stoppingDistance)
            {
                // 计算当前速度，使其逐渐减小直到0
                currentSpeed = followSpeed * (distanceToTarget / stoppingDistance);

                if (distanceToTarget > accelerationDistance)
                {
                    // 如果距离小于加速的阈值距离，使用加速速度
                    currentSpeed = FSprintSpeed;
                }
                else if (currentSpeed > FSpeed)
                {
                    currentSpeed = FSpeed;
                }

                //Debug.Log("Follower #315 CurrentSpeed:" + currentSpeed);

                if (aimPoint != null && aimPoint.Count != 0)
                {
                    // 计算移动方向
                    Vector3 MoveDir = (aimPoint[1] - transform.position).normalized;
                    //Debug.Log(currentSpeed);

                    //移动角色
                    transform.position = Vector3.MoveTowards(transform.position, Destination, currentSpeed * Time.deltaTime);
                    //Debug.Log(Destination);

                    if (FacetoForwordDir == 0)
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
                    else if (FacetoForwordDir == 1)
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

        //特殊移动情况，自定义速度和加速度
        public void CustomAStarMoving(float CustomSpeed, float CustomSprintSpeed, float FacetoForwordDir = 0, GameObject Target = null)
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

            //Debug.LogWarning(distanceToTarget);

            if (distanceToTarget > stoppingDistance)
            {
                // 计算当前速度，使其逐渐减小直到0
                currentSpeed = CustomSpeed * (distanceToTarget / stoppingDistance);

                if (distanceToTarget > accelerationDistance)
                {
                    // 如果距离小于加速的阈值距离，使用加速速度
                    currentSpeed = CustomSprintSpeed;
                }
                else if (currentSpeed > CustomSprintSpeed)
                {
                    currentSpeed = CustomSprintSpeed;
                }


                if (aimPoint != null && aimPoint.Count != 0)
                {
                    // 计算移动方向
                    Vector3 MoveDir = (aimPoint[1] - transform.position).normalized;
                    //Debug.Log(currentSpeed);

                    //移动角色
                    transform.position = Vector3.MoveTowards(transform.position, Destination, currentSpeed * Time.deltaTime);
                    //Debug.Log(Destination);

                    if (FacetoForwordDir == 0)
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
                    else if (FacetoForwordDir == 1)
                    {
                        // 计算目标朝向
                        Quaternion targetRotation = Quaternion.LookRotation((Target.transform.position - transform.position).normalized);

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
