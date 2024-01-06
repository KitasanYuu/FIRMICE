#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace Michsky.LSS
{
    [CustomEditor(typeof(LSS_LoadingScreen))]
    [System.Serializable]
    public class LSS_LoadingScreenEditor : Editor
    {
        private LSS_LoadingScreen lsTarget;
        private GUISkin customSkin;
        private int currentTab;
        List<Transform> spinnerList = new List<Transform>();
        List<string> spinnerTitles = new List<string>();
        int selectedSpinnerIndex;
        Image pakCountdownFilled;
        Image pakCountdownBG;
        LSS_Spinner selectedSpinnerItem;

        private void OnEnable()
        {
            lsTarget = (LSS_LoadingScreen)target;

            if (lsTarget.spinnerParent != null)
            {
                foreach (Transform child in lsTarget.spinnerParent) { spinnerList.Add(child); }
                foreach (var t in spinnerList) { spinnerTitles.Add(t.name); }

                selectedSpinnerIndex = lsTarget.spinnerHelper;
            }

            if (EditorGUIUtility.isProSkin == true) { customSkin = LSS_EditorHandler.GetDarkEditor(customSkin); }
            else { customSkin = LSS_EditorHandler.GetLightEditor(customSkin); }
        }

        public override void OnInspectorGUI()
        {
            LSS_EditorHandler.DrawComponentHeader(customSkin, "LS Top Header");

            GUIContent[] toolbarTabs = new GUIContent[5];
            toolbarTabs[0] = new GUIContent("Layout");
            toolbarTabs[1] = new GUIContent("Hints");
            toolbarTabs[2] = new GUIContent("Images");
            toolbarTabs[3] = new GUIContent("Resources");
            toolbarTabs[4] = new GUIContent("Settings");

            currentTab = LSS_EditorHandler.DrawTabs(currentTab, toolbarTabs, customSkin);

            if (GUILayout.Button(new GUIContent("Content", "Content"), customSkin.FindStyle("Tab Content")))
                currentTab = 0;
            if (GUILayout.Button(new GUIContent("Hints", "Hints"), customSkin.FindStyle("Tab Hints")))
                currentTab = 1;
            if (GUILayout.Button(new GUIContent("Background", "Background"), customSkin.FindStyle("Tab Background")))
                currentTab = 2;
            if (GUILayout.Button(new GUIContent("Resources", "Resources"), customSkin.FindStyle("Tab Resources")))
                currentTab = 3;
            if (GUILayout.Button(new GUIContent("Settings", "Settings"), customSkin.FindStyle("Tab Settings")))
                currentTab = 4;

            GUILayout.EndHorizontal();

            // Property variables
            var titleObjText = serializedObject.FindProperty("titleObjText");
            var titleObjDescText = serializedObject.FindProperty("titleObjDescText");
            var backgroundImage = serializedObject.FindProperty("backgroundImage");
            var updateHelper = serializedObject.FindProperty("updateHelper");

            // Layout variables
            var titleColor = serializedObject.FindProperty("titleColor");
            var titleSize = serializedObject.FindProperty("titleSize");
            var titleFont = serializedObject.FindProperty("titleFont");
            var descriptionColor = serializedObject.FindProperty("descriptionColor");
            var descriptionSize = serializedObject.FindProperty("descriptionSize");
            var descriptionFont = serializedObject.FindProperty("descriptionFont");
            var hintColor = serializedObject.FindProperty("hintColor");
            var hintSize = serializedObject.FindProperty("hintSize");
            var hintFont = serializedObject.FindProperty("hintFont");
            var statusColor = serializedObject.FindProperty("statusColor");
            var statusSize = serializedObject.FindProperty("statusSize");
            var statusFont = serializedObject.FindProperty("statusFont");
            var pakColor = serializedObject.FindProperty("pakColor");
            var pakSize = serializedObject.FindProperty("pakSize");
            var pakFont = serializedObject.FindProperty("pakFont");
            var spinnerColor = serializedObject.FindProperty("spinnerColor");

            // Hint variables
            var enableRandomHints = serializedObject.FindProperty("enableRandomHints");
            var hintList = serializedObject.FindProperty("hintList");
            var changeHintWithTimer = serializedObject.FindProperty("changeHintWithTimer");
            var hintTimerValue = serializedObject.FindProperty("hintTimerValue");

            // Image variables
            var enableRandomImages = serializedObject.FindProperty("enableRandomImages");
            var imageList = serializedObject.FindProperty("imageList");
            var changeImageWithTimer = serializedObject.FindProperty("changeImageWithTimer");
            var imageTimerValue = serializedObject.FindProperty("imageTimerValue");
            var imageFadingSpeed = serializedObject.FindProperty("imageFadingSpeed");

            // Resources
            var canvasGroup = serializedObject.FindProperty("canvasGroup");
            var backgroundCanvasGroup = serializedObject.FindProperty("backgroundCanvasGroup");
            var contentCanvasGroup = serializedObject.FindProperty("contentCanvasGroup");
            var pakCanvasGroup = serializedObject.FindProperty("pakCanvasGroup");
            var statusObj = serializedObject.FindProperty("statusObj");
            var titleObj = serializedObject.FindProperty("titleObj");
            var descriptionObj = serializedObject.FindProperty("descriptionObj");
            var progressBar = serializedObject.FindProperty("progressBar");
            var hintsText = serializedObject.FindProperty("hintsText");
            var imageObject = serializedObject.FindProperty("imageObject");
            var pakTextObj = serializedObject.FindProperty("pakTextObj");
            var pakCountdownSlider = serializedObject.FindProperty("pakCountdownSlider");
            var pakCountdownLabel = serializedObject.FindProperty("pakCountdownLabel");
            var spinnerParent = serializedObject.FindProperty("spinnerParent");
            var projectorCamera = serializedObject.FindProperty("projectorCamera");

            // Settings
            var setTimeScale = serializedObject.FindProperty("setTimeScale");
            var fadeSpeed = serializedObject.FindProperty("fadeSpeed");
            var contentFadeSpeed = serializedObject.FindProperty("contentFadeSpeed");
            var backgroundFadeSpeed = serializedObject.FindProperty("backgroundFadeSpeed");
            var enablePressAnyKey = serializedObject.FindProperty("enablePressAnyKey");
            var useCountdown = serializedObject.FindProperty("useCountdown");
            var waitForPlayerInput = serializedObject.FindProperty("waitForPlayerInput");
            var useSpecificKey = serializedObject.FindProperty("useSpecificKey");
            var keyCode = serializedObject.FindProperty("keyCode");
            var pakText = serializedObject.FindProperty("pakText");
            var pakCountdownTimer = serializedObject.FindProperty("pakCountdownTimer");
            var enableVirtualLoading = serializedObject.FindProperty("enableVirtualLoading");
            var virtualLoadingTimer = serializedObject.FindProperty("virtualLoadingTimer");
            var customSceneActivation = serializedObject.FindProperty("customSceneActivation");

            // Events
            var onLoadingStart = serializedObject.FindProperty("onLoadingStart");
            var onLoadingEnd = serializedObject.FindProperty("onLoadingEnd");
            var onLoadingDestroy = serializedObject.FindProperty("onLoadingDestroy");

            switch (currentTab)
            {
                case 0:
                    LSS_EditorHandler.DrawHeader(customSkin, "Layout Header", 6);

                    if (titleObj.objectReferenceValue != null)
                    {
                        GUILayout.BeginVertical(EditorStyles.helpBox);
                        EditorGUILayout.LabelField(new GUIContent("Title Text"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                        GUILayout.Space(-16);
                        EditorGUILayout.PropertyField(titleObjText, new GUIContent(""), GUILayout.Height(40));
                        GUILayout.EndVertical();

                        if (titleObj != null)
                            lsTarget.titleObj.text = lsTarget.titleObjText;
                    }

                    if (descriptionObj.objectReferenceValue != null)
                    {
                        GUILayout.BeginVertical(EditorStyles.helpBox);
                        EditorGUILayout.LabelField(new GUIContent("Description Text"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                        GUILayout.Space(-16);
                        EditorGUILayout.PropertyField(titleObjDescText, new GUIContent(""), GUILayout.Height(70));
                        GUILayout.EndVertical();

                        if (titleObj != null)
                            lsTarget.descriptionObj.text = lsTarget.titleObjDescText;
                    }

                    if (titleObj.objectReferenceValue == null && descriptionObj.objectReferenceValue == null)
                        EditorGUILayout.HelpBox("Both Title and Description is disabled.", MessageType.Info);

                    if (enablePressAnyKey.boolValue == true)
                    {
                        GUILayout.BeginVertical(EditorStyles.helpBox);
                        EditorGUILayout.LabelField(new GUIContent("PAK Text"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                        GUILayout.Space(-16);
                        EditorGUILayout.PropertyField(pakText, new GUIContent(""), GUILayout.Height(70));
                        GUILayout.EndVertical();

                        if (pakTextObj != null)
                            lsTarget.pakTextObj.text = lsTarget.pakText;
                    }

                    GUILayout.BeginHorizontal(EditorStyles.helpBox);
                    EditorGUILayout.LabelField(new GUIContent("Selected Spinner"), customSkin.FindStyle("Text"), GUILayout.Width(120));

                    selectedSpinnerIndex = EditorGUILayout.Popup(selectedSpinnerIndex, spinnerTitles.ToArray());
                    lsTarget.spinnerHelper = selectedSpinnerIndex;

                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();

                    if (lsTarget.canvasGroup != null && lsTarget.canvasGroup.alpha == 0 && GUILayout.Button("Make It Visible", customSkin.button)) { lsTarget.canvasGroup.alpha = 1; }
                    else if (lsTarget.canvasGroup != null && GUILayout.Button("Make It Invisible", customSkin.button)) { lsTarget.canvasGroup.alpha = 0; }

                    if (GUILayout.Button("Update Editor", customSkin.button))
                    {
                        if (titleObj.objectReferenceValue != null)
                        {
                            lsTarget.titleObj.fontSize = lsTarget.titleSize;
                            lsTarget.titleObj.font = lsTarget.titleFont;
                            lsTarget.titleObj.color = lsTarget.titleColor;
                        }

                        if (descriptionObj.objectReferenceValue != null)
                        {
                            lsTarget.descriptionObj.fontSize = lsTarget.descriptionSize;
                            lsTarget.descriptionObj.font = lsTarget.descriptionFont;
                            lsTarget.descriptionObj.color = lsTarget.descriptionColor;
                        }

                        if (enableRandomHints.boolValue == true)
                        {
                            lsTarget.hintsText.fontSize = lsTarget.hintSize;
                            lsTarget.hintsText.font = lsTarget.hintFont;
                            lsTarget.hintsText.color = lsTarget.hintColor;
                        }

                        if (statusObj.objectReferenceValue != null)
                        {
                            lsTarget.statusObj.fontSize = lsTarget.statusSize;
                            lsTarget.statusObj.font = lsTarget.statusFont;
                            lsTarget.statusObj.color = lsTarget.statusColor;
                        }

                        if (enablePressAnyKey.boolValue == true)
                        {
                            lsTarget.pakTextObj.fontSize = lsTarget.pakSize;
                            lsTarget.pakTextObj.font = lsTarget.pakFont;
                            lsTarget.pakTextObj.color = lsTarget.pakColor;
                            lsTarget.pakCountdownLabel.color = lsTarget.pakColor;

                            try
                            {
                                pakCountdownFilled = lsTarget.pakCountdownSlider.transform.Find("Filled").GetComponent<Image>();
                                pakCountdownBG = lsTarget.pakCountdownSlider.transform.Find("Background").GetComponent<Image>();
                            }

                            catch { }

                            if (pakCountdownFilled != null && pakCountdownBG != null)
                            {
                                pakCountdownFilled.color = lsTarget.spinnerColor;
                                pakCountdownBG.color = new Color(lsTarget.spinnerColor.r, lsTarget.spinnerColor.g, lsTarget.spinnerColor.b, 0.08f);
                            }
                        }

                        try
                        {
                            selectedSpinnerItem = spinnerList[selectedSpinnerIndex].GetComponent<LSS_Spinner>();
                            selectedSpinnerItem.UpdateValues();
                        }

                        catch { Debug.Log("Loading Screen - Cannot initialize selected Spinner Item.", this); }

                        updateHelper.boolValue = true;
                        updateHelper.boolValue = false;
                    }

                    GUILayout.EndHorizontal();

                    foreach (Transform child in lsTarget.spinnerParent)
                    {
                        if (child.name != spinnerList[selectedSpinnerIndex].ToString().Replace(" (UnityEngine.RectTransform)", "").Trim()) { child.gameObject.SetActive(false); }
                        else { child.gameObject.SetActive(true); }
                    }

                    LSS_EditorHandler.DrawHeader(customSkin, "Customization Header", 10);

                    if (titleObj.objectReferenceValue != null)
                    {
                        GUILayout.BeginVertical(EditorStyles.helpBox);
                        EditorGUILayout.LabelField(new GUIContent("Title", "Font Size / Font / Font Color"), customSkin.FindStyle("Text"));
                        GUILayout.BeginHorizontal();

                        EditorGUILayout.PropertyField(titleSize, new GUIContent(""), GUILayout.Width(40));
                        EditorGUILayout.PropertyField(titleFont, new GUIContent(""));
                        EditorGUILayout.PropertyField(titleColor, new GUIContent(""));

                        GUILayout.EndHorizontal();
                        GUILayout.EndVertical();
                    }

                    if (descriptionObj.objectReferenceValue != null)
                    {
                        GUILayout.BeginVertical(EditorStyles.helpBox);
                        EditorGUILayout.LabelField(new GUIContent("Description", "Font Size / Font / Font Color"), customSkin.FindStyle("Text"), GUILayout.Width(100));
                        GUILayout.BeginHorizontal();

                        EditorGUILayout.PropertyField(descriptionSize, new GUIContent(""), GUILayout.Width(40));
                        EditorGUILayout.PropertyField(descriptionFont, new GUIContent(""));
                        EditorGUILayout.PropertyField(descriptionColor, new GUIContent(""));

                        GUILayout.EndHorizontal();
                        GUILayout.EndVertical();
                    }

                    if (enableRandomHints.boolValue == true)
                    {
                        GUILayout.BeginVertical(EditorStyles.helpBox);
                        EditorGUILayout.LabelField(new GUIContent("Hint", "Font Size / Font / Font Color"), customSkin.FindStyle("Text"), GUILayout.Width(100));
                        GUILayout.BeginHorizontal();

                        EditorGUILayout.PropertyField(hintSize, new GUIContent(""), GUILayout.Width(40));
                        EditorGUILayout.PropertyField(hintFont, new GUIContent(""));
                        EditorGUILayout.PropertyField(hintColor, new GUIContent(""));

                        GUILayout.EndHorizontal();
                        GUILayout.EndVertical();
                    }

                    if (statusObj.objectReferenceValue != null)
                    {
                        GUILayout.BeginVertical(EditorStyles.helpBox);
                        EditorGUILayout.LabelField(new GUIContent("Loading Status", "Font Size / Font / Font Color"), customSkin.FindStyle("Text"), GUILayout.Width(100));
                        GUILayout.BeginHorizontal();

                        EditorGUILayout.PropertyField(statusSize, new GUIContent(""), GUILayout.Width(40));
                        EditorGUILayout.PropertyField(statusFont, new GUIContent(""));
                        EditorGUILayout.PropertyField(statusColor, new GUIContent(""));

                        GUILayout.EndHorizontal();
                        GUILayout.EndVertical();
                    }

                    if (enablePressAnyKey.boolValue == true)
                    {
                        GUILayout.BeginVertical(EditorStyles.helpBox);
                        EditorGUILayout.LabelField(new GUIContent("Press Any Key", "Font Size / Font / Font Color"), customSkin.FindStyle("Text"), GUILayout.Width(100));
                        GUILayout.BeginHorizontal();

                        EditorGUILayout.PropertyField(pakSize, new GUIContent(""), GUILayout.Width(40));
                        EditorGUILayout.PropertyField(pakFont, new GUIContent(""));
                        EditorGUILayout.PropertyField(pakColor, new GUIContent(""));

                        GUILayout.EndHorizontal();
                        GUILayout.EndVertical();
                    }
                  
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField(new GUIContent("Spinner"), customSkin.FindStyle("Text"), GUILayout.Width(100));
                    GUILayout.BeginHorizontal();

                    EditorGUILayout.PropertyField(spinnerColor, new GUIContent(""));

                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                    GUILayout.BeginHorizontal();

                    if (GUILayout.Button("Restore Values", customSkin.button))
                    {
                        if (titleObj.objectReferenceValue != null)
                        {
                            lsTarget.titleSize = lsTarget.titleObj.fontSize;
                            lsTarget.titleFont = lsTarget.titleObj.font;
                            lsTarget.titleColor = lsTarget.titleObj.color;
                        }

                        if (descriptionObj.objectReferenceValue != null)
                        {
                            lsTarget.descriptionSize = lsTarget.descriptionObj.fontSize;
                            lsTarget.descriptionFont = lsTarget.descriptionObj.font;
                            lsTarget.descriptionColor = lsTarget.descriptionObj.color;
                        }

                        if (enableRandomHints.boolValue == true)
                        {
                            lsTarget.hintSize = lsTarget.hintsText.fontSize;
                            lsTarget.hintFont = lsTarget.hintsText.font;
                            lsTarget.hintColor = lsTarget.hintsText.color;
                        }

                        if (statusObj.objectReferenceValue != null)
                        {
                            lsTarget.statusSize = lsTarget.statusObj.fontSize;
                            lsTarget.statusFont = lsTarget.statusObj.font;
                            lsTarget.statusColor = lsTarget.statusObj.color;
                        }

                        if (enablePressAnyKey.boolValue == true)
                        {
                            lsTarget.pakSize = lsTarget.pakTextObj.fontSize;
                            lsTarget.pakFont = lsTarget.pakTextObj.font;
                            lsTarget.pakColor = lsTarget.pakTextObj.color;
                        }
                    }

                    if (GUILayout.Button("Update Values", customSkin.button))
                    {
                        if (titleObj.objectReferenceValue != null)
                        {
                            lsTarget.titleObj.fontSize = lsTarget.titleSize;
                            lsTarget.titleObj.font = lsTarget.titleFont;
                            lsTarget.titleObj.color = lsTarget.titleColor;
                        }

                        if (descriptionObj.objectReferenceValue != null)
                        {
                            lsTarget.descriptionObj.fontSize = lsTarget.descriptionSize;
                            lsTarget.descriptionObj.font = lsTarget.descriptionFont;
                            lsTarget.descriptionObj.color = lsTarget.descriptionColor;
                        }

                        if (enableRandomHints.boolValue == true)
                        {
                            lsTarget.hintsText.fontSize = lsTarget.hintSize;
                            lsTarget.hintsText.font = lsTarget.hintFont;
                            lsTarget.hintsText.color = lsTarget.hintColor;
                        }

                        if (statusObj.objectReferenceValue != null)
                        {
                            lsTarget.statusObj.fontSize = lsTarget.statusSize;
                            lsTarget.statusObj.font = lsTarget.statusFont;
                            lsTarget.statusObj.color = lsTarget.statusColor;
                        }

                        if (enablePressAnyKey.boolValue == true)
                        {
                            lsTarget.pakTextObj.fontSize = lsTarget.pakSize;
                            lsTarget.pakTextObj.font = lsTarget.pakFont;
                            lsTarget.pakTextObj.color = lsTarget.pakColor;
                            lsTarget.pakCountdownLabel.color = lsTarget.pakColor;

                            try
                            {
                                pakCountdownFilled = lsTarget.pakCountdownSlider.transform.Find("Filled").GetComponent<Image>();
                                pakCountdownBG = lsTarget.pakCountdownSlider.transform.Find("Background").GetComponent<Image>();
                            }

                            catch { }

                            if (pakCountdownFilled != null && pakCountdownBG != null)
                            {
                                pakCountdownFilled.color = lsTarget.spinnerColor;
                                pakCountdownBG.color = new Color(lsTarget.spinnerColor.r, lsTarget.spinnerColor.g, lsTarget.spinnerColor.b, 0.08f);
                            }
                        }

                        try
                        {
                            selectedSpinnerItem = spinnerList[selectedSpinnerIndex].GetComponent<LSS_Spinner>();
                            selectedSpinnerItem.UpdateValues();
                        }

                        catch { Debug.Log("Loading Screen - Cannot initialize selected Spinner Item.", this); }

                        updateHelper.boolValue = true;
                        updateHelper.boolValue = false;
                    }

                    GUILayout.EndHorizontal();
                    break;

                case 1:
                    LSS_EditorHandler.DrawHeader(customSkin, "Hints Header", 6);
                    enableRandomHints.boolValue = LSS_EditorHandler.DrawToggle(enableRandomHints.boolValue, customSkin, "Enable Random Hints");

                    if (enableRandomHints.boolValue == true)
                    {
                        GUILayout.BeginVertical(EditorStyles.helpBox);
                        GUILayout.Space(-3);
                        changeHintWithTimer.boolValue = LSS_EditorHandler.DrawTogglePlain(changeHintWithTimer.boolValue, customSkin, "Use Timer", "Use a timer to change the hint.");
                        GUILayout.Space(3);

                        if (changeHintWithTimer.boolValue == true)
                        {
                            LSS_EditorHandler.DrawPropertyCW(hintTimerValue, customSkin, "Hint Timer", 80);
                        }

                        GUILayout.EndVertical();

                        GUILayout.BeginHorizontal();
                        EditorGUI.indentLevel = 1;
                        EditorGUILayout.PropertyField(hintList, new GUIContent("Hint List"), true);
                        EditorGUI.indentLevel = 0;
                        GUILayout.Space(6);
                        GUILayout.EndHorizontal();

                        if (lsTarget.hintsText == null)
                        {
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.HelpBox("'Hint Text Object' is missing.", MessageType.Error);
                            GUILayout.EndHorizontal();
                        }
                    }

                    else if (enableRandomHints.boolValue == false && hintsText != null)
                        lsTarget.hintsText.gameObject.SetActive(false);

                    break;

                case 2:
                    LSS_EditorHandler.DrawHeader(customSkin, "Background Header", 6);
                    enableRandomImages.boolValue = LSS_EditorHandler.DrawToggle(enableRandomImages.boolValue, customSkin, "Enable Random Images");

                    if (enableRandomImages.boolValue == true)
                    {
                        GUILayout.BeginVertical(EditorStyles.helpBox);
                        GUILayout.Space(-3);
                        changeImageWithTimer.boolValue = LSS_EditorHandler.DrawTogglePlain(changeImageWithTimer.boolValue, customSkin, "Use Timer", "Use a timer to change the background.");
                        GUILayout.Space(3);

                        if (changeImageWithTimer.boolValue == true)
                        {
                            LSS_EditorHandler.DrawProperty(imageTimerValue, customSkin, "Image Timer");
                            LSS_EditorHandler.DrawProperty(imageFadingSpeed, customSkin, "Transition Speed", "Set the smooth background transition speed between random images.");
                        }

                        GUILayout.EndVertical();

                        GUILayout.BeginHorizontal();
                        EditorGUI.indentLevel = 1;
                        EditorGUILayout.PropertyField(imageList, new GUIContent("Image List"), true);
                        EditorGUI.indentLevel = 0;
                        GUILayout.EndHorizontal();

                        if (lsTarget.imageObject == null)
                        {
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.HelpBox("'Image Object' is not assigned. Go to Resources tab and assign the correct variable.", MessageType.Error);
                            GUILayout.EndHorizontal();
                        }
                    }

                    else
                    {
                        LSS_EditorHandler.DrawProperty(backgroundImage, customSkin, "Background Image");

                        if (lsTarget.imageObject.sprite != lsTarget.backgroundImage)
                        {
                            lsTarget.imageObject.sprite = lsTarget.backgroundImage;
                            updateHelper.boolValue = true;
                            updateHelper.boolValue = false;
                        }
                    }

                    break;

                case 3:
                    LSS_EditorHandler.DrawHeader(customSkin, "Resources Header", 6);

                    LSS_EditorHandler.DrawProperty(canvasGroup, customSkin, "Main CG");
                    LSS_EditorHandler.DrawProperty(backgroundCanvasGroup, customSkin, "Background CG");
                    LSS_EditorHandler.DrawProperty(contentCanvasGroup, customSkin, "Content CG");
                    LSS_EditorHandler.DrawProperty(pakCanvasGroup, customSkin, "PAK Canvas Group");
                    LSS_EditorHandler.DrawProperty(statusObj, customSkin, "Status Object");
                    LSS_EditorHandler.DrawProperty(titleObj, customSkin, "Title Object");
                    LSS_EditorHandler.DrawProperty(descriptionObj, customSkin, "Description Object");
                    if (enableRandomHints.boolValue == true) { LSS_EditorHandler.DrawProperty(hintsText, customSkin, "Hints Text"); }
                    LSS_EditorHandler.DrawProperty(progressBar, customSkin, "Progress Bar");
                    LSS_EditorHandler.DrawProperty(spinnerParent, customSkin, "Spinner Parent");

                    if (enableRandomImages.boolValue == true)
                    {
                        LSS_EditorHandler.DrawProperty(imageObject, customSkin, "Image Object");
                    }

                    if (enablePressAnyKey.boolValue == true)
                    {
                        LSS_EditorHandler.DrawProperty(pakTextObj, customSkin, "PAK Text Object");
                        LSS_EditorHandler.DrawProperty(pakCountdownSlider, customSkin, "PAK Countdown");
                        LSS_EditorHandler.DrawProperty(pakCountdownLabel, customSkin, "PAK Countdown Label");
                    }

                    break;

                case 4:
                    LSS_EditorHandler.DrawHeader(customSkin, "Settings Header", 6);
                    LSS_EditorHandler.DrawPropertyCW(fadeSpeed, customSkin, "Transition Speed", "Sets the overall transition speed.", 140);
                    LSS_EditorHandler.DrawPropertyCW(backgroundFadeSpeed, customSkin, "BG Transition Speed", "Sets the background transition speed.", 140);
                    LSS_EditorHandler.DrawPropertyCW(contentFadeSpeed, customSkin, "Content Trans. Speed", "Sets the content transition speed.", 140);
                    LSS_EditorHandler.DrawPropertyCW(projectorCamera, customSkin, "Projector Camera", "Renders the camera inside the loading screen.", 140);
                    setTimeScale.boolValue = LSS_EditorHandler.DrawToggle(setTimeScale.boolValue, customSkin, "Set Time Scale To Default", "Sets Time Scale to 1 on load call.");
                    customSceneActivation.boolValue = LSS_EditorHandler.DrawToggle(customSceneActivation.boolValue, customSkin, "Custom Scene Activation [Experimental]", "Bypasses the default activation process, aims to help transition stutters for heavy-weight scenes.");

                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.Space(-3);
                    enableVirtualLoading.boolValue = LSS_EditorHandler.DrawTogglePlain(enableVirtualLoading.boolValue, customSkin, "Enable Virtual Loading", "Enables the virtual loading feature. See docs for more information.");
                    GUILayout.Space(3);

                    if (enableVirtualLoading.boolValue == true)
                    {
                        LSS_EditorHandler.DrawProperty(virtualLoadingTimer, customSkin, "Loading Duration", "Sets the virtual loading duration.");
                    }

                    GUILayout.EndVertical();

                    if (customSceneActivation.boolValue == true) { GUI.enabled = false; }

                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.Space(-3);
                    enablePressAnyKey.boolValue = LSS_EditorHandler.DrawTogglePlain(enablePressAnyKey.boolValue, customSkin, "Enable Press Any Key", "Enables the press any key feature.");
                    GUILayout.Space(3);

                    if (enablePressAnyKey.boolValue == true)
                    {
                        GUILayout.BeginVertical(EditorStyles.helpBox);
                        GUILayout.Space(-3);
                        useCountdown.boolValue = LSS_EditorHandler.DrawTogglePlain(useCountdown.boolValue, customSkin, "Use Countdown", "Use countdown to deactive the loading screen.");
                        GUILayout.Space(3);

                        if (useCountdown.boolValue == true)
                        {
                            LSS_EditorHandler.DrawProperty(pakCountdownTimer, customSkin, "PAK Countdown", "Sets the press any key countdown duration.");
                        }
                      
                        GUILayout.EndVertical();
                       
                        GUILayout.BeginVertical(EditorStyles.helpBox);
                        GUILayout.Space(-3);
                        useSpecificKey.boolValue = LSS_EditorHandler.DrawTogglePlain(useSpecificKey.boolValue, customSkin, "Use Specific PAK Key", "Allows to set a specific key for the press any key feature.");
                        GUILayout.Space(3);

                        if (useSpecificKey.boolValue == true)
                        {
                            LSS_EditorHandler.DrawProperty(keyCode, customSkin, "Key Code", "Set a key for the press any key feature.");
                        }

                        GUILayout.EndVertical();

                        waitForPlayerInput.boolValue = LSS_EditorHandler.DrawToggle(waitForPlayerInput.boolValue, customSkin, "Wait For Player Input", "Wait for player input to active the loaded scene.");
                    }

                    GUILayout.EndVertical();
                    GUI.enabled = true;

                    LSS_EditorHandler.DrawHeader(customSkin, "Events Header", 10);
                    EditorGUILayout.PropertyField(onLoadingStart);
                    EditorGUILayout.PropertyField(onLoadingEnd);
                    EditorGUILayout.PropertyField(onLoadingDestroy);
                    break;
            }

            serializedObject.ApplyModifiedProperties();
            if (Application.isPlaying == false) { Repaint(); }
        }
    }
}
#endif