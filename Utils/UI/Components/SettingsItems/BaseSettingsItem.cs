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
        
        private int _leftPadding;

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

            var descText = DescriptionObject.AddComponent<Text>();
            descText.text = SettingsEntry.Description;
            descText.font = UIConstants.DefaultFont;
            descText.fontSize = UIConstants.SETTINGS_DESCRIPTION_FONT_SIZE;
            descText.color = UIConstants.SETTINGS_DESCRIPTION_COLOR;
            descText.alignment = TextAnchor.UpperLeft;
            descText.horizontalOverflow = HorizontalWrapMode.Wrap;
            descText.verticalOverflow = VerticalWrapMode.Truncate;

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

            var labelText = labelObj.AddComponent<Text>();
            labelText.text = SettingsEntry.Name;
            labelText.font = UIConstants.DefaultFont;
            labelText.fontSize = UIConstants.SETTINGS_LABEL_FONT_SIZE;
            labelText.color = UIConstants.SETTINGS_LABEL_COLOR;
            labelText.alignment = TextAnchor.MiddleLeft;

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
            // Derived classes can override to unsubscribe from events
        }
    }
}

