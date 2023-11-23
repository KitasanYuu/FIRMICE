// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;


namespace AmplifyAnimationPack
{

	public class WallPointClass : MonoBehaviour, IInteractable
	{
		[SerializeField]
		private Transform positionMarker;

		public void Interact( CharacterClass _player )
		{

			_player.uiManager.InteractionText_Disable();
			_player.objToInteract = null;

			_player.rigid.useGravity = false;
			_player.baseColl.enabled = false;

			_player.AdjustRotation( transform.rotation * Quaternion.AngleAxis( 180f , transform.up ) , 0.1f );

			Vector3 target = Vector3.Cross( transform.forward , transform.up );
			float rightSign = -Mathf.Sign( Vector3.Dot( _player.transf.position - transform.position , target ) );
			float dist = Vector3.Project( _player.transf.position - transform.position , target ).magnitude;
			_player.AdjustPosition( positionMarker.position - transform.forward * 0.45f + transform.right * dist * rightSign , 0.1f , true );

			_player.ChangeState<HangState>();
			_player.ChangeMovementMode( CharacterMovementModes.animationBased );
			_player.ChangeMovementType( CharacterMovementTypes.climbDown );
			_player.canMove = false;
		}

	}
}