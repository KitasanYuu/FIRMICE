// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;

namespace AmplifyAnimationPack
{

    public class DodgeState : CharacterStateBase
    {
		private Quaternion targetRotation;

		public override void EnterState()
		{
			charRef.ChangeMovementType( CharacterMovementTypes.dodge );
		}

		public override void ExitState()
		{
		}

		public override void UpdateState()
		{
			RotateCharToEnemy();
		}

		private void RotateCharToEnemy()
		{
			targetRotation = Quaternion.Euler(
				charRef.transf.eulerAngles.x ,
				Quaternion.LookRotation( charRef.lockOnTarget.transform.position - charRef.transf.position ).eulerAngles.y ,
				charRef.transf.rotation.eulerAngles.z );
			charRef.transf.rotation = Quaternion.RotateTowards( charRef.transf.rotation , targetRotation , 7f );
		}

	}
}
