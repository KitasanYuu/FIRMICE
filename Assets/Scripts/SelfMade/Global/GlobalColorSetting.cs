using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GlobalColorSetting", menuName = "Global/GlobalColorSetting", order = 1)]
public class GlobalColorSetting : ScriptableObject
{
    public Color AllyColor;
    public Color NeutralColor;
    public Color EnemyColor;

    public Color AllyBGImageColor;
    public Color NeutralBGImageColor;
    public Color EnemyBGImageColor;
}
