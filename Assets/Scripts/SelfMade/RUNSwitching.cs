using UnityEngine;

public class FOVController : MonoBehaviour
{
    [SerializeField]
    private Camera targetCamera; // ��Inspector�н�Ŀ�������ק������

    [SerializeField]
    private float normalFOV = 60f; // Ĭ�ϵ�FOV
    [SerializeField]
    private float zoomedFOV = 30f; // ��סShiftʱ��FOV

    [SerializeField]
    private float rotationSpeed = 2f; // ��ת�ٶ�
    [SerializeField]
    private float fovLerpSpeed = 5f; // ��ֵ�ٶ�

    void Update()
    {
        // ����Ƿ�ס Shift ��
        bool isShiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        // �����ס Shift ��ͬʱ���� WASD ��
        if (isShiftPressed && (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)))
        {
            // ��������� FOV
            ChangeFOV(zoomedFOV);
        }
        else
        {
            // �ָ�Ĭ�� FOV
            ChangeFOV(normalFOV);
        }

        // ͨ�������ת���
        RotateCamera();
    }

    void ChangeFOV(float targetFOV)
    {
        if (targetCamera != null)
        {
            // ʹ�ò�ֵ�𽥸ı� FOV
            float currentFOV = targetCamera.fieldOfView;
            float newFOV = Mathf.Lerp(currentFOV, targetFOV, Time.deltaTime * fovLerpSpeed);
            targetCamera.fieldOfView = newFOV;
        }
    }

    void RotateCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        // ������ʵ�ָ��������ת������߼�
        // �����ʹ������������ת�߼�������������Ҫ���߼�
    }
}
