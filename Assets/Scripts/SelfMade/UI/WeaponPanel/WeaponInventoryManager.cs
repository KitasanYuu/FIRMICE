using Battle;
using CustomInspector;
using DataManager;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YuuTool;

public class WeaponInventoryManager : MonoBehaviour
{
    [ReadOnly] public string CurrentSelectedWeapon;
    [ReadOnly] public GameObject CurrentSelectedGrip;
    public InventoryTestSO weapons;
    public List<WeaponHold> DataList = new List<WeaponHold>();
    public List<LockedWeapon> LockedWeapon = new List<LockedWeapon>();
    public List<CurrentSelectedWeapon> SelectedWeapons = new List<CurrentSelectedWeapon>();
    [ReadOnly] public List<GameObject> WeaponGrip = new List<GameObject>();
    private Dictionary<WeaponHold, GameObject> _weaponGripDictionary = new Dictionary<WeaponHold, GameObject>();
    private List<WeaponHold> _previousDataList = new List<WeaponHold>();
    public GameObject _weaponPanelPrefab;

    public Color _weaponSelectedColor;
    public Color _weaponDefaultColor;
    public Color _weaponOccupiedColor;

    private AudioClip _hoverSound;
    private AudioClip _clickSound;

    private Transform _weaponLayout;

    private WeaponDetailCell WDC;
    private WeaponRender _weaponRender;
    private Kacha _kacha;
    [HideInInspector]public PanelIdentity _panelID;
    private LocalDataSaver LDS = new LocalDataSaver();
    private ResourceReader RR = new ResourceReader();

    private WeaponCell previousSelectWeapon;

    private void OnEnable()
    {
        if (LockedWeapon.Count > 0)
        {
            foreach(LockedWeapon l in LockedWeapon)
            {
                if(l._lockedPage != _panelID.PageNum)
                {
                    l._lockedWeapon.GetComponent<WeaponCell>().SetOccupyStatus(true);
                    l._lockedWeapon.GetComponent<WeaponCell>().SetSelectStatus(false);
                }

                if (l._lockedPage == _panelID.PageNum)
                {
                    previousSelectWeapon = l._lockedWeapon.GetComponent<WeaponCell>();
                    previousSelectWeapon.PanelRecovering();
                    previousSelectWeapon.SetOccupyStatus(false);
                    previousSelectWeapon.SetSelectStatus(true);
                }
            }
        }
    }

    private void OnDisable()
    {
        if(CurrentSelectedGrip != null)
        {
            WeaponCell wc = CurrentSelectedGrip.GetComponent<WeaponCell>();
            WeaponLockRequest(CurrentSelectedGrip, _panelID.PageNum);
            CurrentSelectedGrip = null;
        }

    }

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
                _weaponPanel.name = "Layout_"+weapon.ID;
                WeaponGrip.Add(_weaponPanel);

                // 将 WeaponHold 和 GameObject 添加到字典
                _weaponGripDictionary.Add(weapon, _weaponPanel);
            }

            // Set parameters for newly instantiated or updated weapon panel
            WeaponCell _weaponcell = _weaponPanel.GetComponent<WeaponCell>();
            _weaponcell.SetStartParameter(weapon.ID, this, WDC,_clickSound,_hoverSound ,_weaponSelectedColor, _weaponDefaultColor,_weaponOccupiedColor);
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
        _weaponcell.SetStartParameter(weapon.ID, this, WDC,_clickSound,_hoverSound, _weaponSelectedColor, _weaponDefaultColor,_weaponOccupiedColor);
    }

    public void WeaponSelected(WeaponCell _weaponCell,GameObject Grip)
    {
        if (previousSelectWeapon != _weaponCell)
        {
            if (previousSelectWeapon != null)
            {
                previousSelectWeapon?.SetSelectStatus(false);
            }

            _weaponCell.SetSelectStatus(true);
            previousSelectWeapon = _weaponCell;
            CurrentSelectedGrip = Grip;
        }

        WeaponDetailChange(_weaponCell);
    }

    public void WeaponDetailChange(WeaponCell _weaponCell)
    {
        string CurrentSelectWeaponID = _weaponCell.WeaponID;

        if (CurrentSelectWeaponID == null)
        {
            WDC.gameObject.SetActive(false);
        }
        else
        {
            WDC.gameObject.SetActive(true);
        }

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
            Sprite loadedSprite = ImageLoader.LoadKachaImageAsSprite(weaponID + ".YPic");
            if (loadedSprite != null)
            {
                Debug.Log("UsingSavingPic");
                wc._weaponImage.color = Color.white;
                wc._weaponImage.sprite = loadedSprite;
            }
            else
            {
                Debug.Log("UsingKacha");
                _kacha.CaptureSnapShot(wc,weaponID,250);
            }
        }
    }

    public void WeaponLockRequest(GameObject lockedWeapon, int lockedPage)
    {
        WeaponUnlockRequest(lockedPage);

        // 添加新的 LockedWeapon
        LockedWeapon weapon = new LockedWeapon();
        weapon._lockedWeapon = lockedWeapon;
        weapon._lockedPage = lockedPage;
        LockedWeapon.Add(weapon);
    }

    public void WeaponUnlockRequest(int lockedPage)
    {
        // 遍历 LockedWeapons 列表，找到与 lockedPage 相同的项并移除
        for (int i = LockedWeapon.Count - 1; i >= 0; i--)
        {
            if (LockedWeapon[i]._lockedPage == lockedPage)
            {
                LockedWeapon.RemoveAt(i);
            }
        }
    }

    public void ClearCurrentWeaponSelect(int page)
    {
        CurrentSelectedGrip = null;
        CurrentSelectedWeapon = null;
        previousSelectWeapon = null;
        WeaponUnlockRequest(page);
    }

    private void ResourcesInit()
    {
        _hoverSound = RR.GetUIAudioClip("Hover");
        _clickSound = RR.GetUIAudioClip("Click");
        _weaponDefaultColor = RR.GetColor("WeaponDefaultColor");
        _weaponSelectedColor = RR.GetColor("WeaponSelectedColor");
        _weaponOccupiedColor = RR.GetColor("WeaponOccupiedColor");
    }

    private void ComponentInit()
    {
        _panelID = GetComponent<PanelIdentity>();
        _weaponLayout = transform.FindDeepChild("LayoutContent");
        _kacha = GetComponent<Kacha>();
        _weaponRender = GetComponent<WeaponRender>();
        WDC = transform.FindDeepChild("WeaponDetail").gameObject.GetComponent<WeaponDetailCell>();
    }
}

[System.Serializable]
public class LockedWeapon
{
    public GameObject _lockedWeapon;
    public int _lockedPage;
}

[System.Serializable]
public class CurrentSelectedWeapon
{
    public string _weaponID;
    public int _selectedPage;
}