// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;


namespace AmplifyAnimationPack
{
	public class IdleStateBehavior : StateMachineBehaviour
	{
		private CharacterClass charRef;


		//OnStateEnter is called before OnStateEnter is called on any state inside this state machine
		override public void OnStateEnter( Animator animator , AnimatorStateInfo stateInfo , int layerIndex )
		{
			if( charRef == null )
			{
				charRef = animator.transform.parent.GetComponent<CharacterClass>();
			}

			charRef.ChangeState<GroundedState>();
		}

	}
}
