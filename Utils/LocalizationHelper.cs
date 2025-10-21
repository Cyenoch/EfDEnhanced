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
    private static readonly Dictionary<SystemLanguage, Dictionary<string, string>> LocalizationData = new();
    
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
            
            // 任务追踪器
            { "QuestTracker_Title", "活跃任务" },
            { "QuestTracker_Progress", "进度: {0}/{1}" },
            { "QuestTracker_NoQuests", "无进行中的任务" },
            { "QuestTracker_TaskComplete", "✓" },  // 已完成标记
            { "QuestTracker_TaskPending", "○" },   // 未完成标记
            { "QuestTracker_CheckboxLabel", "局内追踪" },  // 任务详情界面的追踪复选框标签

            // 设置界面
            { "Settings_Title", "EfD Enhanced 设置" },
            { "Settings_ResetButton", "恢复默认" },
            { "Settings_CloseButton", "关闭" },
            { "Settings_ModSettings_Button", "EfD Enhanced 设置" },

            // 设置类别
            { "Settings_Category_PreRaidCheck", "Raid前检查" },
            { "Settings_Category_QuestTracker", "任务追踪器" },

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
            
            { "QuestTracker_Title", "活躍任務" },
            { "QuestTracker_Progress", "進度: {0}/{1}" },
            { "QuestTracker_NoQuests", "無進行中的任務" },
            { "QuestTracker_TaskComplete", "✓" },
            { "QuestTracker_TaskPending", "○" },
            { "QuestTracker_CheckboxLabel", "局內追蹤" },

            // 設置界面
            { "Settings_Title", "EfD Enhanced 設置" },
            { "Settings_ResetButton", "恢復默認" },
            { "Settings_CloseButton", "關閉" },
            { "Settings_ModSettings_Button", "EfD Enhanced 設置" },

            // 設置類別
            { "Settings_Category_PreRaidCheck", "Raid前檢查" },
            { "Settings_Category_QuestTracker", "任務追蹤器" },

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
            
            { "QuestTracker_Title", "Active Quests" },
            { "QuestTracker_Progress", "Progress: {0}/{1}" },
            { "QuestTracker_NoQuests", "No active quests" },
            { "QuestTracker_TaskComplete", "✓" },
            { "QuestTracker_TaskPending", "○" },
            { "QuestTracker_CheckboxLabel", "Track in Raid" },

            // Settings UI
            { "Settings_Title", "EfD Enhanced Settings" },
            { "Settings_ResetButton", "Reset to Defaults" },
            { "Settings_CloseButton", "Close" },
            { "Settings_ModSettings_Button", "EfD Enhanced Settings" },

            // Settings Categories
            { "Settings_Category_PreRaidCheck", "Pre-Raid Check" },
            { "Settings_Category_QuestTracker", "Quest Tracker" },

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
            
            { "QuestTracker_Title", "アクティブクエスト" },
            { "QuestTracker_Progress", "進行状況: {0}/{1}" },
            { "QuestTracker_NoQuests", "進行中のクエストなし" },
            { "QuestTracker_TaskComplete", "✓" },
            { "QuestTracker_TaskPending", "○" },
            { "QuestTracker_CheckboxLabel", "レイド中追跡" },

            // 設定UI
            { "Settings_Title", "EfD Enhanced 設定" },
            { "Settings_ResetButton", "デフォルトに戻す" },
            { "Settings_CloseButton", "閉じる (Alt+O)" },
            { "Settings_ModSettings_Button", "EfD Enhanced 設定" },

            // 設定カテゴリ
            { "Settings_Category_PreRaidCheck", "レイド前チェック" },
            { "Settings_Category_QuestTracker", "クエストトラッカー" },

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

