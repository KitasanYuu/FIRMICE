using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
namespace cowsins {
public class ShowAndHide : MonoBehaviour
{
    [SerializeField] private GameObject panel;

    private bool input, holding;

    private void Awake() => holding = false; 
    private void Update()
    {
    #if ENABLE_INPUT_SYSTEM
        input = Keyboard.current.qKey.isPressed;
        if (!input) holding = false; 
    #endif

        if (input && !holding)
        {
            holding = true; 
            if (panel.activeSelf == false) panel.SetActive(true);
            else panel.SetActive(false); 
        }
    }
}
}