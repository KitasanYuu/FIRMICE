using CustomInspector;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using YuuTool;

public class WeaponCell : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [ReadOnly]public string WeaponID;
    public GameObject Selected;
    public GameObject UnderLine;
    public CanvasGroup HoverImage;
    public TextMeshProUGUI _weaponName;
    public TextMeshProUGUI _weaponType;
    public Image _weaponImage;
    public Image _hoverImage;

    private WeaponDetailCell WDC;
    private WeaponInventoryManager WIM;

    private bool _selectStatus;

    public Color _weaponSelectedColor;
    public Color _weaponDefaultColor;

    void Start()
    {
        ComponentInit();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetSelectStatus(bool AmISelected)
    {
        UnderLine?.SetActive(!AmISelected);
        Selected?.SetActive(AmISelected);
        _selectStatus = AmISelected;
    }

    private void ComponentInit()
    {
        Selected = transform.FindDeepChild("Selected").gameObject;
        UnderLine = transform.FindDeepChild("UnderLine").gameObject;
        HoverImage = transform.FindDeepChild("HoverImage").gameObject.GetComponent<CanvasGroup>();
        _weaponImage = transform.FindDeepChild("WeaponImage")?.gameObject.GetComponent<Image>();
        _hoverImage = transform.FindDeepChild("HoverImage").gameObject.GetComponent<Image>();
        _weaponName = transform.FindDeepChild("WeaponName")?.gameObject.GetComponent<TextMeshProUGUI>();
        _weaponType = transform.FindDeepChild("WeaponType")?.gameObject.GetComponent<TextMeshProUGUI>();
        WIM.RequestInfo(this, WeaponID);
    }

    public void SetStartParameter(string weaponID,WeaponInventoryManager wim,WeaponDetailCell wdc,Color SelectedColor,Color DefaultColor)
    {
        WeaponID = weaponID;
        WDC = wdc;
        WIM = wim;
        _weaponDefaultColor = DefaultColor;
        _weaponSelectedColor = SelectedColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.clickCount == 2)
        {
            _hoverImage.color = _weaponSelectedColor;
            WIM.SelectWeaponChangedRequest(WeaponID);
            WIM.WeaponSelected(this);
            Debug.Log("OnPointClick:" + eventData.ToString());
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        _hoverImage.color = _selectStatus ? _weaponSelectedColor : _weaponDefaultColor;
        WIM.SelectWeaponChangedRequest(WeaponID);
        StartCoroutine(HoverImageChange());
        Debug.Log("OnPointEnter:" + eventData.ToString());
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        _hoverImage.color = _weaponDefaultColor;
        StopAllCoroutines();
        HoverImage.alpha = 0;
        WDC._currentWeaponSelected = null;
        Debug.Log("OnPointExit:" + eventData.ToString());
    }

    private IEnumerator HoverImageChange()
    {
        float currentTime = 0f;
        float fadeInDuration = 0.3f;

        while (currentTime < fadeInDuration)
        {
            currentTime += Time.deltaTime;
            HoverImage.alpha = Mathf.Lerp(0, 1, currentTime / fadeInDuration);
            yield return null;
        }

        HoverImage.alpha = 1; // 确保最终Alpha值为1
    }

}
