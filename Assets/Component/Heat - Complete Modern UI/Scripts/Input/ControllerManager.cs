using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;

namespace Michsky.UI.Heat
{
    [DefaultExecutionOrder(-100)]
    [DisallowMultipleComponent]
    public class ControllerManager : MonoBehaviour
    {
        // Static Instance
        public static ControllerManager instance;

        // Resources
        public ControllerPresetManager presetManager;
        public GameObject firstSelected;
        public List<PanelManager> panels = new List<PanelManager>();
        public List<ButtonManager> buttons = new List<ButtonManager>();
        public List<BoxButtonManager> boxButtons = new List<BoxButtonManager>();
        public List<ShopButtonManager> shopButtons = new List<ShopButtonManager>();
        public List<SettingsElement> settingsElements = new List<SettingsElement>();
        [Tooltip("Objects in this list will be enabled when the gamepad is un-plugged.")]
        public List<GameObject> keyboardObjects = new List<GameObject>();
        [Tooltip("Objects in this list will be enabled when the gamepad is plugged.")]
        public List<GameObject> gamepadObjects = new List<GameObject>();
        public List<HotkeyEvent> hotkeyObjects = new List<HotkeyEvent>();

        // Settings
        [Tooltip("Checks for input changes each frame.")]
        public bool alwaysUpdate = true;
        public bool affectCursor = true;
        public InputAction gamepadHotkey;

        // Helpers
        Vector3 cursorPos;
        Vector3 lastCursorPos;
        Navigation customNav = new Navigation();
        [HideInInspector] public int currentManagerIndex;

        [HideInInspector] public bool gamepadConnected;
        [HideInInspector] public bool gamepadEnabled;
        [HideInInspector] public bool keyboardEnabled;

        [HideInInspector] public float hAxis;
        [HideInInspector] public float vAxis;

        [HideInInspector] public string currentController;
        [HideInInspector] public ControllerPreset currentControllerPreset;

        void Awake()
        {
            instance = this;
        }

        void Start()
        {
            InitInput();
        }

        void Update()
        {
            if (!alwaysUpdate)
                return;

            CheckForController();
            CheckForEmptyObject();
        }

        void InitInput()
        {
            gamepadHotkey.Enable();

            if (Gamepad.current == null) { gamepadConnected = false; SwitchToKeyboard(); }
            else { gamepadConnected = true; SwitchToGamepad(); }
        }

        void CheckForEmptyObject()
        {
            if (!gamepadEnabled) { return; }
            else if (EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.gameObject.activeInHierarchy) { return; }

            if (gamepadHotkey.triggered && panels.Count != 0 && panels[currentManagerIndex].panels[panels[currentManagerIndex].currentPanelIndex].firstSelected != null)
            {
                SelectUIObject(panels[currentManagerIndex].panels[panels[currentManagerIndex].currentPanelIndex].firstSelected);
            }
        }

        public void CheckForController()
        {
            if (Gamepad.current == null) { gamepadConnected = false; }
            else
            {
                gamepadConnected = true;
                hAxis = Gamepad.current.rightStick.x.ReadValue();
                vAxis = Gamepad.current.rightStick.y.ReadValue();
            }

            cursorPos = Mouse.current.position.ReadValue();

            if (gamepadConnected && gamepadEnabled && !keyboardEnabled && cursorPos != lastCursorPos) { SwitchToKeyboard(); }
            else if (gamepadConnected && !gamepadEnabled && keyboardEnabled && gamepadHotkey.triggered) { SwitchToGamepad(); }
            else if (!gamepadConnected && !keyboardEnabled) { SwitchToKeyboard(); }
        }

        void CheckForCurrentObject()
        {
            if ((EventSystem.current.currentSelectedGameObject == null || !EventSystem.current.currentSelectedGameObject.activeInHierarchy) && panels.Count != 0)
            {
                SelectUIObject(panels[currentManagerIndex].panels[panels[currentManagerIndex].currentPanelIndex].firstSelected);
            }
        }

        public void SwitchToGamepad()
        {
            if (affectCursor) { Cursor.visible = false; }
          
            for (int i = 0; i < keyboardObjects.Count; i++) 
            {
                if (keyboardObjects[i] == null)
                    continue;

                keyboardObjects[i].SetActive(false);
            }

            for (int i = 0; i < gamepadObjects.Count; i++)
            {
                if (gamepadObjects[i] == null)
                    continue;

                gamepadObjects[i].SetActive(true);
                LayoutRebuilder.ForceRebuildLayoutImmediate(gamepadObjects[i].GetComponentInParent<RectTransform>());
            }

            customNav.mode = Navigation.Mode.Automatic;

            for (int i = 0; i < buttons.Count; i++) { if (buttons[i] != null && !buttons[i].useUINavigation) { buttons[i].AddUINavigation(); } }
            for (int i = 0; i < boxButtons.Count; i++) { if (boxButtons[i] != null && !boxButtons[i].useUINavigation) { boxButtons[i].AddUINavigation(); } }
            for (int i = 0; i < shopButtons.Count; i++) { if (shopButtons[i] != null && !shopButtons[i].useUINavigation) { shopButtons[i].AddUINavigation(); } }
            for (int i = 0; i < settingsElements.Count; i++) { if (settingsElements[i] != null && !settingsElements[i].useUINavigation) { settingsElements[i].AddUINavigation(); } }

            gamepadEnabled = true;
            keyboardEnabled = false;
            lastCursorPos = Mouse.current.position.ReadValue();

            CheckForGamepadType();
            CheckForCurrentObject();
        }

        void CheckForGamepadType()
        {
            currentController = Gamepad.current.displayName;

            // Search for main and custom gameapds
            if (Gamepad.current is XInputController && presetManager != null && presetManager.xboxPreset != null) { currentControllerPreset = presetManager.xboxPreset; }
#if !UNITY_WEBGL && !UNITY_IOS && !UNITY_ANDROID && !UNITY_STANDALONE_LINUX
            else if (Gamepad.current is DualSenseGamepadHID && presetManager != null && presetManager.dualsensePreset != null) { currentControllerPreset = presetManager.dualsensePreset; }
#endif
            else
            {
                for (int i = 0; i < presetManager.customPresets.Count; i++)
                {
                    if (currentController == presetManager.customPresets[i].controllerName)
                    {
                        currentControllerPreset = presetManager.customPresets[i];
                        break;
                    }
                }
            }

            foreach (HotkeyEvent he in hotkeyObjects) 
            {
                if (he == null)
                    continue;

                he.controllerPreset = currentControllerPreset;
                he.UpdateUI();
            }
        }

        public void SwitchToKeyboard()
        {
            if (affectCursor) { Cursor.visible = true; }
            if (presetManager != null && presetManager.keyboardPreset != null) 
            {
                currentControllerPreset = presetManager.keyboardPreset;
             
                foreach (HotkeyEvent he in hotkeyObjects)
                {
                    if (he == null)
                        continue;

                    he.controllerPreset = currentControllerPreset;
                    he.UpdateUI();
                }
            }

            for (int i = 0; i < gamepadObjects.Count; i++) 
            {
                if (gamepadObjects[i] == null)
                    continue;

                gamepadObjects[i].SetActive(false);
            }

            for (int i = 0; i < keyboardObjects.Count; i++)
            {
                if (keyboardObjects[i] == null)
                    continue;

                keyboardObjects[i].SetActive(true);
                LayoutRebuilder.ForceRebuildLayoutImmediate(keyboardObjects[i].GetComponentInParent<RectTransform>());
            }

            customNav.mode = Navigation.Mode.None;

            for (int i = 0; i < buttons.Count; i++) { if (buttons[i] != null && !buttons[i].useUINavigation) { buttons[i].DisableUINavigation(); } }
            for (int i = 0; i < boxButtons.Count; i++) { if (boxButtons[i] != null && !boxButtons[i].useUINavigation) { boxButtons[i].DisableUINavigation(); } }
            for (int i = 0; i < shopButtons.Count; i++) { if (shopButtons[i] != null && !shopButtons[i].useUINavigation) { shopButtons[i].DisableUINavigation(); } }
            for (int i = 0; i < settingsElements.Count; i++) { if (settingsElements[i] != null && !settingsElements[i].useUINavigation) { settingsElements[i].DisableUINavigation(); } }

            gamepadEnabled = false;
            keyboardEnabled = true;
        }

        public void SelectUIObject(GameObject tempObj)
        {
            if (!gamepadEnabled)
                return;

            EventSystem.current.SetSelectedGameObject(tempObj.gameObject);
        }
    }
}