using System;
using UnityEngine; 

namespace cowsins
{ 
/// <summary>
/// Handles events for the UI. These can be accessed anywhere.
/// </summary>
public class UIEvents
{
    public static Action<float, float, bool> onHealthChanged;

    public static Action<float, float, float, float> basicHealthUISetUp;

    public static Action forbiddenInteraction, disableInteractionUI, onFinishInteractionProgress, onDashGained,onEnemyHit, disableWeaponUI, enableWeaponDisplay;

    public static Action<float> onInteractionProgressChanged, onHeatRatioChanged;
    
    public static Action<string> allowedInteraction, onEnemyKilled;

    public static Action<WeaponController> onGenerateInspectionUI;

    public static Action<int> onInitializeDashUI, onDashUsed;

    public static Action<int,int,bool,bool> onBulletsChanged;

    public static Action<int> onUnholsteringWeapon;

    public static Action<bool,bool> onDetectReloadMethod;

    public static Action<Weapon_SO> setWeaponDisplay;

    public static Action<GameObject> onEnableAttachmentUI;

    public static Action<Attachment, bool> onAttachmentUIElementClicked; 

    public static Action<Attachment, int> onAttachmentUIElementClickedNewAttachment;

    public static Action<int> onCoinsChange;
}
}