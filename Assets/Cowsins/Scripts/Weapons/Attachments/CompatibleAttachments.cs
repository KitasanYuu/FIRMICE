namespace cowsins {

/// <summary>
/// Stores the compatible attachments of a WeaponIdentification object.
/// </summary>
[System.Serializable]
public class CompatibleAttachments 
{
    public Barrel[] barrels;
    public Scope[] scopes;
    public Stock[] stocks;
    public Grip[] grips;
    public Magazine[] magazines;
    public Flashlight[] flashlights;
    public Laser[] lasers;
}
}