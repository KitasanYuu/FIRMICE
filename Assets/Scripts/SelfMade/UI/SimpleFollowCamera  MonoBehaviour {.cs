using UnityEngine;

public class SimpleCameraLookAt : MonoBehaviour
{
    public Transform target; // Ŀ������

    void Update()
    {
        if (target != null)
        {
            // ���㳯��Ŀ������ķ���
            Vector3 lookAtDirection = target.position - transform.position;

            // ������ת�Ƕ�
            Quaternion rotation = Quaternion.LookRotation(lookAtDirection, Vector3.up);

            // Ӧ����ת�����
            transform.rotation = rotation;
        }
    }
}
