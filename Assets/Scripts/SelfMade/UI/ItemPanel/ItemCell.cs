using CustomInspector;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemCell : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IPointerClickHandler
{
    [ReadOnly] public string currentItemID;

    public GameObject _hoverStatus;
    public GameObject _hoverDetail;
    public TextMeshProUGUI hoverDetailText;

    [HideInInspector]
    public int currentItemCount;

    private CanvasGroup _sCanvasGroup;
    private Transform _sTransform;

    [HideInInspector]
    public bool isSelected;

    [HideInInspector]
    public ItemInventoryManager IIM;


    void Start()
    {
        ComponentInit();
    }

    void Update()
    {
        //hoverDetailText.text = currentItemCount.ToString();
    }

    public void SetParameter(ItemInventoryManager iIm)
    {
        IIM = iIm;
        hoverDetailText.text = currentItemCount.ToString();
    }

    private void ComponentInit()
    {
        _sCanvasGroup = _hoverStatus.GetComponent<CanvasGroup>();
        _sTransform = _hoverStatus.transform;
    }

    public void SetSelectStatus(bool status)
    {
        isSelected = status;
        if (status)
        {
            StartCoroutine(HoverStatus(status));
        }
        else
        {
            StopAllCoroutines();
            _sCanvasGroup.alpha = 0;
            _sTransform.localScale = Vector3.one;
        }
    }

    private IEnumerator HoverStatus(bool isSelected = false)
    {
        if(_sCanvasGroup== null || _sTransform == null)
        {
            _sCanvasGroup= _hoverStatus.GetComponent<CanvasGroup>();
            _sTransform = _hoverStatus.transform;
        }

        if (isSelected)
        {
            _sTransform.localScale = new Vector3(1.05f, 1.05f, 1.05f);

        }

        float currentTime = 0f;
        float fadeInDuration = 0.1f;

        while (currentTime < fadeInDuration)
        {
            currentTime += Time.unscaledDeltaTime;
            _sCanvasGroup.alpha = Mathf.Lerp(0, 1, currentTime / fadeInDuration);
            yield return null;
        }

        _sCanvasGroup.alpha = 1; // 确保最终Alpha值为1
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        IIM.SelectCell(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isSelected)
        {
            StartCoroutine(HoverStatus());
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isSelected)
        {
            StopAllCoroutines();
            CanvasGroup _sCanvasGroup = _hoverStatus.GetComponent<CanvasGroup>();
            _sCanvasGroup.alpha = 0;
        }
    }
}
