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
    public int itemCount;

    // 使用JsonUtility序列化和反序列化对象
    public ItemHold Clone()
    {
        // 将对象序列化为JSON字符串
        string json = JsonUtility.ToJson(this);
        // 将JSON字符串反序列化为新的对象
        return JsonUtility.FromJson<ItemHold>(json);
    }
}