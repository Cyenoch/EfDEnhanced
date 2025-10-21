using System;
using System.Collections.Generic;
using System.Linq;
using Duckov.Quests;
using Duckov.UI;
using EfDEnhanced.Utils;
using EfDEnhanced.Utils.Settings;
using EfDEnhanced.Utils.UI.Constants;
using LeTai.TrueShadow;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EfDEnhanced.Features;

/// <summary>
/// 活跃任务追踪器 - 在Raid中显示进行中的任务
/// </summary>
public class ActiveQuestTracker : MonoBehaviour
{
    private static ActiveQuestTracker? _instance;
    
    // UI元素
    private GameObject? _rootCanvas;
    private GameObject? _questPanel;
    private GameObject? _questListContainer;
    private readonly List<QuestEntryUI> _questEntries = new List<QuestEntryUI>();
    
    // 配置
    private bool _isActive;
    
    public static ActiveQuestTracker? Instance => _instance;
    
    /// <summary>
    /// 创建追踪器实例
    /// </summary>
    public static ActiveQuestTracker Create()
    {
        if (_instance != null)
        {
            return _instance;
        }
        
        GameObject trackerObject = new GameObject("ActiveQuestTracker");
        DontDestroyOnLoad(trackerObject);
        
        ActiveQuestTracker tracker = trackerObject.AddComponent<ActiveQuestTracker>();
        tracker.BuildUI();
        
        _instance = tracker;
        ModLogger.Log("QuestTracker", "Quest tracker created successfully");
        
        return tracker;
    }
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
        
        // 取消订阅所有事件
        UnregisterEvents();
    }
    
    /// <summary>
    /// 构建UI
    /// </summary>
    private void BuildUI()
    {
        try
        {
            // 创建Canvas
            _rootCanvas = new GameObject("QuestTrackerCanvas");
            _rootCanvas.transform.SetParent(transform);
            
            Canvas canvas = _rootCanvas.AddComponent<Canvas>();
            UIStyles.ConfigureCanvas(canvas, UIConstants.QUEST_TRACKER_SORT_ORDER);
            
            CanvasScaler scaler = _rootCanvas.AddComponent<CanvasScaler>();
            UIStyles.ConfigureCanvasScaler(scaler);
            
            // 创建主面板（左上角）
            _questPanel = new GameObject("QuestPanel");
            _questPanel.transform.SetParent(_rootCanvas.transform, false);
            
            RectTransform panelRect = _questPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 1); // 左上角锚点
            panelRect.anchorMax = new Vector2(0, 1); // 左上角锚点
            panelRect.pivot = new Vector2(0, 1); // 轴心点在左上角
            
            // 计算高度：屏幕高度的指定比例
            float maxHeight = Screen.height * UIConstants.QUEST_PANEL_SCREEN_HEIGHT_RATIO;
            
            // 设置尺寸
            panelRect.sizeDelta = new Vector2(UIConstants.QUEST_PANEL_WIDTH, maxHeight);
            
            // 设置位置：左上角向右和向下偏移（负值）
            panelRect.anchoredPosition = new Vector2(-10, -10);
            
            // 直接创建任务列表容器（不使用ScrollRect）
            _questListContainer = new GameObject("QuestListContainer");
            _questListContainer.transform.SetParent(_questPanel.transform, false);
            
            RectTransform contentRect = _questListContainer.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.anchoredPosition = new Vector2(0, -5); // 距离顶部5px
            contentRect.sizeDelta = new Vector2(-10, 0); // 左右各留5px边距
            
            VerticalLayoutGroup layoutGroup = _questListContainer.AddComponent<VerticalLayoutGroup>();
            UIStyles.ConfigureVerticalLayout(layoutGroup, UIConstants.QUEST_ENTRY_SPACING, 
                new RectOffset(0, 0, 0, 0));
            
            ContentSizeFitter sizeFitter = _questListContainer.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            // 默认隐藏
            _rootCanvas.SetActive(false);
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestTracker.BuildUI failed: {ex}");
        }
    }
    
    /// <summary>
    /// 启用追踪器（进入Raid时调用）
    /// </summary>
    public void Enable()
    {
        try
        {
            // 检查设置是否启用任务追踪器
            if (!ModSettings.EnableQuestTracker.Value)
            {
                return;
            }

            if (_isActive || _rootCanvas == null)
            {
                return;
            }

            _isActive = true;

            // 检查初始UI状态 - 如果有菜单打开则隐藏追踪器
            bool anyMenuOpen = View.ActiveView != null;
            _rootCanvas.SetActive(!anyMenuOpen);

            // 应用设置中的位置和缩放
            ApplySettingsToUI();

            // 订阅所有事件
            RegisterEvents();

            // 订阅设置变化事件以实时更新UI
            ModSettings.TrackerPositionX.ValueChanged += OnTrackerPositionChanged;
            ModSettings.TrackerPositionY.ValueChanged += OnTrackerPositionChanged;
            ModSettings.TrackerScale.ValueChanged += OnTrackerScaleChanged;
            ModSettings.TrackerShowDescription.ValueChanged += OnShowDescriptionChanged;

            RefreshQuestList();
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestTracker.Enable failed: {ex}");
        }
    }
    
    /// <summary>
    /// 禁用追踪器（离开Raid时调用）
    /// </summary>
    public void Disable()
    {
        try
        {
            if (!_isActive)
            {
                return;
            }

            _isActive = false;
            _rootCanvas?.SetActive(false);
            ClearQuestList();

            // 取消订阅所有事件
            UnregisterEvents();

            // 取消订阅设置变化事件
            ModSettings.TrackerPositionX.ValueChanged -= OnTrackerPositionChanged;
            ModSettings.TrackerPositionY.ValueChanged -= OnTrackerPositionChanged;
            ModSettings.TrackerScale.ValueChanged -= OnTrackerScaleChanged;
            ModSettings.TrackerShowDescription.ValueChanged -= OnShowDescriptionChanged;
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestTracker.Disable failed: {ex}");
        }
    }
    
    /// <summary>
    /// 注册事件监听
    /// </summary>
    private void RegisterEvents()
    {
        try
        {
            // 追踪状态变化（手动勾选/取消追踪）
            QuestTrackingManager.OnTrackingChanged += OnQuestTrackingChanged;

            // 任务列表变化（新任务、任务完成等）
            QuestManager.onQuestListsChanged += OnQuestListsChanged;

            // 任务状态变化
            Quest.onQuestStatusChanged += OnQuestStatusChanged;

            // 任务完成
            Quest.onQuestCompleted += OnQuestCompleted;

            // 子任务完成
            QuestManager.OnTaskFinishedEvent += OnTaskFinished;

            // UI状态变化（菜单打开/关闭时隐藏/显示追踪器）
            View.OnActiveViewChanged += OnUIStateChanged;
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestTracker.RegisterEvents failed: {ex}");
        }
    }
    
    /// <summary>
    /// 取消注册事件监听
    /// </summary>
    private void UnregisterEvents()
    {
        try
        {
            QuestTrackingManager.OnTrackingChanged -= OnQuestTrackingChanged;
            QuestManager.onQuestListsChanged -= OnQuestListsChanged;
            Quest.onQuestStatusChanged -= OnQuestStatusChanged;
            Quest.onQuestCompleted -= OnQuestCompleted;
            QuestManager.OnTaskFinishedEvent -= OnTaskFinished;
            View.OnActiveViewChanged -= OnUIStateChanged;
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestTracker.UnregisterEvents failed: {ex}");
        }
    }

    /// <summary>
    /// UI状态变化回调（菜单打开/关闭时调用）
    /// </summary>
    private void OnUIStateChanged()
    {
        try
        {
            if (!_isActive || _rootCanvas == null)
            {
                return;
            }

            // 检查是否有任何菜单打开
            bool anyMenuOpen = View.ActiveView != null;

            // 当有菜单打开时隐藏追踪器，没有菜单时显示追踪器
            bool shouldShow = !anyMenuOpen;
            if (_rootCanvas.activeSelf != shouldShow)
            {
                _rootCanvas.SetActive(shouldShow);
            }
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestTracker.OnUIStateChanged failed: {ex}");
        }
    }

    /// <summary>
    /// 刷新任务列表
    /// </summary>
    private void RefreshQuestList()
    {
        try
        {
            if (QuestManager.Instance == null)
            {
                return;
            }
            
            var activeQuests = QuestManager.Instance.ActiveQuests;
            if (activeQuests == null || activeQuests.Count == 0)
            {
                ClearQuestList();
                return;
            }
            
            // 只显示未完成且被追踪的任务
            var trackedIncompleteQuests = activeQuests
                .Where(q => q != null && !q.Complete && QuestTrackingManager.IsQuestTracked(q.ID))
                .ToList();
            
            if (trackedIncompleteQuests.Count == 0)
            {
                ClearQuestList();
                return;
            }
            
            // 更新现有条目或创建新条目
            UpdateQuestEntries(trackedIncompleteQuests);
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestTracker.RefreshQuestList failed: {ex}");
        }
    }
    
    /// <summary>
    /// 更新任务条目
    /// </summary>
    private void UpdateQuestEntries(List<Quest> quests)
    {
        try
        {
            // 移除不再存在的任务
            for (int i = _questEntries.Count - 1; i >= 0; i--)
            {
                var entry = _questEntries[i];
                if (!quests.Any(q => q.ID == entry.QuestID))
                {
                    Destroy(entry.GameObject);
                    _questEntries.RemoveAt(i);
                }
            }
            
            // 更新或创建任务条目
            foreach (var quest in quests)
            {
                var existingEntry = _questEntries.FirstOrDefault(e => e.QuestID == quest.ID);
                if (existingEntry != null)
                {
                    existingEntry.UpdateDisplay(quest);
                }
                else
                {
                    CreateQuestEntry(quest);
                }
            }
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestTracker.UpdateQuestEntries failed: {ex}");
        }
    }
    
    /// <summary>
    /// 创建任务条目UI
    /// </summary>
    private void CreateQuestEntry(Quest quest)
    {
        try
        {
            if (_questListContainer == null)
            {
                return;
            }
            
            GameObject entryObj = new GameObject($"QuestEntry_{quest.ID}");
            entryObj.transform.SetParent(_questListContainer.transform, false);
            
            RectTransform entryRect = entryObj.AddComponent<RectTransform>();
            entryRect.sizeDelta = new Vector2(0, 0); // 由LayoutGroup控制
            
            // 不添加背景，完全透明
            // 只添加一条分隔线作为边框
            Outline entryOutline = entryObj.AddComponent<Outline>();
            entryOutline.effectColor = new Color(0.6f, 0.6f, 0.6f, 0.3f);
            entryOutline.effectDistance = new Vector2(0, -1); // 只在底部显示线
            
            // 垂直布局
            VerticalLayoutGroup entryLayout = entryObj.AddComponent<VerticalLayoutGroup>();
            entryLayout.childForceExpandWidth = true;
            entryLayout.childForceExpandHeight = false;
            entryLayout.childControlWidth = true;
            entryLayout.childControlHeight = true;
            entryLayout.spacing = 3; // 增加内部间距
            entryLayout.padding = new RectOffset(8, 8, 6, 6); // 增加内边距
            
            ContentSizeFitter entryFitter = entryObj.AddComponent<ContentSizeFitter>();
            entryFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            // 任务标题行（标题 + 进度）
            GameObject titleRowObj = new GameObject("TitleRow");
            titleRowObj.transform.SetParent(entryObj.transform, false);
            
            RectTransform titleRowRect = titleRowObj.AddComponent<RectTransform>();
            
            HorizontalLayoutGroup titleRowLayout = titleRowObj.AddComponent<HorizontalLayoutGroup>();
            titleRowLayout.childForceExpandWidth = false;
            titleRowLayout.childForceExpandHeight = false;
            titleRowLayout.childControlWidth = true; // 改为true，让LayoutGroup控制子元素宽度
            titleRowLayout.childControlHeight = true;
            titleRowLayout.childAlignment = TextAnchor.MiddleLeft;
            titleRowLayout.spacing = 5;
            
            ContentSizeFitter titleRowFitter = titleRowObj.AddComponent<ContentSizeFitter>();
            titleRowFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained; // 保持不变，由父容器限制
            titleRowFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            // 任务标题（左侧）
            GameObject titleObj = new GameObject("QuestTitle");
            titleObj.transform.SetParent(titleRowObj.transform, false);
            
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = quest.DisplayName;
            titleText.fontSize = UIConstants.QUEST_TITLE_FONT_SIZE;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = UIConstants.QUEST_TITLE_COLOR;
            titleText.enableWordWrapping = false;
            titleText.overflowMode = TextOverflowModes.Ellipsis;
            
            // 使用TrueShadow获得更好的阴影效果
            UIStyles.ApplyStandardTextShadow(titleObj, isTitle: true);
            
            LayoutElement titleLayout = titleObj.AddComponent<LayoutElement>();
            titleLayout.flexibleWidth = 1; // 占据剩余空间
            titleLayout.preferredWidth = 200; // 设置首选宽度
            titleLayout.preferredHeight = -1;
            
            // 任务进度（右侧）
            GameObject progressBadgeObj = new GameObject("ProgressBadge");
            progressBadgeObj.transform.SetParent(titleRowObj.transform, false);
            
            TextMeshProUGUI progressBadgeText = progressBadgeObj.AddComponent<TextMeshProUGUI>();
            int finishedTaskCount = quest.Tasks?.Count(t => t != null && t.IsFinished()) ?? 0;
            int totalTaskCount = quest.Tasks?.Count ?? 0;
            progressBadgeText.text = $"{finishedTaskCount}/{totalTaskCount}";
            progressBadgeText.fontSize = UIConstants.QUEST_PROGRESS_FONT_SIZE;
            progressBadgeText.fontStyle = FontStyles.Bold;
            progressBadgeText.color = UIConstants.QUEST_PROGRESS_COLOR;
            progressBadgeText.alignment = TextAlignmentOptions.MidlineRight;
            
            // 使用TrueShadow
            UIStyles.ApplyStandardTextShadow(progressBadgeObj, isTitle: false);
            
            LayoutElement progressBadgeLayout = progressBadgeObj.AddComponent<LayoutElement>();
            progressBadgeLayout.minWidth = 40;
            progressBadgeLayout.preferredHeight = -1;
            
            // 任务描述（简介）- 根据设置决定是否显示
            if (ModSettings.TrackerShowDescription.Value && !string.IsNullOrEmpty(quest.Description))
            {
                GameObject descObj = new GameObject("QuestDescription");
                descObj.transform.SetParent(entryObj.transform, false);

                TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
                descText.text = quest.Description;
                descText.fontSize = UIConstants.QUEST_DESC_FONT_SIZE;
                descText.fontStyle = FontStyles.Italic;
                descText.color = UIConstants.QUEST_DESC_COLOR;
                descText.enableWordWrapping = true;
                descText.overflowMode = TextOverflowModes.Truncate;

                // 使用TrueShadow
                UIStyles.ApplyStandardTextShadow(descObj, isTitle: false);

                LayoutElement descLayout = descObj.AddComponent<LayoutElement>();
                descLayout.preferredHeight = -1;
                descLayout.flexibleWidth = 1; // 允许自适应宽度
            }
            
            // 任务进度容器
            GameObject progressContainer = new GameObject("ProgressContainer");
            progressContainer.transform.SetParent(entryObj.transform, false);
            
            VerticalLayoutGroup taskListLayout = progressContainer.AddComponent<VerticalLayoutGroup>();
            UIStyles.ConfigureVerticalLayout(taskListLayout, UIConstants.QUEST_TASK_SPACING);
            
            ContentSizeFitter progressFitter = progressContainer.AddComponent<ContentSizeFitter>();
            progressFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            // 创建UI对象
            var questEntry = new QuestEntryUI
            {
                QuestID = quest.ID,
                GameObject = entryObj,
                TitleText = titleText,
                ProgressBadgeText = progressBadgeText,
                ProgressContainer = progressContainer
            };
            
            _questEntries.Add(questEntry);
            
            // 初始化显示
            questEntry.UpdateDisplay(quest);
            
            // 强制刷新布局
            Canvas.ForceUpdateCanvases();
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestTracker.CreateQuestEntry failed: {ex}");
        }
    }
    
    /// <summary>
    /// 清空任务列表
    /// </summary>
    private void ClearQuestList()
    {
        try
        {
            foreach (var entry in _questEntries)
            {
                if (entry.GameObject != null)
                {
                    Destroy(entry.GameObject);
                }
            }
            _questEntries.Clear();
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestTracker.ClearQuestList failed: {ex}");
        }
    }
    
    /// <summary>
    /// 任务追踪状态变化回调（手动勾选/取消追踪）
    /// </summary>
    private void OnQuestTrackingChanged(int questId, bool isTracked)
    {
        try
        {
            if (!_isActive)
            {
                return;
            }
            
            ModLogger.Log("QuestTracker", $"Quest {questId} tracking changed to {isTracked}");
            RefreshQuestList();
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestTracker.OnQuestTrackingChanged failed: {ex}");
        }
    }
    
    /// <summary>
    /// 任务列表变化回调（新任务激活、任务完成等）
    /// </summary>
    private void OnQuestListsChanged(QuestManager manager)
    {
        try
        {
            if (!_isActive)
            {
                return;
            }
            
            ModLogger.Log("QuestTracker", "Quest lists changed");
            RefreshQuestList();
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestTracker.OnQuestListsChanged failed: {ex}");
        }
    }
    
    /// <summary>
    /// 任务状态变化回调
    /// </summary>
    private void OnQuestStatusChanged(Quest quest)
    {
        try
        {
            if (!_isActive)
            {
                return;
            }
            
            // 只更新已追踪的任务
            if (QuestTrackingManager.IsQuestTracked(quest.ID))
            {
                var entry = _questEntries.FirstOrDefault(e => e.QuestID == quest.ID);
                if (entry != null)
                {
                    ModLogger.Log("QuestTracker", $"Quest {quest.ID} status changed, updating display");
                    entry.UpdateDisplay(quest);
                }
            }
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestTracker.OnQuestStatusChanged failed: {ex}");
        }
    }
    
    /// <summary>
    /// 任务完成回调
    /// </summary>
    private void OnQuestCompleted(Quest quest)
    {
        try
        {
            if (!_isActive)
            {
                return;
            }
            
            ModLogger.Log("QuestTracker", $"Quest {quest.ID} completed");
            RefreshQuestList();
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestTracker.OnQuestCompleted failed: {ex}");
        }
    }
    
    /// <summary>
    /// 子任务完成回调
    /// </summary>
    private void OnTaskFinished(Quest quest, Task task)
    {
        try
        {
            if (!_isActive)
            {
                return;
            }

            // 只更新已追踪的任务
            if (QuestTrackingManager.IsQuestTracked(quest.ID))
            {
                var entry = _questEntries.FirstOrDefault(e => e.QuestID == quest.ID);
                if (entry != null)
                {
                    ModLogger.Log("QuestTracker", $"Task in quest {quest.ID} finished, updating display");
                    entry.UpdateDisplay(quest);
                }
            }
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestTracker.OnTaskFinished failed: {ex}");
        }
    }

    /// <summary>
    /// 应用设置到UI（位置、缩放）
    /// </summary>
    private void ApplySettingsToUI()
    {
        try
        {
            if (_questPanel == null) return;

            RectTransform panelRect = _questPanel.GetComponent<RectTransform>();
            if (panelRect == null) return;

            // 应用位置设置
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            // 将归一化坐标(0-1)转换为屏幕坐标
            // Since we're using right-top anchored position, we use negative X (right to left)
            // but positive Y (top to bottom) would be incorrect - it should be negative too
            float xPos = ModSettings.TrackerPositionX.Value * screenWidth;
            float yPos = -ModSettings.TrackerPositionY.Value * screenHeight;

            ModLogger.Log("QuestTracker", $"Position calculation: X={ModSettings.TrackerPositionX.Value} * {screenWidth} = {xPos}, Y={ModSettings.TrackerPositionY.Value} * {screenHeight} = {yPos}");

            panelRect.anchoredPosition = new Vector2(xPos, yPos);

            // 应用缩放设置
            float scale = ModSettings.TrackerScale.Value;
            panelRect.localScale = new Vector3(scale, scale, 1f);

            ModLogger.Log("QuestTracker", $"Applied settings - Position: ({xPos}, {yPos}), Scale: {scale}");
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestTracker.ApplySettingsToUI failed: {ex}");
        }
    }

    /// <summary>
    /// 位置设置变化回调
    /// </summary>
    private void OnTrackerPositionChanged(object? sender, SettingsValueChangedEventArgs<float> args)
    {
        if (_isActive)
        {
            ApplySettingsToUI();
        }
    }

    /// <summary>
    /// 缩放设置变化回调
    /// </summary>
    private void OnTrackerScaleChanged(object? sender, SettingsValueChangedEventArgs<float> args)
    {
        if (_isActive)
        {
            ApplySettingsToUI();
        }
    }

    /// <summary>
    /// 显示描述设置变化回调
    /// </summary>
    private void OnShowDescriptionChanged(object? sender, SettingsValueChangedEventArgs<bool> args)
    {
        if (_isActive)
        {
            // 刷新任务列表以更新描述显示
            RefreshQuestList();
        }
    }
}

/// <summary>
/// 任务条目UI数据
/// </summary>
public class QuestEntryUI
{
    public int QuestID { get; set; }
    public GameObject? GameObject { get; set; }
    public TextMeshProUGUI? TitleText { get; set; }
    public TextMeshProUGUI? ProgressBadgeText { get; set; }
    public GameObject? ProgressContainer { get; set; }
    
    private readonly List<TaskUIElement> _taskElements = new List<TaskUIElement>();
    private string _lastProgressText = "";
    
    /// <summary>
    /// 更新显示
    /// </summary>
    public void UpdateDisplay(Quest quest)
    {
        try
        {
            if (ProgressContainer == null)
            {
                return;
            }
            
            // 更新进度徽章 (只在变化时更新)
            if (ProgressBadgeText != null && quest.Tasks != null)
            {
                int finishedCount = quest.Tasks.Count(t => t != null && t.IsFinished());
                string newProgressText = $"{finishedCount}/{quest.Tasks.Count}";
                
                if (_lastProgressText != newProgressText)
                {
                    ProgressBadgeText.text = newProgressText;
                    _lastProgressText = newProgressText;
                }
            }
            
            // 更新任务进度 (复用现有UI元素)
            if (quest.Tasks != null && quest.Tasks.Count > 0)
            {
                int taskIndex = 0;
                foreach (var task in quest.Tasks)
                {
                    if (task == null)
                    {
                        continue;
                    }
                    
                    bool isFinished = task.IsFinished();
                    
                    // 格式化任务描述（使用本地化图标）
                    string statusIcon = isFinished 
                        ? LocalizationHelper.Get("QuestTracker_TaskComplete") 
                        : LocalizationHelper.Get("QuestTracker_TaskPending");
                    string taskDesc = task.Description;
                    
                    // 显示额外描述（如果有）
                    if (task.ExtraDescriptsions != null && task.ExtraDescriptsions.Length > 0)
                    {
                        taskDesc += " " + string.Join(" ", task.ExtraDescriptsions);
                    }
                    
                    string fullText = $"  {statusIcon} {taskDesc}";
                    Color taskColor = isFinished ? UIConstants.QUEST_COMPLETE_COLOR : UIConstants.QUEST_INCOMPLETE_COLOR;
                    
                    // 复用或创建UI元素
                    if (taskIndex < _taskElements.Count)
                    {
                        // 复用现有元素 (只在内容变化时更新)
                        var element = _taskElements[taskIndex];
                        if (element.LastText != fullText)
                        {
                            element.TextComponent.text = fullText;
                            element.LastText = fullText;
                        }
                        if (element.LastColor != taskColor)
                        {
                            element.TextComponent.color = taskColor;
                            element.LastColor = taskColor;
                        }
                        if (!element.GameObject.activeSelf)
                        {
                            element.GameObject.SetActive(true);
                        }
                    }
                    else
                    {
                        // 创建新元素
                        GameObject taskObj = new GameObject($"Task_{task.ID}");
                        taskObj.transform.SetParent(ProgressContainer.transform, false);
                        
                        TextMeshProUGUI taskText = taskObj.AddComponent<TextMeshProUGUI>();
                        taskText.text = fullText;
                        taskText.fontSize = UIConstants.QUEST_TASK_FONT_SIZE;
                        taskText.color = taskColor;
                        taskText.enableWordWrapping = true;
                        taskText.overflowMode = TextOverflowModes.Truncate; // 防止超出边界
                        
                        // 使用TrueShadow
                        UIStyles.ApplyStandardTextShadow(taskObj, isTitle: false);
                        
                        LayoutElement taskLayout = taskObj.AddComponent<LayoutElement>();
                        taskLayout.preferredHeight = -1;
                        taskLayout.flexibleWidth = 1; // 允许自适应宽度但不超出容器
                        
                        _taskElements.Add(new TaskUIElement
                        {
                            GameObject = taskObj,
                            TextComponent = taskText,
                            LastText = fullText,
                            LastColor = taskColor
                        });
                    }
                    
                    taskIndex++;
                }
                
                // 隐藏多余的UI元素 (而不是销毁)
                for (int i = taskIndex; i < _taskElements.Count; i++)
                {
                    if (_taskElements[i].GameObject.activeSelf)
                    {
                        _taskElements[i].GameObject.SetActive(false);
                    }
                }
            }
            else
            {
                // 没有任务时隐藏所有元素
                foreach (var element in _taskElements)
                {
                    if (element.GameObject.activeSelf)
                    {
                        element.GameObject.SetActive(false);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestEntryUI.UpdateDisplay failed: {ex}");
        }
    }
}

/// <summary>
/// 任务UI元素缓存
/// </summary>
public class TaskUIElement
{
    public GameObject GameObject { get; set; } = null!;
    public TextMeshProUGUI TextComponent { get; set; } = null!;
    public string LastText { get; set; } = "";
    public Color LastColor { get; set; }
}

