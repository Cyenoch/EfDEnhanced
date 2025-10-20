using System;
using System.Collections.Generic;
using System.Linq;
using Duckov.ItemUsage;
using Duckov.Quests;
using Duckov.Weathers;
using ItemStatsSystem;
using UnityEngine;

namespace EfDEnhanced.Utils;

/// <summary>
/// 任务所需物品信息
/// </summary>
public class QuestItemRequirement
{
    public string QuestName { get; set; } = string.Empty;
    public int ItemTypeID { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int RequiredCount { get; set; }
    public int CurrentCount { get; set; }
    public bool IsSatisfied => CurrentCount >= RequiredCount;
}

/// <summary>
/// 检查结果数据结构
/// </summary>
public class RaidCheckResult
{
    public bool HasWeapon { get; set; }
    public bool HasAmmo { get; set; }
    public bool HasMedicine { get; set; }
    public bool HasFood { get; set; }
    public bool IsWeatherSafe { get; set; }
    public bool IsStormComing { get; set; }
    public List<QuestItemRequirement> QuestItems { get; set; } = new List<QuestItemRequirement>();
    
    /// <summary>
    /// 是否携带了所有任务物品
    /// </summary>
    public bool HasAllQuestItems => QuestItems.All(q => q.IsSatisfied);
    
    /// <summary>
    /// 是否满足所有条件
    /// </summary>
    public bool IsReady => HasWeapon && HasAmmo && HasMedicine && HasFood && IsWeatherSafe && !IsStormComing && HasAllQuestItems;
    
    /// <summary>
    /// 获取所有警告信息
    /// </summary>
    public List<string> GetWarnings()
    {
        var warnings = new List<string>();
        
        if (!HasWeapon)
        {
            warnings.Add(LocalizationHelper.Get("Warning_NoWeapon"));
        }
        
        if (!HasAmmo)
        {
            warnings.Add(LocalizationHelper.Get("Warning_NoAmmo"));
        }
        
        if (!HasMedicine)
        {
            warnings.Add(LocalizationHelper.Get("Warning_NoMedicine"));
        }
        
        if (!HasFood)
        {
            warnings.Add(LocalizationHelper.Get("Warning_NoFood"));
        }
        
        if (!IsWeatherSafe)
        {
            warnings.Add(LocalizationHelper.Get("Warning_StormyWeather"));
        }
        
        if (IsStormComing)
        {
            warnings.Add(LocalizationHelper.Get("Warning_StormComing"));
        }
        
        // 添加任务物品警告
        foreach (var questItem in QuestItems.Where(q => !q.IsSatisfied))
        {
            warnings.Add(LocalizationHelper.GetFormatted("Warning_QuestItem", 
                questItem.ItemName, 
                questItem.CurrentCount, 
                questItem.RequiredCount, 
                questItem.QuestName));
        }
        
        return warnings;
    }
    
    /// <summary>
    /// 获取格式化的警告文本
    /// </summary>
    public string GetWarningText()
    {
        var warnings = GetWarnings();
        if (warnings.Count == 0)
        {
            return LocalizationHelper.Get("RaidCheck_AllClear");
        }
        
        return LocalizationHelper.Get("RaidCheck_HasIssues") + "\n" + string.Join("\n", warnings);
    }
}

/// <summary>
/// Raid进入前检查工具类
/// </summary>
public static class RaidCheckUtility
{
    /// <summary>
    /// 检查玩家是否准备好进入Raid
    /// </summary>
    public static RaidCheckResult CheckPlayerReadiness()
    {
        try
        {
            // 获取玩家角色背包和装备
            var mainCharacter = LevelManager.Instance?.MainCharacter;
            var characterItem = mainCharacter?.CharacterItem;
            var inventory = characterItem?.Inventory;
            
            if (inventory == null || characterItem == null)
            {
                ModLogger.LogWarning("RaidCheck", "Player character inventory or item is null, skipping check");
                return new RaidCheckResult
                {
                    HasWeapon = true,
                    HasAmmo = true,
                    HasMedicine = true,
                    HasFood = true,
                    IsWeatherSafe = true
                };
            }
            
            // 收集所有物品：背包 + 装备栏
            var allItems = GetAllPlayerItems(inventory, characterItem);
            
            var result = new RaidCheckResult
            {
                HasWeapon = HasWeapon(allItems),
                HasAmmo = HasAmmo(allItems),
                HasMedicine = HasMedicalItems(allItems),
                HasFood = HasFoodItems(allItems),
                IsWeatherSafe = IsWeatherSafe(),
                IsStormComing = IsStormComingSoon(),
                QuestItems = CheckQuestItems()
            };
            
            ModLogger.Log("RaidCheck", $"Check result - Weapon: {result.HasWeapon}, Ammo: {result.HasAmmo}, Medicine: {result.HasMedicine}, Food: {result.HasFood}, Weather: {result.IsWeatherSafe}, StormComing: {result.IsStormComing}, QuestItems: {result.QuestItems.Count}");
            
            return result;
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"RaidCheck failed: {ex}");
            // 出错时返回全部通过，避免阻止游戏
            return new RaidCheckResult
            {
                HasWeapon = true,
                HasAmmo = true,
                HasMedicine = true,
                HasFood = true,
                IsWeatherSafe = true
            };
        }
    }
    
    /// <summary>
    /// 获取玩家所有物品（背包 + 装备栏）
    /// </summary>
    private static List<Item> GetAllPlayerItems(Inventory inventory, Item characterItem)
    {
        var allItems = new List<Item>();
        
        try
        {
            // 添加背包中的物品
            foreach (var item in inventory)
            {
                if (item != null)
                {
                    allItems.Add(item);
                }
            }
            
            // 添加装备栏中的物品
            // 装备栏slot哈希值来自 CharacterEquipmentController
            int[] equipmentSlotHashes = new int[]
            {
                "Armor".GetHashCode(),      // 护甲
                "Helmat".GetHashCode(),     // 头盔
                "FaceMask".GetHashCode(),   // 面罩
                "Backpack".GetHashCode(),   // 背包
                "Headset".GetHashCode(),    // 耳机
                "PrimaryWeapon".GetHashCode(),   // 主武器
                "SecondaryWeapon".GetHashCode(), // 副武器
                "MeleeWeapon".GetHashCode()      // 近战武器
            };
            
            foreach (var slotHash in equipmentSlotHashes)
            {
                try
                {
                    var slot = characterItem.Slots?.GetSlot(slotHash);
                    if (slot?.Content != null)
                    {
                        allItems.Add(slot.Content);
                        ModLogger.Log("RaidCheck", $"Found equipped item: {slot.Content.DisplayName}");
                    }
                }
                catch (Exception ex)
                {
                    ModLogger.LogWarning($"Failed to check equipment slot {slotHash}: {ex.Message}");
                }
            }
            
            ModLogger.Log("RaidCheck", $"Total items checked: {allItems.Count} (inventory + equipment)");
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"GetAllPlayerItems failed: {ex}");
        }
        
        return allItems;
    }
    
    /// <summary>
    /// 检查是否携带枪支
    /// 使用游戏官方标准：检查 IsGun 布尔标志
    /// </summary>
    private static bool HasWeapon(List<Item> allItems)
    {
        try
        {
            foreach (var item in allItems)
            {
                if (item == null) continue;
                
                // 游戏官方方式：检查 IsGun 标志（由 ItemSetting_Gun.SetMarkerParam 设置）
                if (item.GetBool("IsGun", false))
                {
                    ModLogger.Log("RaidCheck", $"Found weapon: {item.DisplayName}");
                    return true;
                }
            }
            
            return false;
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"HasWeapon check failed: {ex}");
            return true; // 出错时假设有武器
        }
    }
    
    /// <summary>
    /// 检查是否携带弹药（包括背包中的额外弹药）
    /// 使用游戏官方标准：检查 IsBullet 布尔标志
    /// </summary>
    private static bool HasAmmo(List<Item> allItems)
    {
        try
        {
            foreach (var item in allItems)
            {
                if (item == null) continue;
                
                // 游戏官方方式：检查 IsBullet 标志（由 ItemSetting_Bullet.SetMarkerParam 设置）
                if (item.GetBool("IsBullet", false))
                {
                    ModLogger.Log("RaidCheck", $"Found ammo: {item.DisplayName}");
                    return true;
                }
            }
            
            return false;
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"HasAmmo check failed: {ex}");
            return true; // 出错时假设有弹药
        }
    }
    
    /// <summary>
    /// 检查是否携带药品
    /// 使用游戏官方标准：检查 UsageUtilities 中的 Drug 组件
    /// </summary>
    private static bool HasMedicalItems(List<Item> allItems)
    {
        try
        {
            foreach (var item in allItems)
            {
                if (item == null) continue;
                
                // 游戏官方方式：检查UsageUtilities中是否有Drug类型
                if (item.UsageUtilities?.behaviors != null)
                {
                    foreach (var behavior in item.UsageUtilities.behaviors)
                    {
                        if (behavior is Drug)
                        {
                            ModLogger.Log("RaidCheck", $"Found medicine: {item.DisplayName}");
                            return true;
                        }
                    }
                }
            }
            
            return false;
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"HasMedicalItems check failed: {ex}");
            return true; // 出错时假设有药品
        }
    }
    
    /// <summary>
    /// 检查是否携带食物
    /// 使用游戏官方标准：检查 UsageUtilities 中的 FoodDrink 组件
    /// </summary>
    private static bool HasFoodItems(List<Item> allItems)
    {
        try
        {
            foreach (var item in allItems)
            {
                if (item == null) continue;
                
                // 游戏官方方式：检查UsageUtilities中是否有FoodDrink类型
                if (item.UsageUtilities?.behaviors != null)
                {
                    foreach (var behavior in item.UsageUtilities.behaviors)
                    {
                        if (behavior is FoodDrink)
                        {
                            ModLogger.Log("RaidCheck", $"Found food: {item.DisplayName}");
                            return true;
                        }
                    }
                }
            }
            
            return false;
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"HasFoodItems check failed: {ex}");
            return true; // 出错时假设有食物
        }
    }
    
    /// <summary>
    /// 检查天气是否安全（非风暴）
    /// </summary>
    private static bool IsWeatherSafe()
    {
        try
        {
            Weather currentWeather = WeatherManager.GetWeather();
            bool isSafe = currentWeather != Weather.Stormy_I && currentWeather != Weather.Stormy_II;
            
            ModLogger.Log("RaidCheck", $"Current weather: {currentWeather}, Safe: {isSafe}");
            
            return isSafe;
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"IsWeatherSafe check failed: {ex}");
            return true; // 出错时假设天气安全
        }
    }
    
    /// <summary>
    /// 检查风暴是否即将来临（24小时内）
    /// </summary>
    private static bool IsStormComingSoon()
    {
        try
        {
            Weather currentWeather = WeatherManager.GetWeather();
            
            // 如果已经在风暴中，不需要再警告"即将来临"
            if (currentWeather == Weather.Stormy_I || currentWeather == Weather.Stormy_II)
            {
                return false;
            }
            
            // 获取风暴到达时间（ETA）
            var stormETA = WeatherManager.Instance.Storm.GetStormETA(GameClock.Now);
            bool isComingSoon = stormETA.TotalHours < 24.0 && stormETA.TotalHours > 0;
            
            ModLogger.Log("RaidCheck", $"Storm ETA: {stormETA.TotalHours:F2} hours, Coming soon: {isComingSoon}");
            
            return isComingSoon;
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"IsStormComingSoon check failed: {ex}");
            return false; // 出错时假设风暴不会来临
        }
    }
    
    /// <summary>
    /// 检查所有活跃任务所需的物品
    /// 这些是任务要求玩家携带到 Raid 中的物品（Quest.RequiredItemID）
    /// </summary>
    private static List<QuestItemRequirement> CheckQuestItems()
    {
        var questItems = new List<QuestItemRequirement>();
        
        try
        {
            // 检查 QuestManager 是否存在
            if (QuestManager.Instance == null)
            {
                ModLogger.LogWarning("QuestCheck", "QuestManager.Instance is null");
                return questItems;
            }
            
            var activeQuests = QuestManager.Instance.ActiveQuests;
            if (activeQuests == null || activeQuests.Count == 0)
            {
                ModLogger.Log("QuestCheck", "No active quests found");
                return questItems;
            }
            
            ModLogger.Log("QuestCheck", $"Found {activeQuests.Count} active quests");
            
            // 遍历所有活跃任务
            foreach (var quest in activeQuests)
            {
                if (quest == null)
                {
                    continue;
                }
                
                // 如果任务已完成，跳过
                if (quest.Complete)
                {
                    continue;
                }
                
                try
                {
                    // 检查任务的 RequiredItemID（任务要求携带的物品）
                    int itemTypeID = quest.RequiredItemID;
                    int requiredCount = quest.RequiredItemCount;
                    
                    // 如果任务没有要求携带物品，跳过
                    if (itemTypeID <= 0 || requiredCount <= 0)
                    {
                        continue;
                    }
                    
                    var itemMeta = ItemAssetsCollection.GetMetaData(itemTypeID);
                    if (itemMeta.id == 0)
                    {
                        ModLogger.LogWarning("QuestCheck", $"Item metadata not found for ID: {itemTypeID}");
                        continue;
                    }
                    
                    // 使用游戏的官方方法获取物品数量（包括背包、仓库等所有位置）
                    int currentCount = ItemUtilities.GetItemCount(itemTypeID);
                    
                    var requirement = new QuestItemRequirement
                    {
                        QuestName = quest.DisplayName,
                        ItemTypeID = itemTypeID,
                        ItemName = itemMeta.DisplayName,
                        RequiredCount = requiredCount,
                        CurrentCount = currentCount
                    };
                    
                    questItems.Add(requirement);
                    
                    string status = requirement.IsSatisfied ? "✓" : "✗";
                    ModLogger.Log("QuestCheck", $"{status} Quest: {quest.DisplayName}, Item: {itemMeta.DisplayName}, Required: {requiredCount}, Current: {currentCount}");
                }
                catch (Exception questEx)
                {
                    ModLogger.LogError($"Failed to check quest {quest.DisplayName}: {questEx.Message}");
                }
            }
            
            ModLogger.Log("QuestCheck", $"Total quest items to check: {questItems.Count}");
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"CheckQuestItems failed: {ex}");
        }
        
        return questItems;
    }
}

