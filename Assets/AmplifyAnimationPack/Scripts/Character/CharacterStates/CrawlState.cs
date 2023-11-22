// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;


namespace AmplifyAnimationPack
{

	public class CrawlState : CharacterStateBase
	{
		// input variables
		private float inputRight;
		private float inputUp;
		private float movementAxisInputs;

		// target variables
		private Vector3 targetDirection;
		private Quaternion targetRotation;

		public override void EnterState()
		{
			// setup components
			charRef.baseColl.enabled = true;
			charRef.rigid.interpolation = RigidbodyInterpolation.None;
			charRef.rigid.velocity = Vector3.zero;
			charRef.rigid.useGravity = true;
			charRef.hitbox.SetActive( false );

			charRef.canMove = false;

			charRef.ChangeMovementMode( CharacterMovementModes.animationBased );
			charRef.ChangeMovementType( CharacterMovementTypes.crawl );
			charRef.ChangeCrawlAction( CharacterCrawlActions.idle );
			charRef.currRotAmount = charRef.crawlRotAmount;

			charRef.targetHeadMarkerPos = charRef.crawlHeadMarkerPos;

		}

		public override void ExitState()
		{
		}

		public override void UpdateState()
		{
			// calculates speed
			movementAxisInputs = Mathf.Abs( inputRight ) + Mathf.Abs( inputUp );
			movementAxisInputs = Mathf.Clamp( movementAxisInputs , 0f , 1f );
			charRef.animBeh.anim.SetFloat( "Speed" , movementAxisInputs );

			if( movementAxisInputs != 0f )
			{
				// set target values
				targetDirection = charRef.GetFollowCamTransform().GetChild( 0 ).forward * inputUp + charRef.GetFollowCamTransform().GetChild( 0 ).right * inputRight;
				targetRotation = Quaternion.Euler( charRef.transf.eulerAngles.x , Quaternion.LookRotation( targetDirection ).eulerAngles.y , charRef.transf.rotation.eulerAngles.z );

				// update animator values
				charRef.animBeh.anim.SetFloat( "TargetDir" ,
					Vector3.Angle( new Vector3( charRef.transf.forward.x , 0f , charRef.transf.forward.z ) , new Vector3( targetDirection.x , 0f , targetDirection.z ) ) );

				if( charRef.canMove )
				{
					if( charRef.animBeh.anim.GetFloat( "TargetDir" ) < 135f )
					{
						// rotate the character
						charRef.transf.rotation = Quaternion.RotateTowards( charRef.transf.rotation , targetRotation , charRef.currRotAmount );
					}

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

		public override void InputJump( bool _actionVal )
		{
			if( _actionVal )
			{
				if( charRef.canMove )
				{
					charRef.canMove = false;
					charRef.ChangeCrawlAction( CharacterCrawlActions.exit );
				}
			}
		}

		public override void InputSprint( bool _actionVal )
		{
			if( charRef.canMove )
			{
				if( _actionVal && movementAxisInputs > 0f && charRef.animBeh.anim.GetFloat( "TargetDir" ) > 75f )
				{
					charRef.animBeh.anim.SetFloat( "CrawlRollSide" , Mathf.Sign( Vector3.Dot( charRef.transf.right , targetDirection ) ) );
					charRef.ChangeCrawlAction( CharacterCrawlActions.barrelRoll );
					charRef.canMove = false;
				}
				else
				{
					charRef.ChangeCrawlAction( CharacterCrawlActions.idle );
				}
			}
		}

	}

}
