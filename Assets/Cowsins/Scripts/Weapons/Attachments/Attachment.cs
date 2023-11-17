using UnityEngine;
namespace cowsins {
    public abstract class Attachment : MonoBehaviour
    {
        [Header("Basic")]
        [Tooltip("Identifier of the attachment. You can have the same attachment within different weapons as long as they share this attachment identifier" +
                "scriptable object.")]public AttachmentIdentifier_SO attachmentIdentifier; 
        [Tooltip("Weight to add to the weapon. If it weighs less, set a negative value.")]public float weightAdded;
        [Range(-1f,1f),Tooltip("Percentage added to the base reload speed")] public float reloadSpeedIncrease;
        [Range(-1f, 1f), Tooltip("Percentage added to the base aim speed")] public float aimSpeedIncrease;
        [Range(-1f, 1f), Tooltip("Percentage added to the base fire rate")] public float fireRateIncrease;
        [Range(-.99f, 5f), Tooltip("Percentage added to the base damage")] public float damageIncrease;
        [Range(-1f, 1f)] public float cameraShakeMultiplier;
        [Range(-1f, 1f), Tooltip("Percentage added to the base penetration")] public float penetrationIncrease;
    }
}