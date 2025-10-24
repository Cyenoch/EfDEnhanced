using System;
using EfDEnhanced.Utils.Settings;
using EfDEnhanced.Utils.UI.Constants;
using UnityEngine;
using UnityEngine.UI;

namespace EfDEnhanced.Utils.UI.Components.SettingsItems
{
    /// <summary>
    /// Base class for all settings item UI components
    /// Each item contains the setting control and optional description
    /// </summary>
    public abstract class BaseSettingsItem : MonoBehaviour
    {
        protected ISettingsEntry SettingsEntry { get; private set; } = null!;
        protected GameObject ContentContainer { get; private set; } = null!;
        protected GameObject DescriptionObject { get; private set; } = null!;
        protected Text? DescriptionText { get; private set; }
        protected Text? LabelText { get; private set; }

        private int _leftPadding;
        private Action<SystemLanguage>? _languageChangeHandler;

        /// <summary>
        /// Initialize the settings item with a settings entry
        /// </summary>
        public virtual void Initialize(ISettingsEntry entry, int leftPadding = 0)
        {
            SettingsEntry = entry ?? throw new ArgumentNullException(nameof(entry));
            _leftPadding = leftPadding;

            SetupItemLayout();
            BuildContent();
            BuildDescription();

            // Create and store the handler so we can properly unsubscribe later
            _languageChangeHandler = OnLanguageChanged;
            LocalizationHelper.OnLanguageChanged += _languageChangeHandler;
        }

        /// <summary>
        /// Handle language changes by refreshing all localized text
        /// </summary>
        protected virtual void OnLanguageChanged(SystemLanguage newLanguage)
        {
            try
            {
                RefreshLocalizedText();
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"BaseSettingsItem.OnLanguageChanged failed: {ex}");
            }
        }

        /// <summary>
        /// Refresh all localized text in this settings item
        /// Override in derived classes to refresh control-specific text
        /// </summary>
        protected virtual void RefreshLocalizedText()
        {
            // Refresh label text
            if (LabelText != null)
            {
                LabelText.text = SettingsEntry.Name;
            }

            // Refresh description text
            if (DescriptionText != null)
            {
                DescriptionText.text = SettingsEntry.Description;
            }
        }

        /// <summary>
        /// Setup the base item layout structure
        /// </summary>
        private void SetupItemLayout()
        {
            // Configure the item's RectTransform
            var rectTransform = gameObject.GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(0.5f, 1);

            // Add VerticalLayoutGroup for content + description
            var layoutGroup = gameObject.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.spacing = 2; // Minimal spacing between content and description
            layoutGroup.padding = new RectOffset(_leftPadding, 0, 0, 2); // Minimal bottom padding

            // Add ContentSizeFitter
            var sizeFitter = gameObject.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            // Create content container (where the actual control goes)
            ContentContainer = new GameObject("Content");
            ContentContainer.transform.SetParent(transform, false);

            var contentRect = ContentContainer.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);

            var contentLayout = ContentContainer.AddComponent<HorizontalLayoutGroup>();
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = true;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.spacing = UIConstants.SETTINGS_LABEL_SPACING;
            contentLayout.childAlignment = TextAnchor.MiddleLeft;

            var contentSizeFitter = ContentContainer.AddComponent<ContentSizeFitter>();
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            // Add LayoutElement to control minimum height
            var layoutElement = ContentContainer.AddComponent<LayoutElement>();
            layoutElement.minHeight = 30; // Reduced from 36 to make UI more compact
            layoutElement.preferredHeight = 30;
        }

        /// <summary>
        /// Build the setting control content (override in derived classes)
        /// </summary>
        protected abstract void BuildContent();

        /// <summary>
        /// Build the description text below the control
        /// </summary>
        private void BuildDescription()
        {
            if (string.IsNullOrEmpty(SettingsEntry.Description))
            {
                return; // No description
            }

            DescriptionObject = new GameObject("Description");
            DescriptionObject.transform.SetParent(transform, false);

            var descRect = DescriptionObject.AddComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0, 1);
            descRect.anchorMax = new Vector2(1, 1);
            descRect.pivot = new Vector2(0.5f, 1);

            DescriptionText = DescriptionObject.AddComponent<Text>();
            DescriptionText.text = SettingsEntry.Description;
            DescriptionText.font = UIConstants.DefaultFont;
            DescriptionText.fontSize = UIConstants.SETTINGS_DESCRIPTION_FONT_SIZE;
            DescriptionText.color = UIConstants.SETTINGS_DESCRIPTION_COLOR;
            DescriptionText.alignment = TextAnchor.UpperLeft;
            DescriptionText.horizontalOverflow = HorizontalWrapMode.Wrap;
            DescriptionText.verticalOverflow = VerticalWrapMode.Truncate;

            // Add ContentSizeFitter for auto-sizing
            var descSizeFitter = DescriptionObject.AddComponent<ContentSizeFitter>();
            descSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            descSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            // Add LayoutElement for bottom margin
            var descLayout = DescriptionObject.AddComponent<LayoutElement>();
            descLayout.preferredHeight = -1; // Auto-size
        }

        /// <summary>
        /// Helper to create a label for the setting
        /// </summary>
        protected GameObject CreateLabel()
        {
            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(ContentContainer.transform, false);

            var labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0.5f);
            labelRect.anchorMax = new Vector2(0, 0.5f);
            labelRect.pivot = new Vector2(0, 0.5f);

            LabelText = labelObj.AddComponent<Text>();
            LabelText.text = SettingsEntry.Name;
            LabelText.font = UIConstants.DefaultFont;
            LabelText.fontSize = UIConstants.SETTINGS_LABEL_FONT_SIZE;
            LabelText.color = UIConstants.SETTINGS_LABEL_COLOR;
            LabelText.alignment = TextAnchor.MiddleLeft;

            // Set label to take up a reasonable amount of space
            var labelLayout = labelObj.AddComponent<LayoutElement>();
            labelLayout.preferredWidth = UIConstants.SETTINGS_LABEL_WIDTH;
            labelLayout.flexibleWidth = 0;

            return labelObj;
        }

        /// <summary>
        /// Cleanup when destroyed
        /// </summary>
        protected virtual void OnDestroy()
        {
            // Unsubscribe from language changes
            if (_languageChangeHandler != null)
            {
                LocalizationHelper.OnLanguageChanged -= _languageChangeHandler;
                _languageChangeHandler = null;
            }

            // Derived classes can override to unsubscribe from events
        }
    }
}

