using UnityEngine;
using StarterAssets;
using Cinemachine;

public class ControllerSwitcher : MonoBehaviour
{
    public GameObject playerArmature;
    public AvatarController avatarController;
    public GameObject playerFollowCamera;
    public GameObject fpsCamera;

    private bool tabPressed;

    void Start()
    {

    }

    void Update()
    {
        // 在Update中检测Tab键按下和释放
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            tabPressed = true;
        }

        if (Input.GetKeyUp(KeyCode.Tab))
        {
            tabPressed = false;
            SwitchController();
        }
    }


    void SwitchController()
    {
        // 获取引用
        avatarController = playerArmature.GetComponent<AvatarController>();


        if (avatarController != null)
        {
            // 切换 IsTPS 属性值
            avatarController.IsTPS = !avatarController.IsTPS;

            // 根据 IsTPS 的状态来启用或禁用 FPSCamera
            fpsCamera.SetActive(!avatarController.IsTPS);
        }
    }

    void SetCinemachineVirtualCameraState(GameObject cameraObject, bool state)
    {
        // 启用或禁用子对象下的 CinemachineVirtualCamera 组件
        CinemachineVirtualCamera virtualCamera = cameraObject.GetComponentInChildren<CinemachineVirtualCamera>();
        if (virtualCamera != null)
        {
            virtualCamera.enabled = state;
        }
    }
}
