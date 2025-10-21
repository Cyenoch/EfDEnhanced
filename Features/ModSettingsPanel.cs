using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EfDEnhanced.Utils;
using EfDEnhanced.Utils.Settings;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Duckov;
using Duckov.UI.Animations;

namespace EfDEnhanced.Features
{
    /// <summary>
    /// Settings panel UI for EfDEnhanced mod
    /// Automatically builds UI from ModSettings entries
    /// Opened from pause menu
    /// Inherits from UIPanel and manually blocks input when open
    /// </summary>
    public class ModSettingsPanel : UIPanel
    {
        private Canvas? canvas;
        private CanvasGroup? canvasGroup;
        private GameObject? contentPanel;
        private Coroutine? fadeCoroutine;
        private const float FadeDuration = 0.2f;

        private void Awake()
        {
            ModLogger.Log("ModSettingsPanel", "Awake called - building UI");
            BuildUI();
        }

        private void Update()
        {
            // Handle ESC key to close the panel when it's open
            if (IsOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                ModLogger.Log("ModSettingsPanel", "ESC pressed - closing settings panel");
                CloseAndReturnToPauseMenu();
            }
        }

        private void CancelFadeIfRunning()
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }
        }

        private Coroutine StartFade(IEnumerator routine)
        {
            CancelFadeIfRunning();
            fadeCoroutine = StartCoroutine(routine);
            return fadeCoroutine;
        }

        private void ApplyCanvasState(float alpha, bool interactable, bool blocksRaycasts)
        {
            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.alpha = alpha;
            canvasGroup.interactable = interactable;
            canvasGroup.blocksRaycasts = blocksRaycasts;
        }

        /// <summary>
        /// Build the settings UI programmatically from ModSettings
        /// </summary>
        private void BuildUI()
        {
            try
            {
                ModLogger.Log("ModSettingsPanel", "BuildUI started");

                // Only setup canvas components if they don't exist
                if (canvas == null)
                {
                    canvas = gameObject.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvas.sortingOrder = 1000;
                    ModLogger.Log("ModSettingsPanel", "Canvas created");
                }

                if (gameObject.GetComponent<CanvasScaler>() == null)
                {
                    var canvasScaler = gameObject.AddComponent<CanvasScaler>();
                    canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    canvasScaler.referenceResolution = new Vector2(1920, 1080);
                }

                if (gameObject.GetComponent<GraphicRaycaster>() == null)
                {
                    gameObject.AddComponent<GraphicRaycaster>();
                }

                if (canvasGroup == null)
                {
                    ModLogger.Log("ModSettingsPanel", "About to create CanvasGroup");
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                    ModLogger.Log("ModSettingsPanel", $"CanvasGroup created - null? {canvasGroup == null}");

                    canvasGroup.alpha = 0f;
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;
                    ModLogger.Log("ModSettingsPanel", "CanvasGroup configured");
                }

                // Create or recreate content
                RebuildContent();

                ModLogger.Log("ModSettingsPanel", "UI built successfully");
            }
            catch (System.Exception ex)
            {
                ModLogger.LogError($"Failed to build settings UI: {ex}");
            }
        }

        /// <summary>
        /// Rebuild just the content panel and its children
        /// </summary>
        private void RebuildContent()
        {
            // Create semi-transparent background (only if it doesn't exist)
            if (transform.Find("Background") == null)
            {
                CreateBackground();
            }

            // Ensure content panel exists
            if (contentPanel == null)
            {
                contentPanel = new GameObject("ContentPanel");
                contentPanel.transform.SetParent(transform, false);

                var contentRect = contentPanel.AddComponent<RectTransform>();
                contentRect.anchorMin = new Vector2(0.5f, 0.5f);
                contentRect.anchorMax = new Vector2(0.5f, 0.5f);
                contentRect.sizeDelta = new Vector2(800, 900);
                contentRect.anchoredPosition = Vector2.zero;

                // Content background
                var contentBg = contentPanel.AddComponent<Image>();
                contentBg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
            }

            // Clear existing content children but keep the contentPanel itself
            for (int i = contentPanel.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(contentPanel.transform.GetChild(i).gameObject);
            }

            // Add content
            CreateHeader();
            CreateScrollView();
            CreateFooter();

            // Maintain current state if the panel was open
            if (canvasGroup != null && IsOpen)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
            else if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }

        private void CreateBackground()
        {
            var bg = new GameObject("Background");
            bg.transform.SetParent(transform, false);

            var bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;

            var bgImage = bg.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.7f);

            // Click background to close
            var button = bg.AddComponent<Button>();
            button.onClick.AddListener(() => CloseAndReturnToPauseMenu());
        }

        private void CreateHeader()
        {
            if (contentPanel == null) return;

            var header = new GameObject("Header");
            header.transform.SetParent(contentPanel.transform, false);

            var headerRect = header.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 1);
            headerRect.anchorMax = new Vector2(1, 1);
            headerRect.sizeDelta = new Vector2(0, 80);
            headerRect.anchoredPosition = new Vector2(0, -40);

            // Title
            var title = new GameObject("Title");
            title.transform.SetParent(header.transform, false);

            var titleRect = title.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.sizeDelta = Vector2.zero;

            var titleText = title.AddComponent<TextMeshProUGUI>();
            titleText.text = LocalizationHelper.Get("Settings_Title");
            titleText.fontSize = 32;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;
            titleText.fontStyle = FontStyles.Bold;
        }

        private void CreateScrollView()
        {
            if (contentPanel == null) return;

            var scrollViewObj = new GameObject("ScrollView");
            scrollViewObj.transform.SetParent(contentPanel.transform, false);

            var scrollRect = scrollViewObj.AddComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0, 0);
            scrollRect.anchorMax = new Vector2(1, 1);
            scrollRect.offsetMin = new Vector2(20, 100);
            scrollRect.offsetMax = new Vector2(-20, -100);

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

            // Content
            var content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);

            var contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 0);

            var verticalLayout = content.AddComponent<VerticalLayoutGroup>();
            verticalLayout.spacing = 8; // Reduced from 15 to 8
            verticalLayout.padding = new RectOffset(15, 15, 15, 15); // Reduced from 20 to 15
            verticalLayout.childControlHeight = false;
            verticalLayout.childControlWidth = true;
            verticalLayout.childForceExpandHeight = false;
            verticalLayout.childForceExpandWidth = true;

            content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollView.content = contentRect;
            scrollView.viewport = viewportRect;
            scrollView.horizontal = false;
            scrollView.vertical = true;

            // Build settings UI from ModSettings
            BuildSettingsFromEntries(content);
        }

        /// <summary>
        /// Automatically build settings UI from registered SettingsEntry objects
        /// </summary>
        private void BuildSettingsFromEntries(GameObject parent)
        {
            var categories = ModSettings.GetCategories().ToList();

            foreach (var category in categories)
            {
                // Create section header
                CreateSectionHeader(parent, category);

                // Get all settings in this category
                var categorySettings = ModSettings.GetSettingsByCategory(category);

                foreach (var setting in categorySettings)
                {
                    if (setting is BoolSettingsEntry boolEntry)
                    {
                        CreateToggleForEntry(parent, boolEntry);
                    }
                    else if (setting is RangedFloatSettingsEntry rangedFloatEntry)
                    {
                        CreateSliderForEntry(parent, rangedFloatEntry);
                    }
                    else if (setting is RangedIntSettingsEntry rangedIntEntry)
                    {
                        CreateSliderForEntry(parent, rangedIntEntry);
                    }
                    else if (setting is OptionsSettingsEntry optionsEntry)
                    {
                        CreateDropdownForEntry(parent, optionsEntry);
                    }
                    else if (setting is FloatSettingsEntry floatEntry)
                    {
                        // Only plain FloatSettingsEntry (not ranged)
                        CreateInputFieldForEntry(parent, floatEntry);
                    }
                    else if (setting is IntSettingsEntry intEntry)
                    {
                        // Only plain IntSettingsEntry (not ranged)
                        CreateInputFieldForEntry(parent, intEntry);
                    }
                    else if (setting is StringSettingsEntry stringEntry)
                    {
                        CreateInputFieldForEntry(parent, stringEntry);
                    }
                }

                CreateSpacer(parent);
            }
        }

        private void CreateToggleForEntry(GameObject parent, BoolSettingsEntry entry)
        {
            var rowContainer = new GameObject($"Toggle_{entry.Key}");
            rowContainer.transform.SetParent(parent.transform, false);
            var rowRect = rowContainer.AddComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(0, 32);

            var layoutGroup = rowContainer.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.spacing = 10;
            layoutGroup.padding = new RectOffset(8, 8, 2, 2);
            layoutGroup.childAlignment = TextAnchor.MiddleLeft;
            layoutGroup.childControlHeight = false;
            layoutGroup.childControlWidth = false; // Fixed: was true, now false for better layout
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;

            var toggleObj = new GameObject("Checkbox");
            toggleObj.transform.SetParent(rowContainer.transform, false);
            var toggleRect = toggleObj.AddComponent<RectTransform>();
            toggleRect.sizeDelta = new Vector2(28, 28);

            var toggleLayout = toggleObj.AddComponent<LayoutElement>();
            toggleLayout.minWidth = 28;
            toggleLayout.minHeight = 28;
            toggleLayout.preferredWidth = 28;
            toggleLayout.preferredHeight = 28;

            var bgImage = toggleObj.AddComponent<Image>();
            bgImage.color = new Color(0.15f, 0.15f, 0.15f, 1f);

            var border = toggleObj.AddComponent<Outline>();
            border.effectColor = new Color(0.45f, 0.45f, 0.45f, 1f);
            border.effectDistance = new Vector2(1.5f, -1.5f);

            var checkmarkObj = new GameObject("Checkmark");
            checkmarkObj.transform.SetParent(toggleObj.transform, false);
            var checkmarkRect = checkmarkObj.AddComponent<RectTransform>();
            checkmarkRect.anchorMin = new Vector2(0.2f, 0.2f);
            checkmarkRect.anchorMax = new Vector2(0.8f, 0.8f);
            checkmarkRect.offsetMin = Vector2.zero;
            checkmarkRect.offsetMax = Vector2.zero;

            var checkmarkImage = checkmarkObj.AddComponent<Image>();
            checkmarkImage.color = new Color(0.35f, 0.95f, 0.35f, 1f);

            var toggle = toggleObj.AddComponent<Toggle>();
            toggle.targetGraphic = bgImage;
            toggle.graphic = checkmarkImage;

            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(rowContainer.transform, false);
            var labelLayout = labelObj.AddComponent<LayoutElement>();
            labelLayout.flexibleWidth = 1;
            labelLayout.minHeight = 28;

            var labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = entry.Name;
            labelText.fontSize = 20;
            labelText.color = Color.white;
            labelText.alignment = TextAlignmentOptions.MidlineLeft;

            // Set initial value without triggering events
            toggle.SetIsOnWithoutNotify(entry.Value);
            UpdateToggleVisuals(toggle, checkmarkImage, bgImage, border);

            toggle.onValueChanged.AddListener(value =>
            {
                entry.Value = value;
                UpdateToggleVisuals(toggle, checkmarkImage, bgImage, border);
            });

            entry.ValueChanged += (sender, args) =>
            {
                // Use SetIsOnWithoutNotify to prevent event loops
                toggle.SetIsOnWithoutNotify(args.NewValue);
                UpdateToggleVisuals(toggle, checkmarkImage, bgImage, border);
            };
        }

        private static void UpdateToggleVisuals(Toggle toggle, Image checkmarkImage, Image background, Outline border)
        {
            // Add null checks to prevent exceptions during UI rebuild
            if (toggle == null || checkmarkImage == null || background == null || border == null)
            {
                return;
            }

            bool isOn = toggle.isOn;

            try
            {
                checkmarkImage.enabled = isOn;
                checkmarkImage.gameObject.SetActive(isOn);
                background.color = isOn ? new Color(0.18f, 0.32f, 0.18f, 1f) : new Color(0.15f, 0.15f, 0.15f, 1f);
                border.effectColor = isOn ? new Color(0.45f, 0.85f, 0.45f, 1f) : new Color(0.45f, 0.45f, 0.45f, 1f);
            }
            catch (System.Exception)
            {
                // Ignore exceptions during UI destruction/rebuild
            }
        }

        private void CreateSliderForEntry(GameObject parent, RangedFloatSettingsEntry entry)
        {
            var (slider, valueText) = CreateSliderRow(parent, $"Slider_{entry.Key}", entry.Name, entry.MinValue, entry.MaxValue, entry.Value, false);
            valueText.text = FormatFloatValue(entry, entry.Value);

            slider.onValueChanged.AddListener(value =>
            {
                entry.Value = value;
                valueText.text = FormatFloatValue(entry, value);
            });

            entry.ValueChanged += (sender, args) =>
            {
                slider.value = args.NewValue;
                valueText.text = FormatFloatValue(entry, args.NewValue);
            };
        }

        private void CreateSliderForEntry(GameObject parent, RangedIntSettingsEntry entry)
        {
            var (slider, valueText) = CreateSliderRow(parent, $"Slider_{entry.Key}", entry.Name, entry.MinValue, entry.MaxValue, entry.Value, true);
            valueText.text = entry.Value.ToString();

            slider.onValueChanged.AddListener(value =>
            {
                int intValue = Mathf.RoundToInt(value);
                if (entry.Value != intValue)
                {
                    entry.Value = intValue;
                }
                valueText.text = intValue.ToString();
            });

            entry.ValueChanged += (sender, args) =>
            {
                slider.value = args.NewValue;
                valueText.text = args.NewValue.ToString();
            };
        }

        private (Slider slider, TextMeshProUGUI valueText) CreateSliderRow(
            GameObject parent,
            string name,
            string labelText,
            float minValue,
            float maxValue,
            float initialValue,
            bool wholeNumbers)
        {
            var rowObj = new GameObject(name);
            rowObj.transform.SetParent(parent.transform, false);

            var rowRect = rowObj.AddComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(0, 60); // Increased height for vertical layout

            var layout = rowObj.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 4;
            layout.padding = new RectOffset(8, 8, 4, 4);
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            // Label container with value text
            var labelContainer = new GameObject("LabelContainer");
            labelContainer.transform.SetParent(rowObj.transform, false);
            var labelContainerRect = labelContainer.AddComponent<RectTransform>();
            labelContainerRect.sizeDelta = new Vector2(0, 24);

            var labelHorizontalLayout = labelContainer.AddComponent<HorizontalLayoutGroup>();
            labelHorizontalLayout.spacing = 10;
            labelHorizontalLayout.childAlignment = TextAnchor.MiddleLeft;
            labelHorizontalLayout.childControlWidth = false;
            labelHorizontalLayout.childControlHeight = true;
            labelHorizontalLayout.childForceExpandWidth = false;
            labelHorizontalLayout.childForceExpandHeight = false;

            // Label
            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(labelContainer.transform, false);
            var labelLayout = labelObj.AddComponent<LayoutElement>();
            labelLayout.flexibleWidth = 1;
            labelLayout.minHeight = 24;

            var label = labelObj.AddComponent<TextMeshProUGUI>();
            label.text = labelText;
            label.fontSize = 18;
            label.color = Color.white;
            label.alignment = TextAlignmentOptions.MidlineLeft;

            // Value label
            var valueObj = new GameObject("Value");
            valueObj.transform.SetParent(labelContainer.transform, false);
            var valueLayout = valueObj.AddComponent<LayoutElement>();
            valueLayout.minWidth = 80;
            valueLayout.preferredWidth = 80;
            valueLayout.flexibleWidth = 0;
            valueLayout.minHeight = 24;

            var valueText = valueObj.AddComponent<TextMeshProUGUI>();
            valueText.fontSize = 16;
            valueText.color = new Color(0.8f, 0.8f, 0.8f);
            valueText.alignment = TextAlignmentOptions.MidlineRight;

            // Slider container
            var sliderContainer = new GameObject("Slider");
            sliderContainer.transform.SetParent(rowObj.transform, false);
            var sliderContainerRect = sliderContainer.AddComponent<RectTransform>();
            sliderContainerRect.sizeDelta = new Vector2(0, 24);

            var slider = CreateSliderComponent(sliderContainer, minValue, maxValue, initialValue, wholeNumbers);

            return (slider, valueText);
        }

        private Slider CreateSliderComponent(GameObject container, float min, float max, float value, bool wholeNumbers)
        {
            var slider = container.AddComponent<Slider>();
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = value;
            slider.wholeNumbers = wholeNumbers;

            // Configure the RectTransform that was automatically added with the Slider
            var sliderRect = container.GetComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0, 0);
            sliderRect.anchorMax = new Vector2(1, 1);
            sliderRect.offsetMin = Vector2.zero;
            sliderRect.offsetMax = Vector2.zero;

            // Background
            var background = new GameObject("Background");
            background.transform.SetParent(container.transform, false);
            var bgRect = background.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0f, 0.3f);
            bgRect.anchorMax = new Vector2(1f, 0.7f);
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);

            // Set background reference for slider
            slider.targetGraphic = bgImage;

            // Fill Area
            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(container.transform, false);
            var fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0f, 0.3f);
            fillAreaRect.anchorMax = new Vector2(1f, 0.7f);
            fillAreaRect.offsetMin = Vector2.zero;
            fillAreaRect.offsetMax = Vector2.zero;

            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            var fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            var fillImage = fill.AddComponent<Image>();
            fillImage.color = new Color(0.2f, 0.6f, 0.8f, 1f);

            slider.fillRect = fillRect;

            // Handle Slide Area
            var handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(container.transform, false);
            var handleAreaRect = handleArea.AddComponent<RectTransform>();
            handleAreaRect.anchorMin = new Vector2(0f, 0f);
            handleAreaRect.anchorMax = new Vector2(1f, 1f);
            handleAreaRect.offsetMin = Vector2.zero;
            handleAreaRect.offsetMax = Vector2.zero;

            var handle = new GameObject("Handle");
            handle.transform.SetParent(handleArea.transform, false);
            var handleRect = handle.AddComponent<RectTransform>();
            handleRect.anchorMin = new Vector2(0.5f, 0.5f);
            handleRect.anchorMax = new Vector2(0.5f, 0.5f);
            handleRect.sizeDelta = new Vector2(14, 18); // Smaller thumb size
            var handleImage = handle.AddComponent<Image>();
            handleImage.color = new Color(0.9f, 0.9f, 0.9f, 1f);

            // Add a subtle border to the handle
            var handleOutline = handle.AddComponent<Outline>();
            handleOutline.effectColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            handleOutline.effectDistance = new Vector2(1, -1);

            slider.handleRect = handleRect;

            return slider;
        }

        private void CreateInputFieldForEntry<T>(GameObject parent, SettingsEntry<T> entry)
        {
            // Placeholder for input fields - can be implemented later if needed
            ModLogger.Log("ModSettingsPanel", $"Input field for {entry.Name} not implemented yet");
        }

        private void CreateDropdownForEntry(GameObject parent, OptionsSettingsEntry entry)
        {
            // Placeholder for dropdowns - can be implemented later if needed
            ModLogger.Log("ModSettingsPanel", $"Dropdown for {entry.Name} not implemented yet");
        }

        private string FormatFloatValue(RangedFloatSettingsEntry entry, float value)
        {
            return $"{value:F2}";
        }

        private void CreateSectionHeader(GameObject parent, string title)
        {
            var header = new GameObject($"Header_{title}");
            header.transform.SetParent(parent.transform, false);

            var rect = header.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 36); // Reduced from 40 to 36

            var text = header.AddComponent<TextMeshProUGUI>();
            text.text = title;
            text.fontSize = 24;
            text.fontStyle = FontStyles.Bold;
            text.color = new Color(1f, 0.8f, 0.2f);
            text.alignment = TextAlignmentOptions.Left;
        }

        private void CreateSpacer(GameObject parent, float height = 10) // Reduced from 20 to 10
        {
            var spacer = new GameObject("Spacer");
            spacer.transform.SetParent(parent.transform, false);

            var rect = spacer.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, height);
        }

        private void CreateFooter()
        {
            if (contentPanel == null) return;

            var footer = new GameObject("Footer");
            footer.transform.SetParent(contentPanel.transform, false);

            var footerRect = footer.AddComponent<RectTransform>();
            footerRect.anchorMin = new Vector2(0, 0);
            footerRect.anchorMax = new Vector2(1, 0);
            footerRect.sizeDelta = new Vector2(0, 80);
            footerRect.anchoredPosition = new Vector2(0, 40);

            var layout = footer.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childForceExpandWidth = false;
            layout.spacing = 24;

            AddFooterSpacer(footer);

            CreateButton(footer, LocalizationHelper.Get("Settings_ResetButton"), () =>
            {
                ModLogger.Log("ModSettingsPanel", "Reset to defaults button clicked");

                try
                {
                    bool wasOpen = IsOpen;

                    // Temporarily disable interaction to prevent event loops during rebuild
                    if (canvasGroup != null)
                    {
                        canvasGroup.interactable = false;
                    }

                    ModSettings.ResetToDefaults();
                    ModLogger.Log("ModSettingsPanel", "Settings reset, rebuilding UI");

                    // Use the new RebuildContent method instead of destroying and rebuilding everything
                    RebuildContent();

                    // Restore the open state if it was open before
                    if (canvasGroup != null)
                    {
                        ApplyCanvasState(wasOpen ? 1f : 0f, wasOpen, wasOpen);
                    }

                    ModLogger.Log("ModSettingsPanel", "UI rebuild completed");
                }
                catch (System.Exception ex)
                {
                    ModLogger.LogError($"Error during settings reset: {ex}");
                }
            });

            CreateButton(footer, LocalizationHelper.Get("Settings_CloseButton"), CloseAndReturnToPauseMenu);

            AddFooterSpacer(footer);
        }

        private static void AddFooterSpacer(GameObject parent)
        {
            var spacer = new GameObject("Spacer");
            spacer.transform.SetParent(parent.transform, false);
            spacer.AddComponent<LayoutElement>().flexibleWidth = 1;
        }

        private Button CreateButton(GameObject parent, string label, System.Action onClick)
        {
            var btnObj = new GameObject($"Button_{label}");
            btnObj.transform.SetParent(parent.transform, false);

            var rect = btnObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(220, 46);

            var btnImage = btnObj.AddComponent<Image>();
            btnImage.color = new Color(0.3f, 0.3f, 0.3f);

            var button = btnObj.AddComponent<Button>();
            button.targetGraphic = btnImage;
            button.onClick.AddListener(onClick.Invoke);

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = 20;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;

            return button;
        }

        private void CreateButton(GameObject parent, string label, Vector2 position, System.Action onClick)
        {
            var button = CreateButton(parent, label, onClick);
            var rect = button.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
        }

        /// <summary>
        /// Override OnOpen to block game input and show UI
        /// Called by UIPanel.Open() - we manually block input using InputManager
        /// </summary>
        protected override void OnOpen()
        {
            ModLogger.Log("ModSettingsPanel", "OnOpen called");
            base.OnOpen();

            if (canvasGroup == null)
            {
                ModLogger.LogError("ModSettingsPanel.OnOpen: canvasGroup is null!");
                return;
            }

            // Manually block game input and show cursor
            InputManager.DisableInput(gameObject);
            UnityEngine.Cursor.visible = true;
            UnityEngine.Cursor.lockState = CursorLockMode.None;

            // Enable interaction (GameObject is already active)
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            // Fade in the UI
            StartCoroutine(FadeIn());

            ModLogger.Log("ModSettingsPanel", $"Opened - input blocked, cursor shown");
        }

        /// <summary>
        /// Override OnClose to restore game input and hide UI
        /// Called by UIPanel.Close() - we manually restore input using InputManager
        /// </summary>
        protected override void OnClose()
        {
            base.OnClose();

            if (canvasGroup == null) return;

            // Manually restore game input
            InputManager.ActiveInput(gameObject);

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            // Fade out the UI
            StartCoroutine(FadeOut());

            ModLogger.Log("ModSettingsPanel", "Closed (game input restored)");
        }

        private IEnumerator FadeIn()
        {
            if (canvasGroup == null) yield break;

            float duration = 0.2f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                yield return null;
            }

            canvasGroup.alpha = 1f;
        }

        private IEnumerator FadeOut()
        {
            if (canvasGroup == null) yield break;

            float duration = 0.2f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                yield return null;
            }

            canvasGroup.alpha = 0f;
            // Don't SetActive(false) - keep GameObject active for future opens
        }

        /// <summary>
        /// Check if panel is currently open
        /// </summary>
        public bool IsOpen => canvasGroup != null && canvasGroup.alpha > 0f;

        /// <summary>
        /// Public Open method to show the panel
        /// </summary>
        public void Open()
        {
            // Manually trigger OnOpen (can't call base.Open as it's internal)
            OnOpen();
        }

        /// <summary>
        /// Close settings panel and return to pause menu
        /// </summary>
        private void CloseAndReturnToPauseMenu()
        {
            Close();

            // Re-open pause menu after closing settings (not needed)
            // PauseMenu.Show();
        }
    }
}