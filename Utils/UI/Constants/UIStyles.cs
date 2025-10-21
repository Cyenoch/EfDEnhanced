using UnityEngine;
using UnityEngine.UI;

namespace EfDEnhanced.Utils.UI.Constants
{
    /// <summary>
    /// UI样式预设 - 提供常用的样式配置
    /// </summary>
    public static class UIStyles
    {
        /// <summary>
        /// 按钮样式枚举
        /// </summary>
        public enum ButtonStyle
        {
            Primary,
            Secondary,
            Success,
            Danger,
            Ghost
        }

        /// <summary>
        /// 获取按钮样式对应的颜色
        /// </summary>
        public static Color GetButtonColor(ButtonStyle style)
        {
            return style switch
            {
                ButtonStyle.Primary => UIConstants.BUTTON_PRIMARY,
                ButtonStyle.Success => UIConstants.BUTTON_SUCCESS,
                ButtonStyle.Danger => UIConstants.BUTTON_DANGER,
                ButtonStyle.Secondary => UIConstants.BUTTON_SECONDARY,
                ButtonStyle.Ghost => new Color(0f, 0f, 0f, 0.1f),
                _ => UIConstants.BUTTON_SECONDARY
            };
        }

        /// <summary>
        /// 应用标准阴影到文本组件
        /// </summary>
        public static void ApplyStandardTextShadow(GameObject textObject, bool isTitle = false)
        {
            var shadow = textObject.GetComponent<LeTai.TrueShadow.TrueShadow>();
            if (shadow == null)
            {
                shadow = textObject.AddComponent<LeTai.TrueShadow.TrueShadow>();
            }

            if (isTitle)
            {
                shadow.Size = UIConstants.TITLE_SHADOW_SIZE;
                shadow.Spread = UIConstants.TITLE_SHADOW_SPREAD;
                shadow.OffsetDistance = UIConstants.TITLE_SHADOW_DISTANCE;
            }
            else
            {
                shadow.Size = UIConstants.TEXT_SHADOW_SIZE;
                shadow.Spread = UIConstants.TEXT_SHADOW_SPREAD;
                shadow.OffsetDistance = UIConstants.TEXT_SHADOW_DISTANCE;
            }

            shadow.OffsetAngle = -90f;
            shadow.Color = UIConstants.SHADOW_COLOR;
        }

        /// <summary>
        /// 配置标准的VerticalLayoutGroup
        /// </summary>
        public static void ConfigureVerticalLayout(VerticalLayoutGroup layout, int spacing, RectOffset? padding = null)
        {
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.spacing = spacing;
            layout.padding = padding ?? new RectOffset(0, 0, 0, 0);
        }

        /// <summary>
        /// 配置标准的HorizontalLayoutGroup
        /// </summary>
        public static void ConfigureHorizontalLayout(HorizontalLayoutGroup layout, int spacing, TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childAlignment = alignment;
            layout.spacing = spacing;
        }

        /// <summary>
        /// 创建标准的Canvas配置
        /// </summary>
        public static void ConfigureCanvas(Canvas canvas, int sortOrder, RenderMode renderMode = RenderMode.ScreenSpaceOverlay)
        {
            canvas.renderMode = renderMode;
            canvas.sortingOrder = sortOrder;
        }

        /// <summary>
        /// 创建标准的CanvasScaler配置
        /// </summary>
        public static void ConfigureCanvasScaler(CanvasScaler scaler)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            // 使用游戏本体的自适应逻辑
            float screenAspect = (float)Screen.width / Screen.height;
            float refAspect = 1920f / 1080f; // 16:9
            scaler.matchWidthOrHeight = (screenAspect > refAspect) ? 1f : 0f;
        }
    }
}

