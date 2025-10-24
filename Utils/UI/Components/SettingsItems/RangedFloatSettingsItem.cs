using System;
using EfDEnhanced.Utils.Settings;
using EfDEnhanced.Utils.UI.Constants;
using UnityEngine;
using UnityEngine.UI;

namespace EfDEnhanced.Utils.UI.Components.SettingsItems
{
    /// <summary>
    /// Settings item for ranged float settings (slider)
    /// </summary>
    public class RangedFloatSettingsItem : BaseSettingsItem
    {
        private RangedFloatSettingsEntry _floatEntry = null!;
        private Slider _slider = null!;
        private Text _valueText = null!;

        public override void Initialize(ISettingsEntry entry, int leftPadding = 0)
        {
            _floatEntry = entry as RangedFloatSettingsEntry ?? throw new ArgumentException("Entry must be RangedFloatSettingsEntry");
            base.Initialize(entry, leftPadding);
        }

        protected override void BuildContent()
        {
            // Create label
            CreateLabel();

            // Create slider container
            var sliderContainer = new GameObject("SliderContainer");
            sliderContainer.transform.SetParent(ContentContainer.transform, false);

            var sliderContainerRect = sliderContainer.AddComponent<RectTransform>();
            sliderContainerRect.anchorMin = new Vector2(0, 0.5f);
            sliderContainerRect.anchorMax = new Vector2(1, 0.5f);
            sliderContainerRect.pivot = new Vector2(0.5f, 0.5f);
            sliderContainerRect.sizeDelta = Vector2.zero;

            // Add horizontal layout for slider + value
            var sliderLayout = sliderContainer.AddComponent<HorizontalLayoutGroup>();
            sliderLayout.childControlWidth = true;
            sliderLayout.childControlHeight = true;
            sliderLayout.childForceExpandWidth = false;
            sliderLayout.childForceExpandHeight = false;
            sliderLayout.spacing = 10;
            sliderLayout.childAlignment = TextAnchor.MiddleLeft;

            // Set layout element
            var sliderContainerLayout = sliderContainer.AddComponent<LayoutElement>();
            sliderContainerLayout.preferredWidth = UIConstants.SETTINGS_CONTROL_MIN_WIDTH;
            sliderContainerLayout.flexibleWidth = 1;

            // Create slider (using ModSlider but extracting the actual slider component)
            var sliderObj = new GameObject("Slider");
            sliderObj.transform.SetParent(sliderContainer.transform, false);
            
            var sliderRect = sliderObj.AddComponent<RectTransform>();
            _slider = sliderObj.AddComponent<Slider>();
            _slider.minValue = _floatEntry.MinValue;
            _slider.maxValue = _floatEntry.MaxValue;
            _slider.value = _floatEntry.Value;
            _slider.wholeNumbers = false;
            _slider.onValueChanged.AddListener(OnSliderChanged);

            // Create slider background
            var sliderBg = new GameObject("Background");
            sliderBg.transform.SetParent(sliderObj.transform, false);
            var sliderBgRect = sliderBg.AddComponent<RectTransform>();
            sliderBgRect.anchorMin = new Vector2(0, 0.5f);
            sliderBgRect.anchorMax = new Vector2(1, 0.5f);
            sliderBgRect.pivot = new Vector2(0.5f, 0.5f);
            sliderBgRect.sizeDelta = new Vector2(0, 2);
            
            var sliderBgImage = sliderBg.AddComponent<Image>();
            sliderBgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            // Create fill area
            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObj.transform, false);
            var fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0, 0.5f);
            fillAreaRect.anchorMax = new Vector2(1, 0.5f);
            fillAreaRect.pivot = new Vector2(0.5f, 0.5f);
            fillAreaRect.sizeDelta = new Vector2(0, 2);

            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            var fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0, 0);
            fillRect.anchorMax = new Vector2(0, 1);
            fillRect.pivot = new Vector2(0.5f, 0.5f);
            fillRect.sizeDelta = new Vector2(0, 0);
            
            var fillImage = fill.AddComponent<Image>();
            fillImage.color = UIConstants.BUTTON_PRIMARY;

            // Create handle area
            var handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(sliderObj.transform, false);
            var handleAreaRect = handleArea.AddComponent<RectTransform>();
            handleAreaRect.anchorMin = new Vector2(0, 0);
            handleAreaRect.anchorMax = new Vector2(1, 1);
            handleAreaRect.sizeDelta = new Vector2(-10, 0);

            var handle = new GameObject("Handle");
            handle.transform.SetParent(handleArea.transform, false);
            var handleRect = handle.AddComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(12, 12);
            var handleImage = handle.AddComponent<Image>();
            handleImage.color = Color.white;

            // Setup slider references
            _slider.fillRect = fillRect;
            _slider.handleRect = handleRect;
            _slider.targetGraphic = handleImage;

            // Layout element for slider
            var sliderLayoutElement = sliderObj.AddComponent<LayoutElement>();
            sliderLayoutElement.preferredWidth = -1;
            sliderLayoutElement.flexibleWidth = 1;
            sliderLayoutElement.minHeight = 20;

            // Create value text
            var valueTextObj = new GameObject("Value");
            valueTextObj.transform.SetParent(sliderContainer.transform, false);
            
            _valueText = valueTextObj.AddComponent<Text>();
            _valueText.font = UIConstants.DefaultFont;
            _valueText.fontSize = UIConstants.SETTINGS_FONT_SIZE;
            _valueText.color = UIConstants.SETTINGS_TEXT_COLOR;
            _valueText.alignment = TextAnchor.MiddleRight;
            _valueText.text = FormatValue(_floatEntry.Value);

            var valueLayout = valueTextObj.AddComponent<LayoutElement>();
            valueLayout.preferredWidth = 60;
            valueLayout.flexibleWidth = 0;

            // Subscribe to settings changes
            _floatEntry.ValueChanged += OnSettingsValueChanged;
        }

        private void OnSliderChanged(float value)
        {
            _floatEntry.Value = value;
            _valueText.text = FormatValue(value);
        }

        private void OnSettingsValueChanged(object sender, SettingsValueChangedEventArgs<float> e)
        {
            if (_slider != null && !Mathf.Approximately(_slider.value, e.NewValue))
            {
                _slider.value = e.NewValue;
            }
            if (_valueText != null)
            {
                _valueText.text = FormatValue(e.NewValue);
            }
        }

        private string FormatValue(float value)
        {
            // Show 2 decimal places for values between 0-10, 1 decimal for larger values
            if (Mathf.Abs(value) < 10f)
            {
                return value.ToString("F2");
            }
            else if (Mathf.Abs(value) < 100f)
            {
                return value.ToString("F1");
            }
            else
            {
                return value.ToString("F0");
            }
        }

        protected override void OnDestroy()
        {
            if (_floatEntry != null)
            {
                _floatEntry.ValueChanged -= OnSettingsValueChanged;
            }
            if (_slider != null)
            {
                _slider.onValueChanged.RemoveListener(OnSliderChanged);
            }
            base.OnDestroy();
        }
    }
}

