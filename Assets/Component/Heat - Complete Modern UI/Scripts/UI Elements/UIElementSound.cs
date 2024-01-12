using UnityEngine;
using UnityEngine.EventSystems;

namespace Michsky.UI.Heat
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Heat UI/Audio/UI Element Sound")]
    public class UIElementSound : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
    {
        [Header("Resources")]
        public AudioSource audioSource;

        [Header("Custom SFX")]
        public AudioClip hoverSFX;
        public AudioClip clickSFX;

        [Header("Settings")]
        public bool enableHoverSound = true;
        public bool enableClickSound = true;

        void OnEnable()
        {
            if (UIManagerAudio.instance != null && audioSource == null)
            {
                audioSource = UIManagerAudio.instance.audioSource;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (enableHoverSound)
            {
                if (hoverSFX == null) { audioSource.PlayOneShot(UIManagerAudio.instance.UIManagerAsset.hoverSound); }
                else { audioSource.PlayOneShot(hoverSFX); }
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (enableClickSound)
            {
                if (clickSFX == null) { audioSource.PlayOneShot(UIManagerAudio.instance.UIManagerAsset.clickSound); }
                else { audioSource.PlayOneShot(clickSFX); }
            }
        }
    }
}