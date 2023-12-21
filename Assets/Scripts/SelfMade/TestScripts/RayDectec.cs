using playershooting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Detector
{

    public class RayDectec : MonoBehaviour
    {
        private TPSShootController tpsShootController;

        public bool isBlockedL = false;
        public bool isBlockedR = false;
        [SerializeField] private LayerMask AvoidColliderLayerMask;
        [SerializeField] private float rayLength = 5.0f; // ���߳���
        [SerializeField] private Vector3[] rayDirectionsL; // ���߷�������L
        [SerializeField] private Vector3[] rayDirectionsR; // ���߷�������R

        private int frameCount = 0; // ��ǰ֡������
        [SerializeField] private int detectionInterval = 10; // �������ÿ10֡����һ�μ��

        private void Awake()
        {
            tpsShootController = FindObjectOfType<TPSShootController>();
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
                if (Physics.Raycast(transform.position, transform.TransformDirection(direction), out hit, rayLength, AvoidColliderLayerMask))
                {
                    // �����⵽��ָ���㼶�Ķ����򷵻�true��ʾ���赲
                    Debug.DrawRay(transform.position, transform.TransformDirection(direction) * rayLength, Color.red);
                    return true; // ��⵽�赲������Ҫ�����������
                }
                else
                {
                    Debug.DrawRay(transform.position, transform.TransformDirection(direction) * rayLength, Color.green);
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