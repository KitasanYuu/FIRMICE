using UnityEngine;

public class EmissionColorController : MonoBehaviour
{
    private Material material; // ������Ĳ���
    public Color colorA = Color.black; // ��ɫA
    public Color colorB = Color.white; // ��ɫB
    public float duration = 2.0f; // �仯����ʱ��
    public float lerpcolor;

    private float startTime;

    void Start()
    {
        // ��ȡ�����ϵĲ���
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            material = renderer.material;
        }
        else
        {
            Debug.LogError("No Renderer component found on this GameObject.");
            enabled = false; // ���ýű�
            return;
        }

        startTime = Time.time;
    }

    void Update()
    {
        float t = Mathf.PingPong(Time.time - startTime, duration) / duration;

        // ��ȡ��ǰǿ��ֵ
        float currentIntensity = material.GetColor("_EmissionColor").maxColorComponent;

        // �ֱ���ARGBͨ��
        float lerpedR = Mathf.Lerp(colorA.r, colorB.r, t);
        float lerpedG = Mathf.Lerp(colorA.g, colorB.g, t);
        float lerpedB = Mathf.Lerp(colorA.b, colorB.b, t);

        // �����µ���ɫ
        Color lerpedColor = new Color(lerpedR, lerpedG, lerpedB);

        // ���µ���ɫ�뵱ǰǿ��ֵ�ϲ�
        lerpedColor *= lerpcolor;
        //Debug.Log(lerpedColor);
        // ���ò��ʵ�Emission��ɫ
        material.SetColor("_EmissionColor", lerpedColor);
    }
}
