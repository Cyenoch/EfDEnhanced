using System;
using Duckov.Options;

namespace EfDEnhanced.Utils.Settings
{
    /// <summary>
    /// Non-generic interface for settings entries
    /// Used for storing heterogeneous settings in a single collection
    /// </summary>
    public interface ISettingsEntry
    {
        string Key { get; }
        string NameKey { get; }
        string? DescriptionKey { get; }
        string CategoryKey { get; }
        string Name { get; }
        string? Description { get; }
        string Category { get; }
        int Version { get; }
        bool WasModifiedByUser { get; }
        void Reset();
        void Reload();
    }

    /// <summary>
    /// Generic interface for settings entries with type information
    /// Provides type-safe access to the value
    /// Note: Cannot use 'out T' because Value property is both readable and writable
    /// </summary>
    public interface ISettingsEntry<T> : ISettingsEntry
    {
        /// <summary>
        /// Event fired when the setting value changes
        /// </summary>
        event EventHandler<SettingsValueChangedEventArgs<T>>? ValueChanged;

        /// <summary>
        /// Current value of this setting (type-safe)
        /// </summary>
        T Value { get; set; }

        /// <summary>
        /// Default value for this setting
        /// </summary>
        T DefaultValue { get; }
    }

    /// <summary>
    /// Event arguments for settings value changes
    /// </summary>
    public class SettingsValueChangedEventArgs<T>(T oldValue, T newValue) : EventArgs
    {
        public T OldValue { get; } = oldValue;
        public T NewValue { get; } = newValue;
    }

    /// <summary>
    /// Base class for all settings entries
    /// Provides persistence, events, default value management, and version control
    /// </summary>
    public abstract class SettingsEntry<T>(string prefix, string key, string nameKey, T defaultValue, string categoryKey, string? descriptionKey = null, int version = 1) : ISettingsEntry<T>
    {
        /// <summary>
        /// Event fired when the setting value changes
        /// </summary>
        public event EventHandler<SettingsValueChangedEventArgs<T>>? ValueChanged;

        /// <summary>
        /// Unique key for this setting (used for persistence)
        /// </summary>
        public string Key { get; } = $"{prefix}_{key}";

        /// <summary>
        /// Localization key for the setting name
        /// </summary>
        public string NameKey { get; } = nameKey;

        /// <summary>
        /// Localization key for the setting description
        /// </summary>
        public string? DescriptionKey { get; } = descriptionKey;

        /// <summary>
        /// Localization key for the category
        /// </summary>
        public string CategoryKey { get; } = categoryKey;

        /// <summary>
        /// Get localized name for this setting
        /// </summary>
        public string Name => LocalizationHelper.Get(NameKey);

        /// <summary>
        /// Get localized description for this setting
        /// </summary>
        public string? Description => string.IsNullOrEmpty(DescriptionKey) ? null : LocalizationHelper.Get(DescriptionKey);

        /// <summary>
        /// Get localized category name
        /// </summary>
        public string Category => LocalizationHelper.Get(CategoryKey);

        /// <summary>
        /// Default value for this setting
        /// </summary>
        public T DefaultValue { get; } = defaultValue;

        /// <summary>
        /// Version of this setting's default value
        /// Increment this when changing default values to trigger migration
        /// </summary>
        public int Version { get; } = version;

        private T _cachedValue = defaultValue;
        private bool _isInitialized = false;
        private bool _wasModifiedByUser = false;

        /// <summary>
        /// Current value of this setting
        /// </summary>
        public T Value
        {
            get
            {
                if (!_isInitialized)
                {
                    _cachedValue = LoadValueWithMigration();
                    _isInitialized = true;
                }
                return _cachedValue;
            }
            set
            {
                T oldValue = _cachedValue;
                if (!Equals(oldValue, value) && Validate(value))
                {
                    _cachedValue = value;
                    _wasModifiedByUser = true;
                    SaveValue(value);
                    SaveVersion();
                    SaveModifiedFlag();
                    OnValueChanged(oldValue, value);
                }
            }
        }

        /// <summary>
        /// Check if this setting was modified by user
        /// </summary>
        public bool WasModifiedByUser => _wasModifiedByUser;

        /// <summary>
        /// Reset this setting to its default value (user-initiated reset)
        /// </summary>
        public void Reset()
        {
            T oldValue = _cachedValue;
            _cachedValue = DefaultValue;
            _wasModifiedByUser = false;
            SaveValue(DefaultValue);
            SaveVersion();
            SaveModifiedFlag();
            OnValueChanged(oldValue, DefaultValue);
        }

        /// <summary>
        /// Validate a value before setting it
        /// Override to implement custom validation logic
        /// </summary>
        protected virtual bool Validate(T value)
        {
            return true;
        }

        /// <summary>
        /// Allow derived classes to coerce persisted values into a valid range/shape.
        /// </summary>
        /// <param name="value">Loaded value from storage.</param>
        /// <returns>Coerced value (defaults to the input).</returns>
        protected virtual T CoerceValue(T value)
        {
            return value;
        }

        /// <summary>
        /// Load value with version migration support
        /// </summary>
        private T LoadValueWithMigration()
        {
            try
            {
                // Load saved version
                int savedVersion = OptionsManager.Load(GetVersionKey(), 0);
                bool wasModified = OptionsManager.Load(GetModifiedFlagKey(), false);

                _wasModifiedByUser = wasModified;

                // If version changed and user hasn't modified the setting, use new default
                if (savedVersion < Version && !wasModified)
                {
                    Utils.ModLogger.Log("SettingsEntry", $"{Key}: Upgrading from v{savedVersion} to v{Version}, applying new default: {DefaultValue}");
                    SaveValue(DefaultValue);
                    SaveVersion();
                    return DefaultValue;
                }

                // Load value with fallback to default on parse failure
                T loadedValue = LoadValue();

                // Allow derived classes to coerce the loaded value
                T coercedValue = CoerceValue(loadedValue);

                if (!Equals(coercedValue, loadedValue))
                {
                    Utils.ModLogger.Log("SettingsEntry", $"{Key}: Adjusted persisted value {loadedValue} -> {coercedValue}");
                    SaveValue(coercedValue);
                    SaveModifiedFlag();
                }

                if (!Validate(coercedValue))
                {
                    Utils.ModLogger.LogWarning("SettingsEntry", $"{Key}: Loaded invalid value {coercedValue}, resetting to default {DefaultValue}");
                    _wasModifiedByUser = false;
                    SaveValue(DefaultValue);
                    SaveVersion();
                    SaveModifiedFlag();
                    return DefaultValue;
                }

                return coercedValue;
            }
            catch (Exception ex)
            {
                Utils.ModLogger.LogError($"Failed to load setting {Key}, resetting to default: {ex.Message}");
                ResetToDefault();
                return DefaultValue;
            }
        }

        /// <summary>
        /// Load value from persistent storage
        /// </summary>
        protected virtual T LoadValue()
        {
            try
            {
                return OptionsManager.Load(Key, DefaultValue);
            }
            catch (Exception ex)
            {
                Utils.ModLogger.LogError($"Parse error for setting {Key}: {ex.Message}");
                throw; // Re-throw to trigger reset in LoadValueWithMigration
            }
        }

        /// <summary>
        /// Save value to persistent storage
        /// </summary>
        protected virtual void SaveValue(T value)
        {
            try
            {
                OptionsManager.Save(Key, value);
            }
            catch (Exception ex)
            {
                Utils.ModLogger.LogError($"Failed to save setting {Key}: {ex.Message}");
            }
        }

        /// <summary>
        /// Save current version
        /// </summary>
        private void SaveVersion()
        {
            try
            {
                OptionsManager.Save(GetVersionKey(), Version);
            }
            catch (Exception ex)
            {
                Utils.ModLogger.LogError($"Failed to save version for {Key}: {ex.Message}");
            }
        }

        /// <summary>
        /// Save modified flag
        /// </summary>
        private void SaveModifiedFlag()
        {
            try
            {
                OptionsManager.Save(GetModifiedFlagKey(), _wasModifiedByUser);
            }
            catch (Exception ex)
            {
                Utils.ModLogger.LogError($"Failed to save modified flag for {Key}: {ex.Message}");
            }
        }

        private string GetVersionKey() => $"{Key}_Version";
        private string GetModifiedFlagKey() => $"{Key}_Modified";

        /// <summary>
        /// Reset to default value (used internally for error recovery)
        /// </summary>
        private void ResetToDefault()
        {
            _cachedValue = DefaultValue;
            _wasModifiedByUser = false;
            SaveValue(DefaultValue);
            SaveVersion();
            SaveModifiedFlag();
        }

        /// <summary>
        /// Trigger ValueChanged event
        /// </summary>
        protected virtual void OnValueChanged(T oldValue, T newValue)
        {
            ValueChanged?.Invoke(this, new SettingsValueChangedEventArgs<T>(oldValue, newValue));
            Utils.ModLogger.Log("SettingsEntry", $"{Key}: Value changed from {oldValue} to {newValue}");
        }

        /// <summary>
        /// Force reload value from storage (useful for external changes)
        /// </summary>
        public void Reload()
        {
            T newValue = LoadValue();
            if (!Equals(_cachedValue, newValue))
            {
                T oldValue = _cachedValue;
                _cachedValue = newValue;
                OnValueChanged(oldValue, newValue);
            }
        }

        public override string ToString()
        {
            return $"{Name}: {Value}";
        }
    }
}
