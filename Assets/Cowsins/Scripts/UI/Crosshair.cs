/// <summary>
/// This script belongs to cowsins™ as a part of the cowsins´ FPS Engine. All rights reserved. 
/// </summary>
using UnityEngine;

namespace cowsins
{

    public class Crosshair : MonoBehaviour
    {
        #region variables

        [Tooltip("Attach your PlayerMovement player "), SerializeField]
        private PlayerMovement player;

        private PlayerStats playerStats;

        private WeaponController weaponController;

        private InteractManager interactManager;

        private CrosshairShape crosshairShape;

        private Rigidbody rb;

        [Header("Variables")]

        [SerializeField, Tooltip("If enabled, the crosshair will not be displayed when the game is paused.")] private bool hideCrosshairOnPaused;

        [SerializeField, Tooltip("If enabled, the crosshair will not be displayed when the player is inspecting.")] private bool hideCrosshairOnInspecting;

        [Tooltip(" How much space it takes from your screen"), SerializeField]
        private float size = 10f;

        [Tooltip(" Thickness of the crosshair  "), SerializeField]
        private float width = 2f;

        private float originalWidth;

        public float enemySpottedWidth;

        [Tooltip(" Original spread you want to start with "), SerializeField]
        private float defaultSpread = 10f;

        [SerializeField] private float walkSpread, runSpread, crouchSpread, jumpSpread;

        private Color color = Color.grey;

        [Tooltip(" Crosshair Color "), SerializeField]
        private Color defaultColor;

        [Tooltip(" Color of the crosshair whenever you aim at an enemy "), SerializeField]
        private Color enemySpottedColor;

        [SerializeField] private float resizeSpeed = 3f;

        [HideInInspector]
        public float spread;

        [Header("Hitmarker")]

        [SerializeField] private bool hitmarker;

        [SerializeField] private GameObject hitmarkerObj;

        #endregion

        private void Awake()
        {
            ResetCrosshair();

            playerStats = player.GetComponent<PlayerStats>();
            weaponController = player.GetComponent<WeaponController>();
            interactManager = player.GetComponent<InteractManager>();
            rb = player.GetComponent<Rigidbody>(); 

            crosshairShape = GetComponent<CrosshairShape>(); 
        }

        private void Update()
        {
            // If we are shooting do not continue
            if (InputManager.shooting && player.weapon != null && player.weapon.canShoot)
                if (spread != defaultSpread) spread = Mathf.Lerp(spread, defaultSpread, resizeSpeed * Time.deltaTime / 10); // if this is not the current spread, fall back to the original one

            // Manage different sizes
            if (player.grounded)
            {
                if (player.currentSpeed == player.runSpeed) Resize(runSpread);
                else
                {
                    if (player.currentSpeed == player.walkSpeed)
                    {
                        if (rb.velocity.magnitude < .2f) Resize(defaultSpread);
                        else Resize(walkSpread);
                    }

                    if (player.currentSpeed == player.crouchSpeed) Resize(crouchSpread);
                }
            }
            else Resize(jumpSpread);
        }

        private void ResetCrosshair()
        {
            spread = defaultSpread;
            color = defaultColor;
            originalWidth = width;
        }

        /// <summary>
        /// Resize the crosshair to a new value.
        /// </summary>
        public void Resize(float newSize) => spread = Mathf.Lerp(spread, newSize, resizeSpeed * Time.deltaTime);
        /// <summary>
        /// Change color of the crosshair
        /// </summary>
        public void SpotEnemy(bool condition)
        {
            color = (condition) ? enemySpottedColor : defaultColor;
            width = (condition) ? Mathf.Lerp(width, enemySpottedWidth, resizeSpeed) : Mathf.Lerp(width, originalWidth, resizeSpeed);
        }

        /// <summary>
        /// Draw the crosshair as our UI
        /// </summary>
        void OnGUI()
        {
            if (playerStats.isDead
                || weaponController.weapon != null && weaponController.isAiming && player.weapon.removeCrosshairOnAiming
                || PauseMenu.Instance != null && PauseMenu.isPaused && hideCrosshairOnPaused
                || interactManager.inspecting && hideCrosshairOnInspecting) return;
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.Apply();


            if (crosshairShape.parts.downPart) GUI.DrawTexture(new Rect(Screen.width / 2 - width / 2, (Screen.height / 2 - size / 2) + spread / 2, width, size), texture);

            if (crosshairShape.parts.topPart) GUI.DrawTexture(new Rect(Screen.width / 2 - width / 2, (Screen.height / 2 - size / 2) - spread / 2, width, size), texture);

            if (crosshairShape.parts.rightPart) GUI.DrawTexture(new Rect((Screen.width / 2 - size / 2) + spread / 2, Screen.height / 2 - width / 2, size, width), texture);

            if (crosshairShape.parts.leftPart) GUI.DrawTexture(new Rect((Screen.width / 2 - size / 2) - spread / 2, Screen.height / 2 - width / 2, size, width), texture);

            if (crosshairShape.parts.center)
            {
                float radius = Mathf.Min(width, size) / 2;
                Vector2 center = new Vector2(Screen.width / 2, Screen.height / 2);
                Rect circleRect = new Rect(center.x - radius, center.y - radius, radius * 2, radius * 2);

                GUI.DrawTexture(circleRect, texture);
            }
        }
    }
}