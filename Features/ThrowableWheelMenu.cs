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

            public int TotalCount => Items.Sum(item => item.StackCount);
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
                if (stackIndex < 0 || stackIndex >= _throwableStacks.Count)
                {
                    ModLogger.LogWarning($"ThrowableWheelMenu: Stack index out of range: {stackIndex}");
                    return;
                }

                // Get the stack and use the first item
                ThrowableStack stack = _throwableStacks[stackIndex];
                if (stack.Items.Count > 0)
                {
                    var item = stack.Items[0];
                    if (item != null)
                    {
                        ItemUsageHelper.UseItem(item);
                    }
                }
                else
                {
                    ModLogger.LogWarning($"ThrowableWheelMenu: Stack has no items: {stack.DisplayName}");
                }
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"ThrowableWheelMenu: Failed to invoke item: {ex}");
            }
        }

        /// <summary>
        /// Check if an item is a throwable (grenade, etc.)
        /// </summary>
        private bool IsThrowableItem(Item item)
        {
            try
            {
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
            catch (Exception ex)
            {
                ModLogger.LogError($"ThrowableWheelMenu: Error checking if item is throwable: {ex}");
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

                // Filter throwable items
                foreach (Item item in allItems)
                {
                    if (item == null) continue;

                    // Check if item is a throwable
                    if (IsThrowableItem(item))
                    {
                        _throwableItems.Add(item);

                        int typeID = item.TypeID;

                        // Add to existing stack or create new one
                        if (!stacksByTypeID.ContainsKey(typeID))
                        {
                            stacksByTypeID[typeID] = new ThrowableStack
                            {
                                TypeID = typeID.ToString(),
                                Icon = item.Icon,
                                DisplayName = item.DisplayName
                            };
                        }

                        stacksByTypeID[typeID].Items.Add(item);
                    }
                }

                // Convert stacks to list and create menu items
                foreach (var stack in stacksByTypeID.Values)
                {
                    _throwableStacks.Add(stack);

                    // Create menu item with icon and count
                    menuItems.Add(new PieMenuItem(
                        // Use the current stack index as the menu item ID
                        (_throwableStacks.Count - 1).ToString(),
                        stack.Icon,
                        stack.TotalCount,
                        stack.DisplayName
                    ));

                    ModLogger.Log("ThrowableWheelMenu",
                        $"Added throwable stack: {stack.DisplayName} x{stack.TotalCount} ({stack.Items.Count} items)");
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

