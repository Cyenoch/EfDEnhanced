using System;
using Cysharp.Threading.Tasks;
using Duckov.UI;
using EfDEnhanced.Utils;
using EfDEnhanced.Utils.UI.Components;
using EfDEnhanced.Utils.UI.Constants;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        // 创建警告文本容器（带边距）
        GameObject warningContainer = new("WarningContainer");
        warningContainer.transform.SetParent(_panel.transform, false);

        RectTransform containerRect = warningContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.sizeDelta = new Vector2(700, 350);
        containerRect.anchoredPosition = new Vector2(0, 20);

        // 创建警告文本
        GameObject warningObj = new("Warning");
        warningObj.transform.SetParent(warningContainer.transform, false);

        RectTransform warningRect = warningObj.AddComponent<RectTransform>();
        warningRect.anchorMin = Vector2.zero;
        warningRect.anchorMax = Vector2.one;
        warningRect.offsetMin = new Vector2(30, 10); // 左下边距
        warningRect.offsetMax = new Vector2(-30, -10); // 右上边距

        _warningText = warningObj.AddComponent<TextMeshProUGUI>();
        _warningText.fontSize = UIConstants.RAID_CHECK_WARNING_FONT_SIZE;
        _warningText.alignment = TextAlignmentOptions.TopLeft;
        _warningText.color = Color.white;
        _warningText.enableWordWrapping = true; // 启用自动换行
        _warningText.overflowMode = TextOverflowModes.Overflow; // 允许内容溢出但会换行

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

