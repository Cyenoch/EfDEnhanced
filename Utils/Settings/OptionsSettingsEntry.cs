using System;
using System.Linq;

namespace EfDEnhanced.Utils.Settings
{
    /// <summary>
    /// Settings entry for multiple choice options (dropdown)
    /// Stores selected option as an integer index
    /// </summary>
    public class IndexedOptionsSettingsEntry : SettingsEntry<int>
    {
        /// <summary>
        /// Localization keys for each option
        /// </summary>
        public string[] OptionKeys { get; }

        /// <summary>
        /// Get localized option names
        /// </summary>
        public string[] Options => OptionKeys.Select(key => LocalizationHelper.Get(key)).ToArray();

        /// <summary>
        /// Get the currently selected option text
        /// </summary>
        public string SelectedOption => LocalizationHelper.Get(OptionKeys[Value]);

        public IndexedOptionsSettingsEntry(
            string prefix,
            string key,
            string nameKey,
            int defaultValue,
            string[] optionKeys,
            string categoryKey,
            string? descriptionKey = null,
            int version = 1)
            : base(prefix, key, nameKey, defaultValue, categoryKey, descriptionKey, version)
        {
            if (optionKeys == null || optionKeys.Length == 0)
            {
                throw new ArgumentException("Options array cannot be null or empty", nameof(optionKeys));
            }

            OptionKeys = optionKeys;

            // Validate default value is within range
            if (defaultValue < 0 || defaultValue >= optionKeys.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(defaultValue),
                    $"Default value {defaultValue} is out of range for {optionKeys.Length} options");
            }
        }

        /// <summary>
        /// Validate that value is within the valid range of option indices
        /// </summary>
        protected override bool Validate(int value)
        {
            return value >= 0 && value < OptionKeys.Length;
        }

        /// <summary>
        /// Coerce loaded values to be within valid range
        /// </summary>
        protected override int CoerceValue(int value)
        {
            if (value < 0)
            {
                Utils.ModLogger.LogWarning("OptionsSettingsEntry", 
                    $"{Key}: Value {value} is below minimum, clamping to 0");
                return 0;
            }

            if (value >= OptionKeys.Length)
            {
                Utils.ModLogger.LogWarning("OptionsSettingsEntry",
                    $"{Key}: Value {value} exceeds maximum {OptionKeys.Length - 1}, clamping to max");
                return OptionKeys.Length - 1;
            }

            return value;
        }

        public override string ToString()
        {
            return $"{Name}: {SelectedOption}";
        }
    }
}

