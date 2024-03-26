using AvatarMain;
using Battle;
using BattleHealth;
using CustomInspector;
using Detector;
using playershooting;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YuuTool;

public class PlayerStatusManager : MonoBehaviour
{
    public bool TestButton;

    [HorizontalLine(message: "总面板", 2, FixedColor.Gray)]
    [ReadOnly] public GameObject Player;
    [ReadOnly] public GameObject AmmoCount;
    [ReadOnly] public GameObject PlayerDetail;

    [HorizontalLine(message: "HP面板", 2, FixedColor.Gray)]
    [ReadOnly] public GameObject PlayerHealthBar;
    [ReadOnly] public Image _playerHealthBar;
    [ReadOnly] public GameObject PlayerHealthFadeBar;
    [ReadOnly] public Image _playerHealthFadeBar;

    [HorizontalLine(message: "Ammo面板", 2, FixedColor.Gray)]
    [ReadOnly, SerializeField] private WeaponShooter CurrentWeapon;
    private WeaponShooter _previousWeapon;
    [ReadOnly] public GameObject CurrentAmmo;
    [ReadOnly] public TextMeshProUGUI _currentAmmo;
    [ReadOnly] public GameObject CurrentAmmoHide0;
    [ReadOnly] public GameObject AmmoTotal;
    [ReadOnly] public TextMeshProUGUI _ammoTotal;
    [ReadOnly] public GameObject AmmoTotalHide01;
    [ReadOnly] public GameObject AmmoTotalHide02;
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

    [HideInInspector] public Camera mainCamera;
    [HideInInspector] public GameObject _aimPanelAnchor;
    [HideInInspector] public GameObject _normalPanelAnchorR;
    [HideInInspector] public GameObject _normalPanelAnchorL;

    private float PreviousHP = -1;
    private float PreviousTotalHP = -1;

    private bool resourceInitComplete;
    private bool componentInitComplete;
    private bool registerInfoIn;

    private bool _initComplete;

    private RectTransform _panelRectTransform;
    private Canvas _renderCanvas;

    private VirtualHP _virtualHP;
    private RayDectec _rayDetec;
    private AvatarController _avatarController;
    private TPSShootController _tpsshootController;

    private Coroutine HPBarFadeCoroutine;

    [HideInInspector]
    public float ScaleFactor;

    void Start()
    {
        ResourceInit();
        ComponentInit();
    }

    void Update()
    {
        UIParameterUpdate();
        WeaponParameterUpdate();
    }

    private void LateUpdate()
    {
        PanelPositionAdjust();
    }

    private void PanelPositionAdjust()
    {
        Transform panelAnchor = null;

        if (_tpsshootController.isAiming)
        {
            _panelRectTransform.pivot = new Vector2(0.5f, 0.5f);
            panelAnchor = _aimPanelAnchor.transform;

            if(_tpsshootController.targetCameraSide ==1)
                PlayerDetail.transform.SetSiblingIndex(AmmoCount.transform.GetSiblingIndex() + 1);
            else if (_tpsshootController.targetCameraSide == 0)
                AmmoCount.transform.SetSiblingIndex(PlayerDetail.transform.GetSiblingIndex() + 1);
        }

        else if(!_tpsshootController.isAiming)
        {
            if (_rayDetec.isBlockedL)
            {
                _panelRectTransform.pivot = new Vector2(0f, 0.5f);
                panelAnchor = _normalPanelAnchorR.transform;
                PlayerDetail.transform.SetSiblingIndex(AmmoCount.transform.GetSiblingIndex() + 1);
            }
            else if (_rayDetec.isBlockedR)
            {
                _panelRectTransform.pivot = new Vector2(1f, 0.5f);
                panelAnchor = _normalPanelAnchorL.transform;
                AmmoCount.transform.SetSiblingIndex(PlayerDetail.transform.GetSiblingIndex() + 1);
            }
            else
            {
                _panelRectTransform.pivot = new Vector2(0f, 0.5f);
                panelAnchor = _normalPanelAnchorR.transform;
                PlayerDetail.transform.SetSiblingIndex(AmmoCount.transform.GetSiblingIndex() + 1);
            }

        }


        // 计算摄像机到NPC的距离
        float distance = Vector3.Distance(mainCamera.transform.position, Player.transform.position);
        //Debug.Log(distance);
        // 根据距离调整血条的缩放
        float scaleFactor = 0.7f + (0.3f * (distance - 2) / (6 - 2));
        ScaleFactor = scaleFactor;
        _panelRectTransform.localScale = new Vector3(scaleFactor, scaleFactor, 1);

        // 将NPC的世界坐标转换为屏幕坐标
        Vector3 screenPos = mainCamera.WorldToScreenPoint(panelAnchor.position + new Vector3(0, 0, 0));

        // 将屏幕坐标转换为Canvas坐标系下的位置
        Vector2 canvasPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_renderCanvas.GetComponent<RectTransform>(), screenPos, _renderCanvas.worldCamera, out canvasPos);
        // 更新血条的位置
        _panelRectTransform.localPosition = canvasPos;
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

    private void WeaponParameterUpdate()
    {
        CurrentWeapon = _tpsshootController?.CurrentWeapon;

        if (CurrentWeapon == null)
            return;

        if (CurrentWeapon.needReload)
            if(CurrentWeapon.CurrentBulletCount >= 10)
            {
                CurrentAmmoHide0.SetActive(false);
                CurrentAmmo.SetActive(true);

                _currentAmmo.text = CurrentWeapon.CurrentBulletCount.ToString();
            }
            else
            {
                CurrentAmmo.SetActive(true);
                CurrentAmmoHide0.SetActive(true);

                _currentAmmo.text = CurrentWeapon.CurrentBulletCount.ToString();
            }
        else
            _currentAmmo.text = "999";

        if (CurrentWeapon.LimitAmmo)
            if (CurrentWeapon.MaxAmmoCarry >= 100)
            {
                AmmoTotalHide01.SetActive(false);
                AmmoTotalHide02.SetActive(false);
                AmmoTotal.SetActive(true);

                _ammoTotal.text = CurrentWeapon.MaxAmmoCarry.ToString();
            }
            else if (CurrentWeapon.MaxAmmoCarry < 100 && CurrentWeapon.MaxAmmoCarry >= 10)
            {
                AmmoTotalHide01.SetActive(true);
                AmmoTotalHide02.SetActive(false);
                AmmoTotal.SetActive(true);

                _ammoTotal.text = CurrentWeapon.MaxAmmoCarry.ToString();
            }
            else if(CurrentWeapon.MaxAmmoCarry < 10)
            {
                AmmoTotalHide01.SetActive(true);
                AmmoTotalHide02.SetActive(true);
                AmmoTotal.SetActive(true);

                _ammoTotal.text = CurrentWeapon.MaxAmmoCarry.ToString();
            }

            else
                _ammoTotal.text = "9999";

        if (CurrentWeapon != _previousWeapon)
        {
            if (CurrentWeapon.needReload)
                _ammoPreMag.text = CurrentWeapon.AmmoPreMag.ToString();
            else
                _ammoPreMag.text = "999";
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
        CurrentAmmoHide0 = transform.FindDeepChild("CurrentAmmoHide0").gameObject;
        AmmoTotal = transform.FindDeepChild("AmmoTotal").gameObject;
        AmmoTotalHide01 = transform.FindDeepChild("AmmoTotalHide01").gameObject;
        AmmoTotalHide02 = transform.FindDeepChild("AmmoTotalHide02").gameObject;
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
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        _renderCanvas = GetComponentInParent<Canvas>();
        _panelRectTransform = GetComponent<RectTransform>();
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

    public void HPRegister(GameObject Register,GameObject AimPanelAnchor,GameObject PanelAnchorL, GameObject PanelAnchorR)
    {
        Player = Register;
        _virtualHP = Register?.GetComponent<VirtualHP>();
        _avatarController = Register?.GetComponent<AvatarController>();
        _tpsshootController = Register?.GetComponent<TPSShootController>();
        _rayDetec = Register?.GetComponent<RayDectec>();

        if(_virtualHP == null || _avatarController == null || _tpsshootController == null || _rayDetec == null)
        {
            _virtualHP.SetRegistResult(false);
            return;
        }


        if (AimPanelAnchor != null && PanelAnchorL != null && PanelAnchorR != null)
        {
            _aimPanelAnchor = AimPanelAnchor;
            _normalPanelAnchorL = PanelAnchorL;
            _normalPanelAnchorR = PanelAnchorR;
        }
        else
        {
            _virtualHP.SetRegistResult(false);
            return;
        }

        _virtualHP.SetRegistResult(true);
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
