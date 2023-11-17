using UnityEngine;
namespace cowsins {
public class BulletUIEffect : MonoBehaviour
{
    [SerializeField] private float size,lerpSpeed;

    private void Update() => transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(1, 1, 1), Time.deltaTime * lerpSpeed); 

    public void Shoot() => transform.localScale = new Vector3(size, size, size); 
}
}