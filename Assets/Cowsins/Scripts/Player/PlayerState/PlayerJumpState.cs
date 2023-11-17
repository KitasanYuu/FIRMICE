using UnityEngine;
namespace cowsins {
public class PlayerJumpState : PlayerBaseState
{
    public PlayerJumpState(PlayerStates currentContext, PlayerStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory) { }

    private PlayerMovement player;

    private PlayerStats stats; 

    public override void EnterState() {
        player = _ctx.GetComponent<PlayerMovement>();
        stats = _ctx.GetComponent<PlayerStats>();
        player.events.OnJump.Invoke();
        player.Jump();
    }

    public override void UpdateState() {
        CheckSwitchState(); 
        HandleMovement();
        CheckUnCrouch(); 
    }

    public override void FixedUpdateState() { }

    public override void ExitState() {}

    public override void CheckSwitchState() {

        if (player.ReadyToJump && InputManager.jumping && (player.CanJump && player.grounded || player.wallRunning || player.jumpCount > 0 && player.maxJumps > 1 && player.CanJump))
            SwitchState(_factory.Jump());

        if (stats.health <= 0) SwitchState(_factory.Die());

        if (player.grounded || player.wallRunning )
           SwitchState(_factory.Default());
        if (player.canDash && InputManager.dashing && (player.infiniteDashes || player.currentDashes > 0 && !player.infiniteDashes)) SwitchState(_factory.Dash());

        if (InputManager.crouchingDown && !player.wallRunning)
        {
            if (player.allowCrouchWhileJumping) SwitchState(_factory.Crouch());
        }
    }

    public override void InitializeSubState() { }

    void HandleMovement()
    {
        player.Movement(stats.controllable);
        player.Look();
    }

    private bool canUnCrouch = false;

    private void CheckUnCrouch()
    {

        RaycastHit hitt;
        if (!InputManager.crouching) // Prevent from uncrouching when there´s a roof and we can get hit with it
        {
            if (Physics.Raycast(_ctx.transform.position, _ctx.transform.up, out hitt, 5.5f, player.weapon.hitLayer))
            {
                canUnCrouch = false;
            }
            else
                canUnCrouch = true;
        }
        if (canUnCrouch)
        {
            player.events.OnStopCrouch.Invoke(); // Invoke your own method on the moment you are standing up NOT WHILE YOU ARE NOT CROUCHING
            player.StopCrouch();
        }
    }
}
}