using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace Michsky.UI.Heat
{
    [RequireComponent(typeof(Animator))]
    public class NotificationManager : MonoBehaviour
    {
        // Content
        public Sprite icon;
        [TextArea] public string notificationText = "Notification Text";
        public string localizationKey;
        public AudioClip customSFX;

        // Resources
        [SerializeField] private Animator itemAnimator;
        [SerializeField] private Image iconObj;
        [SerializeField] private TextMeshProUGUI textObj;

        // Settings
        public bool useLocalization = true;
        [SerializeField] private bool updateOnAnimate = true;
        [Range(0, 10)] public float minimizeAfter = 3;
        public DefaultState defaultState = DefaultState.Minimized;
        public AfterMinimize afterMinimize = AfterMinimize.Disable;

        // Events
        public UnityEvent onDestroy = new UnityEvent();

        // Helpers
        bool isOn;
        LocalizedObject localizedObject;

        public enum DefaultState { Minimized, Expanded }
        public enum AfterMinimize { Disable, Destroy }

        void Start()
        {
            if (itemAnimator == null) { itemAnimator = GetComponent<Animator>(); }
            if (useLocalization)
            {
                localizedObject = textObj.GetComponent<LocalizedObject>();

                if (localizedObject == null || !localizedObject.CheckLocalizationStatus()) { useLocalization = false; }
                else if (localizedObject != null && !string.IsNullOrEmpty(localizationKey))
                {
                    // Forcing component to take the localized output on awake
                    notificationText = localizedObject.GetKeyOutput(localizationKey);

                    // Change text on language change
                    localizedObject.onLanguageChanged.AddListener(delegate
                    {
                        notificationText = localizedObject.GetKeyOutput(localizationKey);
                        UpdateUI();
                    });
                }
            }

            if (defaultState == DefaultState.Minimized) { gameObject.SetActive(false); }
            else if (defaultState == DefaultState.Expanded) { ExpandNotification(); }

            UpdateUI();
        }

        public void UpdateUI()
        {
            iconObj.sprite = icon;
            textObj.text = notificationText;
        }

        public void AnimateNotification()
        {
            ExpandNotification();
        }

        public void ExpandNotification()
        {
            if (isOn)
            {
                StopCoroutine("DisableAnimator");
                StartCoroutine("DisableAnimator");

                if (minimizeAfter != 0)
                {
                    StopCoroutine("MinimizeItem");
                    StartCoroutine("MinimizeItem");
                }

                return;
            }

            isOn = true;
            gameObject.SetActive(true);
            itemAnimator.enabled = true;
            itemAnimator.Play("In");

            if (updateOnAnimate) { UpdateUI(); }
            if (minimizeAfter != 0) { StopCoroutine("MinimizeItem"); StartCoroutine("MinimizeItem"); }

            if (customSFX != null && UIManagerAudio.instance != null) { UIManagerAudio.instance.audioSource.PlayOneShot(customSFX); }
            else if (UIManagerAudio.instance != null && UIManagerAudio.instance.UIManagerAsset.notificationSound != null) { UIManagerAudio.instance.audioSource.PlayOneShot(UIManagerAudio.instance.UIManagerAsset.notificationSound); }

            StopCoroutine("DisableAnimator");
            StartCoroutine("DisableAnimator");

            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        }

        public void MinimizeNotification()
        {
            if (!isOn)
                return;

            StopCoroutine("DisableAnimator");

            itemAnimator.enabled = true;
            itemAnimator.Play("Out");

            StopCoroutine("DisableItem");
            StartCoroutine("DisableItem");
        }

        public void DestroyNotification()
        {
            onDestroy.Invoke();
            Destroy(gameObject);
        }

        IEnumerator DisableAnimator()
        {
            yield return new WaitForSeconds(HeatUIInternalTools.GetAnimatorClipLength(itemAnimator, "Notification_In"));
            itemAnimator.enabled = false;
        }

        IEnumerator DisableItem()
        {
            yield return new WaitForSeconds(HeatUIInternalTools.GetAnimatorClipLength(itemAnimator, "Notification_Out"));

            isOn = false;

            if (afterMinimize == AfterMinimize.Disable) { gameObject.SetActive(false); }
            else if (afterMinimize == AfterMinimize.Destroy) { DestroyNotification(); }
        }

        IEnumerator MinimizeItem()
        {
            yield return new WaitForSeconds(minimizeAfter);
            MinimizeNotification();
        }
    }
}