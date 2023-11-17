/// <summary>
/// This script belongs to cowsins™ as a part of the cowsins´ FPS Engine. All rights reserved. 
/// </summary>
using UnityEngine;
namespace cowsins {
public class Bullet : MonoBehaviour
{
    [HideInInspector]public float speed,damage;

    [HideInInspector] public Vector3 destination;

    [HideInInspector] public bool gravity;

    [HideInInspector] public Transform player;

    [HideInInspector]public bool hurtsPlayer;

    [HideInInspector] public bool explosionOnHit;

    [HideInInspector] public GameObject explosionVFX;

    [HideInInspector] public float explosionRadius,explosionForce;

    [HideInInspector] public float criticalMultiplier;

    [HideInInspector] public float duration;

    private void Start()
    {
        transform.LookAt(destination);
        Invoke("DestroyProjectile", duration);
    }
    private void Update() => transform.Translate(0.0f, 0.0f, speed * Time.deltaTime);

    private bool projectileHasAlreadyHit = false; // Prevent from double hitting issues

    private void OnTriggerEnter(Collider other)
    {
            if (projectileHasAlreadyHit) return;
            if (other.CompareTag("Critical"))
            {
                CowsinsUtilities.GatherDamageableParent(other.transform).Damage(damage * criticalMultiplier);
                DestroyProjectile();
                projectileHasAlreadyHit = true;
                return;
            }
            else if (other.CompareTag("BodyShot"))
            {
                CowsinsUtilities.GatherDamageableParent(other.transform).Damage(damage);
                DestroyProjectile();
                projectileHasAlreadyHit = true;
                return;
            }
            else if (other.GetComponent<IDamageable>() != null && !other.CompareTag("Player"))
            {
                other.GetComponent<IDamageable>().Damage(damage);
                DestroyProjectile();
                projectileHasAlreadyHit = true;
                return;
            }
            if (other.gameObject.layer == 3 || other.gameObject.layer == 8 || other.gameObject.layer == 10
            || other.gameObject.layer == 11 || other.gameObject.layer == 12 || other.gameObject.layer == 13 || other.gameObject.layer == 7) DestroyProjectile(); // Whenever it touches ground / object layer
    }


    private void DestroyProjectile()
    {
        if (explosionOnHit)
        {
            if (explosionVFX != null)
            {
                Vector3 contact = GetComponent<Collider>().ClosestPoint(transform.position);
                GameObject impact = Instantiate(explosionVFX, contact, Quaternion.identity);
                impact.transform.rotation = Quaternion.LookRotation(player.position);
            }
            Collider[] cols = Physics.OverlapSphere(transform.position, explosionRadius);

            foreach (Collider c in cols)
            {
                IDamageable damageable = c.GetComponent<IDamageable>();
                PlayerMovement playerMovement = c.GetComponent<PlayerMovement>();
                Rigidbody rigidbody = c.GetComponent<Rigidbody>();

                if (damageable != null)
                {
                    if (c.CompareTag("Player") && hurtsPlayer)
                    {
                        float dmg = damage * Mathf.Clamp01(1 - Vector3.Distance(c.transform.position, transform.position) / explosionRadius);
                        damageable.Damage(dmg);
                    }
                    if (!c.CompareTag("Player"))
                    {
                        float dmg = damage * Mathf.Clamp01(1 - Vector3.Distance(c.transform.position, transform.position) / explosionRadius);
                        damageable.Damage(dmg);
                    }
                }
                if (playerMovement != null)
                {
                    CamShake.instance.ExplosionShake(Vector3.Distance(CamShake.instance.gameObject.transform.position, transform.position));
                }
                if (rigidbody != null && c != this)
                {
                    rigidbody.AddExplosionForce(explosionForce, transform.position, explosionRadius, 5, ForceMode.Force);
                }
            }
        }      
        Destroy(this.gameObject);
    }
}
}