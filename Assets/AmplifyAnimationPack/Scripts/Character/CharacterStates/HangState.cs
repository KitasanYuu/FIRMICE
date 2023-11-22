// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;


namespace AmplifyAnimationPack
{
	public class HangState : CharacterStateBase
	{

		private float rightAxis, upAxis;
		private RaycastHit raycastDetectionHit;

		public override void EnterState()
		{
			charRef.canMove = true;
			charRef.rigid.useGravity = false;
			charRef.baseColl.radius = 0f;

			charRef.animBeh.anim.SetFloat( "Speed" , 0f );

			charRef.ChangeMovementMode( CharacterMovementModes.animationBased );
			charRef.ChangeClimbingAction( CharacterClimbingActions.idle );
			charRef.ChangeMovementType( CharacterMovementTypes.hang );
			charRef.ChangeAttackAction( CharacterCombatActions.none );

		}

		public override void ExitState()
		{

		}

		public override void UpdateState()
		{
			if( charRef.currClimbingAction == CharacterClimbingActions.reach )
			{
				charRef.animBeh.anim.SetFloat( "HangReachHor" , Mathf.Lerp( charRef.animBeh.anim.GetFloat( "HangReachHor" ) , rightAxis , 0.2f ) );
				charRef.animBeh.anim.SetFloat( "HangReachVert" , Mathf.Lerp( charRef.animBeh.anim.GetFloat( "HangReachVert" ) , upAxis , 0.2f ) );
			}
		}

		public override void InputMoveRight( float _axisVal )
		{
			if( charRef.canMove )
			{
				rightAxis = _axisVal;
				charRef.animBeh.anim.SetFloat( "InputRight" , Mathf.Lerp( charRef.animBeh.anim.GetFloat( "InputRight" ) , _axisVal , 0.2f ) );
				charRef.animBeh.anim.SetFloat( "Speed" , _axisVal );

				if( _axisVal != 0f )
				{

					// movement detection
					if( MovementDetection( _axisVal ) )
					{
						// inside corner detection 
						if( CornerDetection( _axisVal , -1f ) )
						{
							charRef.canMove = false;
							charRef.ChangeClimbingAction( CharacterClimbingActions.cornerInside );
						}
						else
						{
							charRef.ChangeClimbingAction( CharacterClimbingActions.move );
						}
					}
					else
					{
						// outside corner detection
						if( CornerDetection( _axisVal , 1f ) )
						{
							charRef.canMove = false;
							charRef.ChangeClimbingAction( CharacterClimbingActions.cornerOutside );
						}
						else
						{
							// hang jump detection
							if( JumpDetection() )
							{
								charRef.ChangeClimbingAction( CharacterClimbingActions.reach );

							}
							else
							{
								charRef.ChangeClimbingAction( CharacterClimbingActions.idle );
							}

						}

					}

				}
				else
				{

					if( upAxis == 0f )
					{
						charRef.ChangeClimbingAction( CharacterClimbingActions.idle );
					}

				}
			}

		}

		public override void InputMoveForward( float _axisVal )
		{
			if( charRef.canMove )
			{
				upAxis = _axisVal;
				charRef.animBeh.anim.SetFloat( "InputFwd" , Mathf.Lerp( charRef.animBeh.anim.GetFloat( "InputFwd" ) , _axisVal , 0.2f ) );

				if( _axisVal != 0f )
				{
					if( JumpDetection() )
					{
						charRef.ChangeClimbingAction( CharacterClimbingActions.reach );
					}

					charRef.animBeh.anim.SetFloat( "HangReachArmSign" , rightAxis );

				}


			}
		}

		public override void InputJump( bool _actionVal )
		{
			if( _actionVal )
			{
				if( charRef.canMove )
				{
					if( rightAxis != 0f || upAxis != 0f )
					{
						if( charRef.currClimbingAction == CharacterClimbingActions.reachBackwards || charRef.currClimbingAction == CharacterClimbingActions.reach )
						{
							charRef.ChangeClimbingAction( CharacterClimbingActions.jump );
							charRef.canMove = false;
						}
					}
					else
					{
						if( charRef.currClimbingAction == CharacterClimbingActions.reachBackwards )
						{
							charRef.ChangeClimbingAction( CharacterClimbingActions.jump );
							charRef.canMove = false;
						}
						else
						{
							charRef.ChangeClimbingAction( CharacterClimbingActions.release );
							charRef.canMove = false;
						}

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
					Debug.DrawLine( charRef.hangPosHelper.position + charRef.transf.up , charRef.hangPosHelper.position + charRef.transf.up + charRef.transf.forward , Color.red , 2f );
					if( !Physics.Raycast( new Ray( charRef.hangPosHelper.position + charRef.transf.up , charRef.transf.forward ) , 1f ) )
					{
						charRef.canMove = false;
						charRef.baseColl.enabled = false;

						charRef.ChangeClimbingAction( CharacterClimbingActions.climb );
					}


				}
			}
		}

		public override void InputCrouch( bool _actionVal )
		{
			if( charRef.canMove )
			{
				if( _actionVal )
				{
					if( Physics.Raycast( new Ray( charRef.hangPosHelper.position - charRef.transf.forward * 3f , -charRef.transf.forward ) , 1f ) )
					{
						charRef.ChangeClimbingAction( CharacterClimbingActions.reachBackwards );
					}
				}
				else
				{
					if( charRef.currClimbingAction == CharacterClimbingActions.reachBackwards )
					{
						charRef.ChangeClimbingAction( CharacterClimbingActions.idle );

					}
				}
			}
		}

		public override void InputAttack( bool _actionVal , CharacterCombatActions _newAttack )
		{
			if( _actionVal )
			{
				if( charRef.canMove )
				{
					charRef.canMove = false;
					charRef.ChangeAttackAction( _newAttack );
				}
			}
		}


		private bool CornerDetection( float _rightAxis , float _frontAxis )
		{
			// different formulas for front and back check
			if( _frontAxis > 0f )
			{
				if( Physics.Raycast( new Ray( charRef.hangPosHelper.position + charRef.transf.right * Mathf.Sign( _rightAxis ) * 0.5f + charRef.transf.forward * Mathf.Sign( _frontAxis ) ,
						-charRef.transf.right * Mathf.Sign( _rightAxis ) ) , out raycastDetectionHit , 1f ) )
				{
					if( raycastDetectionHit.collider.gameObject.GetComponent<WallPointClass>() )
					{
						return true;
					}
				}

				return false;
			}
			else
			{
				if( Physics.Raycast( new Ray( charRef.hangPosHelper.position + charRef.transf.forward * Mathf.Sign( _frontAxis )
					, charRef.transf.right * Mathf.Sign( _rightAxis ) ) , out raycastDetectionHit , 1f ) )
				{
					if( raycastDetectionHit.collider.gameObject.GetComponent<WallPointClass>() )
					{
						return true;
					}
				}

				return false;
			}
		}

		private bool MovementDetection( float _axisVal )
		{
			if( Physics.Raycast( new Ray( charRef.hangPosHelper.position + charRef.transf.right * Mathf.Sign( _axisVal ) * 0.5f , charRef.transf.forward ) , out raycastDetectionHit , 1f ) )
			{
				if( raycastDetectionHit.collider.gameObject.GetComponent<WallPointClass>() )
				{
					return true;
				}
			}

			return false;
		}

		private bool JumpDetection()
		{
			if( Physics.Raycast( new Ray(
				charRef.hangPosHelper.position + charRef.transf.up * 1.17f * ( ( upAxis == 0f ) ? 0f : Mathf.Sign( upAxis ) ) +
				charRef.transf.right * 1.15f * ( ( rightAxis == 0f ) ? 0f : Mathf.Sign( rightAxis ) ) ,
				charRef.transf.forward ) , out raycastDetectionHit , 1f ) )
			{
				if( raycastDetectionHit.collider.gameObject.GetComponent<WallPointClass>() )
				{
					return true;
				}
			}

			return false;

		}


	}
}
