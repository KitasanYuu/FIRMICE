using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Michsky.UI.Heat
{
    [DisallowMultipleComponent]
    public class SettingsElement : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, ISubmitHandler
    {
        // Resources
        [SerializeField] private CanvasGroup highlightCG;

        // Settings
        public bool isInteractable = true;
        public bool useSounds = true;
        [Range(1, 15)] public float fadingMultiplier = 8;
        public bool useUINavigation = false;
        public Navigation.Mode navigationMode = Navigation.Mode.Automatic;
        public GameObject selectOnUp;
        public GameObject selectOnDown;
        public GameObject selectOnLeft;
        public GameObject selectOnRight;
        public bool wrapAround = false;

        // Events
        public UnityEvent onClick = new UnityEvent();
        public UnityEvent onHover = new UnityEvent();
        public UnityEvent onLeave = new UnityEvent();

        // Helpers
        Button targetButton;

        void Start()
        {
            if (ControllerManager.instance != null) { ControllerManager.instance.settingsElements.Add(this); }
            if (UIManagerAudio.instance == null) { useSounds = false; }
         
            if (highlightCG == null)
            {
                highlightCG = new GameObject().AddComponent<CanvasGroup>();
                highlightCG.gameObject.AddComponent<RectTransform>();
                highlightCG.transform.SetParent(transform);
                highlightCG.gameObject.name = "Highlight";
            }

            if (GetComponent<Image>() == null)
            {
                Image raycastImg = gameObject.AddComponent<Image>();
                raycastImg.color = new Color(0, 0, 0, 0);
                raycastImg.raycastTarget = true;
            }
        }

        void OnEnable()
        {
            if (highlightCG != null) { highlightCG.alpha = 0; }
            if (Application.isPlaying && useUINavigation) { AddUINavigation(); }
            else if (Application.isPlaying && !useUINavigation && targetButton == null)
            {
                targetButton = gameObject.AddComponent<Button>();
                targetButton.transition = Selectable.Transition.None;
            }
        }

        public void Interactable(bool value)
        {
            isInteractable = value;
            if (!gameObject.activeInHierarchy) { return; }
            StartCoroutine("SetNormal");
        }

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

        public void OnPointerClick(PointerEventData eventData)
        {
            if (isInteractable == false || eventData.button != PointerEventData.InputButton.Left) { return; }
            if (useSounds) { UIManagerAudio.instance.audioSource.PlayOneShot(UIManagerAudio.instance.UIManagerAsset.clickSound); }

            onClick.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isInteractable) { return; }
            if (useSounds) { UIManagerAudio.instance.audioSource.PlayOneShot(UIManagerAudio.instance.UIManagerAsset.hoverSound); }

            onHover.Invoke();
            StartCoroutine("SetHighlight");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isInteractable)
                return;

            onLeave.Invoke();
            StartCoroutine("SetNormal");
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (!isInteractable) { return; }
            if (useSounds) { UIManagerAudio.instance.audioSource.PlayOneShot(UIManagerAudio.instance.UIManagerAsset.hoverSound); }

            onHover.Invoke();
            StartCoroutine("SetHighlight");
        }

        public void OnDeselect(BaseEventData eventData)
        {
            if (!isInteractable || highlightCG == null)
                return;

            onLeave.Invoke();
            StartCoroutine("SetNormal");
        }

        public void OnSubmit(BaseEventData eventData)
        {
            if (!isInteractable) { return; }
            if (useSounds) { UIManagerAudio.instance.audioSource.PlayOneShot(UIManagerAudio.instance.UIManagerAsset.clickSound); }

            onClick.Invoke();
        }

        IEnumerator SetNormal()
        {
            StopCoroutine("SetHighlight");

            while (highlightCG.alpha > 0.01f)
            {
                highlightCG.alpha -= Time.unscaledDeltaTime * fadingMultiplier;
                yield return null;
            }

            highlightCG.alpha = 0;
        }

        IEnumerator SetHighlight()
        {
            StopCoroutine("SetNormal");

            while (highlightCG.alpha < 0.99f)
            {
                highlightCG.alpha += Time.unscaledDeltaTime * fadingMultiplier;
                yield return null;
            }

            highlightCG.alpha = 1;
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
    }
}