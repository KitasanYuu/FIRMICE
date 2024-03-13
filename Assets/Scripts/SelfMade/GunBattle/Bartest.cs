using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public Transform HPAnchor; // NPC的Transform组件
    public Camera mainCamera; // 主相机
    public Canvas canvas; // Canvas，其Render Mode应该设置为Camera，并且指定了UI Camera
    public float baseOffsetY = 2.0f; // 血条在NPC头顶的基础偏移量
    public float z;

    private HPVisionManager HVM;

    [HideInInspector]
    public float ScaleFactor;

    private RectTransform healthBarRectTransform;

    void Start()
    {
        // 获取血条的RectTransform组件
        healthBarRectTransform = GetComponent<RectTransform>();

        HVM = GetComponentInParent<HPVisionManager>();
        if (HVM != null)
        {
            HVM.RegisterHealthBar(this);
        }
    }

    void LateUpdate()
    {
        // 计算摄像机到NPC的距离
        float distance = Vector3.Distance(mainCamera.transform.position, HPAnchor.position);

        // 根据距离调整血条的缩放
        float scaleFactor = Mathf.Clamp(1 / (distance * 0.1f), 0, 1.2f);
        ScaleFactor = scaleFactor;
        healthBarRectTransform.localScale = new Vector3(scaleFactor, scaleFactor, 1);

        // 将NPC的世界坐标转换为屏幕坐标
        Vector3 screenPos = mainCamera.WorldToScreenPoint(HPAnchor.position + new Vector3(0, baseOffsetY, 0));
        z= screenPos.z;
        // 检查NPC是否位于摄像机后面
        if (screenPos.z < 0)
        {
            // 如果在摄像机后面，则隐藏血条
            healthBarRectTransform.gameObject.SetActive(false);
        }
        else
        {
            // 如果在摄像机前面，则确保血条是可见的
            if (!healthBarRectTransform.gameObject.activeSelf)
            {
                healthBarRectTransform.gameObject.SetActive(true);
            }

            // 将屏幕坐标转换为Canvas坐标系下的位置
            Vector2 canvasPos;
            bool isConverted = RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.GetComponent<RectTransform>(), screenPos, canvas.worldCamera, out canvasPos);
            if (isConverted)
            {
                // 更新血条的位置
                healthBarRectTransform.localPosition = canvasPos;
            }
        }
    }

    void OnDestroy()
    {
        // 在销毁时从控制器中注销
        if (HVM != null)
        {
            HVM.UnregisterHealthBar(this);
        }
    }

    public void SetParameter(Transform Anchor,Camera MainCamera,Canvas scanvas)
    {
        HPAnchor = Anchor;
        mainCamera = MainCamera;
        canvas = scanvas;   
    }

    public void SetVisibility(bool isVisible)
    {
        gameObject.SetActive(isVisible);
    }
}
