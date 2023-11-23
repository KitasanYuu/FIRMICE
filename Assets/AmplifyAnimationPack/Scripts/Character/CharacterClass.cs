// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if CINEMACHINE_PRESENT
using Cinemachine;
#endif


namespace AmplifyAnimationPack
{

	public enum CharacterMovementModes
	{
		animationBased = 0,
		physicsBased = 1
	}

	public enum CharacterMovementTypes
	{
		idle = 0,
		sprint = 1,
		crouch = 2,
		jump = 3,
		falling = 4,
		interact = 5,
		vault = 6,
		shortClimb = 7,
		bigClimb = 8,
		hang = 9,
		crawl = 10,
		cover = 11,
		coverCrouch = 12,
		push = 13,
		barWalk = 14,
		barHang = 15,
		bump = 16,
		climbDown = 17,
		roll = 18,
		dodge = 19,
		ladder = 20
	}

	public enum CharacterColliderTypes
	{
		baseCol = 0,
		crouchCol = 1,
		crawlCol = 2
	}

	public enum CharacterInteractionTypes
	{
		doorOpenOutside = 0,
		doorCloseOutside = 1,
		leverFloorPush = 2,
		leverFloorPull = 3,
		leverWallGoDown = 4,
		leverWallGoUp = 5,
		drink = 6,
		healBandages = 7,
		knockDoor = 8,
		pressButton = 9,
		pressLoop = 10,
		pickLeft = 11,
		pickRight = 12,
		grab = 13,
		doorOpenInside = 14,
		doorCloseInside = 15
	}

	public enum CharacterLocomotionTypes
	{
		stopped = 0,
		walking = 1,
		running = 2,
		strafing = 3
	}

	public enum CharacterCombatActions
	{
		none = 0,
		lightAttack = 1,
		heavyAttack = 2,
		throwObject = 3
	}

	public enum CharacterHitDirections
	{
		upFront = 0,
		right = 1,
		left = 2,
		downBack = 3
	}

	public enum CharacterClimbingActions
	{
		idle = 0,
		move = 1,
		climb = 2,
		cornerOutside = 3,
		cornerInside = 4,
		reachBackwards = 5,
		reach = 6,
		jump = 7,
		getHit = 8,
		release = 9
	}

	public enum CharacterCrawlActions
	{
		idle = 0,
		barrelRoll = 1,
		exit = 2
	}

	public enum CharacterCoverActions
	{
		idle = 0,
		move = 1,
		sneak = 2,
		throwObject = 3,
		cornerOutside = 4,
		cornerInside = 5,
		exit = 6,
		stanceTransition = 7,
		sneakUp = 8,
		throwObjectUp = 9
	}

	public enum CharacterCoverTransitions
	{
		idleToIdle = 0,
		crouchToIdle = 1,
		crouchToCrouch = 2,
		idleToCrouch = 4
	}

	public enum CharacterPushActions
	{
		idle = 0,
		exit = 1
	}

	public enum CharacterBarHangActions
	{
		idle = 0,
		move = 1,
		cornerInside = 2,
		cornerOutside = 3,
		jump = 4,
		climb = 5,
		turn = 6,
		exit = 7
	}

	public enum CharacterBarWalkActions
	{
		idle = 0,
		drop = 1
	}
	public class CharacterClass : MonoBehaviour
	{
		// components
#if CINEMACHINE_PRESENT
		[SerializeField]
		private CinemachineFreeLook followCam;
		private CinemachineCameraOffset followCamOffset;
		private CinemachineBasicMultiChannelPerlin[] cameraNoises;
		[SerializeField]
		private CinemachineFreeLook lockCam;
#endif
		public CharacterUIBehavior uiManager;
		[HideInInspector]
		public CharacterAnimatorBehavior animBeh;
		[HideInInspector]
		public Transform transf;
		[HideInInspector]
		public Transform meshTransf;
		[HideInInspector]
		public Rigidbody rigid;
		[HideInInspector]
		public CapsuleCollider baseColl;
		[HideInInspector]
		public CapsuleCollider crouchColl;
		[HideInInspector]
		public CapsuleCollider crawlColl;
		[HideInInspector]
		public GameObject hitbox;
		[HideInInspector]
		public Transform headMarker;
		private Transform collParent;
		private CapsuleCollider activeColl;

		// state variables
		private CharacterStateBase currState;
		private List<CharacterStateBase> createdStates;

		// raycast variables
		private RaycastHit groundedCheckHit;

		// enums
		[HideInInspector]
		public CharacterMovementTypes currMovementType { get; private set; }
		[HideInInspector]
		public CharacterMovementModes currMovementMode { get; private set; }
		[HideInInspector]
		public CharacterInteractionTypes currInteractType { get; private set; }
		[HideInInspector]
		public CharacterLocomotionTypes currLocomotionType { get; private set; }
		[HideInInspector]
		public CharacterCombatActions currAttackAction { get; private set; }
		[HideInInspector]
		public CharacterClimbingActions currClimbingAction { get; private set; }
		[HideInInspector]
		public CharacterHitDirections currHitDirection { get; private set; }
		[HideInInspector]
		public CharacterCrawlActions currCrawlAction { get; private set; }
		[HideInInspector]
		public CharacterCoverActions currCoverAction { get; private set; }
		[HideInInspector]
		public CharacterCoverTransitions currCoverTransition { get; private set; }
		[HideInInspector]
		public CharacterPushActions currPushAction { get; private set; }
		[HideInInspector]
		public CharacterBarHangActions currBarHangAction { get; private set; }
		[HideInInspector]
		public CharacterBarWalkActions currBarWalkAction { get; private set; }

		// movement variables
		[HideInInspector]
		public bool canMove;
		[HideInInspector]
		public bool canFallOffRoll;
		[HideInInspector]
		public bool isLockedToWalking;
		[HideInInspector]
		public float currRotAmount;
		[Header( "Movement Variables" )]
		[Range( 0.01f , 10f )]
		public float idleRotAmount;
		[Range( 0.01f , 10f )]
		public float sprintRotAmount;
		[Range( 0.01f , 10f )]
		public float crouchRotAmount;
		[Range( 0.01f , 10f )]
		public float crawlRotAmount;
		[Range( 0.01f , 3f )]
		public float bumpDelay;
		[Range( 0f , 15f )]
		public float leanLimitDefault;
		[Range( 0f , 15f )]
		public float leanLimitSprint;

		// jump variables
		[HideInInspector]
		public bool isJumpingForward;
		[Header( "Jump Variables" )]
		public float jumpVelocity = 10f;
		public float jumpForwardVelocity = 15f;
		[Range( 1f , 10f )]
		public float jumpFallMultiplier = 2f;

		// falling variables
		[Header( "Falling Variables" )]
		[Range( 0.1f , 10f )]
		public float fallMultiplier = 1.2f;

		// cover variables
		[Header( "Cover Variables" )]
		[Range( 0f , 10f )]
		public float coverSneakCamOffset = 1f;

		// climbing variables
		[Header( "Climbing Variables" )]
		public float climbingTimeThreshold;

		// interaction variables
		[HideInInspector]
		public IInteractable objToInteract;
		[HideInInspector]
		public Vector3 objToInteractRotationVector;
		[HideInInspector]
		public Vector3 objToInteractContactPoint;
		[HideInInspector]
		public GameObject objectCaught;

		// lockOn variables
		[HideInInspector]
		public Transform lockOnTarget;
		[HideInInspector]
		public bool isLocked;
		[Header( "LockOn Variables" )]
		public float lockOnRange;

		// camera Variables
		private float startRotationY;
		[Header( "Camera Variables" )]
		public bool isYInverted;
		public Vector3 idleHeadMarkerPos;
		public Vector3 crouchHeadMarkerPos;
		public Vector3 crawlHeadMarkerPos;
		public Vector3 coverHeadMarkerPos;
		[HideInInspector]
		public Vector3 targetHeadMarkerPos;
		[HideInInspector]
		public Vector3 cameraTargetOffset;
		public float sprintTargetOffsetAmount;
		public float crouchTargetOffsetAmount;

		// position helpers
		public Transform hangPosHelper { get; private set; }
		public Transform barHangPosHelper { get; private set; }

		private void Awake()
		{
			// get references
			transf = transform;
			rigid = GetComponent<Rigidbody>();
			baseColl = transf.Find( "Colliders/BaseCollider" ).GetComponent<CapsuleCollider>();
			crouchColl = transf.Find( "Colliders/CrouchCollider" ).GetComponent<CapsuleCollider>();
			crawlColl = transf.Find( "Colliders/CrawlCollider" ).GetComponent<CapsuleCollider>();
			collParent = transf.Find( "Colliders" );
			meshTransf = transf.Find( "Mesh" );
			animBeh = meshTransf.GetComponent<CharacterAnimatorBehavior>();
			hitbox = transf.Find( "Hitbox" ).gameObject;
			headMarker = transf.Find( "HeadMarker" );
			hangPosHelper = transf.Find( "Mesh/HangPosHelper" );
			barHangPosHelper = transf.Find( "Mesh/BarHangPosHelper" );

			// create camera noises array
#if CINEMACHINE_PRESENT
			followCamOffset = followCam.GetComponent<CinemachineCameraOffset>();
			cameraNoises = new CinemachineBasicMultiChannelPerlin[ 3 ]
			{
			followCam.GetRig(0).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>(),
			followCam.GetRig(1).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>(),
			followCam.GetRig(2).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>()
			};
#else
			DebugCinemachineError();
#endif

		}

		private void Start()
		{
			Setup();
		}

		void Setup()
		{
			// setup UI
			uiManager.ToggleWarning();
			uiManager.DisableCursor();
			uiManager.InteractionText_Disable();

			// setup variables
			startRotationY = transf.eulerAngles.y;
			activeColl = null;
			objectCaught = null;

			// setup anim behavior
			animBeh.Setup( transf );

			// setup states
			createdStates = new List<CharacterStateBase>();
			ChangeState<GroundedState>();
		}

		private void Update()
		{
			GetInputs();
			GetDebugInputs();

			currState.UpdateState();
			UpdateCamera();
		}

		private void FixedUpdate()
		{
			currState.UpdateStatePhysics();
		}

		private void LateUpdate()
		{
			currState.LateUpdateState();
		}


		void GetInputs()
		{
			currState.InputMoveRight( Convert.ToSingle( Input.GetKey( KeyCode.A ) ) * -1f + Convert.ToSingle( Input.GetKey( KeyCode.D ) ) );
			currState.InputMoveForward( Convert.ToSingle( Input.GetKey( KeyCode.S ) ) * -1f + Convert.ToSingle( Input.GetKey( KeyCode.W ) ) );
			currState.InputSprint( Input.GetKey( KeyCode.LeftShift ) );
			currState.InputCrouch( Input.GetKey( KeyCode.LeftControl ) );
			currState.InputJump( Input.GetKeyDown( KeyCode.Space ) );
			currState.InputInteract( Input.GetKeyDown( KeyCode.E ) );
			currState.InputAttack( Input.GetKeyDown( KeyCode.Mouse0 ) , CharacterCombatActions.lightAttack );
			currState.InputAttack( Input.GetKeyDown( KeyCode.Mouse1 ) , CharacterCombatActions.heavyAttack );
			currState.InputDodge( Input.GetKeyDown( KeyCode.Mouse2 ) );
			currState.InputLockOn( Input.GetKeyDown( KeyCode.Q ) );
		}

		void GetDebugInputs()
		{
			// invert camera rotation
			if( Input.GetKeyDown( KeyCode.I ) )
			{
				isYInverted = !isYInverted;
			}

			// slowMotion
			if( Input.GetKeyDown( KeyCode.Tab ) )
			{
				Time.timeScale = ( Time.timeScale == 1f ) ? 0.2f : 1f;
			}

			// teleport to Playground
			if( Input.GetKeyDown( KeyCode.P ) )
			{
				if( SceneManager.GetActiveScene().name == "DemoScene" )
				{
					if( currState is GroundedState )
					{
						transform.rotation = Quaternion.Euler( 0f , 90f , 0f );
						transform.position = new Vector3( 86.78f , 2.63f , -3.11f );
					}
				}
			}

			// lock to Walking
			isLockedToWalking = Input.GetKey( KeyCode.LeftAlt );

		}

		private void UpdateCamera()
		{
#if CINEMACHINE_PRESENT
			followCam.m_YAxis.m_InvertInput = isYInverted;
			followCamOffset.m_Offset = Vector3.Lerp( followCamOffset.m_Offset , cameraTargetOffset , 0.1f );
#else
			DebugCinemachineError();
#endif
			headMarker.localPosition = Vector3.Lerp( headMarker.localPosition , targetHeadMarkerPos , 0.1f );
		}



		public void CheckIsGrounded()
		{
			if( !Physics.SphereCast( transf.position , 0.4f , -Vector3.up , out groundedCheckHit , 0.6f ) )
			{
				ChangeState<FallingState>();
			}
			else if( groundedCheckHit.collider.name.Contains( "BarObject" ) )
			{
				ChangeState<BarWalkState>();
			}
		}

	#region StateMethods

			public void ChangeState<T>() where T : CharacterStateBase, new()
			{
				CharacterStateBase previousState = null;
				if( currState != null )
				{
					if( typeof( T ) == currState.GetType() ) return;

					previousState = currState;
					currState.ExitState();
				}

				// creates and enters the new state 
				currState = CreateState<T>();
				currState.charRef = this;
				currState.prevState = previousState;
				currState.EnterState();

			}

			CharacterStateBase CreateState<T>() where T : CharacterStateBase, new()
			{
				//check if state has been created
				for( int i = 0 ; i < createdStates.Count ; i++ )
				{
					if( typeof( T ) == createdStates[ i ].GetType() )
					{
						return createdStates[ i ];
					}
				}

				return new T();

			}

	#endregion

	#region EnumsMethods

			public void ChangeMovementMode( CharacterMovementModes _newMode )
			{
				if( currMovementMode == _newMode )
				{
					return;
				}

				currMovementMode = _newMode;
			}

			public void ChangeMovementType( CharacterMovementTypes _newType )
			{
				if( currMovementType == _newType )
				{
					return;
				}

				currMovementType = _newType;
				animBeh.anim.SetInteger( "MovementType" , (int)currMovementType );

			}

			public void ChangeInteractType( CharacterInteractionTypes _newType )
			{
				if( currInteractType == _newType )
				{
					return;
				}

				currInteractType = _newType;
				animBeh.anim.SetInteger( "InteractionType" , (int)currInteractType );
			}

			public void ChangeLocomotionType( CharacterLocomotionTypes _newType )
			{
				if( currLocomotionType == _newType )
				{
					return;
				}

				currLocomotionType = _newType;
				animBeh.anim.SetInteger( "LocomotionType" , (int)currLocomotionType );
			}

			public void ChangeAttackAction( CharacterCombatActions _attackIndex )
			{
				if( currAttackAction == _attackIndex )
				{
					return;
				}
				currAttackAction = _attackIndex;
				animBeh.anim.SetInteger( "CombatAction" , (int)currAttackAction );
			}

			public void ChangeClimbingAction( CharacterClimbingActions _newType )
			{
				if( currClimbingAction == _newType )
				{
					return;
				}
				currClimbingAction = _newType;
				animBeh.anim.SetInteger( "ClimbAction" , (int)currClimbingAction );
			}

			public void ChangeHitDirection( CharacterHitDirections _newDir )
			{
				if( currHitDirection == _newDir )
				{
					return;
				}
				currHitDirection = _newDir;
				animBeh.anim.SetInteger( "HitDirection" , (int)currHitDirection );
			}

			public void ChangeCrawlAction( CharacterCrawlActions _newAction )
			{
				if( currCrawlAction == _newAction )
				{
					return;
				}
				currCrawlAction = _newAction;
				animBeh.anim.SetInteger( "CrawlAction" , (int)currCrawlAction );
			}

			public void ChangeCoverAction( CharacterCoverActions _newAction )
			{
				if( currCoverAction == _newAction )
				{
					return;
				}
				currCoverAction = _newAction;
				animBeh.anim.SetInteger( "CoverAction" , (int)currCoverAction );
			}

			public void ChangeCoverTransition( CharacterCoverTransitions _newTransition )
			{
				if( currCoverTransition == _newTransition )
				{
					return;
				}
				currCoverTransition = _newTransition;
				animBeh.anim.SetInteger( "CoverTransition" , (int)currCoverTransition );
			}

			public void ChangePushAction( CharacterPushActions _newAction )
			{
				if( currPushAction == _newAction )
				{
					return;
				}
				currPushAction = _newAction;
				animBeh.anim.SetInteger( "PushAction" , (int)currPushAction );
			}

			public void ChangeBarHangAction( CharacterBarHangActions _newAction )
			{
				if( currBarHangAction == _newAction )
				{
					return;
				}
				currBarHangAction = _newAction;
				animBeh.anim.SetInteger( "BarHangAction" , (int)currBarHangAction );
			}

			public void ChangeBarWalkAction( CharacterBarWalkActions _newAction )
			{
				if( currBarWalkAction == _newAction )
				{
					return;
				}
				currBarWalkAction = _newAction;
				animBeh.anim.SetInteger( "BarWalkAction" , (int)currBarWalkAction );
			}

	#endregion

	#region AdjustMethods

			public void AdjustPosition( Vector3 _newPos , float _lerpTime , bool is2D )
			{
				if( _lerpTime > 0f )
				{
					StartCoroutine( AdjustPosOverTime( new Vector3( _newPos.x , ( is2D ) ? transf.position.y : _newPos.y , _newPos.z ) , _lerpTime ) );
				}
				else
				{
					transf.position = _newPos;
				}
			}

			IEnumerator AdjustPosOverTime( Vector3 _finalPos , float _timeToLerp )
			{
				float currLerpTime = 0f;
				Vector3 startPos = transf.position;
				while( currLerpTime < _timeToLerp )
				{
					transf.position = Vector3.Lerp( startPos , _finalPos , currLerpTime / _timeToLerp );

					currLerpTime += Time.deltaTime;

					yield return null;
				}

				transf.position = _finalPos;

			}

			public void AdjustRotation( Quaternion _newRot , float _lerpTime )
			{
				if( _lerpTime > 0f )
				{
					StartCoroutine( AdjustRotOverTime( _newRot , _lerpTime ) );
				}
				else
				{
					transf.rotation = _newRot;
				}
			}

			IEnumerator AdjustRotOverTime( Quaternion _finalRot , float _timeToLerp )
			{
				float currLerpTime = 0f;
				Quaternion startRot = transf.rotation;
				while( currLerpTime < _timeToLerp )
				{
					transf.rotation = Quaternion.Lerp( startRot , _finalRot , currLerpTime / _timeToLerp );

					currLerpTime += Time.deltaTime;

					yield return null;
				}

				transf.rotation = _finalRot;
			}

	#endregion

	#region CapsuleMethods

			IEnumerator AdjustCapsuleRadius( float _newRadius , float _adjustTime )
			{
				float currTime = 0f;
				float startingRadius = baseColl.radius;

				while( currTime <= _adjustTime )
				{
					if( !Physics.Raycast( new Ray( transf.position , -transf.forward ) , 0.3f ) )
					{
						baseColl.radius = Mathf.Lerp( startingRadius , _newRadius , currTime / _adjustTime );
						currTime += Time.deltaTime;
					}
					yield return null;

				}
			}

			public void LerpCapsuleRadius( float _finalRadius , float _lerpTime )
			{
				StartCoroutine( AdjustCapsuleRadius( _finalRadius , _lerpTime ) );
			}

			public void ChangeCollider( CharacterColliderTypes _newCollider )
			{

				string newColliderName = "";
				switch( _newCollider )
				{
					case CharacterColliderTypes.baseCol:
					newColliderName = "BaseCollider";
					break;
					case CharacterColliderTypes.crouchCol:
					newColliderName = "CrouchCollider";
					break;
					case CharacterColliderTypes.crawlCol:
					newColliderName = "CrawlCollider";
					break;
					default:
					newColliderName = "BaseCollider";
					break;
				}

				if( activeColl != null )
				{
					if( activeColl.name == newColliderName )
					{
						return;
					}
				}

				for( int i = 0 ; i < collParent.childCount ; i++ )
				{
					collParent.GetChild( i ).gameObject.SetActive( false );
				}

				transf.Find( "Colliders/" + newColliderName ).gameObject.SetActive( true );
				activeColl = transf.Find( "Colliders/" + newColliderName ).GetComponent<CapsuleCollider>();
				activeColl.enabled = true;
			}

	#endregion

	#region LockOnMethods

			public void TriggerLockOn()
			{
				if( isLocked )
				{
#if CINEMACHINE_PRESENT
					followCam.m_XAxis.Value = Mathf.Abs( transf.eulerAngles.y ) - startRotationY;
					followCam.m_YAxis.Value = 0.6f;
					followCam.Priority = 1;
					lockCam.Priority = 0;
#else
					DebugCinemachineError();
#endif
					isLocked = false;
					lockOnTarget = null;

					ChangeLocomotionType( CharacterLocomotionTypes.running );

					return;
				}

				lockOnTarget = null;
				Collider[] lockOnColliders = Physics.OverlapSphere( transf.position , lockOnRange );
				for( int i = 0 ; i < lockOnColliders.Length ; i++ )
				{
					if( lockOnColliders[ i ].name.Contains( "EnemyObject" ) )
					{
						lockOnTarget = lockOnColliders[ i ].transform;
						break;
					}
				}

				if( lockOnTarget != null )
				{
					isLocked = true;
#if CINEMACHINE_PRESENT
					lockCam.LookAt = lockOnTarget;
					followCam.Priority = 0;
					lockCam.Priority = 1;
#else
					DebugCinemachineError();
#endif
				
				ChangeLocomotionType( CharacterLocomotionTypes.strafing );

				}

			}


		#endregion

		#region CameraMethods

		public Transform GetFollowCamTransform()
		{
#if CINEMACHINE_PRESENT
			return followCam.transform;
#else
			DebugCinemachineError();
			return null;
#endif
		}


		public void SetCameraShakeAmplitude( float _valueToSet )
		{
#if CINEMACHINE_PRESENT
			for( int i = 0 ; i < cameraNoises.Length ; i++ )
			{
				cameraNoises[ i ].m_AmplitudeGain = _valueToSet;
			}
#else
			DebugCinemachineError();
#endif

		}

		public void SetCameraShakeFrequency( float _valueToSet )
		{
#if CINEMACHINE_PRESENT
			for( int i = 0 ; i < cameraNoises.Length ; i++ )
			{
				cameraNoises[ i ].m_FrequencyGain = _valueToSet;
			}
#else
			DebugCinemachineError();
#endif
		}

		public void CameraShakeEvent_Start( float _time , float _amplitude , float _frequency ) 
		{
			StartCoroutine( CameraShakeEvent( _time , _amplitude , _frequency ) );
		}

		private IEnumerator CameraShakeEvent( float _waitTime , float _shakeAmplitude , float _shakeFrequency )
		{
			SetCameraShakeAmplitude( _shakeAmplitude );
			SetCameraShakeFrequency( _shakeFrequency );

			yield return new WaitForSeconds( _waitTime );
			SetCameraShakeAmplitude( 0f );
		}

#endregion

#region DebugMethods

		private void OnDrawGizmos()
		{
			Gizmos.DrawWireSphere( transform.position - Vector3.up * 0.6f , 0.4f );
		}

		public void DebugCinemachineError()
		{
			Debug.LogError( "Cinemachine not installed! Please install the Cinemachine package for optimal usage." );

		}

#endregion
	
	}
}

