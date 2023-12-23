using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Follower : MonoBehaviour
{
    public Transform targetToFollow;
    public Vector3 sectorDirection = Vector3.forward;
    public float followSpeed = 2f;
    public float stoppingDistance = 0.5f;
    public float radius = 5f;
    public float angle = 90f;
    public float DelayStartTime;
    public float minDistance = 1f; // 公开的最小距离字段
    // 设置加速的阈值距离和加速速度
    public float accelerationDistance = 5.0f; // 你希望加速的距离阈值
    public float accelerationSpeed = 7.0f;    // 你希望达到的加速速度
    private Vector3 currentTargetPoint;
    private Vector3 lastTargetPosition;
    private bool isInsideSector;
    private bool canGeneratePoint;
    private bool hasReachedTargetPoint = true; // 初始设为true，以允许首次目标点的设置

    private float currentSpeed;

    //测试函数

    void Start()
    {
        if (targetToFollow != null)
        {
            lastTargetPosition = targetToFollow.position;
            isInsideSector = true;
            UpdateTargetPoint();
        }
    }
    private IEnumerator MoveAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        MoveTowardsTarget();
    }

    void Update()
    {


        if (targetToFollow != null)
        {
            //Debug.Log(isInsideSector);
            bool hasMoved = Vector3.Distance(targetToFollow.position, lastTargetPosition) > 0.01f;

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
                    if (hasReachedTargetPoint)
                    {
                        // 只有在到达当前目标点后，才更新下一个目标点
                        UpdateTargetPoint();
                        //Debug.LogError("PointSpwan" + currentTargetPoint);
                        hasReachedTargetPoint = false; // 重置标记
                    }
                    canGeneratePoint = false;
                }
            }
            else if (!hasMoved)
            {
                if (!isInsideSector)
                {
                    isInsideSector = false;
                }
                else
                {
                    isInsideSector = true;
                }
                canGeneratePoint = true;

            }

            lastTargetPosition = targetToFollow.position;
            if (!isInsideSector && !IsInvoking("MoveAfterDelay"))
            {
                StartCoroutine(MoveAfterDelay(DelayStartTime)); // x 是延迟时间，以秒为单位
            }

        }
    }

    bool IsInSector(Vector3 position)
    {
        Vector3 worldSectorDirection = targetToFollow.TransformDirection(sectorDirection.normalized);
        Vector3 toPosition = position - targetToFollow.position;
        toPosition.y = 0; // 忽略 Y 轴的变化

        float angleToPosition = Vector3.Angle(worldSectorDirection, toPosition);

        //Debug.Log("Angle to Position: " + angleToPosition + ", Radius: " + toPosition.magnitude);

        return angleToPosition <= angle / 2 && toPosition.magnitude <= radius;
    }




    void UpdateTargetPoint()
    {
        currentTargetPoint = GenerateRandomPointInSector(targetToFollow.position, sectorDirection, radius, angle, minDistance); // 假设 x 是最小距离
        currentTargetPoint = targetToFollow.InverseTransformPoint(currentTargetPoint);
    }

    Vector3 GenerateRandomPointInSector(Vector3 center, Vector3 direction, float radius, float angle, float minDistance)
    {
        Vector3 worldSectorDirection = targetToFollow.TransformDirection(direction.normalized);
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


    void MoveTowardsTarget()
    {
        Vector3 worldTargetPoint = targetToFollow.TransformPoint(currentTargetPoint);
        worldTargetPoint.y = transform.position.y; // 忽略 Y 轴的变化
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
                transform.forward = moveDirection;
            }
        }
        else
        {
            isInsideSector = true;
            hasReachedTargetPoint = true;
            Debug.LogWarning("REACHED TARGET POINT");
        }

    }

    void OnDrawGizmos()
    {
        if (targetToFollow != null)
        {
            Vector3 worldSectorDirection = targetToFollow.TransformDirection(sectorDirection.normalized);

            // 绘制扇形区域的填充
            DrawSector(targetToFollow.position, worldSectorDirection, radius, angle);

            // 可视化目标点
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(targetToFollow.TransformPoint(currentTargetPoint), stoppingDistance);

            // 绘制跟随者到目标点的轨迹
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, targetToFollow.TransformPoint(currentTargetPoint));

            // 绘制表示最小距离到最大距离范围的覆盖面
            DrawSectorRange(targetToFollow.position, worldSectorDirection, minDistance, radius, angle);
        }
    }

#if UNITY_EDITOR
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
}
