using BattleHealth;
using CustomInspector;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YuuTool;

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

    [SerializeField,ReadOnly] private Image HPBar;
    [SerializeField, ReadOnly] private Image HPFadeBar;
    [SerializeField, ReadOnly] private Image ArmorBar;
    [SerializeField, ReadOnly] private Image ArmorFadeBar;
    [SerializeField, ReadOnly] private Image LevelBackGround;

    [Space2(20)]

    [SerializeField, ReadOnly] private TextMeshProUGUI ObjectName;
    [SerializeField, ReadOnly] private TextMeshProUGUI ObjectLevel;


    private float PreviousHP;
    private float PreviousTotalHP;
    private float PreviousArmor;
    private float PreviousTotalArmor;

    private float CHPRate;
    private float CArmorRate;

    [HideInInspector]
    public float ScaleFactor;

    private Coroutine HPBarFadeCoroutine;
    private Coroutine ArmorBarFadeCoroutine;

    private bool ResourceInitComplete;
    private bool ComponentInitComplete;

    private void Awake()
    {
        ResourceInit();
    }

    void Start()
    {
        ComponentInit();
        ParameterInit();
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

            CHPRate = HPValueRate;
            CArmorRate = ArmorValueRate;

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
        float scaleFactor = Mathf.Clamp(1 / (distance * 0.1f), 0, 0.75f);
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
    private void ResourceInit()
    {
        HPBar = transform.FindDeepChild("HealthBar")?.gameObject.GetComponent<Image>();
        HPFadeBar = transform.FindDeepChild("HealthFadeBar")?.gameObject.GetComponent<Image>();
        ArmorBar = transform.FindDeepChild("ArmorBar")?.gameObject.GetComponent<Image>();
        ArmorFadeBar = transform.FindDeepChild("ArmorFadeBar")?.gameObject.GetComponent<Image>();
        LevelBackGround = transform.FindDeepChild("LevelBackGround")?.gameObject.GetComponent<Image>();

        ObjectName = transform.FindDeepChild("NameContent")?.gameObject.GetComponent<TextMeshProUGUI>();
        ObjectLevel = transform.FindDeepChild("LevelContent")?.gameObject.GetComponent<TextMeshProUGUI>();

        ResourceInitComplete = true;
        RequestParameterProgress();
    }

    private void ComponentInit()
    {
        // 获取血条的RectTransform组件
        healthBarRectTransform = GetComponent<RectTransform>();
        HVM = GetComponentInParent<HPVisionManager>();
        if (HVM != null)
        {
            HVM.RegisterHealthBar(this);
        }

        ComponentInitComplete = true;
        RequestParameterProgress();
    }

    private void RequestParameterProgress()
    {
        if(ResourceInitComplete && ComponentInitComplete)
            HVM?.ParameterSetProgress(gameObject, Object);
    }

    private void ParameterInit()
    {
        HPBar.fillAmount = 1;
        ArmorBar.fillAmount = 1;
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

    public void SetInitBarParameter(float SHPBar,float SArmorBar)
    {
        HPBar.fillAmount = SHPBar;
        ArmorBar.fillAmount= SArmorBar;
    }

    public void SetVisibility(bool isVisible)
    {
        if (!isVisible)
        {
            if (HPBarFadeCoroutine != null)
            {
                StopCoroutine(HPBarFadeCoroutine);
            }
            if (ArmorBarFadeCoroutine != null)
            {
                StopCoroutine(ArmorBarFadeCoroutine);
            }

            ArmorBar.fillAmount = CArmorRate;
            ArmorFadeBar.fillAmount = CArmorRate;
            HPBar.fillAmount = CHPRate;
            HPFadeBar.fillAmount = CHPRate;
        }

        gameObject.SetActive(isVisible);
    }

    public void SetHPColor(Color SetColor)
    {
        HPBar.color = SetColor;
    }

    public void SetArmorColor(Color SetColor)
    {
        ArmorBar.color = SetColor;
    }

    public void SetLevelBackGroundColor(Color SetColor)
    {
        LevelBackGround.color = SetColor;
    }

    public void SetName(string Name)
    {
        ObjectName.text = Name;
    }

    public void SetLevel(string Level)
    {
        ObjectLevel.text = Level;
    }

    #endregion

    #region 处理血条缓冲的协程
    private IEnumerator DoHPFadeEffect (float damage,float valueRate)
    {
        var suddenChangeBar = damage >= 0 ? HPFadeBar : HPBar;
        var slowChangeBar = damage >= 0 ? HPBar : HPFadeBar;
        suddenChangeBar.fillAmount = valueRate;
        while(Mathf.Abs(suddenChangeBar.fillAmount - slowChangeBar.fillAmount) >= 0.0001f)
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
        while (Mathf.Abs(suddenChangeBar.fillAmount - slowChangeBar.fillAmount) >= 0.0001f)
        {
            slowChangeBar.fillAmount = Mathf.Lerp(slowChangeBar.fillAmount, valueRate, Time.deltaTime * transpeed);
            yield return null;
        }
        slowChangeBar.fillAmount = valueRate;
    }
    #endregion

    void OnDestroy()
    {
        // 在销毁时从控制器中注销
        if (HVM != null)
        {
            HVM.UnregisterHealthBar(this);
        }
    }


}
