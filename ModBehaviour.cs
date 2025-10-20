using System;
using System.Collections.Generic;
using System.Linq;
using EfDEnhanced.Utils;
using HarmonyLib;
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

        ModLogger.Log("=== EfDEnhanced Mod Loading ===");
        ModLogger.Log("Version: 2510202032");

        // Initialize localization system
        LocalizationHelper.Initialize();

        _harmonyInstance = new Harmony(HARMONY_ID);
        _harmonyInstance.PatchAll();

        // Log all patched methods
        var patchedMethods = _harmonyInstance.GetPatchedMethods();
        ModLogger.Log("Harmony", $"Applied {patchedMethods.Count()} patches:");
        foreach (var method in patchedMethods)
        {
            ModLogger.Log("Harmony", $"  - {method.DeclaringType?.Name}.{method.Name}");
        }
    }

    void OnDisable()
    {
        LocalizationHelper.Cleanup();
        _harmonyInstance?.UnpatchAll(HARMONY_ID);
    }
    
    void OnDestroy()
    {
        LocalizationHelper.Cleanup();
        _harmonyInstance?.UnpatchAll(HARMONY_ID);
    }
}
