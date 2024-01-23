
using UnityEngine;
using Cinemachine;
using AvatarMain;
using UnityEngine.InputSystem;
using RootMotion.FinalIK;
using Detector;
using TargetFinding;
using CustomInspector;

namespace playershooting
{

    public class TPSShootController : MonoBehaviour
    {
        [ReadOnly]
        public bool isAiming = false;
        [ReadOnly]
        public bool Fire = false;

        [HorizontalLine("自定义的参数",2,FixedColor.Gray)]
        // 用于瞄准的虚拟相机
        [SerializeField] private CinemachineVirtualCamera aimVirtualCamera;
        [SerializeField] private GameObject AimPoint;
        // 普通灵敏度和瞄准灵敏度
        [SerializeField] private float normalSensitivity;
        [SerializeField] private float aimSensitivity;
        // 瞄准时射线检测的LayerMask
        [SerializeField] private LayerMask aimColliderLayerMask = new LayerMask();
        // 用于调试显示的Transform
        [SerializeField] private Transform debugTransform;

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
        private Cinemachine3rdPersonFollow thirdPersonFollow;

        private bool _hasAnimator;
        // animation IDs
        private int _animIDEnterAiming;
        private int _animIDAimStatus;

        [HideInInspector]
        public int AimIKParameter;

        //这些是实例化生成后查找物体用的参数
        private string corshairtag = "Corshair";
        private string corshairname = "Corshair";
        private bool searchInactiveObjects = true;

        //给外部取用
        [HideInInspector]
        public Vector3 TmouseWorldPosition;

        //调试用参数
        private Vector3 DetectPoint;
        private Vector3 TargetPoint;

        private void Awake()
        {

        }
        private void Start()
        {
            ComponentInit();
            _hasAnimator = TryGetComponent(out _animator);
            AssignAnimationIDs();

        }


        private void Update()
        {
            AIM();
            FIRE();

        }

        private void AssignAnimationIDs()
        {
            _animIDEnterAiming = Animator.StringToHash("EnterAiming");
            _animIDAimStatus = Animator.StringToHash("AimStatus");
        }

        private void AIM()
        {
            Vector3 mouseWorldPosition = Vector3.zero;

            _hasAnimator = TryGetComponent(out _animator);

            // 获取鼠标在世界空间中的位置
            Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);


            float minDistance = thirdPersonFollow.CameraDistance;

            //Vector3 cameraPos = Camera.main.transform.position;
            //Vector3 cameraForward = Camera.main.transform.forward;

            //Vector3 rayOrigin = cameraPos + cameraForward * minDistance;

            Vector3 rayOrigin = AimPoint.transform.position;
            DetectPoint = rayOrigin;
            TargetPoint = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()).direction;

            Ray ray = new Ray(rayOrigin, Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()).direction);

            if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
            {

                debugTransform.position = raycastHit.point;

                mouseWorldPosition = raycastHit.point;

                TmouseWorldPosition = mouseWorldPosition;

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

                //让角色的朝向始终与TPSShootCamera的朝向相同
                //transform.forward = Vector3.Lerp(transform.forward, aimVirtualCamera.transform.forward, Time.deltaTime * 20f);

                float targetShoulderOffsetY = avatarController.IsCrouching ? CrouchingY : OriginY;
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
                    if (!avatarController.IsCrouching)
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
                    AimIKParameter = 1;
                    gameObject.GetComponent<AimIK>().enabled = true;
                    //AimIKParameter = 0;
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
                AimIKParameter = 0;

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

        private void FIRE()
        {
            if(isAiming)
            {
                Fire = _input.shoot;
            }
            else
            {
                Fire = false;
            }

        }

        private void ShootSiteChange()
        {
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

        private void ComponentInit()
        {
            // 获取角色控制器和输入
            avatarController = GetComponent<AvatarController>();
            _input = GetComponent<BasicInput>();
            rayDectec = GetComponent<RayDectec>();
            thirdPersonFollow = aimVirtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
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

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            //Gizmos.DrawSphere(DetectPoint, 0.1f);
            Gizmos.DrawRay(DetectPoint, TargetPoint);
        }
#endif

    }
}