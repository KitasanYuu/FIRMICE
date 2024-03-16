using BattleHealth;
using CustomInspector;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YuuTool;

public class EliteHealthBar : MonoBehaviour
{
    [ReadOnly] public GameObject Object;

    public float transpeed = 10f;

    private VirtualHP VHP;
    private HPVisionManager HVM;
    private RectTransform healthBarRectTransform;

    [SerializeField, ReadOnly] private Image HPBar;
    [SerializeField, ReadOnly] private Image HPFadeBar;
    [SerializeField, ReadOnly] private Image ArmorBar;
    [SerializeField, ReadOnly] private Image ArmorFadeBar;

    [Space2(20)]

    [SerializeField, ReadOnly] private TextMeshProUGUI ObjectTitle;
    [SerializeField, ReadOnly] private TextMeshProUGUI ObjectName;
    [SerializeField, ReadOnly] private TextMeshProUGUI ObjectLevel;

    private float PreviousHP=-1;
    private float PreviousTotalHP=-1;
    private float PreviousArmor = -1;
    private float PreviousTotalArmor=-1;

    private float CHPRate;
    private float CArmorRate;

    private Coroutine HPBarFadeCoroutine;
    private Coroutine ArmorBarFadeCoroutine;

    private bool ResourceInitComplete;
    private bool ComponentInitComplete;
    private bool InitParameterAccepted;

    private void Awake()
    {
        ResourceInit();
    }

    void Start()
    {
        ParameterInit();
        ComponentInit();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        UIParameterUpdate();
    }

    private void UIParameterUpdate()
    {
        if (VHP != null)
        {
            float TotalHP = VHP.TotalHP;
            float CurrentHP = VHP.CurrentHP;
            float Armor = VHP.Armor;
            float CurrentArmor = VHP.CurrentArmor;
            float HPValueRate;
            float ArmorValueRate;

            if (TotalHP == 0)
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

            if (PreviousArmor != CurrentArmor || PreviousTotalArmor != Armor)
            {
                float ArmordamageDir = 0;
                ArmordamageDir = CurrentArmor - PreviousArmor;
                PreviousArmor = CurrentArmor;
                PreviousTotalArmor = Armor;

                if (ArmorBarFadeCoroutine != null)
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

    #region 处理血条缓冲的协程
    private IEnumerator DoHPFadeEffect(float damage, float valueRate)
    {
        var suddenChangeBar = damage >= 0 ? HPFadeBar : HPBar;
        var slowChangeBar = damage >= 0 ? HPBar : HPFadeBar;
        suddenChangeBar.fillAmount = valueRate;
        while (Mathf.Abs(suddenChangeBar.fillAmount - slowChangeBar.fillAmount) >= 0.0001f)
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


    #region 外部设参
    public void SetParameter(GameObject RegisterObject, VirtualHP vhp,HPVisionManager hvm)
    {
        Object = RegisterObject;
        VHP = vhp;
        HVM = hvm;

        InitParameterAccepted = true;

        RequestParameterProgress();
    }


    public void SetInitBarParameter(float SHPBar, float SArmorBar)
    {
        HPBar.fillAmount = SHPBar;
        ArmorBar.fillAmount = SArmorBar;
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

    public void SetEliteTitle(string Title)
    {
        ObjectTitle.text = Title;
    }

    public void SetEName(string Name)
    {
        ObjectName.text = Name;
        Debug.Log("111");
    }

    public void SetNameColor(Color namecolor)
    {
        ObjectName.color = namecolor;
    }

    public void SetTitleColor(Color titlecolor)
    {
        ObjectTitle.color = titlecolor;
    }

    public void SetLevel(string Level)
    {
        ObjectLevel.text = Level;
    }

    #endregion

    private void RequestParameterProgress()
    {
        if (ResourceInitComplete && ResourceInitComplete&& InitParameterAccepted)
            HVM.ParameterSetProgress(gameObject, Object);
    }

    private void ComponentInit()
    {
        // 获取血条的RectTransform组件
        healthBarRectTransform = GetComponent<RectTransform>();
        if(HVM == null)
            HVM = GetComponentInParent<HPVisionManager>();
        if (HVM != null)
        {
            HVM.RegisterEliteHealthBar(this);
        }

        ComponentInitComplete = true;
        RequestParameterProgress();
    }

    private void ParameterInit()
    {
        HPBar.fillAmount = 0;
        ArmorBar.fillAmount = 0;
    }

    private void ResourceInit()
    {
        ObjectTitle = transform.FindDeepChild("EliteTitle")?.gameObject.GetComponent<TextMeshProUGUI>();
        ObjectName = transform.FindDeepChild("EliteName")?.gameObject.GetComponent<TextMeshProUGUI>();
        ObjectLevel = transform.FindDeepChild("EliteLevel")?.gameObject.GetComponent<TextMeshProUGUI>();

        HPBar = transform.FindDeepChild("EliteHPBar")?.gameObject.GetComponent<Image>();
        HPFadeBar = transform.FindDeepChild("EliteHPFadeBar ")?.gameObject.GetComponent<Image>();
        ArmorBar = transform.FindDeepChild("EliteArmorBar")?.gameObject.GetComponent<Image>();
        ArmorFadeBar = transform.FindDeepChild("EliteArmorFadeBar ")?.gameObject.GetComponent<Image>();

        ResourceInitComplete = true;
        RequestParameterProgress();
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