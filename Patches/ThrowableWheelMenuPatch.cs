using System;
using HarmonyLib;
using UnityEngine;
using EfDEnhanced.Features;
using EfDEnhanced.Utils;

namespace EfDEnhanced.Patches
{
    /// <summary>
    /// Patch to enable throwable wheel menu with G key (configurable)
    /// Monitors input and toggles the wheel menu for throwable items
    /// Blocks mouse input while menu is open but allows keyboard input
    /// </summary>
    [HarmonyPatch]
    public class ThrowableWheelMenuPatch
    {
        private static ThrowableWheelMenu? _wheelMenu;
        private static bool _wheelMenuInitialized = false;
        private static CharacterInputControl? _inputControl;

        /// <summary>
        /// Patch CharacterInputControl.Update to monitor for G key press/release and capture input control instance
        /// </summary>
        [HarmonyPatch(typeof(CharacterInputControl), "Update")]
        [HarmonyPostfix]
        public static void CharacterInputControl_Update_Postfix(CharacterInputControl __instance)
        {
            try
            {
                // Check if throwable wheel menu is enabled
                if (!ModSettings.ThrowableWheelEnabled.Value)
                {
                    return;
                }

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
                if (GameManager.Paused || Duckov.UI.View.ActiveView != null)
                {
                    if (_wheelMenu != null && _wheelMenu.IsOpen)
                    {
                        // Cancel menu when game state changes (no invoke)
                        _wheelMenu.Cancel();
                    }
                    return;
                }

                // Check for configured hotkey press to show menu
                KeyCode hotkey = ModSettings.ThrowableWheelHotkey.Value;
                if (Input.GetKeyDown(hotkey))
                {
                    if (_wheelMenu != null && !_wheelMenu.IsOpen)
                    {
                        _wheelMenu.Show();
                        ModLogger.Log("ThrowableWheelMenuPatch", $"Throwable wheel menu opened with {hotkey} key");
                    }
                }
                // Check for configured hotkey release to trigger item and hide menu
                else if (Input.GetKeyUp(hotkey))
                {
                    if (_wheelMenu != null && _wheelMenu.IsOpen)
                    {
                        // Hide with invoke - will trigger selected item if any
                        _wheelMenu.Hide(invokeSelectedItem: true);
                        ModLogger.Log("ThrowableWheelMenuPatch", $"Throwable wheel menu closed with {hotkey} key release (invoke if selected)");
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.LogDetailedException(ex, "ThrowableWheelMenuPatch.CharacterInputControl_Update");
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

                GameObject menuObj = new GameObject("EfDEnhanced_ThrowableWheelMenu");
                _wheelMenu = menuObj.AddComponent<ThrowableWheelMenu>();

                // Initialize event listeners
                InitializeEventListeners();

                _wheelMenuInitialized = true;
                ModLogger.Log("ThrowableWheelMenuPatch", "Throwable wheel menu initialized successfully");
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"ThrowableWheelMenuPatch: Failed to initialize wheel menu: {ex}");
                _wheelMenuInitialized = false;
            }
        }

        /// <summary>
        /// Patch to block and clear mouse delta (camera rotation) when wheel menu is open
        /// This prevents accumulated mouse movement from affecting the camera
        /// </summary>
        [HarmonyPatch(typeof(CharacterInputControl), "OnPlayerMouseDelta")]
        [HarmonyPrefix]
        public static bool PreventMouseDeltaWhenWheelOpen()
        {
            try
            {
                if (_wheelMenu != null && _wheelMenu.IsOpen)
                {
                    // Block mouse delta to prevent camera rotation
                    // The input system will discard this delta
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                ExceptionHelper.LogDetailedException(ex, "ThrowableWheelMenuPatch.PreventMouseDeltaWhenWheelOpen");
                return true;
            }
        }

        /// <summary>
        /// Patch to block mouse movement affecting aim when wheel menu is open
        /// </summary>
        [HarmonyPatch(typeof(CharacterInputControl), "OnPlayerMouseMove")]
        [HarmonyPrefix]
        public static bool PreventMouseMoveWhenWheelOpen()
        {
            try
            {
                if (_wheelMenu != null && _wheelMenu.IsOpen)
                {
                    // Block mouse position updates to prevent aim changes
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                ExceptionHelper.LogDetailedException(ex, "ThrowableWheelMenuPatch.PreventMouseMoveWhenWheelOpen");
                return true;
            }
        }

        /// <summary>
        /// Patch to block shooting/trigger input when wheel menu is open
        /// </summary>
        [HarmonyPatch(typeof(CharacterInputControl), "OnPlayerTriggerInputUsingMouseKeyboard")]
        [HarmonyPrefix]
        public static bool PreventTriggerWhenWheelOpen()
        {
            try
            {
                if (_wheelMenu != null && _wheelMenu.IsOpen)
                {
                    // Block trigger input to prevent shooting while using the wheel
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                ExceptionHelper.LogDetailedException(ex, "ThrowableWheelMenuPatch.PreventTriggerWhenWheelOpen");
                return true;
            }
        }

        /// <summary>
        /// Patch to cancel wheel menu when game is paused
        /// </summary>
        [HarmonyPatch(typeof(PauseMenu), "Show")]
        [HarmonyPostfix]
        public static void CancelWheelMenuOnPause()
        {
            try
            {
                if (_wheelMenu != null && _wheelMenu.IsOpen)
                {
                    _wheelMenu.Cancel();
                    ModLogger.Log("ThrowableWheelMenuPatch", "Cancelled throwable wheel menu due to pause");
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.LogDetailedException(ex, "ThrowableWheelMenuPatch.CancelWheelMenuOnPause");
            }
        }

        /// <summary>
        /// Initialize event listeners for closing the wheel menu
        /// </summary>
        private static void InitializeEventListeners()
        {
            // Subscribe to view change events
            Duckov.UI.View.OnActiveViewChanged += OnActiveViewChanged;
            
            // Subscribe to menu opened event to clear input state
            ThrowableWheelMenu.OnMenuOpened += OnWheelMenuOpened;
            
            ModLogger.Log("ThrowableWheelMenuPatch", "Subscribed to view change and menu opened events");
        }

        /// <summary>
        /// Called when wheel menu is opened - clears accumulated input state
        /// </summary>
        private static void OnWheelMenuOpened()
        {
            try
            {
                if (_inputControl == null)
                {
                    ModLogger.LogWarning("ThrowableWheelMenuPatch: Cannot clear input - InputControl is null");
                    return;
                }

                // Use reflection to clear mouse delta field in CharacterInputControl
                // This prevents accumulated mouse movement from affecting camera when menu opens
                var mouseDeltaField = AccessTools.Field(typeof(CharacterInputControl), "mouseDelta");
                if (mouseDeltaField != null)
                {
                    mouseDeltaField.SetValue(_inputControl, Vector2.zero);
                    ModLogger.Log("ThrowableWheelMenuPatch", "Cleared mouseDelta input state");
                }
                else
                {
                    ModLogger.LogWarning("ThrowableWheelMenuPatch: mouseDelta field not found");
                }

                // Clear mousePos field
                var mousePosField = AccessTools.Field(typeof(CharacterInputControl), "mousePos");
                if (mousePosField != null)
                {
                    // Convert Vector3 mousePosition to Vector2 for mousePos field
                    Vector2 currentMousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                    mousePosField.SetValue(_inputControl, currentMousePos);
                    ModLogger.Log("ThrowableWheelMenuPatch", "Reset mousePos to current cursor position");
                }
                else
                {
                    ModLogger.LogWarning("ThrowableWheelMenuPatch: mousePos field not found");
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.LogDetailedException(ex, "ThrowableWheelMenuPatch.OnWheelMenuOpened");
            }
        }

        /// <summary>
        /// Called when active view changes (inventory, map, etc.)
        /// </summary>
        private static void OnActiveViewChanged()
        {
            try
            {
                if (_wheelMenu != null && _wheelMenu.IsOpen && Duckov.UI.View.ActiveView != null)
                {
                    _wheelMenu.Cancel();
                    ModLogger.Log("ThrowableWheelMenuPatch", "Cancelled throwable wheel menu due to view opening");
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.LogDetailedException(ex, "ThrowableWheelMenuPatch.OnActiveViewChanged");
            }
        }
    }
}

