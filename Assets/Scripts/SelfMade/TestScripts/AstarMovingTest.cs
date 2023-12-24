//����A*�㷨Ѱ·���ƶ�����ʾ��
//Ĭ����ʹ�������Game�ϵĵظ���ʵ���ƶ���
//ֻ��Capsule����ȷ�����ˣ��Ǹ�FollowTest�ǲ���Follow�ű���

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
            //���ֻ�����������Ч
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out hitinfo))
            {
                seeker.StartPath(transform.position,hitinfo.point);
            }
        }


        MovingBehavior();

        // �����ܳ���
        //float totalLength = CalculateTotalLength(aimPoint);
        //Debug.LogWarning("�ܳ��ȣ�" + totalLength);
    }

    private void FixedUpdate()
    {
 
    }

    void OnPathComplete(Path path)
    {
        aimPoint = new List<Vector3>(path.vectorPath);
    }

    //Ҫ��Ļ����ɶҲ��ģ���Ϊ������Seeker�ϵĻص��ĵ�
    private void MovingBehavior()
    {
        if (aimPoint != null && aimPoint.Count != 0)
        {
            Vector3 MoveDir = (aimPoint[1] - transform.position).normalized;
            Vector3 Destination = aimPoint[1];

            transform.position = Vector3.MoveTowards(transform.position, Destination, 3 * Time.deltaTime);

            // ʹ���峯���ƶ�����
            if (MoveDir != Vector3.zero)
            {
                // ����Ŀ�곯��
                Quaternion targetRotation = Quaternion.LookRotation(MoveDir);

                // ʹ��Slerp��ֵ��ƽ����ת��Ŀ�곯��
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

        // �������б��������ڵ�֮��ľ��벢���
        for (int i = 0; i < points.Count - 1; i++)
        {
            totalLength += Vector3.Distance(points[i], points[i + 1]);
        }

        return totalLength;
    }
}