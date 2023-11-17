using UnityEngine;
using UnityEngine.InputSystem;
namespace cowsins {
public class DeviceDetection : MonoBehaviour
{
    public enum InputMode
    {
        Keyboard, Controller
    }
    public InputMode mode { get; private set; }

    public static DeviceDetection Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Update() => DetectInputs(); 

    public void DetectInputs()
    {
        bool KeyboardInputReceived = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
        bool controllerInputReceived = Gamepad.current != null && Gamepad.current.IsPressed(); 

        if (controllerInputReceived) mode = InputMode.Controller;
        else if (KeyboardInputReceived)  mode = InputMode.Keyboard;
    }
}
}