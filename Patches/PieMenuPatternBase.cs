using System;
using HarmonyLib;
using UnityEngine;
using EfDEnhanced.Utils;
using EfDEnhanced.Utils.UI.Components;

namespace EfDEnhanced.Patches
{
    /// <summary>
    /// Base class for wheel menu patches that handles common lifecycle and event logic
    /// Derived patches only need to implement menu-specific hotkey and initialization logic
    /// Supports both direct PieMenuComponent usage and wrapper classes (ItemWheelMenu, ThrowableWheelMenu)
    /// </summary>
    public abstract class PieMenuPatternBase
    {
        protected static CharacterInputControl? _inputControl;

        /// <summary>
        /// Clear accumulated mouse input state when menu is opened
        /// Uses reflection to reset mouseDelta and mousePos fields
        /// </summary>
        protected static void ClearInputState()
        {
            try
            {
                if (_inputControl == null)
                {
                    ModLogger.LogWarning($"{nameof(PieMenuPatternBase)}: Cannot clear input - InputControl is null");
                    return;
                }

                // Clear mouseDelta field to prevent accumulated mouse movement from affecting camera
                var mouseDeltaField = AccessTools.Field(typeof(CharacterInputControl), "mouseDelta");
                if (mouseDeltaField != null)
                {
                    mouseDeltaField.SetValue(_inputControl, Vector2.zero);
                    ModLogger.Log(nameof(PieMenuPatternBase), "Cleared mouseDelta input state");
                }
                else
                {
                    ModLogger.LogWarning($"{nameof(PieMenuPatternBase)}: mouseDelta field not found");
                }

                // Reset mousePos field to current cursor position
                var mousePosField = AccessTools.Field(typeof(CharacterInputControl), "mousePos");
                if (mousePosField != null)
                {
                    Vector2 currentMousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                    mousePosField.SetValue(_inputControl, currentMousePos);
                    ModLogger.Log(nameof(PieMenuPatternBase), "Reset mousePos to current cursor position");
                }
                else
                {
                    ModLogger.LogWarning($"{nameof(PieMenuPatternBase)}: mousePos field not found");
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.LogDetailedException(ex, $"{nameof(PieMenuPatternBase)}.ClearInputState");
            }
        }

        /// <summary>
        /// Cancel the menu when active view changes (inventory, map, etc.)
        /// Uses reflection to access IsOpen and Cancel methods
        /// </summary>
        protected static void HandleActiveViewChanged(object? menu)
        {
            try
            {
                if (menu == null || Duckov.UI.View.ActiveView == null)
                {
                    return;
                }

                // Use reflection to check IsOpen and call Cancel
                var isOpenProperty = AccessTools.Property(menu.GetType(), "IsOpen");
                if (isOpenProperty != null && (bool)isOpenProperty.GetValue(menu)!)
                {
                    var cancelMethod = AccessTools.Method(menu.GetType(), "Cancel");
                    if (cancelMethod != null)
                    {
                        cancelMethod.Invoke(menu, null);
                        ModLogger.Log(nameof(PieMenuPatternBase), "Cancelled menu due to view opening");
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.LogDetailedException(ex, $"{nameof(PieMenuPatternBase)}.HandleActiveViewChanged");
            }
        }

        /// <summary>
        /// Cancel the menu when game is paused
        /// Uses reflection to access IsOpen and Cancel methods
        /// </summary>
        protected static void HandlePauseMenuShow(object? menu)
        {
            try
            {
                if (menu == null)
                {
                    return;
                }

                // Use reflection to check IsOpen and call Cancel
                var isOpenProperty = AccessTools.Property(menu.GetType(), "IsOpen");
                if (isOpenProperty != null && (bool)isOpenProperty.GetValue(menu)!)
                {
                    var cancelMethod = AccessTools.Method(menu.GetType(), "Cancel");
                    if (cancelMethod != null)
                    {
                        cancelMethod.Invoke(menu, null);
                        ModLogger.Log(nameof(PieMenuPatternBase), "Cancelled menu due to pause");
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.LogDetailedException(ex, $"{nameof(PieMenuPatternBase)}.HandlePauseMenuShow");
            }
        }

        /// <summary>
        /// Cancel the menu if game state prevents interaction
        /// Uses reflection to access IsOpen and Cancel methods
        /// </summary>
        protected static void CancelIfGameStateBlocks(object? menu)
        {
            try
            {
                if (menu == null || (!GameManager.Paused && Duckov.UI.View.ActiveView == null))
                {
                    return;
                }

                // Use reflection to check IsOpen and call Cancel
                var isOpenProperty = AccessTools.Property(menu.GetType(), "IsOpen");
                if (isOpenProperty != null && (bool)isOpenProperty.GetValue(menu)!)
                {
                    var cancelMethod = AccessTools.Method(menu.GetType(), "Cancel");
                    if (cancelMethod != null)
                    {
                        cancelMethod.Invoke(menu, null);
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.LogDetailedException(ex, $"{nameof(PieMenuPatternBase)}.CancelIfGameStateBlocks");
            }
        }
    }
}
