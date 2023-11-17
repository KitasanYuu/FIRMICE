namespace cowsins {
using UnityEngine;
public class WeaponInspectState : WeaponBaseState
{
    private WeaponController controller;

    private InteractManager interact;

    private PlayerStats stats;

    private float timer; 
    public WeaponInspectState(WeaponStates currentContext, WeaponStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory) { }

    public override void EnterState()
    {
        controller = _ctx.GetComponent<WeaponController>();
        interact = _ctx.GetComponent<InteractManager>(); 
        stats = _ctx.GetComponent<PlayerStats>();

        controller.InitializeInspection();

        timer = 0;
        interact.inspecting = true;

        if (!interact.realtimeAttachmentCustomization) return; 

        _ctx.inspectionUI.gameObject.SetActive(true);
        _ctx.inspectionUI.alpha = 0;  

        interact.GenerateInspectionUI();

        UnlockMouse();
    }


    public override void UpdateState()
    {

        if(interact.realtimeAttachmentCustomization) stats.LoseControl();

        if(timer <= 1) timer += Time.deltaTime; 

        controller.StopAim();

        CheckSwitchState();
    }

    public override void FixedUpdateState() { }
    public override void InitializeSubState() { }
    public override void ExitState()
    {

        interact.inspecting = false;
        controller.DisableInspection();
        stats.CheckIfCanGrantControl();

        LockMouse();

        UIEvents.onEnableAttachmentUI?.Invoke(null); 
    }
    public override void CheckSwitchState()
    {
            if (InputManager.inspecting && timer >= 1) SwitchState(_factory.Default()); 
    }

    private void UnlockMouse()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void LockMouse()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
}