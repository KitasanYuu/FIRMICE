#if UNITY_EDITOR
/// <summary>
/// This script belongs to cowsins™ as a part of the cowsins´ FPS Engine. All rights reserved. 
/// </summary>
using UnityEditor;
using UnityEngine;
namespace cowsins {
[System.Serializable]
[CustomEditor(typeof(WeaponController))]
public class WeaponControllerEditor : Editor
{
    private string[] tabs = { "Inventory", "References", "Variables", "Effects", "Events" };
    private int currentTab = 0;

    override public void OnInspectorGUI()
    {
        serializedObject.Update();
        WeaponController myScript = target as WeaponController;


        Texture2D myTexture = Resources.Load<Texture2D>("CustomEditor/weaponController_CustomEditor") as Texture2D;
        GUILayout.Label(myTexture);

        EditorGUILayout.BeginVertical();
        currentTab = GUILayout.Toolbar(currentTab, tabs);
        EditorGUILayout.Space(10f);
        EditorGUILayout.EndVertical();

        if (currentTab >= 0 || currentTab < tabs.Length)
        {
            switch (tabs[currentTab])
            {
                case "Inventory":
                    EditorGUILayout.LabelField("INVENTORY", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("inventorySize"));
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Attach your weapon scriptable objects here. These are not your initial weapons", EditorStyles.helpBox);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("weapons"));
                    EditorGUILayout.LabelField("These are your initial weapons", EditorStyles.helpBox);
                    EditorGUILayout.Space(5);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("initialWeapons"));
                    if (myScript.initialWeapons.Length > myScript.inventorySize) myScript.initialWeapons = new Weapon_SO[myScript.inventorySize];
                    if (myScript.initialWeapons.Length == myScript.inventorySize) EditorGUILayout.LabelField("You can´t add more initial weapons. This array can´t be bigger than the inventory size", EditorStyles.helpBox);
                    break;
                case "References":
                    EditorGUILayout.LabelField("REFERENCES", EditorStyles.boldLabel);
                    //var weaponProperty = serializedObject.FindProperty("weapon");
                    //EditorGUILayout.PropertyField(weaponProperty);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("mainCamera"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("cameraPivot"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponHolder"));
                    break;
                case "Variables":
                    EditorGUILayout.Space(10f);
                    EditorGUILayout.LabelField("VARIABLES", EditorStyles.boldLabel);
                    EditorGUILayout.Space(2f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("resizeCrosshair"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("autoReload"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("alternateAiming"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("hitLayer"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("removeCrosshairOnAiming"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("canMelee"));
                    if (myScript.canMelee)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("meleeObject"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("holsterMotionObject"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("meleeDuration"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("meleeDelay"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("meleeAttackDamage"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("meleeRange"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("meleeCamShakeAmount"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("reEnableMeleeAfterAction"));
                        if (myScript.meleeDelay > myScript.meleeDuration) myScript.meleeDelay = 0f;
                        EditorGUI.indentLevel--;
                    }
                    break;
                case "Effects":
                    EditorGUILayout.Space(2f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("effects"));
                    EditorGUILayout.Space(2f);
                    break;
                case "Events":
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("events"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("customShot"));
                    break;
            }
            EditorGUILayout.Space(10f);

            serializedObject.ApplyModifiedProperties();

        }
    }
}
}
#endif