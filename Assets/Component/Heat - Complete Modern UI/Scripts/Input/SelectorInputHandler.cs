using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Michsky.UI.Heat
{
    public class SelectorInputHandler : MonoBehaviour
    {
        [Header("Resources")]
        [SerializeField] private HorizontalSelector selectorObject;
        [SerializeField] private GameObject indicator;

        [Header("Settings")]
        public float selectorCooldown = 0.4f;
        [SerializeField] private bool optimizeUpdates = true;
        public bool requireSelecting = true;

        // Helpers
        bool isInCooldown = false;

        void OnEnable()
        {
            if (ControllerManager.instance == null || selectorObject == null) { Destroy(this); }
            if (indicator == null)
            {
                indicator = new GameObject();
                indicator.name = "[Generated Indicator]";
                indicator.transform.SetParent(transform);
            }

            indicator.SetActive(false);
        }

        void Update()
        {
            if (Gamepad.current == null || ControllerManager.instance == null) { indicator.SetActive(false); return; }
            else if (requireSelecting && EventSystem.current.currentSelectedGameObject != gameObject) { indicator.SetActive(false); return; }
            else if (optimizeUpdates && ControllerManager.instance != null && !ControllerManager.instance.gamepadEnabled) { indicator.SetActive(false); return; }
            else if (isInCooldown) { return; }

            indicator.SetActive(true);

            if (ControllerManager.instance.hAxis >= 0.75)
            {
                selectorObject.NextItem();
                isInCooldown = true;

                StopCoroutine("CooldownTimer");
                StartCoroutine("CooldownTimer");
            }

            else if (ControllerManager.instance.hAxis <= -0.75)
            {
                selectorObject.PreviousItem();
                isInCooldown = true;

                StopCoroutine("CooldownTimer");
                StartCoroutine("CooldownTimer");
            }
        }

        IEnumerator CooldownTimer()
        {
            yield return new WaitForSecondsRealtime(selectorCooldown);
            isInCooldown = false;
        }
    }
}