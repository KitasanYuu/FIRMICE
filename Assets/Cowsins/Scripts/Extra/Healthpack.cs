/// <summary>
/// This script belongs to cowsins™ as a part of the cowsins´ FPS Engine. All rights reserved. 
/// </summary>
using UnityEngine;
namespace cowsins {
public class Healthpack : PowerUp
{
    [Tooltip("Amount of health to be restored")] [Range(.1f, 1000), SerializeField] private float healAmount;
    public override void Interact(PlayerStats player)
    {
        base.Interact(player);
        if (player.maxShield == 0 && player.health == player.maxHealth || player.maxShield != 0 && player.shield == player.maxShield) return;
        used = true;
        timer = reappearTime;
        player.Heal(healAmount);
    }

}
}