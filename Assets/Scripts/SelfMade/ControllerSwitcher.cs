using UnityEngine;
using StarterAssets;
using Cinemachine;

public class ControllerSwitcher : MonoBehaviour
{
    public GameObject playerArmature;
    public ThirdPersonController thirdPersonController;
    public FirstPersonController firstPersonController;
    public GameObject playerFollowCamera;
    public GameObject fpsCamera;

    private bool tabPressed = false;

    void Start()
    {
        // 获取引用
        thirdPersonController = playerArmature.GetComponent<ThirdPersonController>();
        firstPersonController = playerArmature.GetComponent<FirstPersonController>();

        // 获取PlayerFollowCamera和FPSCamera的引用
        playerFollowCamera = playerArmature.transform.Find("RINDO/Geometry/PlayerFollowCamera").gameObject;
        fpsCamera = playerArmature.transform.Find("RINDO/Geometry/FPSCamera").gameObject;

        // 初始状态设置为 Third Person 模式
        EnableThirdPersonMode();
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
        if (thirdPersonController != null && firstPersonController != null && playerFollowCamera != null && fpsCamera != null)
        {
            if (thirdPersonController.enabled)
            {
                // 如果 Third Person Controller 启用，切换到 First Person 模式
                EnableFirstPersonMode();
            }
            else
            {
                // 如果 First Person Controller 启用，切换到 Third Person 模式
                EnableThirdPersonMode();
            }
        }
    }

    void EnableThirdPersonMode()
    {
        // 启用 Third Person 模式
        thirdPersonController.enabled = true;
        firstPersonController.enabled = false;

        // 启用 Third Person 摄像机，禁用 First Person 摄像机
        playerFollowCamera.SetActive(true);

        // 禁用 PlayerFollowCamera 下的 CinemachineVirtualCamera 脚本
        SetCinemachineVirtualCameraState(playerFollowCamera, true);
        // 启用 Third Person 摄像机，禁用 First Person 摄像机
        playerFollowCamera.SetActive(true);
        fpsCamera.SetActive(false);
    }

    void EnableFirstPersonMode()
    {
        // 启用 First Person 模式
        thirdPersonController.enabled = false;
        firstPersonController.enabled = true;

        // 启用 First Person 摄像机，禁用 Third Person 摄像机
        //playerFollowCamera.SetActive(false);

        // 启用 PlayerFollowCamera 下的 CinemachineVirtualCamera 脚本
        SetCinemachineVirtualCameraState(playerFollowCamera, false);

        fpsCamera.SetActive(true);
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
