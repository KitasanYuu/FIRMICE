namespace cowsins {
public class WeaponShootingState : WeaponBaseState
{
    private WeaponController controller;

    private PlayerStats stats; 

    public WeaponShootingState(WeaponStates currentContext, WeaponStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory) { }

    public override void EnterState() {

        controller = _ctx.GetComponent<WeaponController>();
        stats = _ctx.GetComponent<PlayerStats>(); 
    }

    public override void UpdateState() {
        if (controller.weapon == null) return;
        CheckSwitchState();
        if (!stats.controllable) return;
        CheckAim();
    }

    public override void FixedUpdateState()
    {
    }

    public override void ExitState() {
        controller.performShootStyle?.Invoke();
    }

    public override void CheckSwitchState() {
        SwitchState(_factory.Default());
        if (controller.weapon.infiniteBullets) return;
        if (InputManager.reloading && stats.controllable && !controller.shooting) SwitchState(_factory.Reload()); 
    }

    public override void InitializeSubState() { }

    private void CheckAim()
    {
        if (InputManager.aiming && controller.weapon.allowAim)controller.Aim();
        CheckStopAim();
    }
    private void CheckStopAim()
    {
        if (!InputManager.aiming) controller.StopAim();
    }
}
}