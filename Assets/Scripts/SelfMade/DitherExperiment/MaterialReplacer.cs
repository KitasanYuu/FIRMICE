using UnityEngine;
using CustomInspector;
using RenderTools;

public class MaterialReplacer : MonoBehaviour
{
    [ReadOnly] public new Renderer renderer;

    public bool TestButton;
    [ReadOnly,SerializeField] public bool Status;
    private bool perviousStatus;
    [ReadOnly] public Material[] OriginMaterials; // 通过Inspector设置新材质数组
    public Material[] TargetMaterials; // 通过Inspector设置新材质数组

    void Start()
    {
        ComponentInit();
        RenderMaterialInit();
    }

    private void Update()
    {
        //SetStatus(TestButton);
        //Debug.Log(perviousStatus);
    }

    public void SetStatus(bool newStatus,AllinOneRenderTool allinone)
    {
        Status = newStatus;

        //Debug.LogWarning(Status);

        if (newStatus != perviousStatus)
        {
            perviousStatus = newStatus;

            Material[] targetmaterial = newStatus ? TargetMaterials : OriginMaterials;

            if (renderer != null)
            {
                // 确保新材质数组的长度与原始材质数组匹配
                if (targetmaterial.Length == renderer.sharedMaterials.Length)
                {
                    // 直接将渲染器的sharedMaterials属性设置为新的材质数组
                    renderer.sharedMaterials = targetmaterial;
                }
                else
                {
                    Debug.LogError("The number of new materials does not match the number of original materials.");
                }
            }

            allinone.Refresh(true);
        }
    }

    private void ComponentInit()
    {
        renderer = GetComponent<Renderer>();
    }

    private void RenderMaterialInit()
    {
        if (renderer != null)
        {
            OriginMaterials = renderer.sharedMaterials;
        }
    }
}
