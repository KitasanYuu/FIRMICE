/// <summary>
/// This script belongs to cowsins� as a part of the cowsins� FPS Engine. All rights reserved. 
/// </summary>
using UnityEngine;

using UnityEditor;
namespace cowsins {
[System.Serializable]
public class DefaultAttachment
{
    public Attachment defaultBarrel,
        defaultScope,
        defaultStock,
        defaultGrip,
        defaultMagazine,
        defaultFlashlight,
        defaultLaser;
}
/// <summary>
/// Attach this to your weapon object ( the one that goes in the weapon array of WeaponController )
/// </summary>
public class WeaponIdentification : MonoBehaviour
{
    public Weapon_SO weapon;

    [Tooltip("Every weapon, excluding melee, must have a firePoint, which is the point where the bullet comes from." +
        "Just make an empty object, call it firePoint for organization purposes and attach it here. ")]
    public Transform[] FirePoint;

    [HideInInspector] public int totalMagazines, magazineSize, bulletsLeftInMagazine, totalBullets; // Internal use

    [Tooltip("Defines the default attachments for your weapon. The first time you pick it up, these attachments will be equipped.")]public DefaultAttachment defaultAttachments; 

    [HideInInspector]public Attachment barrel, 
        scope, 
        stock, 
        grip, 
        magazine, 
        flashlight, 
        laser;


    [Tooltip("Defines all the attachments that can be equipped on your weapon.")]public CompatibleAttachments compatibleAttachments;

        [HideInInspector] public float heatRatio; 

    private void Start()
    {
        totalMagazines = weapon.totalMagazines;
        GetMagazineSize(); 
        GetComponentInChildren<Animator>().keepAnimatorStateOnDisable = true;
    }

    public void GetMagazineSize()
    {
        if (magazine == null) magazineSize = weapon.magazineSize;
        else magazineSize = weapon.magazineSize + magazine.GetComponent<Magazine>().magazineCapacityAdded;

        if (bulletsLeftInMagazine > magazineSize) bulletsLeftInMagazine = magazineSize; 
    }
}

#if UNITY_EDITOR


    [CustomEditor(typeof(WeaponIdentification))]
    public class WeaponIdentificationInspector : Editor
    {

        private string[] tabs = { "Basic", "Attachments" };
        private int currentTab = 0;


        private void OnEnable()
        {
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            Texture2D myTexture = Resources.Load<Texture2D>("CustomEditor/weaponIdentification_CustomEditor") as Texture2D;
            GUILayout.Label(myTexture);


            currentTab = GUILayout.Toolbar(currentTab, tabs);

            if (currentTab >= 0 || currentTab < tabs.Length)
            {
                switch (tabs[currentTab])
                {
                    case "Basic":
                        EditorGUILayout.Space(20f);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("weapon"));

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("FirePoint"));
                        break;
                    case "Attachments":
                        EditorGUILayout.Space(5f);
                        if (GUILayout.Button("Attachments Tutorial", GUILayout.Height(20)))
                        {
                            Application.OpenURL("https://youtu.be/Q1saDyb4eDI");
                        }
                        EditorGUILayout.Space(20f);
                        GUILayout.Label("If you aren't using attachments on this particular weapon, make sure these references are null.", EditorStyles.wordWrappedLabel);
                        EditorGUILayout.Space(20f);
                        EditorGUILayout.LabelField("Assign the original or default attachments that your weapon is meant to have, even when removing other attachments. This could include items like iron sights or standard magazines.", EditorStyles.helpBox);
                        EditorGUILayout.Space(5f);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultAttachments"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("compatibleAttachments"));
                        break;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }

#endif
}
