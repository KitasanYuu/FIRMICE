// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>


namespace AmplifyAnimationPack
{

	public class BumpState : CharacterStateBase
	{
		public override void EnterState()
		{
			charRef.animBeh.anim.SetFloat( "Speed" , 0f );
			charRef.ChangeMovementType( CharacterMovementTypes.bump );

		}

		public override void ExitState()
		{
		}

		public override void UpdateState()
		{
		}
	}

}
