using UnityEngine;

public class EmissionColorController : MonoBehaviour
{
    [SerializeField]
    private GameObject[] targetObjects; // 选择的目标 GameObject 数组
    private Material[] materials; // 目标 GameObject 上的材质数组

    public Color colorA = Color.black; // 颜色A
    public Color colorB = Color.white; // 颜色B

    public float lerpColorIntensity = 1.0f; // 颜色插值强度

    [Range(0.0f, 1.0f)] // 使用 Range 特性来定义滑动条范围
    public float TimeScale = 0.5f; // 自定义 t 值，可以在 Inspector 中使用滑动条

    private TargetBotActiveRange targetbotactive;
    private Color currentColor; // 当前颜色
    private float lerpColorStartTime;

    private void Start()
    {
        targetbotactive = GetComponent<TargetBotActiveRange>();

        if (targetObjects != null && targetObjects.Length > 0)
        {
            materials = new Material[targetObjects.Length];

            for (int i = 0; i < targetObjects.Length; i++)
            {
                GameObject targetObject = targetObjects[i];

                if (targetObject != null)
                {
                    // 获取目标 GameObject 上的 Renderer 组件
                    Renderer renderer = targetObject.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        materials[i] = renderer.material;
                    }
                    else
                    {
                        Debug.LogError("No Renderer component found on target GameObject " + targetObject.name);
                        enabled = false; // 禁用脚本
                        return;
                    }
                }
                else
                {
                    Debug.LogError("Target GameObject at index " + i + " is not assigned.");
                    enabled = false; // 禁用脚本
                    return;
                }
            }
        }
        else
        {
            Debug.LogError("No target GameObjects are assigned.");
            enabled = false; // 禁用脚本
            return;
        }

    }

    void Update()
    {
        // 根据 TargetStatus 的值来控制目标颜色
        Color targetColor = targetbotactive != null && targetbotactive.TargetStatus == 1 ? colorB : colorA;
        // 使用 Lerp 插值并设置材质的 Emission 颜色，保持强度不变
        for (int i = 0; i < materials.Length; i++)
        {
            currentColor = Color.Lerp(currentColor, targetColor * lerpColorIntensity, TimeScale);
            materials[i].SetColor("_EmissionColor", currentColor);
            materials[i].SetColor("_Emission2ndColor", currentColor);
        }
    }
}
