using UnityEngine;
using System.Collections.Generic;
using Michsky.UI.Heat;

public class ButtonSoundActiver : MonoBehaviour
{
    // 将 List<PanelButton> 声明为 public，其他类可以直接访问
    public List<PanelButton> panelButtonList = new List<PanelButton>();

    // 将 List<BoxButtonManager> 声明为 public，其他类可以直接访问
    public List<BoxButtonManager> boxButtonManagerList = new List<BoxButtonManager>();

    // 将 List<ButtonManager> 声明为 public，其他类可以直接访问
    public List<ButtonManager> buttonManagerList = new List<ButtonManager>();

    private HashSet<PanelButton> activatedPanelScripts = new HashSet<PanelButton>();
    private HashSet<BoxButtonManager> activatedBoxScripts = new HashSet<BoxButtonManager>();
    private HashSet<ButtonManager> activatedButtonScripts = new HashSet<ButtonManager>();

    void Start()
    {
        // 获取当前物体及其所有子物体上的所有PanelButton组件，包括未激活的物体
        PanelButton[] panelButtons = GetComponentsInChildren<PanelButton>(true);

        // 将PanelButton数组转换为List
        panelButtonList = new List<PanelButton>(panelButtons);

        // 获取当前物体及其所有子物体上的所有BoxButtonManager组件，包括未激活的物体
        BoxButtonManager[] boxButtonManagers = GetComponentsInChildren<BoxButtonManager>(true);

        // 将BoxButtonManager数组转换为List
        boxButtonManagerList = new List<BoxButtonManager>(boxButtonManagers);

        // 获取当前物体及其所有子物体上的所有ButtonManager组件，包括未激活的物体
        ButtonManager[] buttonManagers = GetComponentsInChildren<ButtonManager>(true);

        // 将ButtonManager数组转换为List
        buttonManagerList = new List<ButtonManager>(buttonManagers);

        // 遍历所有PanelButton组件
        foreach (PanelButton panelButton in panelButtonList)
        {
            // 设置 useSounds 为 true
            panelButton.useSounds = true;

            // 仅将激活状态的脚本添加到已激活脚本的集合中
            if (panelButton.gameObject.activeSelf)
            {
                activatedPanelScripts.Add(panelButton);
            }

            // 这里可以对每个PanelButton进行其他操作
            // 例如输出它们的名称
            Debug.Log("Found PanelButton: " + panelButton.name);
        }

        // 遍历所有BoxButtonManager组件
        foreach (BoxButtonManager boxButtonManager in boxButtonManagerList)
        {
            // 设置 useSounds 为 true
            boxButtonManager.useSounds = true;

            // 仅将激活状态的脚本添加到已激活脚本的集合中
            if (boxButtonManager.gameObject.activeSelf)
            {
                activatedBoxScripts.Add(boxButtonManager);
            }

            // 这里可以对每个BoxButtonManager进行其他操作
            // 例如输出它们的名称
            Debug.Log("Found BoxButtonManager: " + boxButtonManager.name);
        }

        // 遍历所有ButtonManager组件
        foreach (ButtonManager buttonManager in buttonManagerList)
        {
            // 设置 useSounds 为 true
            buttonManager.useSounds = true;

            // 仅将激活状态的脚本添加到已激活脚本的集合中
            if (buttonManager.gameObject.activeSelf)
            {
                activatedButtonScripts.Add(buttonManager);
            }

            // 这里可以对每个ButtonManager进行其他操作
            // 例如输出它们的名称
            Debug.Log("Found ButtonManager: " + buttonManager.name);
        }
    }

    void Update()
    {
        // 检查是否有新的激活状态的PanelButton脚本，如果有，设置 useSounds 为 true
        foreach (PanelButton panelButton in panelButtonList)
        {
            // 如果 panelButton 为 null，说明对象已被销毁，跳过
            if (panelButton == null)
            {
                continue;
            }

            if (panelButton.gameObject.activeSelf && !activatedPanelScripts.Contains(panelButton))
            {
                panelButton.useSounds = true;
                activatedPanelScripts.Add(panelButton);
            }
        }

        // 检查是否有新的激活状态的BoxButtonManager脚本，如果有，设置 useSounds 为 true
        foreach (BoxButtonManager boxButtonManager in boxButtonManagerList)
        {
            // 如果 boxButtonManager 为 null，说明对象已被销毁，跳过
            if (boxButtonManager == null)
            {
                continue;
            }

            if (boxButtonManager.gameObject.activeSelf && !activatedBoxScripts.Contains(boxButtonManager))
            {
                boxButtonManager.useSounds = true;
                activatedBoxScripts.Add(boxButtonManager);
            }
        }

        // 检查是否有新的激活状态的ButtonManager脚本，如果有，设置 useSounds 为 true
        foreach (ButtonManager buttonManager in buttonManagerList)
        {
            // 如果 buttonManager 为 null，说明对象已被销毁，跳过
            if (buttonManager == null)
            {
                continue;
            }

            if (buttonManager.gameObject.activeSelf && !activatedButtonScripts.Contains(buttonManager))
            {
                buttonManager.useSounds = true;
                activatedButtonScripts.Add(buttonManager);
            }
        }

    }
}
