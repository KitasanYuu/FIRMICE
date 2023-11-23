// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;


namespace AmplifyAnimationPack
{

	public class BigClimbState : CharacterStateBase
	{
		private float timerToClimb;
		private RaycastHit detectionRayHit;

		public override void EnterState()
		{
			charRef.rigid.useGravity = false;
			charRef.baseColl.radius = 0.04f;

			charRef.animBeh.anim.SetFloat( "Speed" , 0f );

			charRef.AdjustRotation( Quaternion.AngleAxis( 180f , Vector3.up ) * Quaternion.LookRotation( charRef.objToInteractRotationVector ) , 0.2f );
			charRef.AdjustPosition( charRef.objToInteractContactPoint + charRef.objToInteractRotationVector * 0.51f , 0.2f , true );

			charRef.ChangeMovementType( CharacterMovementTypes.bigClimb );
			charRef.ChangeClimbingAction( CharacterClimbingActions.idle );

			timerToClimb = 0f;

		}

		public override void ExitState()
		{
		}

		public override void UpdateState()
		{
			timerToClimb += Time.deltaTime;

			if( charRef.currClimbingAction == CharacterClimbingActions.idle )
			{

				if( timerToClimb <= charRef.climbingTimeThreshold )
				{
					if( Physics.Raycast( new Ray( charRef.hangPosHelper.position , charRef.transf.forward ) , out detectionRayHit , 1f ) )
					{
						if( detectionRayHit.collider.gameObject.GetComponent<WallPointClass>() )
						{
							charRef.rigid.velocity = Vector3.zero;
							charRef.ChangeState<HangState>();
						}
					}
					else
					{
						charRef.ChangeClimbingAction( CharacterClimbingActions.release );
					}
				}
				else
				{
					charRef.ChangeClimbingAction( CharacterClimbingActions.release );
				}


			}
		}

		public override void InputJump( bool _actionVal )
		{
			if( _actionVal )
			{
				if( charRef.currClimbingAction == CharacterClimbingActions.idle )
				{
					charRef.ChangeClimbingAction( CharacterClimbingActions.release );
				}
			}
		}

	}

}
