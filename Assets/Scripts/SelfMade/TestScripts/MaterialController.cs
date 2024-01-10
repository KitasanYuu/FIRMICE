using UnityEngine;

public class MaterialControllerUI : MonoBehaviour
{
    public AllinOne allinone;

    public bool enableDither = false;
    [Range(0.0f, 1.0f)]
    public float ditherValue = 0.0f;
    public bool disableRenderers = false;

    void Update()
    {
        // 将外部控制的值传递给MaterialController脚本
        if (allinone != null)
        {
            allinone._ENABLEDITHER = enableDither;
            allinone._Dither = ditherValue;
            allinone.disableAllRenderers = disableRenderers;

            // 调用MaterialController的方法以应用更改
            allinone.UpdateMaterialProperties();
            allinone.SetRenderersEnabled(!disableRenderers);
        }
    }
}
