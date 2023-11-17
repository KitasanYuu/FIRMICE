namespace cowsins {
public class WeaponStateFactory
{
    WeaponStates _context; 

    public WeaponStateFactory(WeaponStates currentContext) {_context = currentContext;}

    public WeaponBaseState Default(){ return new WeaponDefaultState(_context, this); }

    public WeaponBaseState Reload() { return new WeaponReloadState(_context, this); }
    
    public WeaponBaseState Shoot() { return new WeaponShootingState(_context, this); }

    public WeaponBaseState Melee() { return new MeleeState(_context, this); }

    public WeaponBaseState Inspect() { return new WeaponInspectState(_context, this);  }
}
}