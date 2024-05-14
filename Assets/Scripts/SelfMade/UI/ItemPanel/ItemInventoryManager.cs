using CustomInspector;
using DataManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YuuTool;

public class ItemInventoryManager : MonoBehaviour
{
    [ReadOnly] public ItemCell currentSelectItem;
    [HideInInspector] private ItemCell previousSelectItem;
    [Space2(10)]
    public bool TestButton;
    public InventoryTestSO testSOData;
    public GameObject itemGrip;
    public ItemDetailUnit IDU;
    public Dictionary<string, int> iItems = new Dictionary<string, int>();
    public List<ItemCell> items;
    private List<ItemHold> iData = new List<ItemHold>();

    [ReadOnly] public GameObject contentAnchor;

    private LocalDataSaver LDS = new LocalDataSaver();
    private int itemsPerFrame = 100; // 每帧生成的物体个数
    private IEnumerator currentCoroutine;

    private void OnEnable()
    {
        RefreshList();
    }

    // Start is called before the first frame update
    void Start()
    {
        contentAnchor = transform.FindDeepChild("ItemLayoutContent").gameObject;

        iData = testSOData.ItemList;

        RefreshList();
    }

    // Update is called once per frame
    void Update()
    {
        if (TestButton)
        {
            TestButton = false;
            RefreshList();
        }
    }

    public void SelectCell(ItemCell ic)
    {
        if(currentSelectItem != null)
        {
            previousSelectItem = currentSelectItem;
            previousSelectItem.SetSelectStatus(false);
        }

        currentSelectItem = ic;
        ic.SetSelectStatus(true);
        IDU.SelectItem(currentSelectItem);
    }

    private void RefreshList()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }

        // 清空现有物品列表
        foreach (ItemCell ic in items)
        {
            Destroy(ic.gameObject);
        }
        items.Clear();

        // 创建新的物品列表
        currentCoroutine = GenerateItemsCoroutine();
        StartCoroutine(currentCoroutine);

        if(currentSelectItem == null && items.Count >0)
        {
            SelectCell(items[0]);
        }
    }

    private IEnumerator GenerateItemsCoroutine()
    {
        List<ItemHold> tempData = new List<ItemHold>();

        foreach (ItemHold item in iData)
        {
            ItemHold newItem = item.Clone();
            //newItem.itemID = item.itemID;
            //newItem.itemCount = item.itemCount;
            // 如果 ItemHold 类中还有其他属性，也需要进行类似的复制操作

            tempData.Add(newItem);
        }

        int itemCount = 0;

        // 创建新的物品列表
        foreach (ItemHold item in tempData)
        {
            if (item.itemID != null)
            {
                int stackLimit = LDS.GetItemStackAbility(item.itemID); // 获取物品堆叠上限

                // 遍历现有的物品单元格
                foreach (ItemCell ic in items)
                {
                    if (ic.currentItemID == item.itemID)
                    {
                        // 如果找到相同类型的物品，则将其堆叠数量增加
                        int spaceLeft = stackLimit - ic.currentItemCount;
                        int addedQuantity = Mathf.Min(spaceLeft, item.itemCount);
                        ic.currentItemCount += addedQuantity;
                        item.itemCount -= addedQuantity;

                        // 如果堆叠数量已满，则退出循环
                        if (ic.currentItemCount >= stackLimit)
                            break;
                    }
                }

                // 如果未找到相同类型的物品或堆叠数量已满，则创建新的物品单元格
                while (item.itemCount > 0)
                {
                    // 创建新的物品单元格
                    GameObject grip = Instantiate(itemGrip, contentAnchor.transform);
                    ItemCell cell = grip.GetComponent<ItemCell>();

                    // 设置物品单元格的数据
                    cell.currentItemID = item.itemID;
                    cell.currentItemCount = Mathf.Min(item.itemCount, stackLimit); // 设置堆叠数量
                    grip.name = item.itemID;
                    items.Add(cell);

                    if (cell.IIM == null)
                    {
                        cell.SetParameter(this);
                    }

                    // 更新剩余物品数量
                    item.itemCount -= stackLimit;
                    itemCount++;

                    // 达到每帧生成物品的上限，等待下一帧再生成
                    if (itemCount >= itemsPerFrame)
                    {
                        yield return null;
                        itemCount = 0;
                    }
                }
            }
        }
    }


}
