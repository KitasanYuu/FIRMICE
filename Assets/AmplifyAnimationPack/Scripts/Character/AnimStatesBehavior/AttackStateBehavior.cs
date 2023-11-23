// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;

namespace AmplifyAnimationPack
{
	public class AttackStateBehavior : StateMachineBehaviour
	{
		private CharacterClass charRef;
		private CharacterAnimatorBehavior animBeh;

		// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
		override public void OnStateEnter( Animator animator , AnimatorStateInfo stateInfo , int layerIndex )
		{
			if( charRef == null )
			{
				charRef = animator.gameObject.GetComponentInParent<CharacterClass>();
			}
			if( animBeh == null )
			{
				animBeh = animator.GetComponent<CharacterAnimatorBehavior>();
			}

			animator.SetInteger( "comboIndex" , animator.GetInteger( "comboIndex" ) + 1 );
			charRef.ChangeAttackAction( CharacterCombatActions.none );
			animBeh.Attack_HitboxOff();
			animator.SetBool( "canComboAttack" , false );
		}

	}

}

