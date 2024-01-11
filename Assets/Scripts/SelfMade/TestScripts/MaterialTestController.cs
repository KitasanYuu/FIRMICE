using UnityEngine;

public class MaterialTestController : MonoBehaviour
{
    private Renderer renderer;
    private Material material;
    public bool _ENABLEDITHER;
    public float _Dither;


    private void Start()
    {
        // 获取Renderer组件上的第一个材质
        renderer = GetComponent<Renderer>();
        material = renderer.material;
    }

    private void Update()
    {
        if (material != null)
        {
            //Debug.Log("Got");
        }
        else if(material == null)
        {
            Debug.Log("NoMat");
        }
        else
        {
            Debug.Log("material");
        }
        material.SetFloat("_ENABLEDITHER", _ENABLEDITHER ? 1.0f : 0.0f);
        material.SetFloat("_Dither", _Dither);

    }
}
