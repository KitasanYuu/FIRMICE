using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTest : MonoBehaviour
{
    private Rigidbody bulletRigdbody;

    public float speed;

    private void Awake()
    {
        bulletRigdbody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        bulletRigdbody.velocity = transform.forward * speed;
    }

    // Update is called once per frame
    private void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }

    public void SetBulletSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

}
