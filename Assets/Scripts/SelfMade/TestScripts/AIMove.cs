using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using VInspector;
using CustomInspector;

namespace TestField
{
    public class AIMove : MonoBehaviour
    {

        public List<Vector3> RoutePoint;
        private Vector3 Destination;
 
        private float Normalspeed;
        private float Sprintspeed;
        private float Aimspeed;
        private float stoppingdistance;

        //MoveMode用于对应三种状态移动速度，0为正常移速，1为瞄准时的速度，2为全速冲刺的速度
        private int movemode;
        private float FacetoForwardDir;
        private GameObject target;


        [HideInInspector]
        public bool HasReachedPoint;
        private Seeker SEEKER;


        private void Start()
        {
        }

        private void Update()
        {

        }

        public void AStarMoving()
        {
            HasReachedPoint = false;
            //这里用if是因为在抵达目标点一瞬间数组会只有一位数0，此时Vector3 aimPoint[1]会取不到值
            int Count = RoutePoint.Count;
            //Debug.LogError(Count);

            if (Count == 1)
            {
                Destination = RoutePoint[index: 0];
            }
            else if (Count >= 1)
            {
                Destination = RoutePoint[index: 1];
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

            float distanceToTarget = CalculateTotalLength(RoutePoint);
            float currentSpeed;

            switch (movemode)
            {
                case 0:
                    currentSpeed = Normalspeed;
                    break;
                case 1:
                    currentSpeed = Aimspeed;
                    break;
                case 2:
                    currentSpeed = Sprintspeed;
                    break;
                default:
                    currentSpeed = Normalspeed;
                    break;
            }
            

            //Debug.LogWarning(distanceToTarget);

            if (distanceToTarget > stoppingdistance)
            {
                if (RoutePoint != null && RoutePoint.Count != 0)
                {
                    // 计算移动方向
                    Vector3 MoveDir = (RoutePoint[1] - transform.position).normalized;
                    //Debug.Log(currentSpeed);

                    //移动角色
                    transform.position = Vector3.MoveTowards(transform.position, Destination, currentSpeed * Time.deltaTime);
                    //Debug.Log("!");
                    Debug.Log(Destination);

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
                        Vector3 TargetPosition = target.transform.position;
                        TargetPosition.y = 0;
                        Vector3 SelfPosition =transform.position;
                        SelfPosition.y = 0;

                        // 计算目标朝向
                        Quaternion targetRotation = Quaternion.LookRotation((TargetPosition - SelfPosition).normalized);

                        // 使用Slerp插值来平滑地转向目标朝向
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5 * Time.deltaTime);
                    }

                    //如果目标抵达了一个路径节点则将这个节点删去
                    if (Vector3.Distance(RoutePoint[1],transform.position) <= 1f)
                    {
                        RoutePoint.RemoveAt(1);
                    }
                }
            }
            else
            {
                HasReachedPoint = true;
                currentSpeed = 0;
                RoutePoint.Clear();
            }
        }

        public void SetMoveParameter(int movemode, float facetoforwardDir = 0, GameObject target = null)
        {
            this.movemode = movemode;
            FacetoForwardDir = facetoforwardDir;
            this.target = target;

        }

        public void SetMoveBasicParameter(float normalspeed, float aimspeed, float sprintspeed, float stopdistance)
        {
            Normalspeed = normalspeed;
            Aimspeed = aimspeed;
            Sprintspeed = sprintspeed;
            stoppingdistance = stopdistance;
        }

        //这个方法用来启动Seeker的路径计算
        public void SeekerCalcu(Vector3 TargetPoint)
        {
            Vector3 TargetPosition = TargetPoint;
            SEEKER.StartPath(transform.position, TargetPoint);

            SEEKER.pathCallback += OnPathComplete; //在每次回调成功后把新路径点加在数组后面
        }

        void OnPathComplete(Path path)
        {
            RoutePoint = new List<Vector3>(path.vectorPath);
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

        // 接受Seeker组件的方法
        public void SetSeekerComponent(Seeker seekerComponent)
        {
            SEEKER = seekerComponent; // 将传入的Seeker组件引用保存在类的成员变量中
        }
    }
}