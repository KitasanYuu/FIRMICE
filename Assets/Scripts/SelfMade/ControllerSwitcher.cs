using UnityEngine;
using StarterAssets;
using Cinemachine;

public class ControllerSwitcher : MonoBehaviour
{
    public GameObject playerArmature;
    public AvatarController avatarController;
    public GameObject playerFollowCamera;
    public GameObject fpsCamera;

    private bool tabPressed = false;

    void Start()
    {

        // ��ȡPlayerFollowCamera��FPSCamera������
        //playerFollowCamera = playerArmature.transform.Find("RINDO/Geometry/PlayerFollowCamera").gameObject;
        //fpsCamera = playerArmature.transform.Find("RINDO/Geometry/FPSCamera").gameObject;

        // ��ʼ״̬����Ϊ Third Person ģʽ
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
        // ��ȡ����
        avatarController = playerArmature.GetComponent<AvatarController>();


        if (avatarController != null)
        {
            // �л� IsTPS ����ֵ
            avatarController.IsTPS = !avatarController.IsTPS;

            // ���� IsTPS ��״̬�����û���� FPSCamera
            fpsCamera.SetActive(!avatarController.IsTPS);
        }
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
