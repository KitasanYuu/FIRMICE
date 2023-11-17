#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine;
using TMPro;
namespace cowsins {
public class DisplayKey : MonoBehaviour
{
    public static PlayerActions inputActions;

    private void Awake()
    {
        if (inputActions == null)
        {
            inputActions = new PlayerActions();
            inputActions.Enable();
        }

    }

    private void Update() => Repaint();

    public void Repaint()
    {
        TextMeshProUGUI txt = GetComponent<TextMeshProUGUI>();
        string device = DeviceDetection.Instance.mode == DeviceDetection.InputMode.Keyboard ? "Keyboard" : "Controller";
        txt.text = inputActions.GameControls.Interacting.GetBindingDisplayString(InputBinding.MaskByGroup(device));
    }
}
}