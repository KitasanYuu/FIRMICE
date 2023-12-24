//这是A*算法寻路的移动基础示例
//默认是使用鼠标点击Game上的地格来实现移动的
//只有Capsule被正确设置了，那个FollowTest是测试Follow脚本的

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Partner;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.GlobalIllumination;

public class AstarMovingTest : MonoBehaviour
{
    Seeker seeker;
    Follower follower;

    public List<Vector3> aimPoint;

    private Vector3 RTargetPosition;
    private Vector3 TargetPosition;

    // Start is called before the first frame update
    void Start()
    {
        seeker = GetComponent<Seeker>();
        //follower = GetComponent<Follower>();

        seeker.pathCallback += OnPathComplete;


    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit hitinfo;
            //这个只对主摄像机生效
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out hitinfo))
            {
                seeker.StartPath(transform.position,hitinfo.point);
            }
        }


        MovingBehavior();

        // 计算总长度
        //float totalLength = CalculateTotalLength(aimPoint);
        //Debug.LogWarning("总长度：" + totalLength);
    }

    private void FixedUpdate()
    {
 
    }

    void OnPathComplete(Path path)
    {
        aimPoint = new List<Vector3>(path.vectorPath);
    }

    //要搬的话这边啥也别改，因为检测的是Seeker上的回调的点
    private void MovingBehavior()
    {
        if (aimPoint != null && aimPoint.Count != 0)
        {
            Vector3 MoveDir = (aimPoint[1] - transform.position).normalized;
            Vector3 Destination = aimPoint[1];

            transform.position = Vector3.MoveTowards(transform.position, Destination, 3 * Time.deltaTime);

            // 使物体朝向移动方向
            if (MoveDir != Vector3.zero)
            {
                // 计算目标朝向
                Quaternion targetRotation = Quaternion.LookRotation(MoveDir);

                // 使用Slerp插值来平滑地转向目标朝向
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5 * Time.deltaTime);
            }

            if (Vector3.Distance(aimPoint[1], transform.position) <= 1f)
            {
                aimPoint.RemoveAt(1);
            }
        }
    }

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