using CustomInspector;
using System.Collections;
using System.Collections.Generic;
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

    public void SelectItem(ItemCell ic)
    {
        currentCell = ic;
        itemName.text = ic.currentItemID;
        LayoutRebuilder.ForceRebuildLayoutImmediate(nameLayoutGroup?.GetComponent<RectTransform>());
    }
}