using CustomInspector;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using YuuTool;

public class WeaponCell : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public bool _occupied;
    public bool _selected;
    [ReadOnly]public string WeaponID;
    public GameObject Occupied;
    public GameObject Selected;
    public GameObject UnderLine;
    public CanvasGroup HoverImage;
    public TextMeshProUGUI _weaponName;
    public TextMeshProUGUI _weaponType;
    public Image _weaponImage;
    public Image _hoverImage;

    private WeaponDetailCell WDC;
    private WeaponInventoryManager WIM;
    private AudioSource _audioSource;

    private bool _selectStatus;

    public AudioClip _hoverClip;
    public AudioClip _clickClip;

    public Color _weaponSelectedColor;
    public Color _weaponOccupiedColor;
    public Color _weaponDefaultColor;

    void Start()
    {
        ComponentInit();
    }

    private void OnDisable()
    {
        _audioSource.clip = null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetSelectStatus(bool AmISelected)
    {
        if (!_occupied)
        {
            Selected.SetActive(AmISelected);
            UnderLine?.SetActive(!AmISelected);
            Selected?.SetActive(AmISelected);
            _selectStatus = AmISelected;
            _selected = AmISelected;
            _hoverImage.color = AmISelected ? _weaponSelectedColor : _weaponDefaultColor;
        }
    }

    public void SetOccupyStatus(bool isOccupied)
    {
        _occupied = isOccupied;
        if (isOccupied)
        {
            Selected.SetActive(false);
            _hoverImage.color = _weaponOccupiedColor;
            Occupied.SetActive(true);
            UnderLine.SetActive(false);
        }
    }

    public void PanelRecovering()
    {
        Selected?.SetActive(false);
        Occupied?.SetActive(false);
        UnderLine?.SetActive(true);
        _hoverImage.color = _weaponDefaultColor;
    }

    private void ComponentInit()
    {
        _audioSource = GetComponent<AudioSource>();
        Occupied = transform.FindDeepChild("Occupied").gameObject;
        Selected = transform.FindDeepChild("Selected").gameObject;
        UnderLine = transform.FindDeepChild("UnderLine").gameObject;
        HoverImage = transform.FindDeepChild("HoverImage").gameObject.GetComponent<CanvasGroup>();
        _weaponImage = transform.FindDeepChild("WeaponImage")?.gameObject.GetComponent<Image>();
        _hoverImage = transform.FindDeepChild("HoverImage").gameObject.GetComponent<Image>();
        _weaponName = transform.FindDeepChild("WeaponName")?.gameObject.GetComponent<TextMeshProUGUI>();
        _weaponType = transform.FindDeepChild("WeaponType")?.gameObject.GetComponent<TextMeshProUGUI>();
        WIM.RequestInfo(this, WeaponID);
    }

    public void SetStartParameter(string weaponID,WeaponInventoryManager wim,WeaponDetailCell wdc,AudioClip click,AudioClip hover,Color SelectedColor,Color DefaultColor,Color OccupiedColor)
    {
        WeaponID = weaponID;
        WDC = wdc;
        WIM = wim;
        _clickClip = click;
        _hoverClip = hover;
        _weaponDefaultColor = DefaultColor;
        _weaponSelectedColor = SelectedColor;
        _weaponOccupiedColor = OccupiedColor;
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


    #region 处理指针调用的函数（不要用这里的调用方法！！！！！！！）
    private void UareSelected()
    {
        SetOccupyStatus(false);
        WIM.SelectWeaponChangedRequest(WeaponID);
        WIM.WeaponSelected(this,gameObject);
    }

    private void UareHovering()
    {
        if(!_occupied)
            _hoverImage.color = _selectStatus ? _weaponSelectedColor : _weaponDefaultColor;
        WIM.SelectWeaponChangedRequest(WeaponID);
        StartCoroutine(HoverImageChange());
    }

    private void UareRecovering()
    {
        if(!_occupied)
            _hoverImage.color = _weaponDefaultColor;
        WIM.SelectWeaponChangedRequest(null);
        StopAllCoroutines();
        HoverImage.alpha = 0;
    }
#endregion

    #region 指针点击调用
    public void OnPointerClick(PointerEventData eventData)
    {

        if (eventData.clickCount == 2)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (!_occupied)
                {
                    UareSelected();
                    _audioSource.clip = _clickClip;
                    _audioSource.Play();
                }
                else
                {
                    //这里准备做强行切换的逻辑，暂时先空着
                }
            }
            else if(eventData.button == PointerEventData.InputButton.Right && _selected)
            {
                WIM.ClearCurrentWeaponSelect(WIM._panelID.PageNum);
                SetSelectStatus(false);
                
            }

            //Debug.Log("OnPointClick:" + eventData.ToString());
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        UareHovering();
        _audioSource.clip = _hoverClip;
        _audioSource.Play();
        //Debug.Log("OnPointEnter:" + eventData.ToString());
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        UareRecovering();
        //WDC._currentWeaponSelected = null;
        //Debug.Log("OnPointExit:" + eventData.ToString());
    }
    #endregion

}
