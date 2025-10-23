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
    /// Uses PieMenuComponent for display
    /// </summary>
    public class ThrowableWheelMenu : MonoBehaviour
    {
        // Event triggered when the menu is opened
        public static event Action? OnMenuOpened;

        private static ThrowableWheelMenu? _instance;

        // Components
        private PieMenuComponent? _pieMenu;

        // State
        private CharacterMainControl Character => CharacterMainControl.Main;
        private List<Item> _throwableItems = new List<Item>();
        private List<ThrowableStack> _throwableStacks = new List<ThrowableStack>();

        // Helper class to store stacked items
        private class ThrowableStack
        {
            public string TypeID { get; set; } = "";
            public List<Item> Items { get; set; } = new List<Item>();
            public Sprite? Icon { get; set; }
            public string DisplayName { get; set; } = "";

            public int TotalCount => Items.Sum(item => item.StackCount);
        }

        public static ThrowableWheelMenu? Instance => _instance;
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

            // Subscribe to settings changes (use shared ItemWheelScale)
            ModSettings.ItemWheelScale.ValueChanged += OnScaleChanged;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }

            // Unsubscribe from settings changes (use shared ItemWheelScale)
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

                // Initialize with configuration (use shared ItemWheelScale)
                var config = PieMenuConfig.Default;
                config.Scale = ModSettings.ItemWheelScale.Value;
                _pieMenu.Initialize(config);

                // Subscribe to events
                _pieMenu.OnItemInvoked += OnItemInvoked;
                _pieMenu.OnMenuShown += OnMenuShown;
                _pieMenu.OnMenuHidden += OnMenuHidden;

                ModLogger.Log("ThrowableWheelMenu", "Pie menu initialized successfully");
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"ThrowableWheelMenu: Failed to initialize pie menu: {ex}");
            }
        }

        private void OnScaleChanged(object? sender, Utils.Settings.SettingsValueChangedEventArgs<float> e)
        {
            if (_pieMenu != null)
            {
                _pieMenu.SetScale(e.NewValue);
                ModLogger.Log("ThrowableWheelMenu", $"Scale changed to {e.NewValue:F2}");
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
                // Parse stack index from ID
                if (!int.TryParse(itemId, out int stackIndex))
                {
                    ModLogger.LogWarning($"ThrowableWheelMenu: Invalid item ID: {itemId}");
                    return;
                }

                if (stackIndex < 0 || stackIndex >= _throwableStacks.Count)
                {
                    ModLogger.LogWarning($"ThrowableWheelMenu: Stack index out of range: {stackIndex}");
                    return;
                }

                // Get the stack and use the first item
                ThrowableStack stack = _throwableStacks[stackIndex];
                if (stack.Items.Count > 0)
                {
                    UseThrowableItem(stack.Items[0]);
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

        private void RefreshItems()
        {
            if (_pieMenu == null) return;

            try
            {
                _throwableItems.Clear();
                _throwableStacks.Clear();
                List<PieMenuItem> menuItems = new List<PieMenuItem>();

                // Get character item (contains inventory)
                Item? characterItem = Character?.CharacterItem;
                if (characterItem == null || characterItem.Inventory == null)
                {
                    ModLogger.LogWarning("ThrowableWheelMenu: Character or inventory is null");
                    _pieMenu.SetItems(menuItems);
                    return;
                }

                // Dictionary to group items by TypeID
                Dictionary<int, ThrowableStack> stacksByTypeID = new Dictionary<int, ThrowableStack>();

                // Iterate through all items in character inventory
                foreach (Item item in characterItem.Inventory)
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
                        (_throwableStacks.Count - 1).ToString(),
                        stack.Icon,
                        stack.TotalCount
                    ));

                    ModLogger.Log("ThrowableWheelMenu",
                        $"Added throwable stack: {stack.DisplayName} x{stack.TotalCount} ({stack.Items.Count} items)");
                }

                ModLogger.Log("ThrowableWheelMenu",
                    $"Found {_throwableItems.Count} throwable items in {_throwableStacks.Count} stacks");

                _pieMenu.SetItems(menuItems);
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"ThrowableWheelMenu: Failed to refresh items: {ex}");
            }
        }

        private void UseThrowableItem(Item item)
        {
            try
            {
                if (Character == null)
                {
                    ModLogger.LogWarning("ThrowableWheelMenu: Character is null, cannot use item");
                    return;
                }

                if (item == null)
                {
                    ModLogger.LogWarning("ThrowableWheelMenu: Item is null");
                    return;
                }

                // Equip the throwable as a skill item
                Character.ChangeHoldItem(item);
                ModLogger.Log("ThrowableWheelMenu", $"Equipped throwable: {item.DisplayName}");
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"ThrowableWheelMenu: Failed to use throwable item: {ex}");
            }
        }
    }
}

