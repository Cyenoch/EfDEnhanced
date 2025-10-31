using System;
using System.Collections.Generic;
using System.Linq;
using ItemStatsSystem;
using UnityEngine;
using EfDEnhanced.Utils;
using EfDEnhanced.Utils.UI.Components;

namespace EfDEnhanced.Features
{
    /// <summary>
    /// Throwable wheel menu that displays when G key is pressed
    /// Shows throwable items (grenades, etc.) from character inventory in a radial pie menu
    /// </summary>
    public class ThrowableWheelMenu : GenericWheelMenuBase<int>
    {
        // Event triggered when the menu is opened
        public static event Action? OnMenuOpened;

        private static ThrowableWheelMenu? _instance;

        // State
        private List<Item> _throwableItems = [];
        private List<ThrowableStack> _throwableStacks = [];

        // Helper class to store stacked items
        private class ThrowableStack
        {
            public string TypeID { get; set; } = "";
            public List<Item> Items { get; set; } = [];
            public Sprite? Icon { get; set; }
            public string DisplayName { get; set; } = "";

            public int TotalCount => Items
                .Where(item => item != null) // Filter out destroyed items
                .Sum(item => Math.Max(1, item.StackCount)); // Ensure at least 1
        }

        public static ThrowableWheelMenu? Instance => _instance;

        protected override void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            base.Awake();
        }

        protected override void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }

            base.OnDestroy();
        }

        protected override void OnMenuShown()
        {
            // Trigger event to clear input state in patches
            OnMenuOpened?.Invoke();
        }

        protected override void OnItemInvoked(int stackIndex)
        {
            try
            {
                // Validate stack index
                if (stackIndex < 0 || stackIndex >= _throwableStacks.Count)
                {
                    ModLogger.LogWarning($"ThrowableWheelMenu: Stack index out of range: {stackIndex}");
                    return;
                }

                // Get the stack
                ThrowableStack stack = _throwableStacks[stackIndex];
                
                // Validate stack exists
                if (stack == null)
                {
                    ModLogger.LogWarning($"ThrowableWheelMenu: Stack at index {stackIndex} is null");
                    return;
                }

                // Validate stack has items list
                if (stack.Items == null)
                {
                    ModLogger.LogWarning($"ThrowableWheelMenu: Stack items list is null");
                    return;
                }

                // Find first valid (non-destroyed) item with additional safety checks
                Item? validItem = null;
                foreach (Item item in stack.Items)
                {
                    try
                    {
                        // Use Unity's proper null check for destroyed objects
                        if (item == null) continue;
                        
                        // Additional validation: try to access a property to ensure object is alive
                        // If accessing TypeID throws, the object is destroyed or invalid
                        int _ = item.TypeID;
                        
                        // Additional check: verify item is still a throwable
                        if (IsThrowableItem(item))
                        {
                            validItem = item;
                            break;
                        }
                    }
                    catch (Exception itemEx)
                    {
                        // Item is destroyed or invalid, skip it
                        ModLogger.LogWarning($"ThrowableWheelMenu: Item validation failed (likely destroyed): {itemEx.Message}");
                        continue;
                    }
                }
                
                if (validItem != null)
                {
                    // Double-check before using
                    try
                    {
                        // Verify item is still valid before using
                        if (validItem == null || !IsThrowableItem(validItem))
                        {
                            ModLogger.LogWarning($"ThrowableWheelMenu: Item became invalid before use");
                            return;
                        }

                        ItemUsageHelper.UseItem(validItem);
                    }
                    catch (Exception useEx)
                    {
                        ModLogger.LogError($"ThrowableWheelMenu: Failed to use item: {useEx}");
                        // Don't rethrow, just log the error
                    }
                }
                else
                {
                    ModLogger.LogWarning($"ThrowableWheelMenu: Stack has no valid items: {stack.DisplayName ?? "Unknown"}");
                }
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"ThrowableWheelMenu: Failed to invoke item: {ex}");
                // Don't rethrow to prevent crash
            }
        }

        /// <summary>
        /// Check if an item is a throwable (grenade, etc.)
        /// </summary>
        private bool IsThrowableItem(Item item)
        {
            try
            {
                // Check if item is null or destroyed
                if (item == null) return false;

                // Additional safety: try to access a property to verify object is alive
                // This will throw if the object has been destroyed
                try
                {
                    int _ = item.TypeID;
                }
                catch
                {
                    // Object is destroyed, return false
                    return false;
                }

                // Check if item is a skill
                if (!item.GetBool("IsSkill"))
                {
                    return false;
                }

                // Check if item has ItemSetting_Skill component
                ItemSetting_Skill? skillSetting = item.GetComponent<ItemSetting_Skill>();
                if (skillSetting == null)
                {
                    return false;
                }

                // Check if skill is Skill_Grenade type
                return skillSetting.Skill is Skill_Grenade;
            }
            catch (NullReferenceException)
            {
                // Item was destroyed during check
                return false;
            }
            catch (Exception ex)
            {
                ModLogger.LogWarning($"ThrowableWheelMenu: Error checking if item is throwable: {ex.Message}");
                return false;
            }
        }

        protected override void RefreshItems()
        {
            try
            {
                _throwableItems.Clear();
                _throwableStacks.Clear();
                List<PieMenuItem> menuItems = [];

                // 使用 InventoryHelper 获取角色背包中的所有物品（包括嵌套物品）
                var allItems = InventoryHelper.GetPlayerItems(ItemSourceFilter.CharacterInventory);
                if (allItems.Count == 0)
                {
                    ModLogger.LogWarning("ThrowableWheelMenu: No items found in inventory");
                    SetMenuItems(menuItems);
                    return;
                }

                // Dictionary to group items by TypeID
                Dictionary<int, ThrowableStack> stacksByTypeID = [];
                // Use HashSet to track added items and prevent duplicate counting
                HashSet<Item> processedItems = [];

                // Filter throwable items
                foreach (Item item in allItems)
                {
                    try
                    {
                        // Check if item is null or destroyed (Unity's == null check works for destroyed objects)
                        if (item == null) continue;

                        // Skip if already processed (prevent duplicate counting)
                        if (processedItems.Contains(item)) continue;

                        // Check if item is a throwable
                        if (IsThrowableItem(item))
                        {
                            // Additional safety: verify item is still valid before accessing properties
                            int typeID;
                            Sprite? icon;
                            string displayName;
                            
                            try
                            {
                                typeID = item.TypeID;
                                icon = item.Icon;
                                displayName = item.DisplayName ?? "Unknown";
                            }
                            catch (Exception propEx)
                            {
                                // Item was destroyed during processing, skip it
                                ModLogger.LogWarning($"ThrowableWheelMenu: Item destroyed during refresh: {propEx.Message}");
                                continue;
                            }

                            // Mark as processed
                            processedItems.Add(item);
                            _throwableItems.Add(item);

                            // Add to existing stack or create new one
                            if (!stacksByTypeID.ContainsKey(typeID))
                            {
                                stacksByTypeID[typeID] = new ThrowableStack
                                {
                                    TypeID = typeID.ToString(),
                                    Icon = icon,
                                    DisplayName = displayName
                                };
                            }

                            stacksByTypeID[typeID].Items.Add(item);
                        }
                    }
                    catch (Exception itemEx)
                    {
                        // Skip this item if any error occurs
                        ModLogger.LogWarning($"ThrowableWheelMenu: Error processing item: {itemEx.Message}");
                        continue;
                    }
                }

                // Convert stacks to list and create menu items
                // Filter out stacks with no valid items
                foreach (var kvp in stacksByTypeID)
                {
                    var stack = kvp.Value;
                    
                    // Filter out destroyed items from the stack
                    stack.Items = stack.Items.Where(item => item != null).ToList();
                    
                    // Skip stacks with no valid items
                    if (stack.Items.Count == 0) continue;
                    
                    // Skip stacks with zero total count
                    int totalCount = stack.TotalCount;
                    if (totalCount <= 0) continue;

                    _throwableStacks.Add(stack);

                    // Create menu item with icon and count
                    menuItems.Add(new PieMenuItem(
                        // Use the current stack index as the menu item ID
                        (_throwableStacks.Count - 1).ToString(),
                        stack.Icon,
                        totalCount,
                        stack.DisplayName
                    ));

                    ModLogger.Log("ThrowableWheelMenu",
                        $"Added throwable stack: {stack.DisplayName} x{totalCount} ({stack.Items.Count} items)");
                }

                ModLogger.Log("ThrowableWheelMenu",
                    $"Found {_throwableItems.Count} throwable items in {_throwableStacks.Count} stacks");

                SetMenuItems(menuItems);
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"ThrowableWheelMenu: Failed to refresh items: {ex}");
            }
        }
    }
}

