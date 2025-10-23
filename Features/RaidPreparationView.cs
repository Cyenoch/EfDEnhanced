using System;
using Cysharp.Threading.Tasks;
using Duckov.UI;
using EfDEnhanced.Utils;
using EfDEnhanced.Utils.UI.Components;
using EfDEnhanced.Utils.UI.Constants;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace EfDEnhanced.Features;

/// <summary>
/// Raid准备检查界面
/// 使用独立的Canvas，不继承View以避免复杂的依赖
/// </summary>
public class RaidPreparationView : MonoBehaviour
{
    private static RaidPreparationView? _instance;
    private UniTaskCompletionSource<bool>? _confirmationSource;

    // UI元素
    private GameObject? _rootCanvas;
    private GameObject? _panel;
    private TextMeshProUGUI? _titleText;
    private TextMeshProUGUI? _warningText;
    private Button? _confirmButton;
    private Button? _cancelButton;

    public static RaidPreparationView? Instance => _instance;

    /// <summary>
    /// 创建并初始化View
    /// </summary>
    public static RaidPreparationView Create()
    {
        if (_instance != null)
        {
            return _instance;
        }

        // 创建根GameObject
        GameObject viewObject = new("RaidPreparationView");
        DontDestroyOnLoad(viewObject);

        RaidPreparationView view = viewObject.AddComponent<RaidPreparationView>();
        view.BuildUI();

        _instance = view;
        ModLogger.Log("RaidPreparationView", "View created successfully");

        return view;
    }

    /// <summary>
    /// 构建UI
    /// </summary>
    private void BuildUI()
    {
        // 创建Canvas
        _rootCanvas = new GameObject("Canvas");
        _rootCanvas.transform.SetParent(transform);

        Canvas canvas = _rootCanvas.AddComponent<Canvas>();
        UIStyles.ConfigureCanvas(canvas, UIConstants.RAID_CHECK_SORT_ORDER);

        CanvasScaler scaler = _rootCanvas.AddComponent<CanvasScaler>();
        UIStyles.ConfigureCanvasScaler(scaler);

        _rootCanvas.AddComponent<GraphicRaycaster>();

        // 创建半透明背景
        GameObject background = new("Background");
        background.transform.SetParent(_rootCanvas.transform, false);

        RectTransform bgRect = background.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        Image bgImage = background.AddComponent<Image>();
        bgImage.color = UIConstants.BACKGROUND_DARK;

        // 创建主面板
        _panel = new GameObject("Panel");
        _panel.transform.SetParent(_rootCanvas.transform, false);

        RectTransform panelRect = _panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(UIConstants.RAID_CHECK_PANEL_WIDTH, UIConstants.RAID_CHECK_PANEL_HEIGHT);
        panelRect.anchoredPosition = Vector2.zero;

        Image panelImage = _panel.AddComponent<Image>();
        panelImage.color = UIConstants.PANEL_BACKGROUND;

        // 创建标题
        GameObject titleObj = new("Title");
        titleObj.transform.SetParent(_panel.transform, false);

        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.sizeDelta = new Vector2(700, 80);
        titleRect.anchoredPosition = new Vector2(0, -50);

        _titleText = titleObj.AddComponent<TextMeshProUGUI>();
        _titleText.text = LocalizationHelper.Get("RaidCheck_Title");
        _titleText.fontSize = UIConstants.RAID_CHECK_TITLE_FONT_SIZE;
        _titleText.alignment = TextAlignmentOptions.Center;
        _titleText.color = new Color(1f, 0.8f, 0f);

        // 创建滚动视图容器
        GameObject scrollViewContainer = new("ScrollViewContainer");
        scrollViewContainer.transform.SetParent(_panel.transform, false);

        RectTransform scrollContainerRect = scrollViewContainer.AddComponent<RectTransform>();
        scrollContainerRect.anchorMin = new Vector2(0.5f, 0.5f);
        scrollContainerRect.anchorMax = new Vector2(0.5f, 0.5f);
        scrollContainerRect.sizeDelta = new Vector2(700, 350);
        scrollContainerRect.anchoredPosition = new Vector2(0, 20);

        // 添加背景
        Image scrollBgImage = scrollViewContainer.AddComponent<Image>();
        scrollBgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        scrollBgImage.raycastTarget = false; // 避免拦截滚轮事件

        // 创建 ScrollRect
        ScrollRect scrollRect = scrollViewContainer.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 20f;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.inertia = true;
        scrollRect.decelerationRate = 0.135f;

        // 创建 Viewport
        GameObject viewport = new("Viewport");
        viewport.transform.SetParent(scrollViewContainer.transform, false);

        RectTransform viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        viewportRect.offsetMin = new Vector2(10, 10); // 边距
        viewportRect.offsetMax = new Vector2(-10, -10);

        // 需要一个可射线的组件作为 Viewport 承载滚轮事件
        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = Color.clear; // 透明但可射线
        viewport.AddComponent<RectMask2D>(); // 使用 Mask 裁剪内容
        scrollRect.viewport = viewportRect;

        // 创建 Content 容器
        GameObject content = new("Content");
        content.transform.SetParent(viewport.transform, false);

        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f); // 左上角锚点
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f); // 从顶部开始
        contentRect.sizeDelta = new Vector2(0, 0); // 宽度匹配父级，高度自适应

        // 添加 VerticalLayoutGroup 控制子元素布局
        VerticalLayoutGroup contentLayout = content.AddComponent<VerticalLayoutGroup>();
        contentLayout.childControlHeight = true;
        contentLayout.childControlWidth = true;
        contentLayout.childForceExpandHeight = false;
        contentLayout.childForceExpandWidth = true;
        contentLayout.padding = new RectOffset(20, 20, 10, 10); // 内边距
        
        // 添加 ContentSizeFitter 自动调整高度
        ContentSizeFitter contentFitter = content.AddComponent<ContentSizeFitter>();
        contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.content = contentRect;

        // 创建警告文本
        GameObject warningObj = new("WarningText");
        warningObj.transform.SetParent(content.transform, false);

        _warningText = warningObj.AddComponent<TextMeshProUGUI>();
        _warningText.fontSize = UIConstants.RAID_CHECK_WARNING_FONT_SIZE;
        _warningText.alignment = TextAlignmentOptions.TopLeft;
        _warningText.color = Color.white;
        _warningText.enableWordWrapping = true; // 启用自动换行
        _warningText.overflowMode = TextOverflowModes.Overflow; // 允许扩展，由ContentSizeFitter控制
        
        // 添加 ContentSizeFitter 让文本高度自适应
        ContentSizeFitter textFitter = warningObj.AddComponent<ContentSizeFitter>();
        textFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        textFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        // 创建按钮容器
        GameObject buttonContainer = new("ButtonContainer");
        buttonContainer.transform.SetParent(_panel.transform, false);

        RectTransform btnContainerRect = buttonContainer.AddComponent<RectTransform>();
        btnContainerRect.anchorMin = new Vector2(0.5f, 0f);
        btnContainerRect.anchorMax = new Vector2(0.5f, 0f);
        btnContainerRect.sizeDelta = new Vector2(700, 80);
        btnContainerRect.anchoredPosition = new Vector2(0, 80);

        HorizontalLayoutGroup layout = buttonContainer.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 40;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;

        // 使用ModButton创建按钮 - 简洁且统一！
        var cancelButtonObj = ModButton.Create(buttonContainer.transform, "CancelButton")
            .SetText("RaidCheck_Cancel")
            .SetStyle(UIStyles.ButtonStyle.Danger)
            .SetSize(UIConstants.RAID_CHECK_BUTTON_WIDTH, UIConstants.RAID_CHECK_BUTTON_HEIGHT)
            .OnClick(OnCancelClicked)
            .Build();
        _cancelButton = cancelButtonObj.GetComponent<Button>();

        var confirmButtonObj = ModButton.Create(buttonContainer.transform, "ConfirmButton")
            .SetText("RaidCheck_Confirm")
            .SetStyle(UIStyles.ButtonStyle.Success)
            .SetSize(UIConstants.RAID_CHECK_BUTTON_WIDTH, UIConstants.RAID_CHECK_BUTTON_HEIGHT)
            .OnClick(OnConfirmClicked)
            .Build();
        _confirmButton = confirmButtonObj.GetComponent<Button>();

        // 确保存在 EventSystem 以接收滚轮和点击事件
        if (EventSystem.current == null)
        {
            GameObject eventSystemObj = new("EventSystem");
            eventSystemObj.transform.SetParent(_rootCanvas.transform, false);
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
        }

        // 默认隐藏
        _rootCanvas.SetActive(false);
    }

    /// <summary>
    /// 显示检查结果并等待用户确认
    /// </summary>
    public async UniTask<bool> ShowAndWaitForConfirmation(RaidCheckResult result)
    {
        if (_warningText == null || _rootCanvas == null) return true;

        try
        {

            // 设置警告文本
            _warningText.text = result.GetWarningText();

            // 显示UI并禁用玩家输入
            _rootCanvas.SetActive(true);
            
            // 等待一帧让 TextMeshPro 和 ContentSizeFitter 计算完成
            await UniTask.Yield();
            
            // 强制重建布局
            Canvas.ForceUpdateCanvases();
            RectTransform? contentRect = _warningText.rectTransform.parent as RectTransform;
            if (contentRect != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
                
                // 调试信息
                ModLogger.Log("RaidPreparationView", $"Content size: {contentRect.sizeDelta}, Text preferred height: {_warningText.preferredHeight}");
            }
            
            InputManager.DisableInput(gameObject);

            ModLogger.Log("RaidPreparationView", "View opened, waiting for user input");

            // 创建异步完成源
            _confirmationSource = new UniTaskCompletionSource<bool>();

            // 等待用户选择
            bool userChoice = await _confirmationSource.Task;

            ModLogger.Log("RaidPreparationView", $"User choice: {(userChoice ? "Continue" : "Cancel")}");

            return userChoice;
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"RaidPreparationView.ShowAndWaitForConfirmation failed: {ex}");
            return false;
        }
        finally
        {
            // 关闭UI并恢复输入
            _rootCanvas.SetActive(false);
            InputManager.ActiveInput(gameObject);
            _confirmationSource = null;
        }
    }

    /// <summary>
    /// 确认按钮点击
    /// </summary>
    private void OnConfirmClicked()
    {
        ModLogger.Log("RaidPreparationView", "Confirm button clicked");
        _confirmationSource?.TrySetResult(true);
    }

    /// <summary>
    /// 取消按钮点击
    /// </summary>
    private void OnCancelClicked()
    {
        ModLogger.Log("RaidPreparationView", "Cancel button clicked");
        _confirmationSource?.TrySetResult(false);
    }

    /// <summary>
    /// 每帧检查输入（ESC和Tab键）
    /// </summary>
    private void Update()
    {
        if (_confirmationSource == null || _rootCanvas == null || !_rootCanvas.activeSelf)
        {
            return;
        }

        // 检查 ESC 键
        if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Escape))
        {
            ModLogger.Log("RaidPreparationView", "ESC key pressed");
            _confirmationSource?.TrySetResult(false);
        }

        // 检查 Tab 键（游戏常用的关闭UI快捷键）
        if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Tab))
        {
            ModLogger.Log("RaidPreparationView", "Tab key pressed");
            _confirmationSource?.TrySetResult(false);
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}

