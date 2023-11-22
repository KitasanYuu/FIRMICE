// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;


namespace AmplifyAnimationPack
{
	public class HardTurnStateBehavior : StateMachineBehaviour
	{
		private CharacterAnimatorBehavior animBeh;

		// OnStateEnter is called before OnStateEnter is called on any state inside this state machine
		override public void OnStateEnter( Animator animator , AnimatorStateInfo stateInfo , int layerIndex )
		{
			if( animBeh == null )
			{
				animBeh = animator.GetComponent<CharacterAnimatorBehavior>();
			}

			animBeh.SetCanMove( false );
		}


		// OnStateExit is called before OnStateExit is called on any state inside this state machine
		override public void OnStateExit( Animator animator , AnimatorStateInfo stateInfo , int layerIndex )
		{
			animBeh.SetCanMove( true );

		}

	}
}