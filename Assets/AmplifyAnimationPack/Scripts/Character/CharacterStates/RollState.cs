// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>


namespace AmplifyAnimationPack
{
	public class RollState : CharacterStateBase
	{
		public override void EnterState()
		{
			charRef.canMove = false;
			charRef.canFallOffRoll = false;

			charRef.ChangeMovementType( CharacterMovementTypes.roll );
		}

		public override void ExitState()
		{
		}

		public override void UpdateState()
		{
			if( charRef.canFallOffRoll )
			{
				charRef.CheckIsGrounded();
			}
		}

	}
}
