using UnityEngine;
namespace cowsins {
public class Barrel : Attachment
{
    [Header("Barrel")]
    [Tooltip("SFX played instead of the original. You cannot leave this unassigned.")]public AudioClip supressedFireSFX;
    [Tooltip("You can leave this unassigned to use the default muzzle flash VFX on your weapon scriptable object")]public GameObject uniqueMuzzleFlashVFX; 
}
}