using System;
using System.Collections.Generic;
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

        _harmonyInstance = new Harmony(HARMONY_ID);
        _harmonyInstance.PatchAll();
    }

    void OnDisable()
    {
        _harmonyInstance.UnpatchAll(HARMONY_ID);
    }
}
