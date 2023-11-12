using UnityEngine;
using UnityEngine.UI;

public class UICanvasController : MonoBehaviour
{
    public CanvasGroup uiCanvasGroup;
    public RawImage blurImage;

    void Start()
    {
        // ��ʼʱ���� UI ��ģ��Ч��
        uiCanvasGroup.alpha = 0f;
        blurImage.gameObject.SetActive(false);
    }

    public void ShowUI()
    {
        // ��ʾ UI������ģ��Ч��
        uiCanvasGroup.alpha = 1f;
        blurImage.gameObject.SetActive(true);
    }

    public void HideUI()
    {
        // ���� UI������ģ��Ч��
        uiCanvasGroup.alpha = 0f;
        blurImage.gameObject.SetActive(false);
    }
}
