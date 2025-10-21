namespace EfDEnhanced.Utils.Settings
{
    /// <summary>
    /// Integer settings entry
    /// </summary>
    public class IntSettingsEntry : SettingsEntry<int>
    {
        public IntSettingsEntry(
            string prefix,
            string key,
            string name,
            int defaultValue,
            string category = "General",
            string? description = null,
            int version = 1)
            : base(prefix, key, name, defaultValue, category, description, version)
        {
        }
    }

    /// <summary>
    /// Integer settings entry with range constraints
    /// </summary>
    public class RangedIntSettingsEntry : IntSettingsEntry
    {
        public int MinValue { get; }
        public int MaxValue { get; }

        public RangedIntSettingsEntry(
            string prefix,
            string key,
            string name,
            int defaultValue,
            int minValue,
            int maxValue,
            string category = "General",
            string? description = null,
            int version = 1)
            : base(prefix, key, name, defaultValue, category, description, version)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }

        protected override bool Validate(int value)
        {
            return value >= MinValue && value <= MaxValue;
        }

        /// <summary>
        /// Clamp a value to the valid range
        /// </summary>
        public int Clamp(int value)
        {
            if (value < MinValue) return MinValue;
            if (value > MaxValue) return MaxValue;
            return value;
        }

        /// <summary>
        /// Get the value as a normalized float (0-1)
        /// </summary>
        public float GetNormalized()
        {
            if (MaxValue == MinValue) return 0f;
            return (float)(Value - MinValue) / (MaxValue - MinValue);
        }

        /// <summary>
        /// Set the value from a normalized float (0-1)
        /// </summary>
        public void SetNormalized(float normalized)
        {
            Value = MinValue + (int)(normalized * (MaxValue - MinValue));
        }
    }
}
