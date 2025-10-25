using EfDEnhanced.Utils;
using EfDEnhanced.Utils.UI.Components;
using HarmonyLib;
using UnityEngine;

namespace EfDEnhanced.Patches;

[HarmonyPatch(typeof(TimeScaleManager), "Update")]
public static class TimeScalePatch
{
    /// <summary>
    /// Prefix returns false to skip the entire original method.
    /// </summary>
    [HarmonyPrefix]
    public static bool Prefix()
    {
        if (PieMenuManager.ActiveMenu != null)
        {
            Time.timeScale = ModSettings.ItemWheelTimeScale.Value;
            Time.fixedDeltaTime = Mathf.Max(0.0005f, Time.timeScale * 0.02f);
            return false;  // Skip original method, use custom time scale
        }
        return true;  // Execute original method for normal time management
    }
}

