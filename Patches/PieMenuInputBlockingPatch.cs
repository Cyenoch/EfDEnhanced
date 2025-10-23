using System;
using HarmonyLib;
using EfDEnhanced.Utils;
using EfDEnhanced.Utils.UI.Components;

namespace EfDEnhanced.Patches
{
    /// <summary>
    /// Patch to block CharacterInputControl input when any PieMenuComponent is displayed
    /// This centralized patch replaces duplicate input blocking logic from individual wheel menu patches
    /// Uses PieMenuManager to check if any menu is currently active
    /// </summary>
    [HarmonyPatch]
    public class PieMenuInputBlockingPatch
    {
        /// <summary>
        /// Block mouse delta (camera rotation) when any pie menu is open
        /// </summary>
        [HarmonyPatch(typeof(CharacterInputControl), "OnPlayerMouseDelta")]
        [HarmonyPrefix]
        public static bool BlockMouseDeltaWhenMenuOpen()
        {
            try
            {
                if (PieMenuManager.ActiveMenu != null && PieMenuManager.ActiveMenu.IsOpen)
                {
                    // Block mouse delta to prevent camera rotation
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                ExceptionHelper.LogDetailedException(ex, "PieMenuInputBlockingPatch.BlockMouseDeltaWhenMenuOpen");
                return true;
            }
        }

        /// <summary>
        /// Block mouse movement affecting aim when any pie menu is open
        /// </summary>
        [HarmonyPatch(typeof(CharacterInputControl), "OnPlayerMouseMove")]
        [HarmonyPrefix]
        public static bool BlockMouseMoveWhenMenuOpen()
        {
            try
            {
                if (PieMenuManager.ActiveMenu != null && PieMenuManager.ActiveMenu.IsOpen)
                {
                    // Block mouse position updates to prevent aim changes
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                ExceptionHelper.LogDetailedException(ex, "PieMenuInputBlockingPatch.BlockMouseMoveWhenMenuOpen");
                return true;
            }
        }

        /// <summary>
        /// Block shooting/trigger input when any pie menu is open
        /// </summary>
        [HarmonyPatch(typeof(CharacterInputControl), "OnPlayerTriggerInputUsingMouseKeyboard")]
        [HarmonyPrefix]
        public static bool BlockTriggerWhenMenuOpen()
        {
            try
            {
                if (PieMenuManager.ActiveMenu != null && PieMenuManager.ActiveMenu.IsOpen)
                {
                    // Block trigger input to prevent shooting while using the menu
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                ExceptionHelper.LogDetailedException(ex, "PieMenuInputBlockingPatch.BlockTriggerWhenMenuOpen");
                return true;
            }
        }
    }
}
