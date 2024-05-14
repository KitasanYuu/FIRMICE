using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelSwitchElf : MonoBehaviour
{
    public InventoryPanelManager IPM;
    public CanvasGroup _currentPanel;
    public CanvasGroup _targetPanel;

    public bool _switch;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //RefreshCanvasGroup();

        if (_switch)
        {
            IPM.OnPanelChanged(_currentPanel, _targetPanel);
            _switch = false;
            CanvasGroup cg = _currentPanel;
            _currentPanel = _targetPanel;
            _targetPanel = cg;
        }
    }

    private void RefreshCanvasGroup()
    {
        // 获取当前场景中的所有CanvasGroup组件
        CanvasGroup[] canvasGroups = IPM?.gameObject.GetComponentsInChildren<CanvasGroup>();

        // 遍历所有CanvasGroup组件
        foreach (CanvasGroup canvasGroup in canvasGroups)
        {
            // 检查CanvasGroup是否处于激活状态
            if (canvasGroup.gameObject.activeInHierarchy)
            {
                _currentPanel = canvasGroup;
                //Debug.Log("Active CanvasGroup: " + canvasGroup.gameObject.name);
            }
        }
    }
}
