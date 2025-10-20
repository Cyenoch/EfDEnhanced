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

