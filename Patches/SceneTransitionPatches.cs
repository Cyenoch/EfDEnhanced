using System;
using Cysharp.Threading.Tasks;
using Duckov.UI;
using EfDEnhanced.Features;
using EfDEnhanced.Utils;
using HarmonyLib;

namespace EfDEnhanced.Patches;

/// <summary>
/// 场景转换拦截补丁
/// 在玩家通过 InteractableBase 进入其他场景前进行装备检查
/// 
/// 拦截目标：
/// 1. 获取 GetComponent<SceneLoaderProxy> 的 sceneID
/// 2. 需要特定道具的 "Interact" GameObject（如需要船票的交互）
/// </summary>
[HarmonyPatch(typeof(InteractableBase), "StartInteract")]
public class SceneTransitionPatches
{
    private static bool _isWaitingForConfirmation = false;
    private static bool _bypassCheck = false;

    /// <summary>
    /// 在StartInteract之前执行检查
    /// </summary>
    static bool Prefix(InteractableBase __instance, CharacterMainControl _interactCharacter)
    {
        try
        {
            // 如果正在等待确认，阻止重复调用
            if (_isWaitingForConfirmation)
            {
                ModLogger.Log("SceneTransitionCheck", "Already waiting for confirmation, blocking duplicate call");
                return false;
            }

            // 如果是用户确认后的调用，放行并重置标志
            if (_bypassCheck)
            {
                ModLogger.Log("SceneTransitionCheck", "Bypassing check (user confirmed)");
                _bypassCheck = false;
                return true;
            }

            // 判断目标scene是否raid
            TryGetTargetSceneId(__instance, out string? sceneId);
            if (sceneId == null)
            {
                ModLogger.Log("SceneTransitionCheck", "No target scene ID found, allowing transition");
                return true;
            }
            if (!RaidCheckUtility.ShouldCheckRaidMap(sceneId))
            {
                ModLogger.Log("SceneTransitionCheck", $"Target scene '{sceneId}' is not a Raid, allowing transition");
                return true;
            }

            string goName = __instance.gameObject.name;
            string sceneName = __instance.gameObject.scene.name;

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

            HandleCheckFailure(__instance, _interactCharacter, result).Forget();

            // 阻止原方法执行
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
        InteractableBase interactable,
        CharacterMainControl character,
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

                // 设置绕过标志并重新调用交互
                _bypassCheck = true;
                _isWaitingForConfirmation = false;

                // 重新触发交互
                // 这次 Prefix 会因为 _bypassCheck = true 而放行
                interactable.StartInteract(character);
            }
            else
            {
                ModLogger.Log("SceneTransitionCheck", "User cancelled transition");
                // 用户取消，无需额外操作
                // InteractableBase 会自动处理取消逻辑
            }
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"HandleCheckFailure failed: {ex}");
        }
        finally
        {
            // 确保标志被重置
            _isWaitingForConfirmation = false;
            ModLogger.Log("SceneTransitionCheck", "HandleCheckFailure completed, waiting flag reset");
        }
    }
}

