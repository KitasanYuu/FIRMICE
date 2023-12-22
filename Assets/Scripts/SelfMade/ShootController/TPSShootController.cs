using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using StarterAssets;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Runtime.CompilerServices;
using RootMotion.FinalIK;
using Detector;

namespace playershooting
{

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
        public float targetCameraSide = 1;
        public float transitionSpeed = 0.5f; // ���������ٶȵ�ֵ
        public bool isBlocked = false;
        public bool swaKeyPressed = false; // ���ڸ��ٰ���״̬

        public float CrouchingY = -0.5f;
        public float OriginY = -0.4f;

        // ��ɫ������������
        private Animator _animator;
        private AvatarController avatarController;
        private StarterAssetsInputs starterAssetsInputs;
        private RayDectec rayDectec;
        public GameObject corshair;

        private bool _hasAnimator;
        // animation IDs
        private int _animIDEnterAiming;
        private int _animIDAimStatus;

        float lastShootTime = 0f;
        public float fireRate = 0.5f; // 0.5��Ϊ�������Ը�����Ҫ��������

        public int AimIKParameter;

        private void Awake()
        {
            // ��ȡ��ɫ������������
            avatarController = GetComponent<AvatarController>();
            starterAssetsInputs = GetComponent<StarterAssetsInputs>();
            rayDectec = GetComponent<RayDectec>();
        }
        private void Start()
        {
            _hasAnimator = TryGetComponent(out _animator);

            AssignAnimationIDs();


        }


        private void Update()
        {
            Vector3 mouseWorldPosition = Vector3.zero;

            _hasAnimator = TryGetComponent(out _animator);

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
                corshair.SetActive(true);
                ShootSiteChange();

                Vector3 worldAimTarget = mouseWorldPosition;
                worldAimTarget.y = transform.position.y;
                Vector3 aimDirection = (worldAimTarget - transform.position).normalized;

                transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);

                float targetShoulderOffsetY = avatarController._isCrouching ? CrouchingY : OriginY;
                float transitionspeed = 5f; // ���������ٶ�

                // ʹ�ò�ֵ�𽥸ı� ShoulderOffset.y ��ֵ
                aimVirtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>().ShoulderOffset.y = Mathf.Lerp(
                    aimVirtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>().ShoulderOffset.y,
                    targetShoulderOffsetY,
                    Time.deltaTime * transitionspeed
                );

                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDEnterAiming, true);
                    if (!avatarController._isCrouching)
                    {
                        _animator.SetFloat(_animIDAimStatus, 0);
                    }
                    else
                    {
                        _animator.SetFloat(_animIDAimStatus, 1);
                        //Debug.LogError(_animIDAimStatus);
                    }
                }

                if (AimIKParameter == 1)
                {
                    gameObject.GetComponent<AimIK>().enabled = true;
                    AimIKParameter = 0;
                }
            }
            else
            {
                // ȡ����׼
                isAiming = false;
                aimVirtualCamera.Priority = 5;
                avatarController.SetSensitivity(normalSensitivity);
                avatarController.SetRotateOnMove(newRorareOnMove: true);
                corshair.SetActive(false);
                gameObject.GetComponent<AimIK>().enabled = false;

                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDEnterAiming, false);
                }
            }

            // ����
            if (starterAssetsInputs.shoot && isAiming)
            {
                // ��ȡ��ǰʱ��
                float currentTime = Time.time;

                if (currentTime - lastShootTime > fireRate)
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
                    //starterAssetsInputs.shoot = false;
                    lastShootTime = currentTime;
                }
            }
        }

        private void AssignAnimationIDs()
        {
            _animIDEnterAiming = Animator.StringToHash("EnterAiming");
            _animIDAimStatus = Animator.StringToHash("AimStatus");
        }

        private void ShootSiteChange()
        {
            Cinemachine3rdPersonFollow thirdPersonFollow = aimVirtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();

            if (!isBlocked && swaKeyPressed)
            {
                swaKeyPressed = false; // ���ð���״̬
            }

            // ��ⰴ�������¼�
            if (Input.GetKeyDown(KeyCode.Tab)) // ������Ҫ�İ����滻 YourKey
            {
                swaKeyPressed = true; // ���ð���״̬Ϊ true
            }

            if (thirdPersonFollow != null)
            {
                if (rayDectec != null)
                {
                    // ������������ targetCameraSide ֵ
                    if (rayDectec.isBlockedL)
                    {
                        targetCameraSide = 1;
                        isBlocked = true;

                    }
                    if (rayDectec.isBlockedR)
                    {
                        targetCameraSide = 0;
                        isBlocked = true;
                    }
                    if (!rayDectec.isBlockedL && !rayDectec.isBlockedR)
                    {
                        isBlocked = false;
                    }

                    if (!isBlocked && swaKeyPressed)
                    {
                        if (targetCameraSide == 0)
                        {
                            targetCameraSide = 1;
                        }
                        else
                        {
                            targetCameraSide = 0;
                        }
                    }
                    // ƽ���ع��� CameraSide ��ֵ
                    thirdPersonFollow.CameraSide = Mathf.Lerp(thirdPersonFollow.CameraSide, targetCameraSide, transitionSpeed * Time.deltaTime);
                }
                else
                {
                    Debug.LogError("rayDetec No Value");
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

        void AimIKStatus(int status)
        {
            AimIKParameter = status;
            Debug.Log(AimIKParameter);
        }

    }
}