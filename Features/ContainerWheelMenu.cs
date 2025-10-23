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
    public class ContainerWheelMenu : MonoBehaviour
    {
        private static ContainerWheelMenu? _instance;

        // Components
        private PieMenuComponent? _pieMenu;

        // State
        private Item? _currentContainer;
        private CharacterMainControl Character => CharacterMainControl.Main;

        public static ContainerWheelMenu? Instance => _instance;
        public bool IsOpen => _pieMenu != null && _pieMenu.IsOpen;
        public Item? CurrentContainer => _currentContainer;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializePieMenu();

            // Subscribe to settings changes
            ModSettings.ItemWheelScale.ValueChanged += OnScaleChanged;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }

            // Unsubscribe from settings changes
            ModSettings.ItemWheelScale.ValueChanged -= OnScaleChanged;
        }

        private void InitializePieMenu()
        {
            try
            {
                // Create pie menu GameObject
                GameObject pieMenuObj = new GameObject("ContainerPieMenu");
                pieMenuObj.transform.SetParent(transform, false);

                _pieMenu = pieMenuObj.AddComponent<PieMenuComponent>();

                // Initialize with configuration
                var config = PieMenuConfig.Default;
                config.Scale = ModSettings.ItemWheelScale.Value;
                _pieMenu.Initialize(config);

                // Subscribe to events
                _pieMenu.OnItemInvoked += OnItemInvoked;
                _pieMenu.OnMenuShown += OnMenuShown;
                _pieMenu.OnMenuHidden += OnMenuHidden;

                ModLogger.Log("ContainerWheelMenu", "Pie menu initialized successfully");
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"ContainerWheelMenu: Failed to initialize pie menu: {ex}");
            }
        }

        private void OnScaleChanged(object? sender, Utils.Settings.SettingsValueChangedEventArgs<float> e)
        {
            if (_pieMenu != null)
            {
                _pieMenu.SetScale(e.NewValue);
                ModLogger.Log("ContainerWheelMenu", $"Scale changed to {e.NewValue:F2}");
            }
        }

        private void OnMenuShown()
        {
            // Any setup needed when menu is shown
            ModLogger.Log("ContainerWheelMenu", $"Menu shown for container: {_currentContainer?.DisplayName}");
        }

        private void OnMenuHidden()
        {
            // Reset container reference when menu is hidden
            _currentContainer = null;
        }

        private void OnItemInvoked(string itemId)
        {
            try
            {
                // Parse item index from ID
                if (!int.TryParse(itemId, out int itemIndex))
                {
                    ModLogger.LogWarning($"ContainerWheelMenu: Invalid item ID: {itemId}");
                    return;
                }

                UseContainerItem(itemIndex);
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"ContainerWheelMenu: Failed to invoke item: {ex}");
            }
        }

        public void Toggle()
        {
            if (_pieMenu != null)
            {
                _pieMenu.Toggle();
            }
        }

        public void Show(Item container)
        {
            if (_pieMenu == null) return;

            try
            {
                if (!ItemUsageHelper.IsContainer(container))
                {
                    ModLogger.LogWarning($"ContainerWheelMenu: Item {container.DisplayName} is not a container");
                    return;
                }

                _currentContainer = container;
                RefreshItems();
                _pieMenu.Show();

                ModLogger.Log("ContainerWheelMenu", $"Showing container menu for: {container.DisplayName}");
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"ContainerWheelMenu: Failed to show container menu: {ex}");
            }
        }

        public void Hide(bool invokeSelectedItem = true)
        {
            if (_pieMenu != null)
            {
                _pieMenu.Hide(invokeSelectedItem);
            }
        }

        /// <summary>
        /// Cancel the wheel menu (for abnormal closes, never invokes items)
        /// </summary>
        public void Cancel()
        {
            if (_pieMenu != null)
            {
                _pieMenu.Cancel();
            }
        }

        /// <summary>
        /// Get the mouse position that should be used by the game's camera system
        /// When wheel menu is open, return saved position to prevent camera movement
        /// </summary>
        public Vector2 GetOverrideMousePosition()
        {
            if (_pieMenu != null)
            {
                return _pieMenu.GetSavedMousePosition();
            }
            return Input.mousePosition;
        }

        private void RefreshItems()
        {
            if (_pieMenu == null || _currentContainer == null) return;

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
                _pieMenu.SetItems(items);
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
