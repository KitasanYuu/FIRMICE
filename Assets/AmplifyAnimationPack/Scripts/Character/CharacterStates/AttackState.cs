// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>


namespace AmplifyAnimationPack
{

	public class AttackState : CharacterStateBase
	{
		public override void EnterState()
		{
			charRef.animBeh.anim.SetInteger( "comboIndex" , 0 );
			charRef.hitbox.SetActive( false );

		}

		public override void ExitState()
		{
		}

		public override void UpdateState()
		{
			charRef.CheckIsGrounded();
		}

		public override void InputAttack( bool _actionVal , CharacterCombatActions _newAttack )
		{
			if( _actionVal )
			{
				if( charRef.animBeh.anim.GetBool( "canComboAttack" ) )
				{
					charRef.ChangeAttackAction( _newAttack );
				}
			}


		}

	}
}
