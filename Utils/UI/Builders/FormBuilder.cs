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
        public FormBuilder AddToggle(string labelLocalizationKey, BoolSettingsEntry setting)
        {
            if (setting == null)
            {
                ModLogger.LogWarning($"Attempted to add toggle with null setting: {labelLocalizationKey}");
                return this;
            }

            var toggle = ModToggle.Create(_parent, $"Toggle_{setting.Key}")
                .SetLabel(labelLocalizationKey)
                .BindToSetting(setting);

            _elements.Add(toggle.Build());
            return this;
        }

        /// <summary>
        /// 添加Float Slider（自动绑定到RangedFloatSettingsEntry）
        /// </summary>
        public FormBuilder AddSlider(string labelLocalizationKey, float min, float max, 
                                      RangedFloatSettingsEntry setting)
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

            _elements.Add(slider.Build());
            return this;
        }

        /// <summary>
        /// 添加Int Slider（自动绑定到RangedIntSettingsEntry）
        /// </summary>
        public FormBuilder AddSlider(string labelLocalizationKey, int min, int max, 
                                      RangedIntSettingsEntry setting)
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

            _elements.Add(slider.Build());
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

