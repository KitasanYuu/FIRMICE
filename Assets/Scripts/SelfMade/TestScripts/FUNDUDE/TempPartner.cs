using UnityEngine;

public class TextureOffsetAnimation : MonoBehaviour
{
    public Vector2 offsetSpeed = new Vector2(0.1f, 0.1f); // X��Y��ƫ���ٶ�
    private Material materialInstance;
    private Vector2 currentOffset;

    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            // ʵ���������Խ�������ʱ�޸�
            materialInstance = renderer.material;
            currentOffset = materialInstance.GetTextureOffset("_MainTex");
        }
    }

    void Update()
    {
        if (materialInstance != null)
        {
            // ����ʱ����ٶȸ���ƫ��
            currentOffset.x = Mathf.Repeat(currentOffset.x + offsetSpeed.x * Time.deltaTime, 1);
            currentOffset.y = Mathf.Repeat(currentOffset.y + offsetSpeed.y * Time.deltaTime, 1);

            // Ӧ���µ�ƫ��ֵ
            materialInstance.mainTextureOffset = currentOffset;
        }
    }
}
