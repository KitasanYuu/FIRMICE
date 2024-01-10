using UnityEngine;
using System.Collections.Generic;

public class AllinOne : MonoBehaviour
{
    // 存储所有Render组件的List
    public List<Renderer> renderersList = new List<Renderer>();

    // 存储所有不重复Material的List
    public List<Material> uniqueMaterialsList = new List<Material>();

    // 控制是否关闭所有Render组件的布尔值
    public bool disableAllRenderers = false;

    // 控制_ENABLEDITHER的布尔值
    public bool _ENABLEDITHER = false;

    // 控制_Dither的范围
    [Range(0.0f, 1.0f)]
    public float _Dither = 0.0f;

    void Start()
    {
        // 获取当前物体及其子物体上的所有Renderer组件
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        // 将所有Renderer添加到renderersList中
        renderersList.AddRange(renderers);

        // 遍历所有Renderer
        foreach (Renderer renderer in renderers)
        {
            // 获取Renderer使用的材质（通常是一个数组）
            Material[] materials = renderer.materials;

            // 遍历Renderer的材质
            foreach (Material material in materials)
            {
                // 检查材质是否已经存在于uniqueMaterialsList中
                if (!uniqueMaterialsList.Contains(material))
                {
                    // 如果不存在，将材质添加到uniqueMaterialsList中
                    uniqueMaterialsList.Add(material);
                }
            }
        }

        // 如果disableAllRenderers为true，则关闭所有Render组件
        //if (disableAllRenderers)
        //{
        //    SetRenderersEnabled(false);
        //}

        // 更新所有材质中的_ENABLEDITHER和_Dither属性
        //UpdateMaterialProperties();
    }

    // 控制所有Render组件的开关状态
    public void SetRenderersEnabled(bool enabled)
    {
        foreach (Renderer renderer in renderersList)
        {
            renderer.enabled = enabled;
        }
    }

    // 更新所有材质中的_ENABLEDITHER和_Dither属性
    public void UpdateMaterialProperties()
    {
        foreach (Material material in uniqueMaterialsList)
        {
            // 检查材质是否包含_ENABLEDITHER属性
            if (material.HasProperty("_ENABLEDITHER"))
            {
                // 使用material.SetInt来设置_ENABLEDITHER属性
                material.SetInt("_ENABLEDITHER", _ENABLEDITHER ? 1 : 0);
            }

            // 检查材质是否包含_Dither属性
            if (material.HasProperty("_Dither"))
            {
                material.SetFloat("_Dither", _Dither);
            }
        }
    }
}
