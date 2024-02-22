using UnityEngine;
using System.Collections;
using Pathfinding;
using System.Collections.Generic;
using AvatarMain;
using TargetFinding;
using VInspector;
using TestField;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Partner
{
    public class AIFollower : AIMove
    {
        public GameObject targetToFollow;
        [Foldout("PointGenerateParameter")]
        public Vector3 sectorDirection = Vector3.forward;
        public float radius = 5f;
        public float angle = 90f;
        public float minDistance = 1f; // 在目标距离内不会生成点

        [Foldout("MoveParameter")]
        public bool CopyTargetParameter;
        public float DelayStartTime;    //延迟x秒后开始移动
        public float followSpeed = 2f;
        public float stoppingDistance = 0.5f;
        // 设置加速的阈值距离和加速速度
        public float accelerationDistance = 5.0f; // 你希望加速的距离阈值
        public float accelerationSpeed = 7.0f;    // 你希望达到的加速速度
        public float rotationSpeed = 5.0f;

        private Vector3 currentTargetPoint;
        private Vector3 lastTargetPosition;
        private bool isInsideSector;
        private bool canGeneratePoint;

        //用来判定是否移动保底
        private bool IsMoving;
        private Vector3 lastPosition;
        private float movementThreshold = 0.01f; // 可以调整这个阈值

        //这里为了在两种跟随模式下切换设定几个中立的速度变量
        private float FSpeed;
        private float FSprintSpeed;

        //直接取到AvatarController中的值
        private AvatarController avatarController;

        private bool NeedtoRetakeTarget;

        //以下是Astar算法所用的参数
        private Seeker seeker;

        //这些是之前用NavMesh导航时所用的参数，现在使用Astar算法后弃用
        //private NavMeshAgent navMeshAgent;
        //private NavMeshPath navMeshPath;

        //这里是仅用作外部调取的函数
        [HideInInspector] public Vector3 MoveDirection;//用于外部获取角色方向
        [HideInInspector] public Vector3 CTargetPosition;//用于外部获取目标点
        [HideInInspector] public float CSpeed;
        [HideInInspector] private float currentSpeed;

        //测试函数

        void Start()
        {
            ComponentInit();
            FollowTargetInit();

            lastPosition = transform.position;

        }

        void Update()
        {
            if (NeedtoRetakeTarget)
            {
                RetakeTarget();
            }

            Move();
        }

        #region 移动调用的方法
        private void Move()
        {
            if(targetToFollow!= null)
            {
                MoveCalcu();
                MovingStart();
                MovingAnimProtect();
            }
        }

        //用来判定启动协程计时执行Move的函数
        private void MovingStart()
        {
            if (targetToFollow != null && !isInsideSector && !IsInvoking("MoveAfterDelay"))
            {
                StartCoroutine(MoveAfterDelay(DelayStartTime)); // x 是延迟时间，以秒为单位
            }
        }

        //对移动使用数据的运算
        private void MoveCalcu()
        {
            CanISpwanTargetPoint();

            Vector3 TargetPoint = targetToFollow.transform.TransformPoint(currentTargetPoint);
            SeekerCalcu(TargetPoint);

            SpeedJudging();

            float distanceToTarget = CalculateTotalLength(RoutePoint);
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

            if (HasReachedPoint)
            {
                currentSpeed = 0;
                isInsideSector = true;
            }

            CSpeed = currentSpeed;
            SetPMoveParameter(currentSpeed);

        }

        protected void SpeedJudging()
        {
            if (avatarController != null && CopyTargetParameter)
            {
                FSpeed = avatarController.MoveSpeed;
                FSprintSpeed = avatarController.SprintSpeed - 1.0f;
            }
            else
            {
                FSpeed = followSpeed;
                FSprintSpeed = accelerationSpeed;
            }
        }

        //协程计时x秒后执行移动操作
        private IEnumerator MoveAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            //下面是移动方法的调用
            PAStarMoving();

            //MoveTowardsTarget();

            //navMeshAgent.SetDestination(targetToFollow.TransformPoint(currentTargetPoint));
            //Debug.Log(navMeshPath.status);
            //hasReachedTargetPoint = true;
        }

        private void MovingAnimProtect()
        {
            // 检查角色位置是否发生了显著变化
            if (Vector3.Distance(transform.position, lastPosition) > movementThreshold)
            {
                IsMoving = true;
            }
            else
            {
                IsMoving = false;
            }

            //Debug.Log("Follower Moving:" + IsMoving);
            // 更新最后的位置
            lastPosition = transform.position;

            if (IsMoving)
            {
                return;
            }
            else
            {
                CSpeed = 0;
            }
        }

        #endregion

        #region Follow移动点生成
        //这个是用来判定跟随者是否已经进入了会刷新随机目标点的自定义范围内
        protected bool IsInSector(Vector3 position)
        {
            Vector3 worldSectorDirection = targetToFollow.transform.TransformDirection(sectorDirection.normalized);
            Vector3 toPosition = position - targetToFollow.transform.position;

            toPosition.y = 0; // 忽略 Y 轴的变化

            float angleToPosition = Vector3.Angle(worldSectorDirection, toPosition);

            //Debug.Log("Angle to Position: " + angleToPosition + ", Radius: " + toPosition.magnitude);

            return angleToPosition <= angle / 2 && toPosition.magnitude <= radius;
        }

        //下面是用来判定是否应该执行生成随机目标点的判定逻辑
        protected void CanISpwanTargetPoint()
        {
            if (targetToFollow != null)
            {
                //Debug.Log(isInsideSector);
                bool hasMoved = Vector3.Distance(targetToFollow.transform.position, lastTargetPosition) > 0.01f;
                //Debug.Log("正在移动"+hasMoved);
                if (hasMoved)
                {
                    // 如果跟随者不在扇形范围内，或者刚刚进入扇形范围，才更新目标点
                    if (IsInSector(transform.position))
                    {
                        isInsideSector = true;
                    }
                    else
                    {
                        // 当目标停止移动时，重新检查是否在扇形内
                        isInsideSector = IsInSector(transform.position);
                    }
                    if (canGeneratePoint && !isInsideSector)
                    {
                        //Debug.Log("Allow to Spwan Point");
                        if (HasReachedPoint)
                        {
                            // 只有在到达当前目标点后，才更新下一个目标点
                            UpdateTargetPoint();
                            //Debug.LogError("PointSpwan" + currentTargetPoint);
                            HasReachedPoint = false; // 重置标记
                        }
                        canGeneratePoint = false;
                    }
                }
                else if (!hasMoved)
                {
                    if (!IsInSector(transform.position))
                    {
                        isInsideSector = false;
                    }
                    else
                    {
                        isInsideSector = true;
                    }
                    canGeneratePoint = true;
                }
                lastTargetPosition = targetToFollow.transform.position;
            }
        }

        //这是用来刷新目标点时调用的函数
        protected void UpdateTargetPoint()
        {
            currentTargetPoint = GenerateRandomPointInSector(targetToFollow.transform.position, sectorDirection, radius, angle, minDistance); // 假设 x 是最小距离
            currentTargetPoint = targetToFollow.transform.InverseTransformPoint(currentTargetPoint);
            CTargetPosition = currentTargetPoint;//给外部读取的目标点值
        }

        //这是随机生成目标点的函数
        protected Vector3 GenerateRandomPointInSector(Vector3 center, Vector3 direction, float radius, float angle, float minDistance)
        {
            Vector3 worldSectorDirection = targetToFollow.transform.TransformDirection(direction.normalized);
            Vector3 randomPoint;

            do
            {
                float randomAngle = Random.Range(-angle / 2, angle / 2);
                Vector3 randomDirection = Quaternion.Euler(0, randomAngle, 0) * worldSectorDirection;
                randomPoint = center + randomDirection * Random.Range(minDistance, radius);
            }
            while (Vector3.Distance(center, randomPoint) < minDistance);

            return randomPoint;
        }

        #endregion

        #region 组件初始化&重新获取
        //用于在Start方法中获取当前物体上的组件
        private void ComponentInit()
        {
            //navMeshAgent = GetComponent<NavMeshAgent>();
            //navMeshPath = new NavMeshPath();

            seeker = GetComponent<Seeker>();
            SetSeekerComponent(seeker);
            if (targetToFollow != null)
            {
                avatarController = targetToFollow.GetComponent<AvatarController>();
                Debug.Log("Follower - AvatarController Initialized!");
            }
            else
            {
                NeedtoRetakeTarget = true;
            }

        }


        //用于在Star方法中初始化随机跟随目标的生成
        private void FollowTargetInit()
        {
            if (targetToFollow != null)
            {
                lastTargetPosition = targetToFollow.transform.position;
                //isInsideSector = true;
                UpdateTargetPoint();
            }
        }

        private void RetakeTarget()
        {
            ObjectSeeker objectseeker = GetComponent<ObjectSeeker>();
            objectseeker.StartSeek(true);
            if (objectseeker.targetToFollow != null)
            {
                targetToFollow = objectseeker.targetToFollow;
                avatarController = targetToFollow.GetComponent<AvatarController>();
                Debug.Log("FollowerRebinded");
                ComponentInit();
                FollowTargetInit();
                NeedtoRetakeTarget = false;
            }
        }
        #endregion

        #region Gizmos绘制
#if UNITY_EDITOR
        //下面是Gizmos上的绘制，仅在编辑视角生效
        void OnDrawGizmos()
        {
            //这个是使用NavMesh时所用的绘制代码
            //if (navMeshPath != null && navMeshPath.corners.Length > 1)
            //{
            //    Gizmos.color = Color.blue;
            //    for (int i = 0; i < navMeshPath.corners.Length - 1; i++)
            //    {
            //        Gizmos.DrawLine(navMeshPath.corners[i], navMeshPath.corners[i + 1]);
            //    }
            //}

            if (targetToFollow != null)
            {
                Vector3 worldSectorDirection = targetToFollow.transform.TransformDirection(sectorDirection.normalized);

                // 绘制扇形区域的填充
                DrawSector(targetToFollow.transform.position, worldSectorDirection, radius, angle);

                // 可视化目标点
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(targetToFollow.transform.TransformPoint(currentTargetPoint), stoppingDistance);

                // 绘制跟随者到目标点的轨迹
                Gizmos.color = Color.white;
                Gizmos.DrawLine(transform.position, targetToFollow.transform.TransformPoint(currentTargetPoint));

                // 绘制表示最小距离到最大距离范围的覆盖面
                DrawSectorRange(targetToFollow.transform.position, worldSectorDirection, minDistance, radius, angle);
            }
        }

        // Helper method to draw the sector
        void DrawSector(Vector3 center, Vector3 direction, float radius, float angle)
        {
            Vector3 startVector = Quaternion.Euler(0, -angle / 2, 0) * direction * radius;
            Vector3 endVector = Quaternion.Euler(0, angle / 2, 0) * direction * radius;
            Handles.color = Color.red;
            Handles.DrawAAPolyLine(5f, center, center + startVector);
            Handles.DrawAAPolyLine(5f, center, center + endVector);

            Handles.color = new Color(0.2f, 0.3f, 1f, 0.2f);
            Handles.DrawSolidArc(center, Vector3.up, startVector, angle, radius);
        }

#if UNITY_EDITOR
        // 绘制表示可随机到范围的覆盖面
        void DrawSectorRange(Vector3 center, Vector3 direction, float minRange, float maxRange, float angle)
        {
            Vector3 startVectorMin = Quaternion.Euler(0, -angle / 2, 0) * direction * minRange;
            Vector3 endVectorMin = Quaternion.Euler(0, angle / 2, 0) * direction * minRange;
            Vector3 startVectorMax = Quaternion.Euler(0, -angle / 2, 0) * direction * maxRange;
            Vector3 endVectorMax = Quaternion.Euler(0, angle / 2, 0) * direction * maxRange;

            Handles.color = new Color(1f, 0f, 1f, 0.4f); // 设置为紫色
            Handles.DrawWireArc(center, Vector3.up, startVectorMin, angle, minRange);
            Handles.DrawWireArc(center, Vector3.up, startVectorMax, angle, maxRange);
            Handles.DrawAAPolyLine(5f, center + startVectorMin, center + startVectorMax);
            Handles.DrawAAPolyLine(5f, center + endVectorMin, center + endVectorMax);
        }
#endif

#endif
        #endregion

        #region 已弃用移动方法
        //正式的Move移动函数，只能和A*用一个，放弃维护了xx
        void MoveTowardsTarget()
        {
            Vector3 worldTargetPoint = targetToFollow.transform.TransformPoint(currentTargetPoint);

            //角色的Container的Y轴本来就应该处在0，如果强行把Y轴变为0在用Rotation转向时会出错
            //worldTargetPoint.y = transform.position.y; // 忽略 Y 轴的变化

            float distanceToTarget = Vector3.Distance(transform.position, worldTargetPoint);
            float currentSpeed;

            if (distanceToTarget > stoppingDistance)
            {
                // 计算当前速度，使其逐渐减小直到0
                currentSpeed = followSpeed * (distanceToTarget / stoppingDistance);

                if (distanceToTarget > accelerationDistance)
                {
                    // 如果距离小于加速的阈值距离，使用加速速度
                    currentSpeed = accelerationSpeed;
                }
                else if (currentSpeed > followSpeed)
                {
                    currentSpeed = followSpeed;
                }

                // 计算移动方向
                Vector3 moveDirection = (worldTargetPoint - transform.position).normalized;

                // 使用计算的速度移动物体
                transform.position = Vector3.MoveTowards(transform.position, worldTargetPoint, currentSpeed * Time.deltaTime);

                // 使物体朝向移动方向
                if (moveDirection != Vector3.zero)
                {
                    // 计算目标朝向
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

                    // 使用Slerp插值来平滑地转向目标朝向
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                    MoveDirection = moveDirection;
                }
            }
        }
        #endregion
    }
}
