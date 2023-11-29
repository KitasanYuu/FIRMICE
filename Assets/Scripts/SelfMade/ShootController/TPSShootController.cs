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

    public float cameraside;
    public float transitionSpeed = 0.5f; // ���������ٶȵ�ֵ
    public bool isChangingSide = false;

    // ��ɫ������������
    private AimAviodL aimaviodl;
    private AimAviodR aimaviodr;
    private AvatarController avatarController;
    private StarterAssetsInputs starterAssetsInputs;

    private void Awake()
    {
        // ��ȡ��ɫ������������
        avatarController = GetComponent<AvatarController>();
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
    }
    private void Start()
    {
        aimaviodl = FindObjectOfType<AimAviodL>(); // ����ͨ��FindObjectOfType��ȡ��������
        aimaviodr = FindObjectOfType<AimAviodR>(); // ����ͨ��FindObjectOfType��ȡ��������

    }


    private void Update()
    {
        Debug.LogError(cameraside);
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
            ShootSiteChange();

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

    private void ShootSiteChange()
    {
        Cinemachine3rdPersonFollow thirdPersonFollow = aimVirtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();

        if (thirdPersonFollow != null)
        {
            float targetCameraSide = cameraside; // Ĭ��ֵ

            // ������������ targetCameraSide ֵ
            if (aimaviodl.isBlockedL)
            {
                targetCameraSide = 1;

            }
            if (aimaviodr.isBlockedR)
            {
                targetCameraSide = 0;
            }


            // ���߼���ֹ��ǽ
            RaycastHit hit;
            Vector3 cameraPosition = thirdPersonFollow.VirtualCamera.State.FinalPosition; // ��ȡ�����λ��
            Vector3 targetPosition = CalculateTargetPosition(targetCameraSide); // ����Ŀ��λ��

            // �������߼���������Ŀ��λ��֮���Ƿ����ϰ���
            if (Physics.Linecast(cameraPosition, targetPosition, out hit))
            {
                // ƽ���ع��� CameraSide ��ֵ
                thirdPersonFollow.CameraSide = Mathf.Lerp(thirdPersonFollow.CameraSide, targetCameraSide, transitionSpeed * Time.deltaTime);
            }
            else
            {
                 // ����������ϰ����ཻ���򲻸ı� CameraSide ��ֵ
                Debug.Log("Camera hit something, can't move there!");
            }
        }
    }
    // ���� CameraSide ����Ŀ��λ��
private Vector3 CalculateTargetPosition(float cameraSide)
{
    // ������� CameraSide ��ֵ����Ŀ��λ�ã�����һ�� Vector3
    // ���������߼�ʵ�ּ���Ŀ��λ�õķ���
    // ���������Ҫ�����������Ŀ��λ�õ����λ��������һ�� Vector3 ���͵�Ŀ��λ��
    // ���磺�����������ǰλ�úͷ��򣬼��� CameraSide ƫ����������Ŀ��λ��
    return Vector3.zero;
}
}
