using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TestInventory", menuName = "Global/TestInventory", order = 1)]
public class InventoryTestSO : ScriptableObject
{
    public List<WeaponHold> DataList = new List<WeaponHold>();
    public List<ItemHold> ItemList = new List<ItemHold>();


}

[System.Serializable]
public class WeaponHold
{
    public string ID;
}

[System.Serializable]
public class ItemHold
{
    public string itemID;
}