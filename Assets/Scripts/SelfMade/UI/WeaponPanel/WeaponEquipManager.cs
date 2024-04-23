using DataManager;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WeaponEquipManager : MonoBehaviour
{
    public List<WeaponEquipCell> EquipCells = new List<WeaponEquipCell>();

    public List<LockedWeapon> _selectedWeapon = new List<LockedWeapon>();

    private WeaponPanel _weaponPanel;
    private WeaponInventoryManager WIM;

    private ResourceReader RR = new ResourceReader();

    private void Start()
    {
        ComponentInit();
        ResourceInit();
    }

    private void ResourceInit()
    {
        _selectedWeapon = WIM.LockedWeapon;
    }

    private void ComponentInit()
    {
        _weaponPanel = GetComponentInParent<WeaponPanel>();
        WIM = _weaponPanel._weaponSelectPanel.GetComponent<WeaponInventoryManager>();
    }

    public void UpdateSelectedWeapon(int weaponPage)
    {
        if (_selectedWeapon.Count == 0)
        {
            foreach(WeaponEquipCell WEC in EquipCells)
            {
                UpdateWeaponCell(WEC, 9999);
            }
        }
        else
        {
            LockedWeapon selectedWeapon = _selectedWeapon.FirstOrDefault(L => L._lockedPage == weaponPage);
            if (selectedWeapon == null)
            {
                UpdateWeaponCell(EquipCells[weaponPage], 9999);
            }
            else
            {
                UpdateWeaponCell(EquipCells[weaponPage], weaponPage);
            }
        }
        
    }

    public void AddEquipCell(WeaponEquipCell WEC)
    {
        if (!EquipCells.Contains(WEC))
        {
            EquipCells.Add(WEC);
            EquipCells = EquipCells.OrderBy(cell => cell.PageNum).ToList();
        }
    }

    private void UpdateWeaponCell(WeaponEquipCell weaponCell, int weaponPage, LockedWeapon selectedWeapon = null)
    {
        if(weaponPage == 9999)
        {
            weaponCell._weaponName.text = "未指派";
            weaponCell._weaponType.text = null;
            weaponCell._weaponImage.color = WIM._weaponDefaultColor;
            weaponCell._weaponImage.sprite = RR.GetPlaceHolderSprite("ItemNotAvaible");
        }
        else
        {
            LockedWeapon _selectedweapon = null;

            if (selectedWeapon == null)
            {
                _selectedweapon = _selectedWeapon.FirstOrDefault(L => L._lockedPage == weaponPage);
            }
            else
            {
                _selectedweapon = selectedWeapon;
            }
                
            if (_selectedweapon != null)
            {
                WeaponCell _weaponCell = _selectedweapon._lockedWeapon.GetComponent<WeaponCell>();
                weaponCell._weaponName.text = _weaponCell._weaponName.text;
                weaponCell._weaponType.text = _weaponCell._weaponType.text;
                weaponCell._weaponImage.color = Color.white;
                weaponCell._weaponImage.sprite = _weaponCell._weaponImage.sprite;
            }
        }
    }

}
