using AvatarMain;
using BattleHealth;
using CustomInspector;
using Detector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YuuTool;

public class PlayerStatusManager : MonoBehaviour
{
    [HorizontalLine(message: "总面板", 2, FixedColor.Gray)]
    [ReadOnly] public GameObject AmmoCount;
    [ReadOnly] public GameObject PlayerDetail;

    [HorizontalLine(message: "HP面板", 2, FixedColor.Gray)]
    [ReadOnly] public GameObject PlayerHealthBar;
    [ReadOnly] public Image _playerHealthBar;
    [ReadOnly] public GameObject PlayerHealthFadeBar;
    [ReadOnly] public Image _playerHealthFadeBar;

    [HorizontalLine(message: "Ammo面板", 2, FixedColor.Gray)]
    [ReadOnly] public GameObject CurrentAmmo;
    [ReadOnly] public TextMeshProUGUI _currentAmmo;
    [ReadOnly] public GameObject AmmoTotal;
    [ReadOnly] public TextMeshProUGUI _ammoTotal;
    [ReadOnly] public GameObject AmmoPreMag;
    [ReadOnly] public TextMeshProUGUI _ammoPreMag;

    [HorizontalLine(message: "Skill面板", 2, FixedColor.Gray)]
    [ReadOnly] public GameObject Skill1;
    [ReadOnly] public GameObject Skill2;
    [ReadOnly] public GameObject Skill3;

    [HorizontalLine(message: "消耗品面板", 2, FixedColor.Gray)]
    [ReadOnly] public GameObject Con1Num;
    [ReadOnly] public TextMeshProUGUI _con1Num;
    [ReadOnly] public GameObject Con2Num;
    [ReadOnly] public TextMeshProUGUI _con2Num;

    private float PreviousHP = -1;
    private float PreviousTotalHP = -1;

    private bool resourceInitComplete;
    private bool componentInitComplete;
    private bool registerInfoIn;

    private bool _initComplete;

    private VirtualHP _virtualHP;
    private RayDectec _rayDetec;
    private AvatarController _avatarController;

    private Coroutine HPBarFadeCoroutine;

    void Start()
    {
        ResourceInit();
        ComponentInit();
    }

    void Update()
    {
        UIParameterUpdate();
    }

    private void UIParameterUpdate()
    {
        if (_virtualHP != null)
        {
            float TotalHP = _virtualHP.TotalHP;
            float CurrentHP = _virtualHP.CurrentHP;
            float Armor = _virtualHP.Armor;
            float CurrentArmor = _virtualHP.CurrentArmor;
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

            //Debug.Log(HPdamageDir);
            //HPBar.fillAmount = HPValueRate;
            //ArmorBar.fillAmount = ArmorValueRate;
        }

    }

    private IEnumerator DoHPFadeEffect(float damage, float valueRate)
    {
        var suddenChangeBar = damage >= 0 ? _playerHealthFadeBar : _playerHealthBar;
        var slowChangeBar = damage >= 0 ? _playerHealthBar : _playerHealthFadeBar;
        suddenChangeBar.fillAmount = valueRate;
        while (Mathf.Abs(suddenChangeBar.fillAmount - slowChangeBar.fillAmount) >= 0.0001f)
        {
            slowChangeBar.fillAmount = Mathf.Lerp(slowChangeBar.fillAmount, valueRate, Time.deltaTime * 2f);
            yield return null;
        }
        slowChangeBar.fillAmount = valueRate;
    }

    private void ResourceInit()
    {
        AmmoCount = transform.FindDeepChild("AmmoCount").gameObject;
        PlayerDetail = transform.FindDeepChild("PlayerDetail").gameObject;

        PlayerHealthBar = transform.FindDeepChild("PlayerHealthBar").gameObject;
        PlayerHealthFadeBar = transform.FindDeepChild("PlayerHealthFadeBar").gameObject;

        CurrentAmmo = transform.FindDeepChild("CurrentAmmo").gameObject;
        AmmoTotal = transform.FindDeepChild("AmmoTotal").gameObject;
        AmmoPreMag = transform.FindDeepChild("AmmoPreMag").gameObject;

        Skill1 = transform.FindDeepChild("Skill1").gameObject;
        Skill2 = transform.FindDeepChild("Skill2").gameObject;
        Skill3 = transform.FindDeepChild("Skill3").gameObject;

        Con1Num = transform.FindDeepChild("Con1Num").gameObject;
        Con2Num = transform.FindDeepChild("Con2Num").gameObject;

        resourceInitComplete = true;
        InitCheck();
    }

    private void ComponentInit()
    {
        _playerHealthBar = PlayerHealthBar.GetComponent<Image>();
        _playerHealthFadeBar = PlayerHealthFadeBar.GetComponent<Image>();
        _currentAmmo = CurrentAmmo.GetComponent<TextMeshProUGUI>();
        _ammoTotal = AmmoTotal.GetComponent<TextMeshProUGUI>();
        _ammoPreMag = AmmoPreMag.GetComponent<TextMeshProUGUI>();
        _con1Num = Con1Num.GetComponent<TextMeshProUGUI>();
        _con2Num = Con2Num.GetComponent<TextMeshProUGUI>();

        componentInitComplete = true;
        InitCheck();
    }

    public void HPRegister(GameObject Register)
    {
        _virtualHP = Register.GetComponent<VirtualHP>();
        _avatarController = Register.GetComponent<AvatarController>();
        _rayDetec = Register.GetComponent<RayDectec>();

        registerInfoIn = true;
        InitCheck();
    }

    private void InitCheck()
    {
        if(registerInfoIn && resourceInitComplete && componentInitComplete)
        {
            _initComplete= true;
        }
    }
}
