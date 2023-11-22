// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;
using UnityEngine.UI;


namespace AmplifyAnimationPack
{

	public class CharacterUIBehavior : MonoBehaviour
	{
		[SerializeField]
		private Text interactionText;
		[SerializeField]
		private Text playgroundText;
		[SerializeField]
		private Text warningText;

		public void ToggleWarning()
		{
			warningText.enabled = ( Time.fixedDeltaTime != 0.0166667f );
		} 

		public void DisableCursor()
		{
			Cursor.lockState = CursorLockMode.Locked;
		}

		public void InteractionText_Enable( string _textToShow )
		{
			interactionText.gameObject.SetActive( true );
			interactionText.text = _textToShow;

		}

		public void InteractionText_Disable()
		{
			interactionText.gameObject.SetActive( false );
		}

		public void PlaygroundText_Enable()
		{
			playgroundText.gameObject.SetActive( true );

		}

		public void PlaygroundText_Disable()
		{
			playgroundText.gameObject.SetActive( false );
		}
	}
}