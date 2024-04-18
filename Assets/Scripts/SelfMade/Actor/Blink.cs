using System.Collections;
using UnityEngine;

public class BlinkController : MonoBehaviour
{
    public SkinnedMeshRenderer targetRenderer;
    public string blinkShapeName = "eye_close"; // 假设0是眨眼的形态键索引
    public float blinkDuration = 0.1f; // 眨眼持续时间
    private float nextBlinkTime = 0f;
    private float blinkTimer = 0f;
    private bool isBlinking = false;

    private void Awake()
    {
        ComponentInit();
    }

    void Start()
    {
        ComponentInit();
        ScheduleNextBlink();
    }

    void Update()
    {
        if (Time.time >= nextBlinkTime && !isBlinking)
        {
            StartCoroutine(Blink());
            ScheduleNextBlink();
        }
    }

    void ScheduleNextBlink()
    {
        nextBlinkTime = Time.time + Random.Range(3f, 10f); // 随机3到10秒眨眼一次
    }

    IEnumerator Blink()
    {
        isBlinking = true;
        int blinkIndex = targetRenderer.sharedMesh.GetBlendShapeIndex(blinkShapeName);

        if (blinkIndex < 0)
        {
            Debug.LogError("Blink shape not found: " + blinkShapeName);
            isBlinking = false;
            yield break;
        }

        // 眨眼动作，可以考虑把这部分代码封装成一个函数以避免重复
        for (int i = 0; i < 2; i++) // 最多执行两次眨眼动作
        {
            // 执行一次完整的眨眼动作
            yield return StartCoroutine(PerformBlink(blinkIndex, blinkDuration));

            // 随机决定是否进行连续眨眼
            if (Random.Range(0f, 1f) > 0.9f) // 假设有10%的概率连续眨眼
            {
                // 短暂等待再次眨眼
                yield return new WaitForSeconds(0.1f);
            }
            else
            {
                // 如果不进行连续眨眼，则跳出循环
                break;
            }
        }

        isBlinking = false;
    }

    IEnumerator PerformBlink(int blinkIndex, float duration)
    {
        // 确保blinkDuration表示整个闭眼和睁眼的总持续时间
        float halfDuration = duration / 2f;

        // 闭眼动作
        float timer = 0f;
        while (timer <= halfDuration)
        {
            float weight = Mathf.Lerp(0f, 100f, timer / halfDuration);
            targetRenderer.SetBlendShapeWeight(blinkIndex, weight);
            timer += Time.deltaTime;
            yield return null;
        }

        // 确保闭眼达到完全闭合状态
        targetRenderer.SetBlendShapeWeight(blinkIndex, 100f);

        // 睁眼动作
        timer = 0f;
        while (timer <= halfDuration)
        {
            float weight = Mathf.Lerp(100f, 0f, timer / halfDuration);
            targetRenderer.SetBlendShapeWeight(blinkIndex, weight);
            timer += Time.deltaTime;
            yield return null;
        }

        // 确保眼睛完全睁开
        targetRenderer.SetBlendShapeWeight(blinkIndex, 0f);
    }



    private void ComponentInit()
    {
        targetRenderer = GetComponent<SkinnedMeshRenderer>();
    }
}
