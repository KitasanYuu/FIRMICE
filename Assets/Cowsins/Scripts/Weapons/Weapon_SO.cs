/// <summary>
/// This script belongs to cowsins� as a part of the cowsins� FPS Engine. All rights reserved. 
/// </summary>

using UnityEngine;
using System;
namespace cowsins {

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Presets;
#endif

#region enum
/// <summary>
/// SHOOTING STYLE ENUMERATOR
/// </summary>
public enum ShootStyle
{
    Hitscan,
    Projectile,
    Melee,
    Custom
};

public enum ShootingMethod
{
    Press,
    PressAndHold,
    HoldAndRelease,
    HoldUntilReady
};


public enum AmmoStyle
{
    pistolRounds, smgRounds, shotgunShells, rifleBullets, sniperBullets, rockets
};

public enum ReloadingStyle
{
    defaultReload, Overheat
};
#endregion

#region others
[System.Serializable]
public class AudioClips
{
    public AudioClip firing, holster, unholster, reload, emptyMagReload, emptyMagShoot;
}
[System.Serializable]
public class BulletHoleImpact
{
    public GameObject defaultImpact, groundIMpact, grassImpact, enemyImpact, metalImpact, mudImpact, woodImpact;
}
#endregion

#region weaponScriptableObject
[CreateAssetMenu(fileName = "NewWeapon", menuName = "COWSINS/New Weapon", order = 1)]
public class Weapon_SO : ScriptableObject
{
    [Tooltip("Attach your weapon prefab here. This weapon prefab will be instantiated on your screen when you own this weapon.")]public WeaponIdentification weaponObject;

    [Tooltip("You weapon�s name. Ex: Glock")] public string _name;

    [Tooltip("Visuals that will appear on a dropped weapon.")] public GameObject pickUpGraphics;

    [Tooltip("Custom image of your weapon")] public Sprite icon;

    [Tooltip("Type of shooting. Hitscan = Instant hit on shooting." +
        "Projectile = spawn a bullet that travels through the world." +
        "Melee: Close range weapons such as swords or knives.")]
    public ShootStyle shootStyle;

    [Tooltip("Defines how Input is processed to shoot.")]
    public ShootingMethod shootMethod;

    [Tooltip("In order to shoot, you need to have a 100% progress value. Define how fast you want to reach 100% here.")] public float holdProgressSpeed;

    //[Tooltip("Type of ammunation weapon will use. Set it now for future updates. ")] public AmmoStyle ammoStyle;

    [Tooltip("The way the weapon reloads. Set it now for future updates")] public ReloadingStyle reloadStyle;

    [Tooltip("Your bullet objects")] public Bullet projectile;

    [Tooltip(" While true, the bullet will draw a parabola. ")] public bool projectileUsesGravity;

    [Tooltip("time to instantiate the projectile sicne you press shoot. Keep 0 if you want shooting to be instantly"), Min(0)] public float shootDelay; 

    [Tooltip("Lifetime of the bullet")] public float bulletDuration;

    [Tooltip("Bullet velocity")] public float speed;

    [Tooltip("Does it explode when hits a target? If so, damage will depend on the distance between the target and the hit point")] public bool explosionOnHit;

    [Tooltip("Toggles the capacity of hurting youtself")] public bool hurtsPlayer;

    [Tooltip("Max reach of the explosion")] public float explosionRadius;

    [Min(0), Tooltip("Force applied to rigidbodies on explosion")] public float explosionForce;

    [Tooltip("VFX on explosion")] public GameObject explosionVFX;

    public bool continuousFire;

    [Tooltip("Time between each shot")] public float fireRate;

    [Tooltip("Time you will have to wait for the weapon to reload")] public float reloadTime;

    public float coolSpeed;

    [Range(.01f,.99f)]public float cooledPercentageAfterOverheat; 

    [Tooltip("Turn it off to set a magazine size")] public bool infiniteBullets;

    [Tooltip("How many bullets you can have per magazine")] public int magazineSize;

    [Tooltip("Bullets instantiated per each shot. Grants the possibility of making shotguns, burstguns etc. Amount of bullets spawned every shot.")]

    public int bulletsPerFire; // Grants the possibility of making shotguns, burstguns etc. Amount of bullets spawned every shot.

    [Tooltip("How much ammo you lose per each fire ( & per each fire point )"), Min(0)]

    public int ammoCostPerFire;

    [Tooltip("How far the bullet is able to travel")] public int bulletRange;

    [Tooltip("Time elapsed until the next bullet spawns. Keep this at 0" +
        "in order to make shotguns." +
        "Change its value in order to make burst weapons.")]
    public float timeBetweenShots;

    // if bulletsPerFire != 1, then how much time do you wanna get for the next bullet to spawn ( while bullets spawned < bulletsPerFire ) 

    [Tooltip("How much the bullet is able to penetrate an object")] [Range(0, 10)] public float penetrationAmount;

    [Tooltip("Damage reduction multiplier. %/100. .8f means 80%")] [Range(0, 1)] public float penetrationDamageReduction;

    [Tooltip("While true it grants the possibility of aiming the weapon")] public bool allowAim;

    [Tooltip("Added position while aiming")] public Vector3 aimingPosition = new Vector3(-1.5f, .5f, 0);

    [Tooltip("Final Rotation aiming")] public Vector3 aimingRotation;

    [Tooltip("Velocity to change between not aiming and aiming states")] [Range(1, 50)] public float aimingSpeed;

    [Tooltip("Camera field of view on aiming")] [Range(1, 179)] public float aimingFOV;

    [Tooltip("Player Movement Speed while Aiming"), Min(.1f)] public float movementSpeedWhileAiming;

    [Tooltip("Sets a specific speed while aiming if true")] public bool setMovementSpeedWhileAiming;

    [Tooltip("Resize crosshair on shooting")] public float crosshairResize;

#if UNITY_EDITOR
    [Tooltip("Attach the preset for the crosshair yo want to display while using this weapon. it MUST be a CrosshairShape Preset")] public Preset crosshairPreset;
#endif


    [Tooltip("Apply spread per shot")] public bool applyBulletSpread;

    [Tooltip("Velocity will be decreased depending on the weight of the weapon if this is true.")] public bool applyWeight;

    [Tooltip("How much spread is applied.")] [Range(0, 2)] public float spreadAmount;

    [Tooltip("How much spread is applied while aiming.")] [Range(0, 2)] public float aimSpreadAmount;

    [Range(1, 3)]
    [Tooltip("Multiplier to increase or decrease your speed while holding the weapon")] public float weightMultiplier = 1f;
    // Multiplier to increase or decrease your speed while holding the weapon

    [Tooltip("Apply recoil on shooting")] public bool applyRecoil;

    public AnimationCurve recoilX, recoilY;

    public float xRecoilAmount, yRecoilAmount;

    public float recoilRelaxSpeed;

    public bool applyDifferentRecoilOnAiming;

    public float xRecoilAmountOnAiming, yRecoilAmountOnAiming;

    [Tooltip("Damage dealt per bullet")] public float damagePerBullet;

    [Tooltip("Damage will decrease or increase depending on how far you are from the target")] public bool applyDamageReductionBasedOnDistance;

    [Tooltip("Damage reduction will be applied for distances larger than this"), Min(0)] public float minimumDistanceToApplyDamageReduction;

    [Tooltip("Adjust the damage reduction amount"), Range(.1f, 1)] public float damageReductionMultiplier;

    [Range(1, 2)] [Tooltip("Damage will get multiplied by this number when hitting a critical shot")] public float criticalDamageMultiplier;

    [Tooltip("Turn this true for a more realistic approach. It will stop using infinite magazines, so you will have to pick new magazines to add more bullets.")] public bool limitedMagazines;

    [Tooltip("Amount of initial magazines. (Full magazines, meaning that 2 magazines of 10 bullets each will result in having 20 initial bullets you can dispose o f)")] [Range(1, 100)] public int totalMagazines;

    [Tooltip("Spawn bullet shells to add realism")] public bool showBulletShells;

    [Tooltip("Graphics")] public BulletHoleImpact bulletHoleImpact;

    public GameObject muzzleVFX, bulletGraphics;

    public bool useProceduralShot;

    public ProceduralShot_SO proceduralShotPattern;

    public AudioClips audioSFX;

    [Tooltip("Per each shot, pitch will be modified by a number between -pitchVariationFiringSFX and +pitchVariationFiringSFX"), Range(0, .2f)] public float pitchVariationFiringSFX;

    [Tooltip("Cam Shake Amount on shoot"), Min(0)] public float camShakeAmount;

    [Tooltip(" If true it will apply a different FOV value when shooting to add an extra layer of detail. FOV will automatically lerp until it reaches its default value.")] public bool applyFOVEffectOnShooting;

    [Tooltip("FOV amount subtracted from the current fov on shooting"), Range(0, 180)] public float FOVValueToSubtract;

    [Tooltip("FOV amount subtracted from the current fov on shooting"), Range(0, 180)] public float AimingFOVValueToSubtract;

    //Melee Exclusive
    [Tooltip("Damage the melee weapon deals per hit")] [Range(0, 1000)] public float damagePerHit;

    [Range(0, 6),Tooltip("Attacking rythm")] public float attackRate;

    [Range(0,2),Tooltip("Time to delay the melee hit")] public float hitDelay; 

    [Range(0, 5)]
    [Tooltip("How far you are able to land a hit")] public float attackRange;

    [HideInInspector] public bool dontShowMagazine;

#if UNITY_EDITOR
    public Preset currentPreset;
#endif

    public string presetName;


}
#endregion

#if UNITY_EDITOR
#region customEditor
/// <summary>
/// CUSTOM EDITOR STUFF
/// </summary>
[System.Serializable]
[CustomEditor(typeof(Weapon_SO))]
public class Weapon_SOEditor : Editor
{
    private string[] tabs = { "Basic", "Shooting", "Stats", "Visuals", "Audio", "UI" };
    private int currentTab = 0;

    override public void OnInspectorGUI()
    {
        serializedObject.Update();
        Weapon_SO myScript = target as Weapon_SO;

        Texture2D myTexture = Resources.Load<Texture2D>("CustomEditor/weaponObject_CustomEditor") as Texture2D;
        GUILayout.Label(myTexture);

        EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(10f);
            if (GUILayout.Button("Adding New Weapons Tutorial", GUILayout.Height(20)))
            {
                Application.OpenURL("https://youtu.be/KoZu1D2gnR4");
            }
            EditorGUILayout.Space(10f);
        currentTab = GUILayout.Toolbar(currentTab, tabs);
        EditorGUILayout.Space(10f);
        EditorGUILayout.EndVertical();

        int style = (int)myScript.shootStyle;

        if (currentTab >= 0 || currentTab < tabs.Length)
        {
            switch (tabs[currentTab])
            {
                case "Basic":
                    EditorGUILayout.LabelField("BASIC SETTINGS", EditorStyles.boldLabel); 
                    GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_name"));
                    EditorGUILayout.LabelField("This represents your first-person weapon in the game.", EditorStyles.helpBox);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponObject"));
                    EditorGUILayout.LabelField("This represents the graphics of your weapon on the ground.", EditorStyles.helpBox);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("pickUpGraphics"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("icon"));
                    EditorGUILayout.Space(20f);
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();

                        Texture2D tex = AssetPreview.GetAssetPreview(myScript.icon);
                        GUILayout.Label("", GUILayout.Height(50), GUILayout.Width(50));
                        GUI.DrawTexture(GUILayoutUtility.GetLastRect(), tex);

                        GUILayout.FlexibleSpace();
                    }
                    EditorGUILayout.EndHorizontal();
                        
                    break;
                case "Shooting":
                    EditorGUILayout.LabelField("Shooting Settings", EditorStyles.boldLabel);
                    GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        if(myScript.shootStyle == ShootStyle.Custom)
                        {
                            EditorGUILayout.Space(10f);
                            if (GUILayout.Button("Custom Shot Weapons Tutorial", GUILayout.Height(20)))
                            {
                                Application.OpenURL("https://youtu.be/OJ6uPo66Ams");
                            }
                            EditorGUILayout.Space(10f);
                        }
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("shootStyle"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("shootMethod"));

                    if (myScript.shootMethod == ShootingMethod.HoldUntilReady || myScript.shootMethod == ShootingMethod.HoldAndRelease)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("holdProgressSpeed"));
                        EditorGUI.indentLevel--;
                    }
                    switch (style)
                    {
                        case 0:
                            WeaponShootingSharedVariables(myScript);
                            break;
                        case 1:
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PrefixLabel("Attach your Projectile here", EditorStyles.helpBox);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("projectile"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("projectileUsesGravity"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("speed"));
                                if (myScript.shootDelay > myScript.fireRate) myScript.shootDelay = myScript.fireRate;
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("shootDelay"));   
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("bulletDuration"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("hurtsPlayer"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("explosionOnHit"));
                            if (myScript.explosionOnHit)
                            {
                                EditorGUI.indentLevel++;
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("explosionRadius"));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("explosionForce"));
                                EditorGUILayout.LabelField("Check new options under 'Visuals' tab. ", EditorStyles.helpBox);
                                EditorGUI.indentLevel--;
                            }
                            WeaponShootingSharedVariables(myScript);
                            EditorGUILayout.Space(5f);
                            EditorGUI.indentLevel--;
                            break;
                        case 2:
                            EditorGUILayout.LabelField("Melee Options", EditorStyles.boldLabel);
                            EditorGUILayout.Space(2f);
                            var attackRangeProperty = serializedObject.FindProperty("attackRange");
                            EditorGUILayout.PropertyField(attackRangeProperty);
                            var attackRateProperty = serializedObject.FindProperty("attackRate");
                            EditorGUILayout.PropertyField(attackRateProperty);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("hitDelay"));
                            break;
                        case 3:
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("continuousFire"));
                            if (!myScript.continuousFire)
                            {
                                EditorGUI.indentLevel++;
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("fireRate"));
                                EditorGUI.indentLevel--;
                            }
                            else EditorGUILayout.LabelField("Continuous Fire will call your custom method ONCE per frame.", EditorStyles.helpBox);
                            break;
                    }
                    break;
                case "Stats":
                    switch (style)
                    {
                        case 0:
                            WeaponStatsSharedVariables(myScript);
                            break;
                        case 1:
                            WeaponStatsSharedVariables(myScript);
                            break;
                        case 2:
                            var damagePerHitProperty = serializedObject.FindProperty("damagePerHit");
                            EditorGUILayout.PropertyField(damagePerHitProperty);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("criticalDamageMultiplier"));
                            break;
                    }
                    break;
                case "Visuals":
                    switch (style)
                    {
                        case 0:
                            EditorGUILayout.Space(5f);
                            EditorGUILayout.LabelField("Visuals", EditorStyles.boldLabel);
                            EditorGUILayout.LabelField("Procedural Animations");
                            GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("useProceduralShot"));
                            if (myScript.useProceduralShot)
                            {
                                EditorGUI.indentLevel++;
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("proceduralShotPattern"));
                                EditorGUI.indentLevel--;
                            }
                            EditorGUILayout.Space(5f);
                            WeaponVisualSharedVariables(myScript);
                            break;
                        case 1:
                            EditorGUILayout.Space(5f);
                            EditorGUILayout.LabelField("Visuals", EditorStyles.boldLabel);
                            EditorGUILayout.LabelField("Procedural Animations");
                            GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("useProceduralShot"));
                            if (myScript.useProceduralShot)
                            {
                                EditorGUI.indentLevel++;
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("proceduralShotPattern"));
                                EditorGUI.indentLevel--;
                            }
                            EditorGUILayout.Space(5f);
                            WeaponVisualSharedVariables(myScript);
                            if (myScript.explosionOnHit)
                            {
                                EditorGUI.indentLevel++;
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("explosionVFX"));
                                EditorGUI.indentLevel--;
                            }
                            break;
                        case 2:
                            EditorGUILayout.Space(10f);
                            EditorGUILayout.LabelField("EFFECTS", EditorStyles.boldLabel);
                            EditorGUILayout.Space(2f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("camShakeAmount"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("applyFOVEffectOnShooting"));
                            if (myScript.applyFOVEffectOnShooting)
                            {
                                EditorGUI.indentLevel++;
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("FOVValueToSubtract"));
                                EditorGUI.indentLevel--;
                            }
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("bulletHoleImpact"));

                            EditorGUILayout.Space(10f);
                            break;
                    }
                    break;
                case "Audio":
                    WeaponAudioSharedVariables(myScript);
                    break;

                case "UI":
                    WeaponUISharedVariables(myScript);
                    if (style == 2) myScript.allowAim = false;

                    break;

            }

            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("PRESETS", EditorStyles.boldLabel);
            GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
            EditorGUILayout.Space(2f);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("currentPreset"));
            EditorGUILayout.Space(2f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("presetName"));
            EditorGUILayout.Space(5f);
            if (GUILayout.Button("Save Settings as a Preset")) CowsinsUtilities.SavePreset(myScript, myScript.presetName);

            EditorGUILayout.Space(5f);

            if (GUILayout.Button("Apply Current Preset"))
            {
                if (myScript.currentPreset != null) CowsinsUtilities.ApplyPreset(myScript.currentPreset, myScript);
                else Debug.LogError("Can�t apply a non existing preset. Please, assign your desired preset to 'currentPreset'. ");
            }
        }
        if ((int)myScript.reloadStyle == 1) myScript.dontShowMagazine = true;
        else myScript.dontShowMagazine = false;


        EditorGUILayout.Space(20f);
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.FlexibleSpace();

            // LOCK ICON 

            GUI.backgroundColor = Color.clear;

            btnTexture = ActiveEditorTracker.sharedTracker.isLocked
                ? Resources.Load<Texture2D>("CustomEditor/lockedIcon") as Texture2D
                : Resources.Load<Texture2D>("CustomEditor/unlockedIcon") as Texture2D;

            button = new GUIContent(btnTexture);

            if (GUILayout.Button(button, GUIStyle.none, GUILayout.Width(lockIconSize), GUILayout.Height(lockIconSize)))
                ActiveEditorTracker.sharedTracker.isLocked = !ActiveEditorTracker.sharedTracker.isLocked;

            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndHorizontal();



        serializedObject.ApplyModifiedProperties();
    }
    private float lockIconSize = 40;
    private Texture btnTexture;
    private GUIContent button;
    private void WeaponUISharedVariables(Weapon_SO myScript)
    {
        EditorGUILayout.Space(20f);
        EditorGUILayout.LabelField("User Interface (UI)", EditorStyles.boldLabel);
        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
        EditorGUILayout.Space(5f);

        var crosshairResizeProperty = serializedObject.FindProperty("crosshairResize");
        EditorGUILayout.PropertyField(crosshairResizeProperty);

            
        if(myScript.crosshairPreset == null)
            EditorGUILayout.LabelField("Crosshair Preset cannot be null", EditorStyles.helpBox);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("crosshairPreset"));

    }

    private void WeaponAudioSharedVariables(Weapon_SO myScript)
    {

        EditorGUILayout.LabelField("Audio", EditorStyles.boldLabel);
        EditorGUILayout.Space(2f);
        var audioSFXProperty = serializedObject.FindProperty("audioSFX");
        EditorGUILayout.PropertyField(audioSFXProperty);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("pitchVariationFiringSFX"));
        EditorGUILayout.Space(10f);
    }

    private void WeaponShootingSharedVariables(Weapon_SO myScript)
    {
        EditorGUILayout.Space(5f);

        if (myScript.shootMethod != ShootingMethod.HoldUntilReady && myScript.shootMethod != ShootingMethod.HoldAndRelease)
        {
            EditorGUI.indentLevel++;
            var fireRateProperty = serializedObject.FindProperty("fireRate");
            EditorGUILayout.PropertyField(fireRateProperty);
            EditorGUI.indentLevel--;
        }



        var bulletRangeProperty = serializedObject.FindProperty("bulletRange");
        EditorGUILayout.PropertyField(bulletRangeProperty);

        var bulletsPerFireProperty = serializedObject.FindProperty("bulletsPerFire");
        EditorGUILayout.PropertyField(bulletsPerFireProperty);

        if (myScript.bulletsPerFire > 1)
        {
            EditorGUI.indentLevel++;
            var timeBetweenShotsProperty = serializedObject.FindProperty("timeBetweenShots");
            EditorGUILayout.PropertyField(timeBetweenShotsProperty);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ammoCostPerFire"));
        if (myScript.ammoCostPerFire > myScript.bulletsPerFire) myScript.ammoCostPerFire = myScript.bulletsPerFire;

        var applyBulletSpreadProperty = serializedObject.FindProperty("applyBulletSpread");
        EditorGUILayout.PropertyField(applyBulletSpreadProperty);

        using (var group = new EditorGUILayout.FadeGroupScope(Convert.ToSingle(myScript.applyBulletSpread)))
        {

            if (group.visible == true)
            {
                EditorGUI.indentLevel++;
                var spreadAmountProperty = serializedObject.FindProperty("spreadAmount");
                EditorGUILayout.PropertyField(spreadAmountProperty);
                if (myScript.allowAim) EditorGUILayout.PropertyField(serializedObject.FindProperty("aimSpreadAmount"));
                EditorGUI.indentLevel--;
            }
        }
        if (!myScript.applyBulletSpread) myScript.spreadAmount = 0;

        EditorGUILayout.PropertyField(serializedObject.FindProperty("applyRecoil"));

        if (myScript.applyRecoil)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("recoilX"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("recoilY"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("xRecoilAmount"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("yRecoilAmount"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("recoilRelaxSpeed"));
            if (myScript.allowAim)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("applyDifferentRecoilOnAiming"));
                if (myScript.applyDifferentRecoilOnAiming)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("xRecoilAmountOnAiming"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("yRecoilAmountOnAiming"));
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUI.indentLevel--;
        }

        if (myScript.shootStyle != ShootStyle.Projectile)
        {
            var penetrationAmountProperty = serializedObject.FindProperty("penetrationAmount");
            EditorGUILayout.PropertyField(penetrationAmountProperty);
            if (myScript.penetrationAmount != 0)
            {
                EditorGUI.indentLevel++;
                var penetrationDamageReductionProperty = serializedObject.FindProperty("penetrationDamageReduction");
                EditorGUILayout.PropertyField(penetrationDamageReductionProperty);
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
        }
    }

    private void WeaponStatsSharedVariables(Weapon_SO myScript)
    {
        EditorGUILayout.LabelField("Weapon Stats", EditorStyles.boldLabel);
        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
        EditorGUILayout.Space(5f);


        if (myScript.dontShowMagazine == false)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("infiniteBullets"));
            if (!myScript.infiniteBullets)
            {
                EditorGUI.indentLevel++;
                var magazineSizeProperty = serializedObject.FindProperty("magazineSize");
                EditorGUILayout.PropertyField(magazineSizeProperty);

                var limitedMagazinesProperty = serializedObject.FindProperty("limitedMagazines");
                EditorGUILayout.PropertyField(limitedMagazinesProperty);
                EditorGUI.indentLevel--;
                using (var group = new EditorGUILayout.FadeGroupScope(Convert.ToSingle(myScript.limitedMagazines)))
                {

                    if (group.visible == true)
                    {
                        EditorGUI.indentLevel++;
                        var totalMagazinesProperty = serializedObject.FindProperty("totalMagazines");
                        EditorGUILayout.PropertyField(totalMagazinesProperty);
                        EditorGUI.indentLevel--;
                    }
                }
            }
        }
        else
        {
            EditorGUI.indentLevel++;
            myScript.limitedMagazines = false;
            myScript.infiniteBullets = false; 
            EditorGUILayout.PropertyField(serializedObject.FindProperty("magazineSize"));
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("coolSpeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cooledPercentageAfterOverheat"));
            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;
        }

        var damagePerBulletProperty = serializedObject.FindProperty("damagePerBullet");
        EditorGUILayout.PropertyField(damagePerBulletProperty);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("criticalDamageMultiplier"));

        if (myScript.shootStyle == ShootStyle.Hitscan)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("applyDamageReductionBasedOnDistance"));
            if (myScript.applyDamageReductionBasedOnDistance)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("minimumDistanceToApplyDamageReduction"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("damageReductionMultiplier"));
                EditorGUI.indentLevel--;
            }
        }

        var reloadStyleProperty = serializedObject.FindProperty("reloadStyle");
        EditorGUILayout.PropertyField(reloadStyleProperty);

        var reloadTimeProperty = serializedObject.FindProperty("reloadTime");
        EditorGUILayout.PropertyField(reloadTimeProperty);

        var allowAimProperty = serializedObject.FindProperty("allowAim");
        EditorGUILayout.PropertyField(allowAimProperty);

        using (var group = new EditorGUILayout.FadeGroupScope(Convert.ToSingle(myScript.allowAim)))
        {

            if (group.visible == true)
            {
                EditorGUI.indentLevel++;
                if (myScript.applyBulletSpread)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("Check new options under spread options. ", EditorStyles.helpBox);
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel++;
                var aimingPositionProperty = serializedObject.FindProperty("aimingPosition");
                EditorGUILayout.PropertyField(aimingPositionProperty);
                var aimingRotationProperty = serializedObject.FindProperty("aimingRotation");
                EditorGUILayout.PropertyField(aimingRotationProperty);
                var aimingSpeedProperty = serializedObject.FindProperty("aimingSpeed");
                EditorGUILayout.PropertyField(aimingSpeedProperty);
                var aimingFOVProperty = serializedObject.FindProperty("aimingFOV");
                EditorGUILayout.PropertyField(aimingFOVProperty);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("setMovementSpeedWhileAiming"));

                if (myScript.setMovementSpeedWhileAiming)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("movementSpeedWhileAiming"));
                    EditorGUI.indentLevel--;
                }

                EditorGUI.indentLevel--;
            }
        }

        var applyWeightProperty = serializedObject.FindProperty("applyWeight");
        EditorGUILayout.PropertyField(applyWeightProperty);


        using (var group = new EditorGUILayout.FadeGroupScope(Convert.ToSingle(myScript.applyWeight)))
        {

            if (group.visible == true)
            {
                EditorGUI.indentLevel++;
                var weightMultiplierProperty = serializedObject.FindProperty("weightMultiplier");
                EditorGUILayout.PropertyField(weightMultiplierProperty);
                EditorGUI.indentLevel--;
            }
        }
        if (!myScript.applyWeight) myScript.weightMultiplier = 1;
    }

    private void WeaponVisualSharedVariables(Weapon_SO myScript)
    {
        EditorGUILayout.LabelField("Camera Shake");
        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
        EditorGUILayout.PropertyField(serializedObject.FindProperty("camShakeAmount"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("applyFOVEffectOnShooting"));
        if (myScript.applyFOVEffectOnShooting)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("FOVValueToSubtract"));
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Your normal aiming FOV value is " + myScript.aimingFOV, EditorStyles.helpBox);
            EditorGUI.indentLevel--;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("AimingFOVValueToSubtract"));
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space(5f);
        var showBulletShellsProperty = serializedObject.FindProperty("showBulletShells");
        EditorGUILayout.PropertyField(showBulletShellsProperty);

        if (myScript.bulletsPerFire == 1) myScript.timeBetweenShots = 0;

        using (var group = new EditorGUILayout.FadeGroupScope(Convert.ToSingle(myScript.showBulletShells)))
        {

            if (group.visible == true)
            {
                EditorGUI.indentLevel++;
                var bulletGraphicsProperty = serializedObject.FindProperty("bulletGraphics");
                EditorGUILayout.PropertyField(bulletGraphicsProperty);
                EditorGUI.indentLevel--;
            }
        }
        if (myScript.explosionOnHit && myScript.shootStyle == ShootStyle.Projectile)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("explosionVFX"));
            EditorGUI.indentLevel--;
        }
        if (myScript.shootStyle != ShootStyle.Projectile)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bulletHoleImpact"));
            EditorGUI.indentLevel--;
        }
        var MuzzleVFXProperty = serializedObject.FindProperty("muzzleVFX");
        EditorGUILayout.PropertyField(MuzzleVFXProperty);
        EditorGUILayout.Space(10f);
    }
}
    #endregion
#endif
}