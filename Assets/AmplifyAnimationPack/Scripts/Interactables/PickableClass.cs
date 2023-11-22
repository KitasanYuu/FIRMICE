// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;

namespace AmplifyAnimationPack
{
	public class PickableClass : MonoBehaviour, IInteractable
	{
		private Rigidbody rigid;
		private CharacterClass charRef;

		private void Awake()
		{
			rigid = GetComponent<Rigidbody>();
		}

		private void OnTriggerEnter( Collider other )
		{
			charRef = other.GetComponentInParent<CharacterClass>();
			if( charRef != null )
			{
				// check to see if the ball is on the floor/catchable
				if( Mathf.Abs( rigid.velocity.y ) < 0.1f )
				{
					charRef.objToInteract = this;
					charRef.uiManager.InteractionText_Enable( "Press E to pick object" );
				}
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

			charRef.uiManager.InteractionText_Disable();

			Vector3 perpToObject = Vector3.Cross( charRef.transf.forward , charRef.transf.position - transform.position );
			float dirToObject = Vector3.Dot( perpToObject , Vector3.up );

			charRef.ChangeInteractType( ( charRef.transf.position.y - transform.position.y < 0.5f ) ? CharacterInteractionTypes.grab :
				( dirToObject > 0f ) ? CharacterInteractionTypes.pickLeft : CharacterInteractionTypes.pickRight );
			charRef.ChangeState<InteractState>();

			charRef.objectCaught = gameObject;
		}

		public void PickObject()
		{
			gameObject.SetActive( false );
			charRef.objToInteract = null;

			gameObject.transform.Find( "Mesh" ).GetComponent<SphereCollider>().enabled = true;

		}

	}
}