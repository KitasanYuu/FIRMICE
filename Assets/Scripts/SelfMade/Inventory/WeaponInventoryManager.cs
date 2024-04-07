using CustomInspector;
using DataManager;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YuuTool;

public class WeaponInventoryManager : MonoBehaviour
{
    public InventoryTestSO weapons;
    public List<WeaponHold> DataList = new List<WeaponHold>();
    [ReadOnly] public List<GameObject> WeaponGrip = new List<GameObject>();
    private Dictionary<WeaponHold, GameObject> _weaponGripDictionary = new Dictionary<WeaponHold, GameObject>();
    private List<WeaponHold> _previousDataList = new List<WeaponHold>();
    public GameObject _weaponPanelPrefab;

    public Color _weaponSelectedColor;
    public Color _weaponDefaultColor;

    private Transform _weaponLayout;

    private WeaponDetailCell WDC;
    private WeaponRender _weaponRender;
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

        List<WeaponHold> addedWeapons = new List<WeaponHold>();
        List<WeaponHold> removedWeapons = new List<WeaponHold>();

        // 寻找新增的物体和被删除的物体
        foreach (var weapon in DataList)
        {
            if (!_previousDataList.Contains(weapon))
                addedWeapons.Add(weapon);
        }

        foreach (var weapon in _previousDataList)
        {
            if (!DataList.Contains(weapon))
                removedWeapons.Add(weapon);
        }

        // 如果有新增或删除的物体，则更新 UI
        if (addedWeapons.Count > 0 || removedWeapons.Count > 0)
        {
            // 先停止所有协程
            StopAllCoroutines();

            // 处理新增和修改的物体
            panelGenerate();

            // 更新 _previousDataList
            _previousDataList = new List<WeaponHold>(DataList);
        }
    }

    private void panelGenerate()
    {
        // 首先确保 WeaponGrip 和 _weaponGripDictionary 中没有空的 GameObject
        WeaponGrip.RemoveAll(item => item == null);
        _weaponGripDictionary = _weaponGripDictionary.Where(kvp => kvp.Value != null).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        foreach (var weapon in DataList)
        {
            GameObject _weaponPanel;

            // 检查是否存在相应的 GameObject，如果存在则更新，否则实例化新的 GameObject
            if (_weaponGripDictionary.ContainsKey(weapon))
            {
                _weaponPanel = _weaponGripDictionary[weapon];
                UpdateWeaponPanel(_weaponPanel, weapon);
            }
            else
            {
                _weaponPanel = Instantiate(_weaponPanelPrefab, _weaponLayout);
                WeaponGrip.Add(_weaponPanel);

                // 将 WeaponHold 和 GameObject 添加到字典
                _weaponGripDictionary.Add(weapon, _weaponPanel);
            }

            // Set parameters for newly instantiated or updated weapon panel
            WeaponCell _weaponcell = _weaponPanel.GetComponent<WeaponCell>();
            _weaponcell.SetStartParameter(weapon.ID, this, WDC, _weaponSelectedColor, _weaponDefaultColor);
        }

        // 处理删除的物体
        var panelsToRemove = new List<GameObject>();
        foreach (var kvp in _weaponGripDictionary)
        {
            if (!DataList.Contains(kvp.Key))
            {
                panelsToRemove.Add(kvp.Value);
            }
        }

        // 移除需要删除的面板
        foreach (var panel in panelsToRemove)
        {
            Destroy(panel);
            WeaponGrip.Remove(panel);
            //_weaponGripDictionary.Remove(panel.GetComponent<WeaponCell>().WeaponHold);
        }
    }

    // 更新单个武器面板
    private void UpdateWeaponPanel(GameObject weaponPanel, WeaponHold weapon)
    {
        WeaponCell _weaponcell = weaponPanel.GetComponent<WeaponCell>();
        _weaponcell.SetStartParameter(weapon.ID, this, WDC, _weaponSelectedColor, _weaponDefaultColor);
    }

    public void WeaponSelected(WeaponCell _weaponCell)
    {
        if (previousSelectWeapon != _weaponCell)
        {
            if (previousSelectWeapon != null)
            {
                previousSelectWeapon?.SetSelectStatus(false);
            }

            _weaponCell.SetSelectStatus(true);
            previousSelectWeapon = _weaponCell;
        }
    }

    public void SelectWeaponChangedRequest(string CurrentSelectWeaponID)
    {
        WDC._currentWeaponSelected = CurrentSelectWeaponID;
        WDC.SelectedWeaponChanged(CurrentSelectWeaponID);
    }

    public void RequestInfo(WeaponCell wc, string weaponID)
    {
        string weaponName = (string)LDS.GetWeapon(weaponID)["WeaponName"];
        wc._weaponName.text = weaponName;
        wc._weaponType.text = LDS.GetWeaponType(weaponID);
        if (_weaponRender.WeaponPrefabComfirm(weaponID))
        {
            Sprite loadedSprite = ImageLoader.LoadImageAsSprite(weaponID + ".YPic");
            if (loadedSprite != null)
            {
                Debug.Log("UsingSavingPic");
                wc._weaponImage.color = Color.white;
                wc._weaponImage.sprite = loadedSprite;
            }
            else
            {
                Debug.Log("UsingKacha");
                _kacha.targetImage = wc._weaponImage;
                _kacha.CaptureSnapShot(weaponID);
            }
        }
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
        _weaponRender = GetComponent<WeaponRender>();
        WDC = transform.FindDeepChild("WeaponDetail").gameObject.GetComponent<WeaponDetailCell>();
    }
}
