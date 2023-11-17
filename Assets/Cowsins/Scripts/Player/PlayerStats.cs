/// <summary>
/// This script belongs to cowsins™ as a part of the cowsins´ FPS Engine. All rights reserved. 
/// </summary>
using UnityEngine;
using UnityEngine.Events;
using cowsins;
namespace cowsins {
public class PlayerStats : MonoBehaviour, IDamageable
{
    [System.Serializable]
    public class Events
    {
        public UnityEvent OnDeath, OnDamage, OnHeal;
    }

    #region variables

        public float health,shield;

        public float maxHealth, maxShield, damageMultiplier,healMultiplier;  

        [Tooltip("Turn on to apply damage on falling from great height")] public bool takesFallDamage;

        [Tooltip("Minimum height ( in units ) the player has to fall from in order to take damage"), SerializeField,Min(1)] private float minimumHeightDifferenceToApplyDamage;

        [Tooltip("How the damage will increase on landing if the damage on fall is going to be applied"), SerializeField] private float fallDamageMultiplier;

        public float? height = null; 
    
        [HideInInspector] public bool isDead;

        private PlayerMovement player; 

        private PlayerStats stats; 
        
        public Events events;

    #endregion

    private void Start()
    {
        GetAllReferences();
        // Apply basic settings 
        health = maxHealth;
        shield = maxShield;
        damageMultiplier = 1;
        healMultiplier = 1;

        UIEvents.basicHealthUISetUp?.Invoke(health,shield,maxHealth,maxShield); 

        GrantControl(); 
    }

    private void Update()
    {
        Controllable = controllable; 

        if (stats.isDead) return; // If player is alive, continue

        if (health <= 0) Die(); // Die in case we ran out of health   
        
        // Manage fall damage
        if (!takesFallDamage) return;
            ManageFallDamage();
    }
    /// <summary>
    /// Our Player Stats is IDamageable, which means it can be damaged
    /// If so, call this method to damage the player
    /// </summary>
    public void Damage(float _damage)
    {
        if (player.canDash && player.dashing && player.damageProtectionWhileDashing) return;

        float damage = Mathf.Abs(_damage); 
        events.OnDamage.Invoke(); // Invoke the custom event

        if (damage <= shield)
            shield -= damage;
        else
        {
            damage = damage - shield;
            shield = 0;
            health -= damage;
        }
        // Effect on damage
        UIEvents.onHealthChanged?.Invoke(health, shield,true);
    }

    public void Heal(float healAmount_)
    {
        float healAmount = Mathf.Abs(healAmount_ * healMultiplier);
        // If we are full health do not heal 
        // Also checks if we have an initial shield or not
        if (maxShield != 0 && shield == maxShield || maxShield == 0 && health == maxHealth) return;

        events.OnHeal.Invoke(); // Invoke our custom event

        if (health + healAmount > maxHealth) // Check if heal exceeds health 
        {
            float remaining = maxHealth - health + healAmount;
            health = maxHealth;

            // Check if we have a shield to be healed
            if(maxShield != 0)
           {
                if (shield + remaining > maxShield) shield = maxShield; // Then we have to apply the remaining heal to our shield 
                else shield += remaining;
            }
        }
        else health += healAmount; // If not just apply your heal

        // effect on heal 
        UIEvents.onHealthChanged?.Invoke(health, shield, false);
    }
    /// <summary>
    /// Perform any actions On death
    /// </summary>
    private void Die()
    {
        isDead = true;
        events.OnDeath.Invoke(); // Invoke a custom event
    }
    /// <summary>
    /// Basically find everything the script needs to work
    /// </summary>
    private void GetAllReferences()
    {
        stats = GetComponent<PlayerStats>();
        player = GetComponent<PlayerMovement>();

        if (PauseMenu.Instance == null) return; 

        PauseMenu.Instance.stats = this; 
    }
    /// <summary>
    /// While airborne, if you exceed a certain time, damage on fall will be applied
    /// </summary>
    private void ManageFallDamage()
    {
        // Grab current player height
        if(!player.grounded && transform.position.y > height || !player.grounded && height == null) height = transform.position.y; 

        // Check if we landed, as well if our current height is lower than the original height. If so, check if we should apply damage
        if(player.grounded && height != null && transform.position.y < height)
        {
            float currentHeight = transform.position.y;

            // Transform nullable variable into a non nullable float for later operations
            float noNullHeight = height ?? default(float);

            float heightDifference = noNullHeight - currentHeight;

            // If the height difference is enough, apply damage
            if (heightDifference > minimumHeightDifferenceToApplyDamage) Damage(heightDifference * fallDamageMultiplier);

            // Reset height
            height = null;
        }
    }

    public bool controllable { get; private set; } = true; 

    public static bool Controllable { get; private set;  }


    public void GrantControl() => controllable = true;

    public void LoseControl() => controllable = false;

    public void ToggleControl() => controllable = !controllable;

    public void CheckIfCanGrantControl()
    {
        if (PauseMenu.isPaused || isDead) return;
        GrantControl(); 
    }
}
}