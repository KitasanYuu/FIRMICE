using UnityEngine;

namespace cowsins
{
    public class CameraFOVManager : MonoBehaviour
    {
        [SerializeField] private Rigidbody player;

        private float baseFOV;
        private Camera cam;
        private PlayerMovement movement;
        private WeaponController weapon;

        private void Start()
        {
            cam = GetComponent<Camera>();
            movement = player.GetComponent<PlayerMovement>();
            weapon = player.GetComponent<WeaponController>();

            baseFOV = cam.fieldOfView; // Initialize baseFOV once in Start
        }

        private void Update()
        {
            if (weapon.isAiming && weapon.weapon != null)
                return; // Not applicable if aiming

            float targetFOV;

            if (movement.wallRunning && movement.canWallRun)
            {
                targetFOV = movement.wallrunningFOV;
            }
            else if (movement.currentSpeed > movement.walkSpeed && player.velocity.magnitude > 0.2f)
            {
                targetFOV = movement.runningFOV;
            }
            else
            {
                targetFOV = baseFOV;
            }

            // Smoothly interpolate FOV towards the target value
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * movement.fadeInFOVAmount);
        }
    }
}
