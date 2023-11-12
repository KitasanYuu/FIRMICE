using UnityEngine;
using StarterAssets;

public class ObjectSwitcher : MonoBehaviour
{
    public GameObject fpsCamera;
    public GameObject object1;
    public GameObject object2;
    public GameObject object3;
    public GameObject object4;

    void Start()
    {
        if (fpsCamera == null)
        {
            Debug.LogError("FPS Camera not assigned to ObjectSwitcher script.");
            return;
        }

        // 初始状态设置为 Third Person 模式
        EnableObjectsBasedOnCameraState();
    }

    void Update()
    {
        // 在Update中检测FPSCamera的启用和禁用
        if (fpsCamera.activeSelf)
        {
            // FPSCamera 启用，禁用指定的四个对象
            SetObjectsActive(false);
        }
        else
        {
            // FPSCamera 禁用，启用指定的四个对象
            SetObjectsActive(true);
        }
    }

    void SetObjectsActive(bool active)
    {
        if (object1 != null)
            object1.SetActive(active);

        if (object2 != null)
            object2.SetActive(active);

        if (object3 != null)
            object3.SetActive(active);

        if (object4 != null)
            object4.SetActive(active);
    }

    void EnableObjectsBasedOnCameraState()
    {
        bool activeState = !fpsCamera.activeSelf;
        SetObjectsActive(activeState);
    }
}
