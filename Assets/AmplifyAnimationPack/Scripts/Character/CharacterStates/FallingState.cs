// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;


namespace AmplifyAnimationPack
{

	public class FallingState : CharacterStateBase
	{
		public override void EnterState()
		{
			charRef.rigid.useGravity = true;
			charRef.rigid.interpolation = RigidbodyInterpolation.Interpolate;
			charRef.baseColl.enabled = true;
			charRef.baseColl.radius = 0.1f;
			charRef.hitbox.SetActive( false );

			charRef.ChangeCollider( CharacterColliderTypes.baseCol );
			charRef.ChangeMovementMode( CharacterMovementModes.physicsBased );
			charRef.ChangeMovementType( CharacterMovementTypes.falling );

			if( prevState != null )
			{
				if( prevState is GroundedState )
				{
					charRef.isJumpingForward = ( charRef.animBeh.anim.velocity.magnitude > 0f && Mathf.Abs( charRef.animBeh.anim.velocity.y ) < 0.5f );
					if( charRef.isJumpingForward )
					{
						charRef.rigid.AddForce( charRef.transf.forward * ( charRef.currLocomotionType == CharacterLocomotionTypes.walking ? 3f : 7f ) , ForceMode.Impulse );
					}
				}
			}

		}

		public override void ExitState()
		{
			charRef.baseColl.radius = 0.5f;
		}

		public override void UpdateState()
		{

			if( Physics.Raycast( charRef.transf.position , -Vector3.up , 0.9f ) )
			{
				if( charRef.currAttackAction == CharacterCombatActions.none )
				{
					charRef.ChangeState<GroundedState>();
				}
				else
				{
					charRef.animBeh.anim.SetBool( "canLand" , true );
				}
			}
		}

		public override void UpdateStatePhysics()
		{
			charRef.rigid.velocity += Vector3.up * Physics.gravity.y * charRef.fallMultiplier * Time.deltaTime;
		}

		public override void InputAttack( bool _actionVal , CharacterCombatActions _newAttack )
		{
			if( _actionVal )
			{
				charRef.ChangeAttackAction( _newAttack );
				charRef.rigid.velocity *= 5f;
			}
		}

	}
}
