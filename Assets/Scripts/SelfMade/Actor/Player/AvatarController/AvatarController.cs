using UnityEngine;
using System.Collections;
using Cinemachine;
using Unity.VisualScripting;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using playershooting;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace AvatarMain
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM 
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class AvatarController : MonoBehaviour
    {
        [Header("Player")]
        public GameObject playerAmature; // 对PlayerAmature对象的引用
        [Tooltip("Move speed of the character in m/s")]
        public bool IsTPS = true;
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 3.0f;
        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 6.0f;
        [Tooltip("Crouch speed of the character in m/s")]
        public float CrouchSpeed = 1.0f;
        [Tooltip("Aim speed of the character in m/s")]
        public float AimSpeed = 2.0f;
        public float CrouchingAimSpeed = 1.0f;

        public float Sensitivity = 1.0f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)]
        public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;
        public float ComplexJumpHeight = 1.4f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        //这些函数用在角色八项移动
        private float MovingDirX;
        private float MovingDirZ;
        private float MovingDirNorX;
        private float MovingDirNorZ;
        [HideInInspector]
        public float _TempMovingX;
        [HideInInspector]
        public float _TempMovingY;
        private float _TargetMovingX;
        private float _TargetMovingY;
        private float AimOrNot;
        [HideInInspector]
        public float _TargetAimOrNot;
        public Vector3 MovingDir;
        public Vector3 MovingDirNor;
        

        // 在类的顶部声明 _lastMoveDirection 字段，但不要赋值
        public Vector3 _lastMoveDirection = Vector3.zero;
        public Vector3 targetDr = Vector3.zero;
        public Vector3 MovingDirection = Vector3.zero;
        public float _lastMoveDr = 0.0f;

        // 在 PlayerMovement 类中创建一个变量来跟踪跳跃次数
        public float MaxJumpCount = 2;
        public int jumpCount;
        private float jumpTimer = 0.05f; // 设置二段跳的等待时间
        public float jumpTimerCurrent = 0.0f;
        public bool Jetted = false;
        public float airSpeed;

        //下蹲
        private GameObject PlayerCameraRoot;
        private CharacterController _characterController;
        private bool _isCrouching = false;
        public bool cantCrouchinAir = true;
        private bool CrouchingDetect = false;//用于头顶障碍物检测
        public float OriginOffset = 1.125f;
        public float CrouchingOffset = 0.75f;

        //滚
        public float rolltargetspeed;
        public AnimationCurve rollSpeedCurve = new AnimationCurve(
    new Keyframe(0, 0, 0, 2),
         new Keyframe(0.5f, 1, 0, 0),
         new Keyframe(1, 0, -2, 0));
        private float rollspeed;
        private bool isRolling;
        private bool rollInPorgress;

        //黑手！滑铲
        public float slideLimitSpeed;
        public float slideTargetspeed;
        public AnimationCurve slideSpeedCurve = new AnimationCurve(
        new Keyframe(0, 0, 0, 2),
             new Keyframe(0.5f, 1, 0, 0),
             new Keyframe(1, 0, -2, 0));
        private float slideSpeed;
        private bool isSliding;
        private bool slideInProgress;


        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        public float _speed;
        public float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        private const string JetStatusParam = "JetStatus";
        private const float ThresholdValue = 1.0f;

        // timeout deltatime
        public float _jumpTimeoutDelta;
        public float _fallTimeoutDelta;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;
        private int _animIDJetStatus;
        private int _animIDis_Crouching;
        private int _animIDMovingX;
        private int _animIDMovingY;
        private int _animIDAimOrNot;
        private int _animIDRoll;
        private int _animIDSlide;

        //检测蹲下时上方是否有障碍物
        public LayerMask detectionLayer;
        public float Crouchradius = 0.22f;
        public Transform sphereCenter; // 公共属性，用于指定球体的中心点
        public bool isObstructed = false;
        private bool DetectedResult = false;

        //瞄准时所用函数
        public bool _rotationOnMove = true;

        public CinemachineVirtualCamera virtualCamera; // 你的Cinemachine虚拟摄像机

        public float minDistance = 1f; // 最小FOV
        public float maxDistance = 4f; // 最大FOV
        public float zoomsensitivity = 5f; // 灵敏度
        public float zoomSpeed = 5f; // 缩放速度
        private float targetDistance;
        public bool isAiming;

        //拿给外部取用的参数
        //[HideInInspector]
        public bool IsCrouching;
        //[HideInInspector]
        public bool IsWalking;
        //[HideInInspector]
        public bool IsSprinting;
        //[HideInInspector]
        public bool IsJumping;
        //[HideInInspector]
        public bool Stopping;
        public bool TestButton;
        public bool IsRolling;
        public bool IsSliding;
        //额外判断参数
        private Vector2 INPUTSTOP = new Vector2(0, 0);

#if ENABLE_INPUT_SYSTEM 
        private PlayerInput _playerInput;
#endif
        private Cinemachine3rdPersonFollow followersettings;
        private Animator _animator;
        private TPSShootController tpsshootcontroller;
        private CharacterController _controller;
        private BasicInput _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
                return false;
#endif
            }
        }


        private void Awake()
        {
            followersettings = virtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            //TPS的CinemachineCamera的平滑处理
            if (virtualCamera != null)
            {
                // 初始化目标距离为当前摄像机的距离
                targetDistance = followersettings.CameraDistance;
            }
            tpsshootcontroller = GetComponent<TPSShootController>();
            _characterController = playerAmature.GetComponent<CharacterController>();
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<BasicInput>();
            _lastMoveDirection = Vector3.zero; // 或者根据需要初始化其他值
#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#else
            Debug.LogError("Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void Update()
        {
            Debug.Log(rollInPorgress);
            SlideProgress();
            RollProgress();
            AimingStatus();
            WalkSwitcher();
            CameraZoom();
            _hasAnimator = TryGetComponent(out _animator);
            JumpAndGravity();
            GroundedCheck();
            MoveStatus();
            Move();
            ModifyControllerProperties();
            PerformDetectionAndDrawGizmos();
            ParameterRelink();
            DebugINFO();
        }

        void FixedUpdate()
        {
            if (!Grounded)
            {
                // 更新计时器
                jumpTimerCurrent -= Time.fixedDeltaTime;
            }
            else
            {
                // 在落地时重置计时器
                jumpTimerCurrent = 0.0f;
            }
        }

        private void LateUpdate()
        {
            CameraRotation();
        }


        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            _animIDJetStatus = Animator.StringToHash("JetStatus");
            _animIDis_Crouching = Animator.StringToHash("is_crouching");
            _animIDMovingX = Animator.StringToHash("MovingX");
            _animIDMovingY = Animator.StringToHash("MovingY");
            _animIDAimOrNot = Animator.StringToHash("AimOrNot");
            _animIDRoll = Animator.StringToHash("Roll");
            _animIDSlide = Animator.StringToHash("Slide");
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        //相机滚轮缩放功能，和滚轮切换武器冲突，暂且废弃
        private void CameraZoom()
        {
            //if (virtualCamera != null)
            //{
            //    if (!isAiming)
            //    {

            //        // 根据鼠标滚轮输入更新目标距离
            //        targetDistance -= Input.GetAxis("Mouse ScrollWheel") * zoomsensitivity;
            //        targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);

            //        // 平滑地插值当前距离到目标距离
            //        followersettings.CameraDistance = Mathf.Lerp(followersettings.CameraDistance, targetDistance, zoomSpeed * Time.deltaTime);
            //    }
            //    else
            //    {
            //        followersettings.CameraDistance = maxDistance;
            //        targetDistance = maxDistance;
            //    }
            //}
        }

        private void CameraRotation()
        {
            // if there is an input and camera position is not fixed
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier * Sensitivity;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier * Sensitivity;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }

        private bool isMove; // 在类的作用域内声明 isMove 变量
        private float targetSpeed;

        //增加下蹲CD以保证跳跃后不能立马下蹲产生动作bug
        IEnumerator CrouchDelay()
        {
            cantCrouchinAir = false; // 禁用下蹲输入
            yield return new WaitForSeconds(0.1f); // 等待0.1秒
            cantCrouchinAir = true; // 允许下蹲输入
        }


        //在各种移动速度中进行判定
        private void MoveStatus()
        {
            IsWalking = false;
            IsSprinting = false;

            if(isAiming && _input.sprint)
            {
                targetSpeed = MoveSpeed;
            }

            if (Grounded && _input.sprint && !_input.crouch && !_isCrouching && !isAiming && _input.move != INPUTSTOP)
            {
                targetSpeed = SprintSpeed;
                airSpeed = targetSpeed;
                IsSprinting = true;
            }

            if (Grounded && _input.crouch && cantCrouchinAir && _speed < slideLimitSpeed)
            {
                if (isAiming)
                {
                    targetSpeed = CrouchingAimSpeed;
                }
                else
                {
                    targetSpeed = CrouchSpeed;
                }
                _isCrouching = true;
                _input.jump = false;    //下蹲禁用跳跃
                CrouchingDetect = true; //特征值允许检测下蹲状态
            }
            else if (Grounded && _input.crouch && cantCrouchinAir && _speed >= slideLimitSpeed)
            {
                _isCrouching = true;
            }

                //判定是否在下蹲环境中没有人为输入下蹲指令
                if (Grounded && !_input.crouch)
            {
                //若是没有检测到碰撞
                if (!DetectedResult)
                {
                    _isCrouching = false;   //退出下蹲状态
                    CrouchingDetect = false;    //下蹲特征值重置
                }

            }

            //进行判定是否触发了下蹲
            if (CrouchingDetect)
            {
                if (isObstructed)//遍历后检测到碰撞
                {
                    DetectedResult = true;  //设定Reselt为True
                    _isCrouching = true;    //设置保持下蹲状态
                    if (isAiming)
                    {
                        targetSpeed = CrouchingAimSpeed;
                    }
                    else
                    {
                        targetSpeed = CrouchSpeed;
                    }
                }
                else
                {
                    DetectedResult = false; //设定Reselt为False
                }
            }

            //检测若是在地面且没有输入蹲下，没有输入奔跑，不在蹲下时，将速度调整为步行速度
            if (Grounded && !_input.crouch && !_input.sprint && !_isCrouching &&_input.move!=INPUTSTOP)
            {
                if (isAiming)
                {
                    targetSpeed = AimSpeed;
                    airSpeed = targetSpeed;
                }
                else
                {
                    targetSpeed = MoveSpeed;
                    airSpeed = targetSpeed;
                }
                IsWalking = true;

            }

            //检查是否移动
            if(_input.move == INPUTSTOP && Grounded && !_isCrouching)
            {
                    Stopping = true;
            }
            else
            {
                Stopping = false;
            }

            //若不在地面，则取上轮if的speed为在空中的速度
            if (!Grounded)
            {
                targetSpeed = airSpeed;
            }

            if (_speed == 0)
            {
                airSpeed = 0;

            }

        }

        private void Move()
        {
            MoveStatus();
            // 根据移动速度、冲刺速度以及是否按下冲刺键来设置目标速度（集合进MoveStatus()）
            // float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

            // 注意：Vector2 的 == 运算符使用近似值，因此不容易受到浮点误差的影响，并且比 magnitude 更方便
            // 如果没有输入，则将目标速度设为 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            //检测地面，若不是则按照airspeed规则
            if (Grounded)
            {
                // accelerate or decelerate to target speed
                if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
                {
                    // creates curved result rather than a linear one giving a more organic speed change
                    // note T in Lerp is clamped, so we don't need to clamp our speed
                    _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                        Time.deltaTime * SpeedChangeRate);

                    // round speed to 3 decimal places
                    _speed = Mathf.Round(_speed * 1000f) / 1000f;
                }
                else
                {
                    _speed = targetSpeed;
                }

                _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
                if (_animationBlend < 0.01f) _animationBlend = 0f;
            }

            //标准化输入方向
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;
            MovingDirX = inputDirection.x;
            MovingDirZ = inputDirection.z;

            // 如果有移动输入，则在玩家移动时旋转玩家。
            if (_input.move != Vector2.zero)
            {
                if (IsTPS)
                {

                    _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                        _mainCamera.transform.eulerAngles.y;
                    float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                        RotationSmoothTime);

                    if (_rotationOnMove)
                    {
                        // rotate to face input direction relative to camera position
                        transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                    }
                }
                else
                {
                    inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
                }
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            if (Grounded)
            {
                if (IsTPS)
                {
                    // move the player TPS
                    _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
                }
                else
                {
                    // move the player FPS
                    _controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

                    isMove = _input.move != Vector2.zero;
                }

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetFloat(_animIDSpeed, _animationBlend);
                    _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
                    if (_isCrouching)
                    {
                        if(targetSpeed < slideLimitSpeed)
                            _animator.SetBool(_animIDis_Crouching, true);
                    }
                    else
                    {
                        _animator.SetBool(_animIDis_Crouching, false);
                    }
                }
            }
        }

        // 在适当的时候更新 _lastMoveDirection
        private void UpdateLastMoveDirection()
        {
            Vector3 previousMoveDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // 在需要存储最近移动方向的地方更新 _lastMoveDirection
            if (previousMoveDirection != Vector3.zero)
            {
                _lastMoveDirection = previousMoveDirection;

                if (_input.move != Vector2.zero)
                {
                    if (IsTPS)
                    {
                        _lastMoveDr = Mathf.Atan2(previousMoveDirection.x, previousMoveDirection.z) * Mathf.Rad2Deg +
                                          _mainCamera.transform.eulerAngles.y;
                        float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _lastMoveDr, ref _rotationVelocity,
                            RotationSmoothTime);

                        // rotate to face input direction relative to camera position
                        transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                        targetDr = Quaternion.Euler(0.0f, _lastMoveDr, 0.0f) * Vector3.forward;
                        _lastMoveDirection = targetDr;
                    }
                    else
                    {
                        _lastMoveDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
                        targetDr = _lastMoveDirection;
                    }

                    if (targetDr == _lastMoveDirection)
                    {
                        MovingDirection = targetDr;
                    }
                }
            }
        }

        private void JumpAndGravity()
        {
            if (!_isCrouching && !slideInProgress && !rollInPorgress)
            {
                if (_input.jump)
                {
                    UpdateLastMoveDirection();
                }

                if (Grounded)
                {
                    jumpCount = 0;
                    Jetted = false;
                    // reset the fall timeout timer
                    _fallTimeoutDelta = FallTimeout;

                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, false);
                        _animator.SetBool(_animIDFreeFall, false);
                    }

                    IsJumping = false;

                    // stop our velocity dropping infinitely when grounded
                    if (_verticalVelocity < 0.0f)
                    {
                        _verticalVelocity = -2f;
                    }

                    // Jump
                    if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                    {
                        //设定第一次跳跃不会触发喷气背包
                        //if (!Jetted)
                        //{
                        //    JetON();
                        //}
                        // the square root of H * -2 * G = how much velocity needed to reach desired height
                        _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                        if (IsTPS)
                        {
                            // update animator if using character
                            if (_hasAnimator)
                            {
                                _animator.SetBool(_animIDJump, true);
                            }
                        }
                        IsJumping = true;
                        jumpTimerCurrent = jumpTimer;
                        jumpCount++;
                    }

                    // jump timeout
                    if (_jumpTimeoutDelta >= 0.0f)
                    {
                        _jumpTimeoutDelta -= Time.deltaTime;
                    }
                }
                else
                {
                    if (_input.jump && jumpTimerCurrent <= 0.0f && jumpCount < MaxJumpCount)
                    {
                        Jetted = false;
                        if (!Jetted)
                        {
                            JetON();
                        }
                        // 执行第二次跳跃逻辑
                        _verticalVelocity = 0f; // 或者你可以设置为一个负数，如果希望角色向下运动
                        _verticalVelocity = Mathf.Sqrt(ComplexJumpHeight * -2f * Gravity);
                        // 增加跳跃次数
                        jumpTimerCurrent = jumpTimer;
                        jumpCount++;
                    }


                    // reset the jump timeout timer
                    _jumpTimeoutDelta = JumpTimeout;

                    // fall timeout
                    if (_fallTimeoutDelta >= 0.0f)
                    {
                        _fallTimeoutDelta -= Time.deltaTime;
                    }
                    else
                    {
                        // update animator if using character
                        if (_hasAnimator)
                        {
                            _animator.SetBool(_animIDFreeFall, true);
                        }
                    }

                    if (IsTPS)
                    {
                        _controller.Move(targetDr.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
                    }
                    else
                    {
                        _controller.Move(_lastMoveDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
                    }

                    // if we are not eded, do not jump
                    _input.jump = false;
                    StartCoroutine(CrouchDelay());

                }

                // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
                if (_verticalVelocity < _terminalVelocity)
                {
                    _verticalVelocity += Gravity * Time.deltaTime;
                }
            }
        }

        private void SlideProgress()
        {
            if (Grounded &&_isCrouching && targetSpeed >= slideLimitSpeed)
            {
                if(rollInPorgress)
                    slideSpeed = rolltargetspeed/2 + slideTargetspeed;
                else
                    slideSpeed = _speed + slideTargetspeed - 2;
                isSliding = true;
                if(isSliding && !slideInProgress)
                {
                    IsSliding = true;
                    IsRolling = false;
                    slideInProgress = true;
                    rollInPorgress = false;
                    _animator.SetBool(_animIDRoll, false);
                    _animator.SetBool(_animIDSlide, true);
                    StartCoroutine(DelaySlideMove(0.2f, slideSpeed));
                }
                else
                {
                    //isSliding = false;
                }
            }
        }

        private IEnumerator DelaySlideMove(float delay, float initialRollSpeed)
        {
            yield return new WaitForSeconds(delay);

            float duration = 1f; // 假设曲线的最后一个键定义了翻滚的总时长
            float timeSinceStarted = 0f;

            while (timeSinceStarted <= duration && IsSliding)
            {
                timeSinceStarted += Time.deltaTime;
                float curveTime = timeSinceStarted / duration; // 计算当前时间在曲线总时长中的比例
                float _slideSpeed = initialRollSpeed * slideSpeedCurve.Evaluate(curveTime); // 使用曲线来调整速度

                _controller.Move(transform.forward.normalized * (_slideSpeed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

                yield return null; // 等待下一帧
            }
        }

        private void RollProgress()
        {
            if (Grounded &&_input.roll)
            {
                if(slideInProgress)
                    rollspeed = slideTargetspeed/2 + rolltargetspeed;
                else
                    rollspeed =_speed + rolltargetspeed-2;
                isRolling = true;
                if (isRolling && !rollInPorgress)
                {
                    IsRolling = true;
                    IsSliding = false;
                    rollInPorgress = true;
                    slideInProgress = false;
                    _animator.SetBool(_animIDSlide, false);
                    _animator.SetBool(_animIDRoll, true);
                    StartCoroutine(DelayRollMove(0.2f,rollspeed));
                }
                else
                {
                    //isRolling = false;
                }
            }
        }

        private IEnumerator DelayRollMove(float delay, float initialRollSpeed)
        {
            yield return new WaitForSeconds(delay);
            float duration = 1.1f; // 假设曲线的最后一个键定义了翻滚的总时长
            float timeSinceStarted = 0f;

            while (timeSinceStarted <= duration && IsRolling)
            {
                timeSinceStarted += Time.deltaTime;
                float curveTime = timeSinceStarted / duration; // 计算当前时间在曲线总时长中的比例
                float _rollSpeed = initialRollSpeed * rollSpeedCurve.Evaluate(curveTime); // 使用曲线来调整速度

                //Debug.Log(_rollSpeed);

                _controller.Move(transform.forward.normalized * (_rollSpeed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
                yield return null; // 等待下一帧
            }
        }


        private IEnumerator DelayedJetStatus(float delay)
        {
            yield return new WaitForSeconds(delay);
            _animator.SetFloat(_animIDJetStatus, 0.0f);
        }

        private void JetON()
        {
            float currentValue = _animator.GetFloat(JetStatusParam);
            if (currentValue == 0)
            {
                _animator.SetFloat(_animIDJetStatus, 1.0f);

                // 启动协程，在 delay 秒后将 JetStatus 设置为 0.0f
                StartCoroutine(DelayedJetStatus(0.1f));
            }
            Jetted = true;
        }

        //下蹲站立时碰撞盒变更大小
        private void ModifyControllerProperties()
        {
            //铸币吧已经有CinemachineCameraTarget还Private个PlayerCameraRoot类
            PlayerCameraRoot = CinemachineCameraTarget;

            // 检查is_crouching的值，并根据需要修改CharacterController的属性
            if (_isCrouching)
            {
                // 当蹲下时更改CharacterController的属性
                _characterController.center = new Vector3(_characterController.center.x, 0.4f, _characterController.center.z);
                _characterController.height = 0.77f;
                PlayerCameraRoot.transform.position = new Vector3(PlayerCameraRoot.transform.position.x, playerAmature.transform.position.y + CrouchingOffset, PlayerCameraRoot.transform.position.z);
            }
            else
            {
                // 当未蹲下时更改CharacterController的属性
                _characterController.center = new Vector3(_characterController.center.x, 0.7f, _characterController.center.z);
                _characterController.height = 1.4f;
                PlayerCameraRoot.transform.position = new Vector3(PlayerCameraRoot.transform.position.x, playerAmature.transform.position.y + OriginOffset, PlayerCameraRoot.transform.position.z);
            }
        }


        // 在 Scene 视图中显示检测球体
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Vector3 center = sphereCenter != null ? sphereCenter.position : transform.position; // 以指定的物体为球体中心，若未指定则使用当前对象的位置
            Gizmos.DrawWireSphere(center, Crouchradius);
        }
        // 检测头上是否有障碍物
        private bool PerformDetectionAndDrawGizmos()
        {
            isObstructed = false;

            Vector3 center = sphereCenter != null ? sphereCenter.position : transform.position; // 以指定的物体为球体中心，若未指定则使用当前对象的位置
            Collider[] hitColliders = Physics.OverlapSphere(center, Crouchradius, detectionLayer); // 检测周围的物体

            // 遍历检测到的碰撞体
            foreach (Collider collider in hitColliders)
            {
                //Debug.Log("Detected object on layer: " + LayerMask.LayerToName(collider.gameObject.layer));
                isObstructed = true; // 如果有物体被检测到，则记录为遇到障碍物
                                     // 如果需要执行特定操作，可以在这里添加代码
            }
            return isObstructed; // 返回是否遇到障碍物
        }

        private void MovingDirNormalize()
        {
            if (MovingDirX <= 1.0f && MovingDirX > 0)
            {
                MovingDirNorX = 1;
            }
            else if (MovingDirX >= -1.0f && MovingDirX < 0)
            {
                MovingDirNorX = -1;
            }
            else
            {
                MovingDirNorX = 0;
            }

            if (MovingDirZ <= 1.0f && MovingDirZ > 0)
            {
                MovingDirNorZ = 1;
            }
            else if (MovingDirZ >= -1.0f && MovingDirZ < 0)
            {
                MovingDirNorZ = -1;
            }
            else
            {
                MovingDirNorZ = 0;
            }

            //Debug.Log(MovingDirNorX);

            MovingDir = new Vector3(MovingDirX, 0.0f, MovingDirZ);
            MovingDirNor = new Vector3(MovingDirNorX, 0.0f, MovingDirNorZ);

            if (_hasAnimator)
            {
                if (MovingDirNorX == -1 && MovingDirNorZ == 0)           //左
                {
                    _TargetMovingX = -1;
                    _TargetMovingY = 0;
                }
                else if (MovingDirNorX == -1 && MovingDirNorZ == 1)                //左前
                {
                    _TargetMovingX = -1;
                    _TargetMovingY = 1;
                }
                else if (MovingDirNorX == 0 && MovingDirNorZ == 1)            //正前
                {
                    _TargetMovingX = 0;
                    _TargetMovingY = 1;
                }
                else if (MovingDirNorX == 1 && MovingDirNorZ == 1)            //右前
                {
                    _TargetMovingX = 1;
                    _TargetMovingY = 1;
                }
                else if (MovingDirNorX == 1 && MovingDirNorZ == 0)            //右
                {
                    _TargetMovingX = 1;
                    _TargetMovingY = 0;
                }
                else if (MovingDirNorX == 1 && MovingDirNorZ == -1)           //右后
                {
                    _TargetMovingX = 1;
                    _TargetMovingY = -1;
                }
                else if (MovingDirNorX == 0 && MovingDirNorZ == -1)           //后方
                {
                    _TargetMovingX = 0;
                    _TargetMovingY = -1;
                }
                else if (MovingDirNorX == -1 && MovingDirNorZ == -1)          //左后
                {
                    _TargetMovingX = -1;
                    _TargetMovingY = -1;
                }

            }

        }

        private void WalkSwitcher()
        {
            if (isAiming)
            {
                MovingDirNormalize();
                _TempMovingX = Mathf.Lerp(_TempMovingX, _TargetMovingX, Time.deltaTime * SpeedChangeRate);
                _TempMovingY = Mathf.Lerp(_TempMovingY, _TargetMovingY, Time.deltaTime * SpeedChangeRate);

                _animator.SetFloat(_animIDMovingX, _TempMovingX);
                _animator.SetFloat(_animIDMovingY, _TempMovingY);
            }
            else
            {
                _animator.SetFloat(_animIDMovingX, 0f);
                _animator.SetFloat(_animIDMovingY, 0f);
            }

            _TargetAimOrNot = Mathf.Lerp(_TargetAimOrNot, AimOrNot, Time.deltaTime * SpeedChangeRate);
            _animator.SetFloat(_animIDAimOrNot, _TargetAimOrNot);

        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }


        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void AimingStatus()
        {
            if (tpsshootcontroller.isAiming)
            {
                isAiming = true;
                AimOrNot = 1;
            }
            else
            {
                isAiming = false;
                AimOrNot = 0;
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }

        public void SetSensitivity(float newSensitivity)
        {
            Sensitivity = newSensitivity;
        }

        public void SetRotateOnMove(bool newRorareOnMove)
        {
            _rotationOnMove = newRorareOnMove;
        }

        private void ParameterRelink()
        {
            IsCrouching = _isCrouching;
        }

        public void SetRollEnd(int RollEnd)
        {
            if(RollEnd == 1)
            {
                isRolling = false;
                _animator.SetBool(_animIDRoll, false);
                StartCoroutine(SetRollFalseDelay(0.1f));
            }
        }

        public void SetSlideEnd(int SlideEnd)
        {
            if(SlideEnd == 1)
            {
                isSliding = false;
                _animator.SetBool(_animIDSlide, false);
                StartCoroutine(SetSlideFalseDelay(0.1f));
            }
        }

        private IEnumerator SetRollFalseDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            rollInPorgress = false;
            IsRolling = false;
        }
        private IEnumerator SetSlideFalseDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            slideInProgress = false;
            IsSliding = false;
        }

        private void DebugINFO()
        {

        }
    }
}
