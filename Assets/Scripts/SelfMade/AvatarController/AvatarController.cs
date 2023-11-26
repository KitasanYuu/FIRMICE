using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
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
        public float MoveSpeed = 2.0f;
        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;
        [Tooltip("Crouch speed of the character in m/s")]
        public float CrouchSpeed = 1.0f;
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
        private CharacterController _characterController;
        public bool _isCrouching = false;
        public bool cantCrouchinAir = true;
        private bool CrouchingDetect = false;//用于头顶障碍物检测

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        public float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        Animator animator;

        private const string JetStatusParam = "JetStatus";
        private const float ThresholdValue = 1.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;
        private int _animIDJetStatus;
        private int _animIDis_Crouching;

        //检测蹲下时上方是否有障碍物
        public LayerMask detectionLayer;
        public float Crouchradius = 0.22f;
        public Transform sphereCenter; // 公共属性，用于指定球体的中心点
        public bool isObstructed = false;
        private bool DetectedResult = false;


#if ENABLE_INPUT_SYSTEM 
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
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
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            _characterController = playerAmature.GetComponent<CharacterController>();
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            animator = GetComponent<Animator>();
            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
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
            _hasAnimator = TryGetComponent(out _animator);
            JumpAndGravity();
            GroundedCheck();
            MoveStatus();
            Move();
            ModifyControllerProperties();
            PerformDetectionAndDrawGizmos();
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
            if (Grounded && _input.sprint && !_input.crouch && !_isCrouching)
            {
                targetSpeed = SprintSpeed;
                airSpeed = targetSpeed;
            }

            if (Grounded && _input.crouch && cantCrouchinAir)
            {
                _isCrouching = true;
                targetSpeed = CrouchSpeed;
                _input.jump = false;    //下蹲禁用跳跃
                CrouchingDetect = true; //特征值允许检测下蹲状态
            }

            //判定是否在下蹲环境中没有人为输入下蹲指令
            if (Grounded && !_input.crouch)
            {
                //若是没有检测到碰撞
                if(!DetectedResult)
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
                    targetSpeed = CrouchSpeed;  //速度切换为下蹲速度
                }
                else
                {
                    DetectedResult = false; //设定Reselt为False
                }
            }

            //检测若是在地面且没有输入蹲下，没有输入奔跑，不在蹲下时，将速度调整为步行速度
            if (Grounded && !_input.crouch && !_input.sprint && !_isCrouching)
            {
                targetSpeed = MoveSpeed;
                airSpeed = targetSpeed;
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

            // 如果有移动输入，则在玩家移动时旋转玩家。
            if (_input.move != Vector2.zero)
            {
                if (IsTPS)
                {

                    _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                        _mainCamera.transform.eulerAngles.y;
                    float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                        RotationSmoothTime);

                    // rotate to face input direction relative to camera position
                    transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
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

                    if(targetDr == _lastMoveDirection)
                    {
                        MovingDirection = targetDr;
                    }
                }
            }
        }

        private void JumpAndGravity()
        {
            if (!_isCrouching)
            {
                if (Input.GetKeyDown(KeyCode.Space))
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
                        jumpTimerCurrent = jumpTimer;
                        jumpCount ++;
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


        private IEnumerator DelayedJetStatus(float delay)
        {
            yield return new WaitForSeconds(delay);
            _animator.SetFloat(_animIDJetStatus, 0.0f);
        }

        private void JetON()
        {
            float currentValue = animator.GetFloat(JetStatusParam);
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
            // 检查is_crouching的值，并根据需要修改CharacterController的属性
            if (_isCrouching)
            {
                // 当蹲下时更改CharacterController的属性
                _characterController.center = new Vector3(_characterController.center.x, 0.4f, _characterController.center.z);
                _characterController.height = 0.77f;
            }
            else
            {
                // 当未蹲下时更改CharacterController的属性
                _characterController.center = new Vector3(_characterController.center.x, 0.7f, _characterController.center.z);
                _characterController.height = 1.4f;
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
    }
}
