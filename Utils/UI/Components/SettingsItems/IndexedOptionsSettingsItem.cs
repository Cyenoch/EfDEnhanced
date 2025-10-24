using System;
using System.Collections.Generic;
using System.Linq;
using EfDEnhanced.Utils.Settings;
using EfDEnhanced.Utils.UI.Constants;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace EfDEnhanced.Utils.UI.Components.SettingsItems
{
    /// <summary>
    /// Settings item for dropdown/options settings
    /// Uses a button-based popup menu instead of Unity's Dropdown component for simplicity
    /// </summary>
    public class IndexedOptionsSettingsItem : BaseSettingsItem
    {
        private IndexedOptionsSettingsEntry _optionsEntry = null!;
        private Button _mainButton = null!;
        private RectTransform _buttonRect = null!;
        private TextMeshProUGUI _buttonText = null!;
        private GameObject _popupRoot = null!;
        private Canvas _popupCanvas = null!;
        private RectTransform _popupRect = null!;
        private GameObject _popupPanel = null!;
        private GameObject _blocker = null!;
        private RectTransform _blockerRect = null!;
        private bool _isPopupOpen = false;
        private List<Image> _optionImages = new List<Image>();
        private Canvas? _rootCanvas;
        private RectTransform? _rootCanvasRect;
        private List<TextMeshProUGUI> _optionTexts = new List<TextMeshProUGUI>();

        public override void Initialize(ISettingsEntry entry, int leftPadding = 0)
        {
            _optionsEntry = entry as IndexedOptionsSettingsEntry ?? throw new ArgumentException("Entry must be IndexedOptionsSettingsEntry");
            base.Initialize(entry, leftPadding);
        }

        protected override void BuildContent()
        {
            try
            {
                ModLogger.Log($"IndexedOptionsSettingsItem: Building content for {_optionsEntry.Key}");
                
                // Create label
                CreateLabel();

                // Create button-based dropdown
                CreateButtonDropdown();
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"IndexedOptionsSettingsItem.BuildContent failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void CreateButtonDropdown()
        {
            // Create button container
            var buttonContainer = new GameObject("ButtonContainer");
            buttonContainer.transform.SetParent(ContentContainer.transform, false);

            var buttonContainerRect = buttonContainer.AddComponent<RectTransform>();
            buttonContainerRect.anchorMin = new Vector2(0, 0.5f);
            buttonContainerRect.anchorMax = new Vector2(0, 0.5f);
            buttonContainerRect.pivot = new Vector2(0, 0.5f);

            var buttonContainerLayout = buttonContainer.AddComponent<LayoutElement>();
            buttonContainerLayout.preferredWidth = UIConstants.SETTINGS_CONTROL_MIN_WIDTH;
            buttonContainerLayout.flexibleWidth = 1;
            buttonContainerLayout.minHeight = 30;

            // Create main button
            var buttonObj = new GameObject("OptionsButton");
            buttonObj.transform.SetParent(buttonContainer.transform, false);

            var buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.anchorMin = Vector2.zero;
            buttonRect.anchorMax = Vector2.one;
            buttonRect.sizeDelta = Vector2.zero;
            _buttonRect = buttonRect;

            var buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            _mainButton = buttonObj.AddComponent<Button>();
            _mainButton.targetGraphic = buttonImage;
            _mainButton.onClick.AddListener(TogglePopup);

            // Set button colors
            var colors = _mainButton.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            colors.selectedColor = Color.white;
            _mainButton.colors = colors;

            // Create button text
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);

            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.pivot = new Vector2(0, 0.5f);
            textRect.sizeDelta = Vector2.zero;
            textRect.offsetMin = new Vector2(10, 0);
            textRect.offsetMax = new Vector2(-25, 0);  // Leave space for arrow

            _buttonText = textObj.AddComponent<TextMeshProUGUI>();
            _buttonText.fontSize = UIConstants.SETTINGS_FONT_SIZE;
            _buttonText.color = Color.white;
            _buttonText.alignment = TextAlignmentOptions.MidlineLeft;
            _buttonText.overflowMode = TMPro.TextOverflowModes.Ellipsis;
            _buttonText.enableWordWrapping = false;
            _buttonText.margin = new Vector4(0, 0, 0, 0);
            _buttonText.raycastTarget = false;
            _buttonText.text = GetCurrentOptionText();

            // Create arrow icon
            var arrowObj = new GameObject("Arrow");
            arrowObj.transform.SetParent(buttonObj.transform, false);

            var arrowRect = arrowObj.AddComponent<RectTransform>();
            arrowRect.anchorMin = new Vector2(1, 0.5f);
            arrowRect.anchorMax = new Vector2(1, 0.5f);
            arrowRect.pivot = new Vector2(0.5f, 0.5f);
            arrowRect.sizeDelta = new Vector2(20, 20);
            arrowRect.anchoredPosition = new Vector2(-12, 0);

            var arrowText = arrowObj.AddComponent<TextMeshProUGUI>();
            arrowText.text = "▼";
            arrowText.fontSize = 12;
            arrowText.color = Color.white;
            arrowText.alignment = TextAlignmentOptions.Center;

            // Create popup panel (initially hidden)
            CreatePopupPanel(buttonContainer.transform);

            // Subscribe to settings changes
            _optionsEntry.ValueChanged += OnSettingsValueChanged;

            ModLogger.Log($"IndexedOptionsSettingsItem: Button dropdown created for {_optionsEntry.Key}");
        }

        private void CreatePopupPanel(Transform parent)
        {
            EnsureRootCanvas(parent);

            Transform popupParent = _rootCanvasRect != null ? _rootCanvasRect.transform : parent;

            _popupRoot = new GameObject("IndexedOptionsPopupRoot");
            var popupRootRect = _popupRoot.AddComponent<RectTransform>();
            popupRootRect.anchorMin = Vector2.zero;
            popupRootRect.anchorMax = Vector2.one;
            popupRootRect.offsetMin = Vector2.zero;
            popupRootRect.offsetMax = Vector2.zero;
            popupRootRect.pivot = new Vector2(0.5f, 0.5f);
            _popupRoot.transform.SetParent(popupParent, false);

            _popupCanvas = _popupRoot.AddComponent<Canvas>();
            _popupCanvas.overrideSorting = true;
            if (_rootCanvas != null)
            {
                _popupCanvas.renderMode = _rootCanvas.renderMode;
                _popupCanvas.worldCamera = _rootCanvas.worldCamera;
                _popupCanvas.sortingOrder = _rootCanvas.sortingOrder + 200;
            }
            else
            {
                _popupCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _popupCanvas.sortingOrder = 200;
            }

            _popupRoot.AddComponent<GraphicRaycaster>();

            // Create blocker first (to catch clicks outside popup)
            _blocker = new GameObject("Blocker");
            _blocker.transform.SetParent(_popupRoot.transform, false);
            _blockerRect = _blocker.AddComponent<RectTransform>();
            _blockerRect.anchorMin = Vector2.zero;
            _blockerRect.anchorMax = Vector2.one;
            _blockerRect.offsetMin = Vector2.zero;
            _blockerRect.offsetMax = Vector2.zero;
            _blocker.SetActive(false);

            var blockerImage = _blocker.AddComponent<Image>();
            blockerImage.color = new Color(0f, 0f, 0f, 0.001f);
            blockerImage.raycastTarget = true;

            var blockerButton = _blocker.AddComponent<Button>();
            blockerButton.transition = Selectable.Transition.None;
            blockerButton.onClick.AddListener(ClosePopup);

            // Create popup panel
            _popupPanel = new GameObject("PopupPanel");
            _popupPanel.transform.SetParent(_popupRoot.transform, false);
            _popupPanel.SetActive(false);

            _popupRect = _popupPanel.AddComponent<RectTransform>();
            _popupRect.anchorMin = new Vector2(0.5f, 0.5f);
            _popupRect.anchorMax = new Vector2(0.5f, 0.5f);
            _popupRect.pivot = new Vector2(0.5f, 1f);

            var popupLayoutElement = _popupPanel.AddComponent<LayoutElement>();
            popupLayoutElement.ignoreLayout = true;

            // Calculate height based on number of options
            int optionCount = _optionsEntry.Options.Length;
            float itemHeight = 28f;
            float maxHeight = 220f;
            float calculatedHeight = Mathf.Min(optionCount * itemHeight, maxHeight);
            _popupRect.sizeDelta = new Vector2(0, calculatedHeight);

            // Add background
            var bg = _popupPanel.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.15f, 1f);

            // Add outline
            var outline = _popupPanel.AddComponent<Outline>();
            outline.effectColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            outline.effectDistance = new Vector2(1, -1);

            // Create scroll view
            var scrollView = new GameObject("ScrollView");
            scrollView.transform.SetParent(_popupPanel.transform, false);

            var scrollRect = scrollView.AddComponent<RectTransform>();
            scrollRect.anchorMin = Vector2.zero;
            scrollRect.anchorMax = Vector2.one;
            scrollRect.sizeDelta = Vector2.zero;
            scrollRect.offsetMin = new Vector2(2, 2);
            scrollRect.offsetMax = new Vector2(-2, -2);

            var scrollComponent = scrollView.AddComponent<ScrollRect>();
            scrollComponent.horizontal = false;
            scrollComponent.vertical = true;
            scrollComponent.movementType = ScrollRect.MovementType.Clamped;

            scrollView.AddComponent<RectMask2D>();

            // Create content
            var content = new GameObject("Content");
            content.transform.SetParent(scrollView.transform, false);

            var contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = Vector2.zero;

            var contentLayout = content.AddComponent<VerticalLayoutGroup>();
            contentLayout.childControlHeight = false;
            contentLayout.childControlWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.childForceExpandWidth = true;
            contentLayout.spacing = 2;
            contentLayout.padding = new RectOffset(0, 0, 0, 0);

            var contentFitter = content.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            scrollComponent.content = contentRect;
            scrollComponent.viewport = scrollRect;

            // Create option buttons
            _optionImages.Clear();
            for (int i = 0; i < _optionsEntry.Options.Length; i++)
            {
                CreateOptionButton(content.transform, i);
            }
        }

        private void EnsureRootCanvas(Transform fallbackParent)
        {
            if (_rootCanvas != null && _rootCanvasRect != null)
            {
                return;
            }

            _rootCanvas = GetComponentInParent<Canvas>()?.rootCanvas;
            if (_rootCanvas != null)
            {
                _rootCanvasRect = _rootCanvas.GetComponent<RectTransform>();
                return;
            }

            ModLogger.LogWarning("IndexedOptionsSettingsItem", "Root canvas not found for popup; using local parent which may still clip dropdown.");
            _rootCanvasRect = fallbackParent as RectTransform;
        }

        private void CreateOptionButton(Transform parent, int index)
        {
            var optionObj = new GameObject($"Option_{index}");
            optionObj.transform.SetParent(parent, false);

            var optionRect = optionObj.AddComponent<RectTransform>();
            optionRect.sizeDelta = new Vector2(0, 26);

            var optionLayout = optionObj.AddComponent<LayoutElement>();
            optionLayout.preferredHeight = 26;

            var optionImage = optionObj.AddComponent<Image>();
            optionImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            
            // Store reference to the image for later color updates
            _optionImages.Add(optionImage);

            var optionButton = optionObj.AddComponent<Button>();
            optionButton.targetGraphic = optionImage;
            
            int capturedIndex = index; // Capture for closure
            optionButton.onClick.AddListener(() => OnOptionSelected(capturedIndex));

            // Set button colors
            var colors = optionButton.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            colors.selectedColor = Color.white;
            optionButton.colors = colors;

            // Create text
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(optionObj.transform, false);

            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.pivot = new Vector2(0, 0.5f);
            textRect.sizeDelta = Vector2.zero;
            textRect.offsetMin = new Vector2(10, 0);
            textRect.offsetMax = new Vector2(-10, 0);

            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = _optionsEntry.Options[index];
            text.fontSize = UIConstants.SETTINGS_FONT_SIZE;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.overflowMode = TMPro.TextOverflowModes.Ellipsis;
            text.enableWordWrapping = false;
            text.margin = new Vector4(0, 0, 0, 0);
            text.raycastTarget = false;
            
            // Store reference to option text for language change updates
            _optionTexts.Add(text);

            // Highlight current selection
            if (index == _optionsEntry.Value)
            {
                optionImage.color = new Color(0.3f, 0.4f, 0.5f, 1f);
            }
        }
        
        private void UpdateOptionColors()
        {
            try
            {
                for (int i = 0; i < _optionImages.Count; i++)
                {
                    if (_optionImages[i] != null)
                    {
                        // Highlight the selected option, normal color for others
                        _optionImages[i].color = (i == _optionsEntry.Value) 
                            ? new Color(0.3f, 0.4f, 0.5f, 1f)  // Selected color (blue-ish)
                            : new Color(0.2f, 0.2f, 0.2f, 1f);  // Normal color
                    }
                }
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"IndexedOptionsSettingsItem.UpdateOptionColors: {ex.Message}");
            }
        }

        private void TogglePopup()
        {
            try
            {
                if (_isPopupOpen)
                {
                    ClosePopup();
                }
                else
                {
                    OpenPopup();
                }
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"IndexedOptionsSettingsItem.TogglePopup: {ex.Message}");
            }
        }

        private void OpenPopup()
        {
            if (_popupPanel == null || _blocker == null)
            {
                return;
            }

            UpdatePopupPosition();

            _popupPanel.SetActive(true);
            _blocker.SetActive(true);
            _popupPanel.transform.SetAsLastSibling();
            _blocker.transform.SetSiblingIndex(Mathf.Max(0, _popupPanel.transform.GetSiblingIndex() - 1));
            _isPopupOpen = true;

            ModLogger.Log($"IndexedOptionsSettingsItem: Popup opened for {_optionsEntry.Key}");
        }

        private void ClosePopup()
        {
            if (_popupPanel == null || _blocker == null)
            {
                return;
            }

            _popupPanel.SetActive(false);
            _blocker.SetActive(false);
            _isPopupOpen = false;

            ModLogger.Log($"IndexedOptionsSettingsItem: Popup closed for {_optionsEntry.Key}");
        }

        private void UpdatePopupPosition()
        {
            try
            {
                if (_popupRect == null || _buttonRect == null)
                {
                    return;
                }

                // Force rebuild button layout first to ensure correct width
                var buttonParent = _buttonRect.parent as RectTransform;
                if (buttonParent != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(buttonParent);
                }
                
                Canvas.ForceUpdateCanvases();

                var canvasRect = _rootCanvasRect ?? _popupRoot.GetComponent<RectTransform>();
                if (canvasRect == null)
                {
                    return;
                }

                var camera = _rootCanvas != null && _rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay
                    ? _rootCanvas.worldCamera
                    : null;

                var buttonCorners = new Vector3[4];
                _buttonRect.GetWorldCorners(buttonCorners);

                var bottomLeft = buttonCorners[0];
                var topLeft = buttonCorners[1];
                var topRight = buttonCorners[2];
                var bottomRight = buttonCorners[3];

                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, RectTransformUtility.WorldToScreenPoint(camera, bottomLeft), camera, out var bottomLeftLocal);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, RectTransformUtility.WorldToScreenPoint(camera, bottomRight), camera, out var bottomRightLocal);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, RectTransformUtility.WorldToScreenPoint(camera, (bottomLeft + bottomRight) * 0.5f), camera, out var bottomCenterLocal);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, RectTransformUtility.WorldToScreenPoint(camera, (topLeft + topRight) * 0.5f), camera, out var topCenterLocal);

                float popupWidth = Mathf.Max(0f, bottomRightLocal.x - bottomLeftLocal.x);
                ModLogger.Log($"IndexedOptionsSettingsItem: Button width = {_buttonRect.rect.width}, Popup width = {popupWidth}");
                _popupRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, popupWidth);

                LayoutRebuilder.ForceRebuildLayoutImmediate(_popupRect);
                float popupHeight = _popupRect.rect.height;

                float canvasMinY = canvasRect.rect.yMin;
                float canvasMaxY = canvasRect.rect.yMax;

                const float verticalMargin = 6f;

                bool shouldOpenUp = bottomCenterLocal.y - popupHeight - verticalMargin < canvasMinY && topCenterLocal.y + popupHeight + verticalMargin <= canvasMaxY;

                _popupRect.pivot = shouldOpenUp ? new Vector2(0.5f, 0f) : new Vector2(0.5f, 1f);

                if (shouldOpenUp)
                {
                    _popupRect.anchoredPosition = new Vector2((bottomLeftLocal.x + bottomRightLocal.x) * 0.5f, topCenterLocal.y + verticalMargin);
                }
                else
                {
                    _popupRect.anchoredPosition = new Vector2((bottomLeftLocal.x + bottomRightLocal.x) * 0.5f, bottomCenterLocal.y - verticalMargin);
                }

                float halfWidth = _popupRect.rect.width * 0.5f;
                float canvasMinX = canvasRect.rect.xMin;
                float canvasMaxX = canvasRect.rect.xMax;
                float clampedX = Mathf.Clamp(_popupRect.anchoredPosition.x, canvasMinX + halfWidth, canvasMaxX - halfWidth);
                _popupRect.anchoredPosition = new Vector2(clampedX, _popupRect.anchoredPosition.y);
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"IndexedOptionsSettingsItem.UpdatePopupPosition: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void OnOptionSelected(int index)
        {
            try
            {
                ModLogger.Log($"IndexedOptionsSettingsItem: Option {index} selected for {_optionsEntry.Key}");
                
                if (index < 0 || index >= _optionsEntry.Options.Length)
                {
                    ModLogger.LogError($"IndexedOptionsSettingsItem: Invalid option index {index}");
                    return;
                }

                _optionsEntry.Value = index;
                UpdateButtonText();
                UpdateOptionColors();
                ClosePopup();

                ModLogger.Log($"IndexedOptionsSettingsItem: Successfully set {_optionsEntry.Key} to {index}");
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"IndexedOptionsSettingsItem.OnOptionSelected: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void OnSettingsValueChanged(object sender, SettingsValueChangedEventArgs<int> e)
        {
            try
            {
                UpdateButtonText();
                UpdateOptionColors();
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"IndexedOptionsSettingsItem.OnSettingsValueChanged: {ex.Message}");
            }
        }

        private void UpdateButtonText()
        {
            if (_buttonText != null)
            {
                _buttonText.text = GetCurrentOptionText();
            }
        }

        private string GetCurrentOptionText()
        {
            try
            {
                int value = _optionsEntry.Value;
                if (value >= 0 && value < _optionsEntry.Options.Length)
                {
                    return _optionsEntry.Options[value];
                }
                return "Invalid";
            }
            catch
            {
                return "Error";
            }
        }

        /// <summary>
        /// Handle language changes by refreshing all option texts
        /// </summary>
        protected override void OnLanguageChanged(SystemLanguage newLanguage)
        {
            try
            {
                base.OnLanguageChanged(newLanguage);
                RefreshOptionTexts();
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"IndexedOptionsSettingsItem.OnLanguageChanged failed: {ex}");
            }
        }
        
        /// <summary>
        /// Refresh all option text displays
        /// </summary>
        private void RefreshOptionTexts()
        {
            try
            {
                // Update option texts
                for (int i = 0; i < _optionTexts.Count && i < _optionsEntry.Options.Length; i++)
                {
                    if (_optionTexts[i] != null)
                    {
                        _optionTexts[i].text = _optionsEntry.Options[i];
                    }
                }
                
                // Update button text
                UpdateButtonText();
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"IndexedOptionsSettingsItem.RefreshOptionTexts failed: {ex}");
            }
        }

        protected override void OnDestroy()
        {
            if (_optionsEntry != null)
            {
                _optionsEntry.ValueChanged -= OnSettingsValueChanged;
            }
            if (_mainButton != null)
            {
                _mainButton.onClick.RemoveListener(TogglePopup);
            }
            if (_blocker != null)
            {
                var blockerButton = _blocker.GetComponent<Button>();
                if (blockerButton != null)
                {
                    blockerButton.onClick.RemoveListener(ClosePopup);
                }
            }
            if (_popupRoot != null)
            {
                Destroy(_popupRoot);
            }
            _optionImages.Clear();
            _optionTexts.Clear();
            LocalizationHelper.OnLanguageChanged -= OnLanguageChanged;
            base.OnDestroy();
        }
    }
}
