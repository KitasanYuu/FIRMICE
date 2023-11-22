// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;

namespace AmplifyAnimationPack
{

	public class CharacterAnimatorBehavior : MonoBehaviour
	{
		// components
		[HideInInspector]
		public Animator anim;
		private Transform charTransf;
		private CharacterClass charRef;

		public void Setup( Transform _charTransf )
		{
			anim = GetComponent<Animator>();
			charTransf = _charTransf;
			charRef = charTransf.GetComponent<CharacterClass>();
		}

		private void OnAnimatorMove()
		{
			if( charRef.currMovementMode == CharacterMovementModes.animationBased )
			{
				charTransf.position += anim.deltaPosition;
				charTransf.rotation *= anim.deltaRotation;
			}
		}

		public void SetCanMove( bool _isTrue )
		{
			charRef.canMove = _isTrue;

		}

		public void Jump_StartJump()
		{
			charRef.rigid.velocity = Vector3.up * charRef.jumpVelocity + charRef.transf.forward * ( charRef.isJumpingForward ? charRef.jumpForwardVelocity : 0f );

		}

		public void Door_OpenDoor()
		{
			DoorClass doorInteract = (DoorClass)charRef.objToInteract;
			doorInteract.OpenDoor();
		}

		public void Door_CloseDoor()
		{
			DoorClass doorInteract = (DoorClass)charRef.objToInteract;
			doorInteract.CloseDoor();

		}

		public void Lever_PushHandle()
		{
			LeverFloorClass leverInteract = (LeverFloorClass)charRef.objToInteract;
			leverInteract.PushHandle();
		}

		public void Lever_PullHandle()
		{
			LeverFloorClass leverInteract = (LeverFloorClass)charRef.objToInteract;
			leverInteract.PullHandle();

		}

		public void Lever_GoUp()
		{
			LeverWallClass leverInteract = (LeverWallClass)charRef.objToInteract;
			leverInteract.HandleGoUp();
		}

		public void Lever_GoDown()
		{
			LeverWallClass leverInteract = (LeverWallClass)charRef.objToInteract;
			leverInteract.HandleGoDown();

		}

		public void Picking_PickObject()
		{
			PickableClass pickableInteract = (PickableClass)charRef.objToInteract;
			pickableInteract.PickObject();

		}

		public void Vault_EnableCollider()
		{
			charRef.baseColl.enabled = true;

		}

		public void Attack_CanComboOn()
		{
			anim.SetBool( "canComboAttack" , true );
		}

		public void Attack_CanComboOff()
		{
			anim.SetBool( "canComboAttack" , false );
		}

		public void Attack_HitboxOn()
		{
			charRef.hitbox.SetActive( true );
		}

		public void Attack_HitboxOff()
		{
			charRef.hitbox.SetActive( false );
		}

		public void BarHangClimb_AdjustPosition()
		{
			charRef.AdjustPosition( charRef.transf.position + charRef.transf.right * 0.2f , 0.1f , true );

		}

		public void Roll_EnableCanFall()
		{
			charRef.canFallOffRoll = true;
		}

		public void Object_ThrowObject()
		{
			charRef.objectCaught.transform.position = charRef.animBeh.anim.GetBoneTransform( HumanBodyBones.RightHand ).position;
			charRef.objectCaught.SetActive( true );

			charRef.objectCaught.GetComponent<Rigidbody>().isKinematic = false;
			charRef.objectCaught.GetComponent<Rigidbody>().AddForce( charRef.transf.forward *
				 ( ( charRef.currMovementType == CharacterMovementTypes.cover || charRef.currMovementType == CharacterMovementTypes.coverCrouch ) ? -1f : 1f )
				* 500f + Vector3.up * 400f );

			charRef.objectCaught = null;

		}

		public void Plunge_Impact()
		{
			charRef.hitbox.SetActive( true );
			charRef.CameraShakeEvent_Start( 0.5f , 1.45f, 9f );
			
		}

		

	}
}