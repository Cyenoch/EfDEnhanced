using System;
using System.Collections;
using EfDEnhanced.Utils.Settings;
using EfDEnhanced.Utils.UI.Constants;
using UnityEngine;
using UnityEngine.UI;

namespace EfDEnhanced.Utils.UI.Components.SettingsItems
{
    /// <summary>
    /// Settings item for keybinding settings
    /// </summary>
    public class KeyCodeSettingsItem : BaseSettingsItem
    {
        private KeyCodeSettingsEntry _keyCodeEntry = null!;
        private Button _button = null!;
        private Text _buttonText = null!;
        private bool _isListening = false;
        private KeyCode _originalValue;

        public override void Initialize(ISettingsEntry entry, int leftPadding = 0)
        {
            _keyCodeEntry = entry as KeyCodeSettingsEntry ?? throw new ArgumentException("Entry must be KeyCodeSettingsEntry");
            base.Initialize(entry, leftPadding);
        }

        protected override void BuildContent()
        {
            // Create label
            CreateLabel();

            // Create button container
            var buttonContainer = new GameObject("ButtonContainer");
            buttonContainer.transform.SetParent(ContentContainer.transform, false);

            var buttonContainerRect = buttonContainer.AddComponent<RectTransform>();
            buttonContainerRect.anchorMin = new Vector2(0, 0.5f);
            buttonContainerRect.anchorMax = new Vector2(0, 0.5f);
            buttonContainerRect.pivot = new Vector2(0, 0.5f);

            // Set layout element
            var buttonContainerLayout = buttonContainer.AddComponent<LayoutElement>();
            buttonContainerLayout.preferredWidth = UIConstants.SETTINGS_CONTROL_MIN_WIDTH;
            buttonContainerLayout.flexibleWidth = 0;
            buttonContainerLayout.minHeight = 30;

            // Create button
            var buttonObj = new GameObject("KeybindButton");
            buttonObj.transform.SetParent(buttonContainer.transform, false);

            var buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.anchorMin = Vector2.zero;
            buttonRect.anchorMax = Vector2.one;
            buttonRect.sizeDelta = Vector2.zero;

            _button = buttonObj.AddComponent<Button>();
            _button.onClick.AddListener(OnButtonClicked);

            // Button background
            var buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.4f, 0.6f, 1f);
            _button.targetGraphic = buttonImage;

            // Button text
            var buttonTextObj = new GameObject("Text");
            buttonTextObj.transform.SetParent(buttonObj.transform, false);

            var buttonTextRect = buttonTextObj.AddComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.sizeDelta = Vector2.zero;

            _buttonText = buttonTextObj.AddComponent<Text>();
            _buttonText.text = KeyCodeSettingsEntry.GetKeyDisplayName(_keyCodeEntry.Value);
            _buttonText.font = UIConstants.DefaultFont;
            _buttonText.fontSize = UIConstants.SETTINGS_FONT_SIZE;
            _buttonText.color = Color.white;
            _buttonText.alignment = TextAnchor.MiddleCenter;

            // Subscribe to settings changes
            _keyCodeEntry.ValueChanged += OnSettingsValueChanged;
        }

        private void OnButtonClicked()
        {
            if (!_isListening)
            {
                StartListening();
            }
        }

        private void StartListening()
        {
            _isListening = true;
            _originalValue = _keyCodeEntry.Value;
            _buttonText.text = LocalizationHelper.Get("Settings_PressAnyKey");
            StartCoroutine(ListenForKeyCoroutine());
        }

        private IEnumerator ListenForKeyCoroutine()
        {
            while (_isListening)
            {
                // Check for ESC to cancel
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    CancelListening();
                    yield break;
                }

                // Check for any key press
                foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKeyDown(keyCode) && keyCode != KeyCode.None)
                    {
                        // Try to set the key (validation happens in the entry)
                        try
                        {
                            _keyCodeEntry.Value = keyCode;
                            _isListening = false;
                            _buttonText.text = KeyCodeSettingsEntry.GetKeyDisplayName(keyCode);
                            yield break;
                        }
                        catch
                        {
                            // Invalid key, keep listening
                            ModLogger.LogWarning($"Invalid key: {keyCode}");
                        }
                    }
                }

                yield return null;
            }
        }

        private void CancelListening()
        {
            _isListening = false;
            _buttonText.text = KeyCodeSettingsEntry.GetKeyDisplayName(_originalValue);
        }

        private void OnSettingsValueChanged(object sender, SettingsValueChangedEventArgs<KeyCode> e)
        {
            if (_buttonText != null && !_isListening)
            {
                _buttonText.text = KeyCodeSettingsEntry.GetKeyDisplayName(e.NewValue);
            }
        }
        
        /// <summary>
        /// Handle language changes - update the "Press Any Key" text if currently listening
        /// </summary>
        protected override void OnLanguageChanged(SystemLanguage newLanguage)
        {
            try
            {
                base.OnLanguageChanged(newLanguage);
                
                // If currently listening, update the prompt text
                if (_isListening && _buttonText != null)
                {
                    _buttonText.text = LocalizationHelper.Get("Settings_PressAnyKey");
                }
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"KeyCodeSettingsItem.OnLanguageChanged failed: {ex}");
            }
        }

        protected override void OnDestroy()
        {
            if (_keyCodeEntry != null)
            {
                _keyCodeEntry.ValueChanged -= OnSettingsValueChanged;
            }
            if (_button != null)
            {
                _button.onClick.RemoveListener(OnButtonClicked);
            }
            base.OnDestroy();
        }
    }
}

