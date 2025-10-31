using System;
using System.Collections.Generic;
using SodaCraft.Localizations;
using UnityEngine;

namespace EfDEnhanced.Utils;

/// <summary>
/// Mod 本地化辅助类
/// 管理 Mod 的多语言文本
/// </summary>
public static class LocalizationHelper
{
    // 本地化键名前缀，避免与游戏原有键冲突
    private const string KeyPrefix = "EfDEnhanced_";

    // 本地化文本数据
    private static readonly Dictionary<SystemLanguage, Dictionary<string, string>> LocalizationData = [];

    /// <summary>
    /// 语言变更事件 - UI 组件可订阅此事件以在语言改变时刷新显示
    /// </summary>
    public static event Action<SystemLanguage>? OnLanguageChanged;

    /// <summary>
    /// 获取当前应该使用的语言（直接跟随游戏设置）
    /// </summary>
    private static SystemLanguage GetCurrentLanguage()
    {
        // 直接使用游戏的语言设置
        return LocalizationManager.CurrentLanguage;
    }

    /// <summary>
    /// 初始化本地化系统
    /// </summary>
    public static void Initialize()
    {
        try
        {
            ModLogger.Log("Localization", "Initializing localization system...");

            // 注册语言切换事件
            LocalizationManager.OnSetLanguage += LanguageChangedHandler;

            // 加载所有语言的翻译
            LoadTranslations();

            // 应用当前语言的翻译（直接跟随游戏语言）
            ApplyTranslations(GetCurrentLanguage());

            ModLogger.Log("Localization", $"Localization initialized for language: {LocalizationManager.CurrentLanguage}");
        }
        catch (System.Exception ex)
        {
            ModLogger.LogError($"Failed to initialize localization: {ex}");
        }
    }

    /// <summary>
    /// 清理本地化系统
    /// </summary>
    public static void Cleanup()
    {
        try
        {
            LocalizationManager.OnSetLanguage -= LanguageChangedHandler;

            // 移除所有覆盖的文本
            foreach (var langData in LocalizationData.Values)
            {
                foreach (var key in langData.Keys)
                {
                    LocalizationManager.RemoveOverrideText(GetFullKey(key));
                }
            }

            ModLogger.Log("Localization", "Localization system cleaned up");
        }
        catch (System.Exception ex)
        {
            ModLogger.LogError($"Failed to cleanup localization: {ex}");
        }
    }

    /// <summary>
    /// 语言切换事件处理 - 同时触发公共事件供 UI 组件订阅
    /// </summary>
    private static void LanguageChangedHandler(SystemLanguage newLanguage)
    {
        try
        {
            ModLogger.Log("Localization", $"Game language changed to: {newLanguage}");
            // 直接使用游戏语言设置
            ApplyTranslations(newLanguage);

            // 触发公共事件，通知所有订阅者
            OnLanguageChanged?.Invoke(newLanguage);
        }
        catch (System.Exception ex)
        {
            ModLogger.LogError($"Failed to handle language change: {ex}");
        }
    }

    /// <summary>
    /// 加载所有语言的翻译数据
    /// </summary>
    private static void LoadTranslations()
    {
        // 简体中文
        LocalizationData[SystemLanguage.ChineseSimplified] = new Dictionary<string, string>
        {
            // Raid 检查相关
            { "RaidCheck_Title", "Raid 准备检查" },
            { "RaidCheck_AllClear", "装备检查通过" },
            { "RaidCheck_HasIssues", "检测到以下问题：\n" },
            { "RaidCheck_Confirm", "继续进入" },
            { "RaidCheck_Cancel", "返回准备" },
            
            // 警告信息 - 使用富文本标记来突出显示
            { "Warning_NoWeapon", "<color=#FF6B6B>⚠ 未携带枪支</color>" },
            { "Warning_NoAmmo", "<color=#FF6B6B>⚠ 未携带弹药</color>" },
            { "Warning_NoMedicine", "<color=#FF6B6B>⚠ 未携带药品</color>" },
            { "Warning_NoFood", "<color=#FF6B6B>⚠ 未携带食物</color>" },
            { "Warning_StormyWeather", "<color=#FF4444>⚠ 当前为风暴天气</color>" },
            { "Warning_StormComing", "<color=#FFA500>⚠ 风暴即将来临（24小时内）</color>" },
            { "Warning_QuestItem", "<color=#FFD700>⚠ 任务物品不足: {0} ({1}/{2}) - {3}</color>" },
            { "Warning_LowAmmo", "<color=#FF8C00>⚠ 武器弹药不足: {0} ({1}) - {2}/{3} 发</color>" },
            { "Warning_QuestWeapon", "<color=#FFD700>⚠ 任务所需武器: {0} - {1} ({2})</color>" },
            
            // 任务追踪器
            { "QuestTracker_Title", "活跃任务" },
            { "QuestTracker_Progress", "进度: {0}/{1}" },
            { "QuestTracker_NoQuests", "无进行中的任务" },
            { "QuestTracker_TaskComplete", "✓" },  // 已完成标记
            { "QuestTracker_TaskPending", "○" },   // 未完成标记
            { "QuestTracker_CheckboxLabel", "局内追踪" },  // 任务详情界面的追踪复选框标签
            { "QuestTracker_HelpText", "使用 . 快捷键隐藏/显示任务追踪面板\n游戏设置中的Mod设置可以调整面板位置" },

            // 设置界面
            { "Settings_Title", "EfD Enhanced 设置" },
            { "Settings_ResetButton", "恢复默认" },
            { "Settings_CloseButton", "关闭" },
            { "Settings_ModSettings_Button", "EfD Enhanced 设置" },

            // 设置类别
            { "Settings_Category_PreRaidCheck", "Raid前检查" },
            { "Settings_Category_QuestTracker", "任务追踪器" },
            { "Settings_Category_Movement", "移动优化" },
            { "Settings_Category_UI", "界面增强" },
            { "Settings_Category_FunFeatures", "有趣的功能" },

            // Pre-Raid Check 设置
            { "Settings_EnableRaidCheck_Name", "启用Raid前检查" },
            { "Settings_EnableRaidCheck_Desc", "启用整个Raid前检查系统" },
            { "Settings_CheckWeapon_Name", "检查武器" },
            { "Settings_CheckWeapon_Desc", "未携带武器时警告" },
            { "Settings_CheckAmmo_Name", "检查弹药" },
            { "Settings_CheckAmmo_Desc", "未携带弹药时警告" },
            { "Settings_CheckMeds_Name", "检查医疗用品" },
            { "Settings_CheckMeds_Desc", "未携带医疗用品时警告" },
            { "Settings_CheckFood_Name", "检查食物/水" },
            { "Settings_CheckFood_Desc", "未携带食物或水时警告" },
            { "Settings_CheckWeather_Name", "警告风暴天气" },
            { "Settings_CheckWeather_Desc", "检测到风暴天气时警告" },
            { "Settings_CheckQuestItems_Name", "检查任务物品" },
            { "Settings_CheckQuestItems_Desc", "检测任务所需物品数量是否足够" },
            { "Settings_CheckQuestWeapons_Name", "检查任务武器" },
            { "Settings_CheckQuestWeapons_Desc", "检测是否携带任务所需武器" },

            // Quest Tracker 设置
            { "Settings_EnableQuestTracker_Name", "启用任务追踪HUD" },
            { "Settings_EnableQuestTracker_Desc", "在Raid中显示活跃任务追踪器" },
            { "Settings_TrackerPositionX_Name", "追踪器水平位置" },
            { "Settings_TrackerPositionX_Desc", "水平位置 (0=左, 1=右)" },
            { "Settings_TrackerPositionY_Name", "追踪器垂直位置" },
            { "Settings_TrackerPositionY_Desc", "垂直位置 (0=顶, 1=底)" },
            { "Settings_TrackerScale_Name", "追踪器缩放" },
            { "Settings_TrackerScale_Desc", "UI缩放倍数" },
            { "Settings_TrackerShowDescription_Name", "显示任务描述" },
            { "Settings_TrackerShowDescription_Desc", "在追踪器中显示任务描述" },
            { "Settings_TrackerFilterByMap_Name", "只显示当前地图任务" },
            { "Settings_TrackerFilterByMap_Desc", "只显示当前地图相关的任务，以及没有地图限制的任务" },
            { "Settings_TrackerToggleHotkey_Name", "任务追踪器折叠/展开快捷键" },
            { "Settings_TrackerToggleHotkey_Desc", "按此键折叠或展开任务追踪列表" },
            { "Settings_TrackerHotkeyUsed_Name", "快捷键已使用标记" },
            { "Settings_TrackerHotkeyUsed_Desc", "内部设置：标记用户是否已使用过显示/隐藏快捷键" },

            // Movement Enhancement Settings
            { "Settings_MovementEnhancement_Name", "移动响应增强" },
            { "Settings_MovementEnhancement_Desc", "优化角色移动感觉，减少粘脚感。提供多个档位：禁用、轻度、中度、重度" },
            { "Settings_Movement_Disabled", "禁用" },
            { "Settings_Movement_Light", "轻度优化" },
            { "Settings_Movement_Medium", "中度优化" },
            { "Settings_Movement_Heavy", "重度优化" },

            // UI Enhancement Settings
            { "Settings_EnableWeaponComparison_Name", "启用武器对比" },
            { "Settings_EnableWeaponComparison_Desc", "在鼠标悬停时对比选中武器和悬停武器的属性，显示差异并用颜色标识优劣" },
            { "Settings_FastBuyEnabled_Name", "启用快速购买" },
            { "Settings_FastBuyEnabled_Desc", "在商店菜单中鼠标悬停物品并按F键快速购买物品" },
            { "Settings_FastSellEnabled_Name", "启用快速贩卖" },
            { "Settings_FastSellEnabled_Desc", "在商店菜单中鼠标悬停物品并按F键快速贩卖物品" },
            { "Settings_ItemWheelScale_Name", "轮盘菜单缩放" },
            { "Settings_ItemWheelScale_Desc", "调整所有轮盘菜单的显示大小（物品轮盘和投掷物轮盘）" },
            { "Settings_ItemWheelTimeScale_Name", "轮盘菜单时间缩放" },
            { "Settings_ItemWheelTimeScale_Desc", "调整所有轮盘菜单的时间缩放（物品轮盘和投掷物轮盘）" },
            { "Settings_ItemWheelSensitivity_Name", "轮盘菜单灵敏度" },
            { "Settings_ItemWheelSensitivity_Desc", "调整所有轮盘菜单的鼠标灵敏度（物品轮盘和投掷物轮盘）" },
            { "Settings_ItemWheelMenuHotkey_Name", "物品轮盘快捷键" },
            { "Settings_ItemWheelMenuHotkey_Desc", "按下并松开此键来打开物品轮盘菜单" },
            { "Settings_ThrowableWheelEnabled_Name", "启用投掷物轮盘" },
            { "Settings_ThrowableWheelEnabled_Desc", "启用投掷物轮盘菜单功能" },
            { "Settings_ThrowableWheelHotkey_Name", "投掷物轮盘快捷键" },
            { "Settings_ThrowableWheelHotkey_Desc", "按下并松开此键来打开投掷物轮盘菜单（默认G键）" },
            { "Settings_PressAnyKey", "按任意键..." },

            { "Settings_AutoTrackNewQuests_Name", "自动追踪新接受的任务" },
            { "Settings_AutoTrackNewQuests_Desc", "新接受任务时自动将其添加到局内追踪列表" },
            { "Settings_EnableDuckShit_Name", "启用鸭子排便" },
            { "Settings_EnableDuckShit_Desc", "启用鸭子会根据能量和水分消耗自动排便的功能" },
        };

        // 繁体中文
        LocalizationData[SystemLanguage.ChineseTraditional] = new Dictionary<string, string>
        {
            { "RaidCheck_Title", "Raid 準備檢查" },
            { "RaidCheck_AllClear", "裝備檢查通過" },
            { "RaidCheck_HasIssues", "檢測到以下問題：\n" },
            { "RaidCheck_Confirm", "繼續進入" },
            { "RaidCheck_Cancel", "返回準備" },

            { "Warning_NoWeapon", "<color=#FF6B6B>⚠ 未攜帶槍支</color>" },
            { "Warning_NoAmmo", "<color=#FF6B6B>⚠ 未攜帶彈藥</color>" },
            { "Warning_NoMedicine", "<color=#FF6B6B>⚠ 未攜帶藥品</color>" },
            { "Warning_NoFood", "<color=#FF6B6B>⚠ 未攜帶食物</color>" },
            { "Warning_StormyWeather", "<color=#FF4444>⚠ 當前為風暴天氣</color>" },
            { "Warning_StormComing", "<color=#FFA500>⚠ 風暴即將來臨（24小時內）</color>" },
            { "Warning_QuestItem", "<color=#FFD700>⚠ 任務物品不足: {0} ({1}/{2}) - {3}</color>" },
            { "Warning_LowAmmo", "<color=#FF8C00>⚠ 武器彈藥不足: {0} ({1}) - {2}/{3} 發</color>" },
            { "Warning_QuestWeapon", "<color=#FFD700>⚠ 任務所需武器: {0} - {1} ({2})</color>" },

            { "QuestTracker_Title", "活躍任務" },
            { "QuestTracker_Progress", "進度: {0}/{1}" },
            { "QuestTracker_NoQuests", "無進行中的任務" },
            { "QuestTracker_TaskComplete", "✓" },
            { "QuestTracker_TaskPending", "○" },
            { "QuestTracker_CheckboxLabel", "局內追蹤" },
            { "QuestTracker_HelpText", "使用 . 快捷鍵隱藏/顯示任務追蹤面板\n遊戲設置中的Mod設置可以調整面板位置" },

            // 設置界面
            { "Settings_Title", "EfD Enhanced 設置" },
            { "Settings_ResetButton", "恢復默認" },
            { "Settings_CloseButton", "關閉" },
            { "Settings_ModSettings_Button", "EfD Enhanced 設置" },

            // 設置類別
            { "Settings_Category_PreRaidCheck", "Raid前檢查" },
            { "Settings_Category_QuestTracker", "任務追蹤器" },
            { "Settings_Category_Movement", "移動優化" },
            { "Settings_Category_UI", "界面增強" },
            { "Settings_Category_FunFeatures", "有趣功能" },

            // Pre-Raid Check 設置
            { "Settings_EnableRaidCheck_Name", "啟用Raid前檢查" },
            { "Settings_EnableRaidCheck_Desc", "啟用整個Raid前檢查系統" },
            { "Settings_CheckWeapon_Name", "檢查武器" },
            { "Settings_CheckWeapon_Desc", "未攜帶武器時警告" },
            { "Settings_CheckAmmo_Name", "檢查彈藥" },
            { "Settings_CheckAmmo_Desc", "未攜帶彈藥時警告" },
            { "Settings_CheckMeds_Name", "檢查醫療用品" },
            { "Settings_CheckMeds_Desc", "未攜帶醫療用品時警告" },
            { "Settings_CheckFood_Name", "檢查食物/水" },
            { "Settings_CheckFood_Desc", "未攜帶食物或水時警告" },
            { "Settings_CheckWeather_Name", "警告風暴天氣" },
            { "Settings_CheckWeather_Desc", "檢測到風暴天氣時警告" },
            { "Settings_CheckQuestItems_Name", "檢查任務物品" },
            { "Settings_CheckQuestItems_Desc", "檢測任務所需物品數量是否足夠" },
            { "Settings_CheckQuestWeapons_Name", "檢查任務武器" },
            { "Settings_CheckQuestWeapons_Desc", "檢測是否攜帶任務所需武器" },

            // Quest Tracker 設置
            { "Settings_EnableQuestTracker_Name", "啟用任務追蹤HUD" },
            { "Settings_EnableQuestTracker_Desc", "在Raid中顯示活躍任務追蹤器" },
            { "Settings_TrackerPositionX_Name", "追蹤器水平位置" },
            { "Settings_TrackerPositionX_Desc", "水平位置 (0=左, 1=右)" },
            { "Settings_TrackerPositionY_Name", "追蹤器垂直位置" },
            { "Settings_TrackerPositionY_Desc", "垂直位置 (0=頂, 1=底)" },
            { "Settings_TrackerScale_Name", "追蹤器縮放" },
            { "Settings_TrackerScale_Desc", "UI縮放倍數" },
            { "Settings_TrackerShowDescription_Name", "顯示任務描述" },
            { "Settings_TrackerShowDescription_Desc", "在追蹤器中顯示任務描述" },
            { "Settings_TrackerFilterByMap_Name", "只顯示當前地圖任務" },
            { "Settings_TrackerFilterByMap_Desc", "只顯示當前地圖相關的任務，以及沒有地圖限制的任務" },
            { "Settings_TrackerToggleHotkey_Name", "任務追蹤器折疊/展開快捷鍵" },
            { "Settings_TrackerToggleHotkey_Desc", "按此鍵折疊或展開任務追蹤列表" },
            { "Settings_TrackerHotkeyUsed_Name", "快捷鍵已使用標記" },
            { "Settings_TrackerHotkeyUsed_Desc", "內部設置：標記用戶是否已使用過顯示/隱藏快捷鍵" },

            // Movement Enhancement Settings
            { "Settings_MovementEnhancement_Name", "移動響應增強" },
            { "Settings_MovementEnhancement_Desc", "優化角色移動感覺，減少粘腳感。提供多個檔位：禁用、輕度、中度、重度" },
            { "Settings_Movement_Disabled", "禁用" },
            { "Settings_Movement_Light", "輕度優化" },
            { "Settings_Movement_Medium", "中度優化" },
            { "Settings_Movement_Heavy", "重度優化" },

            // UI Enhancement Settings
            { "Settings_EnableWeaponComparison_Name", "啟用武器對比" },
            { "Settings_EnableWeaponComparison_Desc", "在鼠標懸停時對比選中武器和懸停武器的屬性，顯示差異並用顏色標識優劣" },
            { "Settings_FastBuyEnabled_Name", "啟用快速購買" },
            { "Settings_FastBuyEnabled_Desc", "在商店菜單中鼠標懸停物品並按F鍵快速購買物品" },
            { "Settings_FastSellEnabled_Name", "啟用快速販賣" },
            { "Settings_FastSellEnabled_Desc", "在商店菜單中鼠標懸停物品並按F鍵快速販賣物品" },
            { "Settings_ItemWheelScale_Name", "輪盤選單縮放" },
            { "Settings_ItemWheelScale_Desc", "調整所有輪盤選單的顯示大小（物品輪盤和投擲物輪盤）" },
            { "Settings_ItemWheelTimeScale_Name", "輪盤選單時間縮放" },
            { "Settings_ItemWheelTimeScale_Desc", "調整所有輪盤選單的時間縮放（物品輪盤和投擲物輪盤）" },
            { "Settings_ItemWheelSensitivity_Name", "輪盤選單靈敏度" },
            { "Settings_ItemWheelSensitivity_Desc", "調整所有輪盤選單的滑鼠靈敏度（物品輪盤和投擲物輪盤）" },
            { "Settings_ItemWheelMenuHotkey_Name", "物品輪盤快捷鍵" },
            { "Settings_ItemWheelMenuHotkey_Desc", "按下並鬆開此鍵來打開物品輪盤選單" },
            { "Settings_ThrowableWheelEnabled_Name", "啟用投擲物輪盤" },
            { "Settings_ThrowableWheelEnabled_Desc", "啟用投擲物輪盤選單功能" },
            { "Settings_ThrowableWheelHotkey_Name", "投擲物輪盤快捷鍵" },
            { "Settings_ThrowableWheelHotkey_Desc", "按下並鬆開此鍵來打開投擲物輪盤選單（預設G鍵）" },
            { "Settings_PressAnyKey", "按任意鍵..." },

            { "Settings_AutoTrackNewQuests_Name", "自動追蹤新接受任務" },
            { "Settings_AutoTrackNewQuests_Desc", "新接受任務時自動將其添加到局內追蹤列表" },
            { "Settings_EnableDuckShit_Name", "啟用鴨子排便" },
            { "Settings_EnableDuckShit_Desc", "啟用鴨子會根據能量和水分消耗自動排便的功能" },
        };

        // 英语
        LocalizationData[SystemLanguage.English] = new Dictionary<string, string>
        {
            { "RaidCheck_Title", "Raid Preparation Check" },
            { "RaidCheck_AllClear", "Equipment check passed" },
            { "RaidCheck_HasIssues", "The following issues detected:\n" },
            { "RaidCheck_Confirm", "Continue Anyway" },
            { "RaidCheck_Cancel", "Go Back" },

            { "Warning_NoWeapon", "<color=#FF6B6B>⚠ No weapon equipped</color>" },
            { "Warning_NoAmmo", "<color=#FF6B6B>⚠ No ammunition</color>" },
            { "Warning_NoMedicine", "<color=#FF6B6B>⚠ No medical supplies</color>" },
            { "Warning_NoFood", "<color=#FF6B6B>⚠ No food or drinks</color>" },
            { "Warning_StormyWeather", "<color=#FF4444>⚠ Stormy weather conditions</color>" },
            { "Warning_StormComing", "<color=#FFA500>⚠ Storm approaching (within 24 hours)</color>" },
            { "Warning_QuestItem", "<color=#FFD700>⚠ Quest item insufficient: {0} ({1}/{2}) - {3}</color>" },
            { "Warning_LowAmmo", "<color=#FF8C00>⚠ Low ammo for weapon: {0} ({1}) - {2}/{3} rounds</color>" },
            { "Warning_QuestWeapon", "<color=#FFD700>⚠ Quest required weapon: {0} - {1} ({2})</color>" },

            { "QuestTracker_Title", "Active Quests" },
            { "QuestTracker_Progress", "Progress: {0}/{1}" },
            { "QuestTracker_NoQuests", "No active quests" },
            { "QuestTracker_TaskComplete", "✓" },
            { "QuestTracker_TaskPending", "○" },
            { "QuestTracker_CheckboxLabel", "Track in Raid" },
            { "QuestTracker_HelpText", "Press . to hide/show quest tracker\nMod settings in game settings can adjust panel position" },

            // Settings UI
            { "Settings_Title", "EfD Enhanced Settings" },
            { "Settings_ResetButton", "Reset to Defaults" },
            { "Settings_CloseButton", "Close" },
            { "Settings_ModSettings_Button", "EfD Enhanced Settings" },

            // Settings Categories
            { "Settings_Category_PreRaidCheck", "Pre-Raid Check" },
            { "Settings_Category_QuestTracker", "Quest Tracker" },
            { "Settings_Category_Movement", "Movement Enhancement" },
            { "Settings_Category_UI", "UI Enhancement" },
            { "Settings_Category_FunFeatures", "Fun Features" },

            // Pre-Raid Check Settings
            { "Settings_EnableRaidCheck_Name", "Enable Pre-Raid Check" },
            { "Settings_EnableRaidCheck_Desc", "Enable the entire pre-raid check system" },
            { "Settings_CheckWeapon_Name", "Check Weapon" },
            { "Settings_CheckWeapon_Desc", "Warn if no weapon is equipped" },
            { "Settings_CheckAmmo_Name", "Check Ammunition" },
            { "Settings_CheckAmmo_Desc", "Warn if no ammunition is available" },
            { "Settings_CheckMeds_Name", "Check Medical Supplies" },
            { "Settings_CheckMeds_Desc", "Warn if no medical supplies are available" },
            { "Settings_CheckFood_Name", "Check Food/Water" },
            { "Settings_CheckFood_Desc", "Warn if no food or water is available" },
            { "Settings_CheckWeather_Name", "Warn Stormy Weather" },
            { "Settings_CheckWeather_Desc", "Warn about stormy weather conditions" },
            { "Settings_CheckQuestItems_Name", "Check Quest Items" },
            { "Settings_CheckQuestItems_Desc", "Check if required quest items are sufficient" },
            { "Settings_CheckQuestWeapons_Name", "Check Quest Weapons" },
            { "Settings_CheckQuestWeapons_Desc", "Check if quest-required weapons are equipped" },

            // Quest Tracker Settings
            { "Settings_EnableQuestTracker_Name", "Enable Quest Tracker HUD" },
            { "Settings_EnableQuestTracker_Desc", "Show active quest tracker during raids" },
            { "Settings_TrackerPositionX_Name", "Tracker Horizontal Position" },
            { "Settings_TrackerPositionX_Desc", "Horizontal position (0=left, 1=right)" },
            { "Settings_TrackerPositionY_Name", "Tracker Vertical Position" },
            { "Settings_TrackerPositionY_Desc", "Vertical position (0=top, 1=bottom)" },
            { "Settings_TrackerScale_Name", "Tracker Scale" },
            { "Settings_TrackerScale_Desc", "UI scale multiplier" },
            { "Settings_TrackerShowDescription_Name", "Show Quest Descriptions" },
            { "Settings_TrackerShowDescription_Desc", "Display quest descriptions in tracker" },
            { "Settings_TrackerFilterByMap_Name", "Show Only Current Map Quests" },
            { "Settings_TrackerFilterByMap_Desc", "Show only quests for the current map and quests without map requirements" },
            { "Settings_TrackerToggleHotkey_Name", "Quest Tracker Collapse/Expand Hotkey" },
            { "Settings_TrackerToggleHotkey_Desc", "Press this key to collapse or expand the quest tracker list" },
            { "Settings_TrackerHotkeyUsed_Name", "Hotkey Used Flag" },
            { "Settings_TrackerHotkeyUsed_Desc", "Internal setting: Marks whether the user has used the show/hide hotkey" },

            // Movement Enhancement Settings
            { "Settings_MovementEnhancement_Name", "Movement Response Enhancement" },
            { "Settings_MovementEnhancement_Desc", "Optimize character movement feel, reduce sticky movement. Multiple presets: Disabled, Light, Medium, Heavy" },
            { "Settings_Movement_Disabled", "Disabled" },
            { "Settings_Movement_Light", "Light Optimization" },
            { "Settings_Movement_Medium", "Medium Optimization" },
            { "Settings_Movement_Heavy", "Heavy Optimization" },

            // UI Enhancement Settings
            { "Settings_EnableWeaponComparison_Name", "Enable Weapon Comparison" },
            { "Settings_EnableWeaponComparison_Desc", "Compare selected weapon with hovered weapon in inventory, showing differences with color-coded indicators" },
            { "Settings_FastBuyEnabled_Name", "Enable Fast Buy" },
            { "Settings_FastBuyEnabled_Desc", "Quickly buy items by hovering over them in the shop menu and pressing F" },
            { "Settings_FastSellEnabled_Name", "Enable Fast Sell" },
            { "Settings_FastSellEnabled_Desc", "Quickly sell items by hovering over them in the shop menu and pressing F" },
            { "Settings_ItemWheelScale_Name", "Wheel Menu Scale" },
            { "Settings_ItemWheelScale_Desc", "Adjust the display size of all wheel menus (item wheel and throwable wheel)" },
            { "Settings_ItemWheelTimeScale_Name", "Wheel Menu Time Scale" },
            { "Settings_ItemWheelTimeScale_Desc", "Adjust the time scale of all wheel menus (item wheel and throwable wheel)" },
            { "Settings_ItemWheelSensitivity_Name", "Wheel Menu Sensitivity" },
            { "Settings_ItemWheelSensitivity_Desc", "Adjust the mouse sensitivity of all wheel menus (item wheel and throwable wheel)" },
            { "Settings_ItemWheelMenuHotkey_Name", "Item Wheel Hotkey" },
            { "Settings_ItemWheelMenuHotkey_Desc", "Press and release this key to open the item wheel menu" },
            { "Settings_ThrowableWheelEnabled_Name", "Enable Throwable Wheel" },
            { "Settings_ThrowableWheelEnabled_Desc", "Enable the throwable wheel menu feature" },
            { "Settings_ThrowableWheelHotkey_Name", "Throwable Wheel Hotkey" },
            { "Settings_ThrowableWheelHotkey_Desc", "Press and release this key to open the throwable wheel menu (default G key)" },
            { "Settings_PressAnyKey", "Press any key..." },

            { "Settings_AutoTrackNewQuests_Name", "Auto-track new accepted quests" },
            { "Settings_AutoTrackNewQuests_Desc", "Automatically add new accepted quests to the in-raid tracker" },
            { "Settings_EnableDuckShit_Name", "Enable Duck Shit" },
            { "Settings_EnableDuckShit_Desc", "Enable the feature where ducks automatically defecate based on energy and water consumption" },
        };

        // 日语
        LocalizationData[SystemLanguage.Japanese] = new Dictionary<string, string>
        {
            { "RaidCheck_Title", "レイド準備チェック" },
            { "RaidCheck_AllClear", "装備チェック完了" },
            { "RaidCheck_HasIssues", "次の問題が検出されました：\n" },
            { "RaidCheck_Confirm", "続行する" },
            { "RaidCheck_Cancel", "戻る" },

            { "Warning_NoWeapon", "<color=#FF6B6B>⚠ 武器を装備していません</color>" },
            { "Warning_NoAmmo", "<color=#FF6B6B>⚠ 弾薬がありません</color>" },
            { "Warning_NoMedicine", "<color=#FF6B6B>⚠ 医療品がありません</color>" },
            { "Warning_NoFood", "<color=#FF6B6B>⚠ 食料がありません</color>" },
            { "Warning_StormyWeather", "<color=#FF4444>⚠ 嵐の天候</color>" },
            { "Warning_StormComing", "<color=#FFA500>⚠ 嵐が接近中（24時間以内）</color>" },
            { "Warning_QuestItem", "<color=#FFD700>⚠ クエストアイテム不足: {0} ({1}/{2}) - {3}</color>" },
            { "Warning_LowAmmo", "<color=#FF8C00>⚠ 武器の弾薬不足: {0} ({1}) - {2}/{3} 発</color>" },
            { "Warning_QuestWeapon", "<color=#FFD700>⚠ クエスト必須武器: {0} - {1} ({2})</color>" },

            { "QuestTracker_Title", "アクティブクエスト" },
            { "QuestTracker_Progress", "進行状況: {0}/{1}" },
            { "QuestTracker_NoQuests", "進行中のクエストなし" },
            { "QuestTracker_TaskComplete", "✓" },
            { "QuestTracker_TaskPending", "○" },
            { "QuestTracker_CheckboxLabel", "レイド中追跡" },
            { "QuestTracker_HelpText", "「.」キーで表示切替\nゲーム設定のMod設定でパネル位置を調整できます" },

            // 設定UI
            { "Settings_Title", "EfD Enhanced 設定" },
            { "Settings_ResetButton", "デフォルトに戻す" },
            { "Settings_CloseButton", "閉じる (Alt+O)" },
            { "Settings_ModSettings_Button", "EfD Enhanced 設定" },

            // 設定カテゴリ
            { "Settings_Category_PreRaidCheck", "レイド前チェック" },
            { "Settings_Category_QuestTracker", "クエストトラッカー" },
            { "Settings_Category_Movement", "移動最適化" },
            { "Settings_Category_UI", "UI強化" },
            { "Settings_Category_FunFeatures", "おもしろ機能" },

            // Pre-Raid Check 設定
            { "Settings_EnableRaidCheck_Name", "レイド前チェックを有効化" },
            { "Settings_EnableRaidCheck_Desc", "レイド前チェックシステム全体を有効化" },
            { "Settings_CheckWeapon_Name", "武器チェック" },
            { "Settings_CheckWeapon_Desc", "武器を装備していない場合に警告" },
            { "Settings_CheckAmmo_Name", "弾薬チェック" },
            { "Settings_CheckAmmo_Desc", "弾薬がない場合に警告" },
            { "Settings_CheckMeds_Name", "医療品チェック" },
            { "Settings_CheckMeds_Desc", "医療品がない場合に警告" },
            { "Settings_CheckFood_Name", "食料/水チェック" },
            { "Settings_CheckFood_Desc", "食料または水がない場合に警告" },
            { "Settings_CheckWeather_Name", "嵐の天候警告" },
            { "Settings_CheckWeather_Desc", "嵐の天候状態を警告" },
            { "Settings_CheckQuestItems_Name", "クエストアイテムチェック" },
            { "Settings_CheckQuestItems_Desc", "必要なクエストアイテムが十分かどうかをチェック" },
            { "Settings_CheckQuestWeapons_Name", "クエスト武器チェック" },
            { "Settings_CheckQuestWeapons_Desc", "クエスト必須武器を装備しているかどうかをチェック" },

            // Quest Tracker 設定
            { "Settings_EnableQuestTracker_Name", "クエストトラッカーHUDを有効化" },
            { "Settings_EnableQuestTracker_Desc", "レイド中にアクティブクエストトラッカーを表示" },
            { "Settings_TrackerPositionX_Name", "トラッカー水平位置" },
            { "Settings_TrackerPositionX_Desc", "水平位置 (0=左, 1=右)" },
            { "Settings_TrackerPositionY_Name", "トラッカー垂直位置" },
            { "Settings_TrackerPositionY_Desc", "垂直位置 (0=上, 1=下)" },
            { "Settings_TrackerScale_Name", "トラッカースケール" },
            { "Settings_TrackerScale_Desc", "UIスケール倍率" },
            { "Settings_TrackerShowDescription_Name", "クエスト説明を表示" },
            { "Settings_TrackerShowDescription_Desc", "トラッカーにクエスト説明を表示" },
            { "Settings_TrackerFilterByMap_Name", "現在のマップのクエストのみ表示" },
            { "Settings_TrackerFilterByMap_Desc", "現在のマップに関連するクエストと、マップ制限のないクエストのみ表示" },
            { "Settings_TrackerToggleHotkey_Name", "クエストトラッカー折りたたみホットキー" },
            { "Settings_TrackerToggleHotkey_Desc", "このキーを押してクエストトラッカーリストを折りたたむ/展開する" },
            { "Settings_TrackerHotkeyUsed_Name", "ホットキー使用済みフラグ" },
            { "Settings_TrackerHotkeyUsed_Desc", "内部設定：ユーザーが表示/非表示ホットキーを使用したかどうかを示す" },

            // Movement Enhancement Settings
            { "Settings_MovementEnhancement_Name", "移動レスポンス強化" },
            { "Settings_MovementEnhancement_Desc", "キャラクター移動感を最適化し、粘着感を軽減。複数のプリセット：無効、軽量、中程度、重量" },
            { "Settings_Movement_Disabled", "無効" },
            { "Settings_Movement_Light", "軽量最適化" },
            { "Settings_Movement_Medium", "中程度最適化" },
            { "Settings_Movement_Heavy", "重量最適化" },

            // UI Enhancement Settings
            { "Settings_EnableWeaponComparison_Name", "武器比較を有効化" },
            { "Settings_EnableWeaponComparison_Desc", "インベントリで選択した武器とホバーした武器を比較し、差異を色付きインジケーターで表示" },
            { "Settings_FastBuyEnabled_Name", "快速購買を有効化" },
            { "Settings_FastBuyEnabled_Desc", "店舗メニューでアイテムをホバーしてFキーを押すと、アイテムを迅速に購入" },
            { "Settings_FastSellEnabled_Name", "快速販賣を有効化" },
            { "Settings_FastSellEnabled_Desc", "店舗メニューでアイテムをホバーしてFキーを押すと、アイテムを迅速に販売" },
            { "Settings_ItemWheelScale_Name", "ホイールメニュースケール" },
            { "Settings_ItemWheelScale_Desc", "すべてのホイールメニューの表示サイズを調整（アイテムホイールと投擲物ホイール）" },
            { "Settings_ItemWheelTimeScale_Name", "ホイールメニュー時間縮放" },
            { "Settings_ItemWheelTimeScale_Desc", "すべてのホイールメニューの時間縮放を調整（アイテムホイールと投擲物ホイール）" },
            { "Settings_ItemWheelSensitivity_Name", "ホイールメニュー感度" },
            { "Settings_ItemWheelSensitivity_Desc", "すべてのホイールメニューのマウス感度を調整（アイテムホイールと投擲物ホイール）" },
            { "Settings_ItemWheelMenuHotkey_Name", "アイテムホイールホットキー" },
            { "Settings_ItemWheelMenuHotkey_Desc", "このキーを押して離すとアイテムホイールメニューが開きます" },
            { "Settings_ThrowableWheelEnabled_Name", "投擲物ホイールを有効化" },
            { "Settings_ThrowableWheelEnabled_Desc", "投擲物ホイールメニュー機能を有効化" },
            { "Settings_ThrowableWheelHotkey_Name", "投擲物ホイールホットキー" },
            { "Settings_ThrowableWheelHotkey_Desc", "このキーを押して離すと投擲物ホイールメニューが開きます（デフォルトGキー）" },
            { "Settings_PressAnyKey", "任意のキーを押してください..." },

            { "Settings_AutoTrackNewQuests_Name", "新規クエストを自動追跡" },
            { "Settings_AutoTrackNewQuests_Desc", "新規クエストを受けた際に自動的に局内追跡リストに追加" },
            { "Settings_EnableDuckShit_Name", "アヒルの排便を有効化" },
            { "Settings_EnableDuckShit_Desc", "アヒルがエネルギーと水分消費に基づいて自動的に排便する機能を有効化" },
        };

        // 法语
        LocalizationData[SystemLanguage.French] = new Dictionary<string, string>
        {
            { "RaidCheck_Title", "Vérification avant Raid" },
            { "RaidCheck_AllClear", "Vérification de l'équipement réussie" },
            { "RaidCheck_HasIssues", "Les problèmes suivants détectés:\n" },
            { "RaidCheck_Confirm", "Continuer quand même" },
            { "RaidCheck_Cancel", "Retour" },

            { "Warning_NoWeapon", "<color=#FF6B6B>⚠ Aucune arme équipée</color>" },
            { "Warning_NoAmmo", "<color=#FF6B6B>⚠ Pas de munitions</color>" },
            { "Warning_NoMedicine", "<color=#FF6B6B>⚠ Pas de fournitures médicales</color>" },
            { "Warning_NoFood", "<color=#FF6B6B>⚠ Pas de nourriture ni de boissons</color>" },
            { "Warning_StormyWeather", "<color=#FF4444>⚠ Conditions météorologiques orageuses</color>" },
            { "Warning_StormComing", "<color=#FFA500>⚠ Tempête approchant (dans les 24 heures)</color>" },
            { "Warning_QuestItem", "<color=#FFD700>⚠ Objet de quête insuffisant: {0} ({1}/{2}) - {3}</color>" },
            { "Warning_LowAmmo", "<color=#FF8C00>⚠ Munitions faibles pour l'arme: {0} ({1}) - {2}/{3} coups</color>" },
            { "Warning_QuestWeapon", "<color=#FFD700>⚠ Arme requise pour la quête: {0} - {1} ({2})</color>" },

            { "QuestTracker_Title", "Quêtes actives" },
            { "QuestTracker_Progress", "Progression: {0}/{1}" },
            { "QuestTracker_NoQuests", "Aucune quête active" },
            { "QuestTracker_TaskComplete", "✓" },
            { "QuestTracker_TaskPending", "○" },
            { "QuestTracker_CheckboxLabel", "Suivre en Raid" },
            { "QuestTracker_HelpText", "Appuyez sur . pour masquer/afficher le suivi des quêtes\nLes paramètres du mod dans les paramètres du jeu peuvent ajuster la position du panneau" },

            // Settings UI
            { "Settings_Title", "Paramètres EfD Enhanced" },
            { "Settings_ResetButton", "Réinitialiser aux valeurs par défaut" },
            { "Settings_CloseButton", "Fermer" },
            { "Settings_ModSettings_Button", "Paramètres EfD Enhanced" },

            // Settings Categories
            { "Settings_Category_PreRaidCheck", "Vérification avant Raid" },
            { "Settings_Category_QuestTracker", "Suivi des quêtes" },
            { "Settings_Category_Movement", "Amélioration du mouvement" },
            { "Settings_Category_UI", "Amélioration de l'interface" },
            { "Settings_Category_FunFeatures", "Fonctionnalités amusantes" },

            // Pre-Raid Check Settings
            { "Settings_EnableRaidCheck_Name", "Activer la vérification avant Raid" },
            { "Settings_EnableRaidCheck_Desc", "Activer tout le système de vérification avant Raid" },
            { "Settings_CheckWeapon_Name", "Vérifier l'arme" },
            { "Settings_CheckWeapon_Desc", "Avertir si aucune arme n'est équipée" },
            { "Settings_CheckAmmo_Name", "Vérifier les munitions" },
            { "Settings_CheckAmmo_Desc", "Avertir si aucune munition n'est disponible" },
            { "Settings_CheckMeds_Name", "Vérifier les fournitures médicales" },
            { "Settings_CheckMeds_Desc", "Avertir si aucune fourniture médicale n'est disponible" },
            { "Settings_CheckFood_Name", "Vérifier la nourriture/l'eau" },
            { "Settings_CheckFood_Desc", "Avertir si aucune nourriture ni eau n'est disponible" },
            { "Settings_CheckWeather_Name", "Avertir conditions orageuses" },
            { "Settings_CheckWeather_Desc", "Avertir des conditions météorologiques orageuses" },
            { "Settings_CheckQuestItems_Name", "Vérifier les objets de quête" },
            { "Settings_CheckQuestItems_Desc", "Vérifier si les objets de quête requis sont suffisants" },
            { "Settings_CheckQuestWeapons_Name", "Vérifier les armes de quête" },
            { "Settings_CheckQuestWeapons_Desc", "Vérifier si les armes requises pour la quête sont équipées" },

            // Quest Tracker Settings
            { "Settings_EnableQuestTracker_Name", "Activer le HUD de suivi des quêtes" },
            { "Settings_EnableQuestTracker_Desc", "Afficher le suivi des quêtes actives pendant les raids" },
            { "Settings_TrackerPositionX_Name", "Position horizontale du suivi" },
            { "Settings_TrackerPositionX_Desc", "Position horizontale (0=gauche, 1=droite)" },
            { "Settings_TrackerPositionY_Name", "Position verticale du suivi" },
            { "Settings_TrackerPositionY_Desc", "Position verticale (0=haut, 1=bas)" },
            { "Settings_TrackerScale_Name", "Échelle du suivi" },
            { "Settings_TrackerScale_Desc", "Multiplicateur d'échelle de l'interface" },
            { "Settings_TrackerShowDescription_Name", "Afficher les descriptions des quêtes" },
            { "Settings_TrackerShowDescription_Desc", "Afficher les descriptions des quêtes dans le suivi" },
            { "Settings_TrackerFilterByMap_Name", "Afficher uniquement les quêtes de la carte actuelle" },
            { "Settings_TrackerFilterByMap_Desc", "Afficher uniquement les quêtes de la carte actuelle et les quêtes sans restriction de carte" },
            { "Settings_TrackerToggleHotkey_Name", "Raccourci réduction/expansion du suivi des quêtes" },
            { "Settings_TrackerToggleHotkey_Desc", "Appuyez sur cette touche pour réduire ou développer la liste de suivi des quêtes" },
            { "Settings_TrackerHotkeyUsed_Name", "Indicateur de raccourci utilisé" },
            { "Settings_TrackerHotkeyUsed_Desc", "Paramètre interne: Indique si l'utilisateur a utilisé le raccourci afficher/masquer" },

            // Movement Enhancement Settings
            { "Settings_MovementEnhancement_Name", "Amélioration de la réponse au mouvement" },
            { "Settings_MovementEnhancement_Desc", "Optimiser la sensation de mouvement du personnage, réduire le mouvement collant. Plusieurs préréglages: Désactivé, Léger, Moyen, Lourd" },
            { "Settings_Movement_Disabled", "Désactivé" },
            { "Settings_Movement_Light", "Optimisation légère" },
            { "Settings_Movement_Medium", "Optimisation moyenne" },
            { "Settings_Movement_Heavy", "Optimisation lourde" },

            // UI Enhancement Settings
            { "Settings_EnableWeaponComparison_Name", "Activer la comparaison d'armes" },
            { "Settings_EnableWeaponComparison_Desc", "Comparer l'arme sélectionnée avec l'arme survolée dans l'inventaire, affichant les différences avec des indicateurs codés par couleur" },
            { "Settings_FastBuyEnabled_Name", "Activer l'achat rapide" },
            { "Settings_FastBuyEnabled_Desc", "Acheter rapidement des objets en les survolant dans le menu de la boutique et en appuyant sur F" },
            { "Settings_FastSellEnabled_Name", "Activer la vente rapide" },
            { "Settings_FastSellEnabled_Desc", "Vendre rapidement des objets en les survolant dans le menu de la boutique et en appuyant sur F" },
            { "Settings_ItemWheelScale_Name", "Échelle du menu en roue" },
            { "Settings_ItemWheelScale_Desc", "Ajuster la taille d'affichage de tous les menus en roue (roue d'objets et roue de projectiles)" },
            { "Settings_ItemWheelTimeScale_Name", "Échelle de temps du menu en roue" },
            { "Settings_ItemWheelTimeScale_Desc", "Ajuster l'échelle de temps de tous les menus en roue (roue d'objets et roue de projectiles)" },
            { "Settings_ItemWheelSensitivity_Name", "Sensibilité du menu en roue" },
            { "Settings_ItemWheelSensitivity_Desc", "Ajuster la sensibilité de la souris de tous les menus en roue (roue d'objets et roue de projectiles)" },
            { "Settings_ItemWheelMenuHotkey_Name", "Raccourci de la roue d'objets" },
            { "Settings_ItemWheelMenuHotkey_Desc", "Appuyez et relâchez cette touche pour ouvrir le menu de la roue d'objets" },
            { "Settings_ThrowableWheelEnabled_Name", "Activer la roue de projectiles" },
            { "Settings_ThrowableWheelEnabled_Desc", "Activer la fonctionnalité du menu de la roue de projectiles" },
            { "Settings_ThrowableWheelHotkey_Name", "Raccourci de la roue de projectiles" },
            { "Settings_ThrowableWheelHotkey_Desc", "Appuyez et relâchez cette touche pour ouvrir le menu de la roue de projectiles (touche G par défaut)" },
            { "Settings_PressAnyKey", "Appuyez sur une touche..." },

            { "Settings_AutoTrackNewQuests_Name", "Suivre automatiquement les nouvelles quêtes acceptées" },
            { "Settings_AutoTrackNewQuests_Desc", "Ajouter automatiquement les nouvelles quêtes acceptées au suivi en raid" },
            { "Settings_EnableDuckShit_Name", "Activer la défécation des canards" },
            { "Settings_EnableDuckShit_Desc", "Activer la fonctionnalité où les canards défèquent automatiquement en fonction de la consommation d'énergie et d'eau" },
        };

        // 韩语
        LocalizationData[SystemLanguage.Korean] = new Dictionary<string, string>
        {
            { "RaidCheck_Title", "레이드 준비 확인" },
            { "RaidCheck_AllClear", "장비 확인 통과" },
            { "RaidCheck_HasIssues", "다음 문제가 감지되었습니다:\n" },
            { "RaidCheck_Confirm", "계속 진행" },
            { "RaidCheck_Cancel", "돌아가기" },

            { "Warning_NoWeapon", "<color=#FF6B6B>⚠ 무기가 장착되지 않음</color>" },
            { "Warning_NoAmmo", "<color=#FF6B6B>⚠ 탄약 없음</color>" },
            { "Warning_NoMedicine", "<color=#FF6B6B>⚠ 의료품 없음</color>" },
            { "Warning_NoFood", "<color=#FF6B6B>⚠ 음식 또는 음료 없음</color>" },
            { "Warning_StormyWeather", "<color=#FF4444>⚠ 폭풍 날씨 조건</color>" },
            { "Warning_StormComing", "<color=#FFA500>⚠ 폭풍 접근 중 (24시간 이내)</color>" },
            { "Warning_QuestItem", "<color=#FFD700>⚠ 퀘스트 아이템 부족: {0} ({1}/{2}) - {3}</color>" },
            { "Warning_LowAmmo", "<color=#FF8C00>⚠ 무기 탄약 부족: {0} ({1}) - {2}/{3} 발</color>" },
            { "Warning_QuestWeapon", "<color=#FFD700>⚠ 퀘스트 필수 무기: {0} - {1} ({2})</color>" },

            { "QuestTracker_Title", "활성 퀘스트" },
            { "QuestTracker_Progress", "진행도: {0}/{1}" },
            { "QuestTracker_NoQuests", "활성 퀘스트 없음" },
            { "QuestTracker_TaskComplete", "✓" },
            { "QuestTracker_TaskPending", "○" },
            { "QuestTracker_CheckboxLabel", "레이드에서 추적" },
            { "QuestTracker_HelpText", ". 키를 눌러 퀘스트 추적기 표시/숨기기\n게임 설정의 모드 설정에서 패널 위치를 조정할 수 있습니다" },

            // Settings UI
            { "Settings_Title", "EfD Enhanced 설정" },
            { "Settings_ResetButton", "기본값으로 재설정" },
            { "Settings_CloseButton", "닫기" },
            { "Settings_ModSettings_Button", "EfD Enhanced 설정" },

            // Settings Categories
            { "Settings_Category_PreRaidCheck", "레이드 전 확인" },
            { "Settings_Category_QuestTracker", "퀘스트 추적기" },
            { "Settings_Category_Movement", "이동 향상" },
            { "Settings_Category_UI", "UI 향상" },
            { "Settings_Category_FunFeatures", "재미있는 기능" },

            // Pre-Raid Check Settings
            { "Settings_EnableRaidCheck_Name", "레이드 전 확인 활성화" },
            { "Settings_EnableRaidCheck_Desc", "전체 레이드 전 확인 시스템 활성화" },
            { "Settings_CheckWeapon_Name", "무기 확인" },
            { "Settings_CheckWeapon_Desc", "무기가 장착되지 않은 경우 경고" },
            { "Settings_CheckAmmo_Name", "탄약 확인" },
            { "Settings_CheckAmmo_Desc", "탄약이 없는 경우 경고" },
            { "Settings_CheckMeds_Name", "의료품 확인" },
            { "Settings_CheckMeds_Desc", "의료품이 없는 경우 경고" },
            { "Settings_CheckFood_Name", "음식/물 확인" },
            { "Settings_CheckFood_Desc", "음식이나 물이 없는 경우 경고" },
            { "Settings_CheckWeather_Name", "폭풍 날씨 경고" },
            { "Settings_CheckWeather_Desc", "폭풍 날씨 조건에 대해 경고" },
            { "Settings_CheckQuestItems_Name", "퀘스트 아이템 확인" },
            { "Settings_CheckQuestItems_Desc", "필요한 퀘스트 아이템이 충분한지 확인" },
            { "Settings_CheckQuestWeapons_Name", "퀘스트 무기 확인" },
            { "Settings_CheckQuestWeapons_Desc", "퀘스트 필수 무기가 장착되었는지 확인" },

            // Quest Tracker Settings
            { "Settings_EnableQuestTracker_Name", "퀘스트 추적기 HUD 활성화" },
            { "Settings_EnableQuestTracker_Desc", "레이드 중 활성 퀘스트 추적기 표시" },
            { "Settings_TrackerPositionX_Name", "추적기 수평 위치" },
            { "Settings_TrackerPositionX_Desc", "수평 위치 (0=왼쪽, 1=오른쪽)" },
            { "Settings_TrackerPositionY_Name", "추적기 수직 위치" },
            { "Settings_TrackerPositionY_Desc", "수직 위치 (0=위, 1=아래)" },
            { "Settings_TrackerScale_Name", "추적기 크기" },
            { "Settings_TrackerScale_Desc", "UI 크기 배율" },
            { "Settings_TrackerShowDescription_Name", "퀘스트 설명 표시" },
            { "Settings_TrackerShowDescription_Desc", "추적기에 퀘스트 설명 표시" },
            { "Settings_TrackerFilterByMap_Name", "현재 지도의 퀘스트만 표시" },
            { "Settings_TrackerFilterByMap_Desc", "현재 지도와 관련된 퀘스트 및 지도 제한이 없는 퀘스트만 표시" },
            { "Settings_TrackerToggleHotkey_Name", "퀘스트 추적기 접기/펼치기 단축키" },
            { "Settings_TrackerToggleHotkey_Desc", "이 키를 눌러 퀘스트 추적기 목록 접기 또는 펼치기" },
            { "Settings_TrackerHotkeyUsed_Name", "단축키 사용 플래그" },
            { "Settings_TrackerHotkeyUsed_Desc", "내부 설정: 사용자가 표시/숨기기 단축키를 사용했는지 표시" },

            // Movement Enhancement Settings
            { "Settings_MovementEnhancement_Name", "이동 반응 향상" },
            { "Settings_MovementEnhancement_Desc", "캐릭터 이동 느낌 최적화, 끈적한 이동 감소. 여러 사전 설정: 비활성화, 경량, 중간, 무거움" },
            { "Settings_Movement_Disabled", "비활성화" },
            { "Settings_Movement_Light", "경량 최적화" },
            { "Settings_Movement_Medium", "중간 최적화" },
            { "Settings_Movement_Heavy", "무거운 최적화" },

            // UI Enhancement Settings
            { "Settings_EnableWeaponComparison_Name", "무기 비교 활성화" },
            { "Settings_EnableWeaponComparison_Desc", "인벤토리에서 선택한 무기와 마우스 오버한 무기를 비교하여 색상 코딩된 표시기로 차이점 표시" },
            { "Settings_FastBuyEnabled_Name", "빠른 구매 활성화" },
            { "Settings_FastBuyEnabled_Desc", "상점 메뉴에서 아이템에 마우스를 올리고 F 키를 눌러 빠르게 아이템 구매" },
            { "Settings_FastSellEnabled_Name", "빠른 판매 활성화" },
            { "Settings_FastSellEnabled_Desc", "상점 메뉴에서 아이템에 마우스를 올리고 F 키를 눌러 빠르게 아이템 판매" },
            { "Settings_ItemWheelScale_Name", "휠 메뉴 크기" },
            { "Settings_ItemWheelScale_Desc", "모든 휠 메뉴의 표시 크기 조정 (아이템 휠 및 투척물 휠)" },
            { "Settings_ItemWheelTimeScale_Name", "휠 메뉴 시간 크기" },
            { "Settings_ItemWheelTimeScale_Desc", "모든 휠 메뉴의 시간 크기 조정 (아이템 휠 및 투척물 휠)" },
            { "Settings_ItemWheelSensitivity_Name", "휠 메뉴 감도" },
            { "Settings_ItemWheelSensitivity_Desc", "모든 휠 메뉴의 마우스 감도 조정 (아이템 휠 및 투척물 휠)" },
            { "Settings_ItemWheelMenuHotkey_Name", "아이템 휠 단축키" },
            { "Settings_ItemWheelMenuHotkey_Desc", "이 키를 누르고 놓으면 아이템 휠 메뉴가 열립니다" },
            { "Settings_ThrowableWheelEnabled_Name", "투척물 휠 활성화" },
            { "Settings_ThrowableWheelEnabled_Desc", "투척물 휠 메뉴 기능 활성화" },
            { "Settings_ThrowableWheelHotkey_Name", "투척물 휠 단축키" },
            { "Settings_ThrowableWheelHotkey_Desc", "이 키를 누르고 놓으면 투척물 휠 메뉴가 열립니다 (기본값 G 키)" },
            { "Settings_PressAnyKey", "아무 키나 누르세요..." },

            { "Settings_AutoTrackNewQuests_Name", "새로 수락한 퀘스트 자동 추적" },
            { "Settings_AutoTrackNewQuests_Desc", "새로 수락한 퀘스트를 자동으로 레이드 추적 목록에 추가" },
            { "Settings_EnableDuckShit_Name", "오리 배변 활성화" },
            { "Settings_EnableDuckShit_Desc", "오리가 에너지 및 물 소비에 따라 자동으로 배변하는 기능 활성화" },
        };

        // 德语
        LocalizationData[SystemLanguage.German] = new Dictionary<string, string>
        {
            { "RaidCheck_Title", "Raid-Vorbereitungsprüfung" },
            { "RaidCheck_AllClear", "Ausrüstungsprüfung bestanden" },
            { "RaidCheck_HasIssues", "Folgende Probleme erkannt:\n" },
            { "RaidCheck_Confirm", "Trotzdem fortfahren" },
            { "RaidCheck_Cancel", "Zurück" },

            { "Warning_NoWeapon", "<color=#FF6B6B>⚠ Keine Waffe ausgerüstet</color>" },
            { "Warning_NoAmmo", "<color=#FF6B6B>⚠ Keine Munition</color>" },
            { "Warning_NoMedicine", "<color=#FF6B6B>⚠ Keine medizinischen Vorräte</color>" },
            { "Warning_NoFood", "<color=#FF6B6B>⚠ Kein Essen oder Getränke</color>" },
            { "Warning_StormyWeather", "<color=#FF4444>⚠ Stürmische Wetterbedingungen</color>" },
            { "Warning_StormComing", "<color=#FFA500>⚠ Sturm naht (innerhalb von 24 Stunden)</color>" },
            { "Warning_QuestItem", "<color=#FFD700>⚠ Quest-Gegenstand unzureichend: {0} ({1}/{2}) - {3}</color>" },
            { "Warning_LowAmmo", "<color=#FF8C00>⚠ Niedrige Munition für Waffe: {0} ({1}) - {2}/{3} Schuss</color>" },
            { "Warning_QuestWeapon", "<color=#FFD700>⚠ Quest-erforderliche Waffe: {0} - {1} ({2})</color>" },

            { "QuestTracker_Title", "Aktive Quests" },
            { "QuestTracker_Progress", "Fortschritt: {0}/{1}" },
            { "QuestTracker_NoQuests", "Keine aktiven Quests" },
            { "QuestTracker_TaskComplete", "✓" },
            { "QuestTracker_TaskPending", "○" },
            { "QuestTracker_CheckboxLabel", "Im Raid verfolgen" },
            { "QuestTracker_HelpText", "Drücke . zum Ein-/Ausblenden des Quest-Trackers\nMod-Einstellungen in den Spiel-Einstellungen können die Panel-Position anpassen" },

            // Settings UI
            { "Settings_Title", "EfD Enhanced Einstellungen" },
            { "Settings_ResetButton", "Auf Standard zurücksetzen" },
            { "Settings_CloseButton", "Schließen" },
            { "Settings_ModSettings_Button", "EfD Enhanced Einstellungen" },

            // Settings Categories
            { "Settings_Category_PreRaidCheck", "Vor-Raid-Prüfung" },
            { "Settings_Category_QuestTracker", "Quest-Tracker" },
            { "Settings_Category_Movement", "Bewegungsverbesserung" },
            { "Settings_Category_UI", "UI-Verbesserung" },
            { "Settings_Category_FunFeatures", "Spaß-Funktionen" },

            // Pre-Raid Check Settings
            { "Settings_EnableRaidCheck_Name", "Vor-Raid-Prüfung aktivieren" },
            { "Settings_EnableRaidCheck_Desc", "Das gesamte Vor-Raid-Prüfungssystem aktivieren" },
            { "Settings_CheckWeapon_Name", "Waffe prüfen" },
            { "Settings_CheckWeapon_Desc", "Warnen, wenn keine Waffe ausgerüstet ist" },
            { "Settings_CheckAmmo_Name", "Munition prüfen" },
            { "Settings_CheckAmmo_Desc", "Warnen, wenn keine Munition verfügbar ist" },
            { "Settings_CheckMeds_Name", "Medizinische Vorräte prüfen" },
            { "Settings_CheckMeds_Desc", "Warnen, wenn keine medizinischen Vorräte verfügbar sind" },
            { "Settings_CheckFood_Name", "Essen/Wasser prüfen" },
            { "Settings_CheckFood_Desc", "Warnen, wenn kein Essen oder Wasser verfügbar ist" },
            { "Settings_CheckWeather_Name", "Vor stürmischem Wetter warnen" },
            { "Settings_CheckWeather_Desc", "Vor stürmischen Wetterbedingungen warnen" },
            { "Settings_CheckQuestItems_Name", "Quest-Gegenstände prüfen" },
            { "Settings_CheckQuestItems_Desc", "Prüfen, ob erforderliche Quest-Gegenstände ausreichend sind" },
            { "Settings_CheckQuestWeapons_Name", "Quest-Waffen prüfen" },
            { "Settings_CheckQuestWeapons_Desc", "Prüfen, ob Quest-erforderliche Waffen ausgerüstet sind" },

            // Quest Tracker Settings
            { "Settings_EnableQuestTracker_Name", "Quest-Tracker HUD aktivieren" },
            { "Settings_EnableQuestTracker_Desc", "Aktiven Quest-Tracker während Raids anzeigen" },
            { "Settings_TrackerPositionX_Name", "Tracker horizontale Position" },
            { "Settings_TrackerPositionX_Desc", "Horizontale Position (0=links, 1=rechts)" },
            { "Settings_TrackerPositionY_Name", "Tracker vertikale Position" },
            { "Settings_TrackerPositionY_Desc", "Vertikale Position (0=oben, 1=unten)" },
            { "Settings_TrackerScale_Name", "Tracker-Skalierung" },
            { "Settings_TrackerScale_Desc", "UI-Skalierungsmultiplikator" },
            { "Settings_TrackerShowDescription_Name", "Quest-Beschreibungen anzeigen" },
            { "Settings_TrackerShowDescription_Desc", "Quest-Beschreibungen im Tracker anzeigen" },
            { "Settings_TrackerFilterByMap_Name", "Nur Quests der aktuellen Karte anzeigen" },
            { "Settings_TrackerFilterByMap_Desc", "Nur Quests für die aktuelle Karte und Quests ohne Kartenanforderungen anzeigen" },
            { "Settings_TrackerToggleHotkey_Name", "Quest-Tracker Ein-/Ausklappen Tastenkürzel" },
            { "Settings_TrackerToggleHotkey_Desc", "Diese Taste drücken, um die Quest-Tracker-Liste ein- oder auszuklappen" },
            { "Settings_TrackerHotkeyUsed_Name", "Tastenkürzel verwendet Flagge" },
            { "Settings_TrackerHotkeyUsed_Desc", "Interne Einstellung: Markiert, ob der Benutzer das Ein-/Ausblenden-Tastenkürzel verwendet hat" },

            // Movement Enhancement Settings
            { "Settings_MovementEnhancement_Name", "Bewegungsreaktionsverbesserung" },
            { "Settings_MovementEnhancement_Desc", "Charakterbewegungsgefühl optimieren, klebrige Bewegung reduzieren. Mehrere Voreinstellungen: Deaktiviert, Leicht, Mittel, Schwer" },
            { "Settings_Movement_Disabled", "Deaktiviert" },
            { "Settings_Movement_Light", "Leichte Optimierung" },
            { "Settings_Movement_Medium", "Mittlere Optimierung" },
            { "Settings_Movement_Heavy", "Schwere Optimierung" },

            // UI Enhancement Settings
            { "Settings_EnableWeaponComparison_Name", "Waffenvergleich aktivieren" },
            { "Settings_EnableWeaponComparison_Desc", "Ausgewählte Waffe mit überfahrener Waffe im Inventar vergleichen, Unterschiede mit farbcodierten Indikatoren anzeigen" },
            { "Settings_FastBuyEnabled_Name", "Schnellkauf aktivieren" },
            { "Settings_FastBuyEnabled_Desc", "Gegenstände schnell kaufen, indem man sie im Ladenmenü überfährt und F drückt" },
            { "Settings_FastSellEnabled_Name", "Schnellverkauf aktivieren" },
            { "Settings_FastSellEnabled_Desc", "Gegenstände schnell verkaufen, indem man sie im Ladenmenü überfährt und F drückt" },
            { "Settings_ItemWheelScale_Name", "Radmenü-Skalierung" },
            { "Settings_ItemWheelScale_Desc", "Anzeigegröße aller Radmenüs anpassen (Gegenstandsrad und Wurfobjektrad)" },
            { "Settings_ItemWheelTimeScale_Name", "Radmenü-Zeitskalierung" },
            { "Settings_ItemWheelTimeScale_Desc", "Zeitskalierung aller Radmenüs anpassen (Gegenstandsrad und Wurfobjektrad)" },
            { "Settings_ItemWheelSensitivity_Name", "Radmenü-Empfindlichkeit" },
            { "Settings_ItemWheelSensitivity_Desc", "Mausempfindlichkeit aller Radmenüs anpassen (Gegenstandsrad und Wurfobjektrad)" },
            { "Settings_ItemWheelMenuHotkey_Name", "Gegenstandsrad Tastenkürzel" },
            { "Settings_ItemWheelMenuHotkey_Desc", "Diese Taste drücken und loslassen, um das Gegenstandsradmenü zu öffnen" },
            { "Settings_ThrowableWheelEnabled_Name", "Wurfobjektrad aktivieren" },
            { "Settings_ThrowableWheelEnabled_Desc", "Wurfobjektradmenü-Funktion aktivieren" },
            { "Settings_ThrowableWheelHotkey_Name", "Wurfobjektrad Tastenkürzel" },
            { "Settings_ThrowableWheelHotkey_Desc", "Diese Taste drücken und loslassen, um das Wurfobjektradmenü zu öffnen (Standard: G-Taste)" },
            { "Settings_PressAnyKey", "Beliebige Taste drücken..." },

            { "Settings_AutoTrackNewQuests_Name", "Neu akzeptierte Quests automatisch verfolgen" },
            { "Settings_AutoTrackNewQuests_Desc", "Neu akzeptierte Quests automatisch zur Raid-Tracker-Liste hinzufügen" },
            { "Settings_EnableDuckShit_Name", "Enten-Kot aktivieren" },
            { "Settings_EnableDuckShit_Desc", "Funktion aktivieren, bei der Enten automatisch koten, basierend auf Energie- und Wasserverbrauch" },
        };

        // 西班牙语
        LocalizationData[SystemLanguage.Spanish] = new Dictionary<string, string>
        {
            { "RaidCheck_Title", "Verificación de Preparación para Raid" },
            { "RaidCheck_AllClear", "Verificación de equipo aprobada" },
            { "RaidCheck_HasIssues", "Se detectaron los siguientes problemas:\n" },
            { "RaidCheck_Confirm", "Continuar de todos modos" },
            { "RaidCheck_Cancel", "Volver" },

            { "Warning_NoWeapon", "<color=#FF6B6B>⚠ No hay arma equipada</color>" },
            { "Warning_NoAmmo", "<color=#FF6B6B>⚠ No hay munición</color>" },
            { "Warning_NoMedicine", "<color=#FF6B6B>⚠ No hay suministros médicos</color>" },
            { "Warning_NoFood", "<color=#FF6B6B>⚠ No hay comida ni bebidas</color>" },
            { "Warning_StormyWeather", "<color=#FF4444>⚠ Condiciones climáticas tormentosas</color>" },
            { "Warning_StormComing", "<color=#FFA500>⚠ Tormenta aproximándose (dentro de 24 horas)</color>" },
            { "Warning_QuestItem", "<color=#FFD700>⚠ Objeto de misión insuficiente: {0} ({1}/{2}) - {3}</color>" },
            { "Warning_LowAmmo", "<color=#FF8C00>⚠ Munición baja para arma: {0} ({1}) - {2}/{3} rondas</color>" },
            { "Warning_QuestWeapon", "<color=#FFD700>⚠ Arma requerida para misión: {0} - {1} ({2})</color>" },

            { "QuestTracker_Title", "Misiones Activas" },
            { "QuestTracker_Progress", "Progreso: {0}/{1}" },
            { "QuestTracker_NoQuests", "No hay misiones activas" },
            { "QuestTracker_TaskComplete", "✓" },
            { "QuestTracker_TaskPending", "○" },
            { "QuestTracker_CheckboxLabel", "Rastrear en Raid" },
            { "QuestTracker_HelpText", "Presiona . para ocultar/mostrar el rastreador de misiones\nLa configuración del mod en la configuración del juego puede ajustar la posición del panel" },

            // Settings UI
            { "Settings_Title", "Configuración EfD Enhanced" },
            { "Settings_ResetButton", "Restablecer a valores predeterminados" },
            { "Settings_CloseButton", "Cerrar" },
            { "Settings_ModSettings_Button", "Configuración EfD Enhanced" },

            // Settings Categories
            { "Settings_Category_PreRaidCheck", "Verificación Pre-Raid" },
            { "Settings_Category_QuestTracker", "Rastreador de Misiones" },
            { "Settings_Category_Movement", "Mejora de Movimiento" },
            { "Settings_Category_UI", "Mejora de Interfaz" },
            { "Settings_Category_FunFeatures", "Características Divertidas" },

            // Pre-Raid Check Settings
            { "Settings_EnableRaidCheck_Name", "Activar Verificación Pre-Raid" },
            { "Settings_EnableRaidCheck_Desc", "Activar todo el sistema de verificación pre-raid" },
            { "Settings_CheckWeapon_Name", "Verificar Arma" },
            { "Settings_CheckWeapon_Desc", "Advertir si no hay arma equipada" },
            { "Settings_CheckAmmo_Name", "Verificar Munición" },
            { "Settings_CheckAmmo_Desc", "Advertir si no hay munición disponible" },
            { "Settings_CheckMeds_Name", "Verificar Suministros Médicos" },
            { "Settings_CheckMeds_Desc", "Advertir si no hay suministros médicos disponibles" },
            { "Settings_CheckFood_Name", "Verificar Comida/Agua" },
            { "Settings_CheckFood_Desc", "Advertir si no hay comida o agua disponible" },
            { "Settings_CheckWeather_Name", "Advertir Clima Tormentoso" },
            { "Settings_CheckWeather_Desc", "Advertir sobre condiciones climáticas tormentosas" },
            { "Settings_CheckQuestItems_Name", "Verificar Objetos de Misión" },
            { "Settings_CheckQuestItems_Desc", "Verificar si los objetos de misión requeridos son suficientes" },
            { "Settings_CheckQuestWeapons_Name", "Verificar Armas de Misión" },
            { "Settings_CheckQuestWeapons_Desc", "Verificar si las armas requeridas para la misión están equipadas" },

            // Quest Tracker Settings
            { "Settings_EnableQuestTracker_Name", "Activar HUD de Rastreador de Misiones" },
            { "Settings_EnableQuestTracker_Desc", "Mostrar rastreador de misiones activas durante los raids" },
            { "Settings_TrackerPositionX_Name", "Posición Horizontal del Rastreador" },
            { "Settings_TrackerPositionX_Desc", "Posición horizontal (0=izquierda, 1=derecha)" },
            { "Settings_TrackerPositionY_Name", "Posición Vertical del Rastreador" },
            { "Settings_TrackerPositionY_Desc", "Posición vertical (0=arriba, 1=abajo)" },
            { "Settings_TrackerScale_Name", "Escala del Rastreador" },
            { "Settings_TrackerScale_Desc", "Multiplicador de escala de interfaz" },
            { "Settings_TrackerShowDescription_Name", "Mostrar Descripciones de Misiones" },
            { "Settings_TrackerShowDescription_Desc", "Mostrar descripciones de misiones en el rastreador" },
            { "Settings_TrackerFilterByMap_Name", "Mostrar Solo Misiones del Mapa Actual" },
            { "Settings_TrackerFilterByMap_Desc", "Mostrar solo misiones del mapa actual y misiones sin requisitos de mapa" },
            { "Settings_TrackerToggleHotkey_Name", "Atajo de Colapsar/Expandir Rastreador de Misiones" },
            { "Settings_TrackerToggleHotkey_Desc", "Presiona esta tecla para colapsar o expandir la lista del rastreador de misiones" },
            { "Settings_TrackerHotkeyUsed_Name", "Marcador de Atajo Usado" },
            { "Settings_TrackerHotkeyUsed_Desc", "Configuración interna: Marca si el usuario ha usado el atajo de mostrar/ocultar" },

            // Movement Enhancement Settings
            { "Settings_MovementEnhancement_Name", "Mejora de Respuesta de Movimiento" },
            { "Settings_MovementEnhancement_Desc", "Optimizar la sensación de movimiento del personaje, reducir el movimiento pegajoso. Múltiples ajustes preestablecidos: Desactivado, Ligero, Medio, Pesado" },
            { "Settings_Movement_Disabled", "Desactivado" },
            { "Settings_Movement_Light", "Optimización Ligera" },
            { "Settings_Movement_Medium", "Optimización Media" },
            { "Settings_Movement_Heavy", "Optimización Pesada" },

            // UI Enhancement Settings
            { "Settings_EnableWeaponComparison_Name", "Activar Comparación de Armas" },
            { "Settings_EnableWeaponComparison_Desc", "Comparar arma seleccionada con arma sobre la que se pasa el cursor en el inventario, mostrando diferencias con indicadores codificados por color" },
            { "Settings_FastBuyEnabled_Name", "Activar Compra Rápida" },
            { "Settings_FastBuyEnabled_Desc", "Comprar objetos rápidamente pasando el cursor sobre ellos en el menú de la tienda y presionando F" },
            { "Settings_FastSellEnabled_Name", "Activar Venta Rápida" },
            { "Settings_FastSellEnabled_Desc", "Vender objetos rápidamente pasando el cursor sobre ellos en el menú de la tienda y presionando F" },
            { "Settings_ItemWheelScale_Name", "Escala del Menú de Rueda" },
            { "Settings_ItemWheelScale_Desc", "Ajustar el tamaño de visualización de todos los menús de rueda (rueda de objetos y rueda de lanzables)" },
            { "Settings_ItemWheelTimeScale_Name", "Escala de Tiempo del Menú de Rueda" },
            { "Settings_ItemWheelTimeScale_Desc", "Ajustar la escala de tiempo de todos los menús de rueda (rueda de objetos y rueda de lanzables)" },
            { "Settings_ItemWheelSensitivity_Name", "Sensibilidad del Menú de Rueda" },
            { "Settings_ItemWheelSensitivity_Desc", "Ajustar la sensibilidad del mouse de todos los menús de rueda (rueda de objetos y rueda de lanzables)" },
            { "Settings_ItemWheelMenuHotkey_Name", "Atajo de Rueda de Objetos" },
            { "Settings_ItemWheelMenuHotkey_Desc", "Presiona y suelta esta tecla para abrir el menú de la rueda de objetos" },
            { "Settings_ThrowableWheelEnabled_Name", "Activar Rueda de Lanzables" },
            { "Settings_ThrowableWheelEnabled_Desc", "Activar la función del menú de rueda de lanzables" },
            { "Settings_ThrowableWheelHotkey_Name", "Atajo de Rueda de Lanzables" },
            { "Settings_ThrowableWheelHotkey_Desc", "Presiona y suelta esta tecla para abrir el menú de la rueda de lanzables (tecla G por defecto)" },
            { "Settings_PressAnyKey", "Presiona cualquier tecla..." },

            { "Settings_AutoTrackNewQuests_Name", "Rastrear automáticamente misiones aceptadas nuevas" },
            { "Settings_AutoTrackNewQuests_Desc", "Agregar automáticamente misiones aceptadas nuevas a la lista de rastreo en raid" },
            { "Settings_EnableDuckShit_Name", "Activar Caca de Pato" },
            { "Settings_EnableDuckShit_Desc", "Activar la función donde los patos defecan automáticamente basándose en el consumo de energía y agua" },
        };

        // 俄语
        LocalizationData[SystemLanguage.Russian] = new Dictionary<string, string>
        {
            { "RaidCheck_Title", "Проверка готовности к рейду" },
            { "RaidCheck_AllClear", "Проверка снаряжения пройдена" },
            { "RaidCheck_HasIssues", "Обнаружены следующие проблемы:\n" },
            { "RaidCheck_Confirm", "Продолжить" },
            { "RaidCheck_Cancel", "Назад" },

            { "Warning_NoWeapon", "<color=#FF6B6B>⚠ Оружие не экипировано</color>" },
            { "Warning_NoAmmo", "<color=#FF6B6B>⚠ Нет боеприпасов</color>" },
            { "Warning_NoMedicine", "<color=#FF6B6B>⚠ Нет медицинских принадлежностей</color>" },
            { "Warning_NoFood", "<color=#FF6B6B>⚠ Нет еды или напитков</color>" },
            { "Warning_StormyWeather", "<color=#FF4444>⚠ Штормовые погодные условия</color>" },
            { "Warning_StormComing", "<color=#FFA500>⚠ Приближается шторм (в течение 24 часов)</color>" },
            { "Warning_QuestItem", "<color=#FFD700>⚠ Недостаточно предмета квеста: {0} ({1}/{2}) - {3}</color>" },
            { "Warning_LowAmmo", "<color=#FF8C00>⚠ Мало боеприпасов для оружия: {0} ({1}) - {2}/{3} патронов</color>" },
            { "Warning_QuestWeapon", "<color=#FFD700>⚠ Требуемое для квеста оружие: {0} - {1} ({2})</color>" },

            { "QuestTracker_Title", "Активные квесты" },
            { "QuestTracker_Progress", "Прогресс: {0}/{1}" },
            { "QuestTracker_NoQuests", "Нет активных квестов" },
            { "QuestTracker_TaskComplete", "✓" },
            { "QuestTracker_TaskPending", "○" },
            { "QuestTracker_CheckboxLabel", "Отслеживать в рейде" },
            { "QuestTracker_HelpText", "Нажмите . чтобы скрыть/показать трекер квестов\nНастройки мода в настройках игры могут изменять позицию панели" },

            // Settings UI
            { "Settings_Title", "Настройки EfD Enhanced" },
            { "Settings_ResetButton", "Сбросить на значения по умолчанию" },
            { "Settings_CloseButton", "Закрыть" },
            { "Settings_ModSettings_Button", "Настройки EfD Enhanced" },

            // Settings Categories
            { "Settings_Category_PreRaidCheck", "Проверка перед рейдом" },
            { "Settings_Category_QuestTracker", "Трекер квестов" },
            { "Settings_Category_Movement", "Улучшение движения" },
            { "Settings_Category_UI", "Улучшение интерфейса" },
            { "Settings_Category_FunFeatures", "Забавные функции" },

            // Pre-Raid Check Settings
            { "Settings_EnableRaidCheck_Name", "Включить проверку перед рейдом" },
            { "Settings_EnableRaidCheck_Desc", "Включить всю систему проверки перед рейдом" },
            { "Settings_CheckWeapon_Name", "Проверять оружие" },
            { "Settings_CheckWeapon_Desc", "Предупреждать, если оружие не экипировано" },
            { "Settings_CheckAmmo_Name", "Проверять боеприпасы" },
            { "Settings_CheckAmmo_Desc", "Предупреждать, если нет боеприпасов" },
            { "Settings_CheckMeds_Name", "Проверять медицинские принадлежности" },
            { "Settings_CheckMeds_Desc", "Предупреждать, если нет медицинских принадлежностей" },
            { "Settings_CheckFood_Name", "Проверять еду/воду" },
            { "Settings_CheckFood_Desc", "Предупреждать, если нет еды или воды" },
            { "Settings_CheckWeather_Name", "Предупреждать о штормовой погоде" },
            { "Settings_CheckWeather_Desc", "Предупреждать о штормовых погодных условиях" },
            { "Settings_CheckQuestItems_Name", "Проверять предметы квестов" },
            { "Settings_CheckQuestItems_Desc", "Проверять, достаточно ли требуемых предметов квестов" },
            { "Settings_CheckQuestWeapons_Name", "Проверять оружие квестов" },
            { "Settings_CheckQuestWeapons_Desc", "Проверять, экипировано ли требуемое для квеста оружие" },

            // Quest Tracker Settings
            { "Settings_EnableQuestTracker_Name", "Включить HUD трекера квестов" },
            { "Settings_EnableQuestTracker_Desc", "Показывать трекер активных квестов во время рейдов" },
            { "Settings_TrackerPositionX_Name", "Горизонтальная позиция трекера" },
            { "Settings_TrackerPositionX_Desc", "Горизонтальная позиция (0=слева, 1=справа)" },
            { "Settings_TrackerPositionY_Name", "Вертикальная позиция трекера" },
            { "Settings_TrackerPositionY_Desc", "Вертикальная позиция (0=сверху, 1=снизу)" },
            { "Settings_TrackerScale_Name", "Масштаб трекера" },
            { "Settings_TrackerScale_Desc", "Множитель масштаба интерфейса" },
            { "Settings_TrackerShowDescription_Name", "Показывать описания квестов" },
            { "Settings_TrackerShowDescription_Desc", "Отображать описания квестов в трекере" },
            { "Settings_TrackerFilterByMap_Name", "Показывать только квесты текущей карты" },
            { "Settings_TrackerFilterByMap_Desc", "Показывать только квесты текущей карты и квесты без требований к карте" },
            { "Settings_TrackerToggleHotkey_Name", "Горячая клавиша сворачивания/разворачивания трекера квестов" },
            { "Settings_TrackerToggleHotkey_Desc", "Нажмите эту клавишу, чтобы свернуть или развернуть список трекера квестов" },
            { "Settings_TrackerHotkeyUsed_Name", "Флаг использования горячей клавиши" },
            { "Settings_TrackerHotkeyUsed_Desc", "Внутренняя настройка: Отмечает, использовал ли пользователь горячую клавишу показа/скрытия" },

            // Movement Enhancement Settings
            { "Settings_MovementEnhancement_Name", "Улучшение отклика движения" },
            { "Settings_MovementEnhancement_Desc", "Оптимизировать ощущение движения персонажа, уменьшить липкое движение. Несколько предустановок: Отключено, Легкая, Средняя, Тяжелая" },
            { "Settings_Movement_Disabled", "Отключено" },
            { "Settings_Movement_Light", "Легкая оптимизация" },
            { "Settings_Movement_Medium", "Средняя оптимизация" },
            { "Settings_Movement_Heavy", "Тяжелая оптимизация" },

            // UI Enhancement Settings
            { "Settings_EnableWeaponComparison_Name", "Включить сравнение оружия" },
            { "Settings_EnableWeaponComparison_Desc", "Сравнивать выбранное оружие с оружием при наведении в инвентаре, показывая различия с цветовыми индикаторами" },
            { "Settings_FastBuyEnabled_Name", "Включить быструю покупку" },
            { "Settings_FastBuyEnabled_Desc", "Быстро покупать предметы, наводя на них в меню магазина и нажимая F" },
            { "Settings_FastSellEnabled_Name", "Включить быструю продажу" },
            { "Settings_FastSellEnabled_Desc", "Быстро продавать предметы, наводя на них в меню магазина и нажимая F" },
            { "Settings_ItemWheelScale_Name", "Масштаб колесного меню" },
            { "Settings_ItemWheelScale_Desc", "Настроить размер отображения всех колесных меню (колесо предметов и колесо метательных)" },
            { "Settings_ItemWheelTimeScale_Name", "Временной масштаб колесного меню" },
            { "Settings_ItemWheelTimeScale_Desc", "Настроить временной масштаб всех колесных меню (колесо предметов и колесо метательных)" },
            { "Settings_ItemWheelSensitivity_Name", "Чувствительность колесного меню" },
            { "Settings_ItemWheelSensitivity_Desc", "Настроить чувствительность мыши всех колесных меню (колесо предметов и колесо метательных)" },
            { "Settings_ItemWheelMenuHotkey_Name", "Горячая клавиша колеса предметов" },
            { "Settings_ItemWheelMenuHotkey_Desc", "Нажмите и отпустите эту клавишу, чтобы открыть меню колеса предметов" },
            { "Settings_ThrowableWheelEnabled_Name", "Включить колесо метательных" },
            { "Settings_ThrowableWheelEnabled_Desc", "Включить функцию меню колеса метательных" },
            { "Settings_ThrowableWheelHotkey_Name", "Горячая клавиша колеса метательных" },
            { "Settings_ThrowableWheelHotkey_Desc", "Нажмите и отпустите эту клавишу, чтобы открыть меню колеса метательных (по умолчанию клавиша G)" },
            { "Settings_PressAnyKey", "Нажмите любую клавишу..." },

            { "Settings_AutoTrackNewQuests_Name", "Автоматически отслеживать новые принятые квесты" },
            { "Settings_AutoTrackNewQuests_Desc", "Автоматически добавлять новые принятые квесты в список отслеживания рейда" },
            { "Settings_EnableDuckShit_Name", "Включить утиный помет" },
            { "Settings_EnableDuckShit_Desc", "Включить функцию, при которой утки автоматически испражняются на основе потребления энергии и воды" },
        };

        // 葡萄牙语
        LocalizationData[SystemLanguage.Portuguese] = new Dictionary<string, string>
        {
            { "RaidCheck_Title", "Verificação de Preparação para Raid" },
            { "RaidCheck_AllClear", "Verificação de equipamento aprovada" },
            { "RaidCheck_HasIssues", "Os seguintes problemas foram detectados:\n" },
            { "RaidCheck_Confirm", "Continuar mesmo assim" },
            { "RaidCheck_Cancel", "Voltar" },

            { "Warning_NoWeapon", "<color=#FF6B6B>⚠ Nenhuma arma equipada</color>" },
            { "Warning_NoAmmo", "<color=#FF6B6B>⚠ Sem munição</color>" },
            { "Warning_NoMedicine", "<color=#FF6B6B>⚠ Sem suprimentos médicos</color>" },
            { "Warning_NoFood", "<color=#FF6B6B>⚠ Sem comida ou bebidas</color>" },
            { "Warning_StormyWeather", "<color=#FF4444>⚠ Condições climáticas tempestuosas</color>" },
            { "Warning_StormComing", "<color=#FFA500>⚠ Tempestade se aproximando (dentro de 24 horas)</color>" },
            { "Warning_QuestItem", "<color=#FFD700>⚠ Item de missão insuficiente: {0} ({1}/{2}) - {3}</color>" },
            { "Warning_LowAmmo", "<color=#FF8C00>⚠ Munição baixa para arma: {0} ({1}) - {2}/{3} tiros</color>" },
            { "Warning_QuestWeapon", "<color=#FFD700>⚠ Arma necessária para missão: {0} - {1} ({2})</color>" },

            { "QuestTracker_Title", "Missões Ativas" },
            { "QuestTracker_Progress", "Progresso: {0}/{1}" },
            { "QuestTracker_NoQuests", "Nenhuma missão ativa" },
            { "QuestTracker_TaskComplete", "✓" },
            { "QuestTracker_TaskPending", "○" },
            { "QuestTracker_CheckboxLabel", "Rastrear no Raid" },
            { "QuestTracker_HelpText", "Pressione . para ocultar/mostrar o rastreador de missões\nAs configurações do mod nas configurações do jogo podem ajustar a posição do painel" },

            // Settings UI
            { "Settings_Title", "Configurações EfD Enhanced" },
            { "Settings_ResetButton", "Redefinir para padrões" },
            { "Settings_CloseButton", "Fechar" },
            { "Settings_ModSettings_Button", "Configurações EfD Enhanced" },

            // Settings Categories
            { "Settings_Category_PreRaidCheck", "Verificação Pré-Raid" },
            { "Settings_Category_QuestTracker", "Rastreador de Missões" },
            { "Settings_Category_Movement", "Melhoria de Movimento" },
            { "Settings_Category_UI", "Melhoria de Interface" },
            { "Settings_Category_FunFeatures", "Recursos Divertidos" },

            // Pre-Raid Check Settings
            { "Settings_EnableRaidCheck_Name", "Ativar Verificação Pré-Raid" },
            { "Settings_EnableRaidCheck_Desc", "Ativar todo o sistema de verificação pré-raid" },
            { "Settings_CheckWeapon_Name", "Verificar Arma" },
            { "Settings_CheckWeapon_Desc", "Avisar se nenhuma arma estiver equipada" },
            { "Settings_CheckAmmo_Name", "Verificar Munição" },
            { "Settings_CheckAmmo_Desc", "Avisar se não houver munição disponível" },
            { "Settings_CheckMeds_Name", "Verificar Suprimentos Médicos" },
            { "Settings_CheckMeds_Desc", "Avisar se não houver suprimentos médicos disponíveis" },
            { "Settings_CheckFood_Name", "Verificar Comida/Água" },
            { "Settings_CheckFood_Desc", "Avisar se não houver comida ou água disponível" },
            { "Settings_CheckWeather_Name", "Avisar Clima Tempestuoso" },
            { "Settings_CheckWeather_Desc", "Avisar sobre condições climáticas tempestuosas" },
            { "Settings_CheckQuestItems_Name", "Verificar Itens de Missão" },
            { "Settings_CheckQuestItems_Desc", "Verificar se os itens de missão necessários são suficientes" },
            { "Settings_CheckQuestWeapons_Name", "Verificar Armas de Missão" },
            { "Settings_CheckQuestWeapons_Desc", "Verificar se as armas necessárias para a missão estão equipadas" },

            // Quest Tracker Settings
            { "Settings_EnableQuestTracker_Name", "Ativar HUD do Rastreador de Missões" },
            { "Settings_EnableQuestTracker_Desc", "Mostrar rastreador de missões ativas durante raids" },
            { "Settings_TrackerPositionX_Name", "Posição Horizontal do Rastreador" },
            { "Settings_TrackerPositionX_Desc", "Posição horizontal (0=esquerda, 1=direita)" },
            { "Settings_TrackerPositionY_Name", "Posição Vertical do Rastreador" },
            { "Settings_TrackerPositionY_Desc", "Posição vertical (0=topo, 1=base)" },
            { "Settings_TrackerScale_Name", "Escala do Rastreador" },
            { "Settings_TrackerScale_Desc", "Multiplicador de escala da interface" },
            { "Settings_TrackerShowDescription_Name", "Mostrar Descrições de Missões" },
            { "Settings_TrackerShowDescription_Desc", "Exibir descrições de missões no rastreador" },
            { "Settings_TrackerFilterByMap_Name", "Mostrar Apenas Missões do Mapa Atual" },
            { "Settings_TrackerFilterByMap_Desc", "Mostrar apenas missões do mapa atual e missões sem requisitos de mapa" },
            { "Settings_TrackerToggleHotkey_Name", "Atalho de Recolher/Expandir Rastreador de Missões" },
            { "Settings_TrackerToggleHotkey_Desc", "Pressione esta tecla para recolher ou expandir a lista do rastreador de missões" },
            { "Settings_TrackerHotkeyUsed_Name", "Marcador de Atalho Usado" },
            { "Settings_TrackerHotkeyUsed_Desc", "Configuração interna: Marca se o usuário usou o atalho de mostrar/ocultar" },

            // Movement Enhancement Settings
            { "Settings_MovementEnhancement_Name", "Melhoria de Resposta de Movimento" },
            { "Settings_MovementEnhancement_Desc", "Otimizar a sensação de movimento do personagem, reduzir movimento grudento. Múltiplas predefinições: Desativado, Leve, Médio, Pesado" },
            { "Settings_Movement_Disabled", "Desativado" },
            { "Settings_Movement_Light", "Otimização Leve" },
            { "Settings_Movement_Medium", "Otimização Média" },
            { "Settings_Movement_Heavy", "Otimização Pesada" },

            // UI Enhancement Settings
            { "Settings_EnableWeaponComparison_Name", "Ativar Comparação de Armas" },
            { "Settings_EnableWeaponComparison_Desc", "Comparar arma selecionada com arma sobre a qual o cursor está no inventário, mostrando diferenças com indicadores codificados por cor" },
            { "Settings_FastBuyEnabled_Name", "Ativar Compra Rápida" },
            { "Settings_FastBuyEnabled_Desc", "Comprar itens rapidamente passando o cursor sobre eles no menu da loja e pressionando F" },
            { "Settings_FastSellEnabled_Name", "Ativar Venda Rápida" },
            { "Settings_FastSellEnabled_Desc", "Vender itens rapidamente passando o cursor sobre eles no menu da loja e pressionando F" },
            { "Settings_ItemWheelScale_Name", "Escala do Menu de Roda" },
            { "Settings_ItemWheelScale_Desc", "Ajustar o tamanho de exibição de todos os menus de roda (roda de itens e roda de arremessáveis)" },
            { "Settings_ItemWheelTimeScale_Name", "Escala de Tempo do Menu de Roda" },
            { "Settings_ItemWheelTimeScale_Desc", "Ajustar a escala de tempo de todos os menus de roda (roda de itens e roda de arremessáveis)" },
            { "Settings_ItemWheelSensitivity_Name", "Sensibilidade do Menu de Roda" },
            { "Settings_ItemWheelSensitivity_Desc", "Ajustar a sensibilidade do mouse de todos os menus de roda (roda de itens e roda de arremessáveis)" },
            { "Settings_ItemWheelMenuHotkey_Name", "Atalho da Roda de Itens" },
            { "Settings_ItemWheelMenuHotkey_Desc", "Pressione e solte esta tecla para abrir o menu da roda de itens" },
            { "Settings_ThrowableWheelEnabled_Name", "Ativar Roda de Arremessáveis" },
            { "Settings_ThrowableWheelEnabled_Desc", "Ativar a função do menu da roda de arremessáveis" },
            { "Settings_ThrowableWheelHotkey_Name", "Atalho da Roda de Arremessáveis" },
            { "Settings_ThrowableWheelHotkey_Desc", "Pressione e solte esta tecla para abrir o menu da roda de arremessáveis (tecla G padrão)" },
            { "Settings_PressAnyKey", "Pressione qualquer tecla..." },

            { "Settings_AutoTrackNewQuests_Name", "Rastrear automaticamente novas missões aceitas" },
            { "Settings_AutoTrackNewQuests_Desc", "Adicionar automaticamente novas missões aceitas à lista de rastreamento do raid" },
            { "Settings_EnableDuckShit_Name", "Ativar Cocô de Pato" },
            { "Settings_EnableDuckShit_Desc", "Ativar a função onde os patos defecam automaticamente com base no consumo de energia e água" },
        };

        ModLogger.Log("Localization", $"Loaded translations for {LocalizationData.Count} languages");
    }

    /// <summary>
    /// 应用指定语言的翻译
    /// </summary>
    private static void ApplyTranslations(SystemLanguage language)
    {
        // 如果没有该语言的翻译，使用英语作为后备
        if (!LocalizationData.ContainsKey(language))
        {
            ModLogger.LogWarning($"No translations found for {language}, falling back to English");
            language = SystemLanguage.English;
        }

        var translations = LocalizationData[language];
        foreach (var kvp in translations)
        {
            string fullKey = GetFullKey(kvp.Key);
            LocalizationManager.SetOverrideText(fullKey, kvp.Value);
        }

        ModLogger.Log("Localization", $"Applied {translations.Count} translations for {language}");
    }

    /// <summary>
    /// 获取完整的本地化键名
    /// </summary>
    private static string GetFullKey(string shortKey)
    {
        return KeyPrefix + shortKey;
    }

    /// <summary>
    /// 获取本地化文本（便捷方法）
    /// </summary>
    public static string Get(string key)
    {
        return LocalizationManager.GetPlainText(GetFullKey(key));
    }

    /// <summary>
    /// 获取格式化的本地化文本
    /// </summary>
    public static string GetFormatted(string key, params object[] args)
    {
        string text = Get(key);
        try
        {
            return string.Format(text, args);
        }
        catch (System.Exception ex)
        {
            ModLogger.LogError($"Failed to format localization string '{key}': {ex.Message}");
            return text;
        }
    }

    /// <summary>
    /// 为 Text 组件自动更新本地化文本
    /// 使用方式: LocalizationHelper.SetLocalizedText(textComponent, "localization_key");
    /// </summary>
    public static void SetLocalizedText(UnityEngine.UI.Text textComponent, string localizationKey)
    {
        if (textComponent == null) return;

        textComponent.text = Get(localizationKey);

        // 订阅语言变更事件，当语言改变时自动更新文本
        OnLanguageChanged += (lang) =>
        {
            if (textComponent != null && !string.IsNullOrEmpty(localizationKey))
            {
                textComponent.text = Get(localizationKey);
            }
        };
    }

    /// <summary>
    /// 为 TextMeshProUGUI 组件自动更新本地化文本
    /// 使用方式: LocalizationHelper.SetLocalizedText(tmpText, "localization_key");
    /// </summary>
    public static void SetLocalizedText(TMPro.TextMeshProUGUI textComponent, string localizationKey)
    {
        if (textComponent == null) return;

        textComponent.text = Get(localizationKey);

        // 订阅语言变更事件，当语言改变时自动更新文本
        OnLanguageChanged += (lang) =>
        {
            if (textComponent != null && !string.IsNullOrEmpty(localizationKey))
            {
                textComponent.text = Get(localizationKey);
            }
        };
    }
}

