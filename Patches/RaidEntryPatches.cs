using System;
using Cysharp.Threading.Tasks;
using Duckov;
using Duckov.UI;
using EfDEnhanced.Features;
using EfDEnhanced.Utils;
using HarmonyLib;

namespace EfDEnhanced.Patches;

/// <summary>
/// Raid进入点拦截补丁
/// 在玩家选择地图进入Raid前进行装备检查
/// 
/// 注意：Patch NotifyEntryClicked 而不是 LoadTask
/// 因为 LoadTask 是私有方法，Harmony 可能会有问题
/// NotifyEntryClicked 是在用户点击地图条目时调用的入口点
/// </summary>
[HarmonyPatch(typeof(MapSelectionView), "NotifyEntryClicked")]
public class RaidEntryPatches
{
    private static bool _isWaitingForConfirmation = false;
    private static bool _bypassCheck = false;
    
    /// <summary>
    /// 在NotifyEntryClicked之前执行检查
    /// </summary>
    static bool Prefix(MapSelectionView __instance, MapSelectionEntry mapSelectionEntry)
    {
        try
        {
            // 检查 loading 标志，防止重复点击
            var loadingField = typeof(MapSelectionView).GetField("loading",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (loadingField != null)
            {
                bool loading = (bool)loadingField.GetValue(__instance);
                if (loading)
                {
                    ModLogger.Log("RaidCheck", "MapSelectionView is already loading, ignoring click");
                    return false;
                }
            }
            
            // 如果正在等待确认，阻止重复调用
            if (_isWaitingForConfirmation)
            {
                ModLogger.Log("RaidCheck", "Already waiting for confirmation, blocking duplicate call");
                return false;
            }
            
            // 如果是用户确认后的调用，放行并重置标志
            if (_bypassCheck)
            {
                ModLogger.Log("RaidCheck", "Bypassing check (user confirmed)");
                _bypassCheck = false;
                return true;
            }
            
            // 获取目标场景ID
            string sceneID = mapSelectionEntry.SceneID;
            
            // 执行检查，传入场景ID以便只检查该场景相关的任务
            ModLogger.Log("RaidCheck", $"Starting raid readiness check for scene: {sceneID}");
            var result = RaidCheckUtility.CheckPlayerReadiness(sceneID);
            
            // 如果一切正常，直接放行
            if (result.IsReady)
            {
                ModLogger.Log("RaidCheck", "All checks passed, allowing entry");
                return true;
            }
            
            // 有问题，启动异步确认流程
            ModLogger.Log("RaidCheck", $"Issues detected for scene {sceneID}, showing confirmation dialog");
            
            // 立即设置 loading 标志，防止重复点击
            var loadingFieldSet = typeof(MapSelectionView).GetField("loading",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (loadingFieldSet != null)
            {
                loadingFieldSet.SetValue(__instance, true);
                ModLogger.Log("RaidCheck", "Set loading flag to prevent duplicate clicks");
            }
            
            HandleCheckFailure(__instance, mapSelectionEntry, result).Forget();
            
            // 阻止原方法执行
            return false;
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"RaidCheck Prefix patch failed: {ex}");
            // 出错时允许传送，避免阻止游戏正常运行
            return true;
        }
    }
    
    /// <summary>
    /// 处理检查失败的情况
    /// </summary>
    private static async UniTaskVoid HandleCheckFailure(
        MapSelectionView view,
        MapSelectionEntry mapEntry,
        RaidCheckResult result)
    {
        try
        {
            _isWaitingForConfirmation = true;
            
            // 获取或创建准备界面
            var prepView = RaidPreparationView.Instance ?? RaidPreparationView.Create();
            if (prepView == null)
            {
                ModLogger.LogError("Failed to create RaidPreparationView");
                ResetLoadingFlag(view);
                return;
            }
            
            // 显示准备界面并等待用户选择
            ModLogger.Log("RaidCheck", "Showing RaidPreparationView...");
            bool shouldContinue = await prepView.ShowAndWaitForConfirmation(result);
            ModLogger.Log("RaidCheck", $"User confirmation result: {shouldContinue}");
            
            if (shouldContinue)
            {
                ModLogger.Log("RaidCheck", "User chose to continue despite warnings");
                
                // 设置绕过标志并调用原始方法
                _bypassCheck = true;
                
                // 调用原始的 NotifyEntryClicked 逻辑
                // 这次 Prefix 会因为 _bypassCheck = true 而放行
                InvokeOriginalMethod(view, mapEntry);
            }
            else
            {
                ModLogger.Log("RaidCheck", "User cancelled raid entry");
                // 用户取消，重置 loading 标志，让UI恢复正常
                ResetLoadingFlag(view);
            }
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"HandleCheckFailure failed: {ex}");
            // 出错时也要重置 loading 标志
            ResetLoadingFlag(view);
        }
        finally
        {
            // 确保标志被重置，即使前面已经重置过也没关系
            _isWaitingForConfirmation = false;
            ModLogger.Log("RaidCheck", "HandleCheckFailure completed, waiting flag reset");
        }
    }
    
    /// <summary>
    /// 重置 MapSelectionView 的 loading 标志
    /// </summary>
    private static void ResetLoadingFlag(MapSelectionView view)
    {
        try
        {
            var loadingField = typeof(MapSelectionView).GetField("loading",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (loadingField != null)
            {
                loadingField.SetValue(view, false);
                ModLogger.Log("RaidCheck", "Reset loading flag");
            }
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"Failed to reset loading flag: {ex}");
        }
    }
    
    /// <summary>
    /// 调用原始的 NotifyEntryClicked 方法
    /// 通过重新调用方法，但这次 _bypassCheck 标志会让 Prefix 放行
    /// </summary>
    private static void InvokeOriginalMethod(MapSelectionView view, MapSelectionEntry mapEntry)
    {
        try
        {
            ModLogger.Log("RaidCheck", "Invoking original NotifyEntryClicked with bypass flag");
            
            // 直接调用LoadTask方法
            var loadTaskMethod = typeof(MapSelectionView).GetMethod("LoadTask",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (loadTaskMethod != null)
            {
                // 设置loading标志
                var loadingField = typeof(MapSelectionView).GetField("loading",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (loadingField != null)
                {
                    loadingField.SetValue(view, true);
                }
                
                // 设置beacon index
                LevelManager.loadLevelBeaconIndex = mapEntry.BeaconIndex;
                
                // 播放音效
                AudioManager.Post("UI/confirm");
                
                // 调用LoadTask
                var task = loadTaskMethod.Invoke(view, new object[] { mapEntry.SceneID, mapEntry.Cost });
                ModLogger.Log("RaidCheck", "Original method logic executed successfully");
            }
            else
            {
                ModLogger.LogError("Could not find LoadTask method");
                _bypassCheck = false;
            }
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"InvokeOriginalMethod failed: {ex}");
            _bypassCheck = false;
        }
    }
}

