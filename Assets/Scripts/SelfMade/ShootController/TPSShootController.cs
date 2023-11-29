using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using StarterAssets;
using UnityEngine.InputSystem;
using AimAvoidedL;
using AimAvoidedR;

public class TPSShootController : MonoBehaviour
{
    // 用于瞄准的虚拟相机
    [SerializeField] private CinemachineVirtualCamera aimVirtualCamera;
    // 普通灵敏度和瞄准灵敏度
    [SerializeField] private float normalSensitivity;
    [SerializeField] private float aimSensitivity;
    // 瞄准时射线检测的LayerMask
    [SerializeField] private LayerMask aimColliderLayerMask = new LayerMask();
    // 用于调试显示的Transform
    [SerializeField] private Transform debugTransform;
    // 子弹预制件或游戏对象
    [SerializeField] private GameObject bulletPrefab;
    // 子弹生成位置和速度
    [SerializeField] private Transform spawnBulletPosition;
    [SerializeField] public float bulletspeed;

    public bool isAiming = false;

    public float cameraside;
    public float transitionSpeed = 0.5f; // 调整过渡速度的值
    public bool isChangingSide = false;

    // 角色控制器和输入
    private AimAviodL aimaviodl;
    private AimAviodR aimaviodr;
    private AvatarController avatarController;
    private StarterAssetsInputs starterAssetsInputs;

    private void Awake()
    {
        // 获取角色控制器和输入
        avatarController = GetComponent<AvatarController>();
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
    }
    private void Start()
    {
        aimaviodl = FindObjectOfType<AimAviodL>(); // 尝试通过FindObjectOfType获取对象引用
        aimaviodr = FindObjectOfType<AimAviodR>(); // 尝试通过FindObjectOfType获取对象引用

    }


    private void Update()
    {
        Debug.LogError(cameraside);
        Vector3 mouseWorldPosition = Vector3.zero;

        // 获取鼠标在世界空间中的位置
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
        {
            debugTransform.position = raycastHit.point;
            mouseWorldPosition = raycastHit.point;
        }

        // 瞄准
        if (starterAssetsInputs.aim)
        {
            isAiming = true;
            aimVirtualCamera.Priority = 20;
            avatarController.SetSensitivity(aimSensitivity);
            avatarController.SetRotateOnMove(false);
            ShootSiteChange();

            Vector3 worldAimTarget = mouseWorldPosition;
            worldAimTarget.y = transform.position.y;
            Vector3 aimDirection = (worldAimTarget - transform.position).normalized;

            transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);
        }
        else
        {
            // 取消瞄准
            isAiming = false;
            aimVirtualCamera.Priority = 5;
            avatarController.SetSensitivity(normalSensitivity);
            avatarController.SetRotateOnMove(newRorareOnMove: true);
        }

        // 开火
        if (starterAssetsInputs.shoot && isAiming)
        {
            Vector3 aimDir = (mouseWorldPosition - spawnBulletPosition.position).normalized;
            // 生成子弹实例
            GameObject bulletInstance = Instantiate(bulletPrefab, spawnBulletPosition.position, Quaternion.LookRotation(aimDir, Vector3.up));
            // 获取子弹脚本并设置速度
            BulletTest bullettest = bulletInstance.GetComponent<BulletTest>();
            if (bullettest != null)
            {
                bullettest.SetBulletSpeed(bulletspeed);
            }
            else
            {
                Debug.LogError("BulletTest component not found on instantiated object.");
            }
            starterAssetsInputs.shoot = false;
        }
    }

    private void ShootSiteChange()
    {
        Cinemachine3rdPersonFollow thirdPersonFollow = aimVirtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();

        if (thirdPersonFollow != null)
        {
            float targetCameraSide = cameraside; // 默认值

            // 根据条件调整 targetCameraSide 值
            if (aimaviodl.isBlockedL)
            {
                targetCameraSide = 1;

            }
            if (aimaviodr.isBlockedR)
            {
                targetCameraSide = 0;
            }


            // 射线检测防止穿墙
            RaycastHit hit;
            Vector3 cameraPosition = thirdPersonFollow.VirtualCamera.State.FinalPosition; // 获取摄像机位置
            Vector3 targetPosition = CalculateTargetPosition(targetCameraSide); // 计算目标位置

            // 发射射线检查摄像机与目标位置之间是否有障碍物
            if (Physics.Linecast(cameraPosition, targetPosition, out hit))
            {
                // 平滑地过渡 CameraSide 的值
                thirdPersonFollow.CameraSide = Mathf.Lerp(thirdPersonFollow.CameraSide, targetCameraSide, transitionSpeed * Time.deltaTime);
            }
            else
            {
                 // 如果射线与障碍物相交，则不改变 CameraSide 的值
                Debug.Log("Camera hit something, can't move there!");
            }
        }
    }
    // 根据 CameraSide 计算目标位置
private Vector3 CalculateTargetPosition(float cameraSide)
{
    // 这里根据 CameraSide 的值计算目标位置，返回一个 Vector3
    // 请根据你的逻辑实现计算目标位置的方法
    // 这个方法需要根据摄像机和目标位置的相对位置来返回一个 Vector3 类型的目标位置
    // 例如：根据摄像机当前位置和方向，加上 CameraSide 偏移量来计算目标位置
    return Vector3.zero;
}
}
