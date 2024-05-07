using CustomInspector;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemCell : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IPointerClickHandler
{
    [ReadOnly] public string currentItemID;

    public GameObject _selectStatus;
    public GameObject _hoverDetail;
    public TextMeshProUGUI hoverDetailText;

    [HideInInspector]
    public int currentItemCount;

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
        
    }

    private IEnumerator SelectStatus()
    {
        CanvasGroup _sCanvasGroup = _selectStatus.GetComponent<CanvasGroup>();
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
        StartCoroutine(SelectStatus());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
        CanvasGroup _sCanvasGroup = _selectStatus.GetComponent<CanvasGroup>();
        _sCanvasGroup.alpha = 0;
    }
}
