// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;


namespace AmplifyAnimationPack
{

	public class BarWalkState : CharacterStateBase
	{
		private Transform barTransf;
		private RaycastHit barRaycastHit;

		// input vars
		private float inputUp;
		private float inputRight;
		private float movementAxisInputs;
		private Vector3 targetDirection;

		public override void EnterState()
		{
			if( DetectBar() )
			{
				barTransf = barRaycastHit.collider.gameObject.transform;
			}
			else
			{
				Debug.LogError( "INVALID BAR" );
				charRef.ChangeState<FallingState>();
				return;
			}

			charRef.rigid.useGravity = false;
			charRef.baseColl.enabled = true;
			charRef.canMove = true;

			charRef.uiManager.InteractionText_Disable();
			charRef.ChangeCollider( CharacterColliderTypes.crouchCol );
			charRef.ChangeMovementType( CharacterMovementTypes.barWalk );
			charRef.ChangeBarWalkAction( CharacterBarWalkActions.idle );

			float charDirRelativeToBar = Mathf.Sign( Vector3.Dot( charRef.transf.forward , barTransf.transform.forward ) );
			charRef.AdjustRotation( ( charDirRelativeToBar == 1f ) ? barTransf.rotation : barTransf.rotation * Quaternion.AngleAxis( 180f , Vector3.up ) , 0.2f );
			if( charRef.currBarHangAction != CharacterBarHangActions.climb )
			{
				float dist = Vector3.Distance( charRef.transf.position , barTransf.position );
				charRef.AdjustPosition( barTransf.position + barTransf.forward * ( dist - 0.1f ) * -charDirRelativeToBar + barTransf.up * 0.9f , 0.05f , false );
				charRef.ChangeBarHangAction( CharacterBarHangActions.idle );
			}
		}

		public override void ExitState()
		{
		}

		public override void UpdateState()
		{

			if( charRef.currBarWalkAction == CharacterBarWalkActions.idle )
			{

				// calculates speed
				movementAxisInputs = Mathf.Abs( inputRight ) + Mathf.Abs( inputUp );
				movementAxisInputs = Mathf.Clamp( movementAxisInputs , 0f , 1f );
				charRef.animBeh.anim.SetFloat( "Speed" , movementAxisInputs );

				if( movementAxisInputs != 0f )
				{
					// set target values
					targetDirection = charRef.GetFollowCamTransform().GetChild( 0 ).forward * inputUp + charRef.GetFollowCamTransform().GetChild( 0 ).right * inputRight;

					// update animator values
					charRef.animBeh.anim.SetFloat( "TargetDir" ,
						Vector3.Angle( new Vector3( charRef.transf.forward.x , 0f , charRef.transf.forward.z ) , new Vector3( targetDirection.x , 0f , targetDirection.z ) ) );
				}

				if( DetectBar() )
				{
					if( !barRaycastHit.collider.gameObject.name.Contains( "BarObject" ) )
					{
						charRef.objToInteract = null;
						charRef.ChangeState<GroundedState>();
					}
				}
				else
				{
					charRef.objToInteract = null;
					charRef.ChangeState<FallingState>();
				}
			}

		}

		public override void InputMoveForward( float _axisVal )
		{
			inputUp = _axisVal;
		}

		public override void InputMoveRight( float _axisVal )
		{
			inputRight = _axisVal;
		}

		public override void InputInteract( bool _actionVal )
		{
			if( _actionVal )
			{
				if( charRef.canMove )
				{
					if( charRef.currBarWalkAction == CharacterBarWalkActions.idle )
					{
						charRef.ChangeBarHangAction( CharacterBarHangActions.idle );

						charRef.AdjustPosition( charRef.transf.position - charRef.transf.right * 0.2f , 0.3f , true );
						charRef.baseColl.enabled = false;
						charRef.ChangeBarWalkAction( CharacterBarWalkActions.drop );
					}
				}

			}
		}

		private bool DetectBar()
		{
			return Physics.SphereCast( charRef.transf.position , 0.4f , -Vector3.up , out barRaycastHit , 0.6f );


		}

	}
}
