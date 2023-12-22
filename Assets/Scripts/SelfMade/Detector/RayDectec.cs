using playershooting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Detector
{

    public class RayDectec : MonoBehaviour
    {
        private TPSShootController tpsShootController;

        [Tooltip("������ߵĳ�����")]
        [SerializeField] private Transform rayOrigin; // ���ߵķ�����
        public bool isBlockedL = false;
        public bool isBlockedR = false;
        [SerializeField] private LayerMask AvoidColliderLayerMask;
        [SerializeField] private float rayLength = 1.25f; // ���߳���
        [SerializeField] private Vector3[] rayDirectionsL; // ���߷�������L
        [SerializeField] private Vector3[] rayDirectionsR; // ���߷�������R



        private int frameCount = 0; // ��ǰ֡������
        [SerializeField] private int detectionInterval = 30; // �������ÿ30֡����һ�μ��

        private void Awake()
        {
            tpsShootController = GetComponent<TPSShootController>();

            // ���û���ڱ༭����ָ��rayOrigin����ʹ�õ�ǰ�����Transform
            if (rayOrigin == null)
            {
                rayOrigin = transform;
            }
        }

        private void Update()
        {
            Switcher();
        }

        private void PerformRayDetection()
        {
            isBlockedL = CheckRayBlock(rayDirectionsL);
            isBlockedR = CheckRayBlock(rayDirectionsR);
        }

        private bool CheckRayBlock(Vector3[] rayDirections)
        {
            foreach (var direction in rayDirections)
            {
                RaycastHit hit;
                // ʹ��rayOrigin��λ�úͷ��򷢳�����
                if (Physics.Raycast(rayOrigin.position, rayOrigin.TransformDirection(direction), out hit, rayLength, AvoidColliderLayerMask))
                {
                    // �����⵽��ָ���㼶�Ķ����򷵻�true��ʾ���赲
                    Debug.DrawRay(rayOrigin.position, rayOrigin.TransformDirection(direction) * rayLength, Color.red);
                    return true; // ��⵽�赲������Ҫ�����������
                }
                else
                {
                    Debug.DrawRay(rayOrigin.position, rayOrigin.TransformDirection(direction) * rayLength, Color.green);
                }
            }
            return false; // û�м�⵽�赲
        }

        private void Switcher()
        {
            if (tpsShootController != null)
            {
                if (tpsShootController.isAiming)
                {
                    // ÿ��detectionIntervalִ֡��һ�����߼��
                    if (frameCount >= detectionInterval)
                    {
                        PerformRayDetection();
                        frameCount = 0; // ����֡������
                    }
                    frameCount++; // ����֡������
                }
                else
                {
                    return;
                }
            }
            else
            {
                Debug.LogError("RayDetec:No TPSController Found");
            }
        }
    }
}