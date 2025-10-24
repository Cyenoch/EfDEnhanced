using System;
using System.Collections.Generic;
using ItemStatsSystem;
using UnityEngine;
using EfDEnhanced.Utils;
using EfDEnhanced.Utils.UI.Components;

namespace EfDEnhanced.Features
{
    /// <summary>
    /// Item wheel menu that displays when ~ key is pressed
    /// Shows items from hotbar slots in a radial pie menu
    /// </summary>
    public class ItemWheelMenu : GenericWheelMenuBase<int>
    {
        // Event triggered when the menu is opened
        public static event Action? OnMenuOpened;

        private static ItemWheelMenu? _instance;

        // Configuration
        private const int ITEM_COUNT = 6; // Number of hotbar slots

        public static ItemWheelMenu? Instance => _instance;

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

        protected override void OnItemInvoked(int slotIndex)
        {
            try
            {
                UseItem(slotIndex);
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"ItemWheelMenu: Failed to invoke item: {ex}");
            }
        }

        protected override void RefreshItems()
        {
            try
            {
                List<PieMenuItem> items = [];

                for (int i = 0; i < ITEM_COUNT; i++)
                {
                    Item? item = Duckov.ItemShortcut.Get(i);

                    if (item != null && item.Icon != null)
                    {
                        items.Add(new PieMenuItem(i.ToString(), item.Icon, item.StackCount, item.DisplayName));
                        ModLogger.Log("ItemWheelMenu", $"Slot {i}: {item.DisplayName}");
                    }
                    else
                    {
                        // Add empty slot
                        items.Add(new PieMenuItem(i.ToString(), null, 1, "Empty"));
                    }
                }

                SetMenuItems(items);
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"ItemWheelMenu: Failed to refresh items: {ex}");
            }
        }

        private void UseItem(int slotIndex)
        {
            try
            {
                if (Character == null)
                {
                    ModLogger.LogWarning("ItemWheelMenu: Character is null, cannot use item");
                    return;
                }

                Item? item = Duckov.ItemShortcut.Get(slotIndex);

                if (item == null)
                {
                    ModLogger.LogWarning($"ItemWheelMenu: No item in slot {slotIndex}");
                    return;
                }

                // Check if item is a container - if so, show ContainerWheelMenu instead
                if (ItemUsageHelper.IsContainer(item))
                {
                    // Close this menu and open container menu
                    Hide(false);

                    var containerMenu = ContainerWheelMenu.Instance;
                    if (containerMenu != null)
                    {
                        containerMenu.Show(item);
                    }
                    else
                    {
                        ModLogger.LogWarning("ItemWheelMenu: ContainerWheelMenu instance not found");
                    }

                    return;
                }

                // Use ItemUsageHelper to handle the item
                ItemUsageHelper.UseItem(item);
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"ItemWheelMenu: Failed to use item: {ex}");
            }
        }
    }
}
