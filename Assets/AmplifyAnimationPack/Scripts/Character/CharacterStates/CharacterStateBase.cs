// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>


namespace AmplifyAnimationPack
{

	public abstract class CharacterStateBase
	{
		// character referenc
		public CharacterClass charRef;
		public CharacterStateBase prevState;

		// state functions
		public abstract void EnterState();
		public abstract void ExitState();
		public abstract void UpdateState();
		public virtual void UpdateStatePhysics() { }
		public virtual void LateUpdateState() { }

		// input functions
		public virtual void InputMoveForward( float _axisVal ) { }
		public virtual void InputMoveRight( float _axisVal ) { }
		public virtual void InputSprint( bool _actionVal ) { }
		public virtual void InputCrouch( bool _actionVal ) { }
		public virtual void InputJump( bool _actionVal ) { }
		public virtual void InputInteract( bool _actionVal ) { }
		public virtual void InputAttack( bool _actionVal , CharacterCombatActions _newAttack ) { }
		public virtual void InputDodge( bool _actionVal ) { }
		public virtual void InputLockOn( bool _actionVal ) { }

	}
}
