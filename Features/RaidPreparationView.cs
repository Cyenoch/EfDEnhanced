using System;
using Cysharp.Threading.Tasks;
using Duckov.UI;
using EfDEnhanced.Utils;
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
    private GameObject _rootCanvas;
    private GameObject _panel;
    private TextMeshProUGUI _titleText;
    private TextMeshProUGUI _warningText;
    private Button _confirmButton;
    private Button _cancelButton;
    
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
        GameObject viewObject = new GameObject("RaidPreparationView");
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
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000; // 确保在最上层
        
        CanvasScaler scaler = _rootCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        _rootCanvas.AddComponent<GraphicRaycaster>();
        
        // 创建半透明背景
        GameObject background = new GameObject("Background");
        background.transform.SetParent(_rootCanvas.transform, false);
        
        RectTransform bgRect = background.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.7f);
        
        // 创建主面板
        _panel = new GameObject("Panel");
        _panel.transform.SetParent(_rootCanvas.transform, false);
        
        RectTransform panelRect = _panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(800, 600);
        panelRect.anchoredPosition = Vector2.zero;
        
        Image panelImage = _panel.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        
        // 创建标题
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(_panel.transform, false);
        
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.sizeDelta = new Vector2(700, 80);
        titleRect.anchoredPosition = new Vector2(0, -50);
        
        _titleText = titleObj.AddComponent<TextMeshProUGUI>();
        _titleText.text = LocalizationHelper.Get("RaidCheck_Title");
        _titleText.fontSize = 48;
        _titleText.alignment = TextAlignmentOptions.Center;
        _titleText.color = new Color(1f, 0.8f, 0f);
        
        // 创建警告文本容器（带边距）
        GameObject warningContainer = new GameObject("WarningContainer");
        warningContainer.transform.SetParent(_panel.transform, false);
        
        RectTransform containerRect = warningContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.sizeDelta = new Vector2(700, 350);
        containerRect.anchoredPosition = new Vector2(0, 20);
        
        // 创建警告文本
        GameObject warningObj = new GameObject("Warning");
        warningObj.transform.SetParent(warningContainer.transform, false);
        
        RectTransform warningRect = warningObj.AddComponent<RectTransform>();
        warningRect.anchorMin = Vector2.zero;
        warningRect.anchorMax = Vector2.one;
        warningRect.offsetMin = new Vector2(30, 10); // 左下边距
        warningRect.offsetMax = new Vector2(-30, -10); // 右上边距
        
        _warningText = warningObj.AddComponent<TextMeshProUGUI>();
        _warningText.fontSize = 28; // 稍微缩小字体
        _warningText.alignment = TextAlignmentOptions.TopLeft;
        _warningText.color = Color.white;
        _warningText.enableWordWrapping = true; // 启用自动换行
        _warningText.overflowMode = TextOverflowModes.Overflow; // 允许内容溢出但会换行
        
        // 创建按钮容器
        GameObject buttonContainer = new GameObject("ButtonContainer");
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
    
        // 创建取消按钮
        _cancelButton = CreateButton(buttonContainer, LocalizationHelper.Get("RaidCheck_Cancel"), new Color(0.5f, 0.2f, 0.2f));
        _cancelButton.onClick.AddListener(OnCancelClicked);
        
        // 创建确认按钮
        _confirmButton = CreateButton(buttonContainer, LocalizationHelper.Get("RaidCheck_Confirm"), new Color(0.2f, 0.5f, 0.2f));
        _confirmButton.onClick.AddListener(OnConfirmClicked);
        
        // 默认隐藏
        _rootCanvas.SetActive(false);
    }
    
    /// <summary>
    /// 创建按钮
    /// </summary>
    private Button CreateButton(GameObject parent, string text, Color color)
    {
        GameObject btnObj = new GameObject($"Button_{text}");
        btnObj.transform.SetParent(parent.transform, false);
        
        RectTransform btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(280, 60);
        
        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = color;
        
        Button button = btnObj.AddComponent<Button>();
        
        // 按钮文本
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.fontSize = 32;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = Color.white;
        
        return button;
    }
    
    /// <summary>
    /// 显示检查结果并等待用户确认
    /// </summary>
    public async UniTask<bool> ShowAndWaitForConfirmation(RaidCheckResult result)
    {
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
        if (_confirmationSource == null || !_rootCanvas.activeSelf)
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

