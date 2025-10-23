using System;
using System.Collections.Generic;
using ItemStatsSystem;
using UnityEngine;
using EfDEnhanced.Utils;
using EfDEnhanced.Utils.UI.Components;

namespace EfDEnhanced.Features
{
    /// <summary>
    /// Container wheel menu that displays when a container item is invoked
    /// Shows items from container inventory in a radial pie menu
    /// </summary>
    public class ContainerWheelMenu : GenericWheelMenuBase<int>
    {
        private static ContainerWheelMenu? _instance;

        // State
        private Item? _currentContainer;

        public static ContainerWheelMenu? Instance => _instance;
        public Item? CurrentContainer => _currentContainer;

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

        protected override void OnMenuHidden()
        {
            // Reset container reference when menu is hidden
            _currentContainer = null;
        }

        protected override void OnItemInvoked(int itemIndex)
        {
            try
            {
                UseContainerItem(itemIndex);
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"ContainerWheelMenu: Failed to invoke item: {ex}");
            }
        }

        public void Show(Item container)
        {
            if (PieMenu == null) return;

            try
            {
                if (!ItemUsageHelper.IsContainer(container))
                {
                    ModLogger.LogWarning($"ContainerWheelMenu: Item {container.DisplayName} is not a container");
                    return;
                }

                _currentContainer = container;
                RefreshItems();
                PieMenu.Show();

                ModLogger.Log("ContainerWheelMenu", $"Showing container menu for: {container.DisplayName}");
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"ContainerWheelMenu: Failed to show container menu: {ex}");
            }
        }

        protected override void RefreshItems()
        {
            if (_currentContainer == null) return;

            try
            {
                List<PieMenuItem> items = [];
                var containerItems = ItemUsageHelper.GetContainerItems(_currentContainer);

                for (int i = 0; i < containerItems.Count; i++)
                {
                    Item? item = containerItems[i];

                    if (item != null && item.Icon != null)
                    {
                        items.Add(new PieMenuItem(i.ToString(), item.Icon, item.StackCount, item.DisplayName));
                        ModLogger.Log("ContainerWheelMenu", $"Item {i}: {item.DisplayName}");
                    }
                }

                for (int i = 0; i < _currentContainer.Slots.Count; i++)
                {
                    var slot = _currentContainer.Slots[i];
                    if (slot != null && slot.Content != null)
                    {
                        items.Add(new PieMenuItem(i.ToString(), slot.Content.Icon, slot.Content.StackCount, slot.Content.DisplayName));
                        ModLogger.Log("ContainerWheelMenu", $"Slot {i}: {slot.Content.DisplayName}");
                    }
                }

                // If container is empty, add one empty slot to show something
                if (items.Count == 0)
                {
                    items.Add(new PieMenuItem("0", null, 1, "Empty"));
                    ModLogger.Log("ContainerWheelMenu", "Container is empty");
                }

                // 限制菜单显示的最大物品数量（例如最多12个）
                const int MAX_ITEMS = 12;
                if (items.Count > MAX_ITEMS)
                {
                    items = items.GetRange(0, MAX_ITEMS);
                    ModLogger.Log("ContainerWheelMenu", $"Item count exceeds {MAX_ITEMS}, only showing first {MAX_ITEMS} items.");
                }
                SetMenuItems(items);
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"ContainerWheelMenu: Failed to refresh items: {ex}");
            }
        }

        private void UseContainerItem(int itemIndex)
        {
            try
            {
                if (Character == null)
                {
                    ModLogger.LogWarning("ContainerWheelMenu: Character is null, cannot use item");
                    return;
                }

                if (_currentContainer == null)
                {
                    ModLogger.LogWarning("ContainerWheelMenu: No container is currently open");
                    return;
                }

                var containerItems = ItemUsageHelper.GetContainerItems(_currentContainer);

                if (itemIndex < 0 || itemIndex >= containerItems.Count)
                {
                    ModLogger.LogWarning($"ContainerWheelMenu: Item index {itemIndex} is out of range");
                    return;
                }

                Item? item = containerItems[itemIndex];

                if (item == null)
                {
                    ModLogger.LogWarning($"ContainerWheelMenu: No item at index {itemIndex}");
                    return;
                }

                // Use the item helper to handle the item
                ItemUsageHelper.UseItem(item);
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"ContainerWheelMenu: Failed to use item: {ex}");
            }
        }
    }
}
