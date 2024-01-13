using System;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using VInspector;
using CustomInspector;
using Michsky.UI.Heat;

[RequireComponent(typeof(LocalizationManager))]
public class FontManager : MonoBehaviour
{
    [HorizontalLine(1, FixedColor.Yellow, 0)]

    public List<TMP_FontAsset> CNfontList = new List<TMP_FontAsset>();
    public List<TMP_FontAsset> JPfontList = new List<TMP_FontAsset>();
    public List<TMP_FontAsset> GfontList = new List<TMP_FontAsset>();

    public List<GameObject> NeedToRefresh = new List<GameObject>(); // 新添加的 GameObject 列表


    [Space2(20)]

    [Tab("Chinese")]
    public TMP_FontAsset CNlightFont;
    public TMP_FontAsset CNregularFont;
    public TMP_FontAsset CNmediumFont;
    public TMP_FontAsset CNsemiboldFont;
    public TMP_FontAsset CNboldFont;
    public TMP_FontAsset CNcustomFont;

    [Tab("Japanese")]
    public TMP_FontAsset JPlightFont;
    public TMP_FontAsset JPregularFont;
    public TMP_FontAsset JPmediumFont;
    public TMP_FontAsset JPsemiboldFont;
    public TMP_FontAsset JPboldFont;
    public TMP_FontAsset JPcustomFont;

    [Tab("Etc.")]
    public TMP_FontAsset GlightFont;
    public TMP_FontAsset GregularFont;
    public TMP_FontAsset GmediumFont;
    public TMP_FontAsset GsemiboldFont;
    public TMP_FontAsset GboldFont;
    public TMP_FontAsset GcustomFont;

    private LocalizationManager localizationManager;
    private UIManagerFontChanger uimanagerfontchanger;
    private HorizontalSelector horizontalSelector;
    private string selectedTitle;

    // 记录 GameObject 初始状态的字典
    private Dictionary<GameObject, bool> initialObjectStates = new Dictionary<GameObject, bool>();

    void Awake()
    {
        CNfontList.AddRange(new TMP_FontAsset[] { CNlightFont, CNregularFont, CNmediumFont, CNsemiboldFont, CNboldFont, CNcustomFont });
        JPfontList.AddRange(new TMP_FontAsset[] { JPlightFont, JPregularFont, JPmediumFont, JPsemiboldFont, JPboldFont, JPcustomFont });
        GfontList.AddRange(new TMP_FontAsset[] { GlightFont, GregularFont, GmediumFont, GsemiboldFont, GboldFont, GcustomFont });

        uimanagerfontchanger = GetComponent<UIManagerFontChanger>();
        localizationManager = GetComponent<LocalizationManager>();
        horizontalSelector = localizationManager.languageSelector;

    }

    private void Start()
    {
        if (horizontalSelector == null)
        {
            Debug.LogError("Horizontal Selector is not assigned!");
            return;
        }

        // 模拟选择项变化并触发逻辑，获取一次 selectedTitle
        OnSelectionChanged(horizontalSelector.index);

        horizontalSelector.onValueChanged.AddListener(OnSelectionChanged);
    }

    void OnSelectionChanged(int newIndex)
    {
        HorizontalSelector.Item selectedItem = horizontalSelector.items[newIndex];
        selectedTitle = selectedItem.itemTitle;

        // 根据选择项的标题选择相应的字体列表
        List<TMP_FontAsset> selectedFontList = GetSelectedFontList(selectedTitle);

        // 检查是否找到了匹配的字体列表
        if (selectedFontList != null)
        {
            // 将字体列表中的值赋给 UIManagerFontChanger
            uimanagerfontchanger.lightFont = selectedFontList[0];
            uimanagerfontchanger.regularFont = selectedFontList[1];
            uimanagerfontchanger.mediumFont = selectedFontList[2];
            uimanagerfontchanger.semiboldFont = selectedFontList[3];
            uimanagerfontchanger.boldFont = selectedFontList[4];
            uimanagerfontchanger.customFont = selectedFontList[5];

            uimanagerfontchanger.ApplyFonts();

            RefreshObjectStates();
        }
        else
        {
            Debug.LogError("No matching font list found for the selected title: " + selectedTitle);
        }

        // 这里可以执行其他操作
    }


    // 刷新 GameObject 状态
    void RefreshObjectStates()
    {
        foreach (GameObject obj in NeedToRefresh)
        {
            // 如果 GameObject 不为空，则刷新其状态
            if (obj != null)
            {
                // 首次刷新前记录初始状态
                if (!initialObjectStates.ContainsKey(obj))
                {
                    initialObjectStates[obj] = obj.activeSelf;
                }

                // 颠倒状态
                obj.SetActive(!initialObjectStates[obj]);
                obj.SetActive(initialObjectStates[obj]);
            }
        }
    }

    // 根据选择项的标题返回相应的字体列表
    List<TMP_FontAsset> GetSelectedFontList(string title)
    {
        switch (title)
        {
            case "简体中文":
                return CNfontList;
            case "日本語":
                return JPfontList;
            default:
                return GfontList;
        }
    }
}
