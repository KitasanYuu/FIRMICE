using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace cowsins {
/// <summary>
/// This class handles the UI element for attachments, which is clickable and can be used to equip, or drop attachments.
/// </summary>
public class AttachmentUIElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField,Tooltip("Image that represents the background you want to use. This will change colour depending if the attachment is equipped or not")] private Image background; 

    [SerializeField,Tooltip("Displays the attachment icon. The attachment Icon can be set on its Attachment Identifier SO")] private Image iconDisplay;

    [HideInInspector] public Attachment atc;

    [HideInInspector] public int id; 

    [HideInInspector] public bool assigned;

    [HideInInspector] public Color assignedColor, unAssignedColor;


    private bool highlighted = false;

    private void OnEnable()
    {
        // Subscribe to the method on clicking 
        UIEvents.onAttachmentUIElementClickedNewAttachment += DeselectAll; 
    }

    private void OnDisable()
    {
        // Unsubscribe to the method on clicking 
        UIEvents.onAttachmentUIElementClickedNewAttachment -= DeselectAll;
    }

    private void Update()
    {
        // Grab the right scale
        // If the UI is highlighted, assign a bigger scale, if not assign the default one which is 1,1,1 = Vector3.one
        Vector3 scale = highlighted ? Vector3.one * 1.2f : Vector3.one;

        // Apply the scale ( Smooth it using Lerp )
        transform.localScale = Vector3.Lerp(transform.localScale, scale, 5 * Time.deltaTime);
    }

    // Set to highlighted on mouse enter
    public void OnPointerEnter(PointerEventData eventData)
    {
        highlighted = true;
    }
    // Set to not highlighted on mouse exit
    public void OnPointerExit(PointerEventData eventData)
    {
        highlighted = false;
    }

    // Handle on click
    public void OnPointerClick(PointerEventData eventData)
    {
        // If the attachment is assigned, deselect it 
        if (assigned)
        {
            UIEvents.onAttachmentUIElementClicked?.Invoke(atc,true);
            DeselectAll(atc, id); 
        }
        else
        {
            // if its not assigned, equip it
            UIEvents.onAttachmentUIElementClickedNewAttachment?.Invoke(atc, id);
            SelectAsAssigned();
        }
    }

    /// <summary>
    /// Deselect attachment UI element
    /// </summary>
    /// <param name="atc">Current Attachment</param>
    /// <param name="id">ID of the attachment (order ID in its array)</param>
    public void DeselectAll(Attachment atc, int id)
    {
        background.color = unAssignedColor;
        assigned = false; 
    }

    /// <summary>
    /// Select attachment UI Elemen
    /// </summary>
    public void SelectAsAssigned()
    {
        background.color = assignedColor;
        assigned = true; 
    }

    /// <summary>
    /// Set attachment Icon to the attachment UI
    /// </summary>
    /// <param name="icon">Attachment icon</param>
    public void SetIcon(Sprite icon) => iconDisplay.sprite = icon; 
}
}