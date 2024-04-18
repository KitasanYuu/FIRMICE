using UnityEngine;
using CustomInspector;
using YuuTool;
using DataManager;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InventoryPanelManager : MonoBehaviour
{
    public bool Testbutton;
    [ReadOnly] public TextMeshProUGUI Title;
    [ReadOnly] public TextMeshProUGUI SubTitle;

    [ReadOnly] public GameObject _weaponPanel;

    [ReadOnly] public Image _panelSubTitleChangeBar;
    [ReadOnly] public GameObject _renderBox;

    [ReadOnly] public List<GameObject> PidUnit = new List<GameObject>();


    private bool TestStatus;
    private bool _hasInited;

    private CanvasGroup _initCVHolder;

    private ResourceReader RR = new ResourceReader();
    private void Start()
    {
        ComponentInit();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            TestStatus = TestStatus ? false : true;
            SetWeaponPanelStatus(TestStatus);
            Testbutton = false;
        }
    }

    private void OnEnable()
    {
        _hasInited = false;
    }

    public void SetWeaponPanelStatus(bool newstatus)
    {
        if (_renderBox == null && newstatus)
        {
            GameObject boxPrefab = RR.GetGameObject("RenderBox", "CharacterWeaponRendering");
            _renderBox = Instantiate(boxPrefab);
        }
        
        //_weaponPanel.SetActive(newstatus);

        if (!newstatus && _renderBox != null)
            Destroy(_renderBox);
    }

    private void ComponentInit()
    {
        _panelSubTitleChangeBar = transform.FindDeepChild("Subtitlefadebar")?.gameObject.GetComponent<Image>();
        Title = transform.FindDeepChild("Title")?.gameObject.GetComponent<TextMeshProUGUI>();
        SubTitle = transform.FindDeepChild("SubtitleDiscribe")?.gameObject.GetComponent<TextMeshProUGUI>();
        _weaponPanel = transform.FindDeepChild("WeaponPanel")?.gameObject;

        StartCoroutine(PanelTitleChange(_initCVHolder));
        _initCVHolder = null;
    }

    public void OnPanelChanged(CanvasGroup currentPanel , CanvasGroup targetPanel)
    {
        //Debug.Log(currentPanel + "" + targetPanel);
        //StartCoroutine(CanvasGroupFade(_weaponSelectPanelcanvasGroup));
        StartCoroutine(PanelChange(currentPanel, targetPanel));
        StartCoroutine(PanelTitleChange(targetPanel));
    }


    public void PanelInit(GameObject PanelObject,CanvasGroup firstPanel)
    {
        if (!PidUnit.Contains(PanelObject))
        {
            PidUnit.Add(PanelObject);
        }

        if (!_hasInited)
        {
            StartCoroutine(PanelTitleChange(firstPanel));
            _initCVHolder = firstPanel;
            _hasInited = false;
        }
    }

    private IEnumerator PanelChange(CanvasGroup currentPanel, CanvasGroup targetPanel)
    {
        float currentTime = 0f;
        float fadeInDuration = 0.1f;

        while (currentTime < fadeInDuration)
        {
            currentTime += Time.unscaledDeltaTime;
            currentPanel.alpha = Mathf.Lerp(1, 0, currentTime / fadeInDuration);
            yield return null;
        }

        currentPanel.alpha = 0; // 确保最终Alpha值为0
        currentPanel.gameObject.SetActive(false);
        currentPanel.alpha = 1;

        currentTime = 0f;

        targetPanel.alpha = 0;
        targetPanel.gameObject.SetActive(true);

        while (currentTime < fadeInDuration)
        {
            currentTime += Time.unscaledDeltaTime;
            targetPanel.alpha = Mathf.Lerp(0, 1, currentTime / fadeInDuration);
            yield return null;
        }
        targetPanel.alpha = 1; // 确保最终Alpha值为1

    }


    private IEnumerator PanelTitleChange(CanvasGroup NextPanel)
    {
        if (_panelSubTitleChangeBar != null && SubTitle != null && Title != null)
        {
            PanelIdentity _panelIdentity = NextPanel.gameObject.GetComponent<PanelIdentity>();
            SubSelectIdentity subs = RR.GetPanelIDData(_panelIdentity.PID, _panelIdentity.PageNum);
            string _targetPanelTitle = subs.PanelTitle;
            string _targetPanelSubTitle = subs.PanelSubTitle;

            float currentTime = 0f;
            float changeSingleDuration = 0.1f;

            while (currentTime < changeSingleDuration)
            {
                currentTime += Time.unscaledDeltaTime;
                _panelSubTitleChangeBar.fillAmount = Mathf.Lerp(0f, 1f, currentTime / changeSingleDuration);
                yield return null;
            }

            if (_targetPanelSubTitle != null)
            {
                SubTitle.text = _targetPanelSubTitle;
            }

            if (_targetPanelTitle != null)
            {
                Title.text = _targetPanelTitle;
            }

            _panelSubTitleChangeBar.fillAmount = 1f;

            currentTime = 0f;

            while (currentTime < changeSingleDuration)
            {
                currentTime += Time.unscaledDeltaTime;
                _panelSubTitleChangeBar.fillAmount = Mathf.Lerp(1f, 0f, currentTime / changeSingleDuration);
                yield return null;
            }

            _panelSubTitleChangeBar.fillAmount = 0f;
        }
    }

    private IEnumerator CanvasGroupFade(CanvasGroup newCanvasGroup)
    {
        float currentTime = 0f;
        float fadeInDuration = 0.1f;

        while (currentTime < fadeInDuration)
        {
            currentTime += Time.unscaledDeltaTime;
            newCanvasGroup.alpha = Mathf.Lerp(1, 0, currentTime / fadeInDuration);
            yield return null;
        }

        newCanvasGroup.gameObject.SetActive(false);
        newCanvasGroup.alpha = 1; // 确保最终Alpha值为1
    }

}
