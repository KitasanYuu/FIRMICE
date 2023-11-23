// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;

namespace AmplifyAnimationPack
{
	public class ClimbObjectClass : MonoBehaviour, IInteractable
	{

		public void Interact( CharacterClass _player )
		{
			_player.uiManager.InteractionText_Disable();

			if( transform.localScale.y <= 1f )
			{
				_player.ChangeState<ShortClimbState>();
			}
			else
			{
				_player.ChangeState<BigClimbState>();
			}

		}
	}
}