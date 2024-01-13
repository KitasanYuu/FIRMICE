using UnityEngine;
using System.Collections.Generic;
using Michsky.UI.Heat;

public class PanelButtonController : MonoBehaviour
{
    // 将 List<PanelButton> 声明为 public，其他类可以直接访问
    public List<PanelButton> panelButtonList = new List<PanelButton>();

    private HashSet<PanelButton> activatedScripts = new HashSet<PanelButton>();

    void Start()
    {
        // 获取当前物体及其所有子物体上的所有PanelButton组件，包括未激活的物体
        PanelButton[] panelButtons = GetComponentsInChildren<PanelButton>(true);

        // 将PanelButton数组转换为List
        panelButtonList = new List<PanelButton>(panelButtons);

        // 遍历所有PanelButton组件
        foreach (PanelButton panelButton in panelButtonList)
        {
            // 设置 useSounds 为 true，仅对激活状态的脚本执行
            if (panelButton.gameObject.activeSelf)
            {
                panelButton.useSounds = true;
                activatedScripts.Add(panelButton);
            }

            // 这里可以对每个PanelButton进行其他操作
            // 例如输出它们的名称
            Debug.Log("Found PanelButton: " + panelButton.name);
        }
    }

    void Update()
    {
        // 检查是否有新的激活状态的脚本，如果有，设置 useSounds 为 true
        foreach (PanelButton panelButton in panelButtonList)
        {
            if (panelButton.gameObject.activeSelf && !activatedScripts.Contains(panelButton))
            {
                panelButton.useSounds = true;
                activatedScripts.Add(panelButton);
            }
        }
    }
}
