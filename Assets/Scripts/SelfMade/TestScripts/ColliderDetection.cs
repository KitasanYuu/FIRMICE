using UnityEngine;
using StarterAssets;

public class ColliderDetection : MonoBehaviour
{
    public AvatarController AvatarController;
    public LayerMask detectionLayer; // ����ָ���ض��㼶��LayerMask
    public float radius = 5.0f; // ���뾶
    private bool _isObstracted = false;

    void Start()
    {
        // ��ȡ�� AvatarController ʵ��������
        AvatarController = FindObjectOfType<AvatarController>();
        if (AvatarController == null)
        {
            Debug.LogError("No AvatarController found in the scene!");
            return;
        }
    }

    // ���Ƴ��� Gizmos
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }


    // ��ÿһ֡������ִ�м���߼�
    private void Update()
    {
        _isObstracted = false;
        //bool isCrouching = AvatarController._isCrouching;
        // ��ĳ��λ�ô���һ��������壬���뾶Ϊ�ض�����
        Vector3 center = transform.position; // ����ʹ�õ�ǰ�����λ����Ϊ��������

        // ʹ��OverlapSphere�����������Χ������
        Collider[] hitColliders = Physics.OverlapSphere(center, radius, detectionLayer);

        // ������⵽����ײ��
        foreach (Collider collider in hitColliders)
        {
            _isObstracted = true;
            // �Լ�⵽������ִ������Ҫ�Ĳ���
            Debug.Log("Detected object on layer: " + LayerMask.LayerToName(collider.gameObject.layer));
        }

        if(_isObstracted)
        {
            AvatarController._isCrouching = true;
        }else
        {
            AvatarController._isCrouching = false;
        }
    }
}