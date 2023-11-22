// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;


namespace AmplifyAnimationPack
{

	public class BarHangState : CharacterStateBase
	{
		private RaycastHit detectRayHit;

		public override void EnterState()
		{
			charRef.ChangeCollider( CharacterColliderTypes.baseCol );
			charRef.baseColl.enabled = false;
			charRef.rigid.useGravity = false;

			charRef.ChangeMovementType( CharacterMovementTypes.barHang );

			MonoBehaviour auxBar = (MonoBehaviour)charRef.objToInteract;
			float charDirRelativeToBar = Mathf.Sign( Vector3.Dot( charRef.transf.forward , auxBar.transform.right ) );

			charRef.AdjustRotation( auxBar.transform.rotation * Quaternion.AngleAxis( 90f * charDirRelativeToBar , Vector3.up ) , 0.2f );
			Vector3 posDiff = new Vector3( auxBar.transform.position.x - charRef.transf.position.x , 0f , auxBar.transform.position.z - charRef.transf.position.z );
			Vector3 newPos = auxBar.transform.position + auxBar.transform.right * -charDirRelativeToBar * 0.22f
				+ auxBar.transform.forward * posDiff.magnitude * -Mathf.Sign( Vector3.Dot( auxBar.transform.forward , posDiff ) );
			charRef.AdjustPosition( newPos , 0.1f , true );

			charRef.canMove = ( charRef.currBarWalkAction == CharacterBarWalkActions.drop );
			charRef.ChangeBarHangAction( CharacterBarHangActions.idle );
			charRef.ChangeBarWalkAction( CharacterBarWalkActions.idle );

		}

		public override void ExitState()
		{
		}

		public override void UpdateState()
		{
		}

		public override void InputMoveRight( float _axisVal )
		{
			if( charRef.canMove )
			{
				charRef.animBeh.anim.SetFloat( "InputRight" , _axisVal );
				if( _axisVal != 0f )
				{
					if( DetectMovement( _axisVal ) )
					{
						if( CornerDetection( _axisVal , -1f ) )
						{
							charRef.canMove = false;
							charRef.ChangeBarHangAction( CharacterBarHangActions.cornerInside );
						}
						else
						{
							charRef.ChangeBarHangAction( CharacterBarHangActions.move );
						}
					}
					else if( CornerDetection( _axisVal , 1f ) )
					{
						charRef.canMove = false;
						charRef.ChangeBarHangAction( CharacterBarHangActions.cornerOutside );
					}
					else
					{
						charRef.ChangeBarHangAction( CharacterBarHangActions.idle );
					}

				}
				else
				{
					charRef.ChangeBarHangAction( CharacterBarHangActions.idle );
				}
			}
		}

		public override void InputMoveForward( float _axisVal )
		{
			if( _axisVal > 0f )
			{
				if( charRef.canMove && charRef.currBarHangAction == CharacterBarHangActions.idle )
				{
					charRef.canMove = false;
					charRef.ChangeBarHangAction( CharacterBarHangActions.climb );
				}

			}
		}

		public override void InputCrouch( bool _actionVal )
		{
			if( _actionVal )
			{
				if( charRef.canMove && charRef.currBarHangAction == CharacterBarHangActions.idle )
				{
					charRef.canMove = false;
					charRef.ChangeBarHangAction( CharacterBarHangActions.turn );
				}
			}
		}

		public override void InputJump( bool _actionVal )
		{
			if( _actionVal )
			{
				if( charRef.canMove && charRef.currBarHangAction == CharacterBarHangActions.idle &&
					Physics.Raycast( new Ray( charRef.barHangPosHelper.position + charRef.transf.forward * 3.5f , charRef.transf.forward ) , 0.5f ) )
				{
					charRef.canMove = false;
					charRef.ChangeBarHangAction( CharacterBarHangActions.jump );
				}
			}
		}

		public override void InputInteract( bool _actionVal )
		{
			if( _actionVal )
			{
				if( charRef.canMove && charRef.currBarHangAction == CharacterBarHangActions.idle )
				{
					charRef.canMove = false;
					charRef.ChangeBarHangAction( CharacterBarHangActions.exit );
					charRef.ChangeState<FallingState>();
				}
			}
		}

		private bool DetectMovement( float _axisVal )
		{
			if( Physics.Raycast( new Ray( charRef.barHangPosHelper.position + charRef.transf.right * Mathf.Sign( _axisVal ) * 0.5f , charRef.transf.forward ) , out detectRayHit , 1f ) )
			{
				return detectRayHit.collider.GetComponent<BarObjectClass>();
			}
			else
			{
				return false;
			}
		}

		private bool CornerDetection( float _rightAxis , float _frontAxis )
		{
			// different formulas for front and back check
			if( _frontAxis > 0f )
			{

				if( Physics.Raycast( new Ray( charRef.barHangPosHelper.position + charRef.transf.right * Mathf.Sign( _rightAxis ) * 0.5f + charRef.transf.forward * Mathf.Sign( _frontAxis ) ,
						-charRef.transf.right * Mathf.Sign( _rightAxis ) ) , out detectRayHit , 1f ) )
				{
					return detectRayHit.collider.GetComponent<BarObjectClass>();
				}
				else
				{
					return false;
				}
			}
			else
			{

				if( Physics.Raycast( new Ray( charRef.barHangPosHelper.position + charRef.transf.forward * Mathf.Sign( _frontAxis ) , charRef.transf.right * Mathf.Sign( _rightAxis ) )
					, out detectRayHit , 0.7f ) )
				{
					return detectRayHit.collider.GetComponent<BarObjectClass>();
				}
				else
				{
					return false;
				}

			}
		}

	}

}
