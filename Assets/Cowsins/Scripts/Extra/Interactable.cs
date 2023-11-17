/// <summary>
/// This script belongs to cowsins™ as a part of the cowsins´ FPS Engine. All rights reserved. 
/// </summary>
using UnityEngine;
namespace cowsins { 
/// <summary>
/// This component must be add to ANY interactable object in your environment.
/// However, you might want to check other examples such as DoorInteractable.cs or PickUps
/// They are different things, they do different things, but they both are INTERACTABLES and you can interact with both of them.
/// Then, how does that work? Their custom scripts both inherit from interactable.cs, which inherits from MonoBehaviour, so even though they do 
/// different things they are actually the same kind of thing.
/// Since Interact() is a virtual void you can override it on your custom script inheriting from this one
/// 
/// For any doubts check the documentation or contact the support.
/// 
/// </summary>
public class Interactable : MonoBehaviour
{
    protected Transform player;

    [HideInInspector] public bool interactable = false;

    [Tooltip("Text that will be displayed on the Interaction UI")]
    public string interactText;

    private void Start()
    {
        interactable = false;
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }
    /// <summary>
    /// Make sure to override this on your new custom class.
    /// If you still wanna call this method, make sure to write the following line:
    /// base.Interact();
    /// </summary>
    public virtual void Interact()
    {
        Debug.Log("Interacted with" + this.gameObject.name);
    }

    public virtual void Highlight()
    {

    }
}
}