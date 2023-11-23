// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;


namespace AmplifyAnimationPack
{
	public class CoverState : CharacterStateBase
	{
		// input variables
		private float inputRight;
		private float inputFwd;
		private bool inputCrouch;

		private bool mustCrouch;

		private RaycastHit detectRayHit;

		public override void EnterState()
		{
			// setup components
			charRef.ChangeCollider( CharacterColliderTypes.baseCol );
			charRef.baseColl.radius = 0.01f;
			charRef.rigid.interpolation = RigidbodyInterpolation.None;
			charRef.rigid.velocity = Vector3.zero;
			charRef.rigid.useGravity = true;
			charRef.hitbox.SetActive( false );

			charRef.canMove = false;
			mustCrouch = !CoverStandingDetect( 1f );

			charRef.AdjustRotation( Quaternion.AngleAxis( 180f , Vector3.up ) * Quaternion.LookRotation( charRef.objToInteractRotationVector ) , 0f );
			charRef.AdjustPosition( charRef.objToInteractContactPoint + charRef.objToInteractRotationVector * (
				( charRef.currMovementType == CharacterMovementTypes.crouch ) ? 0.5f : 0.3f ) , 0.2f , true );

			charRef.ChangeMovementMode( CharacterMovementModes.animationBased );
			charRef.ChangeCoverTransition( ( mustCrouch && charRef.currMovementType != CharacterMovementTypes.crouch ) ? CharacterCoverTransitions.idleToCrouch : CharacterCoverTransitions.idleToIdle );
			charRef.ChangeMovementType( ( charRef.currMovementType == CharacterMovementTypes.crouch || mustCrouch ) ? CharacterMovementTypes.coverCrouch : CharacterMovementTypes.cover );

			charRef.animBeh.anim.SetFloat( "CoverSide" , -1f );

			charRef.targetHeadMarkerPos = charRef.coverHeadMarkerPos;

		}

		public override void ExitState()
		{

		}

		public override void UpdateState()
		{

			charRef.cameraTargetOffset = ( charRef.currCoverAction == CharacterCoverActions.sneak ) ? Vector3.right * charRef.coverSneakCamOffset * Mathf.Sign( inputRight ) : Vector3.zero;

			if( charRef.canMove )
			{
				mustCrouch = !CoverStandingDetect( -1f );

				if( mustCrouch )
				{
					ForceCrouch();
				}

			}

		}

		public override void InputMoveRight( float _axisVal )
		{
			inputRight = _axisVal;
			charRef.animBeh.anim.SetFloat( "InputRight" , _axisVal );

			if( charRef.canMove )
			{
				if( _axisVal != 0f )
				{
					if( CoverCornerDetect( inputRight , 1f ) )
					{
						charRef.canMove = false;
						charRef.ChangeCoverAction( CharacterCoverActions.cornerInside );
					}
					else
					{
						if( CoverMovementDetect( _axisVal ) )
						{
							charRef.ChangeCoverAction( CharacterCoverActions.move );

						}
						else if( CoverSneakDetect( _axisVal ) )
						{
							charRef.ChangeCoverAction( CharacterCoverActions.sneak );
						}
						else
						{
							charRef.ChangeCoverAction( CharacterCoverActions.idle );
						}
					}
				}
				else
				{
					if( !( inputFwd != 0f && charRef.currCoverAction == CharacterCoverActions.sneakUp ) )
					{
						charRef.ChangeCoverAction( CharacterCoverActions.idle );
					}
				}
			}
		}

		public override void InputMoveForward( float _axisVal )
		{

			inputFwd = _axisVal;

			if( charRef.canMove )
			{
				if( _axisVal > 0f && charRef.currMovementType == CharacterMovementTypes.coverCrouch && mustCrouch )
				{
					charRef.ChangeCoverAction( CharacterCoverActions.sneakUp );
				}
				else if( charRef.currCoverAction == CharacterCoverActions.sneakUp )
				{
					charRef.ChangeCoverAction( CharacterCoverActions.idle );
				}
			}
		}

		public override void InputAttack( bool _actionVal , CharacterCombatActions _newAttack )
		{
			if( _actionVal )
			{
				if( !charRef.objectCaught )
				{
					return;
				}

				if( !CoverMovementDetect( charRef.animBeh.anim.GetFloat( "CoverSide" ) ) )
				{
					charRef.ChangeCoverAction( CharacterCoverActions.throwObject );
				}
				else if( mustCrouch )
				{
					charRef.ChangeCoverAction( CharacterCoverActions.throwObjectUp );
				}
			}
		}

		public override void InputJump( bool _actionVal )
		{
			if( _actionVal )
			{
				if( charRef.currCoverAction == CharacterCoverActions.sneak )
				{
					if( CoverCornerDetect( inputRight , -1f ) )
					{
						charRef.canMove = false;
						charRef.ChangeCoverAction( CharacterCoverActions.cornerOutside );
					}
				}
			}
		}

		public override void InputInteract( bool _actionVal )
		{
			if( _actionVal )
			{
				if( charRef.canMove )
				{
					charRef.ChangeCoverTransition( CharacterCoverTransitions.idleToIdle );
					if( charRef.currMovementType == CharacterMovementTypes.coverCrouch )
					{
						if( inputCrouch )
						{
							charRef.ChangeCoverTransition( CharacterCoverTransitions.crouchToCrouch );
						}
						else
						{
							charRef.ChangeCoverTransition( CharacterCoverTransitions.crouchToIdle );
						}
					}

					charRef.canMove = false;
					charRef.ChangeCoverAction( CharacterCoverActions.exit );
				}
			}
		}

		public override void InputCrouch( bool _actionVal )
		{
			inputCrouch = _actionVal;

			if( mustCrouch )
			{
				return;
			}


			if( ( _actionVal && charRef.currMovementType == CharacterMovementTypes.cover ) ||
				( !_actionVal && charRef.currMovementType == CharacterMovementTypes.coverCrouch ) )
			{
				if( charRef.canMove )
				{
					charRef.ChangeMovementType( ( _actionVal ) ? CharacterMovementTypes.coverCrouch : CharacterMovementTypes.cover );

					if( inputRight == 0f )
					{
						charRef.canMove = false;
						charRef.ChangeCoverAction( CharacterCoverActions.stanceTransition );
					}
				}
			}
		}

		private bool CoverMovementDetect( float _axisVal )
		{
			if( Physics.Raycast( new Ray( charRef.transf.position + charRef.transf.right * -Mathf.Sign( _axisVal ) * 0.4f , -charRef.transf.forward ) , out detectRayHit , 1f ) )
			{
				return detectRayHit.collider.GetComponent<CoverObjectClass>();
			}
			else
			{
				return false;
			}

		}

		private bool CoverSneakDetect( float _axisVal )
		{
			return !Physics.Raycast( new Ray( charRef.transf.position + charRef.transf.right * -Mathf.Sign( _axisVal ) * 0.4f , -charRef.transf.forward ) , out detectRayHit , 0.5f );
		}

		private bool CoverCornerDetect( float _axisVal , float _frontSign )
		{
			if( Physics.Raycast(
				new Ray( charRef.transf.position + charRef.transf.right * ( _frontSign > 0 ? 0f : -Mathf.Sign( _axisVal ) ) + charRef.transf.forward * Mathf.Sign( _frontSign ) ,
				charRef.transf.right * Mathf.Sign( _axisVal ) * -_frontSign ) , out detectRayHit , 1f ) )
			{
				return detectRayHit.collider.GetComponent<CoverObjectClass>();
			}
			else
			{
				return false;
			}
		}

		private bool CoverStandingDetect( float _frontSign )
		{
			if( Physics.Raycast( new Ray( charRef.hangPosHelper.position , charRef.transf.forward * Mathf.Sign( _frontSign ) ) , out detectRayHit , 1f ) )
			{
				return detectRayHit.collider.GetComponent<CoverObjectClass>();
			}
			else
			{
				return false;
			}

		}

		private void ForceCrouch()
		{
			if( charRef.currMovementType == CharacterMovementTypes.coverCrouch )
			{
				return;
			}

			charRef.ChangeMovementType( CharacterMovementTypes.coverCrouch );

			if( inputRight == 0f )
			{
				charRef.canMove = false;
				charRef.ChangeCoverAction( CharacterCoverActions.stanceTransition );
			}
		}

	}

}
