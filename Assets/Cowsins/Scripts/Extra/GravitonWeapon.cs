using UnityEngine;
namespace cowsins {
public class GravitonWeapon : MonoBehaviour
{
    [SerializeField] private Transform playerCamera;

    [SerializeField] private float maxDistance;

    [SerializeField] private LayerMask hitLayer;

    [SerializeField] private float lerpSpeed;

    RaycastHit obj; 
    public void Graviton()
    {
        RaycastHit hit; 
        Ray ray = new Ray(playerCamera.position, playerCamera.forward); 
        if(Physics.Raycast(ray, out hit,maxDistance, hitLayer)) obj = hit;
        if (obj.transform == null ||obj.transform!= null && Vector3.Distance(obj.transform.position,transform.position) > maxDistance * 2) return; 
        obj.transform.position = Vector3.Lerp(obj.transform.position, ray.GetPoint(maxDistance), lerpSpeed * Time.deltaTime);
    }
}
}