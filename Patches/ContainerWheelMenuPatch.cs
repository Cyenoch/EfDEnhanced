using System;
using HarmonyLib;
using UnityEngine;
using EfDEnhanced.Features;
using EfDEnhanced.Utils;

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
