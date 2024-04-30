using CustomInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YuuTool;

public class ItemInventoryManager : MonoBehaviour
{
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
        foreach (ItemHold item in iData)
        {
            if(item.itemID!= null)
            {
                GameObject grip = Instantiate(itemGrip, contentAnchor.transform);
                ItemCell cell = grip.GetComponent<ItemCell>();
                cell.currentItemID = item.itemID;
                grip.name = item.itemID;
                items.Add(cell);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
