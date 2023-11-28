using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using StarterAssets;
using UnityEngine.InputSystem;

public class TPSShootController : MonoBehaviour
{
    // ������׼���������
    [SerializeField] private CinemachineVirtualCamera aimVirtualCamera;
    // ��ͨ�����Ⱥ���׼������
    [SerializeField] private float normalSensitivity;
    [SerializeField] private float aimSensitivity;
    // ��׼ʱ���߼���LayerMask
    [SerializeField] private LayerMask aimColliderLayerMask = new LayerMask();
    // ���ڵ�����ʾ��Transform
    [SerializeField] private Transform debugTransform;
    // �ӵ�Ԥ�Ƽ�����Ϸ����
    [SerializeField] private GameObject bulletPrefab;
    // �ӵ�����λ�ú��ٶ�
    [SerializeField] private Transform spawnBulletPosition;
    [SerializeField] public float bulletspeed;

    public bool isAiming = false;

    // ��ɫ������������
    private AvatarController avatarController;
    private StarterAssetsInputs starterAssetsInputs;

    private void Awake()
    {
        // ��ȡ��ɫ������������
        avatarController = GetComponent<AvatarController>();
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
    }

    private void Update()
    {
        Vector3 mouseWorldPosition = Vector3.zero;

        // ��ȡ���������ռ��е�λ��
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
        {
            debugTransform.position = raycastHit.point;
            mouseWorldPosition = raycastHit.point;
        }

        // ��׼
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
            // ȡ����׼
            isAiming = false;
            aimVirtualCamera.Priority = 5;
            avatarController.SetSensitivity(normalSensitivity);
            avatarController.SetRotateOnMove(newRorareOnMove: true);
        }

        // ����
        if (starterAssetsInputs.shoot && isAiming)
        {
            Vector3 aimDir = (mouseWorldPosition - spawnBulletPosition.position).normalized;
            // �����ӵ�ʵ��
            GameObject bulletInstance = Instantiate(bulletPrefab, spawnBulletPosition.position, Quaternion.LookRotation(aimDir, Vector3.up));
            // ��ȡ�ӵ��ű��������ٶ�
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
