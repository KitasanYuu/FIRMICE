/// <summary>
/// This script belongs to cowsins™ as a part of the cowsins´ FPS Engine. All rights reserved. 
/// </summary>
using UnityEngine;
namespace cowsins {
/// <summary>
/// Incline your weapon whenever you crouch
/// </summary>
public class CrouchTilt : MonoBehaviour
{

    [Tooltip("Rotation desired when crouching"),SerializeField]private Vector3 tiltRot, tiltPosOffset;

    [Tooltip("Tilting / Rotation velocity"), SerializeField] private float tiltSpeed;

    [HideInInspector] public PlayerMovement player;

    [HideInInspector] public WeaponController wp;

    private bool crouching;

    private Quaternion origRot;

    private Vector3 origPos; 

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        wp = GameObject.FindGameObjectWithTag("Player").GetComponent<WeaponController>();
        origRot = transform.localRotation;
        origPos = transform.localPosition; 
    }

    void Update()
    {
        // If we are crouching + not aiming Tilt
        if (player.isCrouching && !wp.isAiming)
        {
            crouching = true;
           transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(tiltRot), Time.deltaTime * tiltSpeed);
            transform.localPosition = Vector3.Lerp(transform.localPosition, origPos+ tiltPosOffset, Time.deltaTime * tiltSpeed);
        }
        else // If not, come back
        {
            crouching = false;
            transform.localRotation = Quaternion.Lerp(transform.localRotation, origRot, Time.deltaTime * tiltSpeed);
            transform.localPosition = Vector3.Lerp(transform.localPosition, origPos, Time.deltaTime * tiltSpeed);
        }
        if(crouching && wp.isAiming) 
       {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, origRot, Time.deltaTime * tiltSpeed);
            transform.localPosition = Vector3.Lerp(transform.localPosition, origPos, Time.deltaTime * tiltSpeed);
        }
    }
}
}