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

        // ��ʼ״̬����Ϊ Third Person ģʽ
        EnableObjectsBasedOnCameraState();
    }

    void Update()
    {
        // ��Update�м��FPSCamera�����úͽ���
        if (fpsCamera.activeSelf)
        {
            // FPSCamera ���ã�����ָ�����ĸ�����
            SetObjectsActive(false);
        }
        else
        {
            // FPSCamera ���ã�����ָ�����ĸ�����
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
