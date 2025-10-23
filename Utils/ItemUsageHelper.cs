using System;
using ItemStatsSystem;
using EfDEnhanced.Utils;

namespace EfDEnhanced.Utils;

/// <summary>
/// Helper class for handling item usage logic
/// Provides centralized methods for using different types of items
/// </summary>
public static class ItemUsageHelper
{
    /// <summary>
    /// Use an item with the character
    /// Handles different item types: containers, usable items, skills, and hand-held items
    /// </summary>
    /// <param name="item">The item to use</param>
    /// <param name="character">The character using the item</param>
    /// <returns>True if the item was successfully used or handled, false otherwise</returns>
    public static bool UseItem(Item item)
    {
        var character = CharacterMainControl.Main;
        try
        {
            if (item == null)
            {
                ModLogger.LogWarning("ItemUsageHelper", "Item is null, cannot use");
                return false;
            }

            if (character == null)
            {
                ModLogger.LogWarning("ItemUsageHelper", "Character is null, cannot use item");
                return false;
            }

            // Check if item is a container
            if (item.Tags.Contains("Container") || item.Tags.Contains("Continer"))
            {
                ModLogger.Log("ItemUsageHelper", $"Item is a container: {item.DisplayName}");
                return true; // Container items are handled by ContainerWheelMenu
            }

            // Check if item is usable through UsageUtilities
            if (item.UsageUtilities != null && item.UsageUtilities.IsUsable(item, character))
            {
                character.UseItem(item);
                ModLogger.Log("ItemUsageHelper", $"Used item: {item.DisplayName}");
                return true;
            }

            // Check if item is a skill
            if (item.GetBool("IsSkill"))
            {
                character.ChangeHoldItem(item);
                ModLogger.Log("ItemUsageHelper", $"Equipped skill: {item.DisplayName}");
                return true;
            }

            // Check if item is hand-held
            if (item.HasHandHeldAgent)
            {
                character.ChangeHoldItem(item);
                ModLogger.Log("ItemUsageHelper", $"Equipped item: {item.DisplayName}");
                return true;
            }

            ModLogger.LogWarning($"ItemUsageHelper: Item {item.DisplayName} is not usable");
            return false;
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"ItemUsageHelper: Failed to use item: {ex}");
            return false;
        }
    }

    /// <summary>
    /// Check if an item is a container
    /// </summary>
    /// <param name="item">The item to check</param>
    /// <returns>True if the item is a container, false otherwise</returns>
    public static bool IsContainer(Item item)
    {
        return item != null && (item.Tags.Contains("Container") || item.Tags.Contains("Continer"));
    }

    /// <summary>
    /// Get items from a container
    /// </summary>
    /// <param name="containerItem">The container item</param>
    /// <returns>List of items in the container inventory, or empty list if container has no inventory</returns>
    public static System.Collections.Generic.List<Item> GetContainerItems(Item containerItem)
    {
        try
        {
            if (containerItem == null)
            {
                ModLogger.LogWarning("ItemUsageHelper", "Container item is null");
                return new System.Collections.Generic.List<Item>();
            }

            if (!IsContainer(containerItem))
            {
                ModLogger.LogWarning($"ItemUsageHelper", "Item {containerItem.DisplayName} is not a container");
                return [];
            }

            var items = new System.Collections.Generic.List<Item>();

            // Get items from container's inventory
            if (containerItem.Inventory != null)
            {
                foreach (var item in containerItem.Inventory)
                {
                    if (item != null)
                    {
                        items.Add(item);
                    }
                }
            }

            // Get items from container's slots
            if (containerItem.Slots != null)
            {
                foreach (var slot in containerItem.Slots)
                {
                    if (slot != null && slot.Content != null)
                    {
                        items.Add(slot.Content);
                    }
                }
            }

            ModLogger.Log("ItemUsageHelper", $"Retrieved {items.Count} items from container {containerItem.DisplayName}");
            return items;
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"ItemUsageHelper: Failed to get container items: {ex}");
            return new System.Collections.Generic.List<Item>();
        }
    }
}
