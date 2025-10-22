using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Duckov.Economy;
using EfDEnhanced.Utils;
using HarmonyLib;
using UnityEngine;

namespace EfDEnhanced.Patches
{
    /// <summary>
    /// Debug patches for interaction system - logs detailed information about interactions
    /// 交互系统调试补丁 - 记录交互系统的详细信息
    /// </summary>
    public class InteractionDebugPatches
    {
        // Toggle this to enable/disable debug logging
        private static readonly bool EnableDebugLogging = false;

        #region InteractableBase Patches

        [HarmonyPatch(typeof(InteractableBase), "StartInteract")]
        public class InteractableBase_StartInteract_Patch
        {
            static void Prefix(InteractableBase __instance, CharacterMainControl _interactCharacter)
            {
                if (!EnableDebugLogging) return;

                try
                {
                    ModLogger.Log("InteractableBase", "========== Start Interaction ==========");
                    ModLogger.Log("InteractableBase", $"Type: {__instance.GetType().Name}");
                    ModLogger.Log("InteractableBase", $"GameObject: {__instance.gameObject.name}");
                    ModLogger.Log("InteractableBase", $"GameObject.Tag: {__instance.gameObject.tag}");
                    ModLogger.Log("InteractableBase", $"GameObject.Layer: {LayerMask.LayerToName(__instance.gameObject.layer)}");
                    ModLogger.Log("InteractableBase", $"InteractName: {__instance.InteractName}");
                    ModLogger.Log("InteractableBase", $"InteractTime: {__instance.InteractTime}");
                    ModLogger.Log("InteractableBase", $"RequireItem: {__instance.requireItem}");
                    ModLogger.Log("InteractableBase", $"CurrentScene: {__instance.gameObject.scene.name}");
                    ModLogger.Log("InteractableBase", $"Position: {__instance.transform.position}");

                    // Log all components on the GameObject
                    var components = __instance.gameObject.GetComponents<Component>();
                    ModLogger.Log("InteractableBase", $"Components count: {components.Length}");
                    foreach (var comp in components)
                    {
                        if (comp != null)
                        {
                            ModLogger.Log("InteractableBase", $"  - Component: {comp.GetType().Name}");
                        }
                    }

                    // Check parent objects for context
                    if (__instance.transform.parent != null)
                    {
                        ModLogger.Log("InteractableBase", $"Parent GameObject: {__instance.transform.parent.gameObject.name}");
                        if (__instance.transform.parent.parent != null)
                        {
                            ModLogger.Log("InteractableBase", $"GrandParent GameObject: {__instance.transform.parent.parent.gameObject.name}");
                        }
                    }

                    if (__instance.requireItem)
                    {
                        ModLogger.Log("InteractableBase", $"RequireItemId: {__instance.requireItemId}");
                        ModLogger.Log("InteractableBase", $"RequireItemName: {__instance.GetRequiredItemName()}");

                        var (hasItem, ItemInstance) = __instance.TryGetRequiredItem(_interactCharacter);
                        ModLogger.Log("InteractableBase", $"HasRequiredItem: {hasItem}");
                        if (ItemInstance != null)
                        {
                            ModLogger.Log("InteractableBase", $"ItemInstance: {ItemInstance.DisplayName}");
                            ModLogger.Log("InteractableBase", $"ItemStackCount: {ItemInstance.StackCount}");
                        }
                    }

                    // Check if this might be a scene transition
                    string goName = __instance.gameObject.name;
                    if (goName.Contains("GoTo") || goName.Contains("Interact") || goName.Contains("Enter") || goName.Contains("Exit"))
                    {
                        ModLogger.Log("InteractableBase", $"*** POTENTIAL SCENE TRANSITION: {goName} ***");
                    }
                }
                catch (Exception ex)
                {
                    ModLogger.LogError($"InteractableBase_StartInteract_Patch failed: {ex}");
                }
            }
        }

        [HarmonyPatch(typeof(InteractableBase), "FinishInteract")]
        public class InteractableBase_FinishInteract_Patch
        {
            static void Prefix(InteractableBase __instance, CharacterMainControl _interactCharacter)
            {
                if (!EnableDebugLogging) return;

                try
                {
                    ModLogger.Log("InteractableBase", $"========== Finish Interaction: {__instance.gameObject.name} ==========");
                }
                catch (Exception ex)
                {
                    ModLogger.LogError($"InteractableBase_FinishInteract_Patch failed: {ex}");
                }
            }
        }

        #endregion
    }
}

