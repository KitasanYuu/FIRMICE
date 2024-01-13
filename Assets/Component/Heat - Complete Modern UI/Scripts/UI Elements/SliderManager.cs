using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;

namespace Michsky.UI.Heat
{
    [RequireComponent(typeof(Slider))]
    public class SliderManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        // Resources
        public Slider mainSlider;
        [SerializeField] private TextMeshProUGUI valueText;
        [SerializeField] private CanvasGroup highlightCG;

        // Saving
        public bool saveValue = false;
        public bool invokeOnAwake = true;
        public string saveKey = "My Slider";

        // Settings
        public bool isInteractable = true;
        public bool usePercent = false;
        public bool showValue = true;
        public bool showPopupValue = true;
        public bool useRoundValue = false;
        public bool useSounds = true;
        [Range(1, 15)] public float fadingMultiplier = 8;

        // Events
        [System.Serializable] public class SliderEvent : UnityEvent<float> { }
        [SerializeField] public SliderEvent onValueChanged = new SliderEvent();

        void Awake()
        {
            if (highlightCG == null) { highlightCG = new GameObject().AddComponent<CanvasGroup>(); highlightCG.gameObject.AddComponent<RectTransform>(); highlightCG.transform.SetParent(transform); highlightCG.gameObject.name = "Highlight"; }
            if (mainSlider == null) { mainSlider = gameObject.GetComponent<Slider>(); }
            if (gameObject.GetComponent<Image>() == null)
            {
                Image raycastImg = gameObject.AddComponent<Image>();
                raycastImg.color = new Color(0, 0, 0, 0);
                raycastImg.raycastTarget = true;
            }

            highlightCG.alpha = 0;
            highlightCG.gameObject.SetActive(false);
            float saveVal = mainSlider.value;

            if (saveValue == true)
            {
                if (PlayerPrefs.HasKey("Slider_" + saveKey) == true) { saveVal = PlayerPrefs.GetFloat("Slider_" + saveKey); }
                else { PlayerPrefs.SetFloat("Slider_" + saveKey, saveVal); }

                mainSlider.value = saveVal;
                mainSlider.onValueChanged.AddListener(delegate { PlayerPrefs.SetFloat("Slider_" + saveKey, mainSlider.value); });
            }

            mainSlider.onValueChanged.AddListener(delegate
            {
                onValueChanged.Invoke(mainSlider.value);
                UpdateUI();
            });

            if (invokeOnAwake == true) { onValueChanged.Invoke(mainSlider.value); }
            UpdateUI();
        }

        void Start()
        {
            if (UIManagerAudio.instance == null)
            {
                useSounds = false;
            }
        }

        public void Interactable(bool value)
        {
            isInteractable = value;
            mainSlider.interactable = isInteractable;
        }

        public void AddUINavigation()
        {
            Navigation customNav = new Navigation();
            customNav.mode = Navigation.Mode.Automatic;
            mainSlider.navigation = customNav;
        }

        public void UpdateUI()
        {
            if (valueText == null)
                return;

            if (useRoundValue == true)
            {
                if (usePercent == true && valueText != null) { valueText.text = Mathf.Round(mainSlider.value * 1.0f).ToString() + "%"; }
                else if (valueText != null) { valueText.text = Mathf.Round(mainSlider.value * 1.0f).ToString(); }
            }

            else
            {
                if (usePercent == true && valueText != null) { valueText.text = mainSlider.value.ToString("F1") + "%"; }
                else if (valueText != null) { valueText.text = mainSlider.value.ToString("F1"); }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (useSounds) { UIManagerAudio.instance.audioSource.PlayOneShot(UIManagerAudio.instance.UIManagerAsset.hoverSound); }
            if (isInteractable == false) { return; }

            StartCoroutine("SetHighlight");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (isInteractable == false)
                return;

            StartCoroutine("SetNormal");
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (isInteractable == false)
                return;

            StartCoroutine("SetHighlight");
        }

        public void OnDeselect(BaseEventData eventData)
        {
            if (isInteractable == false)
                return;

            StartCoroutine("SetNormal");
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
            highlightCG.gameObject.SetActive(false);
        }

        IEnumerator SetHighlight()
        {
            StopCoroutine("SetNormal");
            highlightCG.gameObject.SetActive(true);

            while (highlightCG.alpha < 0.99f)
            {
                highlightCG.alpha += Time.unscaledDeltaTime * fadingMultiplier;
                yield return null;
            }

            highlightCG.alpha = 1;
        }
    }
}