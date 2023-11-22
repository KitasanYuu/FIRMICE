// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>


namespace AmplifyAnimationPack
{
	public class ShortClimbState : CharacterStateBase
	{
		public override void EnterState()
		{
			charRef.rigid.useGravity = false;
			charRef.baseColl.radius = 0.02f;

			charRef.animBeh.anim.SetFloat( "Speed" , 0f );

			charRef.ChangeMovementType( CharacterMovementTypes.shortClimb );

		}

		public override void ExitState()
		{
		}

		public override void UpdateState()
		{
		}

	}
}
