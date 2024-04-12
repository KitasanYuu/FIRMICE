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
    [ReadOnly] public GameObject _backPackPanel;

    [ReadOnly] public GameObject _weaponSelectPanel;

    [ReadOnly] public Image _panelSubTitleChangeBar;
    [ReadOnly] public GameObject _renderBox;

    [ReadOnly] public List<GameObject> PidUnit = new List<GameObject>();
    private CanvasGroup _weaponSelectPanelcanvasGroup;

    private bool TestStatus;
    private bool _hasInited;

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
        
        _weaponPanel.SetActive(newstatus);

        if (!newstatus && _renderBox != null)
            Destroy(_renderBox);
    }

    private void ComponentInit()
    {
        Title = transform.FindDeepChild("Title")?.gameObject.GetComponent<TextMeshProUGUI>();
        SubTitle = transform.FindDeepChild("SubtitleDiscribe")?.gameObject.GetComponent<TextMeshProUGUI>();
        _panelSubTitleChangeBar = transform.FindDeepChild("Subtitlefadebar")?.gameObject.GetComponent<Image>();
        _weaponPanel = transform.FindDeepChild("WeaponPanel")?.gameObject;
        _weaponSelectPanel = transform.FindDeepChild("WeaponSelectPanel")?.gameObject;

        _weaponSelectPanelcanvasGroup = _weaponSelectPanel?.GetComponent<CanvasGroup>();
    }

    public void OnPanelChanged(CanvasGroup currentPanel , CanvasGroup targetPanel)
    {
        PanelIdentity _panelIdentity = targetPanel.gameObject.GetComponent<PanelIdentity>();
        SubSelectIdentity subs = RR.GetPanelIDData(_panelIdentity.PID,_panelIdentity.PageNum);
        string _targetPanelTitle = subs.PanelTitle;
        string _targetPanelSubTitle = subs.PanelSubTitle;

        //Debug.Log(currentPanel + "" + targetPanel);
        //StartCoroutine(CanvasGroupFade(_weaponSelectPanelcanvasGroup));
        StartCoroutine(PanelChange(currentPanel, targetPanel, _targetPanelTitle));
        StartCoroutine(PanelTitleChange(targetPanel, _targetPanelSubTitle));
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
            _hasInited = true;
        }
    }

    private IEnumerator PanelChange(CanvasGroup currentPanel, CanvasGroup targetPanel,string panelTitle = null)
    {
        float currentTime = 0f;
        float fadeInDuration = 0.1f;

        while (currentTime < fadeInDuration)
        {
            currentTime += Time.unscaledDeltaTime;
            currentPanel.alpha = Mathf.Lerp(1, 0, currentTime / fadeInDuration);
            yield return null;
        }

        if (panelTitle != null)
        {
            Title.text = panelTitle;
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

    private IEnumerator PanelTitleChange(CanvasGroup NextPanel,string nextPanelsubtitle = null)
    {
        float currentTime = 0f;
        float changeSingleDuration = 0.1f;

        while (currentTime < changeSingleDuration)
        {
            currentTime += Time.unscaledDeltaTime;
            _panelSubTitleChangeBar.fillAmount = Mathf.Lerp(0f, 1f, currentTime / changeSingleDuration);
            yield return null;
        }

        if(nextPanelsubtitle != null)
        {
            SubTitle.text = nextPanelsubtitle;
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
