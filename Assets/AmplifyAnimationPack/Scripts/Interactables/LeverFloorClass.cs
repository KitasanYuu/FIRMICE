// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;

namespace AmplifyAnimationPack
{
	public class LeverFloorClass : MonoBehaviour, IInteractable
	{
		private CharacterClass charRef;
		private bool isPulled;

		private void Start()
		{
			isPulled = false;
		}

		private void OnTriggerEnter( Collider other )
		{
			charRef = other.GetComponentInParent<CharacterClass>();
			if( charRef != null )
			{
				charRef.objToInteract = this;
				charRef.uiManager.InteractionText_Enable( "Press E to push lever" );
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

		public void Interact( CharacterClass _player )
		{
			if( charRef == null )
			{
				return;
			}

			charRef.ChangeInteractType( ( isPulled ) ? CharacterInteractionTypes.leverFloorPush : CharacterInteractionTypes.leverFloorPull );
			charRef.ChangeState<InteractState>();

			charRef.AdjustPosition( transform.Find( "PlayerPos" ).position , 0.4f , true );
			charRef.AdjustRotation( transform.Find( "PlayerPos" ).rotation , 0.4f );

			isPulled = !isPulled;
		}

		public void PushHandle()
		{
			transform.Find( "BaseMesh" ).GetChild( 0 ).GetComponent<Animator>().SetTrigger( "willPush" );
		}

		public void PullHandle()
		{
			transform.Find( "BaseMesh" ).GetChild( 0 ).GetComponent<Animator>().SetTrigger( "willPull" );
		}

	}
}