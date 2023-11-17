/// <summary>
/// This script belongs to cowsins™ as a part of the cowsins´ FPS Engine. All rights reserved. 
/// </summary>
using UnityEngine;
namespace cowsins {
/// <summary>
/// Inheriting from Interactable, this means you can interact with the door
/// Keep in mind that this is highly subject to change on future updates
/// </summary>
[RequireComponent(typeof(BoxCollider))] // Require a trigger collider to detect side
public class DoorInteractable : Interactable
{
    [SerializeField] private string openInteractionText;

    [SerializeField] private string closeInteractionText;

    [SerializeField] private string lockedInteractionText;

    [SerializeField] private bool isLocked;

    [Tooltip("The pivot point for the door"), SerializeField]
    private Transform doorPivot;

    [Tooltip("How much you want to rotate the door"), SerializeField]
    private float openedDoorRotation;

    [Tooltip("rotation speed"), SerializeField]
    private float  speed;

    [SerializeField] private AudioClip openDoorSFX, closeDoorSFX, lockedDoorSFX;

    private bool isOpened = false;

    private Quaternion closedRot;

    private PlayerMovement pl;

    private int side;


    private void Start()
    {
        // Initial settings
        closedRot = doorPivot.rotation;
        pl = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        interactText = openInteractionText; 
    }
    private void Update()
    {
        if (isLocked) interactText = lockedInteractionText; 
        // if we are trying to it, open it
        if(isOpened) doorPivot.localRotation = Quaternion.Lerp(doorPivot.localRotation,
            Quaternion.Euler(new Vector3(doorPivot.localRotation.x, openedDoorRotation * side, doorPivot.localRotation.z)), 
                Time.deltaTime * speed);
        // If we closed it, close it
        if (!isOpened) doorPivot.rotation = Quaternion.Lerp(doorPivot.rotation, closedRot, Time.deltaTime * speed);
    }
    /// <summary>
    /// Check for interaction. Overriding from Interactable.cs
    /// </summary>
    public override void Interact()
    {
        // Check if its locked
        if (isLocked)
        {
            SoundManager.Instance.PlaySound(lockedDoorSFX, 0, .1f,true, 0); 
            return;
        }
        // Change state
        isOpened = !isOpened;

        // Display appropriate UI
        interactText = (isOpened) ? closeInteractionText : openInteractionText; 

        if(isOpened) SoundManager.Instance.PlaySound(openDoorSFX, 0, .1f, true, 0);
        else SoundManager.Instance.PlaySound(closeDoorSFX, 0, .1f,true, 0);

        // Checking the side where we are opening the door from;
        side = (Vector3.Dot(transform.right, pl.orientation.forward) > 0) ? 1 : -1;
    }
    public void Lock() => isLocked = true;

    public void UnLock() => isLocked = false;

    public void ToggleLock() => isLocked = !isLocked;
}
}
