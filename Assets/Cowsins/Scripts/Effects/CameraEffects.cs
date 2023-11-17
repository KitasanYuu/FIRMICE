using UnityEngine;

namespace cowsins
{
    public class CameraEffects : MonoBehaviour
    {
        [SerializeField] private PlayerMovement player;
        [SerializeField] private Rigidbody playerRigidbody;

        [SerializeField] private float tiltSpeed, tiltAmount;
        [SerializeField] private float headBobAmplitude = 0.2f;
        [SerializeField] private float headBobFrequency = 2f;
        [SerializeField] private float breathingAmplitude = 0.2f;
        [SerializeField] private float breathingFrequency = 2f;
        [SerializeField] private bool applyBreathingRotation;

        private Vector3 origPos;
        private Quaternion origRot;

        private void Awake()
        {
            origPos = transform.localPosition;
            origRot = transform.localRotation;
        }

        private void Update()
        {
            if (!PlayerStats.Controllable) return;

            UpdateTilt();
            UpdateHeadBob();
            UpdateBreathing();
        }

        private void UpdateTilt()
        {
            if (player.currentSpeed == 0) return;

            Quaternion rot = CalculateTilt();
            transform.localRotation = Quaternion.Lerp(transform.localRotation, rot, Time.deltaTime * tiltSpeed);
        }

        private Quaternion CalculateTilt()
        {
            float x = InputManager.x;
            float y = InputManager.y;

            Vector3 vector = new Vector3(y, 0, -x).normalized * tiltAmount;

            return Quaternion.Euler(vector);
        }

        private void UpdateHeadBob()
        {
            if (playerRigidbody.velocity.magnitude < player.walkSpeed || InputManager.jumping)
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, origPos, Time.deltaTime * 2f);
                transform.localRotation = Quaternion.Lerp(transform.localRotation, origRot, Time.deltaTime * 2f);
                return;
            }

            float angle = Time.timeSinceLevelLoad * headBobFrequency;
            float distanceY = headBobAmplitude * Mathf.Sin(angle) / 400f;
            float distanceX = headBobAmplitude * Mathf.Cos(angle) / 100f;

            transform.position = new Vector3(transform.position.x, transform.position.y + Vector3.up.y * distanceY, transform.position.z);
            transform.Rotate(distanceX, 0, 0, Space.Self);
        }

        private void UpdateBreathing()
        {
            float angle = Time.timeSinceLevelLoad * breathingFrequency;
            float distance = breathingAmplitude * Mathf.Sin(angle) / 400f;
            float distanceRot = breathingAmplitude * Mathf.Cos(angle) / 100f;

            transform.position = new Vector3(transform.position.x, transform.position.y + Vector3.up.y * distance, transform.position.z);

            if (applyBreathingRotation)
            {
                transform.Rotate(distanceRot, 0, 0, Space.Self);
            }
        }
    }
}
