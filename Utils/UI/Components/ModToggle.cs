using EfDEnhanced.Utils.Settings;
using EfDEnhanced.Utils.UI.Constants;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;

namespace EfDEnhanced.Utils.UI.Components
{
    /// <summary>
    /// Mod标准化Toggle组件
    /// 支持自动绑定到BoolSettingsEntry，统一样式
    /// </summary>
    public class ModToggle : MonoBehaviour
    {
        private Toggle? _toggle;
        private Image? _background;
        private Image? _checkmark;
        private Outline? _outline;
        private TextMeshProUGUI? _label;
        private BoolSettingsEntry? _boundSetting;
        private string? _labelLocalizationKey;
        private bool _isLocalizationSubscribed = false;

        /// <summary>
        /// Toggle组件
        /// </summary>
        public Toggle Toggle => _toggle!;

        /// <summary>
        /// 当前值
        /// </summary>
        public bool Value
        {
            get => _toggle != null && _toggle.isOn;
            set
            {
                if (_toggle != null)
                {
                    _toggle.isOn = value;
                }
            }
        }

        /// <summary>
        /// 创建ModToggle实例
        /// </summary>
        public static ModToggle Create(Transform parent, string name = "ModToggle")
        {
            // 创建容器
            GameObject containerObj = new(name);
            containerObj.transform.SetParent(parent, false);

            RectTransform containerRect = containerObj.AddComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(0, 0);

            // Add ContentSizeFitter to auto-size based on content
            ContentSizeFitter sizeFitter = containerObj.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            // 添加水平布局
            HorizontalLayoutGroup layout = containerObj.AddComponent<HorizontalLayoutGroup>();
            UIStyles.ConfigureHorizontalLayout(layout, 10, TextAnchor.MiddleLeft);
            layout.padding = new RectOffset(0, 0, 0, 0); // Reduce vertical padding to minimize spacing

            // 创建复选框
            GameObject checkboxObj = new("Checkbox");
            checkboxObj.transform.SetParent(containerObj.transform, false);

            RectTransform checkboxRect = checkboxObj.AddComponent<RectTransform>();
            checkboxRect.sizeDelta = new Vector2(UIConstants.CHECKBOX_SIZE, UIConstants.CHECKBOX_SIZE);

            LayoutElement checkboxLayout = checkboxObj.AddComponent<LayoutElement>();
            checkboxLayout.minWidth = UIConstants.CHECKBOX_SIZE;
            checkboxLayout.minHeight = UIConstants.CHECKBOX_SIZE;
            checkboxLayout.preferredWidth = UIConstants.CHECKBOX_SIZE;
            checkboxLayout.preferredHeight = UIConstants.CHECKBOX_SIZE;

            Image bgImage = checkboxObj.AddComponent<Image>();
            bgImage.color = UIConstants.CHECKBOX_BACKGROUND_UNCHECKED;

            Outline border = checkboxObj.AddComponent<Outline>();
            border.effectColor = UIConstants.CHECKBOX_BORDER_UNCHECKED;
            border.effectDistance = new Vector2(1.5f, -1.5f);

            // 创建勾选标记
            GameObject checkmarkObj = new("Checkmark");
            checkmarkObj.transform.SetParent(checkboxObj.transform, false);

            RectTransform checkmarkRect = checkmarkObj.AddComponent<RectTransform>();
            checkmarkRect.anchorMin = new Vector2(0.2f, 0.2f);
            checkmarkRect.anchorMax = new Vector2(0.8f, 0.8f);
            checkmarkRect.offsetMin = Vector2.zero;
            checkmarkRect.offsetMax = Vector2.zero;

            Image checkmarkImage = checkmarkObj.AddComponent<Image>();
            checkmarkImage.color = UIConstants.CHECKBOX_CHECKMARK;

            // 添加Toggle组件
            Toggle toggle = checkboxObj.AddComponent<Toggle>();
            toggle.targetGraphic = bgImage;
            toggle.graphic = checkmarkImage;

            // 创建标签
            GameObject labelObj = new("Label");
            labelObj.transform.SetParent(containerObj.transform, false);

            LayoutElement labelLayout = labelObj.AddComponent<LayoutElement>();
            labelLayout.flexibleWidth = 1;
            labelLayout.minHeight = 0;

            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.fontSize = UIConstants.SETTINGS_LABEL_FONT_SIZE;
            labelText.color = UIConstants.SETTINGS_LABEL_COLOR;
            labelText.alignment = TextAlignmentOptions.MidlineLeft;
            labelText.enableWordWrapping = false;

            // 添加ModToggle组件
            ModToggle modToggle = containerObj.AddComponent<ModToggle>();
            modToggle._toggle = toggle;
            modToggle._background = bgImage;
            modToggle._checkmark = checkmarkImage;
            modToggle._outline = border;
            modToggle._label = labelText;

            // 设置初始视觉状态
            modToggle.UpdateVisuals();

            // 监听值变化
            toggle.onValueChanged.AddListener((value) =>
            {
                modToggle.UpdateVisuals();
            });

            return modToggle;
        }

        /// <summary>
        /// 设置标签文本（支持本地化）
        /// </summary>
        public ModToggle SetLabel(string localizationKey)
        {
            if (_label != null)
            {
                _labelLocalizationKey = localizationKey;
                _label.text = LocalizationHelper.Get(localizationKey);

                // Subscribe to language changes only once
                if (!_isLocalizationSubscribed)
                {
                    LocalizationHelper.OnLanguageChanged += OnLanguageChanged;
                    _isLocalizationSubscribed = true;
                }
            }
            return this;
        }

        /// <summary>
        /// 设置标签文本（直接文本）
        /// </summary>
        public ModToggle SetLabelDirect(string text)
        {
            if (_label != null)
            {
                _label.text = text;
            }
            return this;
        }

        /// <summary>
        /// 绑定到BoolSettingsEntry（自动双向同步）
        /// </summary>
        public ModToggle BindToSetting(BoolSettingsEntry setting)
        {
            if (setting == null) return this;

            _boundSetting = setting;

            // 设置初始值（不触发事件）
            if (_toggle != null)
            {
                _toggle.SetIsOnWithoutNotify(setting.Value);
                UpdateVisuals();
            }

            // 监听Toggle变化，更新Setting
            _toggle?.onValueChanged.AddListener((value) =>
            {
                if (_boundSetting != null)
                {
                    _boundSetting.Value = value;
                }
            });

            // 监听Setting变化，更新Toggle
            setting.ValueChanged += (sender, args) =>
            {
                if (_toggle != null)
                {
                    _toggle.SetIsOnWithoutNotify(args.NewValue);
                    UpdateVisuals();
                }
            };

            return this;
        }

        /// <summary>
        /// 添加值变化监听
        /// </summary>
        public ModToggle OnValueChanged(UnityAction<bool> callback)
        {
            _toggle?.onValueChanged.AddListener(callback);
            return this;
        }

        /// <summary>
        /// 设置是否可交互
        /// </summary>
        public ModToggle SetInteractable(bool interactable)
        {
            if (_toggle != null)
            {
                _toggle.interactable = interactable;
            }
            return this;
        }

        /// <summary>
        /// 更新视觉状态
        /// </summary>
        private void UpdateVisuals()
        {
            if (_toggle == null || _checkmark == null || _background == null || _outline == null)
                return;

            bool isOn = _toggle.isOn;

            _checkmark.enabled = isOn;
            _checkmark.gameObject.SetActive(isOn);
            _background.color = isOn ? UIConstants.CHECKBOX_BACKGROUND_CHECKED : UIConstants.CHECKBOX_BACKGROUND_UNCHECKED;
            _outline.effectColor = isOn ? UIConstants.CHECKBOX_BORDER_CHECKED : UIConstants.CHECKBOX_BORDER_UNCHECKED;
        }

        /// <summary>
        /// 完成构建，返回GameObject
        /// </summary>
        public GameObject Build()
        {
            return gameObject;
        }

        /// <summary>
        /// Handle language changes by updating the label text
        /// </summary>
        private void OnLanguageChanged(SystemLanguage newLanguage)
        {
            try
            {
                if (_label != null && !string.IsNullOrEmpty(_labelLocalizationKey))
                {
                    _label.text = LocalizationHelper.Get(_labelLocalizationKey);
                }
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"ModToggle.OnLanguageChanged failed: {ex}");
            }
        }

        private void OnDestroy()
        {
            _toggle?.onValueChanged.RemoveAllListeners();
            LocalizationHelper.OnLanguageChanged -= OnLanguageChanged;
        }
    }
}

