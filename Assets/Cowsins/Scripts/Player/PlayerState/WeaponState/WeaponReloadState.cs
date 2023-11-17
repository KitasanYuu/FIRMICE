namespace cowsins {
public class WeaponReloadState : WeaponBaseState
{
    private WeaponController controller;

    private PlayerStats stats; 
    public WeaponReloadState(WeaponStates currentContext, WeaponStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory) { }

    public override void EnterState()
    {
        controller = _ctx.GetComponent<WeaponController>();
        stats = _ctx.GetComponent<PlayerStats>();
        controller.StartReload();
    }

    public override void UpdateState() {
        CheckSwitchState();
        if (!stats.controllable) return;
        CheckStopAim(); 
    }

    public override void FixedUpdateState()
    {
    }

    public override void ExitState() { }

    public override void CheckSwitchState() {
        if(!controller.Reloading)SwitchState(_factory.Default());
    }

    public override void InitializeSubState() { }

    private void CheckStopAim()
    {
        if (!InputManager.aiming) controller.StopAim();
    }

}
}