using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HoverTab
{
    public static void BarPositionAdjust(Canvas canvas, Camera mainCamera, Transform floatAnchor, RectTransform targetTransfrom, Transform PlayerAncho = null, float baseOffsetY = 0, float smoothSpeed = 50f)
    {
        // 计算摄像机到NPC的距离
        float distance = Vector3.Distance(mainCamera.transform.position, floatAnchor.position);

        // 根据距离调整血条的缩放
        float scaleFactor = Mathf.Clamp(1 / (distance * 0.1f), 0, 0.75f);
        targetTransfrom.localScale = new Vector3(scaleFactor, scaleFactor, 1);

        Vector3 screenPos = Vector3.zero;
        // 将NPC的世界坐标转换为屏幕坐标
        if (PlayerAncho != null)
        {
            screenPos = mainCamera.WorldToScreenPoint(PlayerAncho.position + new Vector3(0, baseOffsetY, 0));
        }
        else
        {
            screenPos = mainCamera.WorldToScreenPoint(floatAnchor.position + new Vector3(0, baseOffsetY, 0));
        }

        // 将屏幕坐标转换为Canvas坐标系下的位置
        Vector2 canvasPos;
        bool isConverted = RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.GetComponent<RectTransform>(), screenPos, canvas.worldCamera, out canvasPos);
        if (isConverted)
        {
            // 更新血条的位置，使用平滑过渡
            Vector3 targetPosition = canvasPos;
            targetTransfrom.localPosition = Vector3.Lerp(targetTransfrom.localPosition, targetPosition, smoothSpeed * Time.deltaTime);
        }
    }
}
