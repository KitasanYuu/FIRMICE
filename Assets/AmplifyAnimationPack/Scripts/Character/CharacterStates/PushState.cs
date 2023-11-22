// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;


namespace AmplifyAnimationPack
{

	public class PushState : CharacterStateBase
	{
		private Rigidbody objInteractRigid;

		public override void EnterState()
		{

			charRef.animBeh.anim.SetFloat( "Speed" , 0f );

			charRef.AdjustRotation( Quaternion.AngleAxis( 180f , Vector3.up ) * Quaternion.LookRotation( charRef.objToInteractRotationVector ) , 0.2f );
			charRef.AdjustPosition( charRef.objToInteractContactPoint + charRef.objToInteractRotationVector * 0.51f , 0.2f , true );

			charRef.ChangeMovementType( CharacterMovementTypes.push );
			charRef.ChangePushAction( CharacterPushActions.idle );

			MonoBehaviour auxMono = (MonoBehaviour)charRef.objToInteract;
			objInteractRigid = auxMono.gameObject.GetComponent<Rigidbody>();
			objInteractRigid.isKinematic = false;
		}

		public override void ExitState()
		{
			objInteractRigid.isKinematic = ( objInteractRigid.velocity.y > -1f );
		}

		public override void UpdateState()
		{
		}

		public override void InputMoveForward( float _axisVal )
		{
			charRef.animBeh.anim.SetFloat( "InputFwd" , _axisVal );

			if( objInteractRigid.velocity.y < -1f )
			{
				charRef.ChangePushAction( CharacterPushActions.exit );
			}

		}

		public override void InputInteract( bool _actionVal )
		{
			if( _actionVal )
			{
				charRef.ChangePushAction( CharacterPushActions.exit );
			}
		}
	}

}
