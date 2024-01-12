using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;

namespace Michsky.UI.Heat
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CanvasGroup))]
    public class ModalWindowManager : MonoBehaviour
    {
        // Resources
        public Image windowIcon;
        public TextMeshProUGUI windowTitle;
        public TextMeshProUGUI windowDescription;
        public ButtonManager confirmButton;
        public ButtonManager cancelButton;
        [SerializeField] private Animator mwAnimator;

        // Content
        public Sprite icon;
        public string titleText = "Title";
        [TextArea(0, 4)] public string descriptionText = "Description here";

        // Localization
        public string titleKey;
        public string descriptionKey;

        // Settings
        public bool useCustomContent = false;
        public bool isOn = false;
        public bool closeOnCancel = true;
        public bool closeOnConfirm = true;
        public bool showCancelButton = true;
        public bool showConfirmButton = true;
        public bool useLocalization = true;
        [Range(0.5f, 2)] public float animationSpeed = 1;
        public StartBehaviour startBehaviour = StartBehaviour.Disable;
        public CloseBehaviour closeBehaviour = CloseBehaviour.Disable;
        public InputType inputType = InputType.Focused;

        // Events
        public UnityEvent onConfirm = new UnityEvent();
        public UnityEvent onCancel = new UnityEvent();
        public UnityEvent onOpen = new UnityEvent();
        public UnityEvent onClose = new UnityEvent();

        // Helpers
        string animIn = "In";
        string animOut = "Out";
        string animSpeedKey = "AnimSpeed";

        // Event System
        bool canProcessEventSystem;
        float openStateLength;
        float closeStateLength;
        GameObject latestEventSystemObject;

        public enum StartBehaviour { Enable, Disable }
        public enum CloseBehaviour { Disable, Destroy }
        public enum InputType { Focused, Free }

        void Awake()
        {
            InitModalWindow();
            InitEventSystem();
            UpdateUI();
        }

        void Start()
        {
            if (startBehaviour == StartBehaviour.Disable) { isOn = false; gameObject.SetActive(false); }
            else if (startBehaviour == StartBehaviour.Enable) { isOn = false; OpenWindow(); }
        }

        void Update()
        {
            if (inputType == InputType.Free || !isOn || !canProcessEventSystem || !ControllerManager.instance.gamepadEnabled)
                return;

            CheckForEventButtons();
        }

        void InitModalWindow()
        {
            if (mwAnimator == null) { mwAnimator = gameObject.GetComponent<Animator>(); }
            if (closeOnCancel) { onCancel.AddListener(CloseWindow); }
            if (closeOnConfirm) { onConfirm.AddListener(CloseWindow); }
            if (confirmButton != null) { confirmButton.onClick.AddListener(onConfirm.Invoke); }
            if (cancelButton != null) { cancelButton.onClick.AddListener(onCancel.Invoke); }
            if (useLocalization && !useCustomContent)
            {
                LocalizedObject mainLoc = GetComponent<LocalizedObject>();

                if (mainLoc == null || !mainLoc.CheckLocalizationStatus()) { useLocalization = false; }
                else
                {
                    if (windowTitle != null && !string.IsNullOrEmpty(titleKey))
                    {
                        LocalizedObject titleLoc = windowTitle.gameObject.GetComponent<LocalizedObject>();
                        if (titleLoc != null) 
                        { 
                            titleLoc.tableIndex = mainLoc.tableIndex; 
                            titleLoc.localizationKey = titleKey;
                            titleLoc.UpdateItem();
                        }
                    }

                    if (windowDescription != null && !string.IsNullOrEmpty(descriptionKey))
                    {
                        LocalizedObject descLoc = windowDescription.gameObject.GetComponent<LocalizedObject>();
                        if (descLoc != null) 
                        { 
                            descLoc.tableIndex = mainLoc.tableIndex; 
                            descLoc.localizationKey = descriptionKey;
                            descLoc.UpdateItem();
                        }
                    }
                }
            }

            openStateLength = HeatUIInternalTools.GetAnimatorClipLength(mwAnimator, "ModalWindow_In");
            closeStateLength = HeatUIInternalTools.GetAnimatorClipLength(mwAnimator, "ModalWindow_Out");
        }

        void InitEventSystem()
        {
            if (ControllerManager.instance == null) { canProcessEventSystem = false; }
            else if (cancelButton == null && confirmButton == null) { canProcessEventSystem = false; }
            else { canProcessEventSystem = true; }
        }

        void CheckForEventButtons()
        {
            if (cancelButton != null && EventSystem.current.currentSelectedGameObject != cancelButton.gameObject && EventSystem.current.currentSelectedGameObject != confirmButton.gameObject) { ControllerManager.instance.SelectUIObject(cancelButton.gameObject); }
            else if (confirmButton != null && EventSystem.current.currentSelectedGameObject != cancelButton.gameObject && EventSystem.current.currentSelectedGameObject != confirmButton.gameObject) { ControllerManager.instance.SelectUIObject(confirmButton.gameObject); }
        }

        public void UpdateUI()
        {
            if (!useCustomContent)
            {
                if (windowIcon != null) { windowIcon.sprite = icon; }
                if (windowTitle != null && (!useLocalization || string.IsNullOrEmpty(titleKey))) { windowTitle.text = titleText; }
                if (windowDescription != null && (!useLocalization || string.IsNullOrEmpty(titleKey))) { windowDescription.text = descriptionText; }
            }

            if (showCancelButton && cancelButton != null) { cancelButton.gameObject.SetActive(true); }
            else if (cancelButton != null) { cancelButton.gameObject.SetActive(false); }

            if (showConfirmButton && confirmButton != null) { confirmButton.gameObject.SetActive(true); }
            else if (confirmButton != null) { confirmButton.gameObject.SetActive(false); }
        }

        public void OpenWindow()
        {
            if (isOn) { return; }
            if (EventSystem.current.currentSelectedGameObject != null) { latestEventSystemObject = EventSystem.current.currentSelectedGameObject; }

            gameObject.SetActive(true);
            isOn = true;

            StopCoroutine("DisableObject");
            StopCoroutine("DisableAnimator");
            StartCoroutine("DisableAnimator");

            mwAnimator.enabled = true;
            mwAnimator.SetFloat(animSpeedKey, animationSpeed);
            mwAnimator.Play(animIn);
            onOpen.Invoke();
        }

        public void CloseWindow()
        {
            if (!isOn)
                return;

            if (gameObject.activeSelf == true)
            {
                StopCoroutine("DisableObject");
                StopCoroutine("DisableAnimator");
                StartCoroutine("DisableObject");
            }

            isOn = false;
            mwAnimator.enabled = true;
            mwAnimator.SetFloat(animSpeedKey, animationSpeed);
            mwAnimator.Play(animOut);
            onClose.Invoke();

            if (ControllerManager.instance != null && latestEventSystemObject != null && latestEventSystemObject.activeInHierarchy)
            {
                ControllerManager.instance.SelectUIObject(latestEventSystemObject);
            }
        }

        public void AnimateWindow()
        {
            if (!isOn) { OpenWindow(); }
            else { CloseWindow(); }
        }

        IEnumerator DisableObject()
        {
            yield return new WaitForSecondsRealtime(closeStateLength);

            if (closeBehaviour == CloseBehaviour.Disable) { gameObject.SetActive(false); }
            else if (closeBehaviour == CloseBehaviour.Destroy) { Destroy(gameObject); }

            mwAnimator.enabled = false;
        }

        IEnumerator DisableAnimator()
        {
            yield return new WaitForSecondsRealtime(openStateLength + 0.1f);
            mwAnimator.enabled = false;
        }
    }
}