using System;
using System.Collections.Generic;
using System.Linq;
using Duckov.Economy;
using Duckov.Quests;
using Duckov.Quests.Tasks;
using Duckov.Scenes;
using Duckov.UI;
using EfDEnhanced.Utils;
using EfDEnhanced.Utils.Settings;
using EfDEnhanced.Utils.UI.Constants;
using ItemStatsSystem;
using LeTai.TrueShadow;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace EfDEnhanced.Features;

/// <summary>
/// 活跃任务追踪器 - 在Raid中显示进行中的任务
/// </summary>
public class ActiveQuestTracker : MonoBehaviour
{

    // UI元素
    private GameObject? _rootCanvas;
    private GameObject? _questPanel;
    private GameObject? _questListContainer;
    private GameObject? _helpTextObject;
    private TextMeshProUGUI? _helpTextComponent;
    private readonly List<QuestEntryUI> _questEntries = [];

    // 配置
    private bool _isActive;
    private bool _isCollapsed = false; // 局部折叠状态，不影响设置

    private void Awake()
    {
        BuildUI();
        LevelManager.OnAfterLevelInitialized += OnAfterLevelInitialized;
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
        ModSettings.EnableQuestTracker.ValueChanged += OnEnableQuestTrackerChanged;
        QuestTrackingManager.OnTrackingChanged += OnQuestTrackingChanged;
    }

    private void OnDestroy()
    {
        LevelManager.OnAfterLevelInitialized -= OnAfterLevelInitialized;
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        ModSettings.EnableQuestTracker.ValueChanged -= OnEnableQuestTrackerChanged;
        QuestTrackingManager.OnTrackingChanged -= OnQuestTrackingChanged;
        // 取消订阅所有事件
        UnregisterEvents();
    }

    private void Update()
    {
        // 只在激活状态下监听快捷键
        if (!_isActive) return;

        // 检查折叠/展开快捷键
        if (Input.GetKeyDown(ModSettings.TrackerToggleHotkey.Value))
        {
            ToggleCollapse();
        }
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

            // 给主面板添加布局组件来管理子元素
            VerticalLayoutGroup panelLayoutGroup = _questPanel.AddComponent<VerticalLayoutGroup>();
            UIStyles.ConfigureVerticalLayout(panelLayoutGroup, 5, new RectOffset(0, 0, 0, 0));

            ContentSizeFitter panelSizeFitter = _questPanel.AddComponent<ContentSizeFitter>();
            panelSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            panelSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            // 创建帮助文本（始终显示在顶部）
            _helpTextObject = new GameObject("HelpText");
            _helpTextObject.transform.SetParent(_questPanel.transform, false);

            RectTransform helpTextRect = _helpTextObject.AddComponent<RectTransform>();
            helpTextRect.anchorMin = Vector2.zero;
            helpTextRect.anchorMax = Vector2.one;
            helpTextRect.offsetMin = Vector2.zero;
            helpTextRect.offsetMax = Vector2.zero;

            _helpTextComponent = _helpTextObject.AddComponent<TextMeshProUGUI>();
            _helpTextComponent.fontSize = UIConstants.QUEST_DESC_FONT_SIZE;
            _helpTextComponent.fontStyle = FontStyles.Italic;
            _helpTextComponent.color = new Color(0.7f, 0.7f, 0.7f, 0.8f); // 灰色半透明
            _helpTextComponent.alignment = TextAlignmentOptions.Center;
            _helpTextComponent.enableWordWrapping = true;

            // 使用TrueShadow
            UIStyles.ApplyStandardTextShadow(_helpTextObject, isTitle: false);

            LayoutElement helpTextLayout = _helpTextObject.AddComponent<LayoutElement>();
            helpTextLayout.preferredHeight = -1;

            // 根据是否首次使用来设置提示文本的初始可见性
            UpdateHelpTextVisibility();
            RefreshHelpTextLocalizedText();

            // 直接创建任务列表容器（不使用ScrollRect）
            _questListContainer = new GameObject("QuestListContainer");
            _questListContainer.transform.SetParent(_questPanel.transform, false);

            RectTransform contentRect = _questListContainer.AddComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

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
    /// 更新帮助文本的可见性（首次使用后隐藏）
    /// </summary>
    private void UpdateHelpTextVisibility()
    {
        try
        {
            if (_helpTextObject != null && _questPanel != null)
            {
                // 如果用户已经使用过快捷键，则隐藏提示文本
                bool shouldShow = !ModSettings.TrackerHotkeyUsed.Value;
                _helpTextObject.SetActive(shouldShow);

                ModLogger.Log("QuestTracker", $"Help text visibility: {shouldShow}, ignoreLayout: {!shouldShow}");
            }
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestTracker.UpdateHelpTextVisibility failed: {ex}");
        }
    }

    private void RefreshHelpTextLocalizedText()
    {
        try
        {
            if (_helpTextComponent != null)
            {
                _helpTextComponent.text = LocalizationHelper.Get("QuestTracker_HelpText");
            }
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestTracker.RefreshHelpTextLocalizedText failed: {ex}");
        }
    }

    /// <summary>
    /// 切换折叠/展开状态
    /// </summary>
    private void ToggleCollapse()
    {
        try
        {
            _isCollapsed = !_isCollapsed;

            _questListContainer?.SetActive(!_isCollapsed);

            if (!ModSettings.TrackerHotkeyUsed.Value)
            {
                ModSettings.TrackerHotkeyUsed.Value = true;
            }

            ModLogger.Log("QuestTracker", $"Tracker {(_isCollapsed ? "collapsed" : "expanded")}");
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestTracker.ToggleCollapse failed: {ex}");
        }
    }

    /// <summary>
    /// 启用追踪器（进入Raid时调用）
    /// </summary>
    public void Enable()
    {
        try
        {
            if (_isActive || _rootCanvas == null) return;

            _isActive = true;

            // 检查初始UI状态 - 如果有菜单打开则隐藏追踪器
            bool anyMenuOpen = View.ActiveView != null;
            _rootCanvas.SetActive(!anyMenuOpen);

            // 应用设置中的位置和缩放
            ApplySettingsToUI();

            // 订阅所有事件
            RegisterEvents();

            // 延迟刷新任务列表，等待玩家背包/装备完全加载
            // 这样 SubmitItems 任务的持有数量才能正确显示
            StartCoroutine(DelayedRefreshQuestList());
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestTracker.Enable failed: {ex}");
        }
    }

    /// <summary>
    /// 延迟刷新任务列表（用于进图后等待物品加载）
    /// </summary>
    private System.Collections.IEnumerator DelayedRefreshQuestList()
    {
        yield return new WaitForSeconds(0.5f);
        RefreshQuestList();
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
            _isCollapsed = false; // 重置折叠状态
            _rootCanvas?.SetActive(false);
            ClearQuestList();

            // 取消订阅所有事件
            UnregisterEvents();
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
            QuestManager.onQuestListsChanged += OnQuestListsChanged;
            Quest.onQuestStatusChanged += OnQuestStatusChanged;
            Quest.onQuestCompleted += OnQuestCompleted;
            QuestManager.OnTaskFinishedEvent += OnTaskFinished;
            View.OnActiveViewChanged += OnUIStateChanged;
            CharacterMainControl.OnMainCharacterInventoryChangedEvent = (Action<CharacterMainControl, Inventory, int>)Delegate.Combine(
                CharacterMainControl.OnMainCharacterInventoryChangedEvent,
                new Action<CharacterMainControl, Inventory, int>(OnInventoryChanged));
            PlayerStorage.OnPlayerStorageChange += OnStorageChanged;

            // 金钱变化（用于更新提交金钱类任务的可交互状态）
            EconomyManager.OnMoneyChanged += OnMoneyChanged;

            LocalizationHelper.OnLanguageChanged += OnLanguageChanged;

            // 订阅设置变化事件以实时更新UI
            ModSettings.TrackerPositionX.ValueChanged += OnTrackerPositionChanged;
            ModSettings.TrackerPositionY.ValueChanged += OnTrackerPositionChanged;
            ModSettings.TrackerScale.ValueChanged += OnTrackerScaleChanged;
            ModSettings.TrackerShowDescription.ValueChanged += OnShowDescriptionChanged;
            ModSettings.TrackerFilterByMap.ValueChanged += OnFilterByMapChanged;
            ModSettings.TrackerHotkeyUsed.ValueChanged += OnHotkeyUsedChanged;
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
            QuestManager.onQuestListsChanged -= OnQuestListsChanged;
            Quest.onQuestStatusChanged -= OnQuestStatusChanged;
            Quest.onQuestCompleted -= OnQuestCompleted;
            QuestManager.OnTaskFinishedEvent -= OnTaskFinished;
            View.OnActiveViewChanged -= OnUIStateChanged;
            CharacterMainControl.OnMainCharacterInventoryChangedEvent = (Action<CharacterMainControl, Inventory, int>)Delegate.Remove(
                CharacterMainControl.OnMainCharacterInventoryChangedEvent,
                new Action<CharacterMainControl, Inventory, int>(OnInventoryChanged));
            PlayerStorage.OnPlayerStorageChange -= OnStorageChanged;

            // 取消订阅金钱变化事件
            EconomyManager.OnMoneyChanged -= OnMoneyChanged;

            LocalizationHelper.OnLanguageChanged -= OnLanguageChanged;

            ModSettings.TrackerPositionX.ValueChanged -= OnTrackerPositionChanged;
            ModSettings.TrackerPositionY.ValueChanged -= OnTrackerPositionChanged;
            ModSettings.TrackerScale.ValueChanged -= OnTrackerScaleChanged;
            ModSettings.TrackerShowDescription.ValueChanged -= OnShowDescriptionChanged;
            ModSettings.TrackerFilterByMap.ValueChanged -= OnFilterByMapChanged;
            ModSettings.TrackerHotkeyUsed.ValueChanged -= OnHotkeyUsedChanged;
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestTracker.UnregisterEvents failed: {ex}");
        }
    }

    private void OnEnableQuestTrackerChanged(object? sender, SettingsValueChangedEventArgs<bool> args)
    {
        // 只在 Raid 中响应设置变化
        if (LevelManager.Instance == null || !LevelManager.Instance.IsRaidMap)
        {
            return;
        }

        if (args.NewValue)
        {
            Enable();
        }
        else
        {
            Disable();
        }
    }

    private void OnLanguageChanged(SystemLanguage newLanguage)
    {
        try
        {
            RefreshHelpTextLocalizedText();
            if (_isActive) RefreshQuestList();
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestTracker.OnLanguageChanged failed: {ex}");
        }
    }


    private void OnAfterLevelInitialized()
    {
        if (!ModSettings.EnableQuestTracker.Value) return;
        var isRaidMap = LevelManager.Instance == null ? false : LevelManager.Instance.IsRaidMap;
        ModLogger.Log("QuestTracker", $"OnAfterLevelInitialized: isRaidMap: {isRaidMap}");
        if (isRaidMap)
        {
            Enable();
        }
        else
        {
            Disable();
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

            // 根据设置决定是否按地图过滤
            if (ModSettings.TrackerFilterByMap.Value)
            {
                trackedIncompleteQuests = FilterQuestsByCurrentMap(trackedIncompleteQuests);
            }

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
    /// 根据当前地图过滤任务
    /// 只显示当前地图的任务，以及没有地图限制的任务
    /// </summary>
    private List<Quest> FilterQuestsByCurrentMap(List<Quest> quests)
    {
        try
        {
            // 获取当前地图ID
            string? currentMapID = GetCurrentMapID();

            if (string.IsNullOrEmpty(currentMapID))
            {
                ModLogger.Log("QuestTracker", "Cannot determine current map, showing all quests");
                return quests;
            }

            ModLogger.Log("QuestTracker", $"Filtering quests for current map: {currentMapID}");

            // 过滤任务
            var filteredQuests = quests.Where(q =>
            {
                if (q == null) return false;

                var questSceneInfo = q.RequireSceneInfo;

                // 如果任务没有指定场景要求，显示（可以在任意地图完成）
                if (questSceneInfo == null || string.IsNullOrEmpty(questSceneInfo.ID))
                {
                    ModLogger.Log("QuestTracker", $"Quest '{q.DisplayName}' has no map requirement - showing");
                    return true;
                }

                // 如果任务的场景ID匹配当前地图，显示
                if (questSceneInfo.ID == currentMapID)
                {
                    ModLogger.Log("QuestTracker", $"Quest '{q.DisplayName}' matches current map '{currentMapID}' - showing");
                    return true;
                }
                else
                {
                    List<string> taskSceneIDs = [];
                    // 可能子task有不同地图要求
                    foreach (var task in q.Tasks)
                    {
                        if (task is QuestTask_ReachLocation rl)
                        {
                            var locationField = typeof(QuestTask_ReachLocation).GetField("location", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            if (locationField?.GetValue(rl) is MultiSceneLocation location)
                            {
                                var sceneIDField = typeof(MultiSceneLocation).GetField("sceneID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                if (sceneIDField?.GetValue(location) is string sceneID && !string.IsNullOrEmpty(sceneID))
                                {
                                    taskSceneIDs.Add(sceneID);
                                }
                            }
                        }
                        if (task is QuestTask_Evacuate evacuate)
                        {
                            var requireSceneIDField = typeof(QuestTask_Evacuate).GetField("requireSceneID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            if (requireSceneIDField?.GetValue(evacuate) is string sceneID && !string.IsNullOrEmpty(sceneID))
                            {
                                taskSceneIDs.Add(sceneID);
                            }
                        }
                        if (task is QuestTask_KillCount killCount)
                        {
                            var requireSceneIDField = typeof(QuestTask_KillCount).GetField("requireSceneID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            if (requireSceneIDField?.GetValue(killCount) is string sceneID && !string.IsNullOrEmpty(sceneID))
                            {
                                taskSceneIDs.Add(sceneID);
                            }
                        }
                    }
                    if (taskSceneIDs.Count > 0 && taskSceneIDs.Any(x => x == currentMapID))
                    {
                        return true;
                    }
                    else
                    {
                        ModLogger.Log("QuestTracker", $"Quest '{q.DisplayName}' requires map '{questSceneInfo.ID}' but current is '{currentMapID}' - hiding");
                    }
                }
                return false;
            }).ToList();

            ModLogger.Log("QuestTracker", $"Filtered {quests.Count} quests to {filteredQuests.Count} for map '{currentMapID}'");

            return filteredQuests;
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestTracker.FilterQuestsByCurrentMap failed: {ex}");
            return quests; // 出错时返回所有任务
        }
    }

    /// <summary>
    /// 获取当前地图ID
    /// </summary>
    private string? GetCurrentMapID()
    {
        try
        {
            // 方法1：从 MultiSceneCore 获取主场景ID
            if (MultiSceneCore.Instance != null)
            {
                string? mainSceneID = MultiSceneCore.MainSceneID;
                if (!string.IsNullOrEmpty(mainSceneID))
                {
                    ModLogger.Log("QuestTracker", $"Current map ID from MultiSceneCore: {mainSceneID}");
                    return mainSceneID;
                }
            }

            ModLogger.LogWarning("QuestTracker", "Could not determine current map ID");
            return null;
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestTracker.GetCurrentMapID failed: {ex}");
            return null;
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
                    entry.Cleanup(); // 清理事件订阅
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

            GameObject entryObj = new($"QuestEntry_{quest.ID}");
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
            GameObject titleRowObj = new("TitleRow");
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
            GameObject titleObj = new("QuestTitle");
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
            GameObject progressBadgeObj = new("ProgressBadge");
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

            // 任务描述（简介）- 总是创建，但根据设置控制显示/隐藏
            GameObject? descObj = null;
            if (!string.IsNullOrEmpty(quest.Description))
            {
                descObj = new GameObject("QuestDescription");
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

                // 初始状态根据设置决定
                descObj.SetActive(ModSettings.TrackerShowDescription.Value);
            }

            // 任务进度容器
            GameObject progressContainer = new("ProgressContainer");
            progressContainer.transform.SetParent(entryObj.transform, false);

            VerticalLayoutGroup taskListLayout = progressContainer.AddComponent<VerticalLayoutGroup>();
            UIStyles.ConfigureVerticalLayout(taskListLayout, UIConstants.QUEST_TASK_SPACING);

            ContentSizeFitter progressFitter = progressContainer.AddComponent<ContentSizeFitter>();
            progressFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // 创建UI对象
            var questEntry = new QuestEntryUI(this)
            {
                QuestID = quest.ID,
                GameObject = entryObj,
                TitleText = titleText,
                ProgressBadgeText = progressBadgeText,
                ProgressContainer = progressContainer,
                DescriptionObject = descObj  // 保存描述对象引用
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
                entry.Cleanup(); // 清理事件订阅
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

    private void OnActiveSceneChanged(Scene prevScene, Scene newScene)
    {
        ModLogger.Log("QuestTracker", $"OnActiveSceneChanged: {newScene.name}");
        if (LevelManager.Instance == null || !LevelManager.Instance.IsRaidMap)
        {
            Disable();
        }
        // Enable candition in OnAfterLevelInitialized
    }

    /// <summary>
    /// 任务追踪状态变化回调（手动勾选/取消追踪）
    /// </summary>
    private void OnQuestTrackingChanged(int questId, bool isTracked)
    {
        try
        {
            // 移除 _isActive 检查，因为玩家可能在局外勾选任务
            // 如果当前在 Raid 中，立即刷新列表；否则，下次进入 Raid 时会自动加载
            ModLogger.Log("QuestTracker", $"Quest {questId} tracking changed to {isTracked}, active={_isActive}");

            if (_isActive)
            {
                RefreshQuestList();
            }
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
    /// 背包物品变化回调（用于更新提交物品类任务的持有数量）
    /// </summary>
    private void OnInventoryChanged(CharacterMainControl control, Inventory inventory, int index)
    {
        try
        {
            if (!_isActive)
            {
                return;
            }

            // 获取变化的物品
            Item? changedItem = inventory?.GetItemAt(index);

            if (changedItem != null)
            {
                // 物品存在（添加或替换）：只更新该物品相关的任务
                UpdateQuestsWithItemChanges(changedItem.TypeID);
            }
            else
            {
                // 物品为null（移除或丢弃）：刷新所有包含SubmitItems任务的追踪任务
                // 因为我们不知道是哪个物品被移除了
                ModLogger.Log("QuestTracker", $"Item removed from inventory at index {index}, refreshing all SubmitItems tasks");
                UpdateAllSubmitItemsTasks();
            }
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestTracker.OnInventoryChanged failed: {ex}");
        }
    }

    /// <summary>
    /// 仓库物品变化回调（用于更新提交物品类任务的持有数量）
    /// </summary>
    private void OnStorageChanged(PlayerStorage storage, Inventory inventory, int index)
    {
        try
        {
            if (!_isActive)
            {
                return;
            }

            // 获取变化的物品
            Item? changedItem = inventory?.GetItemAt(index);

            if (changedItem != null)
            {
                // 物品存在（添加或替换）：只更新该物品相关的任务
                UpdateQuestsWithItemChanges(changedItem.TypeID);
            }
            else
            {
                // 物品为null（移除）：刷新所有包含SubmitItems任务的追踪任务
                ModLogger.Log("QuestTracker", $"Item removed from storage at index {index}, refreshing all SubmitItems tasks");
                UpdateAllSubmitItemsTasks();
            }
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestTracker.OnStorageChanged failed: {ex}");
        }
    }

    /// <summary>
    /// 金钱变化回调（用于更新提交金钱类任务）
    /// </summary>
    private void OnMoneyChanged(long oldValue, long newValue)
    {
        try
        {
            if (!_isActive)
            {
                return;
            }

            // 更新所有包含提交金钱类任务的追踪任务
            UpdateQuestsWithMoneyChanges();
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestTracker.OnMoneyChanged failed: {ex}");
        }
    }

    /// <summary>
    /// 当物品数量变化时，更新相关任务的显示
    /// </summary>
    private void UpdateQuestsWithItemChanges(int itemTypeID)
    {
        try
        {
            foreach (var entry in _questEntries)
            {
                if (entry.QuestID == 0)
                {
                    continue;
                }

                // 查找任务
                var quest = QuestManager.Instance?.ActiveQuests?.FirstOrDefault(q => q != null && q.ID == entry.QuestID);
                if (quest == null || quest.Tasks == null)
                {
                    continue;
                }

                // 检查任务是否包含提交此类物品的任务
                bool hasMatchingSubmitTask = quest.Tasks.Any(task =>
                {
                    if (task is SubmitItems submitTask)
                    {
                        // 检查是否是 SubmitItems 类型且物品ID匹配
                        return submitTask != null && submitTask.ItemTypeID == itemTypeID;
                    }
                    return false;
                });

                if (hasMatchingSubmitTask)
                {
                    ModLogger.Log("QuestTracker", $"Item {itemTypeID} changed, updating quest {quest.ID} display");
                    entry.UpdateDisplay(quest);
                }
            }
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestTracker.UpdateQuestsWithItemChanges failed: {ex}");
        }
    }

    /// <summary>
    /// 当金钱数量变化时，更新相关任务的显示
    /// </summary>
    private void UpdateQuestsWithMoneyChanges()
    {
        try
        {
            foreach (var entry in _questEntries)
            {
                if (entry.QuestID == 0)
                {
                    continue;
                }

                // 查找任务
                var quest = QuestManager.Instance?.ActiveQuests?.FirstOrDefault(q => q != null && q.ID == entry.QuestID);
                if (quest == null || quest.Tasks == null)
                {
                    continue;
                }

                // 检查任务是否包含提交金钱的任务
                bool hasSubmitMoneyTask = quest.Tasks.Any(task => task is QuestTask_SubmitMoney);

                if (hasSubmitMoneyTask)
                {
                    ModLogger.Log("QuestTracker", $"Money changed, updating quest {quest.ID} display");
                    entry.UpdateDisplay(quest);
                }
            }
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestTracker.UpdateQuestsWithMoneyChanges failed: {ex}");
        }
    }

    /// <summary>
    /// 更新所有包含SubmitItems任务的追踪任务
    /// 当无法确定具体是哪个物品变化时使用（例如物品被丢弃）
    /// </summary>
    private void UpdateAllSubmitItemsTasks()
    {
        try
        {
            foreach (var entry in _questEntries)
            {
                if (entry.QuestID == 0)
                {
                    continue;
                }

                // 查找任务
                var quest = QuestManager.Instance?.ActiveQuests?.FirstOrDefault(q => q != null && q.ID == entry.QuestID);
                if (quest == null || quest.Tasks == null)
                {
                    continue;
                }

                // 检查任务是否包含提交物品的任务
                bool hasSubmitItemsTask = quest.Tasks.Any(task => task is SubmitItems);

                if (hasSubmitItemsTask)
                {
                    ModLogger.Log("QuestTracker", $"Updating quest {quest.ID} display (contains SubmitItems tasks)");
                    entry.UpdateDisplay(quest);
                }
            }
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestTracker.UpdateAllSubmitItemsTasks failed: {ex}");
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

            // 获取 Canvas 的虚拟分辨率（参考分辨率）
            // Canvas 使用 ScaleWithScreenSize，参考分辨率是 1920x1080
            // 所有位置计算都应该基于这个虚拟分辨率
            float referenceWidth = 1920f;
            float referenceHeight = 1080f;

            // 获取 Canvas RectTransform 以确保它是活跃的
            Canvas? canvas = _rootCanvas?.GetComponent<Canvas>();
            RectTransform? canvasRect = _rootCanvas?.GetComponent<RectTransform>();
            if (canvas == null || canvasRect == null) return;

            // 将归一化坐标(0-1)转换为 Canvas 本地坐标
            // 由于 Quest Panel 的锚点在右上角 (1, 1)
            // X 坐标：正值向右，从中心点计算需要转换
            // Y 坐标：从顶部开始向下计算，应该是负值
            float xPos = ModSettings.TrackerPositionX.Value * referenceWidth;
            float yPos = -ModSettings.TrackerPositionY.Value * referenceHeight;

            if (!ModSettings.TrackerPositionY.WasModifiedByUser)
            {
                // 计算 TimeOfDayDisplay 的偏移量以避免重叠
                float timeDisplayOffset = CalculateTimeOfDayDisplayOffset();
                yPos = -timeDisplayOffset;
                ModLogger.Log("QuestTracker", $"Position calculation: X={ModSettings.TrackerPositionX.Value} * {referenceWidth} = {xPos}, TimeDisplay offset: {timeDisplayOffset}");
            }
            else
            {
                ModLogger.Log("QuestTracker", $"TrackerPositionY WasModifiedByUser is true, not calculating TimeOfDayDisplay offset");
            }

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
    /// 计算 TimeOfDayDisplay 的高度偏移量，用于避免 UI 重叠
    /// </summary>
    private float CalculateTimeOfDayDisplayOffset()
    {
        try
        {
            // 查找场景中的 TimeOfDayDisplay 组件
            TimeOfDayDisplay? timeDisplay = FindObjectOfType<TimeOfDayDisplay>();
            if (timeDisplay == null)
            {
                ModLogger.Log("QuestTracker", "TimeOfDayDisplay not found in scene");
                return 0f;
            }

            // 获取 stormTitleText 的 RectTransform
            if (timeDisplay.stormTitleText == null)
            {
                ModLogger.Log("QuestTracker", "stormTitleText not found in TimeOfDayDisplay");
                return 0f;
            }

            RectTransform stormTextRect = timeDisplay.stormTitleText.GetComponent<RectTransform>();
            if (stormTextRect == null)
            {
                ModLogger.Log("QuestTracker", "RectTransform not found for stormTitleText");
                return 0f;
            }

            // 获取 Canvas 和 CanvasScaler 信息
            Canvas? questCanvas = _rootCanvas?.GetComponent<Canvas>();
            CanvasScaler? scaler = _rootCanvas?.GetComponent<CanvasScaler>();

            if (questCanvas == null || scaler == null)
            {
                ModLogger.Log("QuestTracker", "Canvas or CanvasScaler not found");
                return 0f;
            }

            // 获取 stormTitleText 所在的 Canvas（通常是游戏主 Canvas）
            Canvas? stormTextCanvas = stormTextRect.GetComponentInParent<Canvas>();
            if (stormTextCanvas == null)
            {
                ModLogger.Log("QuestTracker", "Canvas parent not found for stormTitleText");
                return 0f;
            }

            // 获取世界坐标的四个角
            Vector3[] worldCorners = new Vector3[4];
            stormTextRect.GetWorldCorners(worldCorners);
            // worldCorners[0] = 左下角, worldCorners[1] = 左上角, worldCorners[2] = 右上角, worldCorners[3] = 右下角

            // 获取 Quest Panel 的 RectTransform
            RectTransform? questPanelRect = _questPanel?.GetComponent<RectTransform>();
            if (questPanelRect == null)
            {
                ModLogger.Log("QuestTracker", "Quest panel RectTransform not found");
                return 0f;
            }

            // 将 stormTitleText 的左下角世界坐标转换为 Quest Canvas 的本地坐标
            Vector2 stormTextBottomInCanvasLocal = RectTransformUtility.WorldToScreenPoint(questCanvas.worldCamera, worldCorners[0]);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                questCanvas.GetComponent<RectTransform>(),
                stormTextBottomInCanvasLocal,
                questCanvas.worldCamera,
                out Vector2 stormTextLocalPos
            );

            // 获取 Quest Canvas 的尺寸
            RectTransform? questCanvasRect = questCanvas.GetComponent<RectTransform>();
            if (questCanvasRect == null)
            {
                ModLogger.Log("QuestTracker", "Quest canvas RectTransform not found");
                return 0f;
            }

            Vector2 questCanvasSize = questCanvasRect.rect.size;

            // stormTitleText 的本地Y坐标（Canvas 内）
            // 在 Canvas 中，Y 轴向上为正，所以我们需要从 Canvas 顶部计算距离
            // Canvas 顶部在本地坐标中是 +questCanvasSize.y / 2
            float canvasTopY = questCanvasSize.y / 2f;
            float stormTextBottomY = stormTextLocalPos.y;

            // 计算从 Canvas 顶部到 stormText 底部的距离（向下为正）
            float distanceFromCanvasTop = canvasTopY - stormTextBottomY;

            // 添加额外的边距
            float additionalMargin = 14f;
            float totalOffset = distanceFromCanvasTop + additionalMargin;

            ModLogger.Log("QuestTracker", $"TimeOfDayDisplay offset calculated: {totalOffset}px in Canvas space (stormText bottom Y: {stormTextBottomY}, canvas top Y: {canvasTopY}, margin: {additionalMargin})");

            return totalOffset;
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestTracker.CalculateTimeOfDayDisplayOffset failed: {ex}");
            return 0f;
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

    /// <summary>
    /// 地图过滤设置变化回调
    /// </summary>
    private void OnFilterByMapChanged(object? sender, SettingsValueChangedEventArgs<bool> args)
    {
        if (_isActive)
        {
            // 刷新任务列表以应用新的过滤规则
            ModLogger.Log("QuestTracker", $"Map filter setting changed to: {args.NewValue}");
            RefreshQuestList();
        }
    }

    private void OnHotkeyUsedChanged(object? sender, SettingsValueChangedEventArgs<bool> args)
    {
        if (_isActive)
        {
            UpdateHelpTextVisibility();
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
    public GameObject? DescriptionObject { get; set; }  // 描述UI元素

    private readonly List<TaskUIElement> _taskElements = [];
    private string _lastProgressText = "";
    private Quest? _currentQuest; // 保存当前任务引用，用于事件订阅
    private float _lastTaskStatusChangeTime = -1f; // 防抖：记录上次处理Task状态变化的时间
    private const float TASK_STATUS_DEBOUNCE_INTERVAL = 0.25f; // 防抖间隔（250ms）
    private bool _taskStatusChangePending = false; // 防抖：标记是否有待处理的任务状态变更
    private Coroutine? _debouncedUpdateCoroutine; // 防抖协程引用
    private readonly MonoBehaviour _coroutineRunner; // 用于运行协程的 MonoBehaviour 引用

    /// <summary>
    /// 构造函数
    /// </summary>
    public QuestEntryUI(MonoBehaviour coroutineRunner)
    {
        _coroutineRunner = coroutineRunner;
    }

    /// <summary>
    /// 订阅任务的Task事件
    /// </summary>
    private void SubscribeToTaskEvents(Quest quest)
    {
        try
        {
            // 先取消之前的订阅
            UnsubscribeFromTaskEvents();

            _currentQuest = quest;

            if (quest.Tasks != null)
            {
                foreach (var task in quest.Tasks)
                {
                    if (task != null)
                    {
                        // 订阅每个Task的状态变化事件
                        task.onStatusChanged += OnTaskStatusChanged;
                    }
                }
            }

            ModLogger.Log("QuestTracker", $"Subscribed to {quest.Tasks?.Count ?? 0} task events for quest {quest.ID}");
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestEntryUI.SubscribeToTaskEvents failed: {ex}");
        }
    }

    /// <summary>
    /// 取消订阅任务的Task事件
    /// </summary>
    private void UnsubscribeFromTaskEvents()
    {
        try
        {
            if (_currentQuest != null && _currentQuest.Tasks != null)
            {
                foreach (var task in _currentQuest.Tasks)
                {
                    if (task != null)
                    {
                        task.onStatusChanged -= OnTaskStatusChanged;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestEntryUI.UnsubscribeFromTaskEvents failed: {ex}");
        }
    }

    /// <summary>
    /// Task状态变化回调
    /// </summary>
    private void OnTaskStatusChanged(Task task)
    {
        try
        {
            if (_currentQuest != null)
            {
                // 防抖检查：避免在短时间内多次处理相同的事件
                float currentTime = Time.time;
                if (currentTime - _lastTaskStatusChangeTime < TASK_STATUS_DEBOUNCE_INTERVAL)
                {
                    // 标记为待处理，在防抖窗口结束后调用一次
                    _taskStatusChangePending = true;
                    // 如果还没有启动延迟更新协程，就启动一个
                    if (_debouncedUpdateCoroutine == null && _coroutineRunner != null)
                    {
                        _debouncedUpdateCoroutine = _coroutineRunner.StartCoroutine(DebouncedUpdateCoroutine());
                    }
                    return;
                }

                _lastTaskStatusChangeTime = currentTime;
                _taskStatusChangePending = false;

                // 停止之前的延迟更新协程（如果有的话）
                if (_debouncedUpdateCoroutine != null && _coroutineRunner != null)
                {
                    _coroutineRunner.StopCoroutine(_debouncedUpdateCoroutine);
                    _debouncedUpdateCoroutine = null;
                }

                // ModLogger.Log("QuestTracker", $"Task status changed in quest {_currentQuest.ID} {_currentQuest.DisplayName}, updating display");
                UpdateDisplay(_currentQuest);
            }
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestEntryUI.OnTaskStatusChanged failed: {ex}");
        }
    }

    /// <summary>
    /// 延迟更新协程 - 在防抖窗口结束后执行一次更新
    /// </summary>
    private System.Collections.IEnumerator DebouncedUpdateCoroutine()
    {
        // 等待防抖间隔剩余的时间
        float remainingTime = TASK_STATUS_DEBOUNCE_INTERVAL - (Time.time - _lastTaskStatusChangeTime);
        yield return new WaitForSeconds(Mathf.Max(0, remainingTime));

        // 检查是否有待处理的更新
        if (_taskStatusChangePending && _currentQuest != null)
        {
            _taskStatusChangePending = false;
            _lastTaskStatusChangeTime = Time.time;
            UpdateDisplay(_currentQuest);
        }

        _debouncedUpdateCoroutine = null;
    }

    /// <summary>
    /// 清理资源
    /// </summary>
    public void Cleanup()
    {
        try
        {
            UnsubscribeFromTaskEvents();
            _currentQuest = null;
            _lastTaskStatusChangeTime = -1f; // 重置防抖时间戳
            _taskStatusChangePending = false; // 清除待处理标志

            // 停止任何正在运行的延迟更新协程
            if (_debouncedUpdateCoroutine != null && _coroutineRunner != null && _coroutineRunner.gameObject.activeInHierarchy)
            {
                _coroutineRunner.StopCoroutine(_debouncedUpdateCoroutine);
                _debouncedUpdateCoroutine = null;
            }
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestEntryUI.Cleanup failed: {ex}");
        }
    }

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

            // 如果是新任务或任务变了，重新订阅事件
            if (_currentQuest == null || _currentQuest.ID != quest.ID)
            {
                SubscribeToTaskEvents(quest);
            }

            // 检查任务是否所有task都完成
            bool allTasksFinished = quest.AreTasksFinished();

            // 更新标题颜色
            if (TitleText != null)
            {
                TitleText.color = allTasksFinished ? UIConstants.QUEST_COMPLETE_COLOR : UIConstants.QUEST_TITLE_COLOR;
                TitleText.text = quest.DisplayName;
            }

            // 更新描述显示/隐藏状态
            if (DescriptionObject != null)
            {
                // 如果所有task完成，隐藏描述；否则根据设置决定
                bool shouldShow = !allTasksFinished
                    && ModSettings.TrackerShowDescription.Value
                    && !string.IsNullOrEmpty(quest.Description);
                if (DescriptionObject.activeSelf != shouldShow)
                {
                    DescriptionObject.SetActive(shouldShow);
                }
            }

            // 更新进度徽章 (只在变化时更新)
            if (ProgressBadgeText != null && quest.Tasks != null)
            {
                int finishedCount = quest.Tasks.Count(t => t != null && t.IsFinished());
                string newProgressText;
                Color badgeColor;

                if (allTasksFinished)
                {
                    // 所有task完成：显示绿色对勾
                    newProgressText = "✓";
                    badgeColor = UIConstants.QUEST_COMPLETE_COLOR;
                }
                else
                {
                    // 未完成：显示进度数字
                    newProgressText = $"{finishedCount}/{quest.Tasks.Count}";
                    badgeColor = UIConstants.QUEST_PROGRESS_COLOR;
                }

                if (_lastProgressText != newProgressText)
                {
                    ProgressBadgeText.text = newProgressText;
                    ProgressBadgeText.color = badgeColor;
                    _lastProgressText = newProgressText;
                }
            }

            // 更新任务进度 (复用现有UI元素)
            // 如果所有task都完成，隐藏整个进度容器
            if (allTasksFinished)
            {
                if (ProgressContainer.activeSelf)
                {
                    ProgressContainer.SetActive(false);
                }
            }
            else if (quest.Tasks != null && quest.Tasks.Count > 0)
            {
                // 确保进度容器显示
                if (!ProgressContainer.activeSelf)
                {
                    ProgressContainer.SetActive(true);
                }

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
                        GameObject taskObj = new($"Task_{task.ID}");
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

