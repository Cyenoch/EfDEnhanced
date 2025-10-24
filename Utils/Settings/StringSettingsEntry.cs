using System;
using System.Linq;

namespace EfDEnhanced.Utils.Settings
{
    /// <summary>
    /// String settings entry
    /// </summary>
    public class StringSettingsEntry(
        string prefix,
        string key,
        string name,
        string defaultValue,
        string category = "General",
        string? description = null,
        int version = 1) : SettingsEntry<string>(prefix, key, name, defaultValue, category, description, version)
    {
    }

    /// <summary>
    /// String settings entry with predefined options (dropdown/enum-like)
    /// </summary>
    public class OptionsSettingsEntry : StringSettingsEntry
    {
        public string[] Options { get; }

        public OptionsSettingsEntry(
            string prefix,
            string key,
            string name,
            string defaultValue,
            string[] options,
            string category = "General",
            string? description = null,
            int version = 1)
            : base(prefix, key, name, defaultValue, category, description, version)
        {
            if (options == null || options.Length == 0)
                throw new ArgumentException("Options cannot be null or empty", nameof(options));

            if (!options.Contains(defaultValue))
                throw new ArgumentException("Default value must be one of the options", nameof(defaultValue));

            Options = options;
        }

        protected override bool Validate(string value)
        {
            return Options.Contains(value);
        }

        protected override string CoerceValue(string value)
        {
            return Options.Contains(value) ? value : DefaultValue;
        }

        /// <summary>
        /// Get the index of the current value in the options array
        /// </summary>
        public int GetSelectedIndex()
        {
            return Array.IndexOf(Options, Value);
        }

        /// <summary>
        /// Set the value by index
        /// </summary>
        public void SetSelectedIndex(int index)
        {
            if (index >= 0 && index < Options.Length)
            {
                Value = Options[index];
            }
        }

        /// <summary>
        /// Cycle to the next option
        /// </summary>
        public void CycleNext()
        {
            int currentIndex = GetSelectedIndex();
            int nextIndex = (currentIndex + 1) % Options.Length;
            SetSelectedIndex(nextIndex);
        }

        /// <summary>
        /// Cycle to the previous option
        /// </summary>
        public void CyclePrevious()
        {
            int currentIndex = GetSelectedIndex();
            int prevIndex = (currentIndex - 1 + Options.Length) % Options.Length;
            SetSelectedIndex(prevIndex);
        }
    }
}
