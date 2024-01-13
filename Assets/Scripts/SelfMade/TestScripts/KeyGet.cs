using UnityEngine;

namespace Michsky.UI.Heat
{
    public class YourScript : MonoBehaviour
    {
        public HorizontalSelector horizontalSelector;

        void Start()
        {
            // 检查是否分配了水平选择器
            if (horizontalSelector == null)
            {
                Debug.LogError("Horizontal Selector is not assigned!");
                return;
            }

            // 注册事件监听器，当选择项更改时调用 OnSelectionChanged 方法
            horizontalSelector.onValueChanged.AddListener(OnSelectionChanged);
        }

        // 当选择项更改时调用的方法
        void OnSelectionChanged(int newIndex)
        {
            // 获取当前选择项的信息
            HorizontalSelector.Item selectedItem = horizontalSelector.items[newIndex];

            // 打印选择项的信息
            Debug.Log("Selected Item Title: " + selectedItem.itemTitle);
            Debug.Log("Selected Item Localization Key: " + selectedItem.localizationKey);
            // 可以根据需要访问其他项的信息，例如图标等
        }
    }
}
