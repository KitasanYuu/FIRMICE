using UnityEngine;
using System.Collections.Generic;
using RenderTools;

public class RendererCollector : MonoBehaviour
{
    public bool TestButton;
    public Transform rootObject; // 起始对象，通过Inspector设置
    private int lastChildCount = -1; // 存储上一次子物体的数量

    public List<Renderer> collectedRenderers = new List<Renderer>(); // 存储收集到的Renderer组件
    public List<MaterialReplacer> collectedReplacers = new List<MaterialReplacer>(); // 存储收集到的MaterialReplacer脚本

    void Start()
    {
        if (rootObject == null)
            rootObject = this.transform;

        CollectComponents(rootObject);
        lastChildCount = GetTotalChildCount(rootObject); // 初始化子物体数量
    }

    void Update()
    {
        RefreshRenderer();
        
    }

    void CollectComponents(Transform root)
    {
        if (root == null) return;

        MaterialReplacer replacer = root.GetComponent<MaterialReplacer>();
        if (replacer != null && replacer.renderer != null)
        {
            collectedRenderers.Add(replacer.renderer);
            collectedReplacers.Add(replacer);
        }

        foreach (Transform child in root)
        {
            CollectComponents(child);
        }
    }

    void RefreshRenderer()
    {
        // 检查子物体数量是否变化
        if (GetTotalChildCount(rootObject) != lastChildCount)
        {
            // 清空列表，重新收集
            collectedRenderers.Clear();
            collectedReplacers.Clear();
            CollectComponents(rootObject);

            // 更新子物体数量记录
            lastChildCount = GetTotalChildCount(rootObject);
        }
    }

    int GetTotalChildCount(Transform root)
    {
        return root.GetComponentsInChildren<Transform>().Length;
    }

    public void SetRendererStatue(bool RenderStatus, AllinOneRenderTool allinone)
    {
        foreach(MaterialReplacer MR in collectedReplacers)
            MR.SetStatus(RenderStatus,allinone);
    }
}
