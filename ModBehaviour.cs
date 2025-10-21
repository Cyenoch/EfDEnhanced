using System;
using System.Collections.Generic;
using System.Linq;
using EfDEnhanced.Features;
using EfDEnhanced.Utils;
using EfDEnhanced.Utils.Settings;
using HarmonyLib;
using UnityEngine;

namespace EfDEnhanced;

public class ModBehaviour : Duckov.Modding.ModBehaviour
{
    private const string HARMONY_ID = "com.efdenhanced.mod";
    private static Harmony? _harmonyInstance;
    
    private ActiveQuestTracker? _questTracker;
    private ModDebugPanel? _debugPanel;
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

        // Initialize settings system first (required by other components)
        ModSettings.Initialize();

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
        
        // Initialize debug panel (F8 to toggle)
        _debugPanel = ModDebugPanel.Create();
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
            // Unsubscribe from settings events
            if (_questTracker != null)
            {
                ModSettings.EnableQuestTracker.ValueChanged -= OnQuestTrackerEnabledChanged;
                ModSettings.TrackerShowDescription.ValueChanged -= OnQuestTrackerShowDescriptionChanged;
                ModLogger.Log("ModBehaviour", "Unsubscribed from quest tracker settings");
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
    
    /// <summary>
    /// 初始化任务追踪器
    /// </summary>
    private void InitializeQuestTracker()
    {
        try
        {
            _questTracker = ActiveQuestTracker.Create();

            // Subscribe to EnableQuestTracker setting changes
            ModSettings.EnableQuestTracker.ValueChanged += OnQuestTrackerEnabledChanged;

            // Subscribe to show description setting changes (even outside of raid)
            ModSettings.TrackerShowDescription.ValueChanged += OnQuestTrackerShowDescriptionChanged;

            ModLogger.Log("ModBehaviour", "Quest tracker initialized with settings subscriptions");
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"Failed to initialize quest tracker: {ex}");
        }
    }

    /// <summary>
    /// Handle quest tracker enabled setting changes
    /// </summary>
    private void OnQuestTrackerEnabledChanged(object? sender, SettingsValueChangedEventArgs<bool> args)
    {
        try
        {
            ModLogger.Log("ModBehaviour", $"Quest tracker enabled setting changed: {args.OldValue} -> {args.NewValue}");

            // If we're in a raid, update the tracker state
            if (_wasInRaid && _questTracker != null)
            {
                if (args.NewValue)
                {
                    // Enable the tracker if it wasn't enabled
                    _questTracker.Enable();
                }
                else
                {
                    // Disable the tracker if it was enabled
                    _questTracker.Disable();
                }
            }
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"OnQuestTrackerEnabledChanged failed: {ex}");
        }
    }

    /// <summary>
    /// Handle quest tracker description setting changes
    /// </summary>
    private void OnQuestTrackerShowDescriptionChanged(object? sender, SettingsValueChangedEventArgs<bool> args)
    {
        try
        {
            ModLogger.Log("ModBehaviour", $"Quest tracker show description setting changed: {args.OldValue} -> {args.NewValue}");

            // No need to do anything - the tracker itself subscribes to this when active
            // But we log it here for debugging
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"OnQuestTrackerShowDescriptionChanged failed: {ex}");
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
