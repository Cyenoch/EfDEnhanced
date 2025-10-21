using System;
using DG.Tweening;
using Duckov.UI.Animations;
using EfDEnhanced.Utils.UI.Animations;
using UnityEngine;

namespace EfDEnhanced.Utils.UI.Core
{
    /// <summary>
    /// Mod面板基类 - 继承游戏原生UIPanel，添加Mod特定功能
    /// 提供统一的面板管理、动画集成和生命周期管理
    /// </summary>
    public class ModPanel : UIPanel
    {
        /// <summary>
        /// 面板是否正在显示
        /// </summary>
        public bool IsShowing { get; private set; }

        /// <summary>
        /// 面板打开事件
        /// </summary>
        public event Action? OnPanelOpened;

        /// <summary>
        /// 面板关闭事件
        /// </summary>
        public event Action? OnPanelClosed;

        /// <summary>
        /// FadeGroup组件（如果有）
        /// </summary>
        protected FadeGroup FadeGroup => fadeGroup;

        /// <summary>
        /// 是否在关闭时销毁GameObject
        /// </summary>
        protected virtual bool DestroyOnClose => false;

        protected override void OnOpen()
        {
            try
            {
                base.OnOpen();
                IsShowing = true;

                ModLogger.Log("ModPanel", $"{GetType().Name} opened");

                OnPanelOpened?.Invoke();
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"ModPanel.OnOpen failed for {GetType().Name}: {ex}");
            }
        }

        protected override void OnClose()
        {
            try
            {
                base.OnClose();
                IsShowing = false;

                ModLogger.Log("ModPanel", $"{GetType().Name} closed");

                OnPanelClosed?.Invoke();

                if (DestroyOnClose && gameObject != null)
                {
                    Destroy(gameObject);
                }
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"ModPanel.OnClose failed for {GetType().Name}: {ex}");
            }
        }

        /// <summary>
        /// 打开面板（公共方法）
        /// </summary>
        public virtual void Open()
        {
            try
            {
                // 手动触发OnOpen
                OnOpen();

                // 如果有FadeGroup，显示它
                fadeGroup?.Show();
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"ModPanel.Open failed for {GetType().Name}: {ex}");
            }
        }

        /// <summary>
        /// 带动画的打开（使用DOTween）
        /// </summary>
        public virtual void OpenWithAnimation()
        {
            try
            {
                // 调用基本的Open方法
                Open();

                if (fadeGroup != null)
                {
                    // FadeGroup会自动处理动画
                    return;
                }

                // 如果没有FadeGroup，使用DOTween创建简单的弹出动画
                var canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    ModAnimations.PopupShow(transform, canvasGroup);
                }
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"ModPanel.OpenWithAnimation failed for {GetType().Name}: {ex}");
            }
        }

        /// <summary>
        /// 带动画的关闭（使用DOTween）
        /// </summary>
        public virtual void CloseWithAnimation()
        {
            try
            {
                if (fadeGroup != null)
                {
                    // FadeGroup会自动处理动画
                    Close();
                    return;
                }

                // 如果没有FadeGroup，使用DOTween创建简单的关闭动画
                var canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    var sequence = ModAnimations.PopupHide(transform, canvasGroup);
                    sequence.OnComplete(() => Close());
                }
                else
                {
                    Close();
                }
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"ModPanel.CloseWithAnimation failed for {GetType().Name}: {ex}");
                Close(); // 确保即使动画失败也能关闭
            }
        }

        /// <summary>
        /// 切换面板显示/隐藏状态
        /// </summary>
        public virtual void Toggle()
        {
            if (IsShowing)
            {
                CloseWithAnimation();
            }
            else
            {
                OpenWithAnimation();
            }
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        protected virtual void OnDestroy()
        {
            try
            {
                // 停止所有DOTween动画
                ModAnimations.KillAllTweens(gameObject);

                // 清空事件订阅
                OnPanelOpened = null;
                OnPanelClosed = null;

                ModLogger.Log("ModPanel", $"{GetType().Name} destroyed");
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"ModPanel.OnDestroy failed for {GetType().Name}: {ex}");
            }
        }

        /// <summary>
        /// 添加FadeGroup组件（如果不存在）
        /// </summary>
        protected void EnsureFadeGroup()
        {
            if (fadeGroup == null)
            {
                fadeGroup = gameObject.GetComponent<FadeGroup>();
                if (fadeGroup == null)
                {
                    fadeGroup = gameObject.AddComponent<FadeGroup>();
                    ModLogger.Log("ModPanel", $"Added FadeGroup to {GetType().Name}");
                }
            }
        }

        /// <summary>
        /// 添加CanvasGroup组件（如果不存在）
        /// </summary>
        protected CanvasGroup EnsureCanvasGroup()
        {
            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
                ModLogger.Log("ModPanel", $"Added CanvasGroup to {GetType().Name}");
            }
            return canvasGroup;
        }
    }
}

