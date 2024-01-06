using UnityEngine;

namespace Michsky.LSS.Demo
{
    public class PresetButton : MonoBehaviour
    {
        [Header("Resources")]
        [SerializeField] private LSS_Manager targetManager;
        [SerializeField] private GameObject selectedState;
        [HideInInspector] public PresetButtonManager pbManager;

        [Header("Settings")]
        [SerializeField] private string presetName = "Default";
        [HideInInspector] public int index;

        public void SetSelected(bool value)
        {
            selectedState.SetActive(value);

            if (value == true)
            {
                pbManager.SetSelectedButton(index);
                targetManager.presetName = presetName;
            }
        }

        public void SetPresetToManager()
        {
            targetManager.presetName = presetName;
        }
    }
}