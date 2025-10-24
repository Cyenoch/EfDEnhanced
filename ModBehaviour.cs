using System;
using System.Collections.Generic;
using System.Linq;
using EfDEnhanced.Features;
using EfDEnhanced.Utils;
using EfDEnhanced.Utils.Settings;
using HarmonyLib;
using Unity.VisualScripting;
using UnityEngine;

namespace EfDEnhanced;

public class ModBehaviour : Duckov.Modding.ModBehaviour
{
    private const string HARMONY_ID = "com.efdenhanced.mod";
    private static Harmony? _harmonyInstance;

    public static ModBehaviour? Instance { get; private set; }

    void Awake()
    {
        // Set singleton instance
        if (Instance != null)
        {
            ModLogger.LogWarning("Multiple ModBehaviour instances detected!");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Persist across scene changes
        DontDestroyOnLoad(gameObject);

        ModLogger.Log("=== EfDEnhanced Mod Loading ===");
        ModLogger.Log("Version: 2510202100");

        // Initialize localization system
        LocalizationHelper.Initialize();

        // Initialize settings system first (required by other components)
        ModSettings.Initialize();

        // Initialize quest tracking manager
        QuestTrackingManager.Initialize();

        // Initialize patches that need early setup
        Patches.FastBuySell.Initialize();

        _harmonyInstance = new Harmony(HARMONY_ID);
        _harmonyInstance.PatchAll();

        // Log all patched methods
        var patchedMethods = _harmonyInstance.GetPatchedMethods();
        ModLogger.Log("Harmony", $"Applied {patchedMethods.Count()} patches:");
        foreach (var method in patchedMethods)
        {
            ModLogger.Log("Harmony", $"  - {method.DeclaringType?.Name}.{method.Name}");
        }

        transform.AddComponent<ActiveQuestTracker>();
        transform.AddComponent<DuckQuackFeature>();
    }

    void OnDisable()
    {
        CleanupResources();
    }

    void OnDestroy()
    {
        CleanupResources();
    }

    /// <summary>
    /// Clean up all resources and subscriptions
    /// </summary>
    private void CleanupResources()
    {
        try
        {
            var questTracker = transform.GetComponent<ActiveQuestTracker>();
            if (questTracker != null)
            {
                Destroy(questTracker);
            }

            var duckQuackFeature = transform.GetComponent<DuckQuackFeature>();
            if (duckQuackFeature != null)
            {
                Destroy(duckQuackFeature);
            }

            // Clean up localization
            LocalizationHelper.Cleanup();

            // Remove Harmony patches
            _harmonyInstance?.UnpatchAll(HARMONY_ID);

            ModLogger.Log("ModBehaviour", "All resources cleaned up");
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"CleanupResources failed: {ex}");
        }
    }
}
