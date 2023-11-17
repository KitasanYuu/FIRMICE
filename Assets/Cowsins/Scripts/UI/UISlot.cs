/// <summary>
/// This script belongs to cowsins™ as a part of the cowsins´ FPS Engine. All rights reserved. 
/// </summary>
using UnityEngine.UI;
using UnityEngine;
namespace cowsins {
/// <summary>
/// Each slot you see on your UI, which is generated depending on your inventory size, requires this component
/// </summary>
public class UISlot : MonoBehaviour
{
    [HideInInspector]public int id; // Get position of this slot and organize it in the internal array of slots from WeaponController.cs

    [HideInInspector] public Weapon_SO weapon;

    [SerializeField] private Image image;

    [HideInInspector] public Vector3 initScale;

    public Sprite nullWeapon;

    private void Start() => initScale = transform.localScale;

    private void Update() => GetImage();// Get current weapon image

    public void GetImage() => image.sprite = (weapon == null) ? nullWeapon : weapon.icon; 
}
}
