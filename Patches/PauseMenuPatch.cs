using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EfDEnhanced.Utils;
using System;
using System.Reflection;
using Duckov.Options.UI;

namespace EfDEnhanced.Patches
{
    /// <summary>
    /// Adds "Mod Settings" button to the PauseMenu
    /// </summary>
    [HarmonyPatch(typeof(PauseMenu))]
    public class PauseMenuPatch
    {
        private static bool buttonAdded = false;

        /// <summary>
        /// Patch PauseMenu.Start to add mod settings button
        /// </summary>
        [HarmonyPatch("Show")]
        [HarmonyPostfix]
        public static void Show_Postfix()
        {
            try
            {
                if (buttonAdded)
                {
                    ModLogger.Log("PauseMenuPatch", "Mod Settings button already added");
                    return;
                }

                ModLogger.Log("PauseMenuPatch", "Adding Mod Settings button to PauseMenu");

                var __instance = PauseMenu.Instance;

                // Find the Options button (Btn_Options) to use as template
                // Note: PauseMenu.Instance.transform is already the "Menu" node
                Transform optionsButtonTransform = __instance.transform.Find("Layout/Btn_Options");
                if (optionsButtonTransform == null)
                {
                    ModLogger.LogError("PauseMenuPatch: Could not find Btn_Options");
                    return;
                }

                Button settingsButton = optionsButtonTransform.GetComponent<Button>();
                if (settingsButton == null)
                {
                    ModLogger.LogError("PauseMenuPatch: Btn_Options does not have Button component");
                    return;
                }

                ModLogger.Log("PauseMenuPatch", $"Found options button: {settingsButton.gameObject.name}");

                // Clone the settings button
                GameObject modSettingsButtonObj = GameObject.Instantiate(settingsButton.gameObject, settingsButton.transform.parent);
                modSettingsButtonObj.name = "ModSettingsButton";

                // Position it after the settings button
                int settingsIndex = settingsButton.transform.GetSiblingIndex();
                modSettingsButtonObj.transform.SetSiblingIndex(settingsIndex + 1);

                // Remove localization components
                var localizationComponents = modSettingsButtonObj.GetComponentsInChildren<Component>();
                foreach (var comp in localizationComponents)
                {
                    var typeName = comp.GetType().Name;
                    if (typeName.Contains("Localized") || typeName.Contains("Localizor"))
                    {
                        ModLogger.Log("PauseMenuPatch", $"Removing localization component: {typeName}");
                        GameObject.Destroy(comp);
                    }
                }

                // Update button text
                TextMeshProUGUI? buttonText = modSettingsButtonObj.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    string modSettingsText = LocalizationHelper.Get("Settings_ModSettings_Button");
                    buttonText.text = modSettingsText;
                    buttonText.SetText(modSettingsText);
                    buttonText.ForceMeshUpdate();
                    ModLogger.Log("PauseMenuPatch", $"Set button text to: {modSettingsText}");
                }

                // Setup button click event
                Button modButton = modSettingsButtonObj.GetComponent<Button>();
                if (modButton != null)
                {
                    // Remove old listeners
                    modButton.onClick.RemoveAllListeners();
                    
                    // Add new listener to open options panel with mod settings tab selected
                    modButton.onClick.AddListener(() => OpenModSettings());
                    
                    ModLogger.Log("PauseMenuPatch", "Setup button click event");
                }

                buttonAdded = true;
                ModLogger.Log("PauseMenuPatch", "Successfully added Mod Settings button to PauseMenu");
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"PauseMenuPatch.Show_Postfix failed: {ex}");
            }
        }

        /// <summary>
        /// Open OptionsPanel and select Mod Settings tab
        /// </summary>
        private static void OpenModSettings()
        {
            try
            {
                ModLogger.Log("PauseMenuPatch", "Opening Mod Settings");

                // Set flag to select mod settings tab
                OptionsPanelPatch.ShouldSelectModSettingsTab = true;

                // Find and open the OptionsPanel
                var optionsPanel = GameObject.FindObjectOfType<OptionsPanel>();
                if (optionsPanel != null)
                {
                    // Call Open method using reflection
                    var openMethod = typeof(OptionsPanel).GetMethod("Open", BindingFlags.Public | BindingFlags.Instance);
                    if (openMethod != null)
                    {
                        openMethod.Invoke(optionsPanel, null);
                    }
                    else
                    {
                        ModLogger.LogError("PauseMenuPatch: Could not find Open method on OptionsPanel");
                    }
                }
                else
                {
                    ModLogger.Log("PauseMenuPatch: Could not find OptionsPanel instance");
                }
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"PauseMenuPatch.OpenModSettings failed: {ex}");
            }
        }
    }
}

