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
    /// Now uses PieMenuComponent for display
    /// </summary>
    public class ItemWheelMenu : MonoBehaviour
    {
        // Event triggered when the menu is opened
        public static event Action? OnMenuOpened;

        private static ItemWheelMenu? _instance;

        // Components
        private PieMenuComponent? _pieMenu;

        // Configuration
        private const int ITEM_COUNT = 6; // Number of hotbar slots

        // State
        private CharacterMainControl Character => CharacterMainControl.Main;

        public static ItemWheelMenu? Instance => _instance;
        public bool IsOpen => _pieMenu != null && _pieMenu.IsOpen;

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
                GameObject pieMenuObj = new GameObject("PieMenu");
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

                ModLogger.Log("ItemWheelMenu", "Pie menu initialized successfully");
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"ItemWheelMenu: Failed to initialize pie menu: {ex}");
            }
        }

        private void OnScaleChanged(object? sender, Utils.Settings.SettingsValueChangedEventArgs<float> e)
        {
            if (_pieMenu != null)
            {
                _pieMenu.SetScale(e.NewValue);
                ModLogger.Log("ItemWheelMenu", $"Scale changed to {e.NewValue:F2}");
            }
        }

        private void OnMenuShown()
        {
            // Update items when menu is shown
            RefreshItems();

            // Trigger event to clear input state in patches
            OnMenuOpened?.Invoke();
        }

        private void OnMenuHidden()
        {
            // Any cleanup needed when menu is hidden
        }

        private void OnItemInvoked(string itemId)
        {
            try
            {
                // Parse item index from ID
                if (!int.TryParse(itemId, out int slotIndex))
                {
                    ModLogger.LogWarning($"ItemWheelMenu: Invalid item ID: {itemId}");
                    return;
                }

                UseItem(slotIndex);
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"ItemWheelMenu: Failed to invoke item: {ex}");
            }
        }

        public void Toggle()
        {
            if (_pieMenu != null)
            {
                _pieMenu.Toggle();
            }
        }

        public void Show()
        {
            if (_pieMenu != null)
            {
                _pieMenu.Show();
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
            if (_pieMenu == null) return;

            try
            {
                List<PieMenuItem> items = new List<PieMenuItem>();

                for (int i = 0; i < ITEM_COUNT; i++)
                {
                    Item? item = Duckov.ItemShortcut.Get(i);

                    if (item != null && item.Icon != null)
                    {
                        items.Add(new PieMenuItem(i.ToString(), item.Icon));
                        ModLogger.Log("ItemWheelMenu", $"Slot {i}: {item.DisplayName}");
                    }
                    else
                    {
                        // Add empty slot
                        items.Add(new PieMenuItem(i.ToString(), null));
                    }
                }

                _pieMenu.SetItems(items);
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

                // Use the same logic as ItemShortcutButton
                if (item.UsageUtilities != null && item.UsageUtilities.IsUsable(item, Character))
                {
                    Character.UseItem(item);
                    ModLogger.Log("ItemWheelMenu", $"Used item: {item.DisplayName}");
                }
                else if (item.GetBool("IsSkill"))
                {
                    Character.ChangeHoldItem(item);
                    ModLogger.Log("ItemWheelMenu", $"Equipped skill: {item.DisplayName}");
                }
                else if (item.HasHandHeldAgent)
                {
                    Character.ChangeHoldItem(item);
                    ModLogger.Log("ItemWheelMenu", $"Equipped item: {item.DisplayName}");
                }
                else
                {
                    ModLogger.LogWarning($"ItemWheelMenu: Item {item.DisplayName} is not usable");
                }
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"ItemWheelMenu: Failed to use item: {ex}");
            }
        }
    }
}
