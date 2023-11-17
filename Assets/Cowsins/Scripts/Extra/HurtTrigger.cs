using UnityEngine;
namespace cowsins {
public class HurtTrigger : MonoBehaviour
{
    [SerializeField] private float damage; 
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) other.GetComponent<PlayerStats>().Damage(damage); 
    }
}
}