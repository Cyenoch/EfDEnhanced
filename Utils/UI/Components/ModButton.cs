using System;
using DG.Tweening;
using Duckov.UI.Animations;
using EfDEnhanced.Utils.UI.Animations;
using EfDEnhanced.Utils.UI.Constants;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EfDEnhanced.Utils.UI.Components
{
    /// <summary>
    /// Mod标准化按钮组件
    /// 集成ButtonAnimation、音效、DOTween动画和统一样式
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ModButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        private Button? _button;
        private Image? _buttonImage;
        private TextMeshProUGUI? _text;
        private ButtonAnimation? _animation;
        private RectTransform? _rectTransform;
        
        private Tween? _currentTween;
        private UIStyles.ButtonStyle _currentStyle = UIStyles.ButtonStyle.Secondary;

        /// <summary>
        /// 按钮的Button组件
        /// </summary>
        public Button Button => _button;

        /// <summary>
        /// 按钮文本组件
        /// </summary>
        public TextMeshProUGUI TextComponent => _text;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _buttonImage = GetComponent<Image>();
            _rectTransform = GetComponent<RectTransform>();
            
            // 如果没有ButtonAnimation，添加一个
            _animation = GetComponent<ButtonAnimation>();
        }

        /// <summary>
        /// 创建ModButton实例
        /// </summary>
        public static ModButton Create(Transform parent, string name = "ModButton")
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent, false);

            // 添加RectTransform
            RectTransform rect = buttonObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(UIConstants.BUTTON_WIDTH, UIConstants.BUTTON_HEIGHT);

            // 添加Image作为背景
            Image image = buttonObj.AddComponent<Image>();
            image.color = UIConstants.BUTTON_SECONDARY;

            // 添加Button组件
            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = image;

            // 添加ButtonAnimation（游戏原生）
            ButtonAnimation buttonAnim = buttonObj.AddComponent<ButtonAnimation>();

            // 创建文本子对象
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.fontSize = UIConstants.BUTTON_TEXT_FONT_SIZE;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            text.fontStyle = FontStyles.Bold;

            // 添加ModButton组件
            ModButton modButton = buttonObj.AddComponent<ModButton>();
            modButton._text = text;
            modButton._buttonImage = image;

            return modButton;
        }

        /// <summary>
        /// 设置按钮文本（支持本地化）
        /// </summary>
        public ModButton SetText(string localizationKey)
        {
            if (_text != null)
            {
                _text.text = LocalizationHelper.Get(localizationKey);
            }
            return this;
        }

        /// <summary>
        /// 设置按钮文本（直接文本）
        /// </summary>
        public ModButton SetTextDirect(string text)
        {
            if (_text != null)
            {
                _text.text = text;
            }
            return this;
        }

        /// <summary>
        /// 设置按钮样式
        /// </summary>
        public ModButton SetStyle(UIStyles.ButtonStyle style)
        {
            _currentStyle = style;
            if (_buttonImage != null)
            {
                _buttonImage.color = UIStyles.GetButtonColor(style);
            }
            return this;
        }

        /// <summary>
        /// 设置按钮大小
        /// </summary>
        public ModButton SetSize(float width, float height)
        {
            if (_rectTransform != null)
            {
                _rectTransform.sizeDelta = new Vector2(width, height);
            }
            return this;
        }

        /// <summary>
        /// 设置按钮位置
        /// </summary>
        public ModButton SetPosition(Vector2 anchoredPosition)
        {
            if (_rectTransform != null)
            {
                _rectTransform.anchoredPosition = anchoredPosition;
            }
            return this;
        }

        /// <summary>
        /// 添加点击事件监听
        /// </summary>
        public ModButton OnClick(UnityAction action)
        {
            if (_button != null)
            {
                _button.onClick.AddListener(action);
            }
            return this;
        }

        /// <summary>
        /// 移除所有点击事件监听
        /// </summary>
        public ModButton ClearClickListeners()
        {
            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
            }
            return this;
        }

        /// <summary>
        /// 启用DOTween缩放动画
        /// </summary>
        public ModButton WithDOTweenScale()
        {
            // 标记启用，在IPointer事件中使用
            return this;
        }

        /// <summary>
        /// 设置按钮是否可交互
        /// </summary>
        public ModButton SetInteractable(bool interactable)
        {
            if (_button != null)
            {
                _button.interactable = interactable;
            }
            return this;
        }

        /// <summary>
        /// 完成构建，返回GameObject
        /// </summary>
        public GameObject Build()
        {
            return gameObject;
        }

        #region Pointer Events (DOTween动画)

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_button.interactable) return;

            _currentTween?.Kill();
            _currentTween = ModAnimations.ButtonHoverScale(transform, 1.05f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_button.interactable) return;

            _currentTween?.Kill();
            _currentTween = ModAnimations.ButtonReleaseScale(transform);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_button.interactable) return;

            _currentTween?.Kill();
            _currentTween = ModAnimations.ButtonPressScale(transform);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_button.interactable) return;

            _currentTween?.Kill();
            _currentTween = ModAnimations.ButtonReleaseScale(transform);
        }

        #endregion

        private void OnDestroy()
        {
            _currentTween?.Kill();
            _button?.onClick.RemoveAllListeners();
        }
    }
}

