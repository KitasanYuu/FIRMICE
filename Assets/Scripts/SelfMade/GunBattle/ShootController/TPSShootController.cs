
using UnityEngine;
using Cinemachine;
using Avatar;
using UnityEngine.InputSystem;
using RootMotion.FinalIK;
using Detector;
using TargetFinding;

namespace playershooting
{

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

        public bool isAiming = false;
        public float targetCameraSide = 1;
        public float transitionSpeed = 0.5f; // 调整过渡速度的值
        public bool isBlocked = false;
        public bool swaKeyPressed = false; // 用于跟踪按键状态

        public float CrouchingY = -0.5f;
        public float OriginY = -0.4f;

        // 角色控制器和输入
        private Animator _animator;
        private AvatarController avatarController;
        private BasicInput _input;
        private RayDectec rayDectec;
        public GameObject corshair;

        private bool _hasAnimator;
        // animation IDs
        private int _animIDEnterAiming;
        private int _animIDAimStatus;


        public int AimIKParameter;

        //这些是实例化生成后查找物体用的参数
        private string corshairtag = "Corshair";
        private string corshairname = "Corshair";
        private bool searchInactiveObjects = true;

        //给外部取用
        public Vector3 TmouseWorldPosition;

        private void Awake()
        {
            // 获取角色控制器和输入
            avatarController = GetComponent<AvatarController>();
            _input = GetComponent<BasicInput>();
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

            // 获取鼠标在世界空间中的位置
            Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            //Transform hitTransform = null;
            if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
            {
                debugTransform.position = raycastHit.point;
                mouseWorldPosition = raycastHit.point;
                TmouseWorldPosition = mouseWorldPosition;
                //hitTransform = raycastHit.transform;
            }

            // 瞄准
            if (_input.aim)
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

                float targetShoulderOffsetY = avatarController._isCrouching ? CrouchingY : OriginY;
                float transitionspeed = 5f; // 调整过渡速度

                // 使用插值逐渐改变 ShoulderOffset.y 的值
                aimVirtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>().ShoulderOffset.y = Mathf.Lerp(
                    aimVirtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>().ShoulderOffset.y,
                    targetShoulderOffsetY,
                    Time.deltaTime * transitionspeed
                );

                if (corshair != null)
                {
                    corshair.SetActive(true);
                }
                else
                {
                    FindCorshair();
                }


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
                // 取消瞄准
                isAiming = false;
                aimVirtualCamera.Priority = 5;
                avatarController.SetSensitivity(normalSensitivity);
                avatarController.SetRotateOnMove(newRorareOnMove: true);
                gameObject.GetComponent<AimIK>().enabled = false;

                if (corshair != null)
                {
                    corshair.SetActive(false);
                }

                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDEnterAiming, false);
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
                swaKeyPressed = false; // 重置按键状态
            }

            // 检测按键按下事件
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                swaKeyPressed = true; // 设置按键状态为 true
            }

            if (thirdPersonFollow != null)
            {
                if (rayDectec != null)
                {
                    // 根据条件调整 targetCameraSide 值
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
                    // 平滑地过渡 CameraSide 的值
                    thirdPersonFollow.CameraSide = Mathf.Lerp(thirdPersonFollow.CameraSide, targetCameraSide, transitionSpeed * Time.deltaTime);
                }
                else
                {
                    Debug.LogError("rayDetec No Value");
                }
            }
        }
        // 根据 CameraSide 计算目标位置

        private void FindCorshair()
        {
            if (corshair == null)
            {
                TargetSeeker targetseeker = GetComponent<TargetSeeker>();
                if (targetseeker != null)
                {
                    targetseeker.objectTagToFind = corshairtag;
                    targetseeker.objectNameToFind = corshairname;
                    targetseeker.searchInactiveObjects = searchInactiveObjects;
                    targetseeker.SetStatus(true);
                    if (targetseeker.foundObject != null)
                    {
                        corshair = targetseeker.foundObject;
                        targetseeker.SetStatus(false);
                    }
                }
            }
        }

        private Vector3 CalculateTargetPosition(float cameraSide)
        {
            // 这里根据 CameraSide 的值计算目标位置，返回一个 Vector3
            // 请根据你的逻辑实现计算目标位置的方法
            // 这个方法需要根据摄像机和目标位置的相对位置来返回一个 Vector3 类型的目标位置
            // 例如：根据摄像机当前位置和方向，加上 CameraSide 偏移量来计算目标位置
            return Vector3.zero;
        }

        void AimIKStatus(int status)
        {
            AimIKParameter = status;
            //Debug.Log(AimIKParameter);
        }

    }
}