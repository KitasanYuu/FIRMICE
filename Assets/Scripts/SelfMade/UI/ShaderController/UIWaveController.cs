using CustomInspector;
using UnityEngine;

[ExecuteInEditMode]
public class ShaderDataController : MonoBehaviour
{
    [SerializeField,ReadOnly]private Material material; // 分配你想要控制的材质
    [SerializeField,ReadOnly]private RectTransform rectTransform;
    public Color color;
    public Color ToColor;
    [Range(0, 1)] public float OriginColorPassValue;
    [Range(0,1)] public float ControlParameter;
    [Range(45, 135)] public float Angle = 45;
    [Range(0,20)]public float ShapeKey =1;
    [Range(1, 5)] public float Tilling =1;
    [Range(0, 1)] public float WaveSpacing;

    void Update()
    {
        if (material != null)
        {
            if(rectTransform != null)
            {
                float aspectRatio = rectTransform.rect.height / rectTransform.rect.width;
                // 设置着色器中的宽高比变量
                material.SetFloat("_CanvasAspectRatio", aspectRatio);
            }
            else
            {
                rectTransform = GetComponent<RectTransform>();
            }

            // 示例：动态修改颜色和控制参数
            material.SetColor("_Color", CraftColor(color, OriginColorPassValue)); // 设置为红色
            material.SetColor("_TargetColor",ToColor); // 设置为蓝色
            material.SetFloat("_ControlParameter",ControlParameter);
            material.SetFloat("_Angle", Angle);
            material.SetFloat("_ShapeKey", ShapeKey);
            material.SetFloat("_Tilling", Tilling);
            material.SetFloat("_WaveSpacing", WaveSpacing);
            // 添加更多属性的控制，如需要
        }
        else
        {
            material = GetComponent<Material>();
        }
    }

    private Color CraftColor(Color color,float Passvalue)
    {
        float a = color.a;
        float r = color.r;
        float g = color.g;
        float b = color.b;

        Color newcolor = new Color(r,g,b, Passvalue);

        return newcolor;
    }
}
