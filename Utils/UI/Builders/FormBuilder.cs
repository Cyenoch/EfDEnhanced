using System.Collections.Generic;
using EfDEnhanced.Utils.Settings;
using EfDEnhanced.Utils.UI.Components;
using EfDEnhanced.Utils.UI.Constants;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EfDEnhanced.Utils.UI.Builders
{
    /// <summary>
    /// 表单构建器 - 自动生成设置UI
    /// 使用Builder模式链式调用，大幅减少重复代码
    /// </summary>
    public class FormBuilder
    {
        private readonly Transform _parent;
        private readonly List<GameObject> _elements = new List<GameObject>();
        private readonly Dictionary<GameObject, BoolSettingsEntry> _conditionalElements = new Dictionary<GameObject, BoolSettingsEntry>();

        /// <summary>
        /// 创建FormBuilder
        /// </summary>
        /// <param name="parent">父级Transform（通常是ScrollView的Content）</param>
        public FormBuilder(Transform parent)
        {
            _parent = parent;
        }

        /// <summary>
        /// 添加分节标题
        /// </summary>
        public FormBuilder AddSection(string titleLocalizationKey)
        {
            GameObject sectionObj = new GameObject($"Section_{titleLocalizationKey}");
            sectionObj.transform.SetParent(_parent, false);

            RectTransform rect = sectionObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 36);

            TextMeshProUGUI text = sectionObj.AddComponent<TextMeshProUGUI>();
            text.text = LocalizationHelper.Get(titleLocalizationKey);
            text.fontSize = UIConstants.SETTINGS_SECTION_FONT_SIZE;
            text.fontStyle = FontStyles.Bold;
            text.color = new Color(1f, 0.8f, 0.2f);
            text.alignment = TextAlignmentOptions.Left;

            _elements.Add(sectionObj);
            return this;
        }

        /// <summary>
        /// 添加Toggle（自动绑定到BoolSettingsEntry）
        /// </summary>
        /// <param name="labelLocalizationKey">标签本地化键</param>
        /// <param name="setting">绑定的设置项</param>
        /// <param name="visibilityCondition">可见性条件（可选）</param>
        /// <param name="leftPadding">左边距（用于表示层级关系，默认0）</param>
        public FormBuilder AddToggle(string labelLocalizationKey, BoolSettingsEntry setting, 
                                      BoolSettingsEntry? visibilityCondition = null, int leftPadding = 0)
        {
            if (setting == null)
            {
                ModLogger.LogWarning($"Attempted to add toggle with null setting: {labelLocalizationKey}");
                return this;
            }

            var toggle = ModToggle.Create(_parent, $"Toggle_{setting.Key}")
                .SetLabel(labelLocalizationKey)
                .BindToSetting(setting);

            GameObject element = toggle.Build();
            _elements.Add(element);

            // 应用左边距
            if (leftPadding > 0)
            {
                var layout = element.GetComponent<HorizontalLayoutGroup>();
                if (layout != null)
                {
                    layout.padding.left += leftPadding;
                }
            }

            // 如果有可见性条件，注册并设置初始状态
            if (visibilityCondition != null)
            {
                _conditionalElements[element] = visibilityCondition;
                element.SetActive(visibilityCondition.Value);

                // 监听条件变化
                visibilityCondition.ValueChanged += (sender, args) =>
                {
                    if (element != null)
                    {
                        element.SetActive(args.NewValue);
                    }
                };
            }

            return this;
        }

        /// <summary>
        /// 添加Float Slider（自动绑定到RangedFloatSettingsEntry）
        /// </summary>
        /// <param name="labelLocalizationKey">标签本地化键</param>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <param name="setting">绑定的设置项</param>
        /// <param name="visibilityCondition">可见性条件（可选）</param>
        /// <param name="leftPadding">左边距（用于表示层级关系，默认0）</param>
        public FormBuilder AddSlider(string labelLocalizationKey, float min, float max, 
                                      RangedFloatSettingsEntry setting,
                                      BoolSettingsEntry? visibilityCondition = null, int leftPadding = 0)
        {
            if (setting == null)
            {
                ModLogger.LogWarning($"Attempted to add slider with null setting: {labelLocalizationKey}");
                return this;
            }

            var slider = ModSlider.Create(_parent, $"Slider_{setting.Key}")
                .SetLabel(labelLocalizationKey)
                .SetRange(min, max)
                .BindToSetting(setting)
                .WithValuePreview();

            GameObject element = slider.Build();
            _elements.Add(element);

            // 应用左边距
            if (leftPadding > 0)
            {
                var rect = element.GetComponent<RectTransform>();
                if (rect != null)
                {
                    var layout = element.GetComponent<VerticalLayoutGroup>();
                    if (layout != null)
                    {
                        layout.padding.left += leftPadding;
                    }
                }
            }

            // 如果有可见性条件，注册并设置初始状态
            if (visibilityCondition != null)
            {
                _conditionalElements[element] = visibilityCondition;
                element.SetActive(visibilityCondition.Value);

                // 监听条件变化
                visibilityCondition.ValueChanged += (sender, args) =>
                {
                    if (element != null)
                    {
                        element.SetActive(args.NewValue);
                    }
                };
            }

            return this;
        }

        /// <summary>
        /// 添加Int Slider（自动绑定到RangedIntSettingsEntry）
        /// </summary>
        /// <param name="labelLocalizationKey">标签本地化键</param>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <param name="setting">绑定的设置项</param>
        /// <param name="visibilityCondition">可见性条件（可选）</param>
        /// <param name="leftPadding">左边距（用于表示层级关系，默认0）</param>
        public FormBuilder AddSlider(string labelLocalizationKey, int min, int max, 
                                      RangedIntSettingsEntry setting,
                                      BoolSettingsEntry? visibilityCondition = null, int leftPadding = 0)
        {
            if (setting == null)
            {
                ModLogger.LogWarning($"Attempted to add slider with null setting: {labelLocalizationKey}");
                return this;
            }

            var slider = ModSlider.Create(_parent, $"Slider_{setting.Key}")
                .SetLabel(labelLocalizationKey)
                .SetRange(min, max)
                .BindToSetting(setting)
                .SetWholeNumbers(true)
                .WithValuePreview();

            GameObject element = slider.Build();
            _elements.Add(element);

            // 应用左边距
            if (leftPadding > 0)
            {
                var rect = element.GetComponent<RectTransform>();
                if (rect != null)
                {
                    var layout = element.GetComponent<VerticalLayoutGroup>();
                    if (layout != null)
                    {
                        layout.padding.left += leftPadding;
                    }
                }
            }

            // 如果有可见性条件，注册并设置初始状态
            if (visibilityCondition != null)
            {
                _conditionalElements[element] = visibilityCondition;
                element.SetActive(visibilityCondition.Value);

                // 监听条件变化
                visibilityCondition.ValueChanged += (sender, args) =>
                {
                    if (element != null)
                    {
                        element.SetActive(args.NewValue);
                    }
                };
            }

            return this;
        }

        /// <summary>
        /// 添加自定义Toggle（不绑定到Setting）
        /// </summary>
        public FormBuilder AddCustomToggle(string labelLocalizationKey, bool initialValue, 
                                            System.Action<bool> onValueChanged)
        {
            var toggle = ModToggle.Create(_parent, "CustomToggle")
                .SetLabel(labelLocalizationKey);

            if (toggle.Toggle != null)
            {
                toggle.Toggle.isOn = initialValue;
                toggle.Toggle.onValueChanged.AddListener((value) => onValueChanged?.Invoke(value));
            }

            _elements.Add(toggle.Build());
            return this;
        }

        /// <summary>
        /// 添加自定义Slider（不绑定到Setting）
        /// </summary>
        public FormBuilder AddCustomSlider(string labelLocalizationKey, float min, float max, 
                                            float initialValue, System.Action<float> onValueChanged)
        {
            var slider = ModSlider.Create(_parent, "CustomSlider")
                .SetLabel(labelLocalizationKey)
                .SetRange(min, max)
                .WithValuePreview();

            if (slider.Slider != null)
            {
                slider.Slider.value = initialValue;
                slider.Slider.onValueChanged.AddListener((value) => onValueChanged?.Invoke(value));
            }

            _elements.Add(slider.Build());
            return this;
        }

        /// <summary>
        /// 添加空白间距
        /// </summary>
        public FormBuilder AddSpacer(float height = -1f)
        {
            if (height < 0) height = UIConstants.SECTION_SPACING;

            GameObject spacer = new GameObject("Spacer");
            spacer.transform.SetParent(_parent, false);

            RectTransform rect = spacer.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, height);

            _elements.Add(spacer);
            return this;
        }

        /// <summary>
        /// 添加描述文本
        /// </summary>
        public FormBuilder AddDescription(string textLocalizationKey)
        {
            GameObject descObj = new GameObject("Description");
            descObj.transform.SetParent(_parent, false);

            RectTransform rect = descObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 40);

            TextMeshProUGUI text = descObj.AddComponent<TextMeshProUGUI>();
            text.text = LocalizationHelper.Get(textLocalizationKey);
            text.fontSize = 16;
            text.fontStyle = FontStyles.Italic;
            text.color = new Color(0.7f, 0.7f, 0.7f);
            text.alignment = TextAlignmentOptions.Left;
            text.enableWordWrapping = true;

            LayoutElement layout = descObj.AddComponent<LayoutElement>();
            layout.preferredHeight = -1;

            _elements.Add(descObj);
            return this;
        }

        /// <summary>
        /// 添加自定义元素
        /// </summary>
        public FormBuilder AddCustomElement(GameObject element)
        {
            if (element != null)
            {
                element.transform.SetParent(_parent, false);
                _elements.Add(element);
            }
            return this;
        }

        /// <summary>
        /// 完成构建
        /// </summary>
        public FormBuilder Build()
        {
            ModLogger.Log("FormBuilder", $"Built form with {_elements.Count} elements");
            return this;
        }

        /// <summary>
        /// 清除所有元素
        /// </summary>
        public void Clear()
        {
            foreach (var element in _elements)
            {
                if (element != null)
                {
                    Object.Destroy(element);
                }
            }
            _elements.Clear();
        }

        /// <summary>
        /// 获取所有创建的元素
        /// </summary>
        public IReadOnlyList<GameObject> GetElements()
        {
            return _elements.AsReadOnly();
        }
    }
}

