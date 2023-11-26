using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using StarterAssets;

public class TPSShootController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera aimVirtualCamera;
    [SerializeField] private float normalSensitivity;
    [SerializeField] private float aimSensitivity;

    private AvatarController avatarController;
    private StarterAssetsInputs starterAssetsInputs;

    private void Awake()
    {
        avatarController = GetComponent<AvatarController>();
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
    }
    private void Update()
    {
        if (starterAssetsInputs.aim)
        {
            aimVirtualCamera.Priority = 20;
            avatarController.SetSensitivity(aimSensitivity);
        }
        else
        {
            aimVirtualCamera.Priority = 5;
            avatarController.SetSensitivity(normalSensitivity);
        }

    }

}
