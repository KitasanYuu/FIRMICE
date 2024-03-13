using BattleHealth;
using CustomInspector;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [ReadOnly]public GameObject Object;
    public Transform HPAnchor; // NPC的Transform组件
    public Camera mainCamera; // 主相机
    public Canvas canvas; // Canvas，其Render Mode应该设置为Camera，并且指定了UI Camera
    public float baseOffsetY = 2.0f; // 血条在NPC头顶的基础偏移量

    public float transpeed = 10f;

    private VirtualHP VHP;
    private HPVisionManager HVM;
    private RectTransform healthBarRectTransform;

    public Image HPBar;
    public Image HPFadeBar;
    public Image ArmorBar;
    public Image ArmorFadeBar;

    private float PreviousHP;
    private float PreviousTotalHP;
    private float PreviousArmor;
    private float PreviousTotalArmor;

    [HideInInspector]
    public float ScaleFactor;

    private Coroutine HPBarFadeCoroutine;
    private Coroutine ArmorBarFadeCoroutine;

    void Start()
    {
        ComponentInit();
    }

    void LateUpdate()
    {
        BarPositionAdjust();
        UIParameterUpdate();
    }

    private void UIParameterUpdate()
    {
        if(VHP!= null)
        {
            float TotalHP = VHP.TotalHP;
            float CurrentHP = VHP.CurrentHP;
            float Armor = VHP.Armor;
            float CurrentArmor = VHP.CurrentArmor;
            float HPValueRate;
            float ArmorValueRate;

            if(TotalHP == 0)
                HPValueRate = 0;
            else
                HPValueRate = CurrentHP / TotalHP;

            if (Armor == 0)
                ArmorValueRate = 0;
            else
                ArmorValueRate = CurrentArmor / Armor;

            if (PreviousHP != CurrentHP || PreviousTotalHP != TotalHP)
            {
                float HPdamageDir = 0;
                HPdamageDir = CurrentHP - PreviousHP;
                PreviousHP = CurrentHP;
                PreviousTotalHP = TotalHP;

                if (HPBarFadeCoroutine != null)
                {
                    StopCoroutine(HPBarFadeCoroutine);
                }
                HPBarFadeCoroutine = StartCoroutine(DoHPFadeEffect(HPdamageDir, HPValueRate));
            }

            if(PreviousArmor != CurrentArmor || PreviousTotalArmor != Armor)
            {
                float ArmordamageDir = 0;
                ArmordamageDir = CurrentArmor - PreviousArmor;
                PreviousArmor = CurrentArmor;
                PreviousTotalArmor = Armor;

                if(ArmorBarFadeCoroutine != null)
                {
                    StopCoroutine(ArmorBarFadeCoroutine);
                }
                ArmorBarFadeCoroutine = StartCoroutine(DoArmorFadeEffect(ArmordamageDir, ArmorValueRate));
            }

            //Debug.Log(HPdamageDir);
            //HPBar.fillAmount = HPValueRate;
            //ArmorBar.fillAmount = ArmorValueRate;
        }

    }

    #region 控制血条UI在屏幕空间的位置

    private void BarPositionAdjust()
    {
        // 计算摄像机到NPC的距离
        float distance = Vector3.Distance(mainCamera.transform.position, HPAnchor.position);

        // 根据距离调整血条的缩放
        float scaleFactor = Mathf.Clamp(1 / (distance * 0.1f), 0, 1.2f);
        ScaleFactor = scaleFactor;
        healthBarRectTransform.localScale = new Vector3(scaleFactor, scaleFactor, 1);

        // 将NPC的世界坐标转换为屏幕坐标
        Vector3 screenPos = mainCamera.WorldToScreenPoint(HPAnchor.position + new Vector3(0, baseOffsetY, 0));

        // 将屏幕坐标转换为Canvas坐标系下的位置
        Vector2 canvasPos;
        bool isConverted = RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.GetComponent<RectTransform>(), screenPos, canvas.worldCamera, out canvasPos);
        if (isConverted)
        {
            // 更新血条的位置
            healthBarRectTransform.localPosition = canvasPos;
        }
    }

    #endregion

    #region 组件参数初始化
    private void ComponentInit()
    {
        // 获取血条的RectTransform组件
        healthBarRectTransform = GetComponent<RectTransform>();
        HVM = GetComponentInParent<HPVisionManager>();
        if (HVM != null)
        {
            HVM.RegisterHealthBar(this);
        }
    }
    #endregion

    #region 外部设参
    public void SetParameter(GameObject RegisterObject,VirtualHP vhp,Transform Anchor, Camera MainCamera, Canvas scanvas)
    {
        Object = RegisterObject;
        VHP = vhp;
        HPAnchor = Anchor;
        mainCamera = MainCamera;
        canvas = scanvas;
    }

    public void SetVisibility(bool isVisible)
    {
        gameObject.SetActive(isVisible);
    }
    #endregion

    private IEnumerator DoHPFadeEffect (float damage,float valueRate)
    {
        var suddenChangeBar = damage >= 0 ? HPFadeBar : HPBar;
        var slowChangeBar = damage >= 0 ? HPBar : HPFadeBar;
        suddenChangeBar.fillAmount = valueRate;
        while(Mathf.Abs(suddenChangeBar.fillAmount - slowChangeBar.fillAmount) != 0f)
        {
            slowChangeBar.fillAmount = Mathf.Lerp(slowChangeBar.fillAmount, valueRate, Time.deltaTime * transpeed);
            yield return null;
        }
        slowChangeBar.fillAmount = valueRate;
    }

    private IEnumerator DoArmorFadeEffect(float damage, float valueRate)
    {
        var suddenChangeBar = damage >= 0 ? ArmorFadeBar : ArmorBar;
        var slowChangeBar = damage >= 0 ? ArmorBar : ArmorFadeBar;
        suddenChangeBar.fillAmount = valueRate;
        while (Mathf.Abs(suddenChangeBar.fillAmount - slowChangeBar.fillAmount) != 0f)
        {
            slowChangeBar.fillAmount = Mathf.Lerp(slowChangeBar.fillAmount, valueRate, Time.deltaTime * transpeed);
            yield return null;
        }
        slowChangeBar.fillAmount = valueRate;
    }

    void OnDestroy()
    {
        // 在销毁时从控制器中注销
        if (HVM != null)
        {
            HVM.UnregisterHealthBar(this);
        }
    }


}
