/// <summary>
/// This script belongs to cowsins™ as a part of the cowsins´ FPS Engine. All rights reserved. 
/// </summary>
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Events;
namespace cowsins {
public class Pickeable : Interactable
{
    [System.Serializable]
    public class Events
    {
        public UnityEvent OnPickUp;
    }
    [SerializeField] private Events events;

    [Tooltip("Apply the selected effect")]
    public bool rotates, translates;

    [Tooltip("Change the speed of the selected effect"), SerializeField]
    private float rotationSpeed, translationSpeed;

    [SerializeField] protected Image image;

    [Tooltip("Transform under which the graphics will be stored at when instantiated"), SerializeField]
    protected Transform graphics;

    [HideInInspector] public bool dropped;

    [HideInInspector] protected bool pickeable;

    private Transform obj;

    private float timer = 0f;


    public virtual void Start()
    {
        pickeable = false;
        obj = transform.Find("Graphics");
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }
    private void Update() => Movement();

    public override void Interact() => events.OnPickUp.Invoke();

    /// <summary>
    /// Apply effects, usually for more cartoony, stylized, anime approaches
    /// </summary>
    private void Movement()
    {
        if (!rotates && !translates) return;
        if (rotates) obj.Rotate(Vector3.up * rotationSpeed * Time.deltaTime); // Rotate over time
        if (translates) // Go up and down
        {
            timer += Time.deltaTime * translationSpeed; // Timer that controls the movement
            float translateMotion = Mathf.Sin(timer) / 7000f;
            obj.transform.localPosition = new Vector3(obj.transform.localPosition.x, obj.transform.localPosition.y + translateMotion, obj.transform.localPosition.z);
        }
    }

    public virtual void Drop(WeaponController wcon, Transform orientation)
    {
        dropped = true;
        GetComponent<Rigidbody>().AddForce(orientation.forward * 4, ForceMode.Impulse);
        GetComponent<Rigidbody>().AddForce(orientation.up * -2, ForceMode.Impulse);
        float random = Random.Range(-1f, 1f);
        GetComponent<Rigidbody>().AddTorque(new Vector3(random, random, random) * 10);
    }

}
}