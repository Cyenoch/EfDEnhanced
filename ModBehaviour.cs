using System;
using System.Collections.Generic;
using System.Linq;
using EfDEnhanced.Features;
using EfDEnhanced.Utils;
using HarmonyLib;
using UnityEngine;

namespace EfDEnhanced;

public class ModBehaviour : Duckov.Modding.ModBehaviour
{
    private const string HARMONY_ID = "com.efdenhanced.mod";
    private static Harmony? _harmonyInstance;
    
    private ActiveQuestTracker? _questTracker;
    private bool _wasInRaid;

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
        ModLogger.Log("Version: 2510202100");

        // Initialize localization system
        LocalizationHelper.Initialize();
        
        // Initialize quest tracking manager
        QuestTrackingManager.Initialize();

        _harmonyInstance = new Harmony(HARMONY_ID);
        _harmonyInstance.PatchAll();

        // Log all patched methods
        var patchedMethods = _harmonyInstance.GetPatchedMethods();
        ModLogger.Log("Harmony", $"Applied {patchedMethods.Count()} patches:");
        foreach (var method in patchedMethods)
        {
            ModLogger.Log("Harmony", $"  - {method.DeclaringType?.Name}.{method.Name}");
        }
        
        // Initialize quest tracker
        InitializeQuestTracker();
    }
    
    void Update()
    {
        try
        {
            // Check if player entered or left raid
            CheckRaidStatus();
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"ModBehaviour.Update failed: {ex}");
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
    
    /// <summary>
    /// 初始化任务追踪器
    /// </summary>
    private void InitializeQuestTracker()
    {
        try
        {
            _questTracker = ActiveQuestTracker.Create();
            ModLogger.Log("ModBehaviour", "Quest tracker initialized");
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"Failed to initialize quest tracker: {ex}");
        }
    }
    
    /// <summary>
    /// 检查Raid状态并相应启用/禁用追踪器
    /// </summary>
    private void CheckRaidStatus()
    {
        try
        {
            if (LevelManager.Instance == null)
            {
                if (_wasInRaid)
                {
                    _wasInRaid = false;
                    _questTracker?.Disable();
                }
                return;
            }
            
            bool isInRaid = LevelManager.Instance.IsRaidMap;
            
            // 进入Raid
            if (isInRaid && !_wasInRaid)
            {
                _wasInRaid = true;
                _questTracker?.Enable();
                ModLogger.Log("ModBehaviour", "Entered raid - quest tracker enabled");
            }
            // 离开Raid
            else if (!isInRaid && _wasInRaid)
            {
                _wasInRaid = false;
                _questTracker?.Disable();
                ModLogger.Log("ModBehaviour", "Left raid - quest tracker disabled");
            }
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"CheckRaidStatus failed: {ex}");
        }
    }
}
