using System;
using System.Collections.Generic;
using System.Linq;
using EfDEnhanced.Utils.Settings;

namespace EfDEnhanced.Utils
{
    /// <summary>
    /// Centralized settings manager for EfDEnhanced mod
    /// Uses SettingsEntry system with events and persistence
    /// </summary>
    public static class ModSettings
    {
        private const string PREFIX = "EfDEnhanced";

        // Settings category localization keys
        private const string CATEGORY_RAID_CHECK = "Settings_Category_PreRaidCheck";
        private const string CATEGORY_QUEST_TRACKER = "Settings_Category_QuestTracker";
        private const string CATEGORY_MOVEMENT = "Settings_Category_Movement";
        private const string CATEGORY_UI = "Settings_Category_UI";

        // All settings entries
        private static readonly List<ISettingsEntry> _allSettings = [];

        #region Pre-Raid Check Settings

        public static readonly BoolSettingsEntry EnableRaidCheck = Register(
            new BoolSettingsEntry(
                PREFIX,
                "EnableRaidCheck",
                "Settings_EnableRaidCheck_Name",
                true,
                CATEGORY_RAID_CHECK,
                "Settings_EnableRaidCheck_Desc"
            )
        );

        public static readonly BoolSettingsEntry CheckWeapon = Register(
            new BoolSettingsEntry(
                PREFIX,
                "CheckWeapon",
                "Settings_CheckWeapon_Name",
                true,
                CATEGORY_RAID_CHECK,
                "Settings_CheckWeapon_Desc"
            )
        );

        public static readonly BoolSettingsEntry CheckAmmo = Register(
            new BoolSettingsEntry(
                PREFIX,
                "CheckAmmo",
                "Settings_CheckAmmo_Name",
                true,
                CATEGORY_RAID_CHECK,
                "Settings_CheckAmmo_Desc"
            )
        );

        public static readonly BoolSettingsEntry CheckMeds = Register(
            new BoolSettingsEntry(
                PREFIX,
                "CheckMeds",
                "Settings_CheckMeds_Name",
                true,
                CATEGORY_RAID_CHECK,
                "Settings_CheckMeds_Desc"
            )
        );

        public static readonly BoolSettingsEntry CheckFood = Register(
            new BoolSettingsEntry(
                PREFIX,
                "CheckFood",
                "Settings_CheckFood_Name",
                true,
                CATEGORY_RAID_CHECK,
                "Settings_CheckFood_Desc"
            )
        );

        public static readonly BoolSettingsEntry CheckWeather = Register(
            new BoolSettingsEntry(
                PREFIX,
                "CheckWeather",
                "Settings_CheckWeather_Name",
                true,
                CATEGORY_RAID_CHECK,
                "Settings_CheckWeather_Desc"
            )
        );

        #endregion

        #region Active Quest Tracker Settings

        public static readonly BoolSettingsEntry EnableQuestTracker = Register(
            new BoolSettingsEntry(
                PREFIX,
                "EnableQuestTracker",
                "Settings_EnableQuestTracker_Name",
                true,
                CATEGORY_QUEST_TRACKER,
                "Settings_EnableQuestTracker_Desc"
            )
        );

        public static readonly RangedFloatSettingsEntry TrackerPositionX = Register(
            new RangedFloatSettingsEntry(
                PREFIX,
                "TrackerPosX",
                "Settings_TrackerPositionX_Name",
                0f,
                0f,
                1f,
                CATEGORY_QUEST_TRACKER,
                "Settings_TrackerPositionX_Desc"
            )
        );

        public static readonly RangedFloatSettingsEntry TrackerPositionY = Register(
            new RangedFloatSettingsEntry(
                PREFIX,
                "TrackerPosY",
                "Settings_TrackerPositionY_Name",
                0.07f,
                0f,
                1f,
                CATEGORY_QUEST_TRACKER,
                "Settings_TrackerPositionY_Desc"
            )
        );

        public static readonly RangedFloatSettingsEntry TrackerScale = Register(
            new RangedFloatSettingsEntry(
                PREFIX,
                "TrackerScale",
                "Settings_TrackerScale_Name",
                1.2f,
                0.5f,
                3.0f,
                CATEGORY_QUEST_TRACKER,
                "Settings_TrackerScale_Desc"
            )
        );

        public static readonly BoolSettingsEntry TrackerShowDescription = Register(
            new BoolSettingsEntry(
                PREFIX,
                "TrackerShowDescription",
                "Settings_TrackerShowDescription_Name",
                false,
                CATEGORY_QUEST_TRACKER,
                "Settings_TrackerShowDescription_Desc"
            )
        );

        public static readonly BoolSettingsEntry TrackerFilterByMap = Register(
            new BoolSettingsEntry(
                PREFIX,
                "TrackerFilterByMap",
                "Settings_TrackerFilterByMap_Name",
                true,
                CATEGORY_QUEST_TRACKER,
                "Settings_TrackerFilterByMap_Desc"
            )
        );

        public static readonly KeyCodeSettingsEntry TrackerToggleHotkey = Register(
            new KeyCodeSettingsEntry(
                PREFIX,
                "TrackerToggleHotkey",
                "Settings_TrackerToggleHotkey_Name",
                UnityEngine.KeyCode.Period, // Default: . key
                CATEGORY_QUEST_TRACKER,
                "Settings_TrackerToggleHotkey_Desc"
            )
        );

        public static readonly BoolSettingsEntry TrackerHotkeyUsed = Register(
            new BoolSettingsEntry(
                PREFIX,
                "TrackerHotkeyUsed",
                "Settings_TrackerHotkeyUsed_Name",
                false,
                CATEGORY_QUEST_TRACKER,
                "Settings_TrackerHotkeyUsed_Desc"
            )
        );

        public static readonly BoolSettingsEntry AutoTrackNewQuests = Register(
            new BoolSettingsEntry(
                PREFIX,
                "AutoTrackNewQuests",
                "Settings_AutoTrackNewQuests_Name",
                true,
                CATEGORY_QUEST_TRACKER,
                "Settings_AutoTrackNewQuests_Desc"
            )
        );

        #endregion

        #region Movement Enhancement Settings

        public static readonly IndexedOptionsSettingsEntry MovementEnhancement = Register(
            new IndexedOptionsSettingsEntry(
                PREFIX,
                "MovementEnhancement",
                "Settings_MovementEnhancement_Name",
                0, // Default: Medium optimization
                [
                    "Settings_Movement_Disabled",
                    "Settings_Movement_Light",
                    "Settings_Movement_Medium",
                    "Settings_Movement_Heavy"
                ],
                CATEGORY_MOVEMENT,
                "Settings_MovementEnhancement_Desc"
            )
        );

        #endregion

        #region UI Enhancement Settings

        public static readonly BoolSettingsEntry EnableWeaponComparison = Register(
            new BoolSettingsEntry(
                PREFIX,
                "EnableWeaponComparison",
                "Settings_EnableWeaponComparison_Name",
                true,
                CATEGORY_UI,
                "Settings_EnableWeaponComparison_Desc"
            )
        );

        public static readonly RangedFloatSettingsEntry ItemWheelScale = Register(
            new RangedFloatSettingsEntry(
                PREFIX,
                "ItemWheelScale",
                "Settings_ItemWheelScale_Name",
                1.0f,
                0.5f,
                3.0f,
                CATEGORY_UI,
                "Settings_ItemWheelScale_Desc"
            )
        );

        public static readonly KeyCodeSettingsEntry ItemWheelMenuHotkey = Register(
            new KeyCodeSettingsEntry(
                PREFIX,
                "ItemWheelMenuHotkey",
                "Settings_ItemWheelMenuHotkey_Name",
                UnityEngine.KeyCode.BackQuote, // Default: ~ key
                CATEGORY_UI,
                "Settings_ItemWheelMenuHotkey_Desc"
            )
        );

        public static readonly BoolSettingsEntry ThrowableWheelEnabled = Register(
            new BoolSettingsEntry(
                PREFIX,
                "ThrowableWheelEnabled",
                "Settings_ThrowableWheelEnabled_Name",
                true,
                CATEGORY_UI,
                "Settings_ThrowableWheelEnabled_Desc"
            )
        );

        public static readonly KeyCodeSettingsEntry ThrowableWheelHotkey = Register(
            new KeyCodeSettingsEntry(
                PREFIX,
                "ThrowableWheelHotkey",
                "Settings_ThrowableWheelHotkey_Name",
                UnityEngine.KeyCode.G, // Default: G key
                CATEGORY_UI,
                "Settings_ThrowableWheelHotkey_Desc"
            )
        );

        #endregion

        /// <summary>
        /// Register a settings entry and add it to the master list
        /// </summary>
        private static T Register<T>(T entry) where T : ISettingsEntry
        {
            _allSettings.Add(entry);
            return entry;
        }

        /// <summary>
        /// Get all settings entries
        /// </summary>
        public static IEnumerable<object> GetAllSettings()
        {
            return _allSettings;
        }

        /// <summary>
        /// Get all settings of a specific type
        /// </summary>
        public static IEnumerable<T> GetSettings<T>()
        {
            return _allSettings.OfType<T>();
        }

        /// <summary>
        /// Get all settings in a category
        /// </summary>
        public static IEnumerable<object> GetSettingsByCategory(string category)
        {
            // Use pattern matching to simplify category extraction
            return _allSettings.Where(s => GetSettingCategory(s) == category);
        }

        /// <summary>
        /// Extract category from a settings entry using pattern matching
        /// Note: Derived classes must come before base classes
        /// </summary>
        private static string? GetSettingCategory(object setting) => setting switch
        {
            // Derived classes first
            RangedFloatSettingsEntry rangedFloatEntry => rangedFloatEntry.Category,
            RangedIntSettingsEntry rangedIntEntry => rangedIntEntry.Category,
            IndexedOptionsSettingsEntry indexedOptionsEntry => indexedOptionsEntry.Category,
            OptionsSettingsEntry optionsEntry => optionsEntry.Category,
            // Base classes after
            BoolSettingsEntry boolEntry => boolEntry.Category,
            IntSettingsEntry intEntry => intEntry.Category,
            FloatSettingsEntry floatEntry => floatEntry.Category,
            StringSettingsEntry strEntry => strEntry.Category,
            _ => null
        };

        /// <summary>
        /// Get all unique categories
        /// </summary>
        public static IEnumerable<string> GetCategories()
        {
            // Use LINQ to extract unique categories in one line
            return _allSettings
                .Select(GetSettingCategory)
                .Where(category => !string.IsNullOrEmpty(category))
                .Distinct()
                .Cast<string>();  // Safe cast since we filtered nulls
        }

        /// <summary>
        /// Reset all settings to default values
        /// </summary>
        public static void ResetToDefaults()
        {
            // Use helper method to reset settings based on type
            ModLogger.Log("ModSettings", $"Starting reset of {_allSettings.Count} settings to defaults");
            
            int resetCount = 0;
            int failureCount = 0;

            foreach (var setting in _allSettings)
            {
                try
                {
                    var settingKey = setting.Key;
                    ModLogger.Log("ModSettings", $"[{resetCount + 1}/{_allSettings.Count}] Resetting setting: {settingKey}");
                    
                    setting.Reset();
                    resetCount++;
                    
                    ModLogger.Log("ModSettings", $"✓ Successfully reset: {settingKey}");
                }
                catch (Exception ex)
                {
                    failureCount++;
                    ModLogger.LogError($"✗ Failed to reset setting {setting.Key}: {ex.Message}");
                }
            }

            ModLogger.Log("ModSettings", $"Reset complete: {resetCount} successful, {failureCount} failed out of {_allSettings.Count} total settings");
        }

        /// <summary>
        /// Initialize the settings system
        /// </summary>
        public static void Initialize()
        {
            try
            {
                // Trigger loading of all settings by accessing their values
                // This ensures all settings are initialized before they're used
                foreach (var setting in _allSettings)
                {
                    LogSettingInitialization(setting);
                }

                ModLogger.Log("ModSettings", $"Initialized {_allSettings.Count} settings");
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Failed to initialize ModSettings: {ex}");
            }
        }

        /// <summary>
        /// Log initialization of a setting using pattern matching
        /// Note: Derived classes must come before base classes
        /// </summary>
        private static void LogSettingInitialization(object setting)
        {
            var (key, value) = setting switch
            {
                // Derived classes first
                RangedFloatSettingsEntry e => (e.Key, e.Value.ToString()),
                RangedIntSettingsEntry e => (e.Key, e.Value.ToString()),
                IndexedOptionsSettingsEntry e => (e.Key, e.Value.ToString()),
                OptionsSettingsEntry e => (e.Key, e.Value),
                // Base classes after
                BoolSettingsEntry e => (e.Key, e.Value.ToString()),
                IntSettingsEntry e => (e.Key, e.Value.ToString()),
                FloatSettingsEntry e => (e.Key, e.Value.ToString()),
                StringSettingsEntry e => (e.Key, e.Value),
                _ => ("Unknown", "Unknown")
            };

            ModLogger.Log("ModSettings", $"Initialized {key}: {value}");
        }

        /// <summary>
        /// Reload all settings from storage
        /// </summary>
        public static void ReloadAll()
        {
            try
            {
                ModLogger.Log("ModSettings", "Reloading all settings from storage...");

                foreach (var setting in _allSettings)
                {
                    setting.Reload();
                    ModLogger.Log("ModSettings", $"Reloaded {setting.Key}");
                }

                ModLogger.Log("ModSettings", "All settings reloaded from storage");
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Failed to reload settings: {ex}");
            }
        }
    }
}
