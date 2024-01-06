using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using TMPro;
using UnityEngine.Networking;
#if !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace Michsky.LSS
{
    [RequireComponent(typeof(CanvasGroup))]
    [AddComponentMenu("Loading Screen Studio/LSS Loading Screen")]
    public class LSS_LoadingScreen : MonoBehaviour
    {
        public static LSS_LoadingScreen instance = null;

        #region Customization
        public float titleSize = 50;
        public float descriptionSize = 28;
        public float hintSize = 32;
        public float statusSize = 24;
        public float pakSize = 35;
        public TMP_FontAsset titleFont;
        public TMP_FontAsset descriptionFont;
        public TMP_FontAsset hintFont;
        public TMP_FontAsset statusFont;
        public TMP_FontAsset pakFont;
        public Color titleColor = Color.white;
        public Color descriptionColor = Color.white;
        public Color hintColor = Color.white;
        public Color spinnerColor = Color.white;
        public Color statusColor = Color.white;
        public Color pakColor = Color.white;
        [TextArea] public string titleObjText = "Title";
        [TextArea] public string titleObjDescText = "Description";
        [TextArea] public string pakText = "Press {KEY} to continue";
        #endregion

        #region Resources
        public CanvasGroup canvasGroup;
        public CanvasGroup backgroundCanvasGroup;
        public CanvasGroup contentCanvasGroup;
        public CanvasGroup pakCanvasGroup;
        public TextMeshProUGUI statusObj;
        public TextMeshProUGUI titleObj;
        public TextMeshProUGUI descriptionObj;
        public Slider progressBar;
        public Sprite backgroundImage;
        public Transform spinnerParent;
        #endregion

        #region Hints
        [SerializeField] private bool enableRandomHints = true;
        public TextMeshProUGUI hintsText;
        public bool changeHintWithTimer;
        [Range(1, 60)] public float hintTimerValue = 5;
        [TextArea] public List<string> hintList = new List<string>();
        int currentHintIndex = 0;
        #endregion

        #region Background Images
        [SerializeField] private bool enableRandomImages = true;
        public Image imageObject;
        public List<Sprite> imageList = new List<Sprite>();
        public bool changeImageWithTimer;
        public float imageTimerValue = 5;
        [Range(0.1f, 10)] public float imageFadingSpeed = 4;
        int currentImageIndex = 0;
        #endregion

        #region Press Any Key
        public TextMeshProUGUI pakTextObj;
        public TextMeshProUGUI pakCountdownLabel;
        public Slider pakCountdownSlider;
        public bool useSpecificKey = false;
        public bool useCountdown = true;
        public bool waitForPlayerInput = false;
        [Range(1, 30)] public int pakCountdownTimer = 5;
#if ENABLE_LEGACY_INPUT_MANAGER
        public KeyCode keyCode = KeyCode.Space;
#elif ENABLE_INPUT_SYSTEM
        public InputAction keyCode;
#endif
        #endregion

        #region Virtual Loading
        public bool enableVirtualLoading = false;
        public float virtualLoadingTimer = 5;
        public float currentVirtualTime;
        #endregion

        #region Settings
        [SerializeField] private bool setTimeScale = true;
        [SerializeField] private bool enablePressAnyKey = true;
        [SerializeField] private bool customSceneActivation = false;
        [Range(0.25f, 10)] public float fadeSpeed = 4;
        [Range(0.25f, 10)] public float backgroundFadeSpeed = 2;
        [Range(0.25f, 10)] public float contentFadeSpeed = 2;
        #endregion

        #region Cam Mirroring
        [SerializeField] private Camera projectorCamera;
        RenderTexture projectorRT;
        RawImage projectorImage;
        CanvasGroup projectorCG;
        #endregion

        #region Events
        public UnityEvent onLoadingStart = new UnityEvent();
        public UnityEvent onTransitionCompleted = new UnityEvent();
        public UnityEvent onLoadingEnd = new UnityEvent();
        public UnityEvent onLoadingDestroy = new UnityEvent();
        #endregion

        #region Helpers
        // Coroutine States
        [HideInInspector] public bool isFadeInRunning = false;
        [HideInInspector] public bool isFadeInCompleted = false;

        [HideInInspector] public bool isFadeOutRunning = false;
        [HideInInspector] public bool isFadeOutCompleted = false;

        [HideInInspector] public bool isBGFadeInRunning = false;
        [HideInInspector] public bool isBGFadeInCompleted = false;

        [HideInInspector] public bool isBGFadeOutRunning = false;
        [HideInInspector] public bool isBGFadeOutCompleted = false;

        [HideInInspector] public bool isContentFadeInRunning = false;
        [HideInInspector] public bool isContentFadeInCompleted = false;

        [HideInInspector] public bool isContentFadeOutRunning = false;
        [HideInInspector] public bool isContentFadeOutCompleted = false;

        [HideInInspector] public bool isPAKFadeInRunning = false;
        [HideInInspector] public bool isPAKFadeInCompleted = false;

        [HideInInspector] public bool isPAKFadeOutRunning = false;
        [HideInInspector] public bool isPAKFadeOutCompleted;

        // Other Helpers
        public static string presetName = "Default";
        public int spinnerHelper;
        public bool updateHelper = false;
        bool processLoading = false;
        #endregion

        void Awake()
        {
            if (canvasGroup == null) { canvasGroup = gameObject.GetComponent<CanvasGroup>(); }
            if (backgroundCanvasGroup == null) { backgroundCanvasGroup = new GameObject().AddComponent<CanvasGroup>(); backgroundCanvasGroup.transform.SetParent(transform); backgroundCanvasGroup.gameObject.name = "[B]"; }
            if (contentCanvasGroup == null) { contentCanvasGroup = new GameObject().AddComponent<CanvasGroup>(); contentCanvasGroup.transform.SetParent(transform); contentCanvasGroup.gameObject.name = "[C]"; }
            if (pakCanvasGroup == null) { pakCanvasGroup = new GameObject().AddComponent<CanvasGroup>(); pakCanvasGroup.transform.SetParent(transform); pakCanvasGroup.gameObject.name = "[P]"; }
            if (projectorCamera != null)
            {
                projectorRT = new RenderTexture(Screen.currentResolution.width, Screen.currentResolution.height, 24, RenderTextureFormat.RGB111110Float);
                projectorCamera.targetTexture = projectorRT;

                GameObject tempObj = new GameObject();
                tempObj.transform.SetParent(contentCanvasGroup.transform);

                RectTransform tempRect = tempObj.AddComponent<RectTransform>();
                tempRect.anchorMin = new Vector2(0, 0);
                tempRect.anchorMax = new Vector2(1, 1);
                tempRect.anchoredPosition = new Vector2(0, 0);

                projectorImage = tempRect.gameObject.AddComponent<RawImage>();
                projectorImage.texture = projectorRT;
                projectorImage.transform.SetAsFirstSibling();

                projectorCG = projectorImage.gameObject.AddComponent<CanvasGroup>();
            }

            canvasGroup.alpha = 0f;
            backgroundCanvasGroup.alpha = 0;
            contentCanvasGroup.alpha = 0;
            pakCanvasGroup.alpha = 0;

#if !ENABLE_LEGACY_INPUT_MANAGER
            keyCode.Enable();
#endif
        }

        void Start()
        {
            if (enableRandomHints && hintsText != null) { hintsText.text = GetRandomHint(); StartCoroutine("ProcessRandomHint"); }
            else if (!enableRandomHints && hintsText != null && hintList.Count > 0) { hintsText.text = hintList[currentHintIndex]; }

            if (enableRandomImages) { ProcessRandomImages(); }
            else if (imageObject != null) { imageObject.sprite = backgroundImage; }

            if (useCountdown && pakCountdownSlider != null && pakCountdownLabel != null)
            {
                pakCountdownSlider.maxValue = pakCountdownTimer;
                pakCountdownSlider.value = pakCountdownTimer;
                pakCountdownLabel.text = Mathf.Round(pakCountdownSlider.value * 1).ToString();
            }
           
            else if (!useCountdown && pakCountdownSlider != null) 
            {
                pakCountdownSlider.gameObject.SetActive(false); 
            }

            if (statusObj != null) { statusObj.text = "0%"; }
            if (progressBar != null) { progressBar.value = 0; }
        }

        public AsyncOperation loadingProcess = new AsyncOperation();

        public static void LoadScene(string targetScene, string targetPrefab)
        {
            presetName = targetPrefab;
            LoadScene(targetScene);
        }

        public static void LoadScene(string targetScene)
        {
            try
            {
                instance = Instantiate(Resources.Load<GameObject>("Loading Screens/" + presetName).GetComponent<LSS_LoadingScreen>());
                instance.gameObject.SetActive(true);
                DontDestroyOnLoad(instance.gameObject);

                if (instance.setTimeScale) { Time.timeScale = 1; }
                if (instance.customSceneActivation) { instance.enablePressAnyKey = false; instance.waitForPlayerInput = false; }

                instance.processLoading = true;
                instance.loadingProcess = SceneManager.LoadSceneAsync(targetScene);
                instance.loadingProcess.allowSceneActivation = false;
                instance.onLoadingStart.Invoke();
            }

            catch
            {
                Debug.LogError("<b><color=orange>[LSS]</color></b> Cannot initalize the loading screen because either <b><color=orange>'" +
                    targetScene + "'</color></b> scene has not been added to the build window, or <b><color=orange>'" + presetName
                    + "'</color></b> prefab cannot be found in <b>Resources/Loading Screens</b>.");
                instance.processLoading = false;

                if (instance != null)
                {
                    Destroy(instance.gameObject);
                }
            }
        }

        public static void LoadSceneAdditive(string targetScene, string targetPrefab)
        {
            presetName = targetPrefab;
            LoadSceneAdditive(targetScene);
        }

        public static void LoadSceneAdditive(string targetScene)
        {
            try
            {
                instance = Instantiate(Resources.Load<GameObject>("Loading Screens/" + presetName).GetComponent<LSS_LoadingScreen>());
                instance.gameObject.SetActive(true);
                DontDestroyOnLoad(instance.gameObject);

                if (instance.setTimeScale) { Time.timeScale = 1; }

                instance.canvasGroup.alpha = 0f;
                instance.backgroundCanvasGroup.alpha = 0;
                instance.contentCanvasGroup.alpha = 0;
                instance.pakCanvasGroup.alpha = 0;

                instance.processLoading = true;
                instance.loadingProcess = SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Additive);
                instance.loadingProcess.allowSceneActivation = false;
                instance.onLoadingStart.Invoke();
            }

            catch
            {
                Debug.LogError("<b><color=orange>[LSS]</color></b> Cannot initalize the loading screen because either <b><color=orange>'" +
                    targetScene + "'</color></b> scene has not been added to the build window, or <b><color=orange>'" + presetName
                    + "'</color></b> prefab cannot be found in <b>Resources/Loading Screens</b>.");
                instance.processLoading = false;

                if (instance != null)
                {
                    Destroy(instance.gameObject);
                }
            }
        }

        public static void PerformVirtualTransition(float transitionDuration)
        {
            instance = Instantiate(Resources.Load<GameObject>("Loading Screens/" + presetName).GetComponent<LSS_LoadingScreen>());
            instance.gameObject.SetActive(true);
            DontDestroyOnLoad(instance.gameObject);

            if (instance.setTimeScale) { Time.timeScale = 1; }

            instance.StopCoroutine("FadeInLoadingScreen");
            instance.StartCoroutine("FadeInLoadingScreen");
            instance.StartCoroutine("HandleVirtualTransition", transitionDuration);
        }

        public static void EndVirtualTransition()
        {
            instance.StopCoroutine("HandleVirtualTransition");
            instance.StartCoroutine("FadeOutContentScreen", true);
        }

        void Update()
        {
            if (!enableVirtualLoading) { ProcessLoading(); }
            else { ProcessVirtualLoading(); }
        }

        void ProcessLoading()
        {
            if (!processLoading)
                return;

            if (!customSceneActivation && !loadingProcess.allowSceneActivation && isContentFadeInCompleted && !enablePressAnyKey) { loadingProcess.allowSceneActivation = true; }
            else if (!loadingProcess.allowSceneActivation && isContentFadeInCompleted && enablePressAnyKey && !waitForPlayerInput) { loadingProcess.allowSceneActivation = true; }

            if (progressBar != null) { progressBar.value = Mathf.Lerp(progressBar.value, loadingProcess.progress, 0.1f); }
            if (statusObj != null && progressBar != null) { statusObj.text = Mathf.Round(progressBar.value * 100).ToString() + "%"; }

            if (canvasGroup.alpha == 0 && !isFadeInRunning && !isFadeInCompleted)
            {
                StopCoroutine("FadeInLoadingScreen");
                StartCoroutine("FadeInLoadingScreen");
            }

            else if (customSceneActivation && loadingProcess.progress == 0.9f && isContentFadeInCompleted)
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;

                StopCoroutine("FadeOutBackgroundScreen");
                StartCoroutine("FadeOutBackgroundScreen");

                StopCoroutine("FadeOutContentScreen");
                StartCoroutine("FadeOutContentScreen", true);
            }

            else if (!enablePressAnyKey && loadingProcess.isDone && loadingProcess.allowSceneActivation && isContentFadeInCompleted)
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;

                StopCoroutine("FadeOutBackgroundScreen");
                StartCoroutine("FadeOutBackgroundScreen");

                StopCoroutine("FadeOutContentScreen");
                StartCoroutine("FadeOutContentScreen", true);
            }

            else if (enablePressAnyKey && waitForPlayerInput && loadingProcess.progress == 0.9f)
            {
                if (!isPAKFadeInRunning && !isPAKFadeInCompleted && isContentFadeInCompleted)
                {
                    StopCoroutine("FadeOutContentScreen");
                    StartCoroutine("FadeOutContentScreen", false);

                    StopCoroutine("FadeInPAKScreen");
                    StartCoroutine("FadeInPAKScreen");
                }

                else if (isPAKFadeInCompleted && useCountdown)
                {
                    pakCountdownSlider.value -= Time.unscaledDeltaTime;
                    pakCountdownLabel.text = Mathf.Round(pakCountdownSlider.value * 1).ToString();

                    if (pakCountdownSlider.value == 0)
                    {
                        loadingProcess.allowSceneActivation = true;
                        canvasGroup.interactable = false;
                        canvasGroup.blocksRaycasts = false;

                        StopCoroutine("FadeOutBackgroundScreen");
                        StartCoroutine("FadeOutBackgroundScreen");

                        StopCoroutine("FadeOutPAKScreen");
                        StartCoroutine("FadeOutPAKScreen", true);
                    }
                }

#if ENABLE_LEGACY_INPUT_MANAGER
                if (!useSpecificKey && Input.anyKeyDown && !isPAKFadeInRunning && isPAKFadeInCompleted)
#elif ENABLE_INPUT_SYSTEM
                if (!useSpecificKey && Keyboard.current.anyKey.wasPressedThisFrame && !isPAKFadeInRunning && isPAKFadeInCompleted)
#endif
                {
                    loadingProcess.allowSceneActivation = true;
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;

                    StopCoroutine("FadeOutBackgroundScreen");
                    StartCoroutine("FadeOutBackgroundScreen");

                    StopCoroutine("FadeOutPAKScreen");
                    StartCoroutine("FadeOutPAKScreen", true);
                }

#if ENABLE_LEGACY_INPUT_MANAGER
                else if (useSpecificKey && Input.GetKeyDown(keyCode) && !isPAKFadeInRunning && isPAKFadeInCompleted)
#elif ENABLE_INPUT_SYSTEM
                else if (useSpecificKey && keyCode.triggered && !isPAKFadeInRunning && isPAKFadeInCompleted)
#endif
                {
                    loadingProcess.allowSceneActivation = true;
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;

                    StopCoroutine("FadeOutBackgroundScreen");
                    StartCoroutine("FadeOutBackgroundScreen");

                    StopCoroutine("FadeOutPAKScreen");
                    StartCoroutine("FadeOutPAKScreen", true);
                }
            }

            else if (enablePressAnyKey && !waitForPlayerInput && loadingProcess.isDone)
            {
                if (!isPAKFadeInRunning && !isPAKFadeInCompleted)
                {
                    StopCoroutine("FadeInPAKScreen");
                    StartCoroutine("FadeInPAKScreen");
                }

                else if (useCountdown)
                {
                    pakCountdownSlider.value -= Time.unscaledDeltaTime;
                    pakCountdownLabel.text = Mathf.Round(pakCountdownSlider.value * 1).ToString();

                    if (pakCountdownSlider.value == 0)
                    {
                        canvasGroup.interactable = false;
                        canvasGroup.blocksRaycasts = false;

                        StopCoroutine("FadeOutBackgroundScreen");
                        StartCoroutine("FadeOutBackgroundScreen");

                        StopCoroutine("FadeOutPAKScreen");
                        StartCoroutine("FadeOutPAKScreen", true);
                    }
                }

#if ENABLE_LEGACY_INPUT_MANAGER
                if (!useSpecificKey && Input.anyKeyDown && !isPAKFadeInRunning && isPAKFadeInCompleted)
#elif ENABLE_INPUT_SYSTEM
                if (!useSpecificKey && Keyboard.current.anyKey.wasPressedThisFrame && !isPAKFadeInRunning && isPAKFadeInCompleted)
#endif
                {
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;

                    StopCoroutine("FadeOutBackgroundScreen");
                    StartCoroutine("FadeOutBackgroundScreen");

                    StopCoroutine("FadeOutPAKScreen");
                    StartCoroutine("FadeOutPAKScreen", true);
                }

#if ENABLE_LEGACY_INPUT_MANAGER
                else if (useSpecificKey && Input.GetKeyDown(keyCode) && !isPAKFadeInRunning && isPAKFadeInCompleted)
#elif ENABLE_INPUT_SYSTEM
                else if (useSpecificKey && keyCode.triggered && !isPAKFadeInRunning && isPAKFadeInCompleted)
#endif
                {
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;

                    StopCoroutine("FadeOutBackgroundScreen");
                    StartCoroutine("FadeOutBackgroundScreen");

                    StopCoroutine("FadeOutPAKScreen");
                    StartCoroutine("FadeOutPAKScreen", true);
                }
            }
        }

        void ProcessVirtualLoading()
        {
            if (!processLoading)
                return;

            if (progressBar != null) { progressBar.value += 1 / virtualLoadingTimer * Time.unscaledDeltaTime; }
            if (statusObj != null && progressBar != null) { statusObj.text = Mathf.Round(progressBar.value * 100).ToString() + "%"; }

            currentVirtualTime += Time.unscaledDeltaTime;

            if (canvasGroup.alpha == 0 && !isFadeInRunning && !isFadeInCompleted)
            {
                StopCoroutine("FadeInLoadingScreen");
                StartCoroutine("FadeInLoadingScreen");
            }

            if (currentVirtualTime >= virtualLoadingTimer)
            {
                if (!customSceneActivation && !loadingProcess.allowSceneActivation && isContentFadeInCompleted && !enablePressAnyKey) { loadingProcess.allowSceneActivation = true; }
                else if (!loadingProcess.allowSceneActivation && isContentFadeInCompleted && enablePressAnyKey && !waitForPlayerInput) { loadingProcess.allowSceneActivation = true; }

                if (customSceneActivation && loadingProcess.progress == 0.9f && isContentFadeInCompleted)
                {
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;

                    StopCoroutine("FadeOutBackgroundScreen");
                    StartCoroutine("FadeOutBackgroundScreen");

                    StopCoroutine("FadeOutContentScreen");
                    StartCoroutine("FadeOutContentScreen", true);
                }

                else if (!enablePressAnyKey && loadingProcess.isDone && isContentFadeInCompleted)
                {
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;

                    StopCoroutine("FadeOutBackgroundScreen");
                    StartCoroutine("FadeOutBackgroundScreen");

                    StopCoroutine("FadeOutContentScreen");
                    StartCoroutine("FadeOutContentScreen", true);
                }

                else if (enablePressAnyKey && waitForPlayerInput && loadingProcess.progress == 0.9f)
                {
                    if (!isPAKFadeInRunning && !isPAKFadeInCompleted && isContentFadeInCompleted)
                    {
                        StopCoroutine("FadeOutContentScreen");
                        StartCoroutine("FadeOutContentScreen", false);

                        StopCoroutine("FadeInPAKScreen");
                        StartCoroutine("FadeInPAKScreen");
                    }

                    else if (isPAKFadeInCompleted && useCountdown)
                    {
                        pakCountdownSlider.value -= Time.unscaledDeltaTime;
                        pakCountdownLabel.text = Mathf.Round(pakCountdownSlider.value * 1).ToString();

                        if (pakCountdownSlider.value == 0)
                        {
                            loadingProcess.allowSceneActivation = true;
                            canvasGroup.interactable = false;
                            canvasGroup.blocksRaycasts = false;

                            StopCoroutine("FadeOutBackgroundScreen");
                            StartCoroutine("FadeOutBackgroundScreen");

                            StopCoroutine("FadeOutPAKScreen");
                            StartCoroutine("FadeOutPAKScreen", true);
                        }
                    }

#if ENABLE_LEGACY_INPUT_MANAGER
                    if (!useSpecificKey && Input.anyKeyDown && !isPAKFadeInRunning && isPAKFadeInCompleted)
#elif ENABLE_INPUT_SYSTEM
                    if (!useSpecificKey && Keyboard.current.anyKey.wasPressedThisFrame && !isPAKFadeInRunning && isPAKFadeInCompleted)
#endif
                    {
                        loadingProcess.allowSceneActivation = true;
                        canvasGroup.interactable = false;
                        canvasGroup.blocksRaycasts = false;

                        StopCoroutine("FadeOutBackgroundScreen");
                        StartCoroutine("FadeOutBackgroundScreen");

                        StopCoroutine("FadeOutPAKScreen");
                        StartCoroutine("FadeOutPAKScreen", true);
                    }

#if ENABLE_LEGACY_INPUT_MANAGER
                    else if (useSpecificKey && Input.GetKeyDown(keyCode) && !isPAKFadeInRunning && isPAKFadeInCompleted)
#elif ENABLE_INPUT_SYSTEM
                    else if (useSpecificKey && keyCode.triggered && !isPAKFadeInRunning && isPAKFadeInCompleted)
#endif
                    {
                        loadingProcess.allowSceneActivation = true;
                        canvasGroup.interactable = false;
                        canvasGroup.blocksRaycasts = false;

                        StopCoroutine("FadeOutBackgroundScreen");
                        StartCoroutine("FadeOutBackgroundScreen");

                        StopCoroutine("FadeOutPAKScreen");
                        StartCoroutine("FadeOutPAKScreen", true);
                    }
                }

                else if (enablePressAnyKey && !waitForPlayerInput)
                {
                    if (!isPAKFadeInRunning && !isPAKFadeInCompleted)
                    {
                        StopCoroutine("FadeInPAKScreen");
                        StartCoroutine("FadeInPAKScreen");
                    }

                    else if (useCountdown)
                    {
                        pakCountdownSlider.value -= Time.unscaledDeltaTime;
                        pakCountdownLabel.text = Mathf.Round(pakCountdownSlider.value * 1).ToString();

                        if (pakCountdownSlider.value == 0)
                        {
                            canvasGroup.interactable = false;
                            canvasGroup.blocksRaycasts = false;

                            StopCoroutine("FadeOutBackgroundScreen");
                            StartCoroutine("FadeOutBackgroundScreen");

                            StopCoroutine("FadeOutPAKScreen");
                            StartCoroutine("FadeOutPAKScreen", true);
                        }
                    }

#if ENABLE_LEGACY_INPUT_MANAGER
                    if (!useSpecificKey && Input.anyKeyDown && !isPAKFadeInRunning && isPAKFadeInCompleted)
#elif ENABLE_INPUT_SYSTEM
                    if (!useSpecificKey && Keyboard.current.anyKey.wasPressedThisFrame && !isPAKFadeInRunning && isPAKFadeInCompleted)
#endif
                    {
                        canvasGroup.interactable = false;
                        canvasGroup.blocksRaycasts = false;

                        StopCoroutine("FadeOutBackgroundScreen");
                        StartCoroutine("FadeOutBackgroundScreen");

                        StopCoroutine("FadeOutPAKScreen");
                        StartCoroutine("FadeOutPAKScreen", true);
                    }

#if ENABLE_LEGACY_INPUT_MANAGER
                    else if (useSpecificKey && Input.GetKeyDown(keyCode) && !isPAKFadeInRunning && isPAKFadeInCompleted)
#elif ENABLE_INPUT_SYSTEM
                    else if (useSpecificKey && keyCode.triggered && !isPAKFadeInRunning && isPAKFadeInCompleted)
#endif
                    {
                        canvasGroup.interactable = false;
                        canvasGroup.blocksRaycasts = false;

                        StopCoroutine("FadeOutBackgroundScreen");
                        StartCoroutine("FadeOutBackgroundScreen");

                        StopCoroutine("FadeOutPAKScreen");
                        StartCoroutine("FadeOutPAKScreen", true);
                    }
                }
            }
        }

        void ProcessRandomImages()
        {
            if (imageObject == null)
                return;

            imageObject.sprite = GetRandomImage();

            if (changeImageWithTimer) { StartCoroutine("DoImageTransition"); }
            else { enableRandomImages = false; }
        }

        Sprite GetRandomImage()
        {
            currentImageIndex = GetRandomUniqueValue(currentImageIndex, 0, imageList.Count);
            return imageList[currentImageIndex];
        }

        string GetRandomHint()
        {
            currentHintIndex = GetRandomUniqueValue(currentHintIndex, 0, hintList.Count);
            return hintList[currentHintIndex];
        }

        IEnumerator ProcessRandomHint()
        {
            yield return new WaitForSecondsRealtime(hintTimerValue);

            currentHintIndex = GetRandomUniqueValue(currentHintIndex, 0, hintList.Count);
            hintsText.text = hintList[currentHintIndex];

            StartCoroutine("ProcessRandomHint");
        }

        IEnumerator FadeInLoadingScreen()
        {
            isFadeInRunning = true;
            canvasGroup.alpha = 0;

            while (canvasGroup.alpha < 0.99f)
            {
                canvasGroup.alpha += Time.unscaledDeltaTime * fadeSpeed;
                yield return null;
            }

            isFadeInRunning = false;
            isFadeInCompleted = true;
            canvasGroup.alpha = 1;

            StopCoroutine("FadeInBackgroundScreen");
            StartCoroutine("FadeInBackgroundScreen");

            StopCoroutine("FadeInContentScreen");
            StartCoroutine("FadeInContentScreen");
        }

        IEnumerator FadeOutLoadingScreen()
        {
            backgroundCanvasGroup.gameObject.SetActive(false);
            contentCanvasGroup.gameObject.SetActive(false);
            pakCanvasGroup.gameObject.SetActive(false);

            isFadeOutRunning = true;
            onLoadingEnd.Invoke();

            while (canvasGroup.alpha > 0.01f)
            {
                canvasGroup.alpha -= Time.unscaledDeltaTime * fadeSpeed;
                yield return null;
            }

            isFadeOutRunning = false;
            isFadeOutCompleted = true;
            onLoadingDestroy.Invoke();

            Destroy(gameObject);
        }

        IEnumerator FadeInContentScreen()
        {
            isContentFadeInRunning = true;

            while (contentCanvasGroup.alpha < 0.99f)
            {
                contentCanvasGroup.alpha += Time.unscaledDeltaTime * contentFadeSpeed;
                yield return null;
            }

            isContentFadeInRunning = false;
            isContentFadeInCompleted = true;
            contentCanvasGroup.alpha = 1;
            onTransitionCompleted.Invoke();
        }

        IEnumerator FadeOutContentScreen(bool fadeOutScreenAfter)
        {
            isContentFadeOutRunning = true;

            while (contentCanvasGroup.alpha > 0.01f)
            {
                contentCanvasGroup.alpha -= Time.unscaledDeltaTime * contentFadeSpeed;
                yield return null;
            }

            isContentFadeOutRunning = false;
            isContentFadeOutCompleted = true;
            contentCanvasGroup.alpha = 0;

            if (projectorCamera != null)
            {
                projectorImage.gameObject.SetActive(false);
                projectorCamera.gameObject.SetActive(false);
            }

            if (fadeOutScreenAfter)
            {
                StopCoroutine("FadeOutLoadingScreen");
                StartCoroutine("FadeOutLoadingScreen");
            }

            if (customSceneActivation)
            {
                loadingProcess.allowSceneActivation = true;
            }
        }

        IEnumerator FadeInBackgroundScreen()
        {
            isBGFadeInRunning = true;

            while (backgroundCanvasGroup.alpha < 0.99f)
            {
                backgroundCanvasGroup.alpha += Time.unscaledDeltaTime * backgroundFadeSpeed;
                yield return null;
            }

            isBGFadeInRunning = false;
            isBGFadeInCompleted = true;
            backgroundCanvasGroup.alpha = 1;
        }

        IEnumerator FadeOutBackgroundScreen()
        {
            isBGFadeOutRunning = true;

            while (backgroundCanvasGroup.alpha > 0.01f)
            {
                backgroundCanvasGroup.alpha -= Time.unscaledDeltaTime * backgroundFadeSpeed;
                yield return null;
            }

            isBGFadeOutRunning = false;
            isBGFadeOutCompleted = true;
            backgroundCanvasGroup.alpha = 0;
        }

        IEnumerator FadeInPAKScreen()
        {
            isPAKFadeInRunning = true;
            pakCanvasGroup.alpha = 0;

            StopCoroutine("FadeOutContentScreen");
            StartCoroutine("FadeOutContentScreen", false);

            while (pakCanvasGroup.alpha < 0.99f)
            {
                pakCanvasGroup.alpha += Time.unscaledDeltaTime * contentFadeSpeed;
                yield return null;
            }

            isPAKFadeInRunning = false;
            isPAKFadeInCompleted = true;
            pakCanvasGroup.alpha = 1;
        }

        IEnumerator FadeOutPAKScreen(bool fadeOutScreenAfter)
        {
            isPAKFadeOutRunning = true;

            while (pakCanvasGroup.alpha > 0.01f)
            {
                pakCanvasGroup.alpha -= Time.unscaledDeltaTime * contentFadeSpeed;
                yield return null;
            }

            isPAKFadeOutRunning = false;
            isPAKFadeOutCompleted = true;
            pakCanvasGroup.alpha = 0;

            if (fadeOutScreenAfter)
            {
                StopCoroutine("FadeOutLoadingScreen");
                StartCoroutine("FadeOutLoadingScreen");
            }
        }

        IEnumerator DoImageTransition()
        {
            yield return new WaitForSecondsRealtime(imageTimerValue);

            while (imageObject.color.a > 0.01f)
            {
                imageObject.color = Color.Lerp(imageObject.color, new Color(imageObject.color.r, imageObject.color.g, imageObject.color.b, 0), (imageFadingSpeed / 30));
                yield return new WaitForFixedUpdate();
            }

            imageObject.color = new Color(imageObject.color.r, imageObject.color.g, imageObject.color.b, 0);
            imageObject.sprite = GetRandomImage();
            StartCoroutine("DoImageTransitionHelper");
        }

        IEnumerator DoImageTransitionHelper()
        {
            while (imageObject.color.a < 0.99f)
            {
                imageObject.color = Color.Lerp(imageObject.color, new Color(imageObject.color.r, imageObject.color.g, imageObject.color.b, 1), (imageFadingSpeed / 30));
                yield return new WaitForFixedUpdate();
            }

            imageObject.color = new Color(imageObject.color.r, imageObject.color.g, imageObject.color.b, 1);
            StartCoroutine("DoImageTransition");
        }

        IEnumerator HandleVirtualTransition(float timer)
        {
            yield return new WaitForSeconds(timer);
            StartCoroutine("FadeOutContentScreen", true);
        }

        public static int GetRandomUniqueValue(int currentValue, int minValue, int maxValue)
        {
            int value = UnityEngine.Random.Range(minValue, maxValue);

            while (currentValue == value)
            {
                value = UnityEngine.Random.Range(minValue, maxValue);
            }

            return value;
        }
    }
}