// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>


namespace AmplifyAnimationPack
{
	public class VaultState : CharacterStateBase
	{
		public override void EnterState()
		{

			charRef.baseColl.enabled = false;
			charRef.rigid.useGravity = false;

			charRef.ChangeMovementType( CharacterMovementTypes.vault );
		}

		public override void ExitState() { }

		public override void UpdateState() { }
	}
}
