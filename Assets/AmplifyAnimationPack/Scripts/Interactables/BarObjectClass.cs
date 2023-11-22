// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;


namespace AmplifyAnimationPack
{
	public class BarObjectClass : MonoBehaviour, IInteractable
	{
		private CharacterClass charRef;

		public void Interact( CharacterClass _player )
		{
			_player.ChangeState<BarHangState>();
			_player.uiManager.InteractionText_Disable();
		}

		private void OnTriggerEnter( Collider other )
		{
			charRef = other.GetComponentInParent<CharacterClass>();
			if( charRef != null )
			{
				if( charRef.currMovementType != CharacterMovementTypes.barHang && charRef.currMovementType != CharacterMovementTypes.barWalk )
				{
					charRef.objToInteract = this;
					charRef.uiManager.InteractionText_Enable( "Press E to Climb Bar" );
				}
			}
		}

		private void OnTriggerExit( Collider other )
		{
			if( charRef != null )
			{
				if( charRef.currMovementType != CharacterMovementTypes.barHang && charRef.currMovementType != CharacterMovementTypes.barWalk )
				{
					charRef.objToInteract = null;
					charRef.uiManager.InteractionText_Disable();
				}
			}
		}
	}

}
