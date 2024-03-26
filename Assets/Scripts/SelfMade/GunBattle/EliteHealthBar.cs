using BattleHealth;
using CustomInspector;
using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using YuuTool;

public class EliteHealthBar : MonoBehaviour
{
    [ReadOnly] public GameObject Object;

    public float transpeed = 10f;

    public float detectionRadius = 5f; // 检测半径
    public LayerMask targetLayer; // 目标层级

    private VirtualHP VHP;
    private HPVisionManager HVM;
    private RectTransform healthBarRectTransform;
    private Animator _animator;

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
    private bool detectedObject = false; // 布尔变量用于跟踪是否检测到了物体
    private bool DestoryComfirm;

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
        SetStatus();
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

    private void SetStatus()
    {
        // 获取脚本所在物体的位置
        Vector3 center = Object.transform.position;

        // 检测范围内的物体
        Collider[] colliders = Physics.OverlapSphere(center, detectionRadius, targetLayer);

        if (colliders.Length > 0)
        {
            // 在这里处理检测到的物体
            detectedObject = true;
            // Debug.Log("检测到物体：" + colliders[0].gameObject.name);
        }
        else
        {
            detectedObject = false; // 如果没有检测到物体，将变量设置为false
        }

        if (detectedObject)
        {
            // 处理检测到物体的情况
            _animator.SetInteger("AppearStatus", 1);
        }
        else
        {
            // 处理没有检测到物体的情况
            _animator.SetInteger("AppearStatus", 2);
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
        _animator = GetComponent<Animator>();
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

    public void DestoryProgressInit(bool needDestory)
    {
        if(needDestory)
            _animator.SetInteger("AppearStatus", 2);
        DestoryComfirm = true;
    }

    public void SetDestoryAnimPara(int Complete)
    {
        if (Complete ==1 && DestoryComfirm)
            Destroy(this);
    }

    void OnDestroy()
    {
        // 在销毁时从控制器中注销
        if (HVM != null)
        {
            HVM.UnregisterEliteHealthBar(this);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // 获取物体的世界坐标
        Vector3 worldPosition = Object.transform.position;
        worldPosition.y = 0.0f;

        // 绘制在xz平面上的圆
        DrawCircleOnXZPlane(worldPosition, detectionRadius, 360);
    }

    private void DrawCircleOnXZPlane(Vector3 center, float radius, int segments)
    {
        Handles.color = new Color(1f, 0f, 1f, 1f);

        Vector3 axis = Vector3.up;  // 指定轴向为y轴，即绘制在xz平面上
        Handles.DrawWireDisc(center, axis, radius);
    }
#endif
}