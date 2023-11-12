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
        // ��ȡ����
        thirdPersonController = playerArmature.GetComponent<ThirdPersonController>();
        firstPersonController = playerArmature.GetComponent<FirstPersonController>();

        // ��ȡPlayerFollowCamera��FPSCamera������
        playerFollowCamera = playerArmature.transform.Find("RINDO/Geometry/PlayerFollowCamera").gameObject;
        fpsCamera = playerArmature.transform.Find("RINDO/Geometry/FPSCamera").gameObject;

        // ��ʼ״̬����Ϊ Third Person ģʽ
        EnableThirdPersonMode();
    }

    void Update()
    {
        // ��Update�м��Tab�����º��ͷ�
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
                // ��� Third Person Controller ���ã��л��� First Person ģʽ
                EnableFirstPersonMode();
            }
            else
            {
                // ��� First Person Controller ���ã��л��� Third Person ģʽ
                EnableThirdPersonMode();
            }
        }
    }

    void EnableThirdPersonMode()
    {
        // ���� Third Person ģʽ
        thirdPersonController.enabled = true;
        firstPersonController.enabled = false;

        // ���� Third Person ����������� First Person �����
        playerFollowCamera.SetActive(true);

        // ���� PlayerFollowCamera �µ� CinemachineVirtualCamera �ű�
        SetCinemachineVirtualCameraState(playerFollowCamera, true);
        // ���� Third Person ����������� First Person �����
        playerFollowCamera.SetActive(true);
        fpsCamera.SetActive(false);
    }

    void EnableFirstPersonMode()
    {
        // ���� First Person ģʽ
        thirdPersonController.enabled = false;
        firstPersonController.enabled = true;

        // ���� First Person ����������� Third Person �����
        //playerFollowCamera.SetActive(false);

        // ���� PlayerFollowCamera �µ� CinemachineVirtualCamera �ű�
        SetCinemachineVirtualCameraState(playerFollowCamera, false);

        fpsCamera.SetActive(true);
    }

    void SetCinemachineVirtualCameraState(GameObject cameraObject, bool state)
    {
        // ���û�����Ӷ����µ� CinemachineVirtualCamera ���
        CinemachineVirtualCamera virtualCamera = cameraObject.GetComponentInChildren<CinemachineVirtualCamera>();
        if (virtualCamera != null)
        {
            virtualCamera.enabled = state;
        }
    }
}
