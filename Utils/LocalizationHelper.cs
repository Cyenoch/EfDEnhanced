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
    /// 初始化本地化系统
    /// </summary>
    public static void Initialize()
    {
        try
        {
            ModLogger.Log("Localization", "Initializing localization system...");

            // 注册语言切换事件
            LocalizationManager.OnSetLanguage += OnLanguageChanged;

            // 加载所有语言的翻译
            LoadTranslations();

            // 应用当前语言的翻译
            ApplyTranslations(LocalizationManager.CurrentLanguage);

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
            LocalizationManager.OnSetLanguage -= OnLanguageChanged;

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
    /// 语言切换事件处理
    /// </summary>
    private static void OnLanguageChanged(SystemLanguage newLanguage)
    {
        try
        {
            ModLogger.Log("Localization", $"Language changed to: {newLanguage}");
            ApplyTranslations(newLanguage);
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

            // Quest Tracker 设置
            { "Settings_EnableQuestTracker_Name", "启用任务追踪HUD" },
            { "Settings_EnableQuestTracker_Desc", "在Raid中显示活跃任务追踪器" },
            { "Settings_TrackerPositionX_Name", "追踪器水平位置" },
            { "Settings_TrackerPositionX_Desc", "水平位置 (0=左, 1=右)" },
            { "Settings_TrackerPositionY_Name", "追踪器垂直位置" },
            { "Settings_TrackerPositionY_Desc", "垂直位置 (0=底, 1=顶)" },
            { "Settings_TrackerScale_Name", "追踪器缩放" },
            { "Settings_TrackerScale_Desc", "UI缩放倍数" },
            { "Settings_TrackerShowDescription_Name", "显示任务描述" },
            { "Settings_TrackerShowDescription_Desc", "在追踪器中显示任务描述" },
            { "Settings_TrackerFilterByMap_Name", "只显示当前地图任务" },
            { "Settings_TrackerFilterByMap_Desc", "只显示当前地图相关的任务，以及没有地图限制的任务" },

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
            { "Settings_ItemWheelScale_Name", "轮盘菜单缩放" },
            { "Settings_ItemWheelScale_Desc", "调整所有轮盘菜单的显示大小（物品轮盘和投掷物轮盘）" },
            { "Settings_ItemWheelMenuHotkey_Name", "物品轮盘快捷键" },
            { "Settings_ItemWheelMenuHotkey_Desc", "按下并松开此键来打开物品轮盘菜单" },
            { "Settings_ThrowableWheelEnabled_Name", "启用投掷物轮盘" },
            { "Settings_ThrowableWheelEnabled_Desc", "启用投掷物轮盘菜单功能" },
            { "Settings_ThrowableWheelHotkey_Name", "投掷物轮盘快捷键" },
            { "Settings_ThrowableWheelHotkey_Desc", "按下并松开此键来打开投掷物轮盘菜单（默认G键）" },
            { "Settings_PressAnyKey", "按任意键..." },
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

            // Quest Tracker 設置
            { "Settings_EnableQuestTracker_Name", "啟用任務追蹤HUD" },
            { "Settings_EnableQuestTracker_Desc", "在Raid中顯示活躍任務追蹤器" },
            { "Settings_TrackerPositionX_Name", "追蹤器水平位置" },
            { "Settings_TrackerPositionX_Desc", "水平位置 (0=左, 1=右)" },
            { "Settings_TrackerPositionY_Name", "追蹤器垂直位置" },
            { "Settings_TrackerPositionY_Desc", "垂直位置 (0=底, 1=頂)" },
            { "Settings_TrackerScale_Name", "追蹤器縮放" },
            { "Settings_TrackerScale_Desc", "UI縮放倍數" },
            { "Settings_TrackerShowDescription_Name", "顯示任務描述" },
            { "Settings_TrackerShowDescription_Desc", "在追蹤器中顯示任務描述" },
            { "Settings_TrackerFilterByMap_Name", "只顯示當前地圖任務" },
            { "Settings_TrackerFilterByMap_Desc", "只顯示當前地圖相關的任務，以及沒有地圖限制的任務" },

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
            { "Settings_ItemWheelScale_Name", "輪盤選單縮放" },
            { "Settings_ItemWheelScale_Desc", "調整所有輪盤選單的顯示大小（物品輪盤和投擲物輪盤）" },
            { "Settings_ItemWheelMenuHotkey_Name", "物品輪盤快捷鍵" },
            { "Settings_ItemWheelMenuHotkey_Desc", "按下並鬆開此鍵來打開物品輪盤選單" },
            { "Settings_ThrowableWheelEnabled_Name", "啟用投擲物輪盤" },
            { "Settings_ThrowableWheelEnabled_Desc", "啟用投擲物輪盤選單功能" },
            { "Settings_ThrowableWheelHotkey_Name", "投擲物輪盤快捷鍵" },
            { "Settings_ThrowableWheelHotkey_Desc", "按下並鬆開此鍵來打開投擲物輪盤選單（預設G鍵）" },
            { "Settings_PressAnyKey", "按任意鍵..." },
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

            // Quest Tracker Settings
            { "Settings_EnableQuestTracker_Name", "Enable Quest Tracker HUD" },
            { "Settings_EnableQuestTracker_Desc", "Show active quest tracker during raids" },
            { "Settings_TrackerPositionX_Name", "Tracker Horizontal Position" },
            { "Settings_TrackerPositionX_Desc", "Horizontal position (0=left, 1=right)" },
            { "Settings_TrackerPositionY_Name", "Tracker Vertical Position" },
            { "Settings_TrackerPositionY_Desc", "Vertical position (0=bottom, 1=top)" },
            { "Settings_TrackerScale_Name", "Tracker Scale" },
            { "Settings_TrackerScale_Desc", "UI scale multiplier" },
            { "Settings_TrackerShowDescription_Name", "Show Quest Descriptions" },
            { "Settings_TrackerShowDescription_Desc", "Display quest descriptions in tracker" },
            { "Settings_TrackerFilterByMap_Name", "Show Only Current Map Quests" },
            { "Settings_TrackerFilterByMap_Desc", "Show only quests for the current map and quests without map requirements" },

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
            { "Settings_ItemWheelScale_Name", "Wheel Menu Scale" },
            { "Settings_ItemWheelScale_Desc", "Adjust the display size of all wheel menus (item wheel and throwable wheel)" },
            { "Settings_ItemWheelMenuHotkey_Name", "Item Wheel Hotkey" },
            { "Settings_ItemWheelMenuHotkey_Desc", "Press and release this key to open the item wheel menu" },
            { "Settings_ThrowableWheelEnabled_Name", "Enable Throwable Wheel" },
            { "Settings_ThrowableWheelEnabled_Desc", "Enable the throwable wheel menu feature" },
            { "Settings_ThrowableWheelHotkey_Name", "Throwable Wheel Hotkey" },
            { "Settings_ThrowableWheelHotkey_Desc", "Press and release this key to open the throwable wheel menu (default G key)" },
            { "Settings_PressAnyKey", "Press any key..." },
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

            // Quest Tracker 設定
            { "Settings_EnableQuestTracker_Name", "クエストトラッカーHUDを有効化" },
            { "Settings_EnableQuestTracker_Desc", "レイド中にアクティブクエストトラッカーを表示" },
            { "Settings_TrackerPositionX_Name", "トラッカー水平位置" },
            { "Settings_TrackerPositionX_Desc", "水平位置 (0=左, 1=右)" },
            { "Settings_TrackerPositionY_Name", "トラッカー垂直位置" },
            { "Settings_TrackerPositionY_Desc", "垂直位置 (0=下, 1=上)" },
            { "Settings_TrackerScale_Name", "トラッカースケール" },
            { "Settings_TrackerScale_Desc", "UIスケール倍率" },
            { "Settings_TrackerShowDescription_Name", "クエスト説明を表示" },
            { "Settings_TrackerShowDescription_Desc", "トラッカーにクエスト説明を表示" },
            { "Settings_TrackerFilterByMap_Name", "現在のマップのクエストのみ表示" },
            { "Settings_TrackerFilterByMap_Desc", "現在のマップに関連するクエストと、マップ制限のないクエストのみ表示" },

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
            { "Settings_ItemWheelScale_Name", "ホイールメニュースケール" },
            { "Settings_ItemWheelScale_Desc", "すべてのホイールメニューの表示サイズを調整（アイテムホイールと投擲物ホイール）" },
            { "Settings_ItemWheelMenuHotkey_Name", "アイテムホイールホットキー" },
            { "Settings_ItemWheelMenuHotkey_Desc", "このキーを押して離すとアイテムホイールメニューが開きます" },
            { "Settings_ThrowableWheelEnabled_Name", "投擲物ホイールを有効化" },
            { "Settings_ThrowableWheelEnabled_Desc", "投擲物ホイールメニュー機能を有効化" },
            { "Settings_ThrowableWheelHotkey_Name", "投擲物ホイールホットキー" },
            { "Settings_ThrowableWheelHotkey_Desc", "このキーを押して離すと投擲物ホイールメニューが開きます（デフォルトGキー）" },
            { "Settings_PressAnyKey", "任意のキーを押してください..." },
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
}

