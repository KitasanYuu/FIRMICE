/// <summary>
/// This script belongs to cowsins™ as a part of the cowsins´ FPS Engine. All rights reserved. 
/// </summary>
using UnityEngine;
namespace cowsins {
/// <summary>
/// This component must be add to ANY destructible object in your environment.
/// However, you might want to check other examples such as Crate.cs or ExplosiveBarrel.cs
/// They are different things, they do different things, but they both are DESTRUCTIBLES and you can destroy both of them.
/// Then, how does that work? Their custom scripts both inherit from destructible.cs, which inherits from MonoBehaviour, so even though they do 
/// different things they are actually the same kind of thing.
/// Since Die() is a virtual void you can override it on your custom script inheriting from this one
/// 
/// For any doubts check the documentation or contact the support.
/// 
/// </summary>
public class Destructible : MonoBehaviour,IDamageable
{
    float health;
    [Tooltip("Initial health of the destructible"),SerializeField]protected float maxHealth;

    [Tooltip("Instantiate something cool when the object is destroyed, such as coins, a weapon, or any kind of loot, " +
        "whatever you want! If this is empty, no reward will be instantiated"),SerializeField]protected GameObject lootInside;

    [SerializeField]protected AudioClip destroyedSFX; 

    // Set health
    private void Start() => health = maxHealth;

    private void Update()
    {
        // Handle destruction
        if (health <= 0) Die();
    }

    // Handle damage, have in mind that this is also IDamageable
    public void Damage(float damage) => health -= damage;

    /// <summary>
    /// Make sure to override this on your new custom class.
    /// If you still wanna call this method, make sure to write the following line:
    /// base.Die();
    /// Check Crate.cs for a clear example.
    /// </summary>
    public virtual void Die()
    {   
        if (lootInside != null) Instantiate(lootInside, transform.position, Quaternion.identity);
        Destroy(this.gameObject);
    }
}
}