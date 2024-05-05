using CustomInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YuuTool;

public class ItemInventoryManager : MonoBehaviour
{
    public bool TestButton;
    public InventoryTestSO testSOData;
    public GameObject itemGrip;
    public List<ItemCell> items;
    private List<ItemHold> iData = new List<ItemHold>();

    [ReadOnly] public GameObject contentAnchor;

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
        // 创建临时列表保存要删除的元素
        List<ItemCell> cellsToRemove = new List<ItemCell>();

        // 遍历要删除的元素并加入到临时列表中
        foreach (ItemCell ic in items)
        {
            cellsToRemove.Add(ic);
        }

        // 删除临时列表中的元素
        foreach (ItemCell ic in cellsToRemove)
        {
            items.Remove(ic);
            Destroy(ic.gameObject);
        }

        foreach (ItemHold item in iData)
        {
            if (item.itemID != null)
            {
                GameObject grip = Instantiate(itemGrip, contentAnchor.transform);
                ItemCell cell = grip.GetComponent<ItemCell>();
                cell.currentItemID = item.itemID;
                grip.name = item.itemID;
                items.Add(cell);
            }
        }
    }

}
