using System;
using EfDEnhanced.Utils;
using EfDEnhanced.Utils.UI.Builders;
using EfDEnhanced.Utils.UI.Components;
using EfDEnhanced.Utils.UI.Constants;
using UnityEngine;
using UnityEngine.UI;

namespace EfDEnhanced.Features
{
    /// <summary>
    /// Mod settings content for embedding in game's OptionsPanel tab
    /// This is a lightweight version without its own Canvas, designed to be used as tab content
    /// </summary>
    public class ModSettingsContent : MonoBehaviour
    {
        private FormBuilder? _formBuilder;

        /// <summary>
        /// Build the settings content (called by patch)
        /// </summary>
        public void BuildContent()
        {
            try
            {
                ModLogger.Log("ModSettingsContent", "Building mod settings tab content");

                // Create scroll view and form
                CreateScrollViewAndBuildForm();

                ModLogger.Log("ModSettingsContent", "Mod settings tab content built successfully");
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Failed to build mod settings content: {ex}");
            }
        }

        private void CreateScrollViewAndBuildForm()
        {
            // The parent (this.transform) should be positioned as a sibling to Common, Audio, Graphics
            // We just need to add content directly here without creating another ScrollView
            // because the game's OptionsPanel already has a ScrollView that will handle all tab contents

            // Add VerticalLayoutGroup for automatic layout
            var layoutGroup = gameObject.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = false; // Let children control their own height
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.spacing = UIConstants.SETTINGS_ENTRY_SPACING;
            layoutGroup.padding = new RectOffset(20, 20, 20, 20); // Add padding on all sides

            // Add ContentSizeFitter to automatically adjust content height
            var sizeFitter = gameObject.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            // Build form using FormBuilder directly on this GameObject
            BuildFormWithFormBuilder(transform);
        }

        /// <summary>
        /// Build the entire settings form using FormBuilder
        /// </summary>
        private void BuildFormWithFormBuilder(Transform parent)
        {
            try
            {
                _formBuilder = new FormBuilder(parent);

                // Build the form using our elegant builder pattern
                _formBuilder
                    // UI Enhancement Section
                    .AddSection("Settings_Category_UI")
                    .AddToggle("Settings_EnableWeaponComparison_Name", ModSettings.EnableWeaponComparison)
                    .AddSpacer()

                    // Pre-Raid Check Section
                    .AddSection("Settings_Category_PreRaidCheck")
                    .AddToggle("Settings_EnableRaidCheck_Name", ModSettings.EnableRaidCheck)
                    .AddToggle("Settings_CheckWeapon_Name", ModSettings.CheckWeapon, ModSettings.EnableRaidCheck, 30)
                    .AddToggle("Settings_CheckAmmo_Name", ModSettings.CheckAmmo, ModSettings.EnableRaidCheck, 30)
                    .AddToggle("Settings_CheckMeds_Name", ModSettings.CheckMeds, ModSettings.EnableRaidCheck, 30)
                    .AddToggle("Settings_CheckFood_Name", ModSettings.CheckFood, ModSettings.EnableRaidCheck, 30)
                    .AddToggle("Settings_CheckWeather_Name", ModSettings.CheckWeather, ModSettings.EnableRaidCheck, 30)
                    .AddSpacer()

                    // Movement Enhancement Section
                    .AddSection("Settings_Category_Movement")
                    .AddDropdown("Settings_MovementEnhancement_Name", ModSettings.MovementEnhancement)
                    .AddSpacer()

                    // Quest Tracker Section
                    .AddSection("Settings_Category_QuestTracker")
                    .AddToggle("Settings_EnableQuestTracker_Name", ModSettings.EnableQuestTracker)
                    .AddToggle("Settings_TrackerShowDescription_Name", ModSettings.TrackerShowDescription, ModSettings.EnableQuestTracker, 30)
                    .AddToggle("Settings_TrackerFilterByMap_Name", ModSettings.TrackerFilterByMap, ModSettings.EnableQuestTracker, 30)
                    .AddSlider("Settings_TrackerPositionX_Name", 0f, 1f, ModSettings.TrackerPositionX, visibilityCondition: ModSettings.EnableQuestTracker, leftPadding: 30)
                    .AddSlider("Settings_TrackerPositionY_Name", 0f, 1f, ModSettings.TrackerPositionY, visibilityCondition: ModSettings.EnableQuestTracker, leftPadding: 30)
                    .AddSlider("Settings_TrackerScale_Name", 0.5f, 2f, ModSettings.TrackerScale, visibilityCondition: ModSettings.EnableQuestTracker, leftPadding: 30)
                    .AddSpacer()

                    // Reset button at the bottom
                    .AddButton("Settings_ResetButton", OnResetButtonClicked, UIStyles.ButtonStyle.Danger);

                ModLogger.Log("ModSettingsContent", "Form built successfully with FormBuilder");
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Failed to build form with FormBuilder: {ex}");
            }
        }

        private void OnResetButtonClicked()
        {
            try
            {
                ModLogger.Log("ModSettingsContent", "Reset to defaults button clicked");
                ModSettings.ResetToDefaults();
                ModLogger.Log("ModSettingsContent", "Settings reset complete - UI automatically updates via data binding");
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Error during settings reset: {ex}");
            }
        }
    }
}

