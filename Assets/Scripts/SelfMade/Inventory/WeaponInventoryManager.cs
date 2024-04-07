using CustomInspector;
using DataManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YuuTool;

public class WeaponInventoryManager : MonoBehaviour
{
    public InventoryTestSO weapons;
    public List<WeaponHold> DataList = new List<WeaponHold>();
    [ReadOnly] public List<GameObject> WeaponGrip = new List<GameObject>();
    public List<WeaponHold> _previousDataList = new List<WeaponHold>();
    public GameObject _weaponPanelPrefab;

    public Color _weaponSelectedColor;
    public Color _weaponDefaultColor;

    private Transform _weaponLayout;

    private WeaponDetailCell WDC;
    private Kacha _kacha;
    private LocalDataSaver LDS = new LocalDataSaver();
    private ResourceReader RR = new ResourceReader();

    private WeaponCell previousSelectWeapon;

    void Start()
    {
        ResourcesInit();
        ComponentInit();
        DataList = weapons.DataList;
        panelGenerate();

    }

    // Update is called once per frame
    void Update()
    {
        WeaponGrip.RemoveAll(item => item == null);

        if(_previousDataList != DataList)
        {
            foreach(GameObject v in WeaponGrip)
            {
                Destroy(v);
            }

            panelGenerate();
            _previousDataList = DataList;
        }
    }

    private void panelGenerate()
    {
        foreach (var weapon in DataList)
        {
            GameObject _weaponPanel = Instantiate(_weaponPanelPrefab, _weaponLayout);
            WeaponGrip.Add(_weaponPanel);
            WeaponCell _weaponcell = _weaponPanel.GetComponent<WeaponCell>();

            _weaponcell.SetStartParameter(weapon.ID, this,WDC,_weaponSelectedColor,_weaponDefaultColor);
            //_weaponcell._weaponName.text = weaponName;
        }
    }

    public void WeaponSelected(WeaponCell _weaponCell)
    {
        if(previousSelectWeapon != _weaponCell)
        {
            previousSelectWeapon?.SetSelectStatus(false);
            _weaponCell.SetSelectStatus(true);
            previousSelectWeapon = _weaponCell;
        }
    }

    public void SelectWeaponChangedRequest(string CurrentSelectWeaponID)
    {
        WDC._currentWeaponSelected = CurrentSelectWeaponID;
        WDC.SelectedWeaponChanged(CurrentSelectWeaponID);
    }

    public void RequestInfo(WeaponCell wc,string weaponID)
    {
        string weaponName = (string)LDS.GetWeapon(weaponID)["WeaponName"];
        wc._weaponName.text = weaponName;
        wc._weaponType.text = LDS.GetWeaponType(weaponID);
        _kacha.targetImage = wc._weaponImage;
        _kacha.CaptureSnapShot("111");
    }


    private void ResourcesInit()
    {
        _weaponDefaultColor = RR.GetColor("WeaponDefaultColor");
        _weaponSelectedColor = RR.GetColor("WeaponSelectedColor");
    }

    private void ComponentInit()
    {
        _weaponLayout = transform.FindDeepChild("LayoutContent");
        _kacha = GetComponent<Kacha>();
        WDC = transform.FindDeepChild("WeaponDetail").gameObject.GetComponent<WeaponDetailCell>();

    }
}
