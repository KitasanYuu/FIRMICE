using UnityEngine;

public class TextureOffsetAnimation : MonoBehaviour
{
    public Vector2 offsetSpeed = new Vector2(0.1f, 0.1f); // X和Y的偏移速度
    private Material materialInstance;
    private Vector2 currentOffset;

    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            // 实例化材质以进行运行时修改
            materialInstance = renderer.material;
            currentOffset = materialInstance.GetTextureOffset("_MainTex");
        }
    }

    void Update()
    {
        if (materialInstance != null)
        {
            // 根据时间和速度更新偏移
            currentOffset.x = Mathf.Repeat(currentOffset.x + offsetSpeed.x * Time.deltaTime, 1);
            currentOffset.y = Mathf.Repeat(currentOffset.y + offsetSpeed.y * Time.deltaTime, 1);

            // 应用新的偏移值
            materialInstance.mainTextureOffset = currentOffset;
        }
    }
}
