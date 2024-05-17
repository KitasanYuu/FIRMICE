using CustomInspector;
using DataManager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemDetailUnit : MonoBehaviour
{
    [ReadOnly] public ItemCell currentCell;

    public VerticalLayoutGroup nameLayoutGroup;

    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemDescription;

    public Image headerCutline;
    public Image backGroundImage;
    public Image itemImage;

    private LocalDataSaver LDS = new LocalDataSaver();
    public void SelectItem(ItemCell ic)
    {
        currentCell = ic;
        itemName.text = LDS.GetItemName(ic.currentItemID);
        itemDescription.text = LDS.GetItemDescribe(ic.currentItemID);
        LayoutRebuilder.ForceRebuildLayoutImmediate(nameLayoutGroup?.GetComponent<RectTransform>());
    }
}