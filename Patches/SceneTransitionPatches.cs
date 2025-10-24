using System;
using Cysharp.Threading.Tasks;
using Duckov.UI;
using EfDEnhanced.Features;
using EfDEnhanced.Utils;
using HarmonyLib;

namespace EfDEnhanced.Patches;

/// <summary>
/// 场景转换拦截补丁
/// 在玩家通过交互进入其他场景前进行装备检查
/// 
/// 通过 patch CA_Interact.OnStart 在交互 action 启动时拦截
/// 这样可以在更高层面控制交互流程，避免状态不一致
/// </summary>
[HarmonyPatch(typeof(CA_Interact), "OnStart")]
public class SceneTransitionPatches
{
    private static bool _isWaitingForConfirmation = false;
    private static bool _bypassCheck = false;
    private static InteractableBase? _pendingInteractable = null;

    /// <summary>
    /// 在CA_Interact.OnStart之前执行检查
    /// 拦截交互action的启动，检查是否需要raid准备确认
    /// </summary>
    static bool Prefix(CA_Interact __instance, ref bool __result)
    {
        try
        {
            // 获取交互目标
            var interactTarget = __instance.InteractTarget;
            if (interactTarget == null)
            {
                // 没有交互目标，让原方法处理
                return true;
            }

            // 如果正在等待确认，阻止重复调用
            if (_isWaitingForConfirmation)
            {
                ModLogger.Log("SceneTransitionCheck", "Already waiting for confirmation, blocking duplicate call");
                __result = false;
                return false;
            }

            // 如果是用户确认后的调用，放行并重置标志
            if (_bypassCheck)
            {
                ModLogger.Log("SceneTransitionCheck", "Bypassing check (user confirmed)");
                _bypassCheck = false;
                return true; // 让原OnStart执行
            }

            // 判断目标scene是否raid
            if (!TryGetTargetSceneId(interactTarget, out string? sceneId) || string.IsNullOrEmpty(sceneId))
            {
                // 不是场景加载交互，放行
                return true;
            }

            if (!RaidCheckUtility.ShouldCheckRaidMap(sceneId!))
            {
                ModLogger.Log("SceneTransitionCheck", $"Target scene '{sceneId}' is not a Raid, allowing transition");
                return true;
            }

            string goName = interactTarget.gameObject.name;
            string sceneName = interactTarget.gameObject.scene.name;

            ModLogger.Log("SceneTransitionCheck", $"Detected scene transition: {goName} in scene {sceneName} to scene {sceneId}");

            // 执行检查
            var result = RaidCheckUtility.CheckPlayerReadiness(sceneId);

            // 如果一切正常，直接放行
            if (result.IsReady)
            {
                ModLogger.Log("SceneTransitionCheck", "All checks passed, allowing transition");
                return true;
            }

            // 有问题，启动异步确认流程
            ModLogger.Log("SceneTransitionCheck", $"Issues detected for transition {goName}, showing confirmation dialog");

            // 保存当前的交互目标，防止在确认过程中丢失
            _pendingInteractable = interactTarget;

            HandleCheckFailure(__instance, result).Forget();

            // 阻止原方法执行，返回false表示action启动失败
            __result = false;
            return false;
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"SceneTransitionCheck Prefix patch failed: {ex}");
            // 出错时允许传送，避免阻止游戏正常运行
            return true;
        }
    }

    /// <summary>
    /// 判断是否是需要拦截的场景转换交互
    /// </summary>
    private static bool TryGetTargetSceneId(InteractableBase interactable, out string? sceneId)
    {
        var sceneLoaderProxy = interactable.GetComponent<SceneLoaderProxy>();
        sceneId = null;
        if (sceneLoaderProxy == null)
        {
            return false;
        }
        // Use reflection to get private 'sceneID' field
        var field = typeof(SceneLoaderProxy).GetField("sceneID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            var value = field.GetValue(sceneLoaderProxy) as string;
            sceneId = value;
            return !string.IsNullOrEmpty(sceneId);
        }
        return false;
    }

    /// <summary>
    /// 处理检查失败的情况
    /// </summary>
    private static async UniTaskVoid HandleCheckFailure(
        CA_Interact interactAction,
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
                return;
            }

            // 显示准备界面并等待用户选择
            ModLogger.Log("SceneTransitionCheck", "Showing RaidPreparationView...");
            bool shouldContinue = await prepView.ShowAndWaitForConfirmation(result);
            ModLogger.Log("SceneTransitionCheck", $"User confirmation result: {shouldContinue}");

            if (shouldContinue)
            {
                ModLogger.Log("SceneTransitionCheck", "User chose to continue despite warnings");

                // 确保交互目标还是原来的那个
                if (_pendingInteractable != null)
                {
                    ModLogger.Log("SceneTransitionCheck", "Re-setting interact target and starting action");

                    // 重新设置交互目标，确保不会因为玩家移动而丢失
                    interactAction.SetInteractableTarget(_pendingInteractable);

                    // 关键：在重新启动action之前，必须先重置等待标志和设置绕过标志
                    // 否则StartAction会立即调用OnStart，被我们的"Already waiting"检查阻止
                    _isWaitingForConfirmation = false;
                    _bypassCheck = true;

                    // 重新启动 CA_Interact action
                    // 这次会绕过我们的检查，直接执行原OnStart逻辑
                    var character = interactAction.characterController;
                    if (character != null)
                    {
                        ModLogger.Log("SceneTransitionCheck", "Starting action with bypass flag set");
                        character.StartAction(interactAction);
                    }
                    else
                    {
                        ModLogger.LogWarning("Character controller is null, cannot restart action");
                    }
                }
                else
                {
                    ModLogger.LogWarning("Pending interactable is null, cannot restart action");
                }
            }
            else
            {
                ModLogger.Log("SceneTransitionCheck", "User cancelled transition");
                // 用户取消，action已经在OnStart返回false时被阻止了
            }

            // 清理保存的交互目标
            _pendingInteractable = null;
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"HandleCheckFailure failed: {ex}");
        }
        finally
        {
            // 确保标志被重置（如果用户取消或发生错误）
            _isWaitingForConfirmation = false;
            _bypassCheck = false; // 也重置绕过标志，避免意外情况
            ModLogger.Log("SceneTransitionCheck", "HandleCheckFailure completed, all flags reset");
        }
    }
}

