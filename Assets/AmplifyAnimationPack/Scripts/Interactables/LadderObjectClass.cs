// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;

namespace AmplifyAnimationPack
{
	public class LadderObjectClass : MonoBehaviour, IInteractable
	{
		private CharacterClass charRef;

		public void Interact( CharacterClass _player )
		{
			_player.uiManager.InteractionText_Disable();

			bool willEnterDown = ( _player.transf.position.y > transform.position.y );
			Transform currPlayerPos = willEnterDown ? transform.Find( "Mesh/PlayerPos_Up" ) : transform.Find( "Mesh/PlayerPos_Down" );
			_player.AdjustPosition( currPlayerPos.position , 0.2f , true );
			_player.AdjustRotation( currPlayerPos.rotation , 0.2f );
			_player.animBeh.anim.SetBool( "Ladder_WillEnterDown" , willEnterDown ) ;

			_player.ChangeState<LadderState>();

		}

		private void OnTriggerEnter( Collider other )
		{
			charRef = other.GetComponentInParent<CharacterClass>();
			if( charRef != null )
			{
				charRef.objToInteract = this;
				charRef.uiManager.InteractionText_Enable( "Press E to Climb ladder" );
			}
		}

		private void OnTriggerExit( Collider other )
		{

			if( charRef != null )
			{
				charRef.objToInteract = null;
				charRef.uiManager.InteractionText_Disable();
			}
		}

	}
}