// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;

namespace AmplifyAnimationPack
{

	public class PushObjectClass : MonoBehaviour, IInteractable
	{
		public void Interact( CharacterClass _player )
		{
			_player.uiManager.InteractionText_Disable();

			_player.ChangeState<PushState>();
		}

	}
}
