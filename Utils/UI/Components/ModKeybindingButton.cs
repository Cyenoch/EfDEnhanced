using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EfDEnhanced.Utils.Settings;
using EfDEnhanced.Utils.UI.Constants;

namespace EfDEnhanced.Utils.UI.Components
{
    /// <summary>
    /// UI component for rebinding a hotkey
    /// Similar to game's UIKeybindingEntry but for our mod settings
    /// </summary>
    public class ModKeybindingButton : MonoBehaviour
    {
        private KeyCodeSettingsEntry? _settingsEntry;
        private Button? _button;
        private TextMeshProUGUI? _labelText;
        private TextMeshProUGUI? _keyText;
        private bool _isListening = false;

        /// <summary>
        /// Keys that are not allowed to be bound (reserved for game)
        /// </summary>
        private static readonly KeyCode[] ExcludedKeys = new[]
        {
            KeyCode.Mouse0,
            KeyCode.Mouse1,
            KeyCode.Mouse2,
            KeyCode.Escape, // Reserved for canceling rebind
            KeyCode.None
        };

        /// <summary>
        /// Create a keybinding button
        /// </summary>
        public static ModKeybindingButton Create(Transform parent, KeyCodeSettingsEntry settingsEntry)
        {
            GameObject buttonObj = new GameObject("KeybindingButton");
            buttonObj.transform.SetParent(parent, false);

            // Add RectTransform
            RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(0, UIConstants.SETTINGS_FIELD_HEIGHT);

            // Add LayoutElement for proper sizing
            LayoutElement layoutElement = buttonObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = UIConstants.SETTINGS_FIELD_HEIGHT;
            layoutElement.flexibleWidth = 1;

            // Add HorizontalLayoutGroup for label + button layout
            HorizontalLayoutGroup layoutGroup = buttonObj.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.spacing = 10f;
            layoutGroup.childAlignment = TextAnchor.MiddleLeft;

            // Create label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(buttonObj.transform, false);

            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.sizeDelta = new Vector2(300f, UIConstants.SETTINGS_FIELD_HEIGHT);

            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = LocalizationHelper.Get(settingsEntry.NameKey);
            labelText.fontSize = UIConstants.SETTINGS_LABEL_FONT_SIZE;
            labelText.color = UIConstants.SETTINGS_TEXT_COLOR;
            labelText.alignment = TextAlignmentOptions.MidlineLeft;
            labelText.enableWordWrapping = false;
            UIStyles.ApplyStandardTextShadow(labelObj, false);

            LayoutElement labelLayout = labelObj.AddComponent<LayoutElement>();
            labelLayout.preferredWidth = 300f;
            labelLayout.flexibleWidth = 1;

            // Create button
            GameObject btnObj = new GameObject("Button");
            btnObj.transform.SetParent(buttonObj.transform, false);

            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(120f, UIConstants.SETTINGS_FIELD_HEIGHT - 10f);

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = new Color(0.2f, 0.2f, 0.25f, 0.9f);

            Button button = btnObj.AddComponent<Button>();
            button.targetGraphic = btnImage;

            // Button colors
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.2f, 0.2f, 0.25f, 0.9f);
            colors.highlightedColor = new Color(0.3f, 0.3f, 0.35f, 1f);
            colors.pressedColor = new Color(0.4f, 0.4f, 0.5f, 1f);
            colors.selectedColor = new Color(0.3f, 0.3f, 0.35f, 1f);
            colors.disabledColor = new Color(0.15f, 0.15f, 0.2f, 0.5f);
            button.colors = colors;

            LayoutElement btnLayout = btnObj.AddComponent<LayoutElement>();
            btnLayout.preferredWidth = 120f;
            btnLayout.preferredHeight = UIConstants.SETTINGS_FIELD_HEIGHT - 10f;

            // Create button text
            GameObject keyTextObj = new GameObject("KeyText");
            keyTextObj.transform.SetParent(btnObj.transform, false);

            RectTransform keyTextRect = keyTextObj.AddComponent<RectTransform>();
            keyTextRect.anchorMin = Vector2.zero;
            keyTextRect.anchorMax = Vector2.one;
            keyTextRect.sizeDelta = Vector2.zero;
            keyTextRect.anchoredPosition = Vector2.zero;

            TextMeshProUGUI keyText = keyTextObj.AddComponent<TextMeshProUGUI>();
            keyText.text = KeyCodeSettingsEntry.GetKeyDisplayName(settingsEntry.Value);
            keyText.fontSize = UIConstants.SETTINGS_LABEL_FONT_SIZE;
            keyText.color = Color.white;
            keyText.alignment = TextAlignmentOptions.Center;
            keyText.enableWordWrapping = false;

            // Add component and setup
            ModKeybindingButton component = buttonObj.AddComponent<ModKeybindingButton>();
            component._settingsEntry = settingsEntry;
            component._button = button;
            component._labelText = labelText;
            component._keyText = keyText;

            // Wire up button click
            button.onClick.AddListener(() => component.StartListening());

            // Subscribe to settings changes
            settingsEntry.ValueChanged += (sender, args) =>
            {
                if (component._keyText != null)
                {
                    component._keyText.text = KeyCodeSettingsEntry.GetKeyDisplayName(args.NewValue);
                }
            };

            return component;
        }

        /// <summary>
        /// Start listening for key input
        /// </summary>
        private void StartListening()
        {
            if (_isListening || _settingsEntry == null)
            {
                return;
            }

            _isListening = true;
            
            if (_keyText != null)
            {
                _keyText.text = LocalizationHelper.Get("Settings_PressAnyKey");
                _keyText.color = new Color(0.4f, 0.7f, 1f); // Light blue accent
            }

            if (_button != null)
            {
                _button.interactable = false;
            }

            ModLogger.Log("ModKeybindingButton", $"Started listening for key input for {_settingsEntry.Key}");

            StartCoroutine(ListenForKeyCoroutine());
        }

        /// <summary>
        /// Coroutine to listen for key input
        /// </summary>
        private IEnumerator ListenForKeyCoroutine()
        {
            // Wait for a frame to avoid catching the click that started listening
            yield return null;

            bool keyReceived = false;

            while (!keyReceived)
            {
                // Check for Escape to cancel
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    ModLogger.Log("ModKeybindingButton", "Keybinding cancelled by user");
                    StopListening(false);
                    yield break;
                }

                // Check for any key press
                foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKeyDown(keyCode))
                    {
                        // Skip excluded keys
                        if (IsKeyExcluded(keyCode))
                        {
                            continue;
                        }

                        // Validate and set key
                        if (_settingsEntry != null)
                        {
                            // SettingsEntry.Value setter will handle validation
                            try
                            {
                                _settingsEntry.Value = keyCode;
                                ModLogger.Log("ModKeybindingButton", $"Key bound: {keyCode}");
                                keyReceived = true;
                                break;
                            }
                            catch
                            {
                                // Validation failed, continue listening
                                continue;
                            }
                        }
                    }
                }

                yield return null;
            }

            StopListening(true);
        }

        /// <summary>
        /// Stop listening for key input
        /// </summary>
        private void StopListening(bool success)
        {
            _isListening = false;

            if (_button != null)
            {
                _button.interactable = true;
            }

            if (_keyText != null && _settingsEntry != null)
            {
                _keyText.text = KeyCodeSettingsEntry.GetKeyDisplayName(_settingsEntry.Value);
                _keyText.color = Color.white;
            }

            if (success)
            {
                ModLogger.Log("ModKeybindingButton", "Keybinding completed successfully");
            }
        }

        /// <summary>
        /// Check if a key is in the exclusion list
        /// </summary>
        private bool IsKeyExcluded(KeyCode keyCode)
        {
            foreach (var excluded in ExcludedKeys)
            {
                if (keyCode == excluded)
                {
                    return true;
                }
            }
            return false;
        }

        private void OnDestroy()
        {
            // Clean up button listener
            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
            }
        }
    }
}

