/// <summary>
/// This script belongs to cowsins™ as a part of the cowsins´ FPS Engine. All rights reserved. 
/// </summary>
using UnityEngine;

namespace cowsins {
/// <summary>
/// Inheriting from destructible, lets you explode barrels
/// </summary>
public class ExplosiveBarrel : Destructible
{
    [SerializeField]private float explosionRadius;

    [SerializeField] private float explosionForce;

    [Tooltip("Damage dealt on explosion to any Damageable object within the radius." +
        "NOTE:Damage will be scaled depending on how far the object is from the center of the explosion "),SerializeField]
    private float damage; 

    [Header("Effects")]
    [Tooltip("Instantiate this when the barrel explodes"), SerializeField]
    private GameObject destroyedObject, explosionVFX; 

    /// <summary>
    /// Override the method from Destructible.cs
    /// Here we are damaging IDamageables within a certain radius & also instantiating some effect on destructed.
    /// </summary>
    public override void Die()
    {
        SoundManager.Instance.PlaySound(destroyedSFX, 0, .1f, true, 0);
        Collider[] cols = Physics.OverlapSphere(transform.position, explosionRadius);

        Instantiate(destroyedObject,transform.position,Quaternion.identity);
        Instantiate(explosionVFX,transform.position,Quaternion.identity);

        foreach(Collider c in cols)
        {
            if (c.GetComponent<Rigidbody>() != null) 
                c.GetComponent<Rigidbody>().AddExplosionForce(explosionForce / (Vector3.Distance(c.transform.position, transform.position) + .1f), transform.position, explosionRadius, 5,ForceMode.Impulse);
           
            float dmg = damage / Vector3.Distance(c.transform.position, transform.position);
            if (c.CompareTag("BodyShot"))
            {   
                CowsinsUtilities.GatherDamageableParent(c.transform).Damage(dmg);
                continue;
            }
            else if (c.GetComponent<IDamageable>() != null)
            {
                c.GetComponent<IDamageable>().Damage(dmg);
                continue;
            }
                if (c.GetComponent<PlayerMovement>() != null)
              CamShake.instance.ExplosionShake(Vector3.Distance(CamShake.instance.gameObject.transform.position,transform.position));
        }
        base.Die();
    }
}
}