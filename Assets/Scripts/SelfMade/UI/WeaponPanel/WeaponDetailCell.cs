using CustomInspector;
using DataManager;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using YuuTool;

public class WeaponDetailCell : MonoBehaviour
{
    [ReadOnly] public string _currentWeaponSelected;
    [ReadOnly] public GameObject _weaponDetailValue;
    [ReadOnly] public GameObject _weapondetaildescribe;
    [ReadOnly] public TextMeshProUGUI _weaponDetailName;
    [ReadOnly] public TextMeshProUGUI _weaponDetailType;
    [ReadOnly] public TextMeshProUGUI _weaponDetailDescribe;
    [ReadOnly] public TextMeshProUGUI _weaponDamage;
    [ReadOnly] public TextMeshProUGUI _weaponArmorBreak;

    private LocalDataSaver LDS = new LocalDataSaver();

    void Start()
    {
        ComponentInit();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ComponentInit()
    {
        _weaponDetailValue = transform.FindDeepChild("WeaponDetailValue")?.gameObject;
        _weapondetaildescribe = transform.FindDeepChild("WeaponDetailDescribe")?.gameObject;
        _weaponDetailName = transform.FindDeepChild("WeaponDetailName")?.gameObject.GetComponent<TextMeshProUGUI>();
        _weaponDetailType = transform.FindDeepChild("WeaponDetailType")?.gameObject.GetComponent<TextMeshProUGUI>();
        _weaponDetailDescribe = transform.FindDeepChild("WeaponDetailDescribe")?.gameObject.GetComponent<TextMeshProUGUI>();
        _weaponDamage = transform.FindDeepChild("DamageValue")?.gameObject.GetComponent<TextMeshProUGUI>();
        _weaponArmorBreak = transform.FindDeepChild("ArmorBreakValue")?.gameObject.GetComponent<TextMeshProUGUI>();

        gameObject.SetActive(false);
    }

    public void SelectedWeaponChanged(string WeaponID)
    {
        if (WeaponID != null)
        {
            var weaponInfo = LDS.GetWeapon(WeaponID);

            float weapondamage = (float)weaponInfo["Damage"];
            _weaponDamage.text = weapondamage.ToString();
            float armorbreak = (float)weaponInfo["ArmorBreak"];
            _weaponArmorBreak.text = armorbreak.ToString();
            _weaponDetailDescribe.text = (string)weaponInfo["WeaponDescribe"];

            _weaponDetailValue.SetActive(false);
            _weapondetaildescribe.SetActive(false);

            _weaponDetailName.text = (string)weaponInfo["WeaponName"];
            _weaponDetailType.text = LDS.GetWeaponType(WeaponID);

            _weaponDetailValue.SetActive(true);
            _weapondetaildescribe.SetActive(value: true);
        }
    }
}
