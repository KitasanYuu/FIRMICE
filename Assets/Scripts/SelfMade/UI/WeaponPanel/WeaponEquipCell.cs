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

    private AudioClip _hoverClip;
    private AudioClip _clickClip;

    private bool _selectStatus;
    private bool _hasExited;
    private bool _hasPressed;

    public Color _weaponEquipSelectedColor;
    public Color _weaponDefaultColor;

    private InventoryPanelManager IPM;
    private WeaponEquipManager WEM;
    private AudioSource _audiosource;

    private ResourceReader RR = new ResourceReader();

    private void OnEnable()
    {
        _hasPressed = false;
    }

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

    private void OnDisable()
    {
        _audiosource.clip = null;
    }

    private void ResourceInit()
    {
        _hoverClip = RR.GetUIAudioClip("Hover");
        _clickClip = RR.GetUIAudioClip("Click");
        _weaponEquipSelectedColor = RR.GetColor("WeaponEquipSelectColor");
        _weaponDefaultColor = RR.GetColor("WeaponDefaultColor");
    }

    private void ComponentInit()
    {
        _audiosource = GetComponent<AudioSource>();
        IPM = transform.GetComponentInParent<InventoryPanelManager>();
        UnderLine = transform.FindDeepChild("UnderLine").gameObject.GetComponent<Image>();
        WEM = GetComponentInParent<WeaponEquipManager>();
        HoverImage = transform.FindDeepChild("HoverImage").gameObject.GetComponent<CanvasGroup>();
        _weaponImage = transform.FindDeepChild("WeaponImage")?.gameObject.GetComponent<Image>();
        _hoverImage = transform.FindDeepChild("HoverImage").gameObject.GetComponent<Image>();
        _weaponName = transform.FindDeepChild("WeaponName")?.gameObject.GetComponent<TextMeshProUGUI>();
        _weaponType = transform.FindDeepChild("WeaponType")?.gameObject.GetComponent<TextMeshProUGUI>();
        WEM.AddEquipCell(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _hasExited = false;
        _hoverImage.color = _weaponDefaultColor;
        UnderLine.color = Color.white;
        StartCoroutine(HoverImageChange());
        _audiosource.clip = _hoverClip;
        _audiosource.Play();
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
        if (eventData.button == PointerEventData.InputButton.Left && !_hasPressed)
        {
            _hoverImage.color = _weaponEquipSelectedColor;
            UnderLine.color = Color.red;
            _audiosource.clip = _clickClip;
            _audiosource.Play();
            _hasPressed = true;
        }

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_hasExited && eventData.button == PointerEventData.InputButton.Left)
        {
            _hoverImage.color = _weaponDefaultColor;
            UnderLine.color = Color.white;
            StopAllCoroutines();
            toPanel.GetComponent<PanelIdentity>().PageNum = PageNum;
            IPM.OnPanelChanged(thisPanel, toPanel);
            HoverImage.alpha = 0;
        }
    }

    public void SetSelectedDetail(string weaponName,string weaponType,Sprite weaponImagePic)
    {
        _weaponName.text = weaponName;
        _weaponType.text = weaponType;
        _weaponImage.sprite = weaponImagePic;
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
