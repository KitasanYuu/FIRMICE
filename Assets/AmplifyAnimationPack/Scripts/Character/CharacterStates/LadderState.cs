// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;

namespace AmplifyAnimationPack
{

	public class LadderState : CharacterStateBase
	{
		public override void EnterState()
		{
			charRef.rigid.useGravity = false;
			charRef.ChangeCollider( CharacterColliderTypes.baseCol );
			charRef.baseColl.enabled = false;

			charRef.animBeh.anim.SetFloat( "Speed" , 0f );
			charRef.animBeh.anim.SetBool( "canLand" , false );
			charRef.animBeh.anim.SetBool( "Ladder_IsSliding" , false );

			charRef.ChangeMovementType( CharacterMovementTypes.ladder );

			charRef.canMove = true;
		}

		public override void ExitState()
		{
			charRef.animBeh.anim.SetFloat( "InputFwd" , 0f );
			charRef.animBeh.anim.SetFloat( "Speed" , 0f );
			charRef.animBeh.anim.SetBool( "canLand" , false );

		}

		public override void UpdateState()
		{
			if( !charRef.canMove )
			{
				charRef.rigid.velocity = -charRef.transf.up * 3f;
				charRef.animBeh.anim.SetBool( "canLand" , Physics.Raycast( new Ray( charRef.transf.position , -charRef.transf.up ) , 0.9f ) );
				if( charRef.animBeh.anim.GetBool( "canLand" ) )
				{
					charRef.rigid.velocity = Vector3.zero;
				}
			}
		}

		public override void InputMoveForward( float _axisVal )
		{
			if( !charRef.canMove )
			{
				return;
			}


			charRef.animBeh.anim.SetFloat( "InputFwd" , _axisVal );
			charRef.animBeh.anim.SetFloat( "Speed" , _axisVal );

			if( _axisVal > 0f )
			{
				charRef.animBeh.anim.SetBool( "canLand" , !Physics.Raycast( new Ray( charRef.hangPosHelper.position - charRef.transf.up * 0.3f , charRef.transf.forward ) , 0.5f ) );

			}
			else if( _axisVal < 0f )
			{
				charRef.animBeh.anim.SetBool( "canLand" , Physics.Raycast( new Ray( charRef.transf.position , -charRef.transf.up ) , 1f ) );
			}

		}

		public override void InputJump( bool _actionVal )
		{
		
			if( _actionVal )
			{
				if( charRef.animBeh.anim.GetFloat( "Speed" ) > 0f )
				{
					return;
				}

				charRef.animBeh.anim.SetBool( "Ladder_IsSliding" , true );
				charRef.canMove = false;
			}
		}
	}
}
