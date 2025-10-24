using System;
using System.Collections.Generic;
using System.Linq;
using Duckov.UI;
using Duckov.Utilities;
using HarmonyLib;
using ItemStatsSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using EfDEnhanced.Utils;
using System.Collections;

namespace EfDEnhanced.Patches
{
    /// <summary>
    /// Patch for ItemHoveringUI to show weapon comparison with selected weapon
    /// Supports both ranged weapons (IsGun) and melee weapons (IsMelee)
    /// </summary>
    [HarmonyPatch]
    public class ItemHoveringComparisonPatch
    {
        // Store comparison data for each LabelAndValue entry
        private static readonly Dictionary<LabelAndValue, ComparisonData> comparisonDataMap = [];

        // Immutable comparison data class
        private sealed class ComparisonData(TextMeshProUGUI? valueText, string? originalText)
        {
            public TextMeshProUGUI? ValueText { get; } = valueText;
            public string? OriginalText { get; } = originalText;
        }

        /// <summary>
        /// Patch ItemPropertiesDisplay.Setup to add weapon comparison values
        /// </summary>
        [HarmonyPatch(typeof(ItemPropertiesDisplay), "Setup")]
        [HarmonyPostfix]
        static void ItemPropertiesDisplay_Setup_Postfix(ItemPropertiesDisplay __instance, Item targetItem)
        {
            try
            {
                // Always clean up previous comparison data first
                CleanupComparisonData();

                // Check if feature is enabled
                if (!ModSettings.EnableWeaponComparison.Value)
                {
                    return;
                }

                if (targetItem == null)
                {
                    ModLogger.Log("WeaponComparison", "Target item is null, skipping comparison");
                    return;
                }

                // Check if target item is a weapon
                if (!IsWeapon(targetItem))
                {
                    return;
                }

                Item selectedItem = ItemUIUtilities.SelectedItem;

                // Only show comparison if a weapon is selected and it's different from hover item
                if (selectedItem == null || selectedItem == targetItem || !IsWeapon(selectedItem))
                {
                    return;
                }

                ModLogger.Log("WeaponComparison",
                    $"Comparing weapons:\n" +
                    $"  Selected: {selectedItem.DisplayName} (ID: {selectedItem.TypeID})\n" +
                    $"  Hovering: {targetItem.DisplayName} (ID: {targetItem.TypeID})");

                // Extract properties from both weapons
                var selectedProps = ExtractWeaponStats(selectedItem);
                var hoverProps = ExtractWeaponStats(targetItem);

                if (selectedProps.Count == 0 || hoverProps.Count == 0)
                {
                    ModLogger.Log("WeaponComparison", "One or both weapons have no comparable stats");
                    return;
                }

                // Find all LabelAndValue components in the display
                var entries = __instance.GetComponentsInChildren<LabelAndValue>(includeInactive: false);

                if (entries.Length == 0)
                {
                    ModLogger.LogWarning("No property entries found to compare");
                    return;
                }

                foreach (var entry in entries)
                {
                    // Find label text by searching for child named "Label" or first TextMeshProUGUI
                    TextMeshProUGUI? labelText = null;
                    Transform? labelTransform = entry.transform.Find("Label");
                    if (labelTransform != null)
                    {
                        labelText = labelTransform.GetComponent<TextMeshProUGUI>();
                    }

                    // Fallback: use reflection if direct search fails
                    if (labelText == null)
                    {
                        var labelTextField = typeof(LabelAndValue).GetField("labelText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        labelText = labelTextField?.GetValue(entry) as TextMeshProUGUI;
                        if (labelText == null)
                        {
                            ModLogger.Log("WeaponComparison", $"Could not find labelText for entry {entry.name} with neither child 'Label' nor field reflection.");
                        }
                        else
                        {
                            ModLogger.Log("WeaponComparison", $"labelText resolved by reflection for entry {entry.name}");
                        }
                    }

                    if (labelText == null)
                    {
                        ModLogger.Log("WeaponComparison", $"Skipping entry {entry.name}: labelText is still null.");
                        continue;
                    }

                    string? key = labelText.text;
                    if (string.IsNullOrEmpty(key))
                    {
                        ModLogger.Log("WeaponComparison", $"Skipping entry {entry.name}: labelText.text is null or empty.");
                        continue;
                    }

                    var selectedProp = selectedProps.FirstOrDefault(p => p.DisplayName == key);
                    if (selectedProp == null)
                    {
                        ModLogger.Log("WeaponComparison", $"Property [{key}] not found in selected weapon.");
                        continue;
                    }

                    var hoverProp = hoverProps.FirstOrDefault(p => p.DisplayName == key);
                    if (hoverProp == null)
                    {
                        ModLogger.Log("WeaponComparison", $"Property [{key}] not found in hover weapon.");
                        continue;
                    }

                    AddComparisonToEntry(entry, selectedProp.Value, hoverProp.Value, selectedProp.Key, selectedProp.Polarity, selectedProp.RawValue, hoverProp.RawValue);
                }
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Error in ItemPropertiesDisplay_Setup_Postfix: {ex}");
            }
        }

        /// <summary>
        /// Check if item is a weapon using official game flags
        /// Supports both ranged weapons (IsGun) and melee weapons (IsMelee)
        /// </summary>
        private static bool IsWeapon(Item item)
        {
            try
            {
                if (item.Tags.Any(tag => tag.name == "Weapon" || tag.name == "MeleeWeapon" || tag.name == "Gun"))
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Error checking if item is weapon: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Extract weapon stats into a list of comparable data
        /// </summary>
        private static List<StatData> ExtractWeaponStats(Item item)
        {
            var stats = new List<StatData>();
            try
            {
                if (item.Stats != null)
                {
                    // Extract displayed stats
                    foreach (Stat stat in item.Stats)
                    {
                        if (stat.Display)
                        {
                            stats.Add(new StatData(
                              stat.Key,
                              stat.DisplayName,
                              stat.Value.ToString(),
                              StatPolarityMap.GetPolarity(stat.Key),
                              stat.Value
                            ));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Error extracting weapon stats: {ex}");
            }

            return stats;
        }

        // Immutable stat data class
        private sealed class StatData(string key, string displayName, string value, Polarity polarity, object? rawValue)
        {
            public string Key { get; } = key;
            public string DisplayName { get; } = displayName;
            public string Value { get; } = value;
            public Polarity Polarity { get; } = polarity;
            public object? RawValue { get; } = rawValue;
        }

        /// <summary>
        /// Add comparison display to a LabelAndValue entry with color coding
        /// Modifies the original Value text to show: [Selected] → [Hover] with rich text colors
        /// </summary>
        private static void AddComparisonToEntry(LabelAndValue entry, string selectedValue, string hoverValue,
            string propertyName, Polarity polarity, object? selectedRaw, object? hoverRaw)
        {
            try
            {
                // Get or create comparison data for this entry
                if (!comparisonDataMap.TryGetValue(entry, out var compData))
                {
                    compData = new ComparisonData(null, null);
                    comparisonDataMap[entry] = compData;
                }

                // Entry structure: [Label] [Value]
                Transform? valueTransform = null;

                // Try to find Value child by name
                for (int i = 0; i < entry.transform.childCount; i++)
                {
                    var child = entry.transform.GetChild(i);
                    if (child.name == "Value")
                    {
                        valueTransform = child;
                        break;
                    }
                }

                if (valueTransform == null)
                {
                    ModLogger.LogWarning($"Could not find Value transform for {propertyName}");
                    return;
                }

                TextMeshProUGUI? valueText = valueTransform.GetComponent<TextMeshProUGUI>();
                if (valueText == null)
                {
                    ModLogger.LogWarning($"Value transform has no TextMeshProUGUI component for {propertyName}");
                    return;
                }

                // Store original text and reference on first use
                if (compData.ValueText == null)
                {
                    compData = new ComparisonData(valueText, valueText.text);
                    comparisonDataMap[entry] = compData; // Update dictionary with new instance
                }

                // Build comparison text with colors using TextMeshPro rich text tags
                string comparisonText = BuildComparisonText(selectedValue, hoverValue, propertyName, polarity, selectedRaw, hoverRaw);

                // Update the text
                valueText.text = comparisonText;
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Error adding comparison to entry: {ex}");
            }
        }

        /// <summary>
        /// Build comparison text with color coding based on polarity and numeric difference
        /// Returns formatted text: [Selected] → [Hover] with rich text color tags
        /// </summary>
        private static string BuildComparisonText(string selectedValue, string hoverValue,
            string propertyName, Polarity polarity, object? selectedRaw, object? hoverRaw)
        {
            try
            {
                // Define colors as constants (can be moved to UIConstants later)
                const string betterColor = "#66FF66";      // Green for better
                const string worseColor = "#FF6666";       // Red for worse
                const string neutralColor = "#FFFFFF";     // White for neutral
                const string arrowColor = "#CCCCCC";       // Light gray for arrow

                // Use switch expression (C# 8.0+) for cleaner polarity handling
                return polarity switch
                {
                    Polarity.Neutral => FormatComparison(selectedValue, hoverValue, neutralColor, neutralColor, arrowColor),
                    _ => BuildColoredComparison(selectedValue, hoverValue, propertyName, polarity, selectedRaw, hoverRaw,
                                                 betterColor, worseColor, neutralColor, arrowColor)
                };
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Error building comparison text: {ex}");
                return $"{selectedValue} → {hoverValue}"; // Fallback
            }
        }

        // Helper method to format comparison text
        private static string FormatComparison(string selectedValue, string hoverValue,
                                              string selectedColor, string hoverColor, string arrowColor)
        {
            return $"<color={selectedColor}>{selectedValue}</color> <color={arrowColor}>→</color> <color={hoverColor}>{hoverValue}</color>";
        }

        // Build colored comparison based on numeric values
        private static string BuildColoredComparison(string selectedValue, string hoverValue, string propertyName,
                                                     Polarity polarity, object? selectedRaw, object? hoverRaw,
                                                     string betterColor, string worseColor, string neutralColor, string arrowColor)
        {
            // Try to convert display values to numeric for comparison
            if (!TryGetNumericValue(selectedRaw, out float selectedNum) ||
                !TryGetNumericValue(hoverRaw, out float hoverNum))
            {
                // Non-numeric values, use neutral colors
                return FormatComparison(selectedValue, hoverValue, neutralColor, neutralColor, arrowColor);
            }

            // Determine if hover value is better using switch expression
            bool hoverIsBetter = polarity switch
            {
                Polarity.Positive => hoverNum > selectedNum,  // Higher is better
                Polarity.Negative => hoverNum < selectedNum,  // Lower is better
                _ => false
            };

            // Determine colors for each value using pattern matching
            var (selectedColor, hoverColor) = Math.Abs(hoverNum - selectedNum) < 0.001f
              ? (neutralColor, neutralColor)  // Values are equal
              : hoverIsBetter
                ? (worseColor, betterColor)   // Hover is better
                : (betterColor, worseColor);  // Selected is better

            // Detailed logging for color application
            ModLogger.Log("WeaponComparison",
                $"  {propertyName} (): {selectedValue} → {hoverValue} | " +
                $"Polarity: {polarity} | Result: {(hoverIsBetter ? "Better" : "Worse")} ({(hoverIsBetter ? "Green" : "Red")})");

            return FormatComparison(selectedValue, hoverValue, selectedColor, hoverColor, arrowColor);
        }

        /// <summary>
        /// Try to get numeric value from raw object
        /// </summary>
        private static bool TryGetNumericValue(object? value, out float result)
        {
            result = 0f;

            if (value == null)
                return false;

            try
            {
                // Use pattern matching with switch expression (C# 8.0+)
                switch (value)
                {
                    case float f:
                        result = f;
                        return true;
                    case int i:
                        result = i;
                        return true;
                    case double d:
                        result = (float)d;
                        return true;
                    case bool b:
                        result = b ? 1f : 0f;
                        return true;
                    case string str:
                        return float.TryParse(str, out result);
                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Clean up all comparison data and restore original text
        /// </summary>
        private static void CleanupComparisonData()
        {
            try
            {
                int comparisonCount = comparisonDataMap.Count;

                if (comparisonCount > 0)
                {
                    ModLogger.Log("WeaponComparison", $"Cleaning up: {comparisonCount} comparisons");
                }

                // Restore original text for all entries using deconstruction
                foreach (var (_, data) in comparisonDataMap)
                {
                    if (data is { ValueText: not null, OriginalText: not null })
                    {
                        data.ValueText.text = data.OriginalText;
                    }
                }

                comparisonDataMap.Clear();
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Error cleaning up comparison data: {ex}");
            }
        }

        /// <summary>
        /// Clean up comparison data when hovering UI is hidden
        /// </summary>
        [HarmonyPatch(typeof(ItemHoveringUI), "Hide")]
        [HarmonyPrefix]
        static void ItemHoveringUI_Hide_Prefix()
        {
            CleanupComparisonData();
        }
    }
}

