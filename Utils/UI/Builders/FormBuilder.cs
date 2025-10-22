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
    /// <remarks>
    /// 创建FormBuilder
    /// </remarks>
    /// <param name="parent">父级Transform（通常是ScrollView的Content）</param>
    public class FormBuilder(Transform parent)
    {
        private readonly Transform _parent = parent;
        private readonly List<GameObject> _elements = [];
        private readonly Dictionary<GameObject, BoolSettingsEntry> _conditionalElements = [];

        /// <summary>
        /// 添加分节标题
        /// </summary>
        public FormBuilder AddSection(string titleLocalizationKey)
        {
            var sectionObj = new GameObject($"Section_{titleLocalizationKey}");
            sectionObj.transform.SetParent(_parent, false);

            var rect = sectionObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 36);

            var text = sectionObj.AddComponent<TextMeshProUGUI>();
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
                    element?.SetActive(args.NewValue);
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
                    element?.SetActive(args.NewValue);
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
                    element?.SetActive(args.NewValue);
                };
            }

            return this;
        }

        /// <summary>
        /// 添加Dropdown下拉菜单（自动绑定到IndexedOptionsSettingsEntry）
        /// </summary>
        /// <param name="labelLocalizationKey">标签本地化键</param>
        /// <param name="setting">绑定的设置项</param>
        /// <param name="visibilityCondition">可见性条件（可选）</param>
        /// <param name="leftPadding">左边距（用于表示层级关系，默认0）</param>
        public FormBuilder AddDropdown(string labelLocalizationKey, IndexedOptionsSettingsEntry setting,
                                        BoolSettingsEntry? visibilityCondition = null, int leftPadding = 0)
        {
            if (setting == null)
            {
                ModLogger.LogWarning($"Attempted to add dropdown with null setting: {labelLocalizationKey}");
                return this;
            }

            GameObject container = CreateDropdownElement(labelLocalizationKey, setting, leftPadding);
            _elements.Add(container);

            // 如果有可见性条件，注册并设置初始状态
            if (visibilityCondition != null)
            {
                _conditionalElements[container] = visibilityCondition;
                container.SetActive(visibilityCondition.Value);

                // 监听条件变化
                visibilityCondition.ValueChanged += (sender, args) =>
                {
                    container?.SetActive(args.NewValue);
                };
            }

            return this;
        }

        /// <summary>
        /// 创建Dropdown元素（使用var简化类型声明）
        /// </summary>
        private GameObject CreateDropdownElement(string labelLocalizationKey, IndexedOptionsSettingsEntry setting, int leftPadding)
        {
            // Container
            var container = new GameObject($"Dropdown_{setting.Key}");
            container.transform.SetParent(_parent, false);

            var containerRect = container.AddComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(0, UIConstants.SETTINGS_FIELD_HEIGHT);

            var containerLayout = container.AddComponent<VerticalLayoutGroup>();
            containerLayout.childControlHeight = false;
            containerLayout.childControlWidth = true;
            containerLayout.childForceExpandHeight = false;
            containerLayout.childForceExpandWidth = true;
            containerLayout.spacing = 16;
            containerLayout.padding = new RectOffset(leftPadding, 0, 0, 0);

            // Label
            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(container.transform, false);

            var labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.sizeDelta = new Vector2(0, 20);

            var labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = LocalizationHelper.Get(labelLocalizationKey);
            labelText.fontSize = UIConstants.SETTINGS_LABEL_FONT_SIZE;
            labelText.color = UIConstants.SETTINGS_TEXT_COLOR;
            labelText.alignment = TextAlignmentOptions.Left;

            // Dropdown
            var dropdownObj = new GameObject("Dropdown");
            dropdownObj.transform.SetParent(container.transform, false);

            var dropdownRect = dropdownObj.AddComponent<RectTransform>();
            dropdownRect.sizeDelta = new Vector2(0, 30);

            var dropdownBg = dropdownObj.AddComponent<Image>();
            dropdownBg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            var dropdown = dropdownObj.AddComponent<TMP_Dropdown>();

            // Configure dropdown template (required for TMP_Dropdown)
            CreateDropdownTemplate(dropdownObj, dropdown);

            // Populate options
            dropdown.ClearOptions();
            dropdown.AddOptions(new List<string>(setting.Options));
            dropdown.value = setting.Value;
            dropdown.RefreshShownValue();

            // Bind to setting
            dropdown.onValueChanged.AddListener((value) =>
            {
                try
                {
                    setting.Value = value;
                    ModLogger.Log("FormBuilder", $"Dropdown {setting.Key} changed to {value} ({setting.SelectedOption})");
                }
                catch (System.Exception ex)
                {
                    ModLogger.LogError($"Failed to update dropdown setting {setting.Key}: {ex}");
                }
            });

            // Listen to setting changes from other sources（使用discards简化）
            setting.ValueChanged += (_, args) =>
            {
                if (dropdown != null && dropdown.value != args.NewValue)
                {
                    dropdown.value = args.NewValue;
                    dropdown.RefreshShownValue();
                }
            };

            return container;
        }

        /// <summary>
        /// 创建Dropdown的模板（TMP_Dropdown需要）
        /// </summary>
        private void CreateDropdownTemplate(GameObject dropdownObj, TMP_Dropdown dropdown)
        {
            // ===== Caption Label (显示当前选中值) =====
            var captionLabel = new GameObject("Label");
            captionLabel.transform.SetParent(dropdownObj.transform, false);

            var captionRect = captionLabel.AddComponent<RectTransform>();
            captionRect.anchorMin = Vector2.zero;
            captionRect.anchorMax = Vector2.one;
            captionRect.sizeDelta = Vector2.zero;
            captionRect.offsetMin = new Vector2(10, 2);
            captionRect.offsetMax = new Vector2(-30, -2); // 留出箭头空间

            var captionText = captionLabel.AddComponent<TextMeshProUGUI>();
            captionText.fontSize = UIConstants.SETTINGS_FONT_SIZE;
            captionText.color = UIConstants.SETTINGS_TEXT_COLOR;
            captionText.alignment = TextAlignmentOptions.Left;
            captionText.verticalAlignment = VerticalAlignmentOptions.Middle;

            // ===== Arrow (可选的下拉箭头) =====
            var arrow = new GameObject("Arrow");
            arrow.transform.SetParent(dropdownObj.transform, false);

            var arrowRect = arrow.AddComponent<RectTransform>();
            arrowRect.anchorMin = new Vector2(1, 0.5f);
            arrowRect.anchorMax = new Vector2(1, 0.5f);
            arrowRect.pivot = new Vector2(1, 0.5f);
            arrowRect.sizeDelta = new Vector2(20, 20);
            arrowRect.anchoredPosition = new Vector2(-5, 0);

            var arrowText = arrow.AddComponent<TextMeshProUGUI>();
            arrowText.text = "▼";
            arrowText.fontSize = 14;
            arrowText.color = UIConstants.SETTINGS_TEXT_COLOR;
            arrowText.alignment = TextAlignmentOptions.Center;

            // ===== Template (隐藏的弹出列表) =====
            var template = new GameObject("Template");
            template.transform.SetParent(dropdownObj.transform, false);
            template.SetActive(false);

            var templateRect = template.AddComponent<RectTransform>();
            templateRect.anchorMin = new Vector2(0, 0);
            templateRect.anchorMax = new Vector2(1, 0);
            templateRect.pivot = new Vector2(0.5f, 1);
            templateRect.sizeDelta = new Vector2(0, 180);
            templateRect.anchoredPosition = new Vector2(0, -2); // 向下偏移，显示在 dropdown 下方

            var templateBg = template.AddComponent<Image>();
            templateBg.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);

            var templateScroll = template.AddComponent<ScrollRect>();
            templateScroll.horizontal = false;
            templateScroll.vertical = true;
            templateScroll.movementType = ScrollRect.MovementType.Clamped;
            templateScroll.scrollSensitivity = 20f;
            templateScroll.inertia = true;
            templateScroll.decelerationRate = 0.135f;
            templateScroll.elasticity = 0.1f;

            // Viewport (带裁剪功能)
            var viewport = new GameObject("Viewport");
            viewport.transform.SetParent(template.transform, false);

            var viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;
            viewportRect.offsetMin = new Vector2(5, 5);
            viewportRect.offsetMax = new Vector2(-5, -5);

            // 使用 RectMask2D 进行裁剪（比 Mask 更适合 UI，不需要 Image sprite）
            var rectMask = viewport.AddComponent<RectMask2D>();
            rectMask.padding = Vector4.zero;

            // Content
            var content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);

            var contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = Vector2.zero;
            contentRect.anchoredPosition = Vector2.zero;

            var contentLayout = content.AddComponent<VerticalLayoutGroup>();
            contentLayout.childControlHeight = false; // 不控制高度，让 LayoutElement 决定
            contentLayout.childControlWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.childForceExpandWidth = true;
            contentLayout.spacing = 2; // 添加小间距，防止选项紧贴

            var contentFitter = content.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            // Item (template for each option)
            var item = new GameObject("Item");
            item.transform.SetParent(content.transform, false);

            var itemRect = item.AddComponent<RectTransform>();
            itemRect.sizeDelta = new Vector2(0, 32);

            // 添加 LayoutElement 来明确指定每个选项的高度
            var itemLayout = item.AddComponent<LayoutElement>();
            itemLayout.minHeight = 32;
            itemLayout.preferredHeight = 32;

            var itemToggle = item.AddComponent<Toggle>();
            itemToggle.transition = Selectable.Transition.ColorTint;

            var itemBg = item.AddComponent<Image>();
            itemBg.color = new Color(0.2f, 0.2f, 0.2f, 0f);
            itemToggle.targetGraphic = itemBg;

            var colors = itemToggle.colors;
            colors.normalColor = new Color(1, 1, 1, 0);
            colors.highlightedColor = new Color(1, 1, 1, 0.2f);
            colors.pressedColor = new Color(1, 1, 1, 0.3f);
            colors.selectedColor = new Color(1, 1, 1, 0.2f);
            colors.disabledColor = new Color(1, 1, 1, 0.1f);
            itemToggle.colors = colors;

            // Item Label
            var itemLabel = new GameObject("Item Label");
            itemLabel.transform.SetParent(item.transform, false);

            var itemLabelRect = itemLabel.AddComponent<RectTransform>();
            itemLabelRect.anchorMin = Vector2.zero;
            itemLabelRect.anchorMax = Vector2.one;
            itemLabelRect.sizeDelta = Vector2.zero;
            itemLabelRect.offsetMin = new Vector2(10, 2);
            itemLabelRect.offsetMax = new Vector2(-10, -2);

            var itemLabelText = itemLabel.AddComponent<TextMeshProUGUI>();
            itemLabelText.fontSize = UIConstants.SETTINGS_FONT_SIZE;
            itemLabelText.color = UIConstants.SETTINGS_TEXT_COLOR;
            itemLabelText.alignment = TextAlignmentOptions.Left;
            itemLabelText.verticalAlignment = VerticalAlignmentOptions.Middle;

            // ===== Link components =====
            templateScroll.content = contentRect;
            templateScroll.viewport = viewportRect;

            dropdown.template = templateRect;
            dropdown.captionText = captionText; // 使用单独创建的 caption text
            dropdown.itemText = itemLabelText;   // 使用模板中的 item text
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
            // Use conditional assignment
            height = height < 0 ? UIConstants.SECTION_SPACING : height;

            var spacer = new GameObject("Spacer");
            spacer.transform.SetParent(_parent, false);

            var rect = spacer.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, height);

            _elements.Add(spacer);
            return this;
        }

        /// <summary>
        /// 添加描述文本
        /// </summary>
        public FormBuilder AddDescription(string textLocalizationKey)
        {
            var descObj = new GameObject("Description");
            descObj.transform.SetParent(_parent, false);

            var rect = descObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 40);

            var text = descObj.AddComponent<TextMeshProUGUI>();
            text.text = LocalizationHelper.Get(textLocalizationKey);
            text.fontSize = 16;
            text.fontStyle = FontStyles.Italic;
            text.color = new Color(0.7f, 0.7f, 0.7f);
            text.alignment = TextAlignmentOptions.Left;
            text.enableWordWrapping = true;

            var layout = descObj.AddComponent<LayoutElement>();
            layout.preferredHeight = -1;

            _elements.Add(descObj);
            return this;
        }

        /// <summary>
        /// 添加按钮
        /// </summary>
        public FormBuilder AddButton(string textLocalizationKey, System.Action onClick, 
                                      UIStyles.ButtonStyle style = UIStyles.ButtonStyle.Primary)
        {
            var button = ModButton.Create(_parent, "Button")
                .SetText(textLocalizationKey)
                .SetStyle(style)
                .OnClick(() => onClick?.Invoke())
                .Build();

            _elements.Add(button);
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

