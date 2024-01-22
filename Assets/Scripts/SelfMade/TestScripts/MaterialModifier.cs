using UnityEngine;

public class MaterialModifier : MonoBehaviour
{
    // 在Inspector面板中将要应用的材质球分配给这个变量
    public Material newMaterial;

    void Start()
    {
        // 获取物体上的Renderer组件
        Renderer renderer = GetComponent<Renderer>();

        if (renderer != null && newMaterial != null)
        {
            // 使用新的材质球的实例替换原始材质球
            renderer.material = new Material(newMaterial);
        }
    }
}
