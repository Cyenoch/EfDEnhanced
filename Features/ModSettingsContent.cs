using System;
using EfDEnhanced.Utils;
using EfDEnhanced.Utils.UI.Builders;
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
        private SettingsBuilder? _settingsBuilder;
        private bool _isBuilt = false;

        /// <summary>
        /// Build the settings content (called by patch)
        /// </summary>
        public void BuildContent()
        {
            try
            {
                // Prevent rebuilding if already built
                if (_isBuilt)
                {
                    ModLogger.Log("ModSettingsContent", "Mod settings content already built, skipping rebuild");
                    return;
                }

                ModLogger.Log("ModSettingsContent", "Building mod settings tab content");

                // Create scroll view and form
                CreateScrollViewAndBuildForm();

                _isBuilt = true;
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

            // Add VerticalLayoutGroup for automatic layout (only if not already present)
            var layoutGroup = GetComponent<VerticalLayoutGroup>();
            if (layoutGroup == null)
            {
                layoutGroup = gameObject.AddComponent<VerticalLayoutGroup>();
            }
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = false; // Let children control their own height
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.spacing = 4;
            layoutGroup.padding = new RectOffset(20, 20, 8, 20); // Add padding: left, right, top, bottom

            // Add ContentSizeFitter to automatically adjust content height (only if not already present)
            var sizeFitter = GetComponent<ContentSizeFitter>();
            if (sizeFitter == null)
            {
                sizeFitter = gameObject.AddComponent<ContentSizeFitter>();
            }
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            // Build form using SettingsBuilder directly on this GameObject
            BuildFormWithSettingsBuilder(transform);
        }

        /// <summary>
        /// Build the entire settings form using SettingsBuilder
        /// </summary>
        private void BuildFormWithSettingsBuilder(Transform parent)
        {
            try
            {
                _settingsBuilder = new SettingsBuilder(parent);

                // Build the form using our new builder pattern
                _settingsBuilder
                    // UI Enhancement Section
                    .AddSection("Settings_Category_UI")
                    .AddSetting(ModSettings.EnableWeaponComparison)
                    .AddSetting(ModSettings.FastBuyEnabled)
                    .AddSetting(ModSettings.FastSellEnabled)
                    .AddSetting(ModSettings.ItemWheelScale)
                    .AddSetting(ModSettings.ItemWheelMenuHotkey)
                    .AddSetting(ModSettings.ThrowableWheelHotkey)

                    // Pre-Raid Check Section
                    .AddSection("Settings_Category_PreRaidCheck")
                    .AddSetting(ModSettings.EnableRaidCheck)
                    .AddSetting(ModSettings.CheckWeapon, ModSettings.EnableRaidCheck)
                    .AddSetting(ModSettings.CheckAmmo, ModSettings.EnableRaidCheck)
                    .AddSetting(ModSettings.CheckMeds, ModSettings.EnableRaidCheck)
                    .AddSetting(ModSettings.CheckFood, ModSettings.EnableRaidCheck)
                    .AddSetting(ModSettings.CheckQuestItems, ModSettings.EnableRaidCheck)
                    .AddSetting(ModSettings.CheckQuestWeapons, ModSettings.EnableRaidCheck)
                    .AddSetting(ModSettings.CheckWeather, ModSettings.EnableRaidCheck)

                    // Movement Enhancement Section
                    .AddSection("Settings_Category_Movement")
                    .AddSetting(ModSettings.MovementEnhancement)

                    // Quest Tracker Section
                    .AddSection("Settings_Category_QuestTracker")
                    .AddSetting(ModSettings.EnableQuestTracker)
                    .AddSetting(ModSettings.TrackerToggleHotkey, ModSettings.EnableQuestTracker)
                    .AddSetting(ModSettings.AutoTrackNewQuests, ModSettings.EnableQuestTracker)
                    .AddSetting(ModSettings.TrackerShowDescription, ModSettings.EnableQuestTracker)
                    .AddSetting(ModSettings.TrackerFilterByMap, ModSettings.EnableQuestTracker)
                    .AddSetting(ModSettings.TrackerPositionX, ModSettings.EnableQuestTracker)
                    .AddSetting(ModSettings.TrackerPositionY, ModSettings.EnableQuestTracker)
                    .AddSetting(ModSettings.TrackerScale, ModSettings.EnableQuestTracker)

                    // Reset button at the bottom
                    .AddButton("Settings_ResetButton", OnResetButtonClicked, UIStyles.ButtonStyle.Danger);

                ModLogger.Log("ModSettingsContent", "Form built successfully with SettingsBuilder");
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Failed to build form with SettingsBuilder: {ex}");
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

