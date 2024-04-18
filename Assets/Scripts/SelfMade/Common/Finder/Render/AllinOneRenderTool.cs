using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace RenderTools
{
    public class AllinOneRenderTool : MonoBehaviour
    {
        // 存储所有Render组件的List
        public List<Renderer> renderersList = new List<Renderer>();

        // 存储所有不重复Material的List
        public List<Material> uniqueMaterialsList = new List<Material>();

        // 控制是否关闭所有Render组件的布尔值
        public bool disableAllRenderers = false;

        // 控制_ENABLEDITHER的布尔值
        public bool _ENABLEDITHER = false;

        [HideInInspector] public bool isRefreshing;

        // 控制_Dither的范围
        [Range(0.0f, 1.0f)]
        public float _Dither = 0.0f;

        // 公开的GameObject，用于指定要遍历的对象及其直接子对象
        public GameObject targetGameObject;

        void Start()
        {
            RendererInit();
        }

        public void Refresh(bool isrefresh)
        {
            if (isrefresh)
            {
                isRefreshing = true;
                renderersList.Clear();
                uniqueMaterialsList.Clear();

                RendererInit();
            }
        }

        private void RendererInit()
        {
            isRefreshing = true;
            // 获取当前物体及其子物体上的所有Renderer组件
            Renderer[] allRenderers = GetComponentsInChildren<Renderer>(true);

            if (targetGameObject == null)
            {
                // 如果未指定目标GameObject，则遍历全部Renderer
                renderersList.AddRange(allRenderers);
            }
            else
            {
                // 获取指定的GameObject及其直接子对象上的所有Renderer组件
                Renderer[] targetRenderers = targetGameObject.GetComponentsInChildren<Renderer>(true);

                // 将所有Renderer添加到renderersList中，但不包括指定的GameObject及其子对象的Renderer
                renderersList.AddRange(allRenderers.Except(targetRenderers));
            }

            // 遍历所有Renderer
            foreach (Renderer renderer in allRenderers)
            {
                // 检查是否为指定的GameObject及其子对象的Renderer
                if (targetGameObject != null && targetGameObject.GetComponentsInChildren<Renderer>(true).Contains(renderer))
                {
                    // 如果是，跳过处理
                    continue;
                }

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

            //如果disableAllRenderers为true，则关闭所有Render组件
            if (disableAllRenderers)
            {
                SetRenderersEnabled(false);
            }

            //更新所有材质中的_ENABLEDITHER和_Dither属性
            UpdateMaterialProperties();

            isRefreshing = false;
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
}
