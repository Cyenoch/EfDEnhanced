namespace EfDEnhanced.Utils.Settings
{
    /// <summary>
    /// Boolean settings entry
    /// </summary>
    public class BoolSettingsEntry : SettingsEntry<bool>
    {
        public BoolSettingsEntry(
            string prefix,
            string key,
            string nameKey,
            bool defaultValue,
            string categoryKey,
            string? descriptionKey = null,
            int version = 1)
            : base(prefix, key, nameKey, defaultValue, categoryKey, descriptionKey, version)
        {
        }

        /// <summary>
        /// Toggle the current value
        /// </summary>
        public void Toggle()
        {
            Value = !Value;
        }
    }
}
