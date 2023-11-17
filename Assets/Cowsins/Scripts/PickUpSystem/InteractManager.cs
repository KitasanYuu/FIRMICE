/// <summary>
/// This script belongs to cowsins™ as a part of the cowsins´ FPS Engine. All rights reserved. 
/// </summary>
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

namespace cowsins {
public class InteractManager : MonoBehaviour
{
    [System.Serializable]
    public class Events { public UnityEvent OnFinishedInteraction; }

    [Tooltip("Attach your main camera"), SerializeField] private Camera mainCamera; // Attach your main camera

    private Transform orientation;

    [Tooltip("Object with the same height as your camera, used to orientate the player."), SerializeField] private LayerMask mask;

    private GameObject lookingAt;

    [Tooltip("Enable this toggle if you want to be able to drop your weapons")] public bool canDrop;

    [Tooltip("Attach the generic pickeable object here"), SerializeField] private Pickeable weaponGenericPickeable;

    [Tooltip("Attach the generic pickeable object here"), SerializeField] private Pickeable attachmentGenericPickeable;

    [Tooltip("Distance from the player to detect interactable objects"), SerializeField] private float detectInteractionDistance;

    [Tooltip("Distance from the player where the pickeable will be instantiated"), SerializeField] private float droppingDistance;

    [SerializeField, Tooltip("How much time player has to hold the interact button in order to successfully interact")] private float progressRequiredToInteract;

    [HideInInspector] public float progressElapsed;

    [HideInInspector] public bool alreadyInteracted = false;

    [Tooltip("Adjust the interaction interval, the lower, the faster you will be able to interact"), Range(.2f, .7f), SerializeField] private float interactInterval = .4f;

    [Tooltip("If true, the player will be able to inspect the current weapon.")]public bool canInspect;

    [Tooltip("Allows the player to equip and unequip attachments while inspecting. It also displays a custom UI for that.")]public bool realtimeAttachmentCustomization;

    [Tooltip("When inspecting, display current attachments only. Otherwise you will be able to see all compatible attachments.")]public bool displayCurrentAttachmentsOnly; 

    [HideInInspector] public bool inspecting = false;

    public Events events;

    private WeaponController wcon;

    private void OnEnable()
    {
        // Subscribe to the event
        UIEvents.onAttachmentUIElementClicked += DropAttachment; 
    }

    private void OnDisable()
    {
        // Unsubscribe to the event
        UIEvents.onAttachmentUIElementClicked = null;
    }

    private void Start()
    {
        // Grab main references
        wcon = GetComponent<WeaponController>();
        orientation = GetComponent<PlayerMovement>().orientation;
        mainCamera = GetComponent<WeaponController>().mainCamera;
    }

    private void Update()
    {
        // If we already interacted, or the player is not controllable, return!
        if (alreadyInteracted || !PlayerStats.Controllable) return;

        DetectPickeable();
        DetectInput();
        HandleDrop();
    }
    private void DetectPickeable()
    {
        // It will be used later on to determine the hit object.
        RaycastHit interactableHit;

        // If we got a hit from the raycast:
        if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out interactableHit, detectInteractionDistance, mask))
        {
            // Check if the interaction is forbidden
            if (IsForbiddenInteraction(interactableHit))
            {
                // If it is forbidden, call the event
                UIEvents.forbiddenInteraction?.Invoke();
            }
            else
            {
                // If its not, enable interaction UI to display an interaction
                EnableInteractionUI(interactableHit.collider.GetComponent<Interactable>());
            }
        }
        else
        {
            // If we dont find any interactable, disable interactions UI
            DisableLookingAtInteractable();
            DisableInteractionUI();
        }

    }
    // Checks if the interaction is forbidden
    private bool IsForbiddenInteraction(RaycastHit hit)
    {
        // Gather the BulletsPickeable component
        BulletsPickeable bulletsPickable = hit.collider.GetComponent<BulletsPickeable>();
        // Gathers the AttachmentPickeable component
        AttachmentPickeable attachmentPickeable = hit.collider.GetComponent<AttachmentPickeable>();
        // Gathers the WeaponController component
        WeaponController weaponController = GetComponent<WeaponController>();

        // Check if we can pick the pickeable up or not and return a boolean.
        return ( bulletsPickable != null && (weaponController.weapon != null && !weaponController.weapon.limitedMagazines || weaponController.weapon == null)) ||
            (attachmentPickeable != null && ( weaponController.weapon !=null && !attachmentPickeable.CompatibleAttachment(weaponController) || weaponController.weapon == null));
    }

    private void EnableInteractionUI(Interactable interactable)
    {
        if(interactable == null) return;
        interactable.interactable = true;
        // Current interactable is equal to the passed interactable value
        if(lookingAt == interactable.gameObject) return;
        lookingAt = interactable.gameObject;
        interactable.Highlight(); 
        UIEvents.allowedInteraction?.Invoke(interactable.interactText);
    }

    private void DisableLookingAtInteractable()
    {
        if (lookingAt != null)
        {
            lookingAt.GetComponent<Interactable>().interactable = false;
            lookingAt = null;
        }
    }

    private void DisableInteractionUI()
    {
        DisableLookingAtInteractable();
        UIEvents.disableInteractionUI?.Invoke();
    }

    private void DetectInput()
    {
        if (lookingAt == null)
        {
            progressElapsed = -.01f;
            return;
        }
        // If we dont detect an interactable then dont continue
        // However if we detected an interactable + we pressing the interact button, then: 
        if (InputManager.interacting)
        {
            progressElapsed += Time.deltaTime;
            if (progressRequiredToInteract > 0)
            {
                UIEvents.onInteractionProgressChanged?.Invoke(progressElapsed / progressRequiredToInteract);           
            }
        }
        else
        {
            progressElapsed = -.01f;
            UIEvents.onFinishInteractionProgress?.Invoke(); 
        }
        // Interact
        if (progressElapsed >= progressRequiredToInteract) PerformInteraction();
    }

    private void PerformInteraction()
    {
        progressElapsed = -.01f;
        // prevent from spamming
        alreadyInteracted = true;
        // Perform any interaction you may like
        // Please note that classes that inherit from interactable can override the virtual void Interact()
        lookingAt.GetComponent<Interactable>().Interact();
        // Prevent from spamming but let the user interact again
        Invoke("ResetInteractTimer", interactInterval);
        // Manage UI
        lookingAt = null;

        UIEvents.disableInteractionUI?.Invoke();
        UIEvents.onFinishInteractionProgress?.Invoke();
        events.OnFinishedInteraction.Invoke(); // Call our event
    }
    private void HandleDrop()
    {
        // Handles weapon dropping by pressing the drop button
        if (!InputManager.dropping || wcon.weapon == null || wcon.Reloading || !canDrop) return;

        WeaponPickeable pick = Instantiate(weaponGenericPickeable, orientation.position + orientation.forward * droppingDistance, orientation.rotation) as WeaponPickeable;
        pick.Drop(wcon, orientation);
        WeaponIdentification wp = wcon.inventory[wcon.currentWeapon];
        pick.SetPickeableAttachments(wp.barrel,wp.scope,wp.stock,wp.grip,wp.magazine,wp.flashlight,wp.laser); 

        wcon.ReleaseCurrentWeapon();
    }
    private void ResetInteractTimer() => alreadyInteracted = false;

    public void GenerateInspectionUI()
    {
        UIEvents.onGenerateInspectionUI?.Invoke(wcon);
    }

    /// <summary>
    /// Drops the current attachment to the ground ( generates a new attachment pickeable )
    /// </summary>
    /// <param name="atc">Attachment to drop </param>
    /// <param name="enableDefault">Enables the default attachment when dropped if true.</param>
    public void DropAttachment(Attachment atc, bool enableDefault)
    {
        // Spawn a new pickeable.
        AttachmentPickeable pick = Instantiate(attachmentGenericPickeable, orientation.position + orientation.forward * droppingDistance, orientation.rotation) as AttachmentPickeable;
        // Assign the appropriate attachment identifier to the spawned pickeable.
        pick.attachmentIdentifier = atc.attachmentIdentifier;
        // Get visuals
        pick.Drop(wcon, orientation);

        // Grab the current weaponidentification object.
        WeaponIdentification weapon = wcon.inventory[wcon.currentWeapon];
        // Store all the types of attachments and the current attachment of each type inside a dictionary.
        Dictionary<Type, Attachment> attachments = new Dictionary<Type, Attachment> {
        { typeof(Barrel), weapon.barrel },
        { typeof(Scope), weapon.scope },
        { typeof(Stock), weapon.stock },
        { typeof(Grip), weapon.grip },
        { typeof(Magazine), weapon.magazine },
        { typeof(Flashlight), weapon.flashlight },
        { typeof(Laser), weapon.laser }
    };

        // Grab what type of attachment it is, returns barrel, Scope, etc...
        Type attachmentType = atc.GetType();
        // Check if any of the attachments saved in the dictionary is the same type as the attachment to drop type.
        if (attachments.ContainsKey(attachmentType))
        {
            Attachment defAtc = null;

            // Check all the attachment types 
            // This will determine which attachment type matches the dropped attachment
            switch(attachmentType)
            {
                // If the type is Barrel:
                case Type t when t == typeof(Barrel):
                    // If the current barrel is not null, disable it
                    if(weapon.barrel != null)
                    weapon.barrel.gameObject.SetActive(false); 
                    // Because it dropped, store the default barrel in case it exists for later usage.
                    defAtc = weapon.defaultAttachments.defaultBarrel;
                    break;
                case Type t when t == typeof(Scope):
                    if (weapon.scope != null)
                        weapon.scope.gameObject.SetActive(false);
                    defAtc = weapon.defaultAttachments.defaultScope;
                    break;
                case Type t when t == typeof(Stock):
                    if (weapon.stock != null)
                        weapon.stock.gameObject.SetActive(false);
                    defAtc = weapon.defaultAttachments.defaultStock;
                    break;
                case Type t when t == typeof(Grip):
                    if (weapon.grip != null)
                        weapon.grip.gameObject.SetActive(false);
                    defAtc = weapon.defaultAttachments.defaultGrip;
                    break;
                case Type t when t == typeof(Magazine):
                    weapon.magazine.gameObject.SetActive(false);
                    defAtc = weapon.defaultAttachments.defaultMagazine;
                    break;
                case Type t when t == typeof(Flashlight):
                    if (weapon.flashlight != null)
                        weapon.flashlight.gameObject.SetActive(false);
                    defAtc = weapon.defaultAttachments.defaultFlashlight;
                    break;
                case Type t when t == typeof(Laser):
                    if (weapon.laser != null)
                        weapon.laser.gameObject.SetActive(false);
                    defAtc = weapon.defaultAttachments.defaultLaser;
                    break;
            }

            Attachment defaultAttachment = defAtc ?? null;
           
            // If the default attachment is not null, and we should enable default attachments, assign it and enable it
            if (defaultAttachment != null && enableDefault)
            {
                attachments[attachmentType] = defaultAttachment;
                defaultAttachment.gameObject.SetActive(true);
            }
            else
            {
                // Otherwise do not assign anything
                attachments[attachmentType] = null;
            }
        }

        weapon.barrel = attachments[typeof(Barrel)] as Barrel;
        weapon.scope = attachments[typeof(Scope)] as Scope;
        weapon.stock = attachments[typeof(Stock)] as Stock;
        weapon.grip = attachments[typeof(Grip)] as Grip;
        weapon.magazine = attachments[typeof(Magazine)] as Magazine;
        weapon.flashlight = attachments[typeof(Flashlight)] as Flashlight;
        weapon.laser = attachments[typeof(Laser)] as Laser;

        // Adjust the magazine size and unholster the weapon
        wcon.id.GetMagazineSize();
        wcon.UnHolster(weapon.gameObject, false);

        if(displayCurrentAttachmentsOnly)
            UIController.instance.GenerateInspectionUI(wcon); 
    }
}
}