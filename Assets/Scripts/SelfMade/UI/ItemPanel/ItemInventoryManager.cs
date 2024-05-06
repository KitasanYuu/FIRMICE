using CustomInspector;
using DataManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YuuTool;

public class ItemInventoryManager : MonoBehaviour
{
    public bool TestButton;
    public InventoryTestSO testSOData;
    public GameObject itemGrip;
    public Dictionary<string, int> iItems = new Dictionary<string, int>();
    public List<ItemCell> items;
    private List<ItemHold> iData = new List<ItemHold>();

    [ReadOnly] public GameObject contentAnchor;

    private LocalDataSaver LDS = new LocalDataSaver();
    // Start is called before the first frame update
    void Start()
    {
        contentAnchor = transform.FindDeepChild("ItemLayoutContent").gameObject;

        iData = testSOData.ItemList;

        RefeshList();
    }

    // Update is called once per frame
    void Update()
    {
        if (TestButton)
        {
            RefeshList();
            TestButton = false;
        }
    }

    private void RefeshList()
    {
        // 清空现有物品列表
        foreach (ItemCell ic in items)
        {
            Destroy(ic.gameObject);
        }
        items.Clear();

        // 创建新的物品列表
        foreach (ItemHold item in iData)
        {
            if (item.itemID != null)
            {
                int stackLimit = LDS.GetItemStackAbility(item.itemID); // 获取物品堆叠上限
                int itemCount = item.itemCount;

                // 遍历现有的物品单元格
                foreach (ItemCell ic in items)
                {
                    if (ic.currentItemID == item.itemID)
                    {
                        // 如果找到相同类型的物品，则将其堆叠数量增加
                        int spaceLeft = stackLimit - ic.currentItemCount;
                        int addedQuantity = Mathf.Min(spaceLeft, itemCount);
                        ic.currentItemCount += addedQuantity;
                        itemCount -= addedQuantity;

                        // 如果堆叠数量已满，则退出循环
                        if (ic.currentItemCount >= stackLimit)
                            break;
                    }
                }

                // 如果未找到相同类型的物品或堆叠数量已满，则创建新的物品单元格
                while (itemCount > 0)
                {
                    // 创建新的物品单元格
                    GameObject grip = Instantiate(itemGrip, contentAnchor.transform);
                    ItemCell cell = grip.GetComponent<ItemCell>();

                    // 设置物品单元格的数据
                    cell.currentItemID = item.itemID;
                    cell.currentItemCount = Mathf.Min(itemCount, stackLimit); // 设置堆叠数量
                    grip.name = item.itemID;
                    items.Add(cell);

                    // 更新剩余物品数量
                    itemCount -= stackLimit;
                }
            }
        }
    }



}
