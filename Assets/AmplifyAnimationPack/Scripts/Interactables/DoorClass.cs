// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;

namespace AmplifyAnimationPack
{
	public class DoorClass : MonoBehaviour, IInteractable
	{
		private Collider doorCol;

		private CharacterClass charRef;
		private bool isOpen;

		private void Start()
		{
			isOpen = false;
			doorCol = GetComponent<BoxCollider>();
			doorCol.enabled = true;

		}

		private void OnTriggerEnter( Collider other )
		{
			charRef = other.GetComponentInParent<CharacterClass>();
			if( charRef != null )
			{
				charRef.objToInteract = this;
				charRef.uiManager.InteractionText_Enable( "Press E to Open/Close door" );
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
			bool isInFront = ( Mathf.Sign( Vector3.Dot( transform.forward , _player.transf.position - transform.position ) ) < 0f );
			_player.ChangeInteractType( ( isOpen ) ?
				( ( isInFront ) ? CharacterInteractionTypes.doorCloseOutside : CharacterInteractionTypes.doorCloseInside ) :
				( isInFront ) ? CharacterInteractionTypes.doorOpenOutside : CharacterInteractionTypes.doorOpenInside );
			_player.ChangeState<InteractState>();

			Transform playerPosMarker = ( isInFront ) ? transform.Find( "PlayerPosOutside" ) : transform.Find( "PlayerPosInside" );
			_player.AdjustPosition( playerPosMarker.position , 0.4f , true );
			_player.AdjustRotation( playerPosMarker.rotation , 0.4f );

			isOpen = !isOpen;
			doorCol.enabled = !isOpen;

		}

		public void OpenDoor()
		{
			transform.Find( "Mesh" ).GetComponent<Animator>().SetTrigger( "willOpen" );
		}

		public void CloseDoor()
		{
			transform.Find( "Mesh" ).GetComponent<Animator>().SetTrigger( "willClose" );
		}

	}
}