using UnityEngine;
using System.Collections.Generic;

public class MaterialIterator : MonoBehaviour
{
    // 声明一个Dictionary来存储材质和它们的计数
    private Dictionary<Material, int> materialCount = new Dictionary<Material, int>();

    // 声明一个List来存储不重复的材质
    private List<Material> uniqueMaterials = new List<Material>();

    // 公开两个变量，用于修改材质中的属性
    public bool _ENABLEDITHER = false;
    public float _Dither = 0.0f;

    void Start()
    {
        UpdateMaterials();
    }

    // 外部调用的方法，用于更新材质属性
    public void UpdateMaterialProperties(bool enableDither, float ditherValue)
    {
        _ENABLEDITHER = enableDither;
        _Dither = ditherValue;

        UpdateMaterials();
    }

    // 更新材质属性
    void UpdateMaterials()
    {
        // 获取当前物体上的所有Renderer组件
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        // 遍历每个Renderer组件
        foreach (Renderer renderer in renderers)
        {
            // 获取当前Renderer上的所有材质
            Material[] materials = renderer.materials;

            // 遍历每个材质
            foreach (Material material in materials)
            {
                // 检查材质是否已经存在于Dictionary中
                if (materialCount.ContainsKey(material))
                {
                    // 如果存在，增加计数
                    materialCount[material]++;
                }
                else
                {
                    // 如果不存在，添加到Dictionary中
                    materialCount.Add(material, 1);

                    // 同时将不重复的材质添加到uniqueMaterials列表中
                    uniqueMaterials.Add(material);
                }

                // 设置材质中的属性，仅当材质包含这两个属性时
                if (material.HasProperty("_ENABLEDITHER"))
                {
                    material.SetFloat("_ENABLEDITHER", _ENABLEDITHER ? 1.0f : 0.0f);
                }

                if (material.HasProperty("_Dither"))
                {
                    material.SetFloat("_Dither", _Dither);
                }
            }
        }

        // 遍历uniqueMaterials以获取所有不重复的材质
        foreach (Material uniqueMaterial in uniqueMaterials)
        {
            Debug.Log("不重复的材质：" + uniqueMaterial.name);
        }
    }
}
