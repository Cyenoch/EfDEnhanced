using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EfDEnhanced.Utils;
using UnityEngine;

namespace EfDEnhanced.Utils;

/// <summary>
/// 管理用户追踪的任务
/// </summary>
public static class QuestTrackingManager
{
    private static HashSet<int> _trackedQuestIds = new HashSet<int>();
    private static string SaveFilePath => Path.Combine(Application.persistentDataPath, "EfDEnhanced", "TrackedQuests.json");
    
    /// <summary>
    /// 获取所有被追踪的任务ID
    /// </summary>
    public static IReadOnlyCollection<int> TrackedQuestIds => _trackedQuestIds;
    
    /// <summary>
    /// 任务追踪状态改变事件
    /// </summary>
    public static event Action<int, bool>? OnTrackingChanged;
    
    /// <summary>
    /// 初始化，从磁盘加载追踪数据
    /// </summary>
    public static void Initialize()
    {
        try
        {
            LoadFromDisk();
            ModLogger.Log("QuestTracker", $"Initialized with {_trackedQuestIds.Count} tracked quests. location: {SaveFilePath}");
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestTrackingManager.Initialize failed: {ex}");
        }
    }
    
    /// <summary>
    /// 检查任务是否被追踪
    /// </summary>
    public static bool IsQuestTracked(int questId)
    {
        return _trackedQuestIds.Contains(questId);
    }
    
    /// <summary>
    /// 设置任务追踪状态
    /// </summary>
    public static void SetQuestTracked(int questId, bool tracked)
    {
        try
        {
            bool wasTracked = _trackedQuestIds.Contains(questId);
            
            if (tracked)
            {
                if (_trackedQuestIds.Add(questId))
                {
                    ModLogger.Log("QuestTracker", $"Quest {questId} is now tracked");
                    SaveToDisk();
                    OnTrackingChanged?.Invoke(questId, true);
                }
            }
            else
            {
                if (_trackedQuestIds.Remove(questId))
                {
                    ModLogger.Log("QuestTracker", $"Quest {questId} is no longer tracked");
                    SaveToDisk();
                    OnTrackingChanged?.Invoke(questId, false);
                }
            }
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestTrackingManager.SetQuestTracked failed: {ex}");
        }
    }
    
    /// <summary>
    /// 切换任务追踪状态
    /// </summary>
    public static void ToggleQuestTracking(int questId)
    {
        SetQuestTracked(questId, !IsQuestTracked(questId));
    }
    
    /// <summary>
    /// 从磁盘加载
    /// </summary>
    private static void LoadFromDisk()
    {
        try
        {
            if (!File.Exists(SaveFilePath))
            {
                ModLogger.Log("QuestTracker", "No save file found, starting fresh");
                return;
            }
            
            string json = File.ReadAllText(SaveFilePath);
            var data = JsonUtility.FromJson<SaveData>(json);
            
            if (data?.TrackedQuestIds != null)
            {
                _trackedQuestIds = new HashSet<int>(data.TrackedQuestIds);
                ModLogger.Log("QuestTracker", $"Loaded {_trackedQuestIds.Count} tracked quests from disk");
            }
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestTrackingManager.LoadFromDisk failed: {ex}");
        }
    }
    
    /// <summary>
    /// 保存到磁盘
    /// </summary>
    private static void SaveToDisk()
    {
        try
        {
            string directory = Path.GetDirectoryName(SaveFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            var data = new SaveData
            {
                TrackedQuestIds = _trackedQuestIds.ToList()
            };
            
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SaveFilePath, json);
            
            ModLogger.Log("QuestTracker", $"Saved {_trackedQuestIds.Count} tracked quests to disk");
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestTrackingManager.SaveToDisk failed: {ex}");
        }
    }
    
    /// <summary>
    /// 保存数据结构
    /// </summary>
    [Serializable]
    private class SaveData
    {
        public List<int> TrackedQuestIds = new List<int>();
    }
}

