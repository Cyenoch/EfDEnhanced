using System;
using HarmonyLib;
using UnityEngine;
using ItemStatsSystem;
using EfDEnhanced.Features;
using EfDEnhanced.Utils;
using Duckov;

namespace EfDEnhanced.Patches
{
    /// <summary>
    /// Patch to initialize container wheel menu
    /// Container wheel menu is displayed when a container item is invoked from ItemWheelMenu
    /// </summary>
    [HarmonyPatch]
    public class ContainerWheelMenuPatch
    {
        private static ContainerWheelMenu? _containerWheelMenu;
        private static bool _containerWheelMenuInitialized = false;

        /// <summary>
        /// Patch CharacterInputControl.Update to initialize container wheel menu if needed
        /// </summary>
        [HarmonyPatch(typeof(CharacterInputControl), "Update")]
        [HarmonyPostfix]
        public static void CharacterInputControl_Update_Postfix(CharacterInputControl __instance)
        {
            try
            {
                // Initialize container wheel menu if needed
                if (!_containerWheelMenuInitialized)
                {
                    InitializeContainerWheelMenu();
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.LogDetailedException(ex, "ContainerWheelMenuPatch.CharacterInputControl_Update");
            }
        }

        /// <summary>
        /// Patch ShortCutInput to handle container items by showing ContainerWheelMenu
        /// If the shortcut item is a container, open the ContainerWheelMenu instead of using it normally
        /// </summary>
        [HarmonyPatch(typeof(CharacterInputControl), "ShortCutInput")]
        [HarmonyPrefix]
        public static bool ShortCutInput_Prefix(int index)
        {
            try
            {
                Item item = ItemShortcut.Get(index - 3);
                
                // Check if item is a container
                if (item != null && ItemUsageHelper.IsContainer(item))
                {
                    // Initialize container menu if needed
                    if (_containerWheelMenu == null && _containerWheelMenuInitialized)
                    {
                        _containerWheelMenu = ContainerWheelMenu.Instance;
                    }

                    if (_containerWheelMenu != null)
                    {
                        _containerWheelMenu.Show(item);
                        ModLogger.Log("ContainerWheelMenuPatch", $"Showing container wheel menu for: {item.DisplayName}");
                        return false; // Skip the original method
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.LogDetailedException(ex, "ContainerWheelMenuPatch.ShortCutInput_Prefix");
            }

            return true; // Continue with original method
        }

        /// <summary>
        /// Initialize the container wheel menu GameObject
        /// </summary>
        private static void InitializeContainerWheelMenu()
        {
            try
            {
                if (_containerWheelMenuInitialized)
                {
                    return;
                }

                GameObject menuObj = new("EfDEnhanced_ContainerWheelMenu");
                _containerWheelMenu = menuObj.AddComponent<ContainerWheelMenu>();

                _containerWheelMenuInitialized = true;
                ModLogger.Log("ContainerWheelMenuPatch", "Container wheel menu initialized successfully");
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"ContainerWheelMenuPatch: Failed to initialize container wheel menu: {ex}");
            }
        }
    }
}
