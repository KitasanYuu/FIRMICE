using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace Michsky.UI.Heat
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    public class ShopButtonManager : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, ISubmitHandler
    {
        // Content
        public State state = State.Default;
        public Sprite buttonIcon;
        public string buttonTitle = "Button";
        public string titleLocalizationKey;
        public string buttonDescription = "Description";
        public string descriptionLocalizationKey;
        public Sprite priceIcon;
        public string priceText = "123";
        public BackgroundFilter backgroundFilter;

        // Resources
        [SerializeField] private Animator animator;
        public ButtonManager purchaseButton;
        public ButtonManager purchasedButton;
        public GameObject purchasedIndicator;
        public ModalWindowManager purchaseModal;
        public Image iconObj;
        public Image priceIconObj;
        public TextMeshProUGUI titleObj;
        public TextMeshProUGUI descriptionObj;
        public TextMeshProUGUI priceObj;
        public Image filterObj;
        public List<Sprite> filters = new List<Sprite>();

        // Settings
        public bool isInteractable = true;
        public bool enableIcon = false;
        public bool enableTitle = true;
        public bool enableDescription = true;
        public bool enablePrice = true;
        public bool enableFilter = true;
        public bool useModalWindow = true;
        public bool useCustomContent = false;
        public bool useLocalization = true;
        public bool bypassUpdateOnEnable = false;
        public bool useUINavigation = false;
        public Navigation.Mode navigationMode = Navigation.Mode.Automatic;
        public GameObject selectOnUp;
        public GameObject selectOnDown;
        public GameObject selectOnLeft;
        public GameObject selectOnRight;
        public bool wrapAround = false;
        public bool useSounds = true;

        // Events
        public UnityEvent onPurchaseClick = new UnityEvent();
        public UnityEvent onPurchase = new UnityEvent();
        public UnityEvent onClick = new UnityEvent();
        public UnityEvent onHover = new UnityEvent();
        public UnityEvent onLeave = new UnityEvent();
        public UnityEvent onSelect = new UnityEvent();
        public UnityEvent onDeselect = new UnityEvent();

        // Helpers
        bool isInitialized = false;
        float cachedStateLength = 0.5f;
        Button targetButton;
#if UNITY_EDITOR
        public int latestTabIndex = 0;
#endif

        public enum State { Default, Purchased }

        public enum BackgroundFilter
        {
            Aqua,
            Dawn,
            Dusk,
            Emerald,
            Kylo,
            Memory,
            Mice,
            Pinky,
            Retro,
            Rock,
            Sunset,
            Violet,
            Warm,
            Random
        }

        void Awake()
        {
            cachedStateLength = HeatUIInternalTools.GetAnimatorClipLength(animator, "ShopButton_Highlighted") + 0.1f;
            InitializePurchaseEvents();
        }

        void OnEnable()
        {
            if (!isInitialized) { Initialize(); }
            if (!bypassUpdateOnEnable) { UpdateUI(); }
            if (Application.isPlaying && useUINavigation) { AddUINavigation(); }
            else if (Application.isPlaying && !useUINavigation && targetButton == null)
            {
                targetButton = gameObject.AddComponent<Button>();
                targetButton.transition = Selectable.Transition.None;
            }
        }

        void Initialize()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) { return; }
#endif
            if (ControllerManager.instance != null) { ControllerManager.instance.shopButtons.Add(this); }
            if (UIManagerAudio.instance == null) { useSounds = false; }
            if (animator == null) { animator = GetComponent<Animator>(); }
            if (GetComponent<Image>() == null)
            {
                Image raycastImg = gameObject.AddComponent<Image>();
                raycastImg.color = new Color(0, 0, 0, 0);
                raycastImg.raycastTarget = true;
            }

            TriggerAnimation("Start");

            if (useLocalization && !useCustomContent)
            {
                LocalizedObject mainLoc = GetComponent<LocalizedObject>();

                if (mainLoc == null || !mainLoc.CheckLocalizationStatus()) { useLocalization = false; }
                else
                {
                    if (titleObj != null && !string.IsNullOrEmpty(titleLocalizationKey))
                    {
                        LocalizedObject titleLoc = titleObj.gameObject.GetComponent<LocalizedObject>();
                        if (titleLoc != null)
                        {
                            titleLoc.tableIndex = mainLoc.tableIndex;
                            titleLoc.localizationKey = titleLocalizationKey;
                            titleLoc.UpdateItem();
                        }
                    }

                    if (descriptionObj != null && !string.IsNullOrEmpty(descriptionLocalizationKey))
                    {
                        LocalizedObject descLoc = descriptionObj.gameObject.GetComponent<LocalizedObject>();
                        if (descLoc != null)
                        {
                            descLoc.tableIndex = mainLoc.tableIndex;
                            descLoc.localizationKey = descriptionLocalizationKey;
                            descLoc.UpdateItem();
                        }
                    }

                    // Change button text on language change
                    mainLoc.onLanguageChanged.AddListener(delegate
                    {
                        buttonTitle = mainLoc.GetKeyOutput(titleLocalizationKey);
                        buttonDescription = mainLoc.GetKeyOutput(descriptionLocalizationKey);
                        UpdateUI();
                    });
                }
            }

            isInitialized = true;
        }

        public void UpdateUI()
        {
            if (enableIcon && iconObj != null) { iconObj.gameObject.SetActive(true); iconObj.sprite = buttonIcon; }
            else if (iconObj != null) { iconObj.gameObject.SetActive(false); }

            if (enableTitle && titleObj != null) { titleObj.gameObject.SetActive(true); titleObj.text = buttonTitle; }
            else if (titleObj != null) { titleObj.gameObject.SetActive(false); }

            if (enableDescription && descriptionObj != null) { descriptionObj.gameObject.SetActive(true); descriptionObj.text = buttonDescription; }
            else if (descriptionObj != null)
            {
                descriptionObj.gameObject.SetActive(false);
                if (Application.isPlaying && enableTitle && titleObj != null) { titleObj.transform.parent.gameObject.name = titleObj.gameObject.name + "_D"; }
            }

            if (!enablePrice && priceObj != null) { priceObj.transform.parent.gameObject.SetActive(false); }
            else if (enablePrice && priceIconObj != null)
            {
                if (priceObj != null) { priceObj.transform.parent.gameObject.SetActive(true); priceObj.text = priceText; }
                if (priceIconObj != null) { priceIconObj.sprite = priceIcon; }
            }

            if (!enableFilter && filterObj != null) { filterObj.gameObject.SetActive(false); }
            else if (enableFilter && filterObj != null && filters.Count > 1) 
            {
                filterObj.gameObject.SetActive(true);

                if (Application.isPlaying && backgroundFilter == BackgroundFilter.Random) { filterObj.sprite = filters[Random.Range(0, filters.Count - 1)]; }
                else if (filters.Count >= (int)backgroundFilter + 1) { filterObj.sprite = filters[(int)backgroundFilter]; }
            }

            UpdateState();

            if (!Application.isPlaying || !gameObject.activeInHierarchy) { return; }

            animator.enabled = true;
            animator.SetTrigger("Start");

            StopCoroutine("DisableAnimator");
            StartCoroutine("DisableAnimator");
        }

        public void UpdateState()
        {
            if (purchaseButton == null || purchasedButton == null || purchasedIndicator == null)
                return;

            if (state == State.Default)
            {
                purchaseButton.gameObject.SetActive(true);
                purchasedButton.gameObject.SetActive(false);
                purchasedIndicator.gameObject.SetActive(false);
            }

            else if (state == State.Purchased)
            {
                purchaseButton.gameObject.SetActive(false);
                purchasedButton.gameObject.SetActive(true);
                purchasedIndicator.gameObject.SetActive(true);
            }
        }

        public void SetState(State tempState)
        {
            state = tempState;
            UpdateState();
        }

        public void Purchase()
        {
            if (state == State.Purchased)
                return;

            SetState(State.Purchased);
            onPurchase.Invoke();
        }

        public void InitializePurchaseEvents()
        {
            if (purchaseButton == null)
            {
                Debug.LogError("<b>[Shop Button]</b> 'Purchase Button' is missing.", this);
                return;
            }

            purchaseButton.onClick.RemoveAllListeners();

            if (useModalWindow && purchaseModal != null)
            {
                onPurchaseClick.AddListener(delegate
                {
                    purchaseModal.onConfirm.RemoveAllListeners();
                    purchaseModal.onConfirm.AddListener(Purchase);
                    purchaseModal.onConfirm.AddListener(purchaseModal.CloseWindow);
                    purchaseModal.OpenWindow();
                });
            }

            purchaseButton.onClick.AddListener(onPurchaseClick.Invoke);
        }

        public void SetText(string text) { buttonTitle = text; UpdateUI(); }
        public void SetIcon(Sprite icon) { buttonIcon = icon; UpdateUI(); }
        public void SetPrice(string text) { priceText = text; UpdateUI(); }
        public void SetInteractable(bool value) { isInteractable = value; }

        public void AddUINavigation()
        {
            if (targetButton == null)
            {
                if (gameObject.GetComponent<Button>() == null) { targetButton = gameObject.AddComponent<Button>(); }
                else { targetButton = GetComponent<Button>(); }

                targetButton.transition = Selectable.Transition.None;
            }

            if (targetButton.navigation.mode == navigationMode)
                return;

            Navigation customNav = new Navigation();
            customNav.mode = navigationMode;

            if (navigationMode == Navigation.Mode.Vertical || navigationMode == Navigation.Mode.Horizontal) { customNav.wrapAround = wrapAround; }
            else if (navigationMode == Navigation.Mode.Explicit) { StartCoroutine("InitUINavigation", customNav); return; }

            targetButton.navigation = customNav;
        }

        public void DisableUINavigation()
        {
            if (targetButton != null)
            {
                Navigation customNav = new Navigation();
                Navigation.Mode navMode = Navigation.Mode.None;
                customNav.mode = navMode;
                targetButton.navigation = customNav;
            }
        }

        public void InvokeOnClick() 
        { 
            onClick.Invoke(); 
        }

        void TriggerAnimation(string triggername)
        {
            animator.enabled = true;

            animator.ResetTrigger("Start");
            animator.ResetTrigger("Normal");
            animator.ResetTrigger("Highlighted");

            animator.SetTrigger(triggername);

            StopCoroutine("DisableAnimator");
            StartCoroutine("DisableAnimator");
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isInteractable || eventData.button != PointerEventData.InputButton.Left) { return; }
            if (useSounds) { UIManagerAudio.instance.audioSource.PlayOneShot(UIManagerAudio.instance.UIManagerAsset.clickSound); }

            // Invoke click actions
            onClick.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isInteractable) { return; }
            if (useSounds) { UIManagerAudio.instance.audioSource.PlayOneShot(UIManagerAudio.instance.UIManagerAsset.hoverSound); }

            TriggerAnimation("Highlighted");
            onHover.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isInteractable)
                return;

            TriggerAnimation("Normal");
            onLeave.Invoke();
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (!isInteractable) { return; }
            if (useSounds) { UIManagerAudio.instance.audioSource.PlayOneShot(UIManagerAudio.instance.UIManagerAsset.hoverSound); }

            TriggerAnimation("Highlighted");
            onSelect.Invoke();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            if (!isInteractable)
                return;

            TriggerAnimation("Normal");
            onDeselect.Invoke();
        }

        public void OnSubmit(BaseEventData eventData)
        {
            if (!isInteractable) { return; }
            if (useSounds) { UIManagerAudio.instance.audioSource.PlayOneShot(UIManagerAudio.instance.UIManagerAsset.clickSound); }
            if (EventSystem.current.currentSelectedGameObject != gameObject) { TriggerAnimation("Normal"); }

            onClick.Invoke();
        }

        IEnumerator InitUINavigation(Navigation nav)
        {
            yield return new WaitForSecondsRealtime(0.1f);

            if (selectOnUp != null) { nav.selectOnUp = selectOnUp.GetComponent<Selectable>(); }
            if (selectOnDown != null) { nav.selectOnDown = selectOnDown.GetComponent<Selectable>(); }
            if (selectOnLeft != null) { nav.selectOnLeft = selectOnLeft.GetComponent<Selectable>(); }
            if (selectOnRight != null) { nav.selectOnRight = selectOnRight.GetComponent<Selectable>(); }
            
            targetButton.navigation = nav;
        }

        IEnumerator DisableAnimator()
        {
            yield return new WaitForSecondsRealtime(cachedStateLength);
            animator.enabled = false;
        }
    }
}