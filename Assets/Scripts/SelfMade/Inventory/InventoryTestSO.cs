using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TestInventory", menuName = "Global/TestInventory", order = 1)]
public class InventoryTestSO : ScriptableObject
{
    public List<WeaponHold> DataList = new List<WeaponHold>();


}

[System.Serializable]
public class WeaponHold
{
    public string ID;
}