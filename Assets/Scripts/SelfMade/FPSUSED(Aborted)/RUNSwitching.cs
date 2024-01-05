using UnityEngine;

public class FOVController : MonoBehaviour
{
    [SerializeField]
    private Camera targetCamera; // 在Inspector中将目标相机拖拽到这里

    [SerializeField]
    private float normalFOV = 60f; // 默认的FOV
    [SerializeField]
    private float zoomedFOV = 30f; // 按住Shift时的FOV

    [SerializeField]
    private float rotationSpeed = 2f; // 旋转速度
    [SerializeField]
    private float fovLerpSpeed = 5f; // 插值速度

    void Update()
    {
        // 检测是否按住 Shift 键
        bool isShiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        // 如果按住 Shift 并同时按下 WASD 键
        if (isShiftPressed && (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)))
        {
            // 更改相机的 FOV
            ChangeFOV(zoomedFOV);
        }
        else
        {
            // 恢复默认 FOV
            ChangeFOV(normalFOV);
        }

        // 通过鼠标旋转相机
        RotateCamera();
    }

    void ChangeFOV(float targetFOV)
    {
        if (targetCamera != null)
        {
            // 使用插值逐渐改变 FOV
            float currentFOV = targetCamera.fieldOfView;
            float newFOV = Mathf.Lerp(currentFOV, targetFOV, Time.deltaTime * fovLerpSpeed);
            targetCamera.fieldOfView = newFOV;
        }
    }

    void RotateCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        // 在这里实现根据鼠标旋转相机的逻辑
        // 你可以使用上面的相机旋转逻辑或者其他你想要的逻辑
    }
}
