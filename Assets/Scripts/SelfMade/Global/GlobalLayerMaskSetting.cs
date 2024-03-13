using UnityEngine;

[CreateAssetMenu(fileName = "GlobalLayerMaskSetting", menuName = "Global/GlobalLayerMaskSetting", order = 1)]
public class GlobalLayerMaskSetting : ScriptableObject
{
    public LayerMask BulletDestoryLayer;
}
