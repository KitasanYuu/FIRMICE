// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;


namespace AmplifyAnimationPack
{
	public class CoverMoveStateBehavior : StateMachineBehaviour
	{
		// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
		override public void OnStateEnter( Animator animator , AnimatorStateInfo stateInfo , int layerIndex )
		{
			animator.SetFloat( "CoverSide" , Mathf.Sign( animator.GetFloat( "InputRight" ) ) );

		}

	}
}
