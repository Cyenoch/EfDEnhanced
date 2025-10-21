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

        // All settings entries
        private static readonly List<object> _allSettings = [];

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
                1.0f,
                0.5f,
                2.0f,
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

        #endregion

        /// <summary>
        /// Register a settings entry and add it to the master list
        /// </summary>
        private static T Register<T>(T entry)
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
            return _allSettings.Where(s =>
            {
                if (s is BoolSettingsEntry boolEntry) return boolEntry.Category == category;
                if (s is IntSettingsEntry intEntry) return intEntry.Category == category;
                if (s is FloatSettingsEntry floatEntry) return floatEntry.Category == category;
                if (s is StringSettingsEntry strEntry) return strEntry.Category == category;
                if (s is RangedFloatSettingsEntry rangedFloatEntry) return rangedFloatEntry.Category == category;
                if (s is RangedIntSettingsEntry rangedIntEntry) return rangedIntEntry.Category == category;
                if (s is OptionsSettingsEntry optionsEntry) return optionsEntry.Category == category;
                return false;
            });
        }

        /// <summary>
        /// Get all unique categories
        /// </summary>
        public static IEnumerable<string> GetCategories()
        {
            var categories = new HashSet<string>();
            foreach (var setting in _allSettings)
            {
                string? category = null;
                if (setting is BoolSettingsEntry boolEntry) category = boolEntry.Category;
                else if (setting is IntSettingsEntry intEntry) category = intEntry.Category;
                else if (setting is FloatSettingsEntry floatEntry) category = floatEntry.Category;
                else if (setting is StringSettingsEntry strEntry) category = strEntry.Category;
                else if (setting is RangedFloatSettingsEntry rangedFloatEntry) category = rangedFloatEntry.Category;
                else if (setting is RangedIntSettingsEntry rangedIntEntry) category = rangedIntEntry.Category;
                else if (setting is OptionsSettingsEntry optionsEntry) category = optionsEntry.Category;

                if (!string.IsNullOrEmpty(category))
                    categories.Add(category);
            }
            return categories;
        }

        /// <summary>
        /// Reset all settings to default values
        /// </summary>
        public static void ResetToDefaults()
        {
            foreach (var setting in _allSettings)
            {
                if (setting is BoolSettingsEntry boolEntry) boolEntry.Reset();
                else if (setting is IntSettingsEntry intEntry) intEntry.Reset();
                else if (setting is FloatSettingsEntry floatEntry) floatEntry.Reset();
                else if (setting is StringSettingsEntry strEntry) strEntry.Reset();
                else if (setting is RangedFloatSettingsEntry rangedFloatEntry) rangedFloatEntry.Reset();
                else if (setting is RangedIntSettingsEntry rangedIntEntry) rangedIntEntry.Reset();
                else if (setting is OptionsSettingsEntry optionsEntry) optionsEntry.Reset();
            }

            ModLogger.Log("ModSettings", "All settings reset to defaults");
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
                    if (setting is BoolSettingsEntry boolEntry)
                        ModLogger.Log("ModSettings", $"Initialized {boolEntry.Key}: {boolEntry.Value}");
                    else if (setting is IntSettingsEntry intEntry)
                        ModLogger.Log("ModSettings", $"Initialized {intEntry.Key}: {intEntry.Value}");
                    else if (setting is FloatSettingsEntry floatEntry)
                        ModLogger.Log("ModSettings", $"Initialized {floatEntry.Key}: {floatEntry.Value}");
                    else if (setting is StringSettingsEntry strEntry)
                        ModLogger.Log("ModSettings", $"Initialized {strEntry.Key}: {strEntry.Value}");
                    else if (setting is RangedFloatSettingsEntry rangedFloatEntry)
                        ModLogger.Log("ModSettings", $"Initialized {rangedFloatEntry.Key}: {rangedFloatEntry.Value}");
                    else if (setting is RangedIntSettingsEntry rangedIntEntry)
                        ModLogger.Log("ModSettings", $"Initialized {rangedIntEntry.Key}: {rangedIntEntry.Value}");
                    else if (setting is OptionsSettingsEntry optionsEntry)
                        ModLogger.Log("ModSettings", $"Initialized {optionsEntry.Key}: {optionsEntry.Value}");
                }

                ModLogger.Log("ModSettings", $"Initialized {_allSettings.Count} settings");
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Failed to initialize ModSettings: {ex}");
            }
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
                    if (setting is BoolSettingsEntry boolEntry)
                    {
                        bool oldValue = boolEntry.Value;
                        boolEntry.Reload();
                        ModLogger.Log("ModSettings", $"Reloaded {boolEntry.Key}: {oldValue} -> {boolEntry.Value}");
                    }
                    else if (setting is IntSettingsEntry intEntry) intEntry.Reload();
                    else if (setting is FloatSettingsEntry floatEntry) floatEntry.Reload();
                    else if (setting is StringSettingsEntry strEntry) strEntry.Reload();
                    else if (setting is RangedFloatSettingsEntry rangedFloatEntry) rangedFloatEntry.Reload();
                    else if (setting is RangedIntSettingsEntry rangedIntEntry) rangedIntEntry.Reload();
                    else if (setting is OptionsSettingsEntry optionsEntry) optionsEntry.Reload();
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
