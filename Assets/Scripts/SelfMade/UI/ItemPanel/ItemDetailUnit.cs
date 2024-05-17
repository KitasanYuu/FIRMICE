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
    public TextMeshProUGUI itemCount;

    public GameObject interactiveButton;
    public GameObject itemCountObject;

    public Image headerCutline;
    public Image backGroundImage;
    public Image itemImage;

    private LocalDataSaver LDS = new LocalDataSaver();
    public void SelectItem(ItemCell ic)
    {
        if(LDS.GetItemType(ic.currentItemID) == 2)
        {
            interactiveButton.SetActive(true);
        }
        else
        {
            interactiveButton.SetActive(false);
        }

        if (LDS.GetItemStackAbility(ic.currentItemID) == 1)
        {
            itemCountObject.SetActive(false);
        }
        else
        {
            itemCountObject.SetActive(true);
            itemCount.text = ic.currentItemCount.ToString();
        }

        currentCell = ic;
        itemName.text = LDS.GetItemName(ic.currentItemID);
        itemDescription.text = LDS.GetItemDescribe(ic.currentItemID);
        LayoutRebuilder.ForceRebuildLayoutImmediate(nameLayoutGroup?.GetComponent<RectTransform>());
    }
}