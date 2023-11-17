/// <summary>
/// This script belongs to cowsins™ as a part of the cowsins´ FPS Engine. All rights reserved. 
/// </summary>
using UnityEngine;
namespace cowsins {
/// <summary>
/// Ignore collisions with player
/// Use it if you need to, but you might find it handy regarding to bullet shells on the ground.
/// They will interact with the world but not the player, hence we won�t suffer from extra unnecessary friction when walking on top of them.
/// </summary>
public class IgnoreCollision : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {

        if (collision.transform.CompareTag("Player"))
        {
            Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
        }
    }

}
}