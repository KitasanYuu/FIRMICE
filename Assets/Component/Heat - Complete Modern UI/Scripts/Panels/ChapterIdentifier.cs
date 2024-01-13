using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Michsky.UI.Heat
{
    public class ChapterIdentifier : MonoBehaviour
    {
        [Header("Resources")]
        public Animator animator;
        [SerializeField] private RectTransform backgroundRect;
        public Image backgroundImage;
        public TextMeshProUGUI titleObject;
        public TextMeshProUGUI descriptionObject;
        public ButtonManager continueButton;
        public ButtonManager playButton;
        public ButtonManager replayButton;
        public GameObject completedIndicator;
        public GameObject unlockedIndicator;
        public GameObject lockedIndicator;

        [HideInInspector] public ChapterManager chapterManager;
        [HideInInspector] public bool isLocked;
        [HideInInspector] public bool isCurrent;

        public void UpdateBackgroundRect() 
        { 
            chapterManager.currentBackgroundRect = backgroundRect;
            chapterManager.DoStretch();
        }

        public void SetCurrent()
        {
            completedIndicator.SetActive(false);
            unlockedIndicator.SetActive(true);
            lockedIndicator.SetActive(false);

            continueButton.gameObject.SetActive(true);
            playButton.gameObject.SetActive(false);
            replayButton.gameObject.SetActive(true);

            isLocked = false;
            isCurrent = true;
            continueButton.isInteractable = true;
            replayButton.isInteractable = true;
        }

        public void SetLocked()
        {
            completedIndicator.SetActive(false);
            unlockedIndicator.SetActive(false);
            lockedIndicator.SetActive(true);

            continueButton.gameObject.SetActive(false);
            playButton.gameObject.SetActive(true);
            replayButton.gameObject.SetActive(false);

            isLocked = true;
            isCurrent = false;
            playButton.isInteractable = false;
        }

        public void SetUnlocked()
        {
            completedIndicator.SetActive(false);
            unlockedIndicator.SetActive(true);
            lockedIndicator.SetActive(false);

            continueButton.gameObject.SetActive(false);
            playButton.gameObject.SetActive(true);
            replayButton.gameObject.SetActive(false);

            isLocked = false;
            isCurrent = false;
            playButton.isInteractable = true;
        }

        public void SetCompleted()
        {
            completedIndicator.SetActive(true);
            unlockedIndicator.SetActive(false);
            lockedIndicator.SetActive(false);

            continueButton.gameObject.SetActive(false);
            playButton.gameObject.SetActive(false);
            replayButton.gameObject.SetActive(true);

            isLocked = false;
            isCurrent = false;
            replayButton.isInteractable = true;
        }
    }
}