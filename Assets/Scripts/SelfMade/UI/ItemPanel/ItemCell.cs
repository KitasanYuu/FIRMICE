using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemCell : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IPointerClickHandler
{
    public GameObject _selectStatus;

    void Start()
    {
        Transform _sTransform = _selectStatus.transform;
        
    }

    void Update()
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
        throw new System.NotImplementedException();
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
