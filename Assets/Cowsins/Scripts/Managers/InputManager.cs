using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine.SceneManagement;

namespace cowsins
{
    /// <summary>
    /// This script receives inputs from the player and makes it accessible for any script which might require these.
    /// This is pretty convenient regarding to organization, having all inputs in one same place may result in an easier customization later on. 
    /// Note that this script is still subject to major changes in order to make it more adaptable for both old and new Unity input system as well as for 
    /// a rebinding system 
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        #region variables
        // Inputs
        public static bool shooting = false, 
            reloading, 
            aiming, 
            jumping, 
            sprinting, 
            crouching, 
            crouchingDown, 
            interacting, 
            dropping, 
            nextweapon, 
            previousweapon, 
            inspecting, 
            melee, 
            pausing, 
            dashing, 
            invertedAxis,
            yMovementActioned,
            toggleFlashLight;

        public static float x,
            y, 
            scrolling, 
            mousex, 
            mousey, 
            controllerx, 
            controllery, 
            sensitivity_x = 50f, 
            sensitivity_y = 50f, 
            controllerSensitivityX = 30f, 
            controllerSensitivityY = 30f, 
            aimingSensitivityMultiplier = .5f;

        private bool toggleCrouching, 
            toggleSprinting, 
            toggleAiming;

        public enum curDevice
        {
            Keyboard, Gamepad
        };

        public static InputManager inputManager;

        public static PlayerActions inputActions;

        private PlayerMovement player;

        private Vector2 moveInput; 

        #endregion

        private void Awake()
        {
            // Handle singleton
            if (inputManager == null)
            {
                DontDestroyOnLoad(this);
                inputManager = this;
            }
            else Destroy(this.gameObject);

            Init(); 

        }

        private void Start() => player = PlayerStates.instance.GetComponent<PlayerMovement>();

        private void OnEnable()
        {
            inputActions.GameControls.Crouching.started += ctx => toggleCrouching = true;
            inputActions.GameControls.Crouching.canceled += ctx => toggleCrouching = false;

            inputActions.GameControls.Sprinting.started += ctx => toggleSprinting = true;
            inputActions.GameControls.Sprinting.canceled += ctx => toggleSprinting = false;

            inputActions.GameControls.Aiming.started += ctx => toggleAiming = true;
            inputActions.GameControls.Aiming.canceled += ctx => toggleAiming = false;

            inputActions.GameControls.Jumping.started += ctx => jumping = true;

            inputActions.GameControls.ToggleFlashLight.started += ctx => toggleFlashLight = true;

            inputActions.GameControls.Dashing.started += ctx => dashing = true;
            inputActions.GameControls.Dashing.canceled += ctx => dashing = false;


            inputActions.GameControls.Pause.started += ctx => PauseMenu.Instance.TogglePause();

            SceneManager.activeSceneChanged += OnSceneChange;
        }

        private void OnDisable()
        {
            inputActions.Disable();
        }
        private void Update()
        {
            // Handle all the required inputs here
            sensitivity_x = player.sensitivityX;
            sensitivity_y = player.sensitivityY;
            controllerSensitivityX = player.controllerSensitivityX;
            controllerSensitivityY = player.controllerSensitivityY;
            aimingSensitivityMultiplier = player.aimingSensitivityMultiplier;

            if (Mouse.current != null)
            {
                mousex = Mouse.current.delta.x.ReadValue();
                mousey = Mouse.current.delta.y.ReadValue();
            }

            if (Gamepad.current != null)
            {
                controllerx = Gamepad.current.rightStick.x.ReadValue();
                controllery = -Gamepad.current.rightStick.y.ReadValue();
            }

            moveInput = inputActions.GameControls.Movement.ReadValue<Vector2>(); 
            x = moveInput.x;
            y = moveInput.y;

            yMovementActioned =  y > 0;

            reloading = inputActions.GameControls.Reloading.ReadValue<float>() > 0;
            melee = inputActions.GameControls.Melee.ReadValue<float>() > 0;


            // Handle different crouching methods
            if (player.alternateCrouch)
            {
                if (toggleCrouching)
                {
                    crouching = !crouching;
                    crouchingDown = !crouchingDown;
                    toggleCrouching = false;
                }
            }
            else
            {
                crouching = inputActions.GameControls.Crouching.ReadValue<float>() > 0;
                crouchingDown = inputActions.GameControls.Crouching.ReadValue<float>() > 0;
            }

            if (player.alternateSprint)
            {
                if (toggleSprinting)
                {
                    sprinting = !sprinting;
                    toggleSprinting = false;
                }
            }
            else
                sprinting = inputActions.GameControls.Sprinting.ReadValue<float>() > 0;

            shooting = inputActions.GameControls.Firing.ReadValue<float>() > 0;

            scrolling = inputActions.GameControls.Scrolling.ReadValue<Vector2>().y + inputActions.GameControls.ChangeWeapons.ReadValue<float>();

            if (player != null && player.GetComponent<WeaponController>().alternateAiming && player.GetComponent<WeaponController>().weapon != null)
            {
                if (toggleAiming) { aiming = !aiming; toggleAiming = false; }
            }
            else
            {
                if (inputActions.GameControls.Aiming.ReadValue<float>() > 0) aiming = true; else aiming = false;
            }

            interacting = inputActions.GameControls.Interacting.ReadValue<float>() > 0;
            dropping = inputActions.GameControls.Drop.ReadValue<float>() > 0;
            
            inspecting = inputActions.GameControls.Inspect.ReadValue<float>() > 0;

            if (toggleFlashLight) toggleFlashLight = false;
        }

        private void FixedUpdate()
        {
            if (jumping) jumping = false;
            y = inputActions.GameControls.Movement.ReadValue<Vector2>().y;

            if (dashing) dashing = false;            
        }

        #region others

        // Prevents glitchy inputs between scenes
        private void OnSceneChange(Scene current, Scene next)
        {
            inputActions.Disable();
            inputActions.Enable();
        }

        private void Init()
        {
            // Initialize Inputs
            if (inputActions == null)
            {
                inputActions = new PlayerActions();
                inputActions.Enable();
            }
        }

        #endregion
    }

}
