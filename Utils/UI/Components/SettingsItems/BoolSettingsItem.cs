using System;
using EfDEnhanced.Utils.Settings;
using EfDEnhanced.Utils.UI.Constants;
using UnityEngine;
using UnityEngine.UI;

namespace EfDEnhanced.Utils.UI.Components.SettingsItems
{
    /// <summary>
    /// Settings item for boolean/toggle settings
    /// </summary>
    public class BoolSettingsItem : BaseSettingsItem
    {
        private BoolSettingsEntry _boolEntry = null!;
        private Toggle _toggle = null!;

        public override void Initialize(ISettingsEntry entry, int leftPadding = 0)
        {
            _boolEntry = entry as BoolSettingsEntry ?? throw new ArgumentException("Entry must be BoolSettingsEntry");
            base.Initialize(entry, leftPadding);
        }

        protected override void BuildContent()
        {
            // Create label
            CreateLabel();

            // Create toggle container
            var toggleContainer = new GameObject("ToggleContainer");
            toggleContainer.transform.SetParent(ContentContainer.transform, false);

            var toggleContainerRect = toggleContainer.AddComponent<RectTransform>();
            toggleContainerRect.anchorMin = new Vector2(0, 0.5f);
            toggleContainerRect.anchorMax = new Vector2(0, 0.5f);
            toggleContainerRect.pivot = new Vector2(0, 0.5f);

            // Add horizontal layout for toggle
            var toggleLayout = toggleContainer.AddComponent<HorizontalLayoutGroup>();
            toggleLayout.childControlWidth = false;
            toggleLayout.childControlHeight = false;
            toggleLayout.childForceExpandWidth = false;
            toggleLayout.childForceExpandHeight = false;
            toggleLayout.childAlignment = TextAnchor.MiddleLeft;

            // Set layout element
            var toggleContainerLayout = toggleContainer.AddComponent<LayoutElement>();
            toggleContainerLayout.preferredWidth = UIConstants.SETTINGS_CONTROL_MIN_WIDTH;
            toggleContainerLayout.flexibleWidth = 1;

            // Create toggle
            var modToggle = ModToggle.Create(toggleContainer.transform, "Toggle");
            modToggle.Value = _boolEntry.Value;
            modToggle.OnValueChanged(OnToggleChanged);
            _toggle = modToggle.Toggle;

            // Subscribe to settings changes
            _boolEntry.ValueChanged += OnSettingsValueChanged;
        }

        private void OnToggleChanged(bool value)
        {
            _boolEntry.Value = value;
        }

        private void OnSettingsValueChanged(object sender, SettingsValueChangedEventArgs<bool> e)
        {
            if (_toggle != null && _toggle.isOn != e.NewValue)
            {
                _toggle.isOn = e.NewValue;
            }
        }

        protected override void OnDestroy()
        {
            if (_boolEntry != null)
            {
                _boolEntry.ValueChanged -= OnSettingsValueChanged;
            }
            base.OnDestroy();
        }
    }
}

