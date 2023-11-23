// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>


namespace AmplifyAnimationPack
{

	public class InteractState : CharacterStateBase
	{
		public override void EnterState()
		{
			charRef.ChangeMovementType( CharacterMovementTypes.interact );


		}

		public override void ExitState()
		{
		}

		public override void UpdateState()
		{
		}
	}

}
