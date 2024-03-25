using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace AvatarMain
{
	public class BasicInput : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
        public bool aim;
        public bool shoot;
        public bool swa;
        public bool roll;



        [Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

        private void Start()
        {
            SetCursorState(cursorLocked);
        }

#if ENABLE_INPUT_SYSTEM
        public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
			else
			{
				LookInput(new Vector2(0,0));
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}

        public void OnAim(InputValue value)
        {
           AimInput(value.isPressed);
        }

        public void OnShoot(InputValue value)
        {
            ShootInput(value.isPressed);
        }

        public void OnSWA(InputValue value)
        {
           SWAInput(value.isPressed);
        }

        public void OnRoll(InputValue value)
        {
            RollInput(value.isPressed);
        }
#endif


        public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}
        public void AimInput(bool newAimState)
        {
            aim = newAimState;
        }
        public void ShootInput(bool newShootState)
        {
            shoot = newShootState;
        }
        public void SWAInput(bool newSWA)
        {
            swa = newSWA;
        }

        public void RollInput(bool newRoll)
        {
            roll = newRoll;
        }

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
			cursorInputForLook = true;

        }

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}

		// 在 StarterAssetsInputs 类中
		public bool crouch
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current.cKey.isPressed; // 使用适当的输入系统代码
#else
                return Input.GetKey(KeyCode.C);
#endif
            }
        }

    }

}