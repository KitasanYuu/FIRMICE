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
        [SerializeField] private float rayLength = 5.0f; // 射线长度
        [SerializeField] private Vector3[] rayDirectionsL; // 射线方向数组L
        [SerializeField] private Vector3[] rayDirectionsR; // 射线方向数组R

        private int frameCount = 0; // 当前帧计数器
        [SerializeField] private int detectionInterval = 10; // 检测间隔，每10帧进行一次检测

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
                    // 如果检测到了指定层级的对象，则返回true表示被阻挡
                    Debug.DrawRay(transform.position, transform.TransformDirection(direction) * rayLength, Color.red);
                    return true; // 检测到阻挡，不需要检测其他方向
                }
                else
                {
                    Debug.DrawRay(transform.position, transform.TransformDirection(direction) * rayLength, Color.green);
                }
            }
            return false; // 没有检测到阻挡
        }

        private void Switcher()
        {
            if (tpsShootController != null)
            {
                if (tpsShootController.isAiming)
                {
                    // 每隔detectionInterval帧执行一次射线检测
                    if (frameCount >= detectionInterval)
                    {
                        PerformRayDetection();
                        frameCount = 0; // 重置帧计数器
                    }
                    frameCount++; // 增加帧计数器
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