using EfDEnhanced.Utils.UI.Constants;
using UnityEngine;
using UnityEngine.UI;

namespace EfDEnhanced.Utils.UI.Components.SettingsItems
{
    /// <summary>
    /// Section header item for organizing settings into categories
    /// </summary>
    public class SectionHeaderItem : MonoBehaviour
    {
        public void Initialize(string sectionText)
        {
            SetupLayout();
            CreateSectionText(sectionText);
        }

        private void SetupLayout()
        {
            var rectTransform = gameObject.GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(0.5f, 1);

            // Add VerticalLayoutGroup for padding control
            var layoutGroup = gameObject.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.padding = new RectOffset(0, 0, 10, 4); // Top padding for separation, small bottom padding

            // Add ContentSizeFitter
            var sizeFitter = gameObject.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        }

        private void CreateSectionText(string text)
        {
            var textObj = new GameObject("SectionText");
            textObj.transform.SetParent(transform, false);

            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.sizeDelta = Vector2.zero;

            var textComponent = textObj.AddComponent<Text>();
            textComponent.text = text;
            textComponent.font = UIConstants.DefaultFont;
            textComponent.fontSize = UIConstants.SETTINGS_SECTION_FONT_SIZE;
            textComponent.color = UIConstants.QUEST_TITLE_COLOR; // Use golden color for section headers
            textComponent.alignment = TextAnchor.MiddleLeft;
            textComponent.fontStyle = FontStyle.Bold;

            // Add LayoutElement to control text height
            var textLayout = textObj.AddComponent<LayoutElement>();
            textLayout.minHeight = 26; // Slightly larger for better readability
            textLayout.preferredHeight = 26;
        }
    }
}

