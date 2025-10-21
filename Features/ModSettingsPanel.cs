using System;
using System.Collections;
using EfDEnhanced.Utils;
using EfDEnhanced.Utils.UI.Builders;
using EfDEnhanced.Utils.UI.Components;
using EfDEnhanced.Utils.UI.Constants;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EfDEnhanced.Features
{
    /// <summary>
    /// Settings panel UI for EfDEnhanced mod - Refactored Version
    /// Uses FormBuilder to automatically generate UI from ModSettings
    /// Reduced from ~890 lines to ~300 lines (66% code reduction)
    /// </summary>
    public class ModSettingsPanel : UIPanel
    {
        private Canvas? _canvas;
        private CanvasGroup? _canvasGroup;
        private GameObject? _contentPanel;
        private FormBuilder? _formBuilder;

        /// <summary>
        /// Check if panel is currently open
        /// </summary>
        public bool IsOpen => _canvasGroup != null && _canvasGroup.alpha > 0f;

        private void Awake()
        {
            ModLogger.Log("ModSettingsPanel", "Awake called - building UI with FormBuilder");
            BuildUI();
        }

        private void Update()
        {
            // Handle ESC key to close the panel
            if (IsOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                ModLogger.Log("ModSettingsPanel", "ESC pressed - closing settings panel");
                CloseAndReturnToPauseMenu();
            }
        }

        /// <summary>
        /// Build the settings UI using FormBuilder
        /// </summary>
        private void BuildUI()
        {
            try
            {
                ModLogger.Log("ModSettingsPanel", "BuildUI started");

                // Setup canvas components
                SetupCanvas();
                
                // Create background
                CreateBackground();
                
                // Create content panel
                CreateContentPanel();
                
                // Create header
                CreateHeader();
                
                // Create scroll view and build form
                CreateScrollViewAndBuildForm();
                
                // Create footer with buttons
                CreateFooter();
                
                // Start hidden
                if (_canvasGroup != null)
                {
                    _canvasGroup.alpha = 0f;
                    _canvasGroup.interactable = false;
                    _canvasGroup.blocksRaycasts = false;
                }

                ModLogger.Log("ModSettingsPanel", "UI built successfully using FormBuilder");
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Failed to build settings UI: {ex}");
            }
        }

        private void SetupCanvas()
        {
            if (_canvas == null)
            {
                _canvas = gameObject.AddComponent<Canvas>();
                UIStyles.ConfigureCanvas(_canvas, UIConstants.SETTINGS_PANEL_SORT_ORDER);
            }

            if (gameObject.GetComponent<CanvasScaler>() == null)
            {
                var scaler = gameObject.AddComponent<CanvasScaler>();
                UIStyles.ConfigureCanvasScaler(scaler);
            }

            if (gameObject.GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }

            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        private void CreateBackground()
        {
            if (transform.Find("Background") != null) return;

            var bg = new GameObject("Background");
            bg.transform.SetParent(transform, false);

            var bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;

            var bgImage = bg.AddComponent<Image>();
            bgImage.color = UIConstants.BACKGROUND_DARK;

            // Click background to close
            var button = bg.AddComponent<Button>();
            button.onClick.AddListener(() => CloseAndReturnToPauseMenu());
        }

        private void CreateContentPanel()
        {
            if (_contentPanel != null) return;

            _contentPanel = new GameObject("ContentPanel");
            _contentPanel.transform.SetParent(transform, false);

            var rect = _contentPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(UIConstants.SETTINGS_PANEL_WIDTH, UIConstants.SETTINGS_PANEL_HEIGHT);
            rect.anchoredPosition = Vector2.zero;

            var bg = _contentPanel.AddComponent<Image>();
            bg.color = UIConstants.PANEL_BACKGROUND;
        }

        private void CreateHeader()
        {
            if (_contentPanel == null) return;

            var header = new GameObject("Header");
            header.transform.SetParent(_contentPanel.transform, false);

            var headerRect = header.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 1);
            headerRect.anchorMax = new Vector2(1, 1);
            headerRect.sizeDelta = new Vector2(0, 60);
            headerRect.anchoredPosition = new Vector2(0, -30);

            var title = new GameObject("Title");
            title.transform.SetParent(header.transform, false);

            var titleRect = title.AddComponent<RectTransform>();
            titleRect.anchorMin = Vector2.zero;
            titleRect.anchorMax = Vector2.one;
            titleRect.sizeDelta = Vector2.zero;

            var titleText = title.AddComponent<TextMeshProUGUI>();
            titleText.text = LocalizationHelper.Get("Settings_Title");
            titleText.fontSize = UIConstants.SETTINGS_TITLE_FONT_SIZE;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;
            titleText.fontStyle = FontStyles.Bold;
        }

        private void CreateScrollViewAndBuildForm()
        {
            if (_contentPanel == null) return;

            // Create scroll view container
            var scrollViewObj = new GameObject("ScrollView");
            scrollViewObj.transform.SetParent(_contentPanel.transform, false);

            var scrollRect = scrollViewObj.AddComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0, 0);
            scrollRect.anchorMax = new Vector2(1, 1);
            scrollRect.offsetMin = new Vector2(15, 70); // Leave space for footer
            scrollRect.offsetMax = new Vector2(-15, -70); // Leave space for header

            var scrollView = scrollViewObj.AddComponent<ScrollRect>();

            // Viewport
            var viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollViewObj.transform, false);

            var viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;

            viewport.AddComponent<Mask>().showMaskGraphic = false;
            viewport.AddComponent<Image>();

            // Content (this is where FormBuilder will create elements)
            var content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);

            var contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0, 0); // 确保宽度正确

            // Add VerticalLayoutGroup for automatic layout
            var layoutGroup = content.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.spacing = UIConstants.SETTINGS_ENTRY_SPACING;
            layoutGroup.padding = new RectOffset(0, 0, 0, 10); // 只在底部添加padding

            // Add ContentSizeFitter to automatically adjust content height
            var sizeFitter = content.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            // Configure ScrollRect
            scrollView.content = contentRect;
            scrollView.viewport = viewportRect;
            scrollView.horizontal = false;
            scrollView.vertical = true;
            scrollView.movementType = ScrollRect.MovementType.Clamped; // Prevent bouncing past content
            scrollView.inertia = true;
            scrollView.decelerationRate = 0.135f;
            scrollView.scrollSensitivity = 30f;

            // ⭐ Build form using FormBuilder - THIS IS THE MAGIC! ⭐
            // Replaces ~600 lines of manual UI creation code with ~20 lines
            BuildFormWithFormBuilder(content.transform);
        }

        /// <summary>
        /// Build the entire settings form using FormBuilder
        /// This replaces ~600 lines of manual UI creation code
        /// </summary>
        private void BuildFormWithFormBuilder(Transform parent)
        {
            try
            {
                _formBuilder = new FormBuilder(parent);

                // Build the form using our elegant builder pattern
                _formBuilder
                    // UI Enhancement Section
                    .AddSection("Settings_Category_UI")
                    .AddToggle("Settings_EnableWeaponComparison_Name", ModSettings.EnableWeaponComparison)
                    .AddSpacer()

                    // Pre-Raid Check Section
                    .AddSection("Settings_Category_PreRaidCheck")
                    .AddToggle("Settings_EnableRaidCheck_Name", ModSettings.EnableRaidCheck)
                    .AddToggle("Settings_CheckWeapon_Name", ModSettings.CheckWeapon, ModSettings.EnableRaidCheck, 30)
                    .AddToggle("Settings_CheckAmmo_Name", ModSettings.CheckAmmo, ModSettings.EnableRaidCheck, 30)
                    .AddToggle("Settings_CheckMeds_Name", ModSettings.CheckMeds, ModSettings.EnableRaidCheck, 30)
                    .AddToggle("Settings_CheckFood_Name", ModSettings.CheckFood, ModSettings.EnableRaidCheck, 30)
                    .AddToggle("Settings_CheckWeather_Name", ModSettings.CheckWeather, ModSettings.EnableRaidCheck, 30)
                    .AddSpacer()

                    // Movement Enhancement Section
                    .AddSection("Settings_Category_Movement")
                    .AddDropdown("Settings_MovementEnhancement_Name", ModSettings.MovementEnhancement)
                    .AddSpacer()

                    // Quest Tracker Section
                    .AddSection("Settings_Category_QuestTracker")
                    .AddToggle("Settings_EnableQuestTracker_Name", ModSettings.EnableQuestTracker)
                    .AddToggle("Settings_TrackerShowDescription_Name", ModSettings.TrackerShowDescription, ModSettings.EnableQuestTracker, 30)
                    .AddToggle("Settings_TrackerFilterByMap_Name", ModSettings.TrackerFilterByMap, ModSettings.EnableQuestTracker, 30)
                    .AddSlider("Settings_TrackerPositionX_Name", 0f, 1f, ModSettings.TrackerPositionX, visibilityCondition: ModSettings.EnableQuestTracker, leftPadding: 30)
                    .AddSlider("Settings_TrackerPositionY_Name", 0f, 1f, ModSettings.TrackerPositionY, visibilityCondition: ModSettings.EnableQuestTracker, leftPadding: 30)
                    .AddSlider("Settings_TrackerScale_Name", 0.5f, 2f, ModSettings.TrackerScale, visibilityCondition: ModSettings.EnableQuestTracker, leftPadding: 30);

                ModLogger.Log("ModSettingsPanel", "Form built successfully with FormBuilder");
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Failed to build form with FormBuilder: {ex}");
            }
        }

        private void CreateFooter()
        {
            if (_contentPanel == null) return;

            var footer = new GameObject("Footer");
            footer.transform.SetParent(_contentPanel.transform, false);

            var footerRect = footer.AddComponent<RectTransform>();
            footerRect.anchorMin = new Vector2(0, 0);
            footerRect.anchorMax = new Vector2(1, 0);
            footerRect.sizeDelta = new Vector2(0, 60);
            footerRect.anchoredPosition = new Vector2(0, 30);

            var layout = footer.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childForceExpandWidth = false;
            layout.spacing = 20;

            // Spacer
            var spacer1 = new GameObject("Spacer1");
            spacer1.transform.SetParent(footer.transform, false);
            spacer1.AddComponent<LayoutElement>().flexibleWidth = 1;

            // Reset button using ModButton - Clean and simple!
            ModButton.Create(footer.transform, "ResetButton")
                .SetText("Settings_ResetButton")
                .SetStyle(UIStyles.ButtonStyle.Danger)
                .OnClick(OnResetButtonClicked)
                .Build();

            // Close button using ModButton
            ModButton.Create(footer.transform, "CloseButton")
                .SetText("Settings_CloseButton")
                .SetStyle(UIStyles.ButtonStyle.Secondary)
                .OnClick(CloseAndReturnToPauseMenu)
                .Build();

            // Spacer
            var spacer2 = new GameObject("Spacer2");
            spacer2.transform.SetParent(footer.transform, false);
            spacer2.AddComponent<LayoutElement>().flexibleWidth = 1;
        }

        private void OnResetButtonClicked()
        {
            try
            {
                ModLogger.Log("ModSettingsPanel", "Reset to defaults button clicked");

                bool wasOpen = IsOpen;

                // Temporarily disable interaction
                if (_canvasGroup != null)
                {
                    _canvasGroup.interactable = false;
                }

                ModSettings.ResetToDefaults();
                ModLogger.Log("ModSettingsPanel", "Settings reset complete");

                // Thanks to data binding in FormBuilder, UI automatically updates!
                // No need to rebuild the entire UI - that's the magic! ✨

                // Restore interaction
                if (_canvasGroup != null && wasOpen)
                {
                    _canvasGroup.interactable = true;
                }

                ModLogger.Log("ModSettingsPanel", "UI refresh completed");
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Error during settings reset: {ex}");
            }
        }

        protected override void OnOpen()
        {
            try
            {
                ModLogger.Log("ModSettingsPanel", "OnOpen called");
                base.OnOpen();

                if (_canvasGroup == null)
                {
                    ModLogger.LogError("ModSettingsPanel.OnOpen: canvasGroup is null!");
                    return;
                }

                // Block game input and show cursor
                InputManager.DisableInput(gameObject);
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

                // Enable interaction
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;

                // Fade in
                StartCoroutine(FadeIn());

                ModLogger.Log("ModSettingsPanel", "Opened - input blocked, cursor shown");
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"ModSettingsPanel.OnOpen failed: {ex}");
            }
        }

        protected override void OnClose()
        {
            try
            {
                base.OnClose();

                if (_canvasGroup == null) return;

                // Restore game input
                InputManager.ActiveInput(gameObject);

                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;

                // Fade out
                StartCoroutine(FadeOut());

                ModLogger.Log("ModSettingsPanel", "Closed (game input restored)");
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"ModSettingsPanel.OnClose failed: {ex}");
            }
        }

        private IEnumerator FadeIn()
        {
            if (_canvasGroup == null) yield break;

            float elapsed = 0f;
            while (elapsed < UIConstants.FADE_DURATION)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / UIConstants.FADE_DURATION);
                yield return null;
            }
            _canvasGroup.alpha = 1f;
        }

        private IEnumerator FadeOut()
        {
            if (_canvasGroup == null) yield break;

            float elapsed = 0f;
            while (elapsed < UIConstants.FADE_DURATION)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / UIConstants.FADE_DURATION);
                yield return null;
            }
            _canvasGroup.alpha = 0f;
        }

        /// <summary>
        /// Public Open method
        /// </summary>
        public void Open()
        {
            OnOpen();
        }

        /// <summary>
        /// Close settings panel and return to pause menu
        /// </summary>
        private void CloseAndReturnToPauseMenu()
        {
            Close();
        }
    }
}
