using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EfDEnhanced.Utils;
using EfDEnhanced.Features;
using Duckov.Options.UI;
using System.Collections.Generic;
using System.Reflection;
using System;

namespace EfDEnhanced.Patches
{
    /// <summary>
    /// Adds "Mod Settings" tab to the game's OptionsPanel
    /// </summary>
    [HarmonyPatch(typeof(OptionsPanel))]
    public class OptionsPanelPatch
    {
        private static ModSettingsContent? modSettingsContent;
        
        /// <summary>
        /// Flag to indicate if mod settings tab should be selected when panel opens
        /// Set this to true before opening the panel to auto-select mod settings
        /// </summary>
        public static bool ShouldSelectModSettingsTab { get; set; } = false;

        /// <summary>
        /// Patch OptionsPanel.Start to add our mod settings tab
        /// This is called when the panel is first created (e.g., main menu)
        /// </summary>
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void Start_Postfix(OptionsPanel __instance)
        {
            AddModSettingsTab(__instance);
        }

        /// <summary>
        /// Patch OptionsPanel.OnOpen to add our mod settings tab
        /// This is called every time the options panel is opened (e.g., pause menu in-game)
        /// </summary>
        [HarmonyPatch("OnOpen")]
        [HarmonyPostfix]
        public static void OnOpen_Postfix(OptionsPanel __instance)
        {
            AddModSettingsTab(__instance);
            
            // Check if we should auto-select mod settings tab
            if (ShouldSelectModSettingsTab)
            {
                SelectModSettingsTab(__instance);
                ShouldSelectModSettingsTab = false; // Reset flag
            }
        }

        /// <summary>
        /// Shared method to add mod settings tab to the options panel
        /// </summary>
        private static void AddModSettingsTab(OptionsPanel __instance)
        {
            try
            {
                ModLogger.Log("OptionsPanelPatch", "Checking if Mod Settings tab needs to be added");

                // Get the tabButtons list using reflection
                var tabButtonsField = typeof(OptionsPanel).GetField("tabButtons", BindingFlags.NonPublic | BindingFlags.Instance);
                if (tabButtonsField == null)
                {
                    ModLogger.LogError("OptionsPanelPatch: Could not find tabButtons field");
                    return;
                }

                var tabButtons = tabButtonsField.GetValue(__instance) as List<OptionsPanel_TabButton>;
                if (tabButtons == null || tabButtons.Count == 0)
                {
                    ModLogger.LogError("OptionsPanelPatch: tabButtons is null or empty");
                    return;
                }

                // Check if our mod settings tab already exists
                foreach (var btn in tabButtons)
                {
                    if (btn != null && btn.gameObject.name == "ModSettingsTabButton")
                    {
                        ModLogger.Log("OptionsPanelPatch", "Mod Settings tab already exists, skipping");
                        return;
                    }
                }

                ModLogger.Log("OptionsPanelPatch", "Adding Mod Settings tab to OptionsPanel");

                ModLogger.Log("OptionsPanelPatch", $"Found {tabButtons.Count} existing tabs");

                // Find the first tab button as template
                var templateButton = tabButtons[0];
                if (templateButton == null)
                {
                    ModLogger.LogError("OptionsPanelPatch: Template button is null");
                    return;
                }

                // Get the parent container (should be the same parent as other tab buttons)
                Transform tabButtonParent = templateButton.transform.parent;
                if (tabButtonParent == null)
                {
                    ModLogger.LogError("OptionsPanelPatch: Could not find tab button parent");
                    return;
                }

                ModLogger.Log("OptionsPanelPatch", $"Tab button parent: {tabButtonParent.name}");

                // Clone the template button
                GameObject modTabButtonObj = GameObject.Instantiate(templateButton.gameObject, tabButtonParent);
                modTabButtonObj.name = "ModSettingsTabButton";

                // Get the tab button component
                OptionsPanel_TabButton modTabButton = modTabButtonObj.GetComponent<OptionsPanel_TabButton>();
                if (modTabButton == null)
                {
                    ModLogger.LogError("OptionsPanelPatch: Failed to get OptionsPanel_TabButton component");
                    GameObject.Destroy(modTabButtonObj);
                    return;
                }

                // Hide the selected indicator initially (we're not selected by default)
                var selectedIndicatorField = typeof(OptionsPanel_TabButton).GetField("selectedIndicator", BindingFlags.NonPublic | BindingFlags.Instance);
                if (selectedIndicatorField != null)
                {
                    var selectedIndicator = selectedIndicatorField.GetValue(modTabButton) as GameObject;
                    if (selectedIndicator != null)
                    {
                        selectedIndicator.SetActive(false);
                        ModLogger.Log("OptionsPanelPatch", "Deactivated selectedIndicator for mod tab");
                    }
                }
                else
                {
                    ModLogger.LogWarning("OptionsPanelPatch: Could not find selectedIndicator field");
                }

                // Remove any localization components that might interfere with our text
                var localizationComponents = modTabButtonObj.GetComponentsInChildren<Component>();
                foreach (var comp in localizationComponents)
                {
                    var typeName = comp.GetType().Name;
                    if (typeName.Contains("Localized") || typeName.Contains("Localizor"))
                    {
                        ModLogger.Log("OptionsPanelPatch", $"Removing localization component: {typeName}");
                        GameObject.Destroy(comp);
                    }
                }

                // Update button text - force update to avoid sharing with template
                TextMeshProUGUI? buttonText = modTabButtonObj.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    string modSettingsText = LocalizationHelper.Get("Settings_ModSettings_Button");
                    buttonText.text = modSettingsText;
                    // Force the text to update and not be linked to the original
                    buttonText.SetText(modSettingsText);
                    buttonText.ForceMeshUpdate();
                    ModLogger.Log("OptionsPanelPatch", $"Set button text to: {modSettingsText}");
                }
                else
                {
                    ModLogger.LogWarning("OptionsPanelPatch: Could not find TextMeshProUGUI component in mod tab button");
                }

                // Get the tab content field from template to understand structure
                var tabField = typeof(OptionsPanel_TabButton).GetField("tab", BindingFlags.NonPublic | BindingFlags.Instance);
                if (tabField == null)
                {
                    ModLogger.LogError("OptionsPanelPatch: Could not find tab field in OptionsPanel_TabButton");
                    GameObject.Destroy(modTabButtonObj);
                    return;
                }

                // Get the original tab GameObject to use as reference
                var originalTab = tabField.GetValue(templateButton) as GameObject;
                if (originalTab == null)
                {
                    ModLogger.LogError("OptionsPanelPatch: Original tab is null");
                    GameObject.Destroy(modTabButtonObj);
                    return;
                }

                // Get the parent where all tab contents are located
                // From the tree: ScrollView/Viewport/Content contains all tab content GameObjects
                Transform tabContentParent = originalTab.transform.parent;
                if (tabContentParent == null)
                {
                    ModLogger.LogError("OptionsPanelPatch: Could not find tab content parent");
                    GameObject.Destroy(modTabButtonObj);
                    return;
                }

                ModLogger.Log("OptionsPanelPatch", $"Tab content parent: {tabContentParent.name}");

                // Create our mod settings tab content as a sibling to Common, Audio, Graphics, etc.
                GameObject modTabContent = new GameObject("ModSettings");
                modTabContent.transform.SetParent(tabContentParent, false);

                // Copy RectTransform settings from original tab
                RectTransform modTabRect = modTabContent.AddComponent<RectTransform>();
                RectTransform originalTabRect = originalTab.GetComponent<RectTransform>();
                if (originalTabRect != null)
                {
                    modTabRect.anchorMin = originalTabRect.anchorMin;
                    modTabRect.anchorMax = originalTabRect.anchorMax;
                    modTabRect.sizeDelta = originalTabRect.sizeDelta;
                    modTabRect.anchoredPosition = originalTabRect.anchoredPosition;
                    modTabRect.pivot = originalTabRect.pivot;
                }
                else
                {
                    // Fallback: fill the parent
                    modTabRect.anchorMin = Vector2.zero;
                    modTabRect.anchorMax = Vector2.one;
                    modTabRect.sizeDelta = Vector2.zero;
                    modTabRect.anchoredPosition = Vector2.zero;
                }

                // Add our ModSettingsContent component
                modSettingsContent = modTabContent.AddComponent<ModSettingsContent>();
                modSettingsContent.BuildContent();

                // Set the tab field in our new button to point to our content
                tabField.SetValue(modTabButton, modTabContent);

                // Initially hide the tab content (it will be shown when button is clicked)
                modTabContent.SetActive(false);

                // Add our new tab button to the list
                tabButtons.Add(modTabButton);
                tabButtonsField.SetValue(__instance, tabButtons);

                // IMPORTANT: Setup the click event for our button
                // The OptionsPanel.Setup() has already run, so we need to manually add the event
                SetupTabButtonClickEvent(modTabButton, __instance);

                ModLogger.Log("OptionsPanelPatch", $"Successfully added mod settings tab. Total tabs: {tabButtons.Count}");
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"OptionsPanelPatch.AddModSettingsTab failed: {ex}");
            }
        }

        /// <summary>
        /// Select the Mod Settings tab
        /// </summary>
        private static void SelectModSettingsTab(OptionsPanel __instance)
        {
            try
            {
                ModLogger.Log("OptionsPanelPatch", "Attempting to select Mod Settings tab");

                // Get the tabButtons list
                var tabButtonsField = typeof(OptionsPanel).GetField("tabButtons", BindingFlags.NonPublic | BindingFlags.Instance);
                if (tabButtonsField == null)
                {
                    ModLogger.LogError("OptionsPanelPatch: Could not find tabButtons field for selection");
                    return;
                }

                var tabButtons = tabButtonsField.GetValue(__instance) as List<OptionsPanel_TabButton>;
                if (tabButtons == null || tabButtons.Count == 0)
                {
                    ModLogger.LogError("OptionsPanelPatch: tabButtons is null or empty for selection");
                    return;
                }

                // Find the Mod Settings tab button
                OptionsPanel_TabButton? modSettingsButton = null;
                foreach (var btn in tabButtons)
                {
                    if (btn != null && btn.gameObject.name == "ModSettingsTabButton")
                    {
                        modSettingsButton = btn;
                        break;
                    }
                }

                if (modSettingsButton == null)
                {
                    ModLogger.LogError("OptionsPanelPatch: Could not find ModSettingsTabButton for selection");
                    return;
                }

                // Use SetSelection method to select our tab
                var setSelectionMethod = typeof(OptionsPanel).GetMethod("SetSelection", BindingFlags.Public | BindingFlags.Instance);
                if (setSelectionMethod != null)
                {
                    setSelectionMethod.Invoke(__instance, new object[] { modSettingsButton });
                    ModLogger.Log("OptionsPanelPatch", "Successfully selected Mod Settings tab");
                }
                else
                {
                    ModLogger.LogError("OptionsPanelPatch: Could not find SetSelection method");
                }
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"OptionsPanelPatch.SelectModSettingsTab failed: {ex}");
            }
        }

        /// <summary>
        /// Setup the click event for our mod tab button
        /// This mimics what OptionsPanel.Setup() does for built-in tabs
        /// </summary>
        private static void SetupTabButtonClickEvent(OptionsPanel_TabButton button, OptionsPanel panel)
        {
            try
            {
                // Get the onClicked field
                var onClickedField = typeof(OptionsPanel_TabButton).GetField("onClicked", BindingFlags.Public | BindingFlags.Instance);
                if (onClickedField == null)
                {
                    ModLogger.LogError("OptionsPanelPatch: Could not find onClicked field in OptionsPanel_TabButton");
                    return;
                }

                // Get the OnTabButtonClicked method from OptionsPanel
                var onTabButtonClickedMethod = typeof(OptionsPanel).GetMethod("OnTabButtonClicked", BindingFlags.NonPublic | BindingFlags.Instance);
                if (onTabButtonClickedMethod == null)
                {
                    ModLogger.LogError("OptionsPanelPatch: Could not find OnTabButtonClicked method in OptionsPanel");
                    return;
                }

                // Create a delegate for the click handler
                var clickHandler = Delegate.CreateDelegate(
                    typeof(Action<OptionsPanel_TabButton, UnityEngine.EventSystems.PointerEventData>),
                    panel,
                    onTabButtonClickedMethod
                );

                // Get the current value of onClicked
                var currentOnClicked = onClickedField.GetValue(button) as Delegate;

                // Combine with existing delegates (if any)
                var newOnClicked = Delegate.Combine(currentOnClicked, clickHandler);

                // Set the combined delegate back
                onClickedField.SetValue(button, newOnClicked);

                ModLogger.Log("OptionsPanelPatch", "Successfully setup click event for mod settings tab button");
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"OptionsPanelPatch.SetupTabButtonClickEvent failed: {ex}");
            }
        }

    }
}

