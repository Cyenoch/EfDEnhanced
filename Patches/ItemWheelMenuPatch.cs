using System;
using HarmonyLib;
using UnityEngine;
using EfDEnhanced.Features;
using EfDEnhanced.Utils;

namespace EfDEnhanced.Patches
{
    /// <summary>
    /// Patch to enable item wheel menu with ~ key
    /// Monitors input and toggles the wheel menu
    /// Blocks mouse input while menu is open but allows keyboard input
    /// </summary>
    [HarmonyPatch]
    public class ItemWheelMenuPatch : PieMenuPatternBase
    {
        private static ItemWheelMenu? _wheelMenu;
        private static bool _wheelMenuInitialized = false;

        /// <summary>
        /// Patch CharacterInputControl.Update to monitor for ~ key press/release and capture input control instance
        /// </summary>
        [HarmonyPatch(typeof(CharacterInputControl), "Update")]
        [HarmonyPostfix]
        public static void CharacterInputControl_Update_Postfix(CharacterInputControl __instance)
        {
            try
            {
                // Capture the input control instance for clearing input state
                if (_inputControl == null)
                {
                    _inputControl = __instance;
                }

                // Initialize wheel menu if needed
                if (!_wheelMenuInitialized)
                {
                    InitializeWheelMenu();
                }

                // Check if we're in a state where the wheel menu can be opened
                CancelIfGameStateBlocks(_wheelMenu);
                if (GameManager.Paused || Duckov.UI.View.ActiveView != null)
                {
                    return;
                }

                // Check for configured hotkey press to show menu
                KeyCode hotkey = ModSettings.ItemWheelMenuHotkey.Value;
                if (Input.GetKeyDown(hotkey))
                {
                    if (_wheelMenu != null && !_wheelMenu.IsOpen)
                    {
                        _wheelMenu.Show();
                        ModLogger.Log("ItemWheelMenuPatch", $"Wheel menu opened with {hotkey} key");
                    }
                }
                // Check for configured hotkey release to trigger item and hide menu
                else if (Input.GetKeyUp(hotkey))
                {
                    if (_wheelMenu != null && _wheelMenu.IsOpen)
                    {
                        // Hide with invoke - will trigger selected item if any
                        _wheelMenu.Hide(invokeSelectedItem: true);
                        ModLogger.Log("ItemWheelMenuPatch", $"Wheel menu closed with {hotkey} key release (invoke if selected)");
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.LogDetailedException(ex, "ItemWheelMenuPatch.CharacterInputControl_Update");
            }
        }

        /// <summary>
        /// Initialize the wheel menu GameObject
        /// </summary>
        private static void InitializeWheelMenu()
        {
            try
            {
                if (_wheelMenuInitialized)
                {
                    return;
                }

                GameObject menuObj = new GameObject("EfDEnhanced_ItemWheelMenu");
                _wheelMenu = menuObj.AddComponent<ItemWheelMenu>();

                // Initialize event listeners
                InitializeEventListeners();

                _wheelMenuInitialized = true;
                ModLogger.Log("ItemWheelMenuPatch", "Item wheel menu initialized successfully");
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"ItemWheelMenuPatch: Failed to initialize wheel menu: {ex}");
                _wheelMenuInitialized = false;
            }
        }

        /// <summary>
        /// Patch to cancel wheel menu when game is paused
        /// </summary>
        [HarmonyPatch(typeof(PauseMenu), "Show")]
        [HarmonyPostfix]
        public static void CancelWheelMenuOnPause()
        {
            HandlePauseMenuShow(_wheelMenu);
        }

        /// <summary>
        /// Initialize event listeners for closing the wheel menu
        /// </summary>
        private static void InitializeEventListeners()
        {
            // Subscribe to view change events
            Duckov.UI.View.OnActiveViewChanged += OnActiveViewChanged;

            // Subscribe to menu opened event to clear input state
            ItemWheelMenu.OnMenuOpened += OnWheelMenuOpened;

            ModLogger.Log("ItemWheelMenuPatch", "Subscribed to view change and menu opened events");
        }

        /// <summary>
        /// Called when wheel menu is opened - clears accumulated input state
        /// </summary>
        private static void OnWheelMenuOpened()
        {
            ClearInputState();
        }

        /// <summary>
        /// Called when active view changes (inventory, map, etc.)
        /// </summary>
        private static void OnActiveViewChanged()
        {
            HandleActiveViewChanged(_wheelMenu);
        }

    }
}

