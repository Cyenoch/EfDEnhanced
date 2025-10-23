using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace EfDEnhanced.Utils.UI.Components
{
    /// <summary>
    /// Global manager for PieMenuComponent instances
    /// Ensures only one PieMenuComponent can be active at a time
    /// </summary>
    public static class PieMenuManager
    {
        private static PieMenuComponent? _activeMenu = null;
        public static PieMenuComponent? ActiveMenu => _activeMenu;

        /// <summary>
        /// Register a menu as active, cancelling any previously active menu
        /// </summary>
        public static void RegisterActiveMenu(PieMenuComponent menu)
        {
            if (_activeMenu != null && _activeMenu != menu && _activeMenu.IsOpen)
            {
                _activeMenu.Cancel();
            }
            _activeMenu = menu;
        }

        /// <summary>
        /// Unregister a menu if it's currently active
        /// </summary>
        public static void UnregisterMenu(PieMenuComponent menu)
        {
            if (_activeMenu == menu)
            {
                _activeMenu = null;
            }
        }

        /// <summary>
        /// Cancel the currently active menu
        /// </summary>
        public static void CancelActiveMenu()
        {
            if (_activeMenu != null)
            {
                _activeMenu.Cancel();
            }
        }
    }

    /// <summary>
    /// Generic pie menu component that displays items in a radial layout
    /// Items are displayed with icons and can be selected using mouse/virtual cursor
    /// </summary>
    public class PieMenuComponent : MonoBehaviour
    {
        // Events
        public event Action<string>? OnItemInvoked; // Triggered when an item is selected
        public event Action? OnMenuShown; // Triggered when menu is shown
        public event Action? OnMenuHidden; // Triggered when menu is hidden (normal close with possible invoke)
        public event Action? OnMenuCancelled; // Triggered when menu is cancelled (no invoke)

        // Configuration
        private float _wheelRadius = 167f;
        private float _iconSize = 67f;
        private float _innerRadiusRatio = 0.4f;
        private float _segmentGapRatio = 0.95f;
        private float _scale = 1.0f;

        // Colors
        private Color _normalColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        private Color _hoverColor = new Color(0.3f, 0.3f, 0.35f, 0.85f);
        private Color _selectedColor = new Color(0.35f, 0.4f, 0.5f, 0.9f);

        // UI Components
        private Canvas? _canvas;
        private GameObject? _wheelContainer;
        private GameObject? _centerDot;
        private GameObject? _virtualCursorIndicator;
        private GameObject? _labelDisplay;
        private Text? _labelText;
        private List<PieSegment> _segments = new List<PieSegment>();

        // State
        private bool _isOpen = false;
        private Vector2 _savedMousePosition;
        private Vector2 _virtualCursorPosition;
        private int _hoveredIndex = -1;
        private int _selectedIndex = -1;

        // Items
        private List<PieMenuItem> _items = new List<PieMenuItem>();

        // Calculated properties
        private float ScaledWheelRadius => _wheelRadius * _scale;
        private float ScaledIconSize => _iconSize * _scale;
        private float ScaledInnerRadius => ScaledWheelRadius * _innerRadiusRatio;
        private float ScaledItemDistance => (ScaledInnerRadius + ScaledWheelRadius) / 2f;
        private float ScaledDeadZone => ScaledInnerRadius; // Dead zone matches inner ring radius

        public bool IsOpen => _isOpen;

        /// <summary>
        /// Initialize the pie menu with configuration
        /// </summary>
        public void Initialize(PieMenuConfig config)
        {
            _wheelRadius = config.WheelRadius;
            _iconSize = config.IconSize;
            _innerRadiusRatio = config.InnerRadiusRatio;
            _segmentGapRatio = config.SegmentGapRatio;
            _scale = config.Scale;

            if (config.NormalColor.HasValue) _normalColor = config.NormalColor.Value;
            if (config.HoverColor.HasValue) _hoverColor = config.HoverColor.Value;
            if (config.SelectedColor.HasValue) _selectedColor = config.SelectedColor.Value;

            CreateUI();
        }

        /// <summary>
        /// Set the items to display in the pie menu
        /// </summary>
        public void SetItems(List<PieMenuItem> items)
        {
            _items = items;

            // Recreate segments to match item count
            if (_wheelContainer != null)
            {
                // Clear old segments
                foreach (var segment in _segments)
                {
                    if (segment.segmentObject != null)
                    {
                        Destroy(segment.segmentObject);
                    }
                    if (segment.iconHolder != null)
                    {
                        Destroy(segment.iconHolder);
                    }
                }
                _segments.Clear();

                // Create new segments
                CreateWheelSegments();

                // Update item displays
                if (_isOpen)
                {
                    RefreshItems();
                }
            }
        }

        /// <summary>
        /// Update the scale of the pie menu
        /// </summary>
        public void SetScale(float scale)
        {
            _scale = scale;
            RecreateUI();
        }

        /// <summary>
        /// Show the pie menu
        /// </summary>
        public void Show()
        {
            if (_isOpen) return;

            // Register this menu as active, cancelling any other open menu
            PieMenuManager.RegisterActiveMenu(this);

            _isOpen = true;
            gameObject.SetActive(true);

            // Save mouse position
            _savedMousePosition = Input.mousePosition;

            // Reset virtual cursor
            _virtualCursorPosition = Vector2.zero;

            // Reset selection
            _selectedIndex = -1;
            _hoveredIndex = -1;

            // Refresh items
            RefreshItems();

            OnMenuShown?.Invoke();
        }

        /// <summary>
        /// Hide the pie menu (normal close, may invoke selected item)
        /// </summary>
        /// <param name="invokeSelectedItem">If true, invoke the selected item before hiding</param>
        public void Hide(bool invokeSelectedItem = false)
        {
            if (!_isOpen) return;

            // Invoke selected item if requested
            if (invokeSelectedItem && _hoveredIndex >= 0 && _hoveredIndex < _items.Count)
            {
                string itemId = _items[_hoveredIndex].Id;
                OnItemInvoked?.Invoke(itemId);
            }

            _isOpen = false;
            gameObject.SetActive(false);
            UpdateSegmentVisuals();

            // Unregister from global manager
            PieMenuManager.UnregisterMenu(this);

            OnMenuHidden?.Invoke();
        }

        /// <summary>
        /// Cancel the pie menu (abnormal close, never invokes items)
        /// Use this for closing due to game state changes, pause, etc.
        /// </summary>
        public void Cancel()
        {
            if (!_isOpen) return;

            _isOpen = false;
            gameObject.SetActive(false);
            UpdateSegmentVisuals();

            // Unregister from global manager
            PieMenuManager.UnregisterMenu(this);

            OnMenuCancelled?.Invoke();
        }

        /// <summary>
        /// Toggle the pie menu
        /// </summary>
        public void Toggle()
        {
            if (_isOpen)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        /// <summary>
        /// Get the saved mouse position (for preventing camera movement)
        /// </summary>
        public Vector2 GetSavedMousePosition()
        {
            return _savedMousePosition;
        }

        private void Update()
        {
            if (!_isOpen) return;

            // Update virtual cursor position from mouse delta
            if (Mouse.current != null)
            {
                Vector2 mouseDelta = Mouse.current.delta.ReadValue();
                _virtualCursorPosition += mouseDelta;
            }

            // Update virtual cursor indicator position
            if (_virtualCursorIndicator != null)
            {
                RectTransform cursorRect = _virtualCursorIndicator.GetComponent<RectTransform>();
                if (cursorRect != null)
                {
                    cursorRect.anchoredPosition = _virtualCursorPosition;
                }
            }

            // Calculate hovered segment
            if (_virtualCursorPosition.magnitude > ScaledDeadZone)
            {
                float angle = Mathf.Atan2(_virtualCursorPosition.x, _virtualCursorPosition.y) * Mathf.Rad2Deg;
                if (angle < 0) angle += 360f;

                float angleStep = 360f / _items.Count;
                float adjustedAngle = angle + (angleStep / 2f);
                if (adjustedAngle >= 360f) adjustedAngle -= 360f;

                int segmentIndex = Mathf.FloorToInt(adjustedAngle / angleStep);
                segmentIndex = Mathf.Clamp(segmentIndex, 0, _items.Count - 1);

                if (segmentIndex != _hoveredIndex)
                {
                    _hoveredIndex = segmentIndex;
                    UpdateSegmentVisuals();
                    UpdateLabelDisplay();
                }
            }
            else
            {
                if (_hoveredIndex != -1)
                {
                    _hoveredIndex = -1;
                    UpdateSegmentVisuals();
                    UpdateLabelDisplay();
                }
            }

            // Handle input
            // Left click: invoke if hovered, cancel if not
            if (Input.GetMouseButtonDown(0))
            {
                if (_hoveredIndex >= 0 && _hoveredIndex < _items.Count)
                {
                    // Invoke selected item
                    string itemId = _items[_hoveredIndex].Id;
                    OnItemInvoked?.Invoke(itemId);
                    Cancel(); // Cancel after invoking (don't invoke again)
                }
                else
                {
                    // No item selected, just cancel
                    Cancel();
                }
            }
            // Right click (press or release) or Escape: cancel menu
            else if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonUp(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                Cancel();
            }
        }

        private void CreateUI()
        {
            // Create canvas
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 1000;

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            gameObject.AddComponent<GraphicRaycaster>();

            // Create wheel container
            _wheelContainer = new GameObject("WheelContainer");
            _wheelContainer.transform.SetParent(transform, false);

            RectTransform containerRect = _wheelContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.pivot = new Vector2(0.5f, 0.5f);
            containerRect.anchoredPosition = Vector2.zero;
            containerRect.sizeDelta = new Vector2(ScaledWheelRadius * 2.5f, ScaledWheelRadius * 2.5f);

            // Create segments
            CreateWheelSegments();

            // Create center dot
            CreateCenterDot();

            // Create virtual cursor indicator
            CreateVirtualCursorIndicator();

            // Create label display
            CreateLabelDisplay();

            gameObject.SetActive(false);
        }

        private void RecreateUI()
        {
            if (_wheelContainer != null)
            {
                Destroy(_wheelContainer);
                _segments.Clear();
            }

            if (_centerDot != null)
            {
                Destroy(_centerDot);
            }

            if (_virtualCursorIndicator != null)
            {
                Destroy(_virtualCursorIndicator);
            }

            if (_labelDisplay != null)
            {
                Destroy(_labelDisplay);
                _labelText = null;
            }

            // Recreate
            _wheelContainer = new GameObject("WheelContainer");
            _wheelContainer.transform.SetParent(transform, false);

            RectTransform containerRect = _wheelContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.pivot = new Vector2(0.5f, 0.5f);
            containerRect.anchoredPosition = Vector2.zero;
            containerRect.sizeDelta = new Vector2(ScaledWheelRadius * 2.5f, ScaledWheelRadius * 2.5f);

            CreateWheelSegments();
            CreateCenterDot();
            CreateVirtualCursorIndicator();
            CreateLabelDisplay(); // Recreate label display with proper settings

            if (_isOpen)
            {
                RefreshItems();
                UpdateLabelDisplay(); // Update label if menu is open
            }
        }

        private void CreateWheelSegments()
        {
            if (_wheelContainer == null || _items.Count == 0) return;

            float angleStep = 360f / _items.Count;
            float startOffset = -angleStep / 2f;

            for (int i = 0; i < _items.Count; i++)
            {
                float segmentStartAngle = startOffset + (i * angleStep);
                float itemAngle = segmentStartAngle + (angleStep / 2f);
                float angleRad = itemAngle * Mathf.Deg2Rad;

                // Create background segment
                GameObject segmentObj = new GameObject($"Segment_{i}");
                segmentObj.transform.SetParent(_wheelContainer.transform, false);

                RectTransform segmentRect = segmentObj.AddComponent<RectTransform>();
                segmentRect.anchorMin = new Vector2(0.5f, 0.5f);
                segmentRect.anchorMax = new Vector2(0.5f, 0.5f);
                segmentRect.pivot = new Vector2(0.5f, 0.5f);
                segmentRect.sizeDelta = new Vector2(ScaledWheelRadius * 2f, ScaledWheelRadius * 2f);
                segmentRect.anchoredPosition = Vector2.zero;
                segmentRect.localRotation = Quaternion.Euler(0, 0, -segmentStartAngle);

                Image segmentImage = segmentObj.AddComponent<Image>();
                segmentImage.color = _normalColor;
                segmentImage.raycastTarget = false;
                segmentImage.type = Image.Type.Filled;
                segmentImage.fillMethod = Image.FillMethod.Radial360;
                segmentImage.fillOrigin = (int)Image.Origin360.Top;
                // When there's only one item, fill the whole circle; otherwise use gap ratio
                segmentImage.fillAmount = _items.Count == 1 ? 1.0f : (_segmentGapRatio / _items.Count);
                segmentImage.fillClockwise = true;

                Texture2D ringTexture = CreateRingTexture();
                Sprite sprite = Sprite.Create(
                    ringTexture,
                    new Rect(0, 0, ringTexture.width, ringTexture.height),
                    new Vector2(0.5f, 0.5f),
                    100f
                );
                segmentImage.sprite = sprite;

                // Create item icon
                GameObject iconHolder = new GameObject($"Item_{i}");
                iconHolder.transform.SetParent(_wheelContainer.transform, false);

                RectTransform iconRect = iconHolder.AddComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0.5f, 0.5f);
                iconRect.anchorMax = new Vector2(0.5f, 0.5f);
                iconRect.pivot = new Vector2(0.5f, 0.5f);
                iconRect.sizeDelta = new Vector2(ScaledIconSize, ScaledIconSize);

                Vector2 itemPosition = new Vector2(
                    Mathf.Sin(angleRad) * ScaledItemDistance,
                    Mathf.Cos(angleRad) * ScaledItemDistance
                );
                iconRect.anchoredPosition = itemPosition;

                // Create icon image
                GameObject iconObj = new GameObject("Icon");
                iconObj.transform.SetParent(iconHolder.transform, false);

                RectTransform iconImageRect = iconObj.AddComponent<RectTransform>();
                iconImageRect.anchorMin = Vector2.zero;
                iconImageRect.anchorMax = Vector2.one;
                iconImageRect.sizeDelta = Vector2.zero;
                iconImageRect.anchoredPosition = Vector2.zero;

                Image iconImage = iconObj.AddComponent<Image>();
                iconImage.raycastTarget = false;
                iconImage.enabled = false;

                // Create count text (bottom right corner)
                GameObject countTextObj = new GameObject("CountText");
                countTextObj.transform.SetParent(_wheelContainer.transform, false);

                RectTransform countTextRect = countTextObj.AddComponent<RectTransform>();
                countTextRect.anchorMin = new Vector2(0.5f, 0.5f);
                countTextRect.anchorMax = new Vector2(0.5f, 0.5f);
                countTextRect.pivot = new Vector2(.5f, .5f);
                
                // Position count text closer to pie center using same angle but shorter distance
                float countTextDistance = ScaledItemDistance * .5f;
                Vector2 countTextPosition = new Vector2(
                    Mathf.Sin(angleRad) * countTextDistance,
                    Mathf.Cos(angleRad) * countTextDistance
                );
                countTextRect.anchoredPosition = countTextPosition;
                countTextRect.sizeDelta = new Vector2(30f, 20f);

                Text countText = countTextObj.AddComponent<Text>();
                countText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                countText.fontSize = 14;
                countText.fontStyle = FontStyle.Bold;
                countText.color = Color.white;
                countText.alignment = TextAnchor.MiddleCenter;
                countText.raycastTarget = false;
                countText.enabled = false;

                // Add outline for better visibility
                Outline outline = countTextObj.AddComponent<Outline>();
                outline.effectColor = Color.black;
                outline.effectDistance = new Vector2(1f, -1f);

                PieSegment segment = new PieSegment
                {
                    index = i,
                    segmentObject = segmentObj,
                    image = segmentImage,
                    iconImage = iconImage,
                    iconHolder = iconHolder,
                    countText = countText
                };

                _segments.Add(segment);
            }
        }

        private Texture2D CreateRingTexture()
        {
            int size = 256;
            Texture2D texture = new Texture2D(size, size);
            Color[] pixels = new Color[size * size];

            Vector2 center = new Vector2(size / 2f, size / 2f);
            float outerRadius = size / 2f;
            float innerRadius = outerRadius * _innerRadiusRatio;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pos = new Vector2(x, y) - center;
                    float distance = pos.magnitude;

                    if (distance >= innerRadius && distance <= outerRadius)
                    {
                        float alpha = 1f;

                        // Smooth edges
                        if (distance < innerRadius + 2f)
                            alpha = (distance - innerRadius) / 2f;
                        else if (distance > outerRadius - 2f)
                            alpha = (outerRadius - distance) / 2f;

                        pixels[y * size + x] = new Color(1, 1, 1, alpha);
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        private void CreateCenterDot()
        {
            if (_wheelContainer == null) return;

            _centerDot = new GameObject("CenterDot");
            _centerDot.transform.SetParent(_wheelContainer.transform, false);

            RectTransform dotRect = _centerDot.AddComponent<RectTransform>();
            dotRect.anchorMin = new Vector2(0.5f, 0.5f);
            dotRect.anchorMax = new Vector2(0.5f, 0.5f);
            dotRect.pivot = new Vector2(0.5f, 0.5f);
            dotRect.anchoredPosition = Vector2.zero;
            dotRect.sizeDelta = new Vector2(20 * _scale, 20 * _scale);

            Image dotImage = _centerDot.AddComponent<Image>();
            dotImage.color = new Color(1f, 1f, 1f, 0.8f);

            Texture2D dotTexture = new Texture2D(32, 32);
            Color[] pixels = new Color[32 * 32];
            Vector2 center = new Vector2(16, 16);

            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    if (distance <= 14f)
                    {
                        float alpha = 1f - (distance / 14f) * 0.3f;
                        pixels[y * 32 + x] = new Color(1, 1, 1, alpha);
                    }
                    else
                    {
                        pixels[y * 32 + x] = Color.clear;
                    }
                }
            }

            dotTexture.SetPixels(pixels);
            dotTexture.Apply();

            Sprite dotSprite = Sprite.Create(
                dotTexture,
                new Rect(0, 0, 32, 32),
                new Vector2(0.5f, 0.5f),
                100f
            );

            dotImage.sprite = dotSprite;
            dotImage.raycastTarget = false;
        }

        private void CreateVirtualCursorIndicator()
        {
            // Disable for now, use it when debugging
            return;

            /* Disabled - enable when debugging
            if (_wheelContainer == null) return;
            
            _virtualCursorIndicator = new GameObject("VirtualCursor");
            _virtualCursorIndicator.transform.SetParent(_wheelContainer.transform, false);
            _virtualCursorIndicator.transform.SetAsLastSibling();
            
            RectTransform cursorRect = _virtualCursorIndicator.AddComponent<RectTransform>();
            cursorRect.anchorMin = new Vector2(0.5f, 0.5f);
            cursorRect.anchorMax = new Vector2(0.5f, 0.5f);
            cursorRect.pivot = new Vector2(0.5f, 0.5f);
            cursorRect.anchoredPosition = Vector2.zero;
            cursorRect.sizeDelta = new Vector2(50 * _scale, 50 * _scale);
            
            Image cursorImage = _virtualCursorIndicator.AddComponent<Image>();
            cursorImage.color = new Color(1f, 0f, 0f, 0.9f);
            
            Texture2D cursorTexture = new Texture2D(32, 32);
            Color[] pixels = new Color[32 * 32];
            Vector2 center = new Vector2(16, 16);
            
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    
                    // Draw crosshair
                    if ((Mathf.Abs(x - 16) <= 1 && Mathf.Abs(y - 16) <= 8) ||
                        (Mathf.Abs(y - 16) <= 1 && Mathf.Abs(x - 16) <= 8))
                    {
                        pixels[y * 32 + x] = new Color(1, 0, 0, 1f);
                    }
                    else if (distance <= 3f)
                    {
                        pixels[y * 32 + x] = new Color(1, 0, 0, 1f);
                    }
                    else
                    {
                        pixels[y * 32 + x] = Color.clear;
                    }
                }
            }
            
            cursorTexture.SetPixels(pixels);
            cursorTexture.Apply();
            
            Sprite cursorSprite = Sprite.Create(
                cursorTexture,
                new Rect(0, 0, 32, 32),
                new Vector2(0.5f, 0.5f),
                100f
            );
            
            cursorImage.sprite = cursorSprite;
            cursorImage.raycastTarget = false;
            */
        }

        private void CreateLabelDisplay()
        {
            if (_wheelContainer == null) return;

            _labelDisplay = new GameObject("LabelDisplay");
            _labelDisplay.transform.SetParent(_wheelContainer.transform, false);
            _labelDisplay.transform.SetAsLastSibling(); // Ensure it's rendered on top

            RectTransform labelRect = _labelDisplay.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0.5f); // Changed from (0.5f, 0f) to center
            labelRect.anchorMax = new Vector2(0.5f, 0.5f); // Changed from (0.5f, 0f) to center
            labelRect.pivot = new Vector2(0.5f, 0.5f); // Changed from (0.5f, 0f) to center
            labelRect.anchoredPosition = new Vector2(0f, -ScaledWheelRadius * 1.3f); // Position further below the wheel
            labelRect.sizeDelta = new Vector2(ScaledWheelRadius * 3f, ScaledWheelRadius * 0.3f); // Larger size for better visibility

            _labelText = _labelDisplay.AddComponent<Text>();
            _labelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            _labelText.fontSize = 28;
            _labelText.fontStyle = FontStyle.Bold;
            _labelText.color = Color.white;
            _labelText.alignment = TextAnchor.MiddleCenter;
            _labelText.raycastTarget = false;
            _labelText.enabled = false;
            _labelText.horizontalOverflow = HorizontalWrapMode.Overflow; // Prevent text wrapping

            Outline outline = _labelDisplay.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(2f, -2f); // Larger outline for better contrast
        }

        private void RefreshItems()
        {
            for (int i = 0; i < Mathf.Min(_segments.Count, _items.Count); i++)
            {
                PieSegment segment = _segments[i];
                PieMenuItem item = _items[i];

                if (item.Icon != null)
                {
                    segment.iconImage.sprite = item.Icon;
                    segment.iconImage.color = Color.white;
                    segment.iconImage.enabled = true;
                    segment.iconHolder.SetActive(true);

                    // Update count text
                    if (segment.countText != null)
                    {
                        if (item.Count > 1)
                        {
                            segment.countText.text = item.Count.ToString();
                            segment.countText.enabled = true;
                        }
                        else
                        {
                            segment.countText.enabled = false;
                        }
                    }
                }
                else
                {
                    segment.iconHolder.SetActive(false);
                }
            }
        }

        private void UpdateSegmentVisuals()
        {
            for (int i = 0; i < _segments.Count; i++)
            {
                PieSegment segment = _segments[i];

                if (i == _hoveredIndex)
                {
                    segment.image.color = _hoverColor;
                }
                else if (i == _selectedIndex)
                {
                    segment.image.color = _selectedColor;
                }
                else
                {
                    segment.image.color = _normalColor;
                }
            }
        }

        private void UpdateLabelDisplay()
        {
            if (_labelText == null || _hoveredIndex == -1 || _hoveredIndex >= _items.Count)
            {
                if (_labelText != null)
                    _labelText.enabled = false;
                return;
            }

            PieMenuItem item = _items[_hoveredIndex];
            _labelText.text = item.Label ?? item.Id;
            _labelText.enabled = true;
        }

        private class PieSegment
        {
            public int index;
            public GameObject segmentObject = null!;
            public Image image = null!;
            public Image iconImage = null!;
            public GameObject iconHolder = null!;
            public Text? countText = null;
        }
    }

    /// <summary>
    /// Configuration for pie menu
    /// </summary>
    public struct PieMenuConfig
    {
        public float WheelRadius;
        public float IconSize;
        public float InnerRadiusRatio;
        public float SegmentGapRatio;
        public float Scale;

        public Color? NormalColor;
        public Color? HoverColor;
        public Color? SelectedColor;

        public static PieMenuConfig Default => new PieMenuConfig
        {
            WheelRadius = 167f,
            IconSize = 67f,
            InnerRadiusRatio = 0.4f,
            SegmentGapRatio = 0.95f,
            Scale = 1.0f,
            NormalColor = null,
            HoverColor = null,
            SelectedColor = null
        };
    }

    /// <summary>
    /// Item data for pie menu
    /// </summary>
    public class PieMenuItem
    {
        public string Id { get; set; } = "";
        public Sprite? Icon { get; set; }
        public int Count { get; set; } = 1;
        public string? Label { get; set; }

        public PieMenuItem(string id, Sprite? icon = null, int count = 1, string? label = null)
        {
            Id = id;
            Icon = icon;
            Count = count;
            Label = label;
        }
    }
}

