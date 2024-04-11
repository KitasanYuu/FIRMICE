using System.Collections;
using CustomInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using YuuTool;
using DataManager;

public class WeaponEquipCell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{
    [ReadOnly] public string WeaponID;
    public int PageNum;
    [Space2(20)]
    public CanvasGroup thisPanel;
    public CanvasGroup toPanel;

    [HorizontalLine("以下自动获取，不用填写", 2, FixedColor.Gray)]
    public Image UnderLine;
    public CanvasGroup HoverImage;
    public TextMeshProUGUI _weaponName;
    public TextMeshProUGUI _weaponType;
    public Image _weaponImage;
    public Image _hoverImage;

    private bool _selectStatus;
    private bool _hasExited;

    public Color _weaponEquipSelectedColor;
    public Color _weaponDefaultColor;

    private InventoryPanelManager IPM;

    private ResourceReader RR = new ResourceReader();

    // Start is called before the first frame update
    void Start()
    {
        ResourceInit();
        ComponentInit();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ResourceInit()
    {
        _weaponEquipSelectedColor = RR.GetColor("WeaponEquipSelectColor");
        _weaponDefaultColor = RR.GetColor("WeaponDefaultColor");
    }

    private void ComponentInit()
    {
        IPM = transform.GetComponentInParent<InventoryPanelManager>();
        UnderLine = transform.FindDeepChild("UnderLine").gameObject.GetComponent<Image>();
        HoverImage = transform.FindDeepChild("HoverImage").gameObject.GetComponent<CanvasGroup>();
        _weaponImage = transform.FindDeepChild("WeaponImage")?.gameObject.GetComponent<Image>();
        _hoverImage = transform.FindDeepChild("HoverImage").gameObject.GetComponent<Image>();
        _weaponName = transform.FindDeepChild("WeaponName")?.gameObject.GetComponent<TextMeshProUGUI>();
        _weaponType = transform.FindDeepChild("WeaponType")?.gameObject.GetComponent<TextMeshProUGUI>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _hasExited = false;
        _hoverImage.color = _weaponDefaultColor;
        UnderLine.color = Color.white;
        StartCoroutine(HoverImageChange());
        //Debug.Log("OnPointEnter:" + eventData.ToString());
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        _hasExited = true;
        _hoverImage.color = _weaponDefaultColor;
        UnderLine.color = Color.white;
        StopAllCoroutines();
        HoverImage.alpha = 0;
        //WDC._currentWeaponSelected = null;
        //Debug.Log("OnPointExit:" + eventData.ToString());
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _hoverImage.color = _weaponEquipSelectedColor;
        UnderLine.color = Color.red;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_hasExited)
        {
            _hoverImage.color = _weaponDefaultColor;
            UnderLine.color = Color.white;
            StopAllCoroutines();
            toPanel.GetComponent<PanelIdentity>().PageNum = PageNum;
            IPM.OnPanelChanged(thisPanel, toPanel);
            HoverImage.alpha = 0;
        }
    }



    private IEnumerator HoverImageChange()
    {
        float currentTime = 0f;
        float fadeInDuration = 0.3f;

        while (currentTime < fadeInDuration)
        {
            currentTime += Time.unscaledDeltaTime;
            HoverImage.alpha = Mathf.Lerp(0, 1, currentTime / fadeInDuration);
            yield return null;
        }

        HoverImage.alpha = 1; // 确保最终Alpha值为1
    }

}
