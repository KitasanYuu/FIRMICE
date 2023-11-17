using UnityEngine;
using UnityEngine.EventSystems;

namespace cowsins { 

/// <summary>
/// Attachment Group UI groups all the attachment UI elements together.
/// </summary>
public class AttachmentGroupUI : MonoBehaviour, IPointerClickHandler
{
    [HideInInspector]public Transform target;

    private bool active;

    private void OnEnable()
    {
        // Subscribe to the method
        UIEvents.onEnableAttachmentUI += Disable; 
    }
    private void OnDisable()
    {
        // Unsubscribe to the method
        UIEvents.onEnableAttachmentUI -= Disable;
    }
    private void Update()
    {
        // If the target is null, return
        // The target refers to the attachment, so the group is placed on top of it
        if (target == null)
        {
            return;
        }

        // Gather the position
        // First, get the target position, and then transform it into the screen position
        Vector3 objectPosition = target.position;
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(objectPosition);
        // Apply position
        transform.position = screenPosition;
       
    }

    // handle on mouse click ( only if hovering previously )
    public void OnPointerClick(PointerEventData eventData)
    {
        // Handle UI event
        UIEvents.onEnableAttachmentUI?.Invoke(this.gameObject); 

        // If its currently active, disable it
        if(active)
        {
            Disable(null);
            return; 
        }

        // Otherwise, enable it
        Enable(); 
    }

    private void Enable()
    {
        // Set to active
        active = true; 

        // Enable each of the children
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
    }

    private void Disable(GameObject go)
    {
        if (go != null && go == this.gameObject) return; 

        // Set active to false
        active = false; 

        //Deactivate each child
        for (int i = 1; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }
}
}