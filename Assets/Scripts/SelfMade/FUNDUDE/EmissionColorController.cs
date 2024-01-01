using UnityEngine;

public class EmissionColorController : MonoBehaviour
{
    [SerializeField]
    private GameObject[] targetObjects; // ѡ���Ŀ�� GameObject ����
    private Material[] materials; // Ŀ�� GameObject �ϵĲ�������

    public Color colorA = Color.black; // ��ɫA
    public Color colorB = Color.white; // ��ɫB

    public float lerpColorIntensity = 1.0f; // ��ɫ��ֵǿ��

    [Range(0.0f, 1.0f)] // ʹ�� Range ���������廬������Χ
    public float TimeScale = 0.5f; // �Զ��� t ֵ�������� Inspector ��ʹ�û�����

    private TargetBotActiveRange targetbotactive;
    private Color currentColor; // ��ǰ��ɫ
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
                    // ��ȡĿ�� GameObject �ϵ� Renderer ���
                    Renderer renderer = targetObject.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        materials[i] = renderer.material;
                    }
                    else
                    {
                        Debug.LogError("No Renderer component found on target GameObject " + targetObject.name);
                        enabled = false; // ���ýű�
                        return;
                    }
                }
                else
                {
                    Debug.LogError("Target GameObject at index " + i + " is not assigned.");
                    enabled = false; // ���ýű�
                    return;
                }
            }
        }
        else
        {
            Debug.LogError("No target GameObjects are assigned.");
            enabled = false; // ���ýű�
            return;
        }

    }

    void Update()
    {
        // ���� TargetStatus ��ֵ������Ŀ����ɫ
        Color targetColor = targetbotactive != null && targetbotactive.TargetStatus == 1 ? colorB : colorA;
        // ʹ�� Lerp ��ֵ�����ò��ʵ� Emission ��ɫ������ǿ�Ȳ���
        for (int i = 0; i < materials.Length; i++)
        {
            currentColor = Color.Lerp(currentColor, targetColor * lerpColorIntensity, TimeScale);
            materials[i].SetColor("_EmissionColor", currentColor);
            materials[i].SetColor("_Emission2ndColor", currentColor);
        }
    }
}
