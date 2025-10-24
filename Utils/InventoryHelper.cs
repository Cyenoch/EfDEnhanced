using System;
using System.Collections.Generic;
using System.Linq;
using ItemStatsSystem;
using ItemStatsSystem.Items;
using Duckov.ItemUsage;

namespace EfDEnhanced.Utils;

/// <summary>
/// 玩家物品来源筛选器
/// </summary>
[System.Flags]
public enum ItemSourceFilter
{
    None = 0,
    CharacterInventory = 1 << 0,
    PetInventory = 1 << 1,
    SlotCollection = 1 << 2,
    StorageInventory = 1 << 3,
    All = CharacterInventory | PetInventory | SlotCollection | StorageInventory
}

/// <summary>
/// 物品类型检查器
/// </summary>
public static class ItemTypeChecker
{
    /// <summary>
    /// 检查物品是否为枪支
    /// </summary>
    public static bool IsGun(Item item) => item?.GetBool("IsGun", false) ?? false;

    /// <summary>
    /// 检查物品是否为弹药
    /// </summary>
    public static bool IsAmmo(Item item) => item?.GetBool("IsBullet", false) ?? false;

    /// <summary>
    /// 检查物品是否为药品
    /// </summary>
    public static bool IsMedicine(Item item)
    {
        if (item?.UsageUtilities?.behaviors == null) return false;
        return item.UsageUtilities.behaviors.Any(behavior => behavior is Drug);
    }

    /// <summary>
    /// 检查物品是否为食物
    /// </summary>
    public static bool IsFood(Item item)
    {
        if (item?.UsageUtilities?.behaviors == null) return false;
        return item.UsageUtilities.behaviors.Any(behavior => behavior is FoodDrink);
    }

    /// <summary>
    /// 获取物品口径
    /// </summary>
    public static string? GetCaliber(Item item) => item?.Constants?.GetString("Caliber".GetHashCode());
}

public static class InventoryHelper
{
    /// <summary>
    /// 获取玩家所有库存数据
    /// </summary>
    /// <returns>包含所有库存数据的元组，如果玩家数据不可用则返回null</returns>
    public static (Inventory inventory, Inventory petInventory, SlotCollection slotCollection, Inventory storageInventory)? GetPlayerInventoryAndCharacter()
    {
        try
        {
            var main = CharacterMainControl.Main;
            if (main?.CharacterItem == null)
            {
                ModLogger.LogWarning("InventoryHelper", "CharacterMainControl.Main or CharacterItem is null");
                return null;
            }

            return (
                main.CharacterItem.Inventory,
                PetProxy.PetInventory,
                main.CharacterItem.Slots,
                PlayerStorage.Inventory
            );
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"GetPlayerInventoryAndCharacter failed: {ex}");
            return null;
        }
    }

    /// <summary>
    /// 获取玩家所有物品（包括背包、装备、宠物、仓库）
    /// </summary>
    public static List<Item> GetPlayerItems(ItemSourceFilter filter = ItemSourceFilter.All)
    {
        var inventoryData = GetPlayerInventoryAndCharacter();
        if (inventoryData == null)
        {
            ModLogger.LogWarning("InventoryHelper", "Failed to get player inventory data");
            return [];
        }

        var (inventory, petInventory, slotCollection, storageInventory) = inventoryData.Value;
        var items = new List<Item>();

        if (filter.HasFlag(ItemSourceFilter.CharacterInventory) && inventory != null)
        {
            items.AddRange(inventory.Content);
        }

        if (filter.HasFlag(ItemSourceFilter.PetInventory) && petInventory != null)
        {
            items.AddRange(petInventory.Content);
        }

        if (filter.HasFlag(ItemSourceFilter.SlotCollection) && slotCollection != null)
        {
            items.AddRange(slotCollection.list.Where(slot => slot.Content != null).Select(slot => slot.Content));
        }

        if (filter.HasFlag(ItemSourceFilter.StorageInventory) && storageInventory != null)
        {
            items.AddRange(storageInventory.Content);
        }

        // 递归获取所有嵌套物品，共享 visitedItems 避免重复访问
        var allItems = new List<Item>();
        var visitedItems = new HashSet<Item>();
        foreach (var item in items)
        {
            allItems.AddRange(GetAllNestedItems(item, visitedItems));
        }

        return allItems;
    }

    /// <summary>
    /// 递归获取物品内部的所有嵌套物品（包括 Inventory 和 Slots 中的物品）
    /// </summary>
    /// <param name="item">要检查的物品</param>
    /// <param name="visitedItems">已访问的物品集合，防止无限循环</param>
    /// <returns>包含该物品及其所有嵌套物品的列表</returns>
    public static List<Item> GetAllNestedItems(Item item, HashSet<Item>? visitedItems = null)
    {
        if (item == null)
            return [];

        if (visitedItems == null)
            visitedItems = [];

        // 防止无限循环
        if (visitedItems.Contains(item))
            return [];

        visitedItems.Add(item);

        var result = new List<Item> { item };

        try
        {
            // 获取物品内部 Inventory 中的物品
            if (item.Inventory != null)
            {
                foreach (var nestedItem in item.Inventory)
                {
                    if (nestedItem != null)
                    {
                        result.AddRange(GetAllNestedItems(nestedItem, visitedItems));
                    }
                }
            }

            // 获取物品 Slots 中的物品
            if (item.Slots != null)
            {
                foreach (var slot in item.Slots)
                {
                    if (slot?.Content != null)
                    {
                        result.AddRange(GetAllNestedItems(slot.Content, visitedItems));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"GetAllNestedItems failed for item {item.DisplayName}: {ex}");
        }

        return result;
    }

    /// <summary>
    /// 获取玩家携带的物品（排除仓库）
    /// </summary>
    public static List<Item> GetPlayerCarriedItems()
    {
        return GetPlayerItems(ItemSourceFilter.CharacterInventory | ItemSourceFilter.PetInventory | ItemSourceFilter.SlotCollection);
    }

    /// <summary>
    /// 检查是否携带特定类型的物品
    /// </summary>
    public static bool HasItemOfType<T>(ItemSourceFilter filter = ItemSourceFilter.All) where T : class
    {
        return GetPlayerItems(filter).Any(item => item?.GetComponent<T>() != null);
    }

    /// <summary>
    /// 检查是否携带枪支
    /// </summary>
    public static bool HasGun(ItemSourceFilter filter = ItemSourceFilter.All)
    {
        return GetPlayerItems(filter).Any(ItemTypeChecker.IsGun);
    }

    /// <summary>
    /// 检查是否携带弹药
    /// </summary>
    public static bool HasAmmo(ItemSourceFilter filter = ItemSourceFilter.All)
    {
        return GetPlayerItems(filter).Any(ItemTypeChecker.IsAmmo);
    }

    /// <summary>
    /// 检查是否携带药品
    /// </summary>
    public static bool HasMedicine(ItemSourceFilter filter = ItemSourceFilter.All)
    {
        return GetPlayerItems(filter).Any(ItemTypeChecker.IsMedicine);
    }

    /// <summary>
    /// 检查是否携带食物
    /// </summary>
    public static bool HasFood(ItemSourceFilter filter = ItemSourceFilter.All)
    {
        return GetPlayerItems(filter).Any(ItemTypeChecker.IsFood);
    }

    /// <summary>
    /// 获取所有枪支
    /// </summary>
    public static List<Item> GetGuns(ItemSourceFilter filter = ItemSourceFilter.All)
    {
        return GetPlayerItems(filter).Where(ItemTypeChecker.IsGun).ToList();
    }

    /// <summary>
    /// 获取所有弹药
    /// </summary>
    public static List<Item> GetAmmo(ItemSourceFilter filter = ItemSourceFilter.All)
    {
        return GetPlayerItems(filter).Where(ItemTypeChecker.IsAmmo).ToList();
    }

    /// <summary>
    /// 统计特定类型物品的数量
    /// </summary>
    public static int CountItemsOfType(int typeID, ItemSourceFilter filter = ItemSourceFilter.All)
    {
        return GetPlayerItems(filter)
            .Where(item => item?.TypeID == typeID)
            .Sum(item => Math.Max(1, item.StackCount));
    }

    /// <summary>
    /// 统计特定口径弹药的数量
    /// </summary>
    public static int CountAmmoByCaliber(string caliber, ItemSourceFilter filter = ItemSourceFilter.All)
    {
        return GetPlayerItems(filter)
            .Where(item => ItemTypeChecker.IsAmmo(item) && ItemTypeChecker.GetCaliber(item) == caliber)
            .Sum(item => Math.Max(1, item.StackCount));
    }

    /// <summary>
    /// 检查物品是否在指定位置
    /// </summary>
    public static bool IsItemInLocation(Item item, ItemSourceFilter filter = ItemSourceFilter.All)
    {
        if (item == null) return false;

        var items = GetPlayerItems(filter);
        return items.Any(i => i.TypeID == item.TypeID);
    }

    /// <summary>
    /// 检查物品是否在玩家身上（排除仓库）
    /// </summary>
    public static bool IsItemCarried(Item item)
    {
        return IsItemInLocation(item, ItemSourceFilter.CharacterInventory | ItemSourceFilter.PetInventory | ItemSourceFilter.SlotCollection);
    }
}
