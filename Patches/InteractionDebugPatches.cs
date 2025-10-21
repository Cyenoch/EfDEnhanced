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
        private static readonly bool EnableDebugLogging = true;

        #region MultiInteraction Patches

        [HarmonyPatch(typeof(MultiInteraction), "OnTriggerEnter")]
        public class MultiInteraction_OnTriggerEnter_Patch
        {
            static void Prefix(MultiInteraction __instance, Collider other)
            {
                if (!EnableDebugLogging) return;

                try
                {
                    ModLogger.Log("MultiInteraction", "========== Player Entered MultiInteraction Area ==========");
                    ModLogger.Log("MultiInteraction", $"GameObject: {__instance.gameObject.name}");
                    ModLogger.Log("MultiInteraction", $"Position: {__instance.transform.position}");
                    ModLogger.Log("MultiInteraction", $"Scene: {__instance.gameObject.scene.name}");

                    var interactables = __instance.Interactables;
                    ModLogger.Log("MultiInteraction", $"Total Interactables: {interactables.Count}");

                    for (int i = 0; i < interactables.Count; i++)
                    {
                        var interactable = interactables[i];
                        if (interactable == null) continue;

                        ModLogger.Log("MultiInteraction", $"  [{i}] Type: {interactable.GetType().Name}");
                        ModLogger.Log("MultiInteraction", $"      GameObject: {interactable.gameObject.name}");
                        ModLogger.Log("MultiInteraction", $"      InteractName: {interactable.InteractName}");
                        ModLogger.Log("MultiInteraction", $"      Enabled: {interactable.enabled}");
                        ModLogger.Log("MultiInteraction", $"      Interactable: {interactable.CheckInteractable()}");

                        // If it's a CostTaker, log cost details
                        if (interactable is CostTaker costTaker)
                        {
                            LogCostTakerDetails(costTaker, i);
                        }
                    }

                    ModLogger.Log("MultiInteraction", "==========================================================");
                }
                catch (Exception ex)
                {
                    ModLogger.LogError($"MultiInteraction_OnTriggerEnter_Patch failed: {ex}");
                }
            }
        }

        #endregion

        #region CostTaker Patches

        [HarmonyPatch(typeof(CostTaker), "OnInteractFinished")]
        public class CostTaker_OnInteractFinished_Patch
        {
            static void Prefix(CostTaker __instance)
            {
                if (!EnableDebugLogging) return;

                try
                {
                    ModLogger.Log("CostTaker", "========== CostTaker Interaction Finished ==========");
                    ModLogger.Log("CostTaker", $"GameObject: {__instance.gameObject.name}");
                    ModLogger.Log("CostTaker", $"InteractName: {__instance.InteractName}");

                    LogCostTakerDetails(__instance, -1);

                    ModLogger.Log("CostTaker", "=====================================================");
                }
                catch (Exception ex)
                {
                    ModLogger.LogError($"CostTaker_OnInteractFinished_Patch failed: {ex}");
                }
            }
        }

        [HarmonyPatch(typeof(CostTaker), "IsInteractable")]
        public class CostTaker_IsInteractable_Patch
        {
            static void Postfix(CostTaker __instance, bool __result)
            {
                if (!EnableDebugLogging) return;

                try
                {
                    if (!__result)
                    {
                        ModLogger.Log("CostTaker", $"[{__instance.gameObject.name}] Not interactable - Cost not enough");
                        LogCostTakerDetails(__instance, -1);
                    }
                }
                catch (Exception ex)
                {
                    ModLogger.LogError($"CostTaker_IsInteractable_Patch failed: {ex}");
                }
            }
        }

        #endregion

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

        #region Cost System Patches

        [HarmonyPatch(typeof(Cost), "Pay")]
        public class Cost_Pay_Patch
        {
            static void Prefix(Cost __instance, bool accountAvaliable, bool cashAvaliable)
            {
                if (!EnableDebugLogging) return;

                try
                {
                    ModLogger.Log("Cost", "========== Paying Cost ==========");
                    ModLogger.Log("Cost", $"Money: {__instance.money}");
                    ModLogger.Log("Cost", $"Enough: {__instance.Enough}");
                    ModLogger.Log("Cost", $"IsFree: {__instance.IsFree}");

                    if (__instance.items != null && __instance.items.Length > 0)
                    {
                        ModLogger.Log("Cost", $"Items Count: {__instance.items.Length}");
                        foreach (var item in __instance.items)
                        {
                            var meta = ItemStatsSystem.ItemAssetsCollection.GetMetaData(item.id);
                            ModLogger.Log("Cost", $"  - {meta.DisplayName} (ID: {item.id}) x{item.amount}");
                        }
                    }

                    ModLogger.Log("Cost", "=================================");
                }
                catch (Exception ex)
                {
                    ModLogger.LogError($"Cost_Pay_Patch failed: {ex}");
                }
            }

            static void Postfix(bool __result)
            {
                if (!EnableDebugLogging) return;

                try
                {
                    ModLogger.Log("Cost", $"Payment Result: {(__result ? "SUCCESS" : "FAILED")}");
                }
                catch (Exception ex)
                {
                    ModLogger.LogError($"Cost_Pay_Patch Postfix failed: {ex}");
                }
            }
        }

        #endregion

        #region Helper Methods

        private static void LogCostTakerDetails(CostTaker costTaker, int index)
        {
            try
            {
                var cost = costTaker.Cost;
                string prefix = index >= 0 ? $"      " : "";

                ModLogger.Log("CostTaker", $"{prefix}Cost Details:");
                ModLogger.Log("CostTaker", $"{prefix}  Money: {cost.money}");
                ModLogger.Log("CostTaker", $"{prefix}  IsFree: {cost.IsFree}");
                ModLogger.Log("CostTaker", $"{prefix}  Enough: {cost.Enough}");

                if (cost.items != null && cost.items.Length > 0)
                {
                    ModLogger.Log("CostTaker", $"{prefix}  Required Items: {cost.items.Length}");
                    foreach (var item in cost.items)
                    {
                        try
                        {
                            var meta = ItemStatsSystem.ItemAssetsCollection.GetMetaData(item.id);
                            ModLogger.Log("CostTaker", $"{prefix}    - {meta.DisplayName} (ID: {item.id}) x{item.amount}");
                        }
                        catch
                        {
                            ModLogger.Log("CostTaker", $"{prefix}    - Unknown Item (ID: {item.id}) x{item.amount}");
                        }
                    }
                }
                else
                {
                    ModLogger.Log("CostTaker", $"{prefix}  No items required");
                }
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"LogCostTakerDetails failed: {ex}");
            }
        }

        #endregion

        #region UnityEvent Monitoring

        // Monitor UnityEvents from CostTaker
        [HarmonyPatch(typeof(CostTaker), "OnEnable")]
        public class CostTaker_OnEnable_Patch
        {
            static void Postfix(CostTaker __instance)
            {
                if (!EnableDebugLogging) return;

                try
                {
                    ModLogger.Log("CostTaker", $"CostTaker Enabled: {__instance.gameObject.name}");

                    // Monitor onPayed event
                    __instance.onPayed += (ct) =>
                    {
                        ModLogger.Log("CostTaker", $"========== onPayed Event Triggered ==========");
                        ModLogger.Log("CostTaker", $"GameObject: {ct.gameObject.name}");
                        ModLogger.Log("CostTaker", "Action completed - check for scene changes or teleportation");
                        ModLogger.Log("CostTaker", "============================================");
                    };
                }
                catch (Exception ex)
                {
                    ModLogger.LogError($"CostTaker_OnEnable_Patch failed: {ex}");
                }
            }
        }

        #endregion

        // Note: Scene loading detection has been removed due to incorrect method signature
        // If you need to monitor scene loading, use Unity's SceneManager.sceneLoaded event
        // in your ModBehaviour.Awake() method instead.
    }
}

