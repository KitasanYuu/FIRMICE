using UnityEngine;
using UnityEngine.UI;

public class UICanvasController : MonoBehaviour
{
    public CanvasGroup uiCanvasGroup;
    public RawImage blurImage;

    void Start()
    {
        // 初始时禁用 UI 和模糊效果
        uiCanvasGroup.alpha = 0f;
        blurImage.gameObject.SetActive(false);
    }

    public void ShowUI()
    {
        // 显示 UI，启用模糊效果
        uiCanvasGroup.alpha = 1f;
        blurImage.gameObject.SetActive(true);
    }

    public void HideUI()
    {
        // 隐藏 UI，禁用模糊效果
        uiCanvasGroup.alpha = 0f;
        blurImage.gameObject.SetActive(false);
    }
}
