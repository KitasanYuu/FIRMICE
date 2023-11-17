using UnityEngine;
namespace cowsins {
public class ProceduralShot : MonoBehaviour
{
    [SerializeField] private WeaponController weapon;

    private ProceduralShot_SO pattern;

    static ProceduralShot _instance;
    public static ProceduralShot Instance { get { return _instance; } }

    private bool shoot = false;

    private float timer;

    private float x, y, z;

    private float xRot, yRot, zRot;

    private void Awake() => _instance = this;

    private void Update()
    {
        if (!shoot || timer >= 1) return; // Return if we are not shooting

        timer += Time.deltaTime * pattern.playSpeed; // Increase the timer

        // Evaluate positions
        x = pattern.translation.xTranslation.Evaluate(timer);
        y = pattern.translation.yTranslation.Evaluate(timer);
        z = pattern.translation.zTranslation.Evaluate(timer);

        // Evaluate rotations
        xRot = pattern.rotation.xRotation.Evaluate(timer);
        yRot = pattern.rotation.yRotation.Evaluate(timer);
        zRot = pattern.rotation.zRotation.Evaluate(timer);

        // Get the aiming multipliers depending on the state of the WeaponController
        float aimingTransl = weapon.isAiming && pattern != null ? pattern.aimingTranslationMultiplier : 1;
        float aimingRot = weapon.isAiming && pattern != null ? pattern.aimingRotationMultiplier : 1;

        // Apply the motions
        transform.localPosition = new Vector3(x* pattern.translationDistance.x, y * pattern.translationDistance.y, z* pattern.translationDistance.z) * aimingTransl;
        transform.localRotation = Quaternion.Euler(new Vector3(xRot * pattern.rotationDistance.x, yRot * pattern.rotationDistance.y, zRot * pattern.rotationDistance.z) * aimingRot); 
    }
    /// <summary>
    /// Start a Procedural Shot motion given a ProceduralShot_SO ( Procedural Shot Pattern )
    /// </summary>
    public void Shoot(ProceduralShot_SO shot)
    {
        pattern = shot; 
        timer = 0;
        shoot = true;
    }
}
}