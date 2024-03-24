using UnityEngine;
using UnityEngine.EventSystems;

public class ItemCell : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("OnPointClick:" + eventData.ToString());
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("OnPointEnter:" + eventData.ToString());
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("OnPointExit:" + eventData.ToString());
    }

}
