using UnityEngine;
namespace cowsins {
[CreateAssetMenu(fileName = "NewAttachmentIdentifier", menuName = "COWSINS/New Attachment Identifier", order = 2)]
public class AttachmentIdentifier_SO : ScriptableObject
{
    [Tooltip("Name of the attachment that will be displayed on pick up UI elements etc")]public string attachmentName;
    [Tooltip("Icon (Sprite) of the attachment")] public Sprite attachmentIcon;
    [Tooltip("Graphics to be displayed for the pickeable object... NOT FOR THE ACTUAL ATTACHMENT")] public GameObject pickUpGraphics;
}
}