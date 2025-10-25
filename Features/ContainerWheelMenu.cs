using System;
using System.Collections.Generic;
using ItemStatsSystem;
using UnityEngine;
using EfDEnhanced.Utils;
using EfDEnhanced.Utils.UI.Components;
using System.Linq;

namespace EfDEnhanced.Features
{
    /// <summary>
    /// Container wheel menu that displays when a container item is invoked
    /// Shows items from container inventory in a radial pie menu
    /// </summary>
    public class ContainerWheelMenu : GenericWheelMenuBase<string>
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

        protected override void OnItemInvoked(string key)
        {
            if (string.IsNullOrEmpty(key)) return;
            try
            {
                if (key.StartsWith("idx:"))
                {
                    UseSlotItem(int.Parse(key[4..]));
                }
                else if (key.StartsWith("id:"))
                {
                    UseContainerItem(key[3..]);
                }
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"ContainerWheelMenu: Failed to invoke item: {ex}");
            }
        }

        public bool Show(Item container)
        {
            if (PieMenu == null) return false;

            try
            {
                if (!ItemUsageHelper.IsContainer(container))
                {
                    ModLogger.LogWarning($"ContainerWheelMenu: Item {container.DisplayName} is not a container");
                    return false;
                }

                // Check if container is empty
                var containerItems = ItemUsageHelper.GetContainerItems(container);

                // If container is empty, don't open the menu
                if (containerItems.Count == 0)
                {
                    ModLogger.Log("ContainerWheelMenu", $"Container {container.DisplayName} is empty, skipping menu");
                    return false;
                }

                _currentContainer = container;
                RefreshItems();
                PieMenu.Show();

                ModLogger.Log("ContainerWheelMenu", $"Showing container menu for: {container.DisplayName}");
                return true;
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"ContainerWheelMenu: Failed to show container menu: {ex}");
                return false;
            }
        }

        protected override void RefreshItems()
        {
            if (_currentContainer == null) return;

            try
            {
                List<PieMenuItem> items = [];

                if (_currentContainer.Slots != null && _currentContainer.Slots.Count > 0)
                {
                    foreach (var slot in _currentContainer.Slots)
                    {
                        var item = slot.Content;
                        var index = _currentContainer.Slots.list.IndexOf(slot);
                        if (item != null)
                        {
                            items.Add(new PieMenuItem($"idx:{index}", item.Icon, item.StackCount, item.DisplayName));
                            ModLogger.Log("ContainerWheelMenu", $"Item idx:{index}: {item.DisplayName}");
                        }
                        else
                        {
                            items.Add(new PieMenuItem($"", null, 1, "Empty"));
                            ModLogger.Log("ContainerWheelMenu", $"Slot {slot.Key}: {slot.DisplayName}");
                        }
                    }
                }
                else
                {
                    var containerItems = ItemUsageHelper.GetContainerItems(_currentContainer);
                    foreach (var item in containerItems)
                    {
                        items.Add(new PieMenuItem($"id:{item.TypeID}", item.Icon, item.StackCount, item.DisplayName));
                        ModLogger.Log("ContainerWheelMenu", $"Item id:{item.TypeID}: {item.DisplayName}");
                    }
                }

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

        private void UseSlotItem(int slotIndex)
        {
            try
            {
                if (Character == null)
                {
                    ModLogger.LogWarning("ContainerWheelMenu: Character is null, cannot use item");
                }
                if (_currentContainer == null)
                {
                    ModLogger.LogWarning("ContainerWheelMenu: No container is currently open");
                    return;
                }

                var slot = _currentContainer.Slots[slotIndex];
                var item = slot.Content;
                if (item != null)
                {
                    ItemUsageHelper.UseItem(item);
                }
                else
                {
                    ModLogger.LogWarning($"ContainerWheelMenu: No item at slot {slotIndex}");
                }
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"ContainerWheelMenu: Failed to use slot item {slotIndex}: {ex}");
            }
        }

        private void UseContainerItem(string typeID)
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

                var item = containerItems.First(i => i.TypeID.ToString() == typeID);
                if (item == null)
                {
                    ModLogger.LogWarning($"ContainerWheelMenu: No item with typeID {typeID}");
                    return;
                }
                // Use the item helper to handle the item
                ItemUsageHelper.UseItem(item);
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"ContainerWheelMenu: Failed to use item {typeID}: {ex}");
            }
        }
    }
}
