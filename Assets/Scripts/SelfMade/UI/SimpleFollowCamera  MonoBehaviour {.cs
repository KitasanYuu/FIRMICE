using UnityEngine;

public class SimpleCameraLookAt : MonoBehaviour
{
    public Transform target; // 目标物体

    void Update()
    {
        if (target != null)
        {
            // 计算朝向目标物体的方向
            Vector3 lookAtDirection = target.position - transform.position;

            // 计算旋转角度
            Quaternion rotation = Quaternion.LookRotation(lookAtDirection, Vector3.up);

            // 应用旋转到相机
            transform.rotation = rotation;
        }
    }
}
