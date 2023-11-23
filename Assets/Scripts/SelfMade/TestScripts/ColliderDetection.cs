using UnityEngine;
using StarterAssets;

public class ColliderDetection : MonoBehaviour
{
    public AvatarController AvatarController;
    public LayerMask detectionLayer; // 用于指定特定层级的LayerMask
    public float radius = 5.0f; // 检测半径
    private bool _isObstracted = false;

    void Start()
    {
        // 获取对 AvatarController 实例的引用
        AvatarController = FindObjectOfType<AvatarController>();
        if (AvatarController == null)
        {
            Debug.LogError("No AvatarController found in the scene!");
            return;
        }
    }

    // 绘制场景 Gizmos
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }


    // 在每一帧更新中执行检测逻辑
    private void Update()
    {
        _isObstracted = false;
        //bool isCrouching = AvatarController._isCrouching;
        // 在某个位置创建一个检测球体，检测半径为特定距离
        Vector3 center = transform.position; // 这里使用当前对象的位置作为球体中心

        // 使用OverlapSphere方法来检测周围的物体
        Collider[] hitColliders = Physics.OverlapSphere(center, radius, detectionLayer);

        // 遍历检测到的碰撞体
        foreach (Collider collider in hitColliders)
        {
            _isObstracted = true;
            // 对检测到的物体执行你想要的操作
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