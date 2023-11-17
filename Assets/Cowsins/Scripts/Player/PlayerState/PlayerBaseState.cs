namespace cowsins {
public abstract class PlayerBaseState
{
    protected PlayerStates _ctx;
    protected PlayerStateFactory _factory;
    protected PlayerBaseState _currentSubState, _currentSuperState; 

    public PlayerBaseState(PlayerStates currentContext, PlayerStateFactory playerStateFactory) {
        _ctx = currentContext;
        _factory = playerStateFactory; 
    }

 
    public abstract void EnterState();

    public abstract void UpdateState();

    public abstract void FixedUpdateState();

    public abstract void ExitState();

    public abstract void CheckSwitchState();

    public abstract void InitializeSubState();

    void UpdateStates() { }

    protected void SwitchState(PlayerBaseState newState)
    {
        ExitState();

        newState.EnterState();

        _ctx.CurrentState = newState; 
    }

    protected void SetSuperState(PlayerBaseState newSuperState) {
        _currentSuperState = newSuperState;    
    }

    protected void SetSubState(PlayerBaseState newSubState) {
        _currentSubState = newSubState;
        newSubState.SetSuperState(this); 
    }
}
}