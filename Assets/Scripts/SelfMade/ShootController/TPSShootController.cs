using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using StarterAssets;
using UnityEngine.InputSystem;

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

    // 角色控制器和输入
    private AvatarController avatarController;
    private StarterAssetsInputs starterAssetsInputs;

    private void Awake()
    {
        // 获取角色控制器和输入
        avatarController = GetComponent<AvatarController>();
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
    }

    private void Update()
    {
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
}
