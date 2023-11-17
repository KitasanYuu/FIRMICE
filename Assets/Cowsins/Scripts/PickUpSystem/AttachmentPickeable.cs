using UnityEngine;

namespace cowsins { 
public class AttachmentPickeable : Pickeable
{
    [Tooltip("Attachment to be picked up. Notice that attachment identifiers can be shared among attachments in different weapons.")]public AttachmentIdentifier_SO attachmentIdentifier;

    private int attachmentID;

    private Attachment atc; 

    public override void Start()
    {
        base.Start();
        // If the pickeable hasnt been dropped, dont keep going
        if (dropped) return;
        GetVisuals();
    }
    public override void Interact()
    {
        // Reference to WeaponController
        WeaponController wCon = player.GetComponent<WeaponController>(); 

        // If the weapon is null or this is not a compatible attachment for the current unholstered weapon, return
        if (wCon.weapon == null ||!CompatibleAttachment(wCon))
        {
            return;
        }
        
        // If it is compatible, assign a new attachment
        // Afterwards, unholster the current weapon and destroy this pickeable.
        wCon.AssignNewAttachment(atc, attachmentID);

        wCon.UnHolster(wCon.inventory[wCon.currentWeapon].gameObject,true); 

        base.Interact();

        Destroy(this.gameObject);
    }

    /// <summary>
    /// Checks if the attachment is compatible with the current unholstered weapon
    /// </summary>
    /// <param name="wCon">WeaponController, attached to the Player</param>
    /// <returns></returns>
    public bool CompatibleAttachment(WeaponController wCon)
    {
        // Loop through all the different compatible attachments types
        for (int i = 0; i < wCon.weapon.weaponObject.compatibleAttachments.barrels.Length; i++)
        {
            // If the "i" element of the compatible attachments array selected is equal to this attachment, assign it.
            if (wCon.weapon.weaponObject.compatibleAttachments.barrels[i].attachmentIdentifier == attachmentIdentifier)
            {
                atc = wCon.weapon.weaponObject.compatibleAttachments.barrels[i]; 
                attachmentID = i;
                return true;
            }
        }
        for (int i = 0; i < wCon.weapon.weaponObject.compatibleAttachments.scopes.Length; i++)
        {
            if (wCon.weapon.weaponObject.compatibleAttachments.scopes[i].attachmentIdentifier == attachmentIdentifier)
            {
                atc = wCon.weapon.weaponObject.compatibleAttachments.scopes[i];
                attachmentID = i;
                return true;
            }
        }
        for (int i = 0; i < wCon.weapon.weaponObject.compatibleAttachments.stocks.Length; i++)
        {
            if (wCon.weapon.weaponObject.compatibleAttachments.stocks[i].attachmentIdentifier == attachmentIdentifier)
            {
                atc = wCon.weapon.weaponObject.compatibleAttachments.stocks[i];
                attachmentID = i;
                return true;
            }
        }
        for (int i = 0; i < wCon.weapon.weaponObject.compatibleAttachments.grips.Length; i++)
        {
            if (wCon.weapon.weaponObject.compatibleAttachments.grips[i].attachmentIdentifier == attachmentIdentifier)
            {
                atc = wCon.weapon.weaponObject.compatibleAttachments.grips[i];
                attachmentID = i;
                return true;
            }
        }
        for (int i = 0; i < wCon.weapon.weaponObject.compatibleAttachments.magazines.Length; i++)
        {
            if (wCon.weapon.weaponObject.compatibleAttachments.magazines[i].attachmentIdentifier == attachmentIdentifier)
            {
                atc = wCon.weapon.weaponObject.compatibleAttachments.magazines[i];
                attachmentID = i;
                return true;
            }
        }
        for (int i = 0; i < wCon.weapon.weaponObject.compatibleAttachments.flashlights.Length; i++)
        {
            if (wCon.weapon.weaponObject.compatibleAttachments.flashlights[i].attachmentIdentifier == attachmentIdentifier)
            {
                atc = wCon.weapon.weaponObject.compatibleAttachments.flashlights[i];
                attachmentID = i;
                return true;
            }
        }
        for (int i = 0; i < wCon.weapon.weaponObject.compatibleAttachments.lasers.Length; i++)
        {
            if (wCon.weapon.weaponObject.compatibleAttachments.lasers[i].attachmentIdentifier == attachmentIdentifier)
            {
                atc = wCon.weapon.weaponObject.compatibleAttachments.lasers[i];
                attachmentID = i;
                return true;
            }
        }
        return false; 
    }

    // Get visuals of the attachment when dropping
    public override void Drop(WeaponController wcon, Transform orientation)
    {
        base.Drop(wcon, orientation);

        GetVisuals();
    }
    public void GetVisuals()
    {
        // Get whatever we need to display
        if(attachmentIdentifier == null)
        {
            Debug.LogError("Attachment Identifier not set-up! Please assign a proper attachment identifier to your existing attachments, otherwise the system won´t work properly.");
            return;  
        }
        interactText = attachmentIdentifier.attachmentName;
        image.sprite = attachmentIdentifier.attachmentIcon;
        if (attachmentIdentifier.pickUpGraphics == null) return;
        Destroy(graphics.GetChild(0).gameObject); 
        Instantiate(attachmentIdentifier.pickUpGraphics,transform.position,Quaternion.identity,graphics); 
    }
}
}