using UnityEngine;
namespace cowsins {
public class Magazine : Attachment
{
    [Header("Magazine")]
    [Tooltip("Capacity to add to the default magazine. If this magazine supports less bullets, set a negative value.")]public int magazineCapacityAdded; 
}
}