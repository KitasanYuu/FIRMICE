// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;


namespace AmplifyAnimationPack
{
	public class HangIdleStateBehavior : StateMachineBehaviour
	{
		private CharacterClass charRef;

		private RaycastHit hangDetectionHit;
		private Vector3 newPos;

		// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
		override public void OnStateEnter( Animator animator , AnimatorStateInfo stateInfo , int layerIndex )
		{
			if( charRef == null )
			{
				charRef = animator.gameObject.GetComponentInParent<CharacterClass>();
			}

			charRef.canMove = true;

			// adjust character
			Physics.Raycast( new Ray( charRef.hangPosHelper.position , charRef.transf.forward ) , out hangDetectionHit , 2f );
			if( hangDetectionHit.collider != null && hangDetectionHit.collider.gameObject.GetComponent<WallPointClass>() )
			{
				charRef.AdjustRotation( Quaternion.LookRotation( hangDetectionHit.normal ) * Quaternion.AngleAxis( 180f , Vector3.up ) , 0.1f );
				newPos = hangDetectionHit.point + hangDetectionHit.normal * 0.3f;
				float valueToAdd = 0.18f - ( hangDetectionHit.collider.transform.lossyScale.y );
				charRef.AdjustPosition( new Vector3( newPos.x , hangDetectionHit.collider.transform.position.y - 0.94f - valueToAdd , newPos.z ) , 0.2f , false );
			}
			else
			{
				Debug.LogError( "Invalid WallPoint for character adjustment." );
			}

			// reset variables
			animator.SetFloat( "InputFwd" , 0f );
			charRef.ChangeAttackAction( CharacterCombatActions.none );

		}

	}
}