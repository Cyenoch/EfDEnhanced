using System;

namespace EfDEnhanced.Utils.Settings
{
    /// <summary>
    /// Float settings entry
    /// </summary>
    public class FloatSettingsEntry : SettingsEntry<float>
    {
        public FloatSettingsEntry(
            string prefix,
            string key,
            string name,
            float defaultValue,
            string category = "General",
            string? description = null,
            int version = 1)
            : base(prefix, key, name, defaultValue, category, description, version)
        {
        }
    }

    /// <summary>
    /// Float settings entry with range constraints
    /// </summary>
    public class RangedFloatSettingsEntry : FloatSettingsEntry
    {
        public float MinValue { get; }
        public float MaxValue { get; }

        public RangedFloatSettingsEntry(
            string prefix,
            string key,
            string name,
            float defaultValue,
            float minValue,
            float maxValue,
            string category = "General",
            string? description = null,
            int version = 1)
            : base(prefix, key, name, defaultValue, category, description, version)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }

        protected override bool Validate(float value)
        {
            return value >= MinValue && value <= MaxValue;
        }

        protected override float CoerceValue(float value)
        {
            if (value < MinValue) return MinValue;
            if (value > MaxValue) return MaxValue;
            return value;
        }

        /// <summary>
        /// Clamp a value to the valid range
        /// </summary>
        public float Clamp(float value)
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
            if (Math.Abs(MaxValue - MinValue) < float.Epsilon) return 0f;
            return (Value - MinValue) / (MaxValue - MinValue);
        }

        /// <summary>
        /// Set the value from a normalized float (0-1)
        /// </summary>
        public void SetNormalized(float normalized)
        {
            Value = MinValue + normalized * (MaxValue - MinValue);
        }
    }
}
