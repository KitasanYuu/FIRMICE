/// <summary>
/// This script belongs to cowsins™ 2022 as a part of the cowsins´ FPS Engine. All rights reserved. 
/// PlayerMovement is based on Dani´s rigidbody player controller.
/// </summary>

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
namespace cowsins {
// Add a rigidbody if needed, PlayerMovement.cs requires a rigidbody to work 
[RequireComponent(typeof(Rigidbody))]
//[RequireComponent(typeof(____))] Player Movement also requires a non trigger collider. Attach your preffered collider method
public class PlayerMovement : MonoBehaviour
{

    #region others

    [System.Serializable]
    public class Events // Store your events
    {
        public UnityEvent OnMove, OnJump, OnLand, OnCrouch, OnStopCrouch, OnSprint, OnSpawn, OnSlide, OnStartWallRun, OnStopWallRun, OnWallBounce, OnStartDash, OnDashing, OnEndDash;
    }
    [System.Serializable]
    public class FootStepsAudio // store your footsteps audio
    {
        public AudioClip[] defaultStep, grassStep, metalStep, mudStep, woodStep;
    }
    [System.Serializable]
    public enum CancelWallRunMethod // Different methods to dettermine when wallrun should stop
    {
        None, Timer
    }
    [System.Serializable]
    public enum DirectionalJumpMethod // Different methods to dettermine the jump method to apply
    {
        None, InputBased, ForwardMovement
    }
    [System.Serializable]
    public enum DashMethod // Different methods to dettermine the jump method to apply
    {
        ForwardAlways, InputBased, Free
    }
    #endregion

    #region variables
    //Assignables
    [Tooltip("Find your camera parent, this is where it should be attached.")]
    public Transform playerCam;

    [Tooltip("Object with the same height as your camera, used to orientate the player.")]
    public Transform orientation;

    private PlayerStats stats;

    // References
    private Rigidbody rb;

    [HideInInspector] public WeaponController weapon;


    //Rotation and look
    private float xRotation;

    private float desiredX;


    //Movements
    [Tooltip("If true: Speed while running backwards = runSpeed." +
    "       if false: Speed while running backwards = walkSpeed")]
    public bool canRunBackwards;

    [Tooltip("If true: Speed while shooting = runSpeed." +
   "       if false: Speed while shooting = walkSpeed")]
    public bool canRunWhileShooting;

    public bool canJumpWhileCrouching;

    [Tooltip("Player deceleration from running speed to walking"), SerializeField]
    private float loseSpeedDeceleration;

    [Tooltip("Capacity to gain speed."), SerializeField]
    private float acceleration = 4500;

    [Tooltip("Maximum allowed speed.")]
    public float currentSpeed = 20;

    [Min(0.01f)]
    public float runSpeed, walkSpeed, crouchSpeed, crouchTransitionSpeed;

    [Tooltip("Displays speed lines effects at certain speed")] public bool useSpeedLines;

    [SerializeField, Tooltip("Speed lines particle system.")] private ParticleSystem speedLines;

    [SerializeField, Min(0), Tooltip("Speed Lines will only be shown above this speed.")] private float minSpeedToUseSpeedLines;

    [SerializeField, Range(.1f, 2), Tooltip("Being 1 = default, amount of speed lines displayed.")] private float speedLinesAmount = 1;

    [Tooltip("Enable this to instantly run without needing to press the sprint button down.")] public bool autoRun;

    [Tooltip("If false, hold to sprint, and release to stop sprinting.")] public bool alternateSprint;

    [Tooltip("If false, hold to crouch, and release to uncrouch.")] public bool alternateCrouch;

    [Min(0.01f)]
    [Tooltip("Max speed the player can reach. Velocity is clamped by this value.")] public float maxSpeedAllowed = 40;

    [HideInInspector] public bool grounded { get; private set; }

    [Tooltip("Every object with this layer will be detected as ground, so you will be able to walk on it")]
    public LayerMask whatIsGround;

    [Range(0, .5f)]
    [Tooltip("Counter movement."), SerializeField]
    private float frictionForceAmount = 0.175f;

    private float threshold = 0.01f;

    [Tooltip("Maximum slope angle that you can walk through."), SerializeField, Range(10, 80)]
    private float maxSlopeAngle = 35f;


    //Crouch & Slide
    public Vector3 crouchScale { get; private set; } = new Vector3(1, 0.5f, 1);

    private Vector3 playerScale;
    public Vector3 PlayerScale { get { return playerScale; } }

    [Tooltip("When true, player will be allowed to slide.")]
    public bool allowSliding;

    [Tooltip("Force added on sliding."), SerializeField]
    private float slideForce = 400;

    [Tooltip("Slide Friction Amount."), SerializeField]
    private float slideCounterMovement = 0.2f;

    private Vector3 normalVector = Vector3.up;


    //Jumping
    private bool canJump = true;

    public bool CanJump { get { return canJump; } set { canJump = value; } }

    private bool readyToJump = true;

    public bool ReadyToJump { get { return readyToJump; } }

    [Tooltip("Amount of jumps you can do without touching the ground")] [Min(1)] public int maxJumps;

    [Tooltip("Gains jump amounts when wallrunning.")] public bool resetJumpsOnWallrun;

    [Tooltip("Gains jump amounts when wallrunning.")] public bool resetJumpsOnWallBounce;

    [Tooltip("Double jump will reset fall damage, only if your player controller is optable to take fall damage")] public bool doubleJumpResetsFallDamage;

    [Tooltip("Interval between jumping")] [Min(.25f), SerializeField] private float jumpCooldown = .25f;

    [Range(0, .3f), Tooltip("Coyote jump allows users to perform more satisfactory and responsive jumps, especially when jumping off surfaces")] public float coyoteJumpTime;

    [Tooltip("The higher this value is, the higher you will get to jump."), SerializeField]
    private float jumpForce = 550f;

    [Tooltip("Method to apply on jumping when the player is not grounded, related to the directional jump")]
    public DirectionalJumpMethod directionalJumpMethod;

    [Tooltip("Force applied on an object in the direction of the directional jump"), SerializeField]
    private float directionalJumpForce;

    [Tooltip("How much control you own while you are not grounded. Being 0 = no control of it, 1 = Full control.")]
    [Range(0, 1), SerializeField]
    private float controlAirborne = .5f;

    [Tooltip("Turn this on to allow the player to crouch while jumping")]
    public bool allowCrouchWhileJumping;
    //Aim assist
    [Tooltip("Determine wether to apply aim assist or not.")]
    public bool applyAimAssist;

    [Min(.1f), SerializeField]
    private float maximumDistanceToAssistAim;

    [Tooltip("Snapping speed."), SerializeField]
    private float aimAssistSpeed;

    [Tooltip("size of the aim assist range."), SerializeField]
    private float aimAssistSensitivity = 3f;

    private RaycastHit hit;

    private Transform target;


    //Stamina
    [Tooltip("You will lose stamina on performing actions when true.")]
    public bool usesStamina;

    [SerializeField] private float stamina;

    [Tooltip("Minimum stamina required to being able to run again."), SerializeField]
    private float minStaminaRequiredToRun;

    [Tooltip("Max amount of stamina."), SerializeField]
    private float maxStamina;

    [SerializeField, Min(1)] private float staminaRegenMultiplier;

    [SerializeField] private bool LoseStaminaWalking;

    [Tooltip("Amount of stamina lost on jumping."), SerializeField]
    private float staminaLossOnJump;

    [Tooltip("Amount of stamina lost on sliding."), SerializeField]
    private float staminaLossOnSlide;

    [SerializeField] private bool canRun;

    [Tooltip("Our Slider UI Object. Stamina will be shown here."), SerializeField]
    private Slider staminaSlider;

    // Wallrun 
    [Tooltip("When enabled, it will allow the player to wallrun on walls")] public bool canWallRun;

    [Tooltip("When enabled, gravity will be applied on the player while wallrunning. If disabled, the player won´t lose height while wallrunning.")] public bool useGravity;

    [Tooltip("Method to determine length of wallRun.")] public CancelWallRunMethod cancelWallRunMethod;

    [SerializeField, Min(.1f), Tooltip("Duration of wall run for cancelWallRunMethod = TIMER.")] private float wallRunDuration;

    [SerializeField, Range(0, 30), Tooltip("Rotation of the camera when wall running. The rotation direction gets automatically adjusted by FPS Engine.")]
    private float wallrunCameraTiltAmount;

    [SerializeField, Tooltip("Speed of the tilt camera movement. This is essentially used for wall running")] private float cameraTiltTransationSpeed;

    [SerializeField, Min(0), Tooltip("Since we do not want to apply all the gravity force directly to the player, we shall define the force that will counter gravity. This force goes in the opposite direction from gravity.")]
    private float wallrunGravityCounterForce;

    [SerializeField, Min(0), Tooltip("Minimum height above ground (in units) required to being able to start wall run. Wall run motion will be cancelled for heights below this.")]
    private float wallMinimumHeight;

    [SerializeField, Min(0), Tooltip("Maximum speed reachable while wall running.")] private float maxWallRunSpeed;

    [SerializeField, Min(0), Tooltip("Impulse applied on the player when wall run is cancelled. This results in a more satisfactory movement. Note that this force goes in the direction of the normal of the wall the player is wall running.")]
    private float stopWallRunningImpulse;

    [SerializeField, Min(0), Tooltip("When wall jumping, force applied on the Y axis.")]
    private float upwardsWallJumpForce;

    [SerializeField, Min(0), Tooltip("When wall jumping, force applied on the X axis, relative to the normal of the wall.")]
    private float normalWallJumpForce;

    [SerializeField, Tooltip("Define wallrunnable wall layers. By default, this is set to the same as whatIsGround.")] private LayerMask whatIsWallRunWall;

    [HideInInspector] public bool wallRunning { get; private set; } = false;

    // Wall Bounce
    [Tooltip("When enabled, it will allow the player to wall bounce on walls.")] public bool canWallBounce;

    [SerializeField, Tooltip("Force applied to the player on wall bouncing. Note that this force is applied on the direction of the reflection of both the player movement and the wall normal.")]
    private float wallBounceForce;

    [SerializeField, Tooltip("Force applied on the player on wall bouncing ( Y axis – Vertical Force ).")]
    private float wallBounceUpwardsForce;

    [SerializeField, Range(0.1f, 2), Tooltip("maximum Distance to detect a wall you can bounce on. This will use the same layer as wall run walls.")]
    private float oppositeWallDetectionDistance = 1;

    // Dash
    [Tooltip("Method to determine how the dash will work")]
    public DashMethod dashMethod;

    [Tooltip("When enabled, it will allow the player to perform dashes.")]
    public bool canDash;

    [Tooltip("When enabled, player will not receive damage.")]
    public bool damageProtectionWhileDashing;

    [Tooltip("When enabled, it will allow the player to perform dashes.")] public bool dashing;

    [Tooltip("force applied when dashing. Note that this is a constant force, not an impulse, so it is being applied while the dash lasts.")]
    public float dashForce;

    [Tooltip("When enabled, it will allow the player to perform dashes.")] public bool infiniteDashes;

    [Min(1), Tooltip("maximum ( initial ) amount of dashes. Dashes will be regenerated up to “amountOfDashes”, you won´t be able to gain more dashes than this value, check dash Cooldown.")]
    public int amountOfDashes = 3;

    [SerializeField, Min(.1f), Tooltip("Time to regenerate a dash on performing a dash motion.")]
    private float dashCooldown;

    public int currentDashes;

    [Range(.1f, .5f), Tooltip("Duration of the ability ( dash ).")]
    public float dashDuration;

    [Tooltip("When enabled, it will allow the player to shoot while dashing.")]
    public bool canShootWhileDashing;

    //Others
    [HideInInspector] public bool isCrouching;

    [SerializeField] private FootStepsAudio footsteps;


    // Audio
    [Header("Audio")]
    private AudioSource _audio;

    [Tooltip("Volume of the AudioSource."), SerializeField]
    private float footstepVolume;

    [Tooltip("Play speed of the footsteps."), SerializeField, Range(.1f, .95f)] private float footstepSpeed;

    private float stepTimer;

    public Events events;

    [Tooltip("Horizontal sensitivity (X Axis)")] public float sensitivityX = 4;

    [Tooltip("Vertical sensitivity (Y Axis)")] public float sensitivityY = 4;

    [Tooltip("Horizontal sensitivity (X Axis) using controllers")] public float controllerSensitivityX = 35;

    [Tooltip("Vertical sensitivity (Y Axis) using controllers")] public float controllerSensitivityY = 35;

    [Range(.1f, 1f), Tooltip("Sensitivity will be multiplied by this value when aiming")] public float aimingSensitivityMultiplier = .4f;

    [Tooltip("Default field of view of your camera"), Range(1, 179)] public float normalFOV;

    [Tooltip("Running field of view of your camera"), Range(1, 179)] public float runningFOV;

    [Tooltip("Wallrunning field of view of your camera"), Range(1, 179)] public float wallrunningFOV;

    [Tooltip("Fade Speed - Start Transition for the field of view")] public float fadeInFOVAmount;

    [Tooltip("Fade Speed - Finish Transition for the field of view")] public float fadeOutFOVAmount;

    [System.Serializable]
    public class Sounds
        {
            public AudioClip jumpSFX, landSFX;
        }
        [SerializeField] private Sounds sounds;

    #endregion

    private void Awake() => GetAllReferences();
    private void Start()
    {

        playerScale = transform.localScale;
        canRun = true;
        canJump = true;
        jumpCount = maxJumps;
        if (canDash && !infiniteDashes)
        {
            UIEvents.onInitializeDashUI?.Invoke(amountOfDashes); 
            currentDashes = amountOfDashes;
        }

        ResetStamina();
        events.OnSpawn.Invoke();
    }


    private void FixedUpdate()
    {
        if (rb.velocity.magnitude > maxSpeedAllowed) rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeedAllowed);
        Stamina();
        
        if (!PlayerStats.Controllable) return;

        if (canWallRun)
        {
            CheckWalls();
            if (InputManager.yMovementActioned && (wallLeft || wallRight) && CheckHeight() && rb.velocity.magnitude > walkSpeed) WallRun();
            else StopWallRun();
        }
    }

        private void Update()
        {
            if (canWallBounce) CheckOppositeWall();

            if (InputManager.jumping && wallOpposite && canWallBounce && PlayerStats.Controllable&& CheckHeight()) WallBounce();
        }
    public void HandleVelocities()
    {

            HandleSpeedLines();

            if (isCrouching)
                return;

            if (weapon.weapon != null && weapon.isAiming && weapon.weapon.setMovementSpeedWhileAiming)
            {
                currentSpeed = weapon.weapon.movementSpeedWhileAiming;
                return;
            }

            if ((InputManager.sprinting || autoRun) && canRun)
            {
                bool shouldRun = true;
                if (!canRunBackwards && InputManager.y < 0 || (!canRunWhileShooting && InputManager.shooting && weapon.weapon != null))
                {
                    currentSpeed = Mathf.MoveTowards(currentSpeed, walkSpeed, Time.deltaTime * loseSpeedDeceleration);
                    shouldRun = false;
                }

                if (shouldRun && (canRunBackwards || Vector3.Dot(orientation.forward, rb.velocity) > 0))
                {
                    if (canRunWhileShooting || !InputManager.shooting)
                    {
                        currentSpeed = runSpeed;
                    }
                }
            }
            else
            {
                currentSpeed = walkSpeed;
            }

            if (rb.velocity.sqrMagnitude < 0.0001f)
            {
                currentSpeed = walkSpeed;
            }

        }

        public void HandleSpeedLines()
        {
            if (speedLines == null) return; 
            // Check if we want to use speedlines. If false, stop and return.
            if (!useSpeedLines || PauseMenu.isPaused)
            {
                speedLines.Stop();
                return;
            }

            if (rb.velocity.sqrMagnitude > minSpeedToUseSpeedLines * minSpeedToUseSpeedLines)
            {
                speedLines.Play(); // Play speedlines
            }
            else
            {
                speedLines.Stop(); // Stop speedlines
            }

            // HandleEmission
            var emission = speedLines.emission;
            float emissionRate = (rb.velocity.magnitude > runSpeed) ? 200 : 70;
            emission.rateOverTime = emissionRate * speedLinesAmount;
        }


    public void StartCrouch()
    {
        isCrouching = true;
        currentSpeed = crouchSpeed;

        if (rb.velocity.magnitude >= walkSpeed && grounded && allowSliding && !hasJumped)
        { // Handle sliding
            events.OnSlide.Invoke(); // Invoke your own method on the moment you slid NOT WHILE YOU ARE SLIDING
                                     // Add the force on slide
            rb.AddForce(orientation.transform.forward * slideForce);
            //staminaLoss
            if (usesStamina) stamina -= staminaLossOnSlide;
        }
    }

    public void StopCrouch()
    {
        isCrouching = false;
        transform.localScale = Vector3.MoveTowards(transform.localScale, playerScale, Time.deltaTime * crouchTransitionSpeed);
    }

    private void Stamina()
    {
        // Check if we def wanna use stamina
        if (!usesStamina || stats.isDead || !PlayerStats.Controllable) return;

        float oldStamina = stamina; // Store stamina before we change its value

        // We ran out of stamina
        if (stamina <= 0)
        {
            canRun = false;
            canJump = false;
            stamina = 0;
        }

        // Wait for stamina to regenerate up to the min value allowed to start running and jumping again
        if (stamina >= minStaminaRequiredToRun)
        {
            canRun = true; canJump = true;
        }

        // Regen stamina
        if (stamina < maxStamina)
        {
            if (currentSpeed <= walkSpeed && !LoseStaminaWalking
                || currentSpeed < runSpeed && (!LoseStaminaWalking || LoseStaminaWalking && InputManager.x == 0 && InputManager.y == 0))
                stamina += Time.deltaTime * staminaRegenMultiplier;
        }

        // Lose stamina
        if (currentSpeed == runSpeed && canRun && !wallRunning) stamina -= Time.deltaTime;
        if (currentSpeed < runSpeed && LoseStaminaWalking && (InputManager.x != 0 || InputManager.y != 0)) stamina -= Time.deltaTime * (walkSpeed / runSpeed);

        // Stamina UI not found might be a problem, it won´t be shown but you will get notified 
        if (staminaSlider == null)
        {
            Debug.LogWarning("REMEMBER: You forgot to attach your StaminaSlider UI Component, so it won´t be shown.");
            return;
        }

        // Handle stamina UI 
        if (oldStamina != stamina)
            staminaSlider.gameObject.SetActive(true);
        else
            staminaSlider.gameObject.SetActive(false);

        staminaSlider.maxValue = maxStamina;
        staminaSlider.value = stamina;
    }

    /// <summary>
    /// Handle all the basics related to the movement of the player.
    /// </summary>
    public void Movement(bool move)
    {

        //Extra gravity
        rb.AddForce(Vector3.down * Time.deltaTime * 10);

        //Find actual velocity relative to where player is looking
        Vector2 mag = FindVelRelativeToLook();
        float xMag = mag.x, yMag = mag.y;

        //Counteract sliding and sloppy movement
        FrictionForce(InputManager.x, InputManager.y, mag);

        //If speed is larger than maxspeed, cancel out the input so you don't go over max speed
        if (InputManager.x > 0 && xMag > currentSpeed || InputManager.x < 0 && xMag < -currentSpeed) InputManager.x = 0;
        if (InputManager.y > 0 && yMag > currentSpeed || InputManager.y < 0 && yMag < -currentSpeed) InputManager.y = 0;

        float multiplier = (!grounded) ? controlAirborne : 1;
        float multiplierV = (!grounded) ? controlAirborne : 1;

        float multiplier2 = (weapon.weapon != null) ? weapon.weapon.weightMultiplier : 1;

        if (rb.velocity.sqrMagnitude < .02f) rb.velocity = Vector3.zero;

        if (!move)
        {
            if (grounded) rb.velocity = Vector3.zero; 
            return;
        }

        rb.AddForce(orientation.transform.forward * InputManager.y * acceleration * Time.deltaTime * multiplier * multiplierV / multiplier2);
        rb.AddForce(orientation.transform.right * InputManager.x * acceleration * Time.deltaTime * multiplier / multiplier2);
    }

    /// <summary>
    /// Manage the footstep audio playing
    /// </summary>
    public void FootSteps()
    {
        // Reset timer if conditions are met + dont play the footsteps
        if (!grounded || rb.velocity.sqrMagnitude <= .1f)
        {
            stepTimer = 1 - footstepSpeed;
            return;
        }

        // Wait for the next time to play a sound
        stepTimer -= Time.deltaTime * rb.velocity.magnitude / 15;

        // Play the sound and reset
        if (stepTimer <= 0)
        {
            stepTimer = 1 - footstepSpeed;
            _audio.pitch = UnityEngine.Random.Range(.7f, 1.3f); // Add variety to avoid boring and repetitive sounds while walking
            // Remember that you can also add a few more sounds to each of the layers to add even more variety to your sfx.
            if (Physics.Raycast(playerCam.position, Vector3.down, out RaycastHit hit, playerCam.position.y + .01f, whatIsGround))
            {
                int i = 0;
                switch (hit.transform.gameObject.layer)
                {
                    case 3: // Ground
                        i = UnityEngine.Random.Range(0, footsteps.defaultStep.Length);
                        _audio.PlayOneShot(footsteps.defaultStep[i], footstepVolume);
                        break;
                    case 10: // Grass
                        i = UnityEngine.Random.Range(0, footsteps.grassStep.Length);
                        _audio.PlayOneShot(footsteps.grassStep[i], footstepVolume);
                        break;
                    case 11: // Metal
                        i = UnityEngine.Random.Range(0, footsteps.metalStep.Length);
                        _audio.PlayOneShot(footsteps.metalStep[i], footstepVolume);
                        break;
                    case 12: // Mud
                        i = UnityEngine.Random.Range(0, footsteps.mudStep.Length);
                        _audio.PlayOneShot(footsteps.mudStep[i], footstepVolume);
                        break;
                    case 13: // Wood
                        i = UnityEngine.Random.Range(0, footsteps.woodStep.Length);
                        _audio.PlayOneShot(footsteps.woodStep[i], footstepVolume);
                        break;
                }
            }
        }
    }

    public int jumpCount;

    private bool hasJumped = false;

    public void Jump()
    {
        jumpCount--;
        readyToJump = false;
        hasJumped = true;

        if (doubleJumpResetsFallDamage) GetComponent<PlayerStats>().height = transform.position.y;

        //Add jump forces
        if (wallRunning) // When we wallrun, we want to add extra side forces
        {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            rb.AddForce(transform.up * upwardsWallJumpForce);
            rb.AddForce(wallNormal * normalWallJumpForce, ForceMode.Impulse);
        }
        else
        {
            rb.AddForce(Vector2.up * jumpForce * 1.5f);
            rb.AddForce(normalVector * jumpForce * 0.5f);
            // Handle directional jumping
            if (!grounded && directionalJumpMethod != DirectionalJumpMethod.None && maxJumps > 1 && !wallOpposite)
            {
                if (Vector3.Dot(rb.velocity, new Vector3(InputManager.x, 0, InputManager.y)) > .5f)
                    rb.velocity = rb.velocity / 2;
                if (directionalJumpMethod == DirectionalJumpMethod.InputBased) // Input based method for directional jumping
                {
                    rb.AddForce(orientation.right * InputManager.x * directionalJumpForce, ForceMode.Impulse);
                    rb.AddForce(orientation.forward * InputManager.y * directionalJumpForce, ForceMode.Impulse);
                }
                if (directionalJumpMethod == DirectionalJumpMethod.ForwardMovement) // Forward Movement method for directional jumping, dependant on orientation
                    rb.AddForce(orientation.forward * Mathf.Abs(InputManager.y) * directionalJumpForce, ForceMode.VelocityChange);
            }

            //If jumping while falling, reset y velocity.
            if (rb.velocity.y < 0.5f)
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            else if (rb.velocity.y > 0)
                rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y / 2, rb.velocity.z);
        }

        //staminaLoss
        if (usesStamina) stamina -= staminaLossOnJump;
           
                
            SoundManager.Instance.PlaySound(sounds.jumpSFX, 0, 0, false, 0);
        Invoke(nameof(ResetJump), jumpCooldown);
    }

    private float coyoteTimer;

    public bool canCoyote;
    public void HandleCoyoteJump()
    {
        if (grounded) coyoteTimer = 0;
        else coyoteTimer -= Time.deltaTime;

        if (hasJumped) return;
        canCoyote = coyoteTimer > 0 && readyToJump;
    }
    private void ResetJump() => readyToJump = true;

    private float wallRunRotation;
    /// <summary>
    /// Handle all the basics related to camera movement 
    /// </summary>
    public void Look()
    {

        int inverted = (InputManager.invertedAxis) ? -1 : 1;

        float sensM = (weapon.isAiming) ? InputManager.aimingSensitivityMultiplier : 1;

        //Handle the camera movement and look based on the inputs received by the user
        float mouseX = (InputManager.mousex * InputManager.sensitivity_x * Time.fixedDeltaTime + InputManager.controllerx * InputManager.controllerSensitivityX * Time.fixedDeltaTime) * inverted * sensM;
        float mouseY = (InputManager.mousey * InputManager.sensitivity_y * Time.fixedDeltaTime * inverted + InputManager.controllery * InputManager.controllerSensitivityY * Time.fixedDeltaTime * -inverted) * sensM;

        //Find current look rotation
        Vector3 rot = playerCam.transform.localRotation.eulerAngles;
        desiredX = rot.y + mouseX;

        //Rotate, and also make sure we dont over- or under-rotate.
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -89.7f, 89.7f); // The reason why the value is 89.7 instead of 90 is to prevent errors with the wallrun
        if (wallRunning && canWallRun) wallRunRotation = wallLeft ? Mathf.Lerp(wallRunRotation, -wallrunCameraTiltAmount, Time.deltaTime * cameraTiltTransationSpeed) : Mathf.Lerp(wallRunRotation, wallrunCameraTiltAmount, Time.deltaTime * cameraTiltTransationSpeed);
        else wallRunRotation = Mathf.Lerp(wallRunRotation, 0, Time.deltaTime * cameraTiltTransationSpeed);
        //Perform the rotations on: 
        playerCam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, wallRunRotation); // the camera parent
        orientation.transform.localRotation = Quaternion.Euler(0, desiredX, 0); // the orientation

        // Decide wether to use aim assist or not
        if (AimAssistHit() == null || !applyAimAssist || target == null || Vector3.Distance(target.position, transform.position) > maximumDistanceToAssistAim) return;
        // Get the direction to look at
        Vector3 direction = (AimAssistHit().position - transform.position).normalized;
        Quaternion targetRotation = transform.rotation * Quaternion.FromToRotation(transform.forward, direction);
        // Smoothly override our current camera rotation towards the selected enemy
        playerCam.transform.localRotation = Quaternion.Lerp(playerCam.transform.localRotation, targetRotation, Time.deltaTime * aimAssistSpeed);
    }

    /// <summary>
    /// Add friction force to the player when it´s not airborne
    /// Please note that it counters movement, since it goes in the opposite direction to velocity
    /// </summary>
    private void FrictionForce(float x, float y, Vector2 mag)
    {
        // Prevent from adding friction on an airborne body
        if (!grounded || InputManager.jumping || hasJumped) return;

        //Slow down sliding + prevent from infinite sliding
        if (InputManager.crouching)
        {
            rb.AddForce(acceleration * Time.deltaTime * -rb.velocity.normalized * slideCounterMovement);
            return;
        }

        // Counter movement ( Friction while moving )
        // Prevent from sliding not on purpose
        if (Math.Abs(mag.x) > threshold && Math.Abs(x) < 0.05f || (mag.x < -threshold && x > 0) || (mag.x > threshold && x < 0))
            rb.AddForce(acceleration * orientation.transform.right * Time.deltaTime * -mag.x * frictionForceAmount);
        if (Math.Abs(mag.y) > threshold && Math.Abs(y) < 0.05f || (mag.y < -threshold && y > 0) || (mag.y > threshold && y < 0))
            rb.AddForce(acceleration * orientation.transform.forward * Time.deltaTime * -mag.y * frictionForceAmount);

        if (Math.Abs(x) < 0.05f && Math.Abs(y) < 0.05f && grounded && floorAngle > 0) rb.AddForce(acceleration * -rb.velocity * Time.deltaTime);

        //Limit diagonal running. This will also cause a full stop if sliding fast and un-crouching, so not optimal.
        if (Mathf.Sqrt((Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2))) > currentSpeed)
        {
            float fallspeed = rb.velocity.y;
            Vector3 n = rb.velocity.normalized * currentSpeed;
            rb.velocity = new Vector3(n.x, fallspeed, n.z);
        }
    }

    /// <summary>
    /// Find the velocity relative to where the player is looking
    /// Useful for vectors calculations regarding movement and limiting movement
    /// </summary>
    /// <returns></returns>
    public Vector2 FindVelRelativeToLook()
    {
        float lookAngle = orientation.transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = rb.velocity.magnitude;
        float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xMag, yMag);
    }
    /// <summary>
    /// Determine wether this is determined as floor or not
    /// </summary>
    private bool IsFloor(Vector3 v)
    {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < maxSlopeAngle;
    }

    private float FloorAngle(Vector3 v)
    {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle;
    }
    /// <summary>
    /// Basically find everything the script needs to work
    /// </summary>
    void GetAllReferences()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // We check if there is already an input manager before instantiating a new one
        // in order to prevent errors regarding inputs not being received.
        if (InputManager.inputManager == null)
            Instantiate(Resources.Load("InputManager"));

        rb = GetComponent<Rigidbody>();
        _audio = GetComponent<AudioSource>();
        weapon = GetComponent<WeaponController>();
        stats = GetComponent<PlayerStats>();
    }

    private bool cancellingGrounded;

    private float floorAngle; 
    /// <summary>
    /// Handle ground detection
    /// </summary>
    private void OnCollisionStay(Collision other)
    {
        //Make sure we are only checking for walkable layers
        int layer = other.gameObject.layer;
        if (whatIsGround != (whatIsGround | (1 << layer))) return;

        //Iterate through every collision in a physics update
        for (int i = 0; i < other.contactCount; i++)
        {
            Vector3 normal = other.contacts[i].normal;
            //FLOOR
            if (IsFloor(normal))
            {
                if (!grounded)
                {
                                
                    SoundManager.Instance.PlaySound(sounds.landSFX, 0, 0, false, 0);
                    events.OnLand.Invoke(); // We have just landed
                    jumpCount = maxJumps; // Reset jumps left
                    hasJumped = false;
                }
                grounded = true;
                cancellingGrounded = false;
                normalVector = normal;
                CancelInvoke(nameof(StopGrounded));
            }
             floorAngle = FloorAngle(normal); 
        }

        float delay = 3f;
        if (!cancellingGrounded)
        {
            cancellingGrounded = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
    }

    /// <summary>
    /// Returns the transform you want your camera to be sticked to 
    /// </summary>
    private Transform AimAssistHit()
    {
        // Aim assist will work on enemies only, since we dont wanna snap our camera on any object around the environment
        // max range to snap
        float range = 40;
        // Max range depends on the weapon range if you are holding a weapon
        if (weapon.weapon != null) range = weapon.weapon.bulletRange;

        // Detect our potential transform
        RaycastHit hit;
        if (Physics.SphereCast(playerCam.transform.GetChild(0).position, aimAssistSensitivity, playerCam.transform.GetChild(0).transform.forward, out hit, range) && hit.transform.CompareTag("Enemy"))
        {
            target = hit.collider.transform;
        }
        else target = null;
        // Return our target
        return target;
    }

    private void StopGrounded()
    {
        grounded = false;
        coyoteTimer = coyoteJumpTime;
    }

    void ResetStamina() => stamina = maxStamina;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Weapons"))
        {
            Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
        }
    }
    #region wallMovement

    public bool wallLeft { get; private set; }
    public bool wallRight { get; private set; }
    public bool wallOpposite { get; private set; }

    private RaycastHit wallLeftHit, wallRightHit, wallOppositeHit;
    private void CheckWalls()
    {
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out wallLeftHit, .8f, whatIsWallRunWall);
        wallRight = Physics.Raycast(transform.position, orientation.right, out wallRightHit, .8f, whatIsWallRunWall);
    }

    private bool CheckHeight() { return !Physics.Raycast(transform.position, Vector3.down, wallMinimumHeight, whatIsWallRunWall); }

    private Vector3 wallNormal;

    private Vector3 wallDirection;

    private float wallRunTimer = 0;

    //Shootout to Dave on YT for some of the code related to wallrun.
    private void WallRun()
    {
        wallNormal = wallRight ? wallRightHit.normal : wallLeftHit.normal;
        wallDirection = Vector3.Cross(wallNormal, transform.up).normalized * 10;
        // Fixing wallrunning directions depending on the orientation 
        if ((orientation.forward - wallDirection).magnitude > (orientation.forward + wallDirection).magnitude) wallDirection = -wallDirection;

        // Handling WallRun Cancel
        if (OppositeVectors() < -.5f) StopWallRun();

        if (cancelWallRunMethod == CancelWallRunMethod.Timer)
        {
            wallRunTimer -= Time.deltaTime;
            if (wallRunTimer <= 0)
            {
                rb.AddForce(wallNormal * stopWallRunningImpulse, ForceMode.Impulse);
                StopWallRun();
            }
        }

        // Start Wallrunning
        if (!wallRunning) StartWallRunning();

        rb.useGravity = useGravity;

        if (useGravity && rb.velocity.y < 0) rb.AddForce(transform.up * wallrunGravityCounterForce, ForceMode.Force);
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxWallRunSpeed);

        if (!(wallRight && InputManager.x < 0) && !(wallLeft && InputManager.x > 0)) rb.AddForce(-wallNormal * 100, ForceMode.Force);

        rb.AddForce(wallDirection, ForceMode.Force);


    }

    private void StartWallRunning()
    {
        wallRunning = true;
        wallRunTimer = wallRunDuration;
        events.OnStartWallRun.Invoke();
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce(wallDirection, ForceMode.Impulse);
        if (resetJumpsOnWallrun) jumpCount = maxJumps;
    }

    private void StopWallRun()
    {
        if (wallRunning)
        {
            events.OnStopWallRun.Invoke();
            rb.AddForce(wallNormal * stopWallRunningImpulse, ForceMode.Impulse);
            if (resetJumpsOnWallrun) jumpCount = maxJumps - 1;
        }
        wallRunning = false;
        rb.useGravity = true;
    }

    private float OppositeVectors() { return Vector3.Dot(wallDirection, orientation.forward); }

    private void CheckOppositeWall()
    {
        wallOpposite = Physics.Raycast(transform.position, orientation.forward, out wallOppositeHit, oppositeWallDetectionDistance, whatIsWallRunWall);
    }

    public void WallBounce()
    {
        if (resetJumpsOnWallBounce) jumpCount = maxJumps - 1;
        events.OnWallBounce.Invoke();
        Vector3 direction = Vector3.Reflect(orientation.forward, wallOppositeHit.normal);
        rb.AddForce(direction * wallBounceForce, ForceMode.VelocityChange);
        rb.AddForce(transform.up * wallBounceUpwardsForce, ForceMode.Impulse);
    }
    #endregion
    #region dashing

    private void RegainDash()
    {
        // Gain a dash
        currentDashes += 1;
        UIEvents.onDashGained?.Invoke(); 
    }
    public void RegainDash(object s, EventArgs e)
    {
        // Wait to regain a new dash
        Invoke(nameof(RegainDash), dashCooldown);
        UIEvents.onDashUsed?.Invoke(currentDashes);
    }
    
    #endregion
}
}