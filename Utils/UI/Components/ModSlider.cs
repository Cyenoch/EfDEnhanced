using EfDEnhanced.Utils.Settings;
using EfDEnhanced.Utils.UI.Constants;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace EfDEnhanced.Utils.UI.Components
{
    /// <summary>
    /// Mod标准化Slider组件
    /// 支持值预览、自动绑定到RangedFloatSettingsEntry/RangedIntSettingsEntry
    /// </summary>
    public class ModSlider : MonoBehaviour
    {
        private Slider? _slider;
        private TextMeshProUGUI? _label;
        private TextMeshProUGUI? _valueText;
        private RangedFloatSettingsEntry? _boundFloatSetting;
        private RangedIntSettingsEntry? _boundIntSetting;
        private bool _isInteger;
        private string _valueFormat = "F2";

        /// <summary>
        /// Slider组件
        /// </summary>
        public Slider? Slider => _slider;

        /// <summary>
        /// 当前值（float）
        /// </summary>
        public float Value
        {
            get => _slider != null ? _slider.value : 0f;
            set
            {
                if (_slider != null)
                {
                    _slider.value = value;
                }
            }
        }

        /// <summary>
        /// 创建ModSlider实例（使用更简洁的对象初始化语法）
        /// </summary>
        public static ModSlider Create(Transform parent, string name = "ModSlider")
        {
            // 创建容器（使用对象初始化器简化代码）
            var containerObj = new GameObject(name);
            containerObj.transform.SetParent(parent, false);

            var containerRect = containerObj.AddComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(0, 80);

            // 添加垂直布局（使用对象初始化器）
            var layout = containerObj.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 2;
            layout.padding = new RectOffset(8, 8, 4, 4);
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            // 创建标签容器（使用var和简化语法）
            var labelContainer = new GameObject("LabelContainer");
            labelContainer.transform.SetParent(containerObj.transform, false);

            var labelContainerRect = labelContainer.AddComponent<RectTransform>();
            labelContainerRect.sizeDelta = new Vector2(0, 24);

            var labelLayout = labelContainer.AddComponent<HorizontalLayoutGroup>();
            labelLayout.spacing = 16;
            labelLayout.childAlignment = TextAnchor.MiddleLeft;
            labelLayout.childControlWidth = false;
            labelLayout.childControlHeight = true;
            labelLayout.childForceExpandWidth = false;
            labelLayout.childForceExpandHeight = false;

            // 创建标签
            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(labelContainer.transform, false);

            var labelObjLayout = labelObj.AddComponent<LayoutElement>();
            labelObjLayout.flexibleWidth = 1;
            labelObjLayout.minHeight = 24;

            var label = labelObj.AddComponent<TextMeshProUGUI>();
            label.fontSize = UIConstants.SETTINGS_LABEL_FONT_SIZE;
            label.color = Color.white;
            label.alignment = TextAlignmentOptions.MidlineLeft;

            // 创建值显示
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

            // 创建Slider容器
            var sliderContainer = new GameObject("Slider");
            sliderContainer.transform.SetParent(containerObj.transform, false);

            var sliderContainerRect = sliderContainer.AddComponent<RectTransform>();
            sliderContainerRect.sizeDelta = new Vector2(0, 24);

            var slider = CreateSliderComponent(sliderContainer);

            // 添加ModSlider组件（使用对象初始化器）
            var modSlider = containerObj.AddComponent<ModSlider>();
            modSlider._slider = slider;
            modSlider._label = label;
            modSlider._valueText = valueText;

            // 监听值变化，更新显示（使用方法组简化Lambda）
            slider.onValueChanged.AddListener(_ => modSlider.UpdateValueDisplay());

            return modSlider;
        }

        /// <summary>
        /// 创建Slider组件（标准Unity Slider）
        /// </summary>
        private static Slider CreateSliderComponent(GameObject container)
        {
            var slider = container.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0.5f;
            slider.wholeNumbers = false;

            // 背景 (Background)
            var background = new GameObject("Background");
            background.transform.SetParent(container.transform, false);

            var bgRect = background.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0f, 0.25f);
            bgRect.anchorMax = new Vector2(1f, 0.75f);
            bgRect.sizeDelta = Vector2.zero;

            var bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            bgImage.sprite = CreateSliderSprite();
            bgImage.type = Image.Type.Sliced;

            slider.targetGraphic = bgImage;

            // Fill Area（填充容器） - 和 Background 一样的位置和大小
            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(container.transform, false);

            var fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0f, 0.25f);
            fillAreaRect.anchorMax = new Vector2(1f, 0.75f);
            fillAreaRect.anchoredPosition = Vector2.zero;
            fillAreaRect.sizeDelta = Vector2.zero;

            // Fill（实际的填充条） - 关键是 sizeDelta.x > 0，sizeDelta.y = 0
            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);

            var fillRect = fill.AddComponent<RectTransform>();
            fillRect.sizeDelta = new Vector2(10f, 0f); // Unity 默认：宽度10，高度0（自动填充）

            var fillImage = fill.AddComponent<Image>();
            fillImage.color = new Color(0.3f, 0.7f, 1f, 1f); // 更亮的蓝色
            fillImage.sprite = CreateSliderSprite();
            fillImage.type = Image.Type.Sliced;

            slider.fillRect = fillRect;

            // Handle Slide Area（滑块容器） - 充满整个 slider
            var handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(container.transform, false);

            var handleAreaRect = handleArea.AddComponent<RectTransform>();
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.sizeDelta = Vector2.zero;

            // Handle（实际的滑块） - 只设置 sizeDelta
            var handle = new GameObject("Handle");
            handle.transform.SetParent(handleArea.transform, false);

            var handleRect = handle.AddComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20f, 0f); // Unity 默认：宽度20，高度0（自动填充）

            var handleImage = handle.AddComponent<Image>();
            handleImage.color = Color.white; // 纯白色，更醒目
            handleImage.sprite = CreateSliderSprite();
            handleImage.type = Image.Type.Sliced;

            var handleOutline = handle.AddComponent<Outline>();
            handleOutline.effectColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            handleOutline.effectDistance = new Vector2(1, -1);

            slider.handleRect = handleRect;

            return slider;
        }

        /// <summary>
        /// 创建简单的Slider Sprite（纯白色方块，用于Image渲染）
        /// </summary>
        private static Sprite CreateSliderSprite()
        {
            // 创建一个1x1的白色纹理
            Texture2D texture = new(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();

            // 创建sprite，使用border使其可以被Sliced
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, 1, 1),
                new Vector2(0.5f, 0.5f),
                100.0f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(0, 0, 0, 0) // border
            );

            return sprite;
        }

        /// <summary>
        /// 设置标签文本（支持本地化）
        /// </summary>
        public ModSlider SetLabel(string localizationKey)
        {
            if (_label != null)
            {
                _label.text = LocalizationHelper.Get(localizationKey);
            }
            return this;
        }

        /// <summary>
        /// 设置范围
        /// </summary>
        public ModSlider SetRange(float min, float max)
        {
            if (_slider != null)
            {
                _slider.minValue = min;
                _slider.maxValue = max;
            }
            return this;
        }

        /// <summary>
        /// 设置为整数模式
        /// </summary>
        public ModSlider SetWholeNumbers(bool wholeNumbers)
        {
            _isInteger = wholeNumbers;
            if (_slider != null)
            {
                _slider.wholeNumbers = wholeNumbers;
            }
            if (wholeNumbers)
            {
                _valueFormat = "F0";
            }
            UpdateValueDisplay();
            return this;
        }

        /// <summary>
        /// 设置值格式化字符串
        /// </summary>
        public ModSlider SetValueFormat(string format)
        {
            _valueFormat = format;
            UpdateValueDisplay();
            return this;
        }

        /// <summary>
        /// 绑定到RangedFloatSettingsEntry
        /// </summary>
        public ModSlider BindToSetting(RangedFloatSettingsEntry setting)
        {
            if (setting == null) return this;

            _boundFloatSetting = setting;
            _isInteger = false;

            // 设置范围和初始值
            SetRange(setting.MinValue, setting.MaxValue);
            if (_slider != null)
            {
                _slider.SetValueWithoutNotify(setting.Value);
                UpdateValueDisplay();
            }

            // 监听Slider变化（使用null-conditional和简化Lambda）
            _slider?.onValueChanged.AddListener(value => _boundFloatSetting.Value = value);

            // 监听Setting变化
            setting.ValueChanged += (sender, args) =>
            {
                if (_slider != null)
                {
                    _slider.SetValueWithoutNotify(args.NewValue);
                    UpdateValueDisplay();
                }
            };

            return this;
        }

        /// <summary>
        /// 绑定到RangedIntSettingsEntry
        /// </summary>
        public ModSlider BindToSetting(RangedIntSettingsEntry setting)
        {
            if (setting == null) return this;

            _boundIntSetting = setting;
            _isInteger = true;
            _valueFormat = "F0";

            // 设置范围和初始值
            SetRange(setting.MinValue, setting.MaxValue);
            SetWholeNumbers(true);

            if (_slider != null)
            {
                _slider.SetValueWithoutNotify(setting.Value);
                UpdateValueDisplay();
            }

            // 监听Slider变化（使用简化Lambda表达式）
            _slider?.onValueChanged.AddListener(value =>
            {
                int intValue = Mathf.RoundToInt(value);
                if (_boundIntSetting != null && _boundIntSetting.Value != intValue)
                {
                    _boundIntSetting.Value = intValue;
                }
            });

            // 监听Setting变化
            setting.ValueChanged += (sender, args) =>
            {
                if (_slider != null)
                {
                    _slider.SetValueWithoutNotify(args.NewValue);
                    UpdateValueDisplay();
                }
            };

            return this;
        }

        /// <summary>
        /// 添加值变化监听
        /// </summary>
        public ModSlider OnValueChanged(UnityAction<float> callback)
        {
            _slider?.onValueChanged.AddListener(callback);
            return this;
        }

        /// <summary>
        /// 启用值预览
        /// </summary>
        public ModSlider WithValuePreview()
        {
            // 已经有值显示了
            return this;
        }

        /// <summary>
        /// 更新值显示
        /// </summary>
        private void UpdateValueDisplay()
        {
            if (_valueText != null && _slider != null)
            {
                _valueText.text = _slider.value.ToString(_valueFormat);
            }
        }

        /// <summary>
        /// 完成构建，返回GameObject
        /// </summary>
        public GameObject Build()
        {
            return gameObject;
        }

        private void OnDestroy()
        {
            _slider?.onValueChanged.RemoveAllListeners();
        }
    }
}

