using UnityEngine;
using CustomInspector;
using YuuTool;
using DataManager;
using System.Collections;
using UnityEngine.UI;

public class InventoryPanelManager : MonoBehaviour
{
    public bool Testbutton;

    [ReadOnly] public GameObject _weaponPanel;
    [ReadOnly] public GameObject _backPackPanel;

    [ReadOnly] public GameObject _weaponSelectPanel;

    [ReadOnly] public Image _panelSubTitleChangeBar;
    [ReadOnly] public GameObject _renderBox;

    private CanvasGroup _weaponSelectPanelcanvasGroup;

    private bool TestStatus;

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
        _panelSubTitleChangeBar = transform.FindDeepChild("Subtitlefadebar")?.gameObject.GetComponent<Image>();
        _weaponPanel = transform.FindDeepChild("WeaponPanel")?.gameObject;
        _weaponSelectPanel = transform.FindDeepChild("WeaponSelectPanel")?.gameObject;

        _weaponSelectPanelcanvasGroup = _weaponSelectPanel?.GetComponent<CanvasGroup>();
    }

    public void OnPanelChanged(CanvasGroup currentPanel , CanvasGroup targetPanel)
    {
        Debug.Log(currentPanel + "" + targetPanel);
        //StartCoroutine(CanvasGroupFade(_weaponSelectPanelcanvasGroup));
    }

    private IEnumerator PanelChange(GameObject currentPanel, GameObject targetPanel)
    {
        yield return null;
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
