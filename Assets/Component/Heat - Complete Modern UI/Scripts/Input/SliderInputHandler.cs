using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Michsky.UI.Heat
{
    public class SliderInputHandler : MonoBehaviour
    {
        [Header("Resources")]
        [SerializeField] private Slider sliderObject;
        [SerializeField] private GameObject indicator;

        [Header("Settings")]
        [Range(0.1f, 50)] public float valueMultiplier = 1;
        [SerializeField] [Range(0.01f, 1)] private float deadzone = 0.1f;
        [SerializeField] private bool optimizeUpdates = true;
        public bool requireSelecting = true;
        [SerializeField] private bool reversePosition;
        [SerializeField] private bool divideByMaxValue;

        // Helpers
        float divideValue = 1000;

        void OnEnable()
        {
            if (ControllerManager.instance == null || sliderObject == null) { Destroy(this); }
            if (indicator == null)
            {
                indicator = new GameObject();
                indicator.name = "[Generated Indicator]";
                indicator.transform.SetParent(transform);
            }

            indicator.SetActive(false);

            if (divideByMaxValue)
            {
                divideValue = sliderObject.maxValue;
            }
        }

        void Update()
        {
            if (Gamepad.current == null || ControllerManager.instance == null) { indicator.SetActive(false); return; }
            else if (requireSelecting && EventSystem.current.currentSelectedGameObject != gameObject) { indicator.SetActive(false); return; }
            else if (optimizeUpdates && ControllerManager.instance != null && !ControllerManager.instance.gamepadEnabled) { indicator.SetActive(false); return; }

            indicator.SetActive(true);

            if (reversePosition && ControllerManager.instance.hAxis >= deadzone) { sliderObject.value -= (valueMultiplier / divideValue) * ControllerManager.instance.hAxis; }
            else if (!reversePosition && ControllerManager.instance.hAxis >= deadzone) { sliderObject.value += (valueMultiplier / divideValue) * ControllerManager.instance.hAxis; }
            else if (reversePosition && ControllerManager.instance.hAxis <= -deadzone) { sliderObject.value += (valueMultiplier / divideValue) * Mathf.Abs(ControllerManager.instance.hAxis); }
            else if (!reversePosition && ControllerManager.instance.hAxis <= -deadzone) { sliderObject.value -= (valueMultiplier / divideValue) * Mathf.Abs(ControllerManager.instance.hAxis); }
        }
    }
}