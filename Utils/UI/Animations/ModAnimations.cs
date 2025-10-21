using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using EfDEnhanced.Utils.UI.Constants;

namespace EfDEnhanced.Utils.UI.Animations
{
    /// <summary>
    /// Mod UI动画预设 - 封装常用的DOTween动画
    /// 利用游戏已加载的DOTween库提供流畅的动画效果
    /// </summary>
    public static class ModAnimations
    {
        #region Button Animations
        
        /// <summary>
        /// 按钮点击缩放动画
        /// </summary>
        public static Tween ButtonPressScale(Transform button)
        {
            return button.DOScale(UIConstants.BUTTON_SCALE_FACTOR, UIConstants.BUTTON_SCALE_DURATION)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true); // 使用unscaled time，不受时间缩放影响
        }

        /// <summary>
        /// 按钮释放恢复动画
        /// </summary>
        public static Tween ButtonReleaseScale(Transform button)
        {
            return button.DOScale(1f, UIConstants.BUTTON_SCALE_DURATION)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
        }

        /// <summary>
        /// 按钮悬停放大动画
        /// </summary>
        public static Tween ButtonHoverScale(Transform button, float scaleFactor = 1.05f)
        {
            return button.DOScale(scaleFactor, 0.15f)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
        }

        #endregion

        #region Fade Animations
        
        /// <summary>
        /// 淡入动画（CanvasGroup）
        /// </summary>
        public static Tween FadeIn(CanvasGroup canvasGroup, float duration = -1f)
        {
            if (duration < 0) duration = UIConstants.FADE_DURATION;
            
            return canvasGroup.DOFade(1f, duration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
        }

        /// <summary>
        /// 淡出动画（CanvasGroup）
        /// </summary>
        public static Tween FadeOut(CanvasGroup canvasGroup, float duration = -1f)
        {
            if (duration < 0) duration = UIConstants.FADE_DURATION;
            
            return canvasGroup.DOFade(0f, duration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
        }

        /// <summary>
        /// 淡入动画（Image）
        /// </summary>
        public static Tween FadeIn(Image image, float duration = -1f)
        {
            if (duration < 0) duration = UIConstants.FADE_DURATION;
            
            return image.DOFade(1f, duration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
        }

        /// <summary>
        /// 淡出动画（Image）
        /// </summary>
        public static Tween FadeOut(Image image, float duration = -1f)
        {
            if (duration < 0) duration = UIConstants.FADE_DURATION;
            
            return image.DOFade(0f, duration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
        }

        #endregion

        #region Scale Animations
        
        /// <summary>
        /// 弹性放大出现动画
        /// </summary>
        public static Tween ScaleIn(Transform transform, float duration = 0.3f)
        {
            transform.localScale = Vector3.zero;
            return transform.DOScale(1f, duration)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }

        /// <summary>
        /// 缩小消失动画
        /// </summary>
        public static Tween ScaleOut(Transform transform, float duration = 0.3f)
        {
            return transform.DOScale(0f, duration)
                .SetEase(Ease.InBack)
                .SetUpdate(true);
        }

        /// <summary>
        /// 脉冲动画（循环）
        /// </summary>
        public static Tween Pulse(Transform transform, float scaleFactor = 1.1f, float duration = 0.5f)
        {
            return transform.DOScale(scaleFactor, duration)
                .SetEase(Ease.InOutQuad)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true);
        }

        #endregion

        #region Slide Animations
        
        /// <summary>
        /// 从右侧滑入
        /// </summary>
        public static Tween SlideInFromRight(RectTransform rectTransform, float duration = 0.3f)
        {
            Vector2 originalPos = rectTransform.anchoredPosition;
            rectTransform.anchoredPosition = new Vector2(Screen.width, originalPos.y);
            
            return rectTransform.DOAnchorPos(originalPos, duration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
        }

        /// <summary>
        /// 滑出到右侧
        /// </summary>
        public static Tween SlideOutToRight(RectTransform rectTransform, float duration = 0.3f)
        {
            return rectTransform.DOAnchorPosX(Screen.width, duration)
                .SetEase(Ease.InQuad)
                .SetUpdate(true);
        }

        /// <summary>
        /// 从左侧滑入
        /// </summary>
        public static Tween SlideInFromLeft(RectTransform rectTransform, float duration = 0.3f)
        {
            Vector2 originalPos = rectTransform.anchoredPosition;
            rectTransform.anchoredPosition = new Vector2(-Screen.width, originalPos.y);
            
            return rectTransform.DOAnchorPos(originalPos, duration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
        }

        /// <summary>
        /// 滑出到左侧
        /// </summary>
        public static Tween SlideOutToLeft(RectTransform rectTransform, float duration = 0.3f)
        {
            return rectTransform.DOAnchorPosX(-Screen.width, duration)
                .SetEase(Ease.InQuad)
                .SetUpdate(true);
        }

        #endregion

        #region Color Animations
        
        /// <summary>
        /// 颜色渐变动画（Image）
        /// </summary>
        public static Tween ColorTo(Image image, Color targetColor, float duration = 0.3f)
        {
            return image.DOColor(targetColor, duration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
        }

        /// <summary>
        /// 颜色渐变动画（Graphic）
        /// </summary>
        public static Tween ColorTo(Graphic graphic, Color targetColor, float duration = 0.3f)
        {
            return graphic.DOColor(targetColor, duration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
        }

        /// <summary>
        /// 颜色闪烁动画
        /// </summary>
        public static Tween ColorBlink(Graphic graphic, Color targetColor, float duration = 0.5f, int loops = -1)
        {
            Color originalColor = graphic.color;
            return graphic.DOColor(targetColor, duration)
                .SetEase(Ease.InOutQuad)
                .SetLoops(loops, LoopType.Yoyo)
                .SetUpdate(true)
                .OnKill(() => graphic.color = originalColor);
        }

        #endregion

        #region Rotation Animations
        
        /// <summary>
        /// 旋转动画
        /// </summary>
        public static Tween Rotate(Transform transform, float angle, float duration = 0.5f)
        {
            return transform.DORotate(new Vector3(0, 0, angle), duration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
        }

        /// <summary>
        /// 持续旋转动画（循环）
        /// </summary>
        public static Tween RotateLoop(Transform transform, float duration = 1f)
        {
            return transform.DORotate(new Vector3(0, 0, 360), duration, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart)
                .SetUpdate(true);
        }

        #endregion

        #region Sequence Animations
        
        /// <summary>
        /// 弹出窗口动画组合（缩放+淡入）
        /// </summary>
        public static Sequence PopupShow(Transform transform, CanvasGroup canvasGroup)
        {
            transform.localScale = Vector3.zero;
            canvasGroup.alpha = 0f;
            
            Sequence sequence = DOTween.Sequence();
            sequence.Append(transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
            sequence.Join(canvasGroup.DOFade(1f, 0.2f));
            sequence.SetUpdate(true);
            
            return sequence;
        }

        /// <summary>
        /// 关闭窗口动画组合（缩放+淡出）
        /// </summary>
        public static Sequence PopupHide(Transform transform, CanvasGroup canvasGroup)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(canvasGroup.DOFade(0f, 0.15f));
            sequence.Join(transform.DOScale(0.8f, 0.15f).SetEase(Ease.InQuad));
            sequence.SetUpdate(true);
            
            return sequence;
        }

        /// <summary>
        /// 震动动画（用于强调或错误提示）
        /// </summary>
        public static Tween Shake(Transform transform, float strength = 20f, float duration = 0.5f)
        {
            return transform.DOShakePosition(duration, strength, 10, 90, false, true)
                .SetUpdate(true);
        }

        #endregion

        #region Utility Methods
        
        /// <summary>
        /// 停止并清理指定对象上的所有DOTween动画
        /// </summary>
        public static void KillAllTweens(GameObject gameObject)
        {
            if (gameObject != null)
            {
                DOTween.Kill(gameObject.transform);
            }
        }

        /// <summary>
        /// 暂停指定对象上的所有DOTween动画
        /// </summary>
        public static void PauseAllTweens(GameObject gameObject)
        {
            if (gameObject != null)
            {
                DOTween.Pause(gameObject.transform);
            }
        }

        /// <summary>
        /// 恢复指定对象上的所有DOTween动画
        /// </summary>
        public static void ResumeAllTweens(GameObject gameObject)
        {
            if (gameObject != null)
            {
                DOTween.Play(gameObject.transform);
            }
        }

        #endregion
    }
}

