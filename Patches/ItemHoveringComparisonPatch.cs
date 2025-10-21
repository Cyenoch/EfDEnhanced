using System;
using System.Collections.Generic;
using System.Linq;
using Duckov.UI;
using Duckov.Utilities;
using HarmonyLib;
using ItemStatsSystem;
using TMPro;
using UnityEngine;
using EfDEnhanced.Utils;

// Note: Using game's built-in Polarity system to determine if higher/lower values are better
// Polarity.Positive = higher is better (e.g. damage, accuracy)
// Polarity.Negative = lower is better (e.g. recoil, weight)
// Polarity.Neutral = no preference

namespace EfDEnhanced.Patches
{
  /// <summary>
  /// Patch for ItemHoveringUI to show comparison with selected item
  /// Supports:
  /// - Variables/Constants/Stats: Numeric comparison with color coding
  /// - Modifiers: Effect-based comparison, shown separately
  /// </summary>
  [HarmonyPatch]
  public class ItemHoveringComparisonPatch
  {
    // Store comparison data for each LabelAndValue entry
    private static Dictionary<LabelAndValue, ComparisonData> comparisonDataMap = new Dictionary<LabelAndValue, ComparisonData>();

    // Store additional effect displays
    private static List<GameObject> additionalEffectDisplays = new List<GameObject>();

    private class ComparisonData
    {
      public TextMeshProUGUI? selectedValueText;
      public TextMeshProUGUI? originalValueText;
      public Vector2 originalPosition;
    }

    // Structured property data
    private class PropertyData
    {
      public string key = "";           // Internal key for matching
      public string displayName = "";   // Display name
      public string displayValue = "";  // Display value
      public Polarity polarity;         // Polarity for comparison
      public PropertyType type;         // Type of property
      public object? rawValue;          // Raw value for numeric comparison
    }

    private enum PropertyType
    {
      Variable,    // CustomData Variable
      Constant,    // CustomData Constant
      Stat,        // Stat
      Modifier     // ModifierDescription (effects)
    }

    /// <summary>
    /// Patch ItemPropertiesDisplay.Setup to add comparison values
    /// </summary>
    [HarmonyPatch(typeof(ItemPropertiesDisplay), "Setup")]
    [HarmonyPostfix]
    static void ItemPropertiesDisplay_Setup_Postfix(ItemPropertiesDisplay __instance, Item targetItem)
    {
      try
      {
        // Always clean up previous comparison data first
        CleanupComparisonData();

        if (targetItem == null)
        {
          ModLogger.Log("ItemComparison", "Target item is null, skipping comparison");
          return;
        }

        Item selectedItem = ItemUIUtilities.SelectedItem;

        // Only show comparison if an item is selected and it's different from hover item
        if (selectedItem == null || selectedItem == targetItem)
        {
          return;
        }

        // Check if items are comparable (tags intersection)
        if (!AreItemsComparable(selectedItem, targetItem))
        {
          ModLogger.Log("ItemComparison", $"Items not comparable: {selectedItem.DisplayName} vs {targetItem.DisplayName}");
          return;
        }

        ModLogger.Log("ItemComparison",
            $"Comparing items:\n" +
            $"  Selected: {selectedItem.DisplayName} (ID: {selectedItem.TypeID})\n" +
            $"  Hovering: {targetItem.DisplayName} (ID: {targetItem.TypeID})");

        // Extract structured property data from both items
        var selectedProps = ExtractProperties(selectedItem);
        var hoverProps = ExtractProperties(targetItem);

        // Find all LabelAndValue components in the display
        var entries = __instance.GetComponentsInChildren<LabelAndValue>(includeInactive: false);

        if (entries.Length == 0)
        {
          ModLogger.LogWarning("No property entries found to compare");
          return;
        }

        // Separate properties by type
        var selectedNumerics = selectedProps.Where(p => p.type != PropertyType.Modifier).ToList();
        var hoverNumerics = hoverProps.Where(p => p.type != PropertyType.Modifier).ToList();

        var selectedModifiers = selectedProps.Where(p => p.type == PropertyType.Modifier).ToList();
        var hoverModifiers = hoverProps.Where(p => p.type == PropertyType.Modifier).ToList();

        // Check if numeric properties have exactly matching keys
        bool numericKeysMatch = CheckNumericKeysMatch(selectedNumerics, hoverNumerics);

        if (!numericKeysMatch)
        {
          ModLogger.Log("ItemComparison",
              "Numeric properties keys don't match exactly, skipping numeric comparison");
          return;
        }

        ModLogger.Log("ItemComparison", "Numeric properties keys match, proceeding with comparison");

        // Compare numeric properties (Variables/Constants/Stats)
        int entryIndex = 0;
        foreach (var hoverProp in hoverNumerics)
        {
          if (entryIndex >= entries.Length) break;

          var entry = entries[entryIndex];

          // Find matching property by key
          var selectedProp = selectedNumerics.FirstOrDefault(p => p.key == hoverProp.key);

          if (selectedProp != null)
          {
            // Found matching property, add comparison
            AddComparisonToEntry(entry, selectedProp.displayValue, hoverProp.displayValue,
                hoverProp.displayName, hoverProp.polarity, selectedProp.rawValue, hoverProp.rawValue);
          }

          entryIndex++;
        }
      }
      catch (Exception ex)
      {
        ModLogger.LogError($"Error in ItemPropertiesDisplay_Setup_Postfix: {ex}");
      }
    }

    /// <summary>
    /// Check if numeric properties have exactly matching keys (same count and all keys present in both)
    /// </summary>
    private static bool CheckNumericKeysMatch(List<PropertyData> list1, List<PropertyData> list2)
    {
      try
      {
        // First check: same count
        if (list1.Count != list2.Count)
        {
          ModLogger.Log("ItemComparison",
              $"Key count mismatch: {list1.Count} vs {list2.Count}");
          return false;
        }

        // Second check: all keys in list1 are in list2
        var keys1 = list1.Select(p => p.key).ToHashSet();
        var keys2 = list2.Select(p => p.key).ToHashSet();

        bool allKeysMatch = keys1.SetEquals(keys2);

        if (!allKeysMatch)
        {
          var onlyInList1 = keys1.Except(keys2).ToList();
          var onlyInList2 = keys2.Except(keys1).ToList();

          if (onlyInList1.Any())
          {
            ModLogger.Log("ItemComparison",
                $"Keys only in selected: [{string.Join(", ", onlyInList1)}]");
          }
          if (onlyInList2.Any())
          {
            ModLogger.Log("ItemComparison",
                $"Keys only in hover: [{string.Join(", ", onlyInList2)}]");
          }
        }

        return allKeysMatch;
      }
      catch (Exception ex)
      {
        ModLogger.LogError($"Error checking numeric keys match: {ex}");
        return false;
      }
    }

    /// <summary>
    /// Check if two items are comparable based on tag intersection
    /// </summary>
    private static bool AreItemsComparable(Item item1, Item item2)
    {
      try
      {
        // Same type ID - definitely comparable
        if (item1.TypeID == item2.TypeID)
          return true;

        // Check if tags have any intersection
        var tags1 = item1.Tags.ToList();
        var tags2 = item2.Tags.ToList();

        bool hasCommonTags = tags1.Intersect(tags2).Any();

        if (hasCommonTags)
        {
          ModLogger.Log("ItemComparison",
              $"Items comparable via tags: [{string.Join(", ", tags1.Intersect(tags2))}]");
        }

        return hasCommonTags;
      }
      catch (Exception ex)
      {
        ModLogger.LogError($"Error checking item comparability: {ex}");
        return false;
      }
    }

    /// <summary>
    /// Extract all properties from an item into structured data
    /// </summary>
    private static List<PropertyData> ExtractProperties(Item item)
    {
      var properties = new List<PropertyData>();

      try
      {
        // Extract Variables
        if (item.Variables != null)
        {
          foreach (CustomData variable in item.Variables)
          {
            if (variable.Display)
            {
              properties.Add(new PropertyData
              {
                key = variable.Key,
                displayName = variable.DisplayName,
                displayValue = variable.GetValueDisplayString(),
                polarity = Polarity.Neutral,
                type = PropertyType.Variable,
                rawValue = GetRawValue(variable)
              });
            }
          }
        }

        // Extract Constants
        if (item.Constants != null)
        {
          foreach (CustomData constant in item.Constants)
          {
            if (constant.Display)
            {
              properties.Add(new PropertyData
              {
                key = constant.Key,
                displayName = constant.DisplayName,
                displayValue = constant.GetValueDisplayString(),
                polarity = Polarity.Neutral,
                type = PropertyType.Constant,
                rawValue = GetRawValue(constant)
              });
            }
          }
        }

        // Extract Stats
        if (item.Stats != null)
        {
          foreach (Stat stat in item.Stats)
          {
            if (stat.Display)
            {
              properties.Add(new PropertyData
              {
                key = stat.Key,
                displayName = stat.DisplayName,
                displayValue = stat.Value.ToString(),
                polarity = Polarity.Neutral,
                type = PropertyType.Stat,
                rawValue = stat.Value
              });
            }
          }
        }

        // Extract Modifiers (effects)
        if (item.Modifiers != null)
        {
          foreach (ModifierDescription modifier in item.Modifiers)
          {
            if (modifier.Display)
            {
              Polarity polarity = StatInfoDatabase.GetPolarity(modifier.Key);
              if (modifier.Value < 0f)
              {
                polarity = (Polarity)(0 - polarity);
              }

              properties.Add(new PropertyData
              {
                key = modifier.Key,
                displayName = modifier.DisplayName,
                displayValue = modifier.GetDisplayValueString(),
                polarity = polarity,
                type = PropertyType.Modifier,
                rawValue = modifier.Value
              });
            }
          }
        }
      }
      catch (Exception ex)
      {
        ModLogger.LogError($"Error extracting properties: {ex}");
      }

      return properties;
    }

    /// <summary>
    /// Get raw numeric value from CustomData
    /// </summary>
    private static object? GetRawValue(CustomData data)
    {
      try
      {
        switch (data.DataType)
        {
          case CustomDataType.Float:
            return data.GetFloat();
          case CustomDataType.Int:
            return data.GetInt();
          case CustomDataType.Bool:
            return data.GetBool();
          case CustomDataType.String:
            return data.GetString();
          default:
            return null;
        }
      }
      catch
      {
        return null;
      }
    }

    /// <summary>
    /// Add comparison display to a LabelAndValue entry with color coding
    /// </summary>
    private static void AddComparisonToEntry(LabelAndValue entry, string selectedValue, string hoverValue,
        string propertyName, Polarity polarity, object? selectedRaw, object? hoverRaw)
    {
      try
      {
        // Get or create comparison data for this entry
        if (!comparisonDataMap.TryGetValue(entry, out var compData))
        {
          compData = new ComparisonData();
          comparisonDataMap[entry] = compData;
        }

        // Find the existing value text component
        var valueTextComponents = entry.GetComponentsInChildren<TextMeshProUGUI>(includeInactive: false);
        TextMeshProUGUI? originalValueText = null;

        foreach (var textComp in valueTextComponents)
        {
          // The value text is typically the second text component (first is label)
          if (textComp.text == hoverValue || textComp.text.Contains(hoverValue))
          {
            originalValueText = textComp;
            break;
          }
        }

        if (originalValueText == null)
        {
          ModLogger.LogWarning("Could not find original value text component");
          return;
        }

        RectTransform originalRect = originalValueText.GetComponent<RectTransform>();

        // Store original position and reference
        if (compData.originalValueText == null)
        {
          compData.originalValueText = originalValueText;
          compData.originalPosition = originalRect.anchoredPosition;
        }

        // Create or update selected value text (appears on LEFT side, faded)
        if (compData.selectedValueText == null)
        {
          // Create a new GameObject for selected value
          GameObject selectedValueObj = new GameObject("SelectedValueText");
          selectedValueObj.transform.SetParent(originalValueText.transform.parent, false);

          compData.selectedValueText = selectedValueObj.AddComponent<TextMeshProUGUI>();

          // Copy settings from original text
          compData.selectedValueText.font = originalValueText.font;
          compData.selectedValueText.fontSize = originalValueText.fontSize;
          compData.selectedValueText.fontStyle = originalValueText.fontStyle;
          // Use same alignment as original (usually right-aligned for values)
          compData.selectedValueText.alignment = originalValueText.alignment;

          // Set color to slightly faded white (60% opacity)
          compData.selectedValueText.color = new Color(1f, 1f, 1f, 0.6f);

          // Position it
          RectTransform selectedRect = selectedValueObj.GetComponent<RectTransform>();

          // Copy size and anchors from original
          selectedRect.anchorMin = originalRect.anchorMin;
          selectedRect.anchorMax = originalRect.anchorMax;
          selectedRect.pivot = originalRect.pivot;
          selectedRect.sizeDelta = originalRect.sizeDelta;

          // Place selected value to the RIGHT of hovering item value
          // Hovering item value stays at original position (left side)
          selectedRect.anchoredPosition = compData.originalPosition + new Vector2(70f, 0f);
        }

        // Update text values
        compData.selectedValueText.text = $"({selectedValue})";
        originalValueText.text = hoverValue;

        // Apply color coding based on value comparison and polarity
        ApplyComparisonColors(compData.selectedValueText, originalValueText, propertyName, polarity, selectedRaw, hoverRaw);

        // Ensure components are active
        compData.selectedValueText.gameObject.SetActive(true);
      }
      catch (Exception ex)
      {
        ModLogger.LogError($"Error adding comparison to entry: {ex}");
      }
    }

    /// <summary>
    /// Apply color coding to comparison values based on polarity and numeric difference
    /// </summary>
    private static void ApplyComparisonColors(TextMeshProUGUI selectedText, TextMeshProUGUI hoverText,
        string propertyName, Polarity polarity, object? selectedRaw, object? hoverRaw)
    {
      try
      {
        // Define colors
        Color betterColor = new Color(0.4f, 1f, 0.4f, 1f);      // Green for better
        Color worseColor = new Color(1f, 0.4f, 0.4f, 1f);       // Red for worse
        Color neutralColor = new Color(1f, 1f, 1f, 1f);         // White for neutral
        Color fadedBetterColor = new Color(0.4f, 1f, 0.4f, 0.6f);  // Faded green for selected
        Color fadedWorseColor = new Color(1f, 0.4f, 0.4f, 0.6f);   // Faded red for selected
        Color fadedNeutralColor = new Color(1f, 1f, 1f, 0.6f);     // Faded white for selected

        // If polarity is neutral, use neutral colors
        if (polarity == Polarity.Neutral)
        {
          selectedText.color = fadedNeutralColor;
          hoverText.color = neutralColor;
          return;
        }

        // Try to convert raw values to numeric
        if (TryGetNumericValue(selectedRaw, out float selectedNum) &&
            TryGetNumericValue(hoverRaw, out float hoverNum))
        {
          // Determine if hover value is better than selected value
          bool hoverIsBetter = false;

          if (polarity == Polarity.Positive)
          {
            // For positive polarity, higher is better
            hoverIsBetter = hoverNum > selectedNum;
          }
          else if (polarity == Polarity.Negative)
          {
            // For negative polarity, lower is better
            hoverIsBetter = hoverNum < selectedNum;
          }

          // Apply colors
          if (Math.Abs(hoverNum - selectedNum) < 0.001f)
          {
            // Values are equal
            selectedText.color = fadedNeutralColor;
            hoverText.color = neutralColor;
          }
          else if (hoverIsBetter)
          {
            // Hover item is better
            selectedText.color = fadedWorseColor;
            hoverText.color = betterColor;
          }
          else
          {
            // Selected item is better
            selectedText.color = fadedBetterColor;
            hoverText.color = worseColor;
          }

          // Detailed logging for color application
          string comparisonResult = hoverIsBetter ? "Better" : "Worse";
          string colorInfo = hoverIsBetter ? "Green" : "Red";

          ModLogger.Log("ItemComparison",
              $"  {propertyName}: [{selectedNum}] → [{hoverNum}] | " +
              $"Polarity: {polarity} | Result: {comparisonResult} ({colorInfo})");
        }
        else
        {
          // Non-numeric values, use neutral colors
          selectedText.color = fadedNeutralColor;
          hoverText.color = neutralColor;
        }
      }
      catch (Exception ex)
      {
        ModLogger.LogError($"Error applying comparison colors: {ex}");
        // Fallback to neutral colors
        selectedText.color = new Color(1f, 1f, 1f, 0.6f);
        hoverText.color = new Color(1f, 1f, 1f, 1f);
      }
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
        // Handle different numeric types
        if (value is float f)
        {
          result = f;
          return true;
        }
        else if (value is int i)
        {
          result = i;
          return true;
        }
        else if (value is double d)
        {
          result = (float)d;
          return true;
        }
        else if (value is bool b)
        {
          result = b ? 1f : 0f;
          return true;
        }
        else if (value is string s)
        {
          return TryParseNumericValue(s, out result);
        }

        return false;
      }
      catch
      {
        return false;
      }
    }

    /// <summary>
    /// Try to parse a numeric value from a string, handling various formats
    /// </summary>
    private static bool TryParseNumericValue(string value, out float result)
    {
      result = 0f;

      if (string.IsNullOrWhiteSpace(value))
        return false;

      // Remove common non-numeric suffixes and prefixes
      string cleaned = value.Trim()
          .Replace("%", "")      // Remove percentage
          .Replace("+", "")      // Remove plus sign
          .Replace("kg", "")     // Remove weight unit
          .Replace("m", "")      // Remove meter unit
          .Replace("s", "")      // Remove seconds (but be careful with numbers ending in 's')
          .Trim();

      // Try to parse as float
      return float.TryParse(cleaned, System.Globalization.NumberStyles.Any,
          System.Globalization.CultureInfo.InvariantCulture, out result);
    }

    /// <summary>
    /// Clean up all comparison data and UI elements
    /// </summary>
    private static void CleanupComparisonData()
    {
      try
      {
        int comparisonCount = comparisonDataMap.Count;
        int effectCount = additionalEffectDisplays.Count;

        if (comparisonCount > 0 || effectCount > 0)
        {
          ModLogger.Log("ItemComparison", $"Cleaning up: {comparisonCount} comparisons, {effectCount} effect displays");
        }

        // Clean up all comparison data
        foreach (var kvp in comparisonDataMap)
        {
          // Restore original position of value text
          if (kvp.Value.originalValueText != null)
          {
            RectTransform rect = kvp.Value.originalValueText.GetComponent<RectTransform>();
            if (rect != null)
            {
              rect.anchoredPosition = kvp.Value.originalPosition;
            }
          }

          // Destroy comparison UI elements
          if (kvp.Value.selectedValueText != null)
          {
            UnityEngine.Object.Destroy(kvp.Value.selectedValueText.gameObject);
          }
        }

        comparisonDataMap.Clear();

        // Clean up additional effect displays
        foreach (var effectObj in additionalEffectDisplays)
        {
          if (effectObj != null)
          {
            UnityEngine.Object.Destroy(effectObj);
          }
        }

        additionalEffectDisplays.Clear();
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

