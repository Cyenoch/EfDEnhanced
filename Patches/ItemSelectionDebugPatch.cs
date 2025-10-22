using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Duckov.UI;
using Duckov.Utilities;
using EfDEnhanced.Utils;
using HarmonyLib;
using ItemStatsSystem;

namespace EfDEnhanced.Patches
{
    /// <summary>
    /// Debug patch to log detailed item information when an item is selected
    /// 用于在选中物品时输出详细信息的调试补丁
    /// </summary>
    [HarmonyPatch]
    public class ItemSelectionDebugPatch
    {
        private static Item? _lastLoggedItem = null;

        /// <summary>
        /// Patch ItemUIUtilities.Select to log item details when selection changes
        /// </summary>
        [HarmonyPatch(typeof(ItemUIUtilities), "Select")]
        [HarmonyPostfix]
        static void ItemUIUtilities_Select_Postfix(ItemDisplay itemDisplay)
        {
            try
            {
                // Get the selected item
                Item selectedItem = ItemUIUtilities.SelectedItem;

                // Only log if selection changed (avoid duplicate logs)
                if (selectedItem == _lastLoggedItem)
                {
                    return;
                }

                _lastLoggedItem = selectedItem;

                // If nothing selected, log that
                if (selectedItem == null)
                {
                    ModLogger.Log("ItemDebug", "No item selected");
                    return;
                }

                // Build detailed information string
                var info = new StringBuilder();
                info.AppendLine("\n========== ITEM DETAILS ==========");
                
                // Basic Information
                info.AppendLine($"Display Name: {selectedItem.DisplayName}");
                info.AppendLine($"Display Name (Raw Key): {selectedItem.DisplayNameRaw}");
                info.AppendLine($"Description: {selectedItem.Description}");
                info.AppendLine($"Description (Raw Key): {selectedItem.DescriptionRaw}");
                info.AppendLine($"Type ID: {selectedItem.TypeID}");
                info.AppendLine($"Order: {selectedItem.Order}");
                info.AppendLine($"Instance ID: {selectedItem.GetInstanceID()}");
                info.AppendLine($"Sound Key: {selectedItem.SoundKey}");

                // Value and Quality
                info.AppendLine($"\n--- Value & Quality ---");
                info.AppendLine($"Value: {selectedItem.Value}");
                info.AppendLine($"Quality: {selectedItem.Quality}");
                info.AppendLine($"Display Quality: {selectedItem.DisplayQuality}");
                info.AppendLine($"Total Raw Value: {selectedItem.GetTotalRawValue()}");

                // Weight
                info.AppendLine($"\n--- Weight ---");
                info.AppendLine($"Unit Self Weight: {selectedItem.UnitSelfWeight}");
                info.AppendLine($"Self Weight: {selectedItem.SelfWeight}");
                info.AppendLine($"Total Weight: {selectedItem.TotalWeight}");

                // Stack Information
                info.AppendLine($"\n--- Stack Info ---");
                info.AppendLine($"Stackable: {selectedItem.Stackable}");
                info.AppendLine($"Max Stack Count: {selectedItem.MaxStackCount}");
                info.AppendLine($"Current Stack Count: {selectedItem.StackCount}");

                // Durability
                if (selectedItem.UseDurability)
                {
                    info.AppendLine($"\n--- Durability ---");
                    info.AppendLine($"Current Durability: {selectedItem.Durability:F2}");
                    info.AppendLine($"Max Durability: {selectedItem.MaxDurability:F2}");
                    info.AppendLine($"Durability Loss: {selectedItem.DurabilityLoss:F2}");
                    info.AppendLine($"Max Durability With Loss: {selectedItem.MaxDurabilityWithLoss:F2}");
                    info.AppendLine($"Repairable: {selectedItem.Repairable}");
                }

                // Flags
                info.AppendLine($"\n--- Flags ---");
                info.AppendLine($"Can Be Sold: {selectedItem.CanBeSold}");
                info.AppendLine($"Can Drop: {selectedItem.CanDrop}");
                info.AppendLine($"Sticky: {selectedItem.Sticky}");
                info.AppendLine($"Is Character: {selectedItem.IsCharacter}");
                info.AppendLine($"Has Hand Held Agent: {selectedItem.HasHandHeldAgent}");
                info.AppendLine($"Is Usable: {selectedItem.IsUsable(null)}");
                info.AppendLine($"Use Time: {selectedItem.UseTime}");
                info.AppendLine($"Inspected: {selectedItem.Inspected}");
                info.AppendLine($"Inspecting: {selectedItem.Inspecting}");
                info.AppendLine($"Need Inspection: {selectedItem.NeedInspection}");

                // Tags
                if (selectedItem.Tags != null && selectedItem.Tags.Count > 0)
                {
                    info.AppendLine($"\n--- Tags ({selectedItem.Tags.Count}) ---");
                    foreach (var tag in selectedItem.Tags)
                    {
                        if (tag != null)
                        {
                            info.AppendLine($"  - {tag.name}");
                            if (tag.DisplayName != tag.name)
                            {
                                info.AppendLine($"    Display: {tag.DisplayName}");
                            }
                        }
                    }
                }

                // Stats
                if (selectedItem.Stats != null && selectedItem.Stats.Count > 0)
                {
                    info.AppendLine($"\n--- Stats ({selectedItem.Stats.Count}) ---");
                    foreach (var stat in selectedItem.Stats)
                    {
                        if (stat != null)
                        {
                            string displayStatus = stat.Display ? "[Displayed]" : "[Hidden]";
                            info.AppendLine($"  {displayStatus} {stat.Key}: {stat.Value:F2}");
                            if (stat.DisplayName != stat.Key)
                            {
                                info.AppendLine($"    Display Name: {stat.DisplayName}");
                            }
                        }
                    }
                }

                // Slots
                if (selectedItem.Slots != null && selectedItem.Slots.Count > 0)
                {
                    info.AppendLine($"\n--- Slots ({selectedItem.Slots.Count}) ---");
                    foreach (var slot in selectedItem.Slots)
                    {
                        if (slot != null)
                        {
                            string content = slot.Content != null ? $"{slot.Content.DisplayName} (TypeID: {slot.Content.TypeID})" : "<empty>";
                            info.AppendLine($"  - {slot.Key}: {content}");
                            if (slot.DisplayName != slot.Key)
                            {
                                info.AppendLine($"    Display Name: {slot.DisplayName}");
                            }
                        }
                    }
                }

                // Modifiers
                if (selectedItem.Modifiers != null && selectedItem.Modifiers.Count > 0)
                {
                    info.AppendLine($"\n--- Modifiers ({selectedItem.Modifiers.Count}) ---");
                    foreach (var modifier in selectedItem.Modifiers)
                    {
                        if (modifier != null)
                        {
                            info.AppendLine($"  - Key: {modifier.Key}");
                            info.AppendLine($"    Type: {modifier.Type}, Value: {modifier.Value:F2} ({modifier.GetDisplayValueString()})");
                            info.AppendLine($"    Order: {modifier.Order}, Override: {modifier.IsOverrideOrder}");
                            if (modifier.DisplayName != modifier.Key)
                            {
                                info.AppendLine($"    Display Name: {modifier.DisplayName}");
                            }
                        }
                    }
                }

                // Variables (Custom Data)
                if (selectedItem.Variables != null && selectedItem.Variables.Count > 0)
                {
                    info.AppendLine($"\n--- Variables ({selectedItem.Variables.Count}) ---");
                    foreach (var variable in selectedItem.Variables)
                    {
                        if (variable != null)
                        {
                            string rawValue = "";
                            switch (variable.DataType)
                            {
                                case Duckov.Utilities.CustomDataType.Float:
                                    rawValue = variable.GetFloat().ToString("F6");
                                    break;
                                case Duckov.Utilities.CustomDataType.Int:
                                    rawValue = variable.GetInt().ToString();
                                    break;
                                case Duckov.Utilities.CustomDataType.Bool:
                                    rawValue = variable.GetBool().ToString();
                                    break;
                                case Duckov.Utilities.CustomDataType.String:
                                    rawValue = $"\"{variable.GetString()}\"";
                                    break;
                                default:
                                    rawValue = variable.GetValueDisplayString();
                                    break;
                            }
                            info.AppendLine($"  - {variable.Key} ({variable.DataType}): {rawValue}");
                            if (variable.Display && variable.DisplayName != variable.Key)
                            {
                                info.AppendLine($"    Display: {variable.GetValueDisplayString()} ({variable.DisplayName})");
                            }
                        }
                    }
                }

                // Constants
                if (selectedItem.Constants != null && selectedItem.Constants.Count > 0)
                {
                    info.AppendLine($"\n--- Constants ({selectedItem.Constants.Count}) ---");
                    foreach (var constant in selectedItem.Constants)
                    {
                        if (constant != null)
                        {
                            string rawValue = "";
                            switch (constant.DataType)
                            {
                                case Duckov.Utilities.CustomDataType.Float:
                                    rawValue = constant.GetFloat().ToString("F6");
                                    break;
                                case Duckov.Utilities.CustomDataType.Int:
                                    rawValue = constant.GetInt().ToString();
                                    break;
                                case Duckov.Utilities.CustomDataType.Bool:
                                    rawValue = constant.GetBool().ToString();
                                    break;
                                case Duckov.Utilities.CustomDataType.String:
                                    rawValue = $"\"{constant.GetString()}\"";
                                    break;
                                default:
                                    rawValue = constant.GetValueDisplayString();
                                    break;
                            }
                            info.AppendLine($"  - {constant.Key} ({constant.DataType}): {rawValue}");
                            if (constant.Display && constant.DisplayName != constant.Key)
                            {
                                info.AppendLine($"    Display: {constant.GetValueDisplayString()} ({constant.DisplayName})");
                            }
                        }
                    }
                }

                // Effects
                if (selectedItem.Effects != null && selectedItem.Effects.Count > 0)
                {
                    info.AppendLine($"\n--- Effects ({selectedItem.Effects.Count}) ---");
                    foreach (var effect in selectedItem.Effects)
                    {
                        if (effect != null)
                        {
                            info.AppendLine($"  - {effect.name} (Active: {effect.gameObject.activeSelf})");
                        }
                    }
                }

                // Inventory
                if (selectedItem.Inventory != null)
                {
                    info.AppendLine($"\n--- Inventory ---");
                    info.AppendLine($"Capacity: {selectedItem.Inventory.Capacity}");
                    int itemCount = 0;
                    foreach (var invItem in selectedItem.Inventory)
                    {
                        if (invItem != null) itemCount++;
                    }
                    info.AppendLine($"Item Count: {itemCount}");
                    info.AppendLine($"Total Weight: {selectedItem.Inventory.CachedWeight:F2}");
                }

                // Parent Information
                info.AppendLine($"\n--- Hierarchy ---");
                if (selectedItem.ParentItem != null)
                {
                    info.AppendLine($"Parent Item: {selectedItem.ParentItem.DisplayName}");
                }
                else if (selectedItem.InInventory != null)
                {
                    info.AppendLine($"In Inventory: {selectedItem.InInventory.name}");
                }
                else
                {
                    info.AppendLine($"Parent: None (root item)");
                }

                if (selectedItem.PluggedIntoSlot != null)
                {
                    info.AppendLine($"Plugged Into Slot: {selectedItem.PluggedIntoSlot.Key}");
                }

                info.AppendLine("==================================\n");

                // Log the information
                ModLogger.Log("ItemDebug", info.ToString());
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"ItemSelectionDebugPatch error: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}

