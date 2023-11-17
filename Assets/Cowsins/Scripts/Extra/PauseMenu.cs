using UnityEngine;

namespace cowsins
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private GameObject playerUI;

        [SerializeField] private bool disablePlayerUIWhilePaused;  
        public static PauseMenu Instance { get; private set; }

        public static bool isPaused { get; private set; }

        [HideInInspector]public PlayerStats stats;

        [SerializeField] private CanvasGroup menu;

        [SerializeField] private float fadeSpeed;

        private void Awake()
        {
            if (Instance != null && Instance != this) Destroy(this);
            else Instance = this;

            isPaused = false;
            menu.gameObject.SetActive(false);
            menu.alpha = 0;
        }
        
        private void Update()
        {
            if (InputManager.pausing) isPaused = !isPaused;

            if (isPaused)
            {
                stats.LoseControl();
                if (!menu.gameObject.activeSelf)
                {
                    menu.gameObject.SetActive(true);
                    menu.alpha = 0; 
                }
                if (menu.alpha < 1) menu.alpha += Time.deltaTime * fadeSpeed;

                if(disablePlayerUIWhilePaused)playerUI.SetActive(false); 
            }
            else
            {
                menu.alpha -= Time.deltaTime * fadeSpeed;
                if (menu.alpha <= 0) menu.gameObject.SetActive(false);
            }
        }

        public void UnPause()
        {
            isPaused = false;
            stats.CheckIfCanGrantControl();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            playerUI.SetActive(true);
        }

        public void QuitGame() => Application.Quit();

        public void TogglePause()
        {
            isPaused = !isPaused;
            if (isPaused)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                if (disablePlayerUIWhilePaused) playerUI.SetActive(false);
            }
            else
            {
                stats.CheckIfCanGrantControl();
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                playerUI.SetActive(true);
            }
        }

    }
}
