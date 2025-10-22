namespace EfDEnhanced.Utils.Settings
{
    /// <summary>
    /// Boolean settings entry
    /// </summary>
    public sealed class BoolSettingsEntry(
        string prefix,
        string key,
        string nameKey,
        bool defaultValue,
        string categoryKey,
        string? descriptionKey = null,
        int version = 1) : SettingsEntry<bool>(prefix, key, nameKey, defaultValue, categoryKey, descriptionKey, version)
    {

        /// <summary>
        /// Toggle the current value
        /// </summary>
        public void Toggle()
        {
            Value = !Value;
        }
    }
}
