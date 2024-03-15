using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliteHealthBar : MonoBehaviour
{
    private RectTransform healthBarRectTransform;
    private HPVisionManager HVM;

    // Start is called before the first frame update
    void Start()
    {
        ComponentInit();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetVisibility(bool isVisible)
    {
        //if (!isVisible)
        //{
        //    if (HPBarFadeCoroutine != null)
        //    {
        //        StopCoroutine(HPBarFadeCoroutine);
        //    }
        //    if (ArmorBarFadeCoroutine != null)
        //    {
        //        StopCoroutine(ArmorBarFadeCoroutine);
        //    }

        //    ArmorBar.fillAmount = CArmorRate;
        //    ArmorFadeBar.fillAmount = CArmorRate;
        //    HPBar.fillAmount = CHPRate;
        //    HPFadeBar.fillAmount = CHPRate;
        //}

        gameObject.SetActive(isVisible);

    }

    private void ComponentInit()
    {
        // 获取血条的RectTransform组件
        healthBarRectTransform = GetComponent<RectTransform>();
        HVM = GetComponentInParent<HPVisionManager>();
        if (HVM != null)
        {
            HVM.RegisterEliteHealthBar(this);
        }
    }

    void OnDestroy()
    {
        // 在销毁时从控制器中注销
        if (HVM != null)
        {
            HVM.UnregisterEliteHealthBar(this);
        }
    }
}