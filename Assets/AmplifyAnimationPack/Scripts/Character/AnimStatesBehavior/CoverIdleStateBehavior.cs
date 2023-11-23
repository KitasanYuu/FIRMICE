// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;


namespace AmplifyAnimationPack
{
	public class CoverIdleStateBehavior : StateMachineBehaviour
	{
		private CharacterClass charRef;
		private RaycastHit backToWallHit;

		// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
		override public void OnStateEnter( Animator animator , AnimatorStateInfo stateInfo , int layerIndex )
		{
			if( charRef == null )
			{
				charRef = animator.gameObject.GetComponentInParent<CharacterClass>();
			}

			charRef.canMove = true;

			if( Physics.Raycast( new Ray( charRef.transf.position , -charRef.transf.forward ) , out backToWallHit , 1f ) )
			{
				charRef.AdjustRotation( Quaternion.LookRotation( backToWallHit.normal ) , 0.1f );
				charRef.AdjustPosition( backToWallHit.point + backToWallHit.normal * 0.2f , 0.1f , true );
			}
		}

	}
}
