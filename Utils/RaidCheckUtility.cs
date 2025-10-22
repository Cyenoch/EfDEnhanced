using System;
using System.Collections.Generic;
using System.Linq;
using Duckov.ItemUsage;
using Duckov.Quests;
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

    /// <summary>
    /// 是否携带了所有任务物品
    /// </summary>
    public bool HasAllQuestItems => QuestItems.All(q => q.IsSatisfied);

    /// <summary>
    /// 是否有低弹药警告
    /// </summary>
    public bool HasLowAmmoWarnings => LowAmmoWarnings.Count > 0;

    /// <summary>
    /// 是否满足所有条件
    /// </summary>
    public bool IsReady => HasWeapon && HasAmmo && HasMedicine && HasFood && IsWeatherSafe && !IsStormComing && HasAllQuestItems && !HasLowAmmoWarnings;

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

            // 获取玩家数据
            var (inventory, characterItem, petInventory) = GetPlayerInventoryAndCharacter();
            if (inventory == null || characterItem == null || petInventory == null)
            {
                ModLogger.LogWarning("RaidCheck", "Player character data unavailable, skipping check");
                return CreatePassedResult();
            }

            // 收集所有物品：背包 + 装备栏
            var allItems = GetAllPlayerItems(inventory, characterItem, petInventory);

            // 执行所有检查项
            var result = PerformAllChecks(allItems, targetSceneID);

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
            LowAmmoWarnings = []
        };
    }

    /// <summary>
    /// 获取玩家背包和角色物品
    /// </summary>
    private static (Inventory? inventory, Item? characterItem, Inventory? petInventory) GetPlayerInventoryAndCharacter()
    {
        var mainCharacter = LevelManager.Instance?.MainCharacter;
        var characterItem = mainCharacter?.CharacterItem;
        var inventory = characterItem?.Inventory;
        var petInventory = PetProxy.PetInventory;

        return (inventory, characterItem, petInventory);
    }

    /// <summary>
    /// 执行所有检查项
    /// </summary>
    private static RaidCheckResult PerformAllChecks(List<Item> allItems, string? targetSceneID)
    {
        return new RaidCheckResult
        {
            HasWeapon = !ModSettings.CheckWeapon.Value || HasWeapon(allItems),
            HasAmmo = !ModSettings.CheckAmmo.Value || HasAmmo(allItems),
            HasMedicine = !ModSettings.CheckMeds.Value || HasMedicalItems(allItems),
            HasFood = !ModSettings.CheckFood.Value || HasFoodItems(allItems),
            IsWeatherSafe = !ModSettings.CheckWeather.Value || IsWeatherSafe(),
            IsStormComing = ModSettings.CheckWeather.Value && IsStormComingSoon(),
            QuestItems = CheckQuestItems(targetSceneID),
            LowAmmoWarnings = ModSettings.CheckAmmo.Value ? CheckWeaponAmmoSufficiency() : []
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
            $"QuestItems: {result.QuestItems.Count}");
    }

    /// <summary>
    /// 获取玩家所有物品（背包 + 装备栏）
    /// </summary>
    private static List<Item> GetAllPlayerItems(Inventory inventory, Item characterItem, Inventory petInventory)
    {
        var allItems = new List<Item>();

        // 防御性检查
        if (!ExceptionHelper.CheckNotNull(inventory, nameof(inventory), "GetAllPlayerItems") ||
            !ExceptionHelper.CheckNotNull(characterItem, nameof(characterItem), "GetAllPlayerItems") ||
            !ExceptionHelper.CheckNotNull(petInventory, nameof(petInventory), "GetAllPlayerItems"))
        {
            return allItems;
        }

        ExceptionHelper.SafeExecute(() =>
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
            int[] equipmentSlotHashes =
            [
                "Armor".GetHashCode(),      // 护甲
                "Helmat".GetHashCode(),     // 头盔
                "FaceMask".GetHashCode(),   // 面罩
                "Backpack".GetHashCode(),   // 背包
                "Headset".GetHashCode(),    // 耳机
                "PrimaryWeapon".GetHashCode(),   // 主武器
                "SecondaryWeapon".GetHashCode(), // 副武器
                "MeleeWeapon".GetHashCode()      // 近战武器
            ];

            foreach (var slotHash in equipmentSlotHashes)
            {
                ExceptionHelper.SafeExecute(() =>
                {
                    var slot = characterItem.Slots?.GetSlot(slotHash);
                    if (slot?.Content != null)
                    {
                        allItems.Add(slot.Content);
                        ModLogger.Log("RaidCheck", $"Found equipped item: {slot.Content.DisplayName}");
                    }
                }, $"CheckEquipmentSlot_{slotHash}");
            }

            // 添加宠物背包中的物品
            foreach (var item in petInventory)
            {
                if (item != null)
                {
                    allItems.Add(item);
                }
            }

            ModLogger.Log("RaidCheck", $"Total items checked: {allItems.Count} (inventory + equipment + pet)");
        }, "GetAllPlayerItems");

        return allItems;
    }

    /// <summary>
    /// 检查是否携带枪支
    /// 使用游戏官方标准：检查 IsGun 布尔标志
    /// </summary>
    private static bool HasWeapon(List<Item> allItems)
    {
        return ExceptionHelper.SafeExecute(() =>
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
        }, "HasWeapon", true); // 出错时假设有武器
    }

    /// <summary>
    /// 检查是否携带弹药（包括背包中的额外弹药）
    /// 使用游戏官方标准：检查 IsBullet 布尔标志
    /// </summary>
    private static bool HasAmmo(List<Item> allItems)
    {
        return ExceptionHelper.SafeExecute(() =>
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
        }, "HasAmmo", true); // 出错时假设有弹药
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

                        // 如果任务指定了特定场景要求
                        if (questSceneInfo != null && !string.IsNullOrEmpty(questSceneInfo.ID))
                        {
                            // 只检查与目标场景匹配的任务
                            if (questSceneInfo.ID != targetSceneID)
                            {
                                ModLogger.Log("QuestCheck", $"Skipping quest '{quest.DisplayName}' - requires scene '{questSceneInfo.ID}', target is '{targetSceneID}'");
                                continue;
                            }
                            else
                            {
                                ModLogger.Log("QuestCheck", $"Checking quest '{quest.DisplayName}' - matches target scene '{targetSceneID}'");
                            }
                        }
                        // 如果任务没有指定场景要求，说明可以在任意地图完成，需要检查
                        else
                        {
                            ModLogger.Log("QuestCheck", $"Checking quest '{quest.DisplayName}' - no scene requirement (can be done in any map)");
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
                    int currentCount = CountItemInPlayerInventory(itemTypeID);

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
    /// 计算玩家背包、装备栏和宠物身上特定物品的数量（不包括仓库）
    /// 使用游戏内置的方法来判断物品位置
    /// </summary>
    /// <param name="itemTypeID">物品类型ID</param>
    /// <returns>玩家和宠物当前携带的物品数量</returns>
    private static int CountItemInPlayerInventory(int itemTypeID)
    {
        try
        {
            // 使用游戏内置方法查找所有该类型的物品（包括所有位置：仓库、角色、宠物等）
            var allItemsOfType = ItemUtilities.FindAllBelongsToPlayer(item => 
                item != null && item.TypeID == itemTypeID);
            
            if (allItemsOfType == null || allItemsOfType.Count == 0)
            {
                ModLogger.Log("QuestCheck", $"No items of type {itemTypeID} found anywhere");
                return 0;
            }

            int count = 0;

            // 计算在玩家角色和宠物身上的物品（排除仓库）
            foreach (var item in allItemsOfType)
            {
                if (item == null) continue;

                // 首先排除仓库中的物品
                if (item.IsInPlayerStorage())
                {
                    ModLogger.Log("QuestCheck", $"Item {item.DisplayName} is in storage, skipping");
                    continue;
                }

                // 检查是否在玩家角色身上
                bool isOnCharacter = item.IsInPlayerCharacter();
                
                // 检查是否在宠物身上
                bool isOnPet = IsItemInPet(item);

                if (isOnCharacter || isOnPet)
                {
                    // 使用 StackCount 属性获取堆叠数量
                    int stackCount = item.StackCount;
                    count += Math.Max(1, stackCount); // 至少算1个
                    
                    string location = isOnCharacter ? "character" : "pet";
                    ModLogger.Log("QuestCheck", $"Found item on {location}: {item.DisplayName} (stack: {stackCount})");
                }
                else
                {
                    ModLogger.Log("QuestCheck", $"Item {item.DisplayName} found in unknown location, skipping");
                }
            }

            ModLogger.Log("QuestCheck", $"Total found {count} of item type {itemTypeID} on player character and pet");
            return count;
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"CountItemInPlayerInventory failed: {ex}");
            return 0;
        }
    }

    /// <summary>
    /// 检查物品是否在宠物身上
    /// </summary>
    private static bool IsItemInPet(Item item)
    {
        try
        {
            // 获取宠物的 Inventory
            var petInventory = PetProxy.PetInventory;
            if (petInventory == null)
            {
                return false;
            }

            // 检查物品的所有父级中是否有在宠物 Inventory 中的
            return item.GetAllParents().Any(e => e.InInventory == petInventory);
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"IsItemInPet check failed: {ex}");
            return false;
        }
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

    /// <summary>
    /// 检查武器弹药是否充足
    /// 对于每把枪，检查玩家携带的对应口径弹药总数（包括装载在武器中的）是否小于一个弹匣容量
    /// </summary>
    private static List<LowAmmoWarning> CheckWeaponAmmoSufficiency()
    {
        var warnings = new List<LowAmmoWarning>();

        return ExceptionHelper.SafeExecute(() =>
        {
            // 获取玩家角色和背包
            var (inventory, characterItem, petInventory) = GetPlayerInventoryAndCharacter();
            if (inventory == null || characterItem == null || petInventory == null)
            {
                ModLogger.LogWarning("AmmoCheck", "Player character data unavailable");
                return warnings;
            }

            // 获取所有玩家物品（背包 + 装备 + 宠物）
            var allItems = GetAllPlayerItems(inventory, characterItem, petInventory);

            // 获取玩家携带的所有枪支
            var guns = GetAllPlayerGuns(allItems);
            
            ModLogger.Log("AmmoCheck", $"Found {guns.Count} guns to check");

            // 对于每把枪，检查弹药是否充足
            foreach (var gun in guns)
            {
                ExceptionHelper.SafeExecute(() =>
                {
                    CheckSingleGunAmmo(gun, allItems, warnings);
                }, $"CheckAmmoForGun_{gun.DisplayName}");
            }

            ModLogger.Log("AmmoCheck", $"Generated {warnings.Count} low ammo warnings");
            return warnings;
        }, "CheckWeaponAmmoSufficiency", warnings);
    }

    /// <summary>
    /// 获取玩家携带的所有枪支（从背包、装备栏、宠物）
    /// </summary>
    private static List<Item> GetAllPlayerGuns(List<Item> allItems)
    {
        var guns = new List<Item>();

        try
        {
            foreach (var item in allItems)
            {
                if (item == null) continue;

                // 使用游戏官方标准：检查 IsGun 标志
                if (item.GetBool("IsGun", false))
                {
                    guns.Add(item);
                    ModLogger.Log("AmmoCheck", $"Found gun: {item.DisplayName}");
                }
            }
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"GetAllPlayerGuns failed: {ex}");
        }

        return guns;
    }

    /// <summary>
    /// 检查单把枪的弹药是否充足
    /// </summary>
    private static void CheckSingleGunAmmo(Item gun, List<Item> allItems, List<LowAmmoWarning> warnings)
    {
        try
        {
            // 获取枪支的设置组件
            var gunSetting = gun.GetComponent<ItemSetting_Gun>();
            if (gunSetting == null)
            {
                ModLogger.LogWarning("AmmoCheck", $"Gun {gun.DisplayName} has no ItemSetting_Gun component");
                return;
            }

            // 获取弹匣容量
            int magazineCapacity = gunSetting.Capacity;
            if (magazineCapacity <= 0)
            {
                ModLogger.Log("AmmoCheck", $"Gun {gun.DisplayName} has invalid capacity: {magazineCapacity}");
                return;
            }

            // 获取枪支的口径
            var caliberHash = "Caliber".GetHashCode();
            string gunCaliber = gun.Constants.GetString(caliberHash);
            
            if (string.IsNullOrEmpty(gunCaliber))
            {
                ModLogger.LogWarning("AmmoCheck", $"Gun {gun.DisplayName} has no caliber info");
                return;
            }

            ModLogger.Log("AmmoCheck", $"Checking gun: {gun.DisplayName}, Caliber: {gunCaliber}, Capacity: {magazineCapacity}");

            // 统计玩家携带的该口径弹药总数
            int totalAmmoCount = CountAmmoForCaliber(gunCaliber, allItems, gun);

            ModLogger.Log("AmmoCheck", $"Gun {gun.DisplayName}: Found {totalAmmoCount} ammo vs capacity {magazineCapacity}");

            // 如果弹药总数小于弹匣容量，生成警告
            if (totalAmmoCount < magazineCapacity)
            {
                warnings.Add(new LowAmmoWarning
                {
                    WeaponName = gun.DisplayName,
                    AmmoCaliber = gunCaliber,
                    CurrentAmmoCount = totalAmmoCount,
                    MagazineCapacity = magazineCapacity
                });

                ModLogger.LogWarning("AmmoCheck", 
                    $"LOW AMMO: {gun.DisplayName} ({gunCaliber}) - {totalAmmoCount}/{magazineCapacity}");
            }
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"CheckSingleGunAmmo failed for {gun?.DisplayName ?? "unknown"}: {ex}");
        }
    }

    /// <summary>
    /// 统计玩家携带的特定口径弹药总数（包括背包、装备、宠物、以及装载在所有武器中的）
    /// </summary>
    private static int CountAmmoForCaliber(string caliber, List<Item> allItems, Item currentGun)
    {
        int totalCount = 0;

        try
        {
            var caliberHash = "Caliber".GetHashCode();

            foreach (var item in allItems)
            {
                if (item == null) continue;

                // 检查是否是弹药（使用游戏官方标准）
                if (!item.GetBool("IsBullet", false))
                {
                    continue;
                }

                // 检查弹药的口径是否匹配
                string ammoCaliber = item.Constants.GetString(caliberHash);
                if (ammoCaliber != caliber)
                {
                    continue;
                }

                // 统计弹药数量（使用 StackCount）
                int stackCount = Math.Max(1, item.StackCount);
                totalCount += stackCount;

                ModLogger.Log("AmmoCheck", $"Found matching ammo: {item.DisplayName} ({ammoCaliber}) x{stackCount}");
            }

            // 额外统计装载在当前枪支中的弹药
            int loadedAmmo = GetLoadedAmmoCount(currentGun);
            if (loadedAmmo > 0)
            {
                totalCount += loadedAmmo;
                ModLogger.Log("AmmoCheck", $"Gun {currentGun.DisplayName} has {loadedAmmo} loaded ammo");
            }

            // 检查其他同口径枪支中装载的弹药（避免重复计算）
            totalCount += CountAmmoLoadedInOtherGuns(caliber, allItems, currentGun);
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"CountAmmoForCaliber failed: {ex}");
        }

        return totalCount;
    }

    /// <summary>
    /// 获取枪支中装载的弹药数量
    /// </summary>
    private static int GetLoadedAmmoCount(Item gun)
    {
        try
        {
            var gunSetting = gun.GetComponent<ItemSetting_Gun>();
            if (gunSetting == null)
            {
                return 0;
            }

            return gunSetting.BulletCount;
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"GetLoadedAmmoCount failed: {ex}");
            return 0;
        }
    }

    /// <summary>
    /// 统计其他枪支中装载的同口径弹药（避免重复计算）
    /// </summary>
    private static int CountAmmoLoadedInOtherGuns(string caliber, List<Item> allItems, Item currentGun)
    {
        int totalCount = 0;

        try
        {
            var caliberHash = "Caliber".GetHashCode();

            foreach (var item in allItems)
            {
                if (item == null || item == currentGun) continue;

                // 检查是否是枪支
                if (!item.GetBool("IsGun", false))
                {
                    continue;
                }

                // 检查枪支的口径是否匹配
                string gunCaliber = item.Constants.GetString(caliberHash);
                if (gunCaliber != caliber)
                {
                    continue;
                }

                // 获取枪支中装载的弹药数量
                int loadedAmmo = GetLoadedAmmoCount(item);
                if (loadedAmmo > 0)
                {
                    totalCount += loadedAmmo;
                    ModLogger.Log("AmmoCheck", $"Other gun {item.DisplayName} has {loadedAmmo} loaded {caliber} ammo");
                }
            }
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"CountAmmoLoadedInOtherGuns failed: {ex}");
        }

        return totalCount;
    }
}

