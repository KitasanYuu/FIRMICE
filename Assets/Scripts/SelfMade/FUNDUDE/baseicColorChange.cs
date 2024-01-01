using UnityEngine;

public class EmissionColorController : MonoBehaviour
{
    private Material material; // 引用你的材质
    public Color colorA = Color.black; // 颜色A
    public Color colorB = Color.white; // 颜色B
    public float duration = 2.0f; // 变化持续时间
    public float lerpcolor;

    private float startTime;

    void Start()
    {
        // 获取物体上的材质
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            material = renderer.material;
        }
        else
        {
            Debug.LogError("No Renderer component found on this GameObject.");
            enabled = false; // 禁用脚本
            return;
        }

        startTime = Time.time;
    }

    void Update()
    {
        float t = Mathf.PingPong(Time.time - startTime, duration) / duration;

        // 获取当前强度值
        float currentIntensity = material.GetColor("_EmissionColor").maxColorComponent;

        // 分别处理ARGB通道
        float lerpedR = Mathf.Lerp(colorA.r, colorB.r, t);
        float lerpedG = Mathf.Lerp(colorA.g, colorB.g, t);
        float lerpedB = Mathf.Lerp(colorA.b, colorB.b, t);

        // 创建新的颜色
        Color lerpedColor = new Color(lerpedR, lerpedG, lerpedB);

        // 将新的颜色与当前强度值合并
        lerpedColor *= lerpcolor;
        //Debug.Log(lerpedColor);
        // 设置材质的Emission颜色
        material.SetColor("_EmissionColor", lerpedColor);
    }
}
