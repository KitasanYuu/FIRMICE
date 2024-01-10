using UnityEngine;

public class MeshRendererController : MonoBehaviour
{
    // 控制MeshRenderer和SkinnedMeshRenderer的开关状态的布尔变量
    public bool renderersEnabled = true;

    void Start()
    {
        // 在Start方法中初始化MeshRenderer和SkinnedMeshRenderer的状态
        SetRenderersEnabled(renderersEnabled);
    }

    void Update()
    {
        // 在Update函数中检测布尔变量的更改并应用
        if (renderersEnabled != AreRenderersEnabled())
        {
            SetRenderersEnabled(renderersEnabled);
        }
    }

    // 检查MeshRenderer和SkinnedMeshRenderer的开关状态
    bool AreRenderersEnabled()
    {
        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
        SkinnedMeshRenderer[] skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            if (!meshRenderer.enabled)
            {
                return false;
            }
        }

        foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
        {
            if (!skinnedMeshRenderer.enabled)
            {
                return false;
            }
        }

        return true;
    }

    // 设置MeshRenderer和SkinnedMeshRenderer的开关状态
    void SetRenderersEnabled(bool enabled)
    {
        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
        SkinnedMeshRenderer[] skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            meshRenderer.enabled = enabled;
        }

        foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
        {
            skinnedMeshRenderer.enabled = enabled;
        }
    }
}
