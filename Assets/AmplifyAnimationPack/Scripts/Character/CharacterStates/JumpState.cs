// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;


namespace AmplifyAnimationPack
{
	public class JumpState : CharacterStateBase
	{
		private bool canLand;
		private float jumpTime;
		private float currentJumpFowardVel;

		private RaycastHit wallpointDetectionHit;

		public override void EnterState()
		{
			charRef.baseColl.radius = 0.05f;
			charRef.rigid.interpolation = RigidbodyInterpolation.Interpolate;
			charRef.rigid.velocity = Vector3.zero;

			canLand = false;

			charRef.ChangeMovementMode( CharacterMovementModes.physicsBased );
			charRef.ChangeMovementType( CharacterMovementTypes.jump );

			jumpTime = 0f;
			charRef.animBeh.anim.SetFloat( "jumpTime" , jumpTime );
			charRef.animBeh.anim.SetBool( "canLand" , false );

			charRef.isJumpingForward = ( charRef.animBeh.anim.velocity.magnitude > 0f );
			currentJumpFowardVel = charRef.currLocomotionType == CharacterLocomotionTypes.walking ? charRef.jumpForwardVelocity * 0.75f : charRef.jumpForwardVelocity;
		}

		public override void ExitState()
		{
			charRef.animBeh.anim.SetBool( "canLand" , false );

		}

		public override void UpdateState()
		{

			if( Physics.Raycast( new Ray( charRef.hangPosHelper.position , charRef.transf.forward ) , out wallpointDetectionHit , 1f ) )
			{
				if( wallpointDetectionHit.collider.gameObject.GetComponent<WallPointClass>() )
				{
					charRef.rigid.velocity = Vector3.zero;
					charRef.ChangeState<HangState>();

					return;
				}
			}

			if( charRef.rigid.velocity.y < 0 )
			{
				// calculates jumpTime
				jumpTime += Time.deltaTime;
				jumpTime = Mathf.Clamp01( jumpTime );
				charRef.animBeh.anim.SetFloat( "jumpTime" , jumpTime );

				// adds falling velocity
				charRef.rigid.velocity += ( Vector3.up * Physics.gravity.y + charRef.transf.forward * currentJumpFowardVel * ( charRef.isJumpingForward ? 1f : 0f ) )
					* charRef.jumpFallMultiplier * Time.deltaTime;

				canLand = ( charRef.rigid.velocity.y < -1f );
			}

			if( canLand )
			{
				Debug.DrawLine( charRef.transf.position , charRef.transf.position - Vector3.up * 1.6f , Color.blue );
				if( Physics.Raycast( charRef.transf.position , -Vector3.up , 1.6f ) )
				{
					charRef.animBeh.anim.SetBool( "canLand" , true );
				}
			}


		}
	}
}
