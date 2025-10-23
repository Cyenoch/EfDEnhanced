using System;
using System.Collections.Generic;
using System.Linq;
using Duckov.ItemUsage;
using Duckov.MasterKeys;
using Duckov.Quests;
using Duckov.Quests.Tasks;
using Duckov.Scenes;
using Duckov.Weathers;
using ItemStatsSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

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
/// 武器弹药不足警告
/// </summary>
public class LowAmmoWarning
{
    public string WeaponName { get; set; } = string.Empty;
    public string AmmoCaliber { get; set; } = string.Empty;
    public int CurrentAmmoCount { get; set; }
    public int MagazineCapacity { get; set; }
}

/// <summary>
/// 任务所需武器警告
/// </summary>
public class QuestWeaponRequirement
{
    public string QuestName { get; set; } = string.Empty;
    public string TaskDescription { get; set; } = string.Empty;
    public int WeaponTypeID { get; set; }
    public string WeaponName { get; set; } = string.Empty;
    public bool HasWeapon { get; set; }
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
    public List<QuestItemRequirement> QuestItems { get; set; } = [];
    public List<LowAmmoWarning> LowAmmoWarnings { get; set; } = [];
    public List<QuestWeaponRequirement> QuestWeapons { get; set; } = [];

    /// <summary>
    /// 是否携带了所有任务物品
    /// </summary>
    public bool HasAllQuestItems => QuestItems.All(q => q.IsSatisfied);

    /// <summary>
    /// 是否携带了所有任务所需武器
    /// </summary>
    public bool HasAllQuestWeapons => QuestWeapons.All(q => q.HasWeapon);

    /// <summary>
    /// 是否有低弹药警告
    /// </summary>
    public bool HasLowAmmoWarnings => LowAmmoWarnings.Count > 0;

    /// <summary>
    /// 是否满足所有条件
    /// </summary>
    public bool IsReady => HasWeapon && HasAmmo && HasMedicine && HasFood && IsWeatherSafe && !IsStormComing && HasAllQuestItems && !HasLowAmmoWarnings && HasAllQuestWeapons;

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

        // 添加低弹药警告
        foreach (var lowAmmoWarning in LowAmmoWarnings)
        {
            warnings.Add(LocalizationHelper.GetFormatted("Warning_LowAmmo",
                lowAmmoWarning.WeaponName,
                lowAmmoWarning.AmmoCaliber,
                lowAmmoWarning.CurrentAmmoCount,
                lowAmmoWarning.MagazineCapacity));
        }

        // 添加任务武器警告
        foreach (var questWeapon in QuestWeapons.Where(q => !q.HasWeapon))
        {
            warnings.Add(LocalizationHelper.GetFormatted("Warning_QuestWeapon",
                questWeapon.WeaponName,
                questWeapon.TaskDescription,
                questWeapon.QuestName));
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

        return LocalizationHelper.Get("RaidCheck_HasIssues") + string.Join("\n", warnings);
    }
}

/// <summary>
/// Raid进入前检查工具类
/// </summary>
public static class RaidCheckUtility
{
    /// <summary>
    /// 需要进行 Raid 检查的地图列表
    /// 排除新手引导关卡（Level_Guide_1, Level_Guide_2）
    /// </summary>
    private static readonly HashSet<string> RaidMapsToCheck =
    [
        // 农场系列
        "Level_Farm_Main",
        "Level_Farm_01",
        "Level_Farm_JLab_Facility",
        
        // 零号区
        "Level_GroundZero_Main",
        "Level_GroundZero_1",
        "Level_GroundZero_Cave",
        
        // 隐藏仓库
        "Level_HiddenWarehouse",
        "Level_HiddenWarehouse_Main",
        "Level_HiddenWarehouse_CellarUnderGround",
        
        // 实验室
        "Level_JLab_Main",
        "Level_JLab_1",
        "Level_JLab_2",
        
        // 挑战关卡
        "Level_DemoChallenge_1",
        "Level_DemoChallenge_Main",
        
        // 风暴区
        "Level_StormZone_1",
        "Level_StormZone_Main",
        "Level_StormZone_B0",
        "Level_StormZone_B1",
        "Level_StormZone_B2",
        "Level_StormZone_B3",
        "Level_StormZone_B4"
    ];

    /// <summary>
    /// 判断指定场景是否需要进行 Raid 检查
    /// </summary>
    /// <param name="sceneID">场景ID（不包含.unity后缀）</param>
    /// <returns>是否需要检查</returns>
    public static bool ShouldCheckRaidMap(string sceneID)
    {
        if (string.IsNullOrEmpty(sceneID))
        {
            ModLogger.LogWarning("RaidCheck", "Scene ID is null or empty");
            return false;
        }

        // 移除可能的路径前缀和.unity后缀
        var cleanSceneID = sceneID;
        if (cleanSceneID.Contains("/"))
        {
            cleanSceneID = cleanSceneID[(cleanSceneID.LastIndexOf('/') + 1)..];
        }
        if (cleanSceneID.EndsWith(".unity"))
        {
            cleanSceneID = cleanSceneID.Replace(".unity", "");
        }

        bool shouldCheck = RaidMapsToCheck.Contains(cleanSceneID);
        ModLogger.Log("RaidCheck", $"Scene '{sceneID}' (cleaned: '{cleanSceneID}') should check: {shouldCheck}");

        return shouldCheck;
    }

    /// <summary>
    /// 检查玩家是否准备好进入Raid
    /// </summary>
    /// <param name="targetSceneID">目标场景ID（可选，如果提供则只检查该场景相关的任务）</param>
    public static RaidCheckResult CheckPlayerReadiness(string? targetSceneID = null)
    {
        return ExceptionHelper.SafeExecute(() =>
        {
            // 如果Raid检查功能被禁用，直接返回全部通过
            if (!ModSettings.EnableRaidCheck.Value)
            {
                ModLogger.Log("RaidCheck", "Raid check is disabled in settings");
                return CreatePassedResult();
            }

            // 执行所有检查项
            var result = PerformAllChecks(targetSceneID);

            LogCheckResult(result, targetSceneID);

            return result;
        }, "CheckPlayerReadiness", CreatePassedResult());
    }

    /// <summary>
    /// 创建全部通过的结果（用于禁用检查或错误时）
    /// </summary>
    private static RaidCheckResult CreatePassedResult()
    {
        return new RaidCheckResult
        {
            HasWeapon = true,
            HasAmmo = true,
            HasMedicine = true,
            HasFood = true,
            IsWeatherSafe = true,
            IsStormComing = false,
            QuestItems = [],
            LowAmmoWarnings = [],
            QuestWeapons = []
        };
    }

    /// <summary>
    /// 执行所有检查项
    /// </summary>
    private static RaidCheckResult PerformAllChecks(string? targetSceneID)
    {
        // 使用统一的物品检查辅助类
        var basicEquipment = ItemCheckHelper.CheckBasicEquipment();

        return new RaidCheckResult
        {
            HasWeapon = !ModSettings.CheckWeapon.Value || basicEquipment.HasWeapon,
            HasAmmo = !ModSettings.CheckAmmo.Value || basicEquipment.HasAmmo,
            HasMedicine = !ModSettings.CheckMeds.Value || basicEquipment.HasMedicine,
            HasFood = !ModSettings.CheckFood.Value || basicEquipment.HasFood,
            IsWeatherSafe = !ModSettings.CheckWeather.Value || IsWeatherSafe(),
            IsStormComing = ModSettings.CheckWeather.Value && IsStormComingSoon(),
            QuestItems = CheckQuestItems(targetSceneID),
            LowAmmoWarnings = ModSettings.CheckAmmo.Value ? ItemCheckHelper.CheckAmmoSufficiency() : [],
            QuestWeapons = CheckKillTaskWeaponRequirements(targetSceneID)
        };
    }

    /// <summary>
    /// 记录检查结果
    /// </summary>
    private static void LogCheckResult(RaidCheckResult result, string? targetSceneID)
    {
        ModLogger.Log("RaidCheck",
            $"Check result for scene '{targetSceneID ?? "any"}' - " +
            $"Weapon: {result.HasWeapon}, " +
            $"Ammo: {result.HasAmmo}, " +
            $"Medicine: {result.HasMedicine}, " +
            $"Food: {result.HasFood}, " +
            $"Weather: {result.IsWeatherSafe}, " +
            $"StormComing: {result.IsStormComing}, " +
            $"QuestItems: {result.QuestItems.Count}, " +
            $"QuestWeapons: {result.QuestWeapons.Count}");
    }

    /// <summary>
    /// 检查天气是否安全（非风暴）
    /// 如果当前已在raid地图且正处于风暴中，则不检测风暴（已经在里面了）
    /// </summary>
    private static bool IsWeatherSafe()
    {
        try
        {
            // 如果已在raid地图且处于风暴中，不检测风暴
            if (IsInRaidMapDuringStorm())
            {
                ModLogger.Log("RaidCheck", "Already in raid map during storm, skipping weather check");
                return true;
            }

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
    /// 如果当前已在raid地图且正处于风暴中，则不检测风暴预警（已经在里面了）
    /// </summary>
    private static bool IsStormComingSoon()
    {
        try
        {
            // 如果已在raid地图且处于风暴中，不检测风暴预警
            if (IsInRaidMapDuringStorm())
            {
                ModLogger.Log("RaidCheck", "Already in raid map during storm, skipping storm warning check");
                return false;
            }

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
    /// 检查活跃任务所需的物品
    /// 这些是任务要求玩家携带到 Raid 中的物品（Quest.RequiredItemID）
    /// </summary>
    /// <param name="targetSceneID">目标场景ID（可选，如果提供则只检查该场景相关的任务）</param>
    private static List<QuestItemRequirement> CheckQuestItems(string? targetSceneID = null)
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

            ModLogger.Log("QuestCheck", $"Found {activeQuests.Count} active quests, filtering for scene: '{targetSceneID ?? "any"}'");

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
                    // 如果提供了目标场景ID，检查任务是否属于该场景
                    if (!string.IsNullOrEmpty(targetSceneID))
                    {
                        // 获取任务的场景信息
                        var questSceneInfo = quest.RequireSceneInfo;
                        var taskSceneIDs = new List<string>();

                        foreach (var task in quest.Tasks)
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
                            if (task is QuestTask_KillCount kc)
                            {
                                var requireSceneIDField = typeof(QuestTask_KillCount).GetField("requireSceneID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                if (requireSceneIDField?.GetValue(kc) is string sceneID && !string.IsNullOrEmpty(sceneID))
                                {
                                    taskSceneIDs.Add(sceneID);
                                }
                            }
                        }

                        // quest的子tasks可能用到的场景 和 自身要求的场景 ID，去重
                        List<string> relatedSceneIDs = [.. taskSceneIDs.Append(questSceneInfo?.ID ?? string.Empty).Where(x => !string.IsNullOrEmpty(x)).Distinct()];
                        if (relatedSceneIDs.Count == 0)
                        {
                            ModLogger.Log("QuestCheck", $"Checking quest '{quest.DisplayName}' - no scene requirement (can be done in any map)");
                        }
                        else
                        {
                            ModLogger.Log("QuestCheck", $"Checking quest '{quest.DisplayName}' - related scene '{string.Join(", ", relatedSceneIDs)}'");
                            if (!relatedSceneIDs.Any(x => x == targetSceneID))
                            {
                                ModLogger.Log("QuestCheck", $"Skipping quest '{quest.DisplayName}' - related scene '{string.Join(", ", relatedSceneIDs)}', target is '{targetSceneID}'");
                                continue;
                            }
                        }
                    }

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

                    // 只计算玩家当前携带的物品数量（背包+装备），不包括仓库
                    int currentCount = InventoryHelper.CountItemsOfType(itemTypeID,
                        ItemSourceFilter.CharacterInventory | ItemSourceFilter.PetInventory | ItemSourceFilter.SlotCollection);

                    // 检查是否是钥匙，并且已有万能钥匙激活
                    if (itemMeta.tags != null && itemMeta.tags.Any(tag => tag != null && tag.name == "Key"))
                    {
                        if (MasterKeysManager.IsActive(itemTypeID))
                        {
                            currentCount = requiredCount;
                        }
                    }

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

            ModLogger.Log("QuestCheck", $"Total quest items to check for scene '{targetSceneID ?? "any"}': {questItems.Count}");
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"CheckQuestItems failed: {ex}");
        }

        return questItems;
    }

    /// <summary>
    /// 检查击杀任务所需的武器
    /// 遍历所有活跃任务中的 QuestTask_KillCount，检查是否要求特定武器
    /// </summary>
    /// <param name="targetSceneID">目标场景ID（可选，如果提供则只检查该场景相关的任务）</param>
    private static List<QuestWeaponRequirement> CheckKillTaskWeaponRequirements(string? targetSceneID = null)
    {
        var weaponRequirements = new List<QuestWeaponRequirement>();

        try
        {
            // 检查 QuestManager 是否存在
            if (QuestManager.Instance == null)
            {
                ModLogger.LogWarning("QuestWeaponCheck", "QuestManager.Instance is null");
                return weaponRequirements;
            }

            // 获取所有活跃任务
            var activeQuests = QuestManager.Instance.ActiveQuests;
            if (activeQuests == null || activeQuests.Count == 0)
            {
                ModLogger.Log("QuestWeaponCheck", "No active quests found");
                return weaponRequirements;
            }

            ModLogger.Log("QuestWeaponCheck", $"Checking {activeQuests.Count} active quests for weapon requirements");

            // 获取玩家携带的所有物品（使用 InventoryHelper）
            var playerItems = InventoryHelper.GetPlayerCarriedItems();

            // 遍历所有活跃任务
            foreach (var quest in activeQuests)
            {
                if (quest == null)
                {
                    continue;
                }

                // 跳过已完成的任务
                if (quest.Complete)
                {
                    continue;
                }

                try
                {
                    // 如果提供了目标场景ID，检查任务是否属于该场景
                    if (!string.IsNullOrEmpty(targetSceneID))
                    {
                        // 获取任务的场景信息
                        var questSceneInfo = quest.RequireSceneInfo;

                        // 如果任务指定了特定场景要求
                        if (questSceneInfo != null && !string.IsNullOrEmpty(questSceneInfo.ID))
                        {
                            // 只检查与目标场景匹配的任务
                            if (questSceneInfo.ID != targetSceneID)
                            {
                                continue;
                            }
                        }
                    }

                    // 使用反射获取任务的所有任务目标（Tasks）
                    var questType = quest.GetType();
                    var tasksField = questType.GetField("tasks",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    if (tasksField == null)
                    {
                        ModLogger.LogWarning("QuestWeaponCheck", "Failed to get tasks field from Quest");
                        continue;
                    }

                    var tasks = tasksField.GetValue(quest) as List<Task>;
                    if (tasks == null || tasks.Count == 0)
                    {
                        continue;
                    }

                    foreach (var task in tasks)
                    {
                        if (task == null || task.IsFinished())
                        {
                            continue;
                        }

                        // 检查是否是击杀任务（QuestTask_KillCount）
                        var killTask = task as Duckov.Quests.Tasks.QuestTask_KillCount;
                        if (killTask == null)
                        {
                            continue;
                        }

                        // 使用反射获取 withWeapon 和 weaponTypeID 字段
                        var killTaskType = killTask.GetType();

                        var withWeaponField = killTaskType.GetField("withWeapon",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        var weaponTypeIDField = killTaskType.GetField("weaponTypeID",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                        if (withWeaponField == null || weaponTypeIDField == null)
                        {
                            ModLogger.LogWarning("QuestWeaponCheck", "Failed to get fields from QuestTask_KillCount");
                            continue;
                        }

                        bool withWeapon = (bool)(withWeaponField.GetValue(killTask) ?? false);
                        int weaponTypeID = (int)(weaponTypeIDField.GetValue(killTask) ?? 0);

                        // 如果任务要求特定武器
                        if (withWeapon && weaponTypeID > 0)
                        {
                            var weaponMeta = ItemAssetsCollection.GetMetaData(weaponTypeID);
                            if (weaponMeta.id == 0)
                            {
                                ModLogger.LogWarning("QuestWeaponCheck", $"Weapon metadata not found for ID: {weaponTypeID}");
                                continue;
                            }

                            // 检查玩家是否携带该武器
                            bool hasWeapon = playerItems.Any(item => item.TypeID == weaponTypeID);

                            var requirement = new QuestWeaponRequirement
                            {
                                QuestName = quest.DisplayName,
                                TaskDescription = task.Description,
                                WeaponTypeID = weaponTypeID,
                                WeaponName = weaponMeta.DisplayName,
                                HasWeapon = hasWeapon
                            };

                            weaponRequirements.Add(requirement);

                            string status = hasWeapon ? "✓" : "✗";
                            ModLogger.Log("QuestWeaponCheck",
                                $"{status} Quest: {quest.DisplayName}, Task: {task.Description}, " +
                                $"Required Weapon: {weaponMeta.DisplayName}, Has: {hasWeapon}");
                        }
                    }
                }
                catch (Exception questEx)
                {
                    ModLogger.LogError($"Failed to check quest {quest.DisplayName} for weapon requirements: {questEx.Message}");
                }
            }

            ModLogger.Log("QuestWeaponCheck",
                $"Total quest weapon requirements to check for scene '{targetSceneID ?? "any"}': {weaponRequirements.Count}");
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"CheckKillTaskWeaponRequirements failed: {ex}");
        }

        return weaponRequirements;
    }


    /// <summary>
    /// 检查是否当前在raid地图内且处于风暴中
    /// </summary>
    private static bool IsInRaidMapDuringStorm()
    {
        try
        {
            // 检查是否处于风暴
            Weather currentWeather = WeatherManager.GetWeather();
            if (currentWeather != Weather.Stormy_I && currentWeather != Weather.Stormy_II)
            {
                return false; // 不处于风暴
            }

            // 检查当前场景是否是raid地图
            // Scene currentScene = SceneManager.GetActiveScene();
            string currentSceneName = MultiSceneCore.MainSceneID;

            // 使用现有的方法检查是否是raid地图
            if (ShouldCheckRaidMap(currentSceneName))
            {
                ModLogger.Log("RaidCheck", $"Currently in raid map '{currentSceneName}' during storm");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"IsInRaidMapDuringStorm check failed: {ex}");
            return false; // 出错时假设不在raid地图
        }
    }


}

