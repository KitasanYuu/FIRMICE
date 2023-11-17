/// <summary>
/// This script belongs to cowsins™ as a part of the cowsins´ FPS Engine. All rights reserved. 
/// </summary>
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI; 
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace cowsins {
/// <summary>
/// Manage UI actions.
/// This is still subject to change and optimize.
/// </summary>
public class UIController : MonoBehaviour
{
    private InteractManager intManager;

    public PlayerMovement playerMovement; 

    [Tooltip("Use image bars to display player statistics.")] public bool barHealthDisplay;

    [Tooltip("Use text to display player statistics.")] public bool numericHealthDisplay;

    private Action<float, float> healthDisplayMethod;

    [Tooltip("Slider that will display the health on screen"), SerializeField] private Slider healthSlider;

    [Tooltip("Slider that will display the shield on screen"), SerializeField] private Slider shieldSlider;

    [SerializeField, Tooltip("UI Element ( TMPro text ) that displays current and maximum health.")] private TextMeshProUGUI healthTextDisplay;

    [SerializeField, Tooltip("UI Element ( TMPro te¡xt ) that displays current and maximum shield.")] private TextMeshProUGUI shieldTextDisplay;

    [Tooltip("This image shows damage and heal states visually on your screen, you can change the image" +
            "to any you like, but note that color will be overriden by the script"), SerializeField]
    private Image healthStatesEffect;

    [Tooltip(" Color of healthStatesEffect on different actions such as getting hurted or healed"), SerializeField] private Color damageColor, healColor, coinCollectColor;

    [Tooltip("Time for the healthStatesEffect to fade out"), SerializeField] private float fadeOutTime;

    [Tooltip("An object showing death events will be displayed on kill")]public bool displayEvents;

    [Tooltip("UI element which contains the killfeed. Where the kilfeed object will be instantiated and parented to"),SerializeField]
    private GameObject killfeedContainer;

    [Tooltip("Object to spawn"),SerializeField] private GameObject killfeedObject;
        
    [Tooltip("Attach the UI you want to use as your interaction UI")] public GameObject interactUI;

    [Tooltip("Displays the current progress of your interaction"), SerializeField] private Image interactUIProgressDisplay;

    [SerializeField, Tooltip("UI that displays incompatible interactions.")] private GameObject forbiddenInteractionUI; 

     [Tooltip("Inside the interact UI, this is the text that will display the object you want to interact with " +
        "or any custom method you would like." +
        "Do check Interactable.cs for that or, if you want, read our documentation or contact the cowsins support " +
        "in order to make custom interactions."), SerializeField]
    private TextMeshProUGUI interactText;

    [Tooltip("UI enabled when inspecting.")]public CanvasGroup inspectionUI;

    [SerializeField,Tooltip("Text that displays the name of the current weapon when inspecting.")] private TextMeshProUGUI weaponDisplayText_AttachmentsUI;

    [SerializeField,Tooltip("Prefab of the UI element that represents an attachment on-screen when inspecting")] private GameObject attachmentDisplay_UIElement;

    [SerializeField, Tooltip("Group of attachments. Attachment UI elements are wrapped inside these.")]
    private GameObject
        barrels_AttachmentsGroup,
        scopes_AttachmentsGroup,
        stocks_AttachmentsGroup,
        grips_AttachmentsGroup,
        magazines_AttachmentsGroup,
        flashlights_AttachmentsGroup,
        lasers_AttachmentsGroup;

    [SerializeField, Tooltip("Color of an attachment UI element when it is equipped.")] private Color usingAttachmentColor;

    [SerializeField, Tooltip("Color of an attachment UI element when it is unequipped. This is the default color.")] private Color notUsingAttachmentColor;

    [SerializeField, Tooltip("Contains dashUIElements in game.")] private Transform dashUIContainer;

    [SerializeField, Tooltip("Displays a dash slot in-game. This keeps stored at dashUIContainer during runtime.")] private Transform dashUIElement;

    [Tooltip("Attach the appropriate UI here")] public TextMeshProUGUI bulletsUI, magazineUI, reloadUI, lowAmmoUI;

    [Tooltip("Display an icon of your current weapon")] public Image currentWeaponDisplay;

    [Tooltip("Image that represents heat levels of your overheating weapon"), SerializeField] private Image overheatUI;

    [Tooltip(" Attach the CanvasGroup that contains the inventory")] public CanvasGroup inventoryContainer;

    [SerializeField] private GameObject coinsUI;

    [SerializeField] private TextMeshProUGUI coinsText; 

    public Crosshair crosshair;

    public static UIController instance { get; set; }

    private void Awake()
    {
        instance = this;    
    }
    private void Start()
    {
        intManager = PlayerStates.instance.GetComponent<InteractManager>();
        WeaponStates.instance.inspectionUI = inspectionUI;

        if (!CoinManager.Instance.useCoins && coinsUI != null) coinsUI.SetActive(false); 
    }
    private void Update()
    {
        if (healthStatesEffect.color != new Color(healthStatesEffect.color.r,
            healthStatesEffect.color.g,
            healthStatesEffect.color.b, 0)) healthStatesEffect.color -= new Color(0, 0, 0, Time.deltaTime * fadeOutTime);

        // Handle Inspection UI
        if (intManager.inspecting)
        {
            if (inspectionUI.alpha < 1)
                inspectionUI.alpha += Time.deltaTime;
        }
        else
        {
            inspectionUI.alpha -= Time.deltaTime * 2;
            if (inspectionUI.alpha < .1f) inspectionUI.gameObject.SetActive(false);
        }

        //Inventory
        if (InputManager.scrolling != 0 && !InputManager.reloading) inventoryContainer.alpha = 1;
        else if (inventoryContainer.alpha > 0) inventoryContainer.alpha -= Time.deltaTime;
    }

    // HEALTH SYSTEM /////////////////////////////////////////////////////////////////////////////////////////
    private void UpdateHealthUI(float health, float shield, bool damaged)
    {

        healthDisplayMethod?.Invoke(health,shield);

        Color colorSelected = damaged ? damageColor : healColor; 
        healthStatesEffect.color = colorSelected;
    }

    public void UpdateCoinsPanel()
    {
        healthStatesEffect.color = coinCollectColor;
    }

    private void HealthSetUp(float health, float shield,float maxHealth, float maxShield)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth; 
        }
        if (shieldSlider != null)
        {
            shieldSlider.maxValue = maxShield;
        }

        healthDisplayMethod?.Invoke(health, shield);

        if (shield == 0) shieldSlider.gameObject.SetActive(false);
    }

    private void BarHealthDisplayMethod(float health, float shield)
    {
        if (healthSlider != null)
            healthSlider.value = health;

        if (shieldSlider != null)
            shieldSlider.value = shield;
    }
    private void NumericHealthDisplayMethod(float health, float shield)
    {
        if (healthTextDisplay != null)
            healthTextDisplay.text = health.ToString("F0");

        if (shieldTextDisplay != null)
            shieldTextDisplay.text = shield.ToString("F0");
    }

    // INTERACTION /////////////////////////////////////////////////////////////////////////////////////////
    private void AllowedInteraction(string displayText)
    {
        forbiddenInteractionUI.SetActive(false);
        interactUI.SetActive(true);
        interactText.text = displayText;
        interactUI.GetComponent<Animation>().Play();
        interactUI.GetComponent<AudioSource>().Play();
    }

    private void ForbiddenInteraction()
    {
        forbiddenInteractionUI.SetActive(true);
        interactUI.SetActive(false);
    }

    private void DisableInteractionUI()
    {
        forbiddenInteractionUI.SetActive(false);
        interactUI.SetActive(false);
    }
    private void InteractioProgressUpdate(float value)
    {
        interactUIProgressDisplay.gameObject.SetActive(true);
        interactUIProgressDisplay.fillAmount = value;
    }
    private void FinishInteraction()
    {
        interactUIProgressDisplay.gameObject.SetActive(false);
    }

    // UI EVENTS /////////////////////////////////////////////////////////////////////////////////////////
    public void AddKillfeed(string name)
    {
        GameObject killfeed = Instantiate(killfeedObject, transform.position, Quaternion.identity, killfeedContainer.transform);
        killfeed.transform.GetChild(0).Find("Text").GetComponent<TextMeshProUGUI>().text = "You killed: " + name ;
    }

    public void Hitmarker()
    {
        Instantiate(Resources.Load("Hitmarker"),transform.position,Quaternion.identity,transform);
    }

    // INSPECT   /////////////////////////////////////////////////////////////////////////////////////////

    public void GenerateInspectionUI(WeaponController wcon)
    {
        WeaponIdentification weapon = wcon.id;
        bool displayCurrentAttachments = wcon.GetComponent<InteractManager>().displayCurrentAttachmentsOnly; 

        CleanAttachmentGroup(barrels_AttachmentsGroup);
        CleanAttachmentGroup(scopes_AttachmentsGroup);
        CleanAttachmentGroup(stocks_AttachmentsGroup);
        CleanAttachmentGroup(grips_AttachmentsGroup);
        CleanAttachmentGroup(magazines_AttachmentsGroup);
        CleanAttachmentGroup(flashlights_AttachmentsGroup);
        CleanAttachmentGroup(lasers_AttachmentsGroup);

        weaponDisplayText_AttachmentsUI.text = wcon.weapon._name;

        GenerateAttachmentGroup(displayCurrentAttachments,weapon.compatibleAttachments.barrels, barrels_AttachmentsGroup,wcon.inventory[wcon.currentWeapon].barrel, weapon.defaultAttachments.defaultBarrel);
        GenerateAttachmentGroup(displayCurrentAttachments, weapon.compatibleAttachments.scopes, scopes_AttachmentsGroup, wcon.inventory[wcon.currentWeapon].scope, weapon.defaultAttachments.defaultScope);
        GenerateAttachmentGroup(displayCurrentAttachments, weapon.compatibleAttachments.stocks, stocks_AttachmentsGroup, wcon.inventory[wcon.currentWeapon].stock, weapon.defaultAttachments.defaultStock);
        GenerateAttachmentGroup(displayCurrentAttachments, weapon.compatibleAttachments.grips, grips_AttachmentsGroup, wcon.inventory[wcon.currentWeapon].grip, weapon.defaultAttachments.defaultGrip);
        GenerateAttachmentGroup(displayCurrentAttachments, weapon.compatibleAttachments.magazines, magazines_AttachmentsGroup, wcon.inventory[wcon.currentWeapon].magazine, weapon.defaultAttachments.defaultMagazine);
        GenerateAttachmentGroup(displayCurrentAttachments, weapon.compatibleAttachments.flashlights, flashlights_AttachmentsGroup, wcon.inventory[wcon.currentWeapon].flashlight, weapon.defaultAttachments.defaultFlashlight);
        GenerateAttachmentGroup(displayCurrentAttachments, weapon.compatibleAttachments.lasers, lasers_AttachmentsGroup, wcon.id.laser, weapon.defaultAttachments.defaultLaser);
    }

    private void GenerateAttachmentGroup(bool displayCurrentAttachments, Attachment[] attachments, GameObject attachmentsGroup, Attachment atc, Attachment defaultAttachment)
    {
        if (attachments.Length == 0 || displayCurrentAttachments && atc == null)
        {
            attachmentsGroup.SetActive(false);
            return;
        }

        AttachmentGroupUI atcG = attachmentsGroup.GetComponent<AttachmentGroupUI>();
        if (atc != null)
            atcG.target = atc.transform;
        else if(attachments[0] != null) 
            atcG.target = attachments[0].transform;

        attachmentsGroup.SetActive(true);
        for (int i = 0; i < attachments.Length; i++)
        {
            if (attachments[i] == defaultAttachment || displayCurrentAttachments && attachments[i] != atc) continue; // Do not add default attachments to the UI 
            GameObject display = Instantiate(attachmentDisplay_UIElement, attachmentsGroup.transform);
            AttachmentUIElement disp = display.GetComponent<AttachmentUIElement>(); 

            if (attachments[i].attachmentIdentifier.attachmentIcon != null)
                disp.SetIcon(attachments[i].attachmentIdentifier.attachmentIcon);
            disp.assignedColor = usingAttachmentColor;
            disp.unAssignedColor = notUsingAttachmentColor;
            disp.DeselectAll(atc, i); 
            if(attachments[i] == atc)
                disp.SelectAsAssigned(); 
            disp.atc = attachments[i];
            disp.id = i; 
            display.SetActive(false); 
        }
    }

    private void CleanAttachmentGroup(GameObject attachmentsGroup)
    {
        for (int i = 1; i < attachmentsGroup.transform.childCount ; i++)
        {
            Destroy(attachmentsGroup.transform.GetChild(i).gameObject); 
        }
    }
    // MOVEMENT    ////////////////////////////////////////////////////////////////////////////////////////

    private List<GameObject> dashElements; // Stores the UI Elements required to display the current dashes amount

    /// <summary>
    /// Draws the dash UI 
    /// </summary>
    private void DrawDashUI(int amountOfDashes)
    {
        dashElements = new List<GameObject>(amountOfDashes);
        int i = 0;
        while (i < amountOfDashes)
        {
            var uiElement = Instantiate(dashUIElement, dashUIContainer);
            dashElements.Add(uiElement.gameObject);
            i++;
        }
    }

    private void RegainDash()
    {
        // Enable a new UI Element
        var uiElement = Instantiate(dashUIElement, dashUIContainer);
        dashElements.Add(uiElement.gameObject);
    }

    private void DashUsed(int currentDashes)
    {
        // Remove the UI Element
        var element = dashElements[currentDashes];
        dashElements.Remove(element);
        Destroy(element);
    }

    // WEAPON    /////////////////////////////////////////////////////////////////////////////////////////

    private void DetectReloadMethod(bool enable, bool useOverheat)
    {
        bulletsUI.gameObject.SetActive(enable);
        magazineUI.gameObject.SetActive(enable);
        overheatUI.transform.parent.gameObject.SetActive(useOverheat);
    }

    private void UpdateHeatRatio(float heatRatio)
    {
        overheatUI.fillAmount = heatRatio;
    }
    private void UpdateBullets(int bullets, int mag, bool activeReloadUI, bool activeLowAmmoUI)
    {
        bulletsUI.text = bullets.ToString();
        magazineUI.text = " / " + mag.ToString();
        reloadUI.gameObject.SetActive(activeReloadUI);
        lowAmmoUI.gameObject.SetActive(activeLowAmmoUI);
    }
    private void DisableWeaponUI()
    {
        overheatUI.transform.parent.gameObject.SetActive(false);
        bulletsUI.gameObject.SetActive(false);
        magazineUI.gameObject.SetActive(false);
        currentWeaponDisplay.gameObject.SetActive(false);
        reloadUI.gameObject.SetActive(false);
        lowAmmoUI.gameObject.SetActive(false);
    }

    private void SetWeaponDisplay(Weapon_SO weapon) => currentWeaponDisplay.sprite = weapon.icon;

    private void EnableDisplay() => currentWeaponDisplay.gameObject.SetActive(true); 

    // OTHERS    /////////////////////////////////////////////////////////////////////////////////////////
    public void ChangeScene(int scene) => SceneManager.LoadScene(scene);

    public void UpdateCoins(int amount) => coinsText.text = CoinManager.Instance.coins.ToString(); 

    private void OnEnable()
    {
        UIEvents.onHealthChanged += UpdateHealthUI;
        UIEvents.basicHealthUISetUp += HealthSetUp;
        if (barHealthDisplay) healthDisplayMethod += BarHealthDisplayMethod;
        if (numericHealthDisplay) healthDisplayMethod += NumericHealthDisplayMethod;
        UIEvents.allowedInteraction += AllowedInteraction;
        UIEvents.forbiddenInteraction += ForbiddenInteraction;
        UIEvents.disableInteractionUI += DisableInteractionUI;
        UIEvents.onInteractionProgressChanged += InteractioProgressUpdate;
        UIEvents.onFinishInteractionProgress += FinishInteraction;
        UIEvents.onGenerateInspectionUI += GenerateInspectionUI;
        UIEvents.onInitializeDashUI += DrawDashUI;
        UIEvents.onDashGained += RegainDash;
        UIEvents.onDashUsed += DashUsed;
        UIEvents.onEnemyHit += Hitmarker;
        UIEvents.onEnemyKilled += AddKillfeed;
        UIEvents.onDetectReloadMethod += DetectReloadMethod;
        UIEvents.onHeatRatioChanged += UpdateHeatRatio;
        UIEvents.onBulletsChanged += UpdateBullets;
        UIEvents.disableWeaponUI += DisableWeaponUI;
        UIEvents.setWeaponDisplay += SetWeaponDisplay;
        UIEvents.enableWeaponDisplay += EnableDisplay;
        UIEvents.onCoinsChange += UpdateCoins; 

        interactUI.SetActive(false);
    }
    private void OnDisable()
    {
        UIEvents.onHealthChanged = null;
        UIEvents.basicHealthUISetUp = null;
        healthDisplayMethod = null;
        UIEvents.allowedInteraction = null;
        UIEvents.forbiddenInteraction = null;
        UIEvents.disableInteractionUI = null;
        UIEvents.onInteractionProgressChanged = null;
        UIEvents.onFinishInteractionProgress = null;
        UIEvents.onGenerateInspectionUI = null;
        UIEvents.onInitializeDashUI = null;
        UIEvents.onDashGained = null;
        UIEvents.onDashUsed = null;
        UIEvents.onEnemyHit = null;
        UIEvents.onEnemyKilled = null;
        UIEvents.onDetectReloadMethod = null;
        UIEvents.onHeatRatioChanged = null;
        UIEvents.onBulletsChanged = null;
        UIEvents.disableWeaponUI = null;
        UIEvents.setWeaponDisplay = null;
        UIEvents.enableWeaponDisplay = null; 
    }

}
#if UNITY_EDITOR
[System.Serializable]
[CustomEditor(typeof(UIController))]
public class UIControllerEditor : Editor
{
    private string[] tabs = { "Health", "Interaction","Attachments","Weapon","Dashing","Others","UI Events" };
    private int currentTab = 0;

    override public void OnInspectorGUI()
    {
        serializedObject.Update();
        UIController myScript = target as UIController;

        EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(10f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("playerMovement"));
            EditorGUILayout.Space(10f);
            currentTab = GUILayout.Toolbar(currentTab, tabs);
            EditorGUILayout.Space(10f);
        EditorGUILayout.EndVertical();


        if (currentTab >= 0 || currentTab < tabs.Length)
        {
            switch (tabs[currentTab])
            {
                case "Health":
                    EditorGUILayout.LabelField("HEALTH AND SHIELD", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("barHealthDisplay"));
                    if (myScript.barHealthDisplay)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("healthSlider"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("shieldSlider"));
                        EditorGUI.indentLevel--;
                    }
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("numericHealthDisplay"));

                    if (myScript.numericHealthDisplay)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("healthTextDisplay"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("shieldTextDisplay"));
                        EditorGUI.indentLevel--;
                    }

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("healthStatesEffect"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("damageColor"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("healColor")); 
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("coinCollectColor"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("fadeOutTime"));

                    break;
                case "Interaction":

                    EditorGUILayout.LabelField("INTERACTION", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("interactUI"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("interactUIProgressDisplay"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("forbiddenInteractionUI"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("interactText"));

                    break;
                case "Attachments":
                    EditorGUILayout.LabelField("ATTACHMENTS", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("inspectionUI"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponDisplayText_AttachmentsUI"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("attachmentDisplay_UIElement"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("barrels_AttachmentsGroup"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("scopes_AttachmentsGroup"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("stocks_AttachmentsGroup"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("grips_AttachmentsGroup"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("magazines_AttachmentsGroup"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("flashlights_AttachmentsGroup"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("lasers_AttachmentsGroup")); 
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("usingAttachmentColor"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("notUsingAttachmentColor"));
                    break;
                case "Weapon":

                    EditorGUILayout.LabelField("WEAPON", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("bulletsUI"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("magazineUI"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("overheatUI"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("reloadUI"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("lowAmmoUI"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("currentWeaponDisplay"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("inventoryContainer"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("crosshair"));
                    break;
                case "Dashing":

                    EditorGUILayout.LabelField("DASHING", EditorStyles.boldLabel);
                    if (myScript.playerMovement != null && myScript.playerMovement.canDash && !myScript.playerMovement.infiniteDashes)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("dashUIContainer"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("dashUIElement"));
                        EditorGUI.indentLevel--;
                    }

                    break;
                case "Others":
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("coinsUI"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("coinsText"));
                    break; 
                case "UI Events":

                    EditorGUILayout.LabelField("EVENTS", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("displayEvents"));
                    if (myScript.displayEvents)
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("killfeedContainer"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("killfeedObject"));
                    }

                    break;
            }
        }
        serializedObject.ApplyModifiedProperties();

    }
}
#endif
}