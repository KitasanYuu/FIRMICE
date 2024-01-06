using System.Collections.Generic;
using UnityEngine;

namespace Michsky.LSS.Demo
{
    public class PresetButtonManager : MonoBehaviour
    {
        private int selectedButtonIndex = 0;
        private List<PresetButton> buttons = new List<PresetButton>();

        void Start()
        {
            foreach (Transform btn in transform) { buttons.Add(btn.gameObject.GetComponent<PresetButton>()); }
            for (int i = 0; i < buttons.Count; ++i) 
            {
                buttons[i].pbManager = this;
                buttons[i].index = i; 
            }

            SetSelectedButton(selectedButtonIndex);
            buttons[selectedButtonIndex].SetPresetToManager();
        }

        public void SetSelectedButton(int index)
        {
            selectedButtonIndex = index;
            // buttons[selectedButtonIndex].SetSelected(true);

            for (int i = 0; i < buttons.Count; ++i)
            {
                if (i == selectedButtonIndex)
                    continue;

                buttons[i].SetSelected(false);
            }
        }
    }
}