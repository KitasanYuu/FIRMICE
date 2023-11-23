// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;


namespace AmplifyAnimationPack
{

	public class GroundedState : CharacterStateBase
	{
		// input variables
		private float inputRight;
		private float inputUp;
		private float movementAxisInputs;

		// target variables
		private Vector3 targetDirection;
		private Quaternion targetRotation;
		private Vector3 targetDir2D;
		private Vector3 characterFwd2D;

		// ray variables for raycasts
		private Ray detectionRay;
		private RaycastHit detectionRayHit;

		// bump variables
		private float currTimeToBump;

		// lean variables
		private float newLeanAmount;
		private float finalLeanAmount;
		private Quaternion finalLeanRot;
		private float currentLeanLimit;

		public override void EnterState()
		{
			// setup components
			charRef.ChangeCollider( CharacterColliderTypes.baseCol );
			charRef.baseColl.enabled = true;
			charRef.LerpCapsuleRadius( 0.5f , 0.2f );
			charRef.rigid.interpolation = RigidbodyInterpolation.None;
			charRef.rigid.velocity = Vector3.zero;
			charRef.rigid.useGravity = true;
			charRef.hitbox.SetActive( false );
			charRef.uiManager.PlaygroundText_Enable();

			charRef.canMove = true;

			charRef.ChangeMovementMode( CharacterMovementModes.animationBased );
			charRef.ChangeMovementType( CharacterMovementTypes.idle );
			charRef.ChangeAttackAction( CharacterCombatActions.none );
			charRef.currRotAmount = charRef.idleRotAmount;

			charRef.targetHeadMarkerPos = charRef.idleHeadMarkerPos;
			charRef.cameraTargetOffset = Vector3.zero;

		}

		public override void ExitState()
		{
			charRef.meshTransf.rotation = Quaternion.Euler( charRef.meshTransf.eulerAngles.x , charRef.meshTransf.eulerAngles.y , 0f );

			charRef.targetHeadMarkerPos = charRef.idleHeadMarkerPos;
			charRef.transf.rotation = charRef.transf.rotation;

			charRef.cameraTargetOffset = Vector3.zero;
			charRef.SetCameraShakeAmplitude( 0f );
			charRef.uiManager.PlaygroundText_Disable();

		}

		public override void UpdateState()
		{
			charRef.CheckIsGrounded();
			DetectObjectToInteract();
			DetectClimbDown();

			// calculates speed
			movementAxisInputs = Mathf.Abs( inputRight ) + Mathf.Abs( inputUp );
			movementAxisInputs = Mathf.Clamp( movementAxisInputs , 0f , 1f );
			charRef.animBeh.anim.SetFloat( "Speed" , movementAxisInputs );

			if( movementAxisInputs != 0f )
			{
				// set target values
				targetDirection = charRef.GetFollowCamTransform().GetChild( 0 ).forward * inputUp + charRef.GetFollowCamTransform().GetChild( 0 ).right * inputRight;
				targetRotation = Quaternion.Euler( charRef.transf.eulerAngles.x , Quaternion.LookRotation( targetDirection ).eulerAngles.y , charRef.transf.rotation.eulerAngles.z );
				targetDir2D = new Vector3( targetDirection.x , 0f , targetDirection.z );
				characterFwd2D = new Vector3( charRef.transf.forward.x , 0f , charRef.transf.forward.z );

				// update animator values
				charRef.animBeh.anim.SetFloat( "TimeToStop" , 0f );
				charRef.animBeh.anim.SetFloat( "TargetDir" , Vector3.Angle( characterFwd2D , targetDir2D ) );

				if( charRef.isLocked )
				{
					if( charRef.canMove )
					{
						charRef.animBeh.anim.SetFloat( "InputRight" , Mathf.Lerp( charRef.animBeh.anim.GetFloat( "InputRight" ) , inputRight , 0.2f ) );
						charRef.animBeh.anim.SetFloat( "InputFwd" , Mathf.Lerp( charRef.animBeh.anim.GetFloat( "InputFwd" ) , inputUp , 0.2f ) );

						RotateCharToEnemy();
					}

				}
				else
				{

					charRef.animBeh.anim.SetFloat( "InputRight" , inputRight );
					charRef.animBeh.anim.SetFloat( "InputFwd" , inputUp );

					if( charRef.canMove )
					{
						charRef.ChangeLocomotionType( ( charRef.isLockedToWalking ) ? CharacterLocomotionTypes.walking : CharacterLocomotionTypes.running );


						// when not turning around
						if( charRef.animBeh.anim.GetFloat( "TargetDir" ) < 135f )
						{
							// rotate the character
							charRef.transf.rotation = Quaternion.RotateTowards( charRef.transf.rotation , targetRotation , charRef.currRotAmount );
						}

						// detect obstacles
						ObstacleDetection();
					}
				}


			}
			else
			{
				// stops the character
				if( charRef.canMove )
				{
					if( !charRef.isLocked )
					{
						charRef.ChangeLocomotionType( CharacterLocomotionTypes.stopped );
					}
					else
					{
						RotateCharToEnemy();
					}
					charRef.animBeh.anim.SetFloat( "TimeToStop" , Mathf.Min( charRef.animBeh.anim.GetFloat( "TimeToStop" ) + Time.deltaTime , 1f ) );
				}

			}

			// calculates the amount to lean to based on direction
			CalculateLeaningAmount();

		}

		public override void LateUpdateState()
		{
			ApplyLeaning();

		}

	

		public override void InputMoveForward( float _axisVal )
		{
			inputUp = _axisVal;

		}

		public override void InputMoveRight( float _axisVal )
		{
			inputRight = _axisVal;
		}

		public override void InputSprint( bool _actionVal )
		{
			if( charRef.canMove )
			{
				if( _actionVal && movementAxisInputs > 0f )
				{
					if( charRef.currMovementType == CharacterMovementTypes.idle )
					{
						charRef.ChangeMovementType( CharacterMovementTypes.sprint );
						charRef.currRotAmount = charRef.sprintRotAmount;

						charRef.SetCameraShakeAmplitude( 0.4f );
						charRef.SetCameraShakeFrequency( 4f );
						charRef.cameraTargetOffset = new Vector3( 0f , 0f , charRef.sprintTargetOffsetAmount );
					}
				}
				else
				{
					if( charRef.currMovementType == CharacterMovementTypes.sprint )
					{
						charRef.ChangeMovementType( CharacterMovementTypes.idle );
						charRef.currRotAmount = charRef.idleRotAmount;

						charRef.SetCameraShakeAmplitude( 0f );
						charRef.cameraTargetOffset = Vector3.zero;


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
					if( charRef.currMovementType == CharacterMovementTypes.idle )
					{
						charRef.targetHeadMarkerPos = charRef.crouchHeadMarkerPos;

						charRef.ChangeMovementType( CharacterMovementTypes.crouch );
						charRef.ChangeCollider( CharacterColliderTypes.crouchCol );
						charRef.currRotAmount = charRef.crouchRotAmount;

						charRef.cameraTargetOffset = new Vector3( 0f , 0f , charRef.crouchTargetOffsetAmount );
					}
				}
				else
				{
					if( charRef.currMovementType == CharacterMovementTypes.crouch )
					{
						charRef.targetHeadMarkerPos = charRef.idleHeadMarkerPos;

						charRef.ChangeMovementType( CharacterMovementTypes.idle );
						charRef.ChangeCollider( CharacterColliderTypes.baseCol );
						charRef.currRotAmount = charRef.idleRotAmount;

						charRef.cameraTargetOffset = Vector3.zero;
					}
				}
			}
		}

		public override void InputJump( bool _actionVal )
		{
			if( _actionVal )
			{
				if( charRef.canMove )
				{
					if( charRef.currMovementType == CharacterMovementTypes.crouch )
					{
						charRef.ChangeState<CrawlState>();
					}
					else
					{
						charRef.ChangeState<JumpState>();
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
					if( charRef.objToInteract != null )
					{
						charRef.objToInteract.Interact( charRef );
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
					charRef.ChangeAttackAction( ( !charRef.objectCaught ) ? _newAttack : CharacterCombatActions.throwObject );
					charRef.ChangeState<AttackState>();

				}
			}
		}

		public override void InputDodge( bool _actionVal )
		{
			if( _actionVal )
			{
				if( charRef.canMove )
				{
					if( charRef.isLocked )
					{
						charRef.ChangeState<DodgeState>();
					}
					else
					{
						charRef.ChangeState<RollState>();
					}

				}
			}
		}

		public override void InputLockOn( bool _actionVal )
		{
			if( _actionVal )
			{
				if( charRef.canMove )
				{
					charRef.TriggerLockOn();
				}
			}
		}

		private void ObstacleDetection()
		{

			detectionRay = new Ray( charRef.transf.position - Vector3.up * 0.5f , charRef.transf.forward );
			if( Physics.Raycast( detectionRay , out detectionRayHit , 1.7f ) )
			{
				if( charRef.currMovementType != CharacterMovementTypes.crouch )
				{
					if( detectionRayHit.collider.gameObject.GetComponent<ObstacleClass>() )
					{
						charRef.ChangeState<VaultState>();
					}
					else
					{
						currTimeToBump += Time.deltaTime;
						if( currTimeToBump >= charRef.bumpDelay )
						{
							charRef.ChangeState<BumpState>();
						}
					}
				}
			}
			else
			{
				currTimeToBump = 0f;
			}
		}

		private void CalculateLeaningAmount()
		{
			if( charRef.canMove && movementAxisInputs != 0f )
			{
				newLeanAmount = Vector3.SignedAngle( characterFwd2D , targetDir2D , Vector3.up ) / 135f * 90f;
				newLeanAmount = Mathf.Round( newLeanAmount * 100f ) / 100f;
				currentLeanLimit = ( charRef.currMovementType == CharacterMovementTypes.sprint ) ? charRef.leanLimitSprint : charRef.leanLimitDefault;
				newLeanAmount = Mathf.Clamp( newLeanAmount , -currentLeanLimit , currentLeanLimit );

				finalLeanAmount = Mathf.Lerp( finalLeanAmount , newLeanAmount , 0.2f );
			}
			else
			{
				newLeanAmount = 0f;
				finalLeanAmount = Mathf.Lerp( finalLeanAmount , newLeanAmount , 0.2f );
			}


		}

		private void ApplyLeaning()
		{
			if( !charRef.isLocked )
			{
				finalLeanRot = Quaternion.Euler( charRef.meshTransf.eulerAngles.x , charRef.meshTransf.eulerAngles.y , -finalLeanAmount );
				charRef.meshTransf.rotation = finalLeanRot;
			}
		}

		private void RotateCharToEnemy()
		{
			targetRotation = Quaternion.Euler(
				charRef.transf.eulerAngles.x ,
				Quaternion.LookRotation( charRef.lockOnTarget.transform.position - charRef.transf.position ).eulerAngles.y ,
				charRef.transf.rotation.eulerAngles.z );
			charRef.transf.rotation = Quaternion.RotateTowards( charRef.transf.rotation , targetRotation , 7f );
		}

		private void DetectObjectToInteract()
		{
			detectionRay = new Ray( charRef.transf.position , charRef.transf.forward );
			if( Physics.Raycast( detectionRay , out detectionRayHit , 0.7f ) )
			{
				charRef.objToInteractRotationVector = detectionRayHit.normal;
				charRef.objToInteractContactPoint = detectionRayHit.point;

				if( charRef.objToInteract == null )
				{
					if( detectionRayHit.collider.gameObject != null )
					{
						if( detectionRayHit.collider.gameObject.GetComponent<ClimbObjectClass>() )
						{
							charRef.objToInteract = detectionRayHit.collider.gameObject.GetComponent<ClimbObjectClass>();
							charRef.uiManager.InteractionText_Enable( "Press E to Climb" );
						}
						else if( detectionRayHit.collider.gameObject.GetComponent<CoverObjectClass>() )
						{
							charRef.objToInteract = detectionRayHit.collider.gameObject.GetComponent<CoverObjectClass>();
							charRef.uiManager.InteractionText_Enable( "Press E to Enter cover" );
						}
						else if( detectionRayHit.collider.gameObject.GetComponent<PushObjectClass>() )
						{
							charRef.objToInteract = detectionRayHit.collider.gameObject.GetComponent<PushObjectClass>();
							charRef.uiManager.InteractionText_Enable( "Press E to Push" );
						}
					}
				}
			}
			else
			{
				if( charRef.objToInteract != null )
				{
					if( charRef.objToInteract is ClimbObjectClass || charRef.objToInteract is CoverObjectClass || charRef.objToInteract is PushObjectClass )
					{
						charRef.objToInteract = null;
						charRef.uiManager.InteractionText_Disable();
					}

				}
			}
		}

		private void DetectClimbDown()
		{
			detectionRay = new Ray( charRef.transf.position , -charRef.transf.up );
			if( Physics.SphereCast( detectionRay , 0.2f , out detectionRayHit , 1f ) )
			{
				if( charRef.objToInteract == null )
				{
					if( detectionRayHit.collider.gameObject.GetComponent<WallPointClass>() )
					{
						charRef.objToInteract = detectionRayHit.collider.gameObject.GetComponent<WallPointClass>();
						charRef.uiManager.InteractionText_Enable( "Press E to Climb down" );
					}
				}
			}
			else if( charRef.objToInteract is WallPointClass )
			{
				charRef.objToInteract = null;
				charRef.uiManager.InteractionText_Disable();
			}

		}

	}
}
