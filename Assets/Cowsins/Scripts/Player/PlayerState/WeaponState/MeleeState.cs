using UnityEngine;
namespace cowsins { 
public class MeleeState : WeaponBaseState
{
    private WeaponController controller;

    private float timer; 
    public MeleeState(WeaponStates currentContext, WeaponStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory) { }

    public override void EnterState()
    {
        controller = _ctx.GetComponent<WeaponController>(); 
        timer = 0;
        controller.SecondaryMeleeAttack(); 
    }

    public override void UpdateState() {
        controller.StopAim(); 
        CheckSwitchState();
    }

    public override void FixedUpdateState()
    {
    }

    public override void ExitState() {
        controller.FinishMelee(); 
    }

    public override void CheckSwitchState() {
        timer += Time.deltaTime;  

        if(timer >= controller.meleeDuration + controller.meleeDelay) SwitchState(_factory.Default());
    }

    public override void InitializeSubState() { }
}
}