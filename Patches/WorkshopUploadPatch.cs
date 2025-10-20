using System;
using Duckov.Modding;
using EfDEnhanced.Utils;
using HarmonyLib;
using Steamworks;

namespace EfDEnhanced.Patches;

/// <summary>
/// Patch to prevent overwriting workshop item description during upload.
/// Only applies to EfDEnhanced mod (publishedFileId: 3590346461).
/// Other mods will upload normally with their descriptions updated.
/// </summary>
[HarmonyPatch(typeof(SteamUGC), "SetItemDescription")]
public class SteamUGCSetDescriptionPatch
{
    // EfDEnhanced's Steam Workshop ID
    public const ulong EFDENHANCED_WORKSHOP_ID = 3590346461;
    
    private static bool _isUploadingEfDEnhanced = false;
    
    /// <summary>
    /// Enable skipping description updates for EfDEnhanced only.
    /// </summary>
    public static void EnableSkipForEfDEnhanced()
    {
        _isUploadingEfDEnhanced = true;
        ModLogger.Log("WorkshopUpload", "EfDEnhanced upload detected - description will be preserved");
    }
    
    /// <summary>
    /// Disable skipping description updates after upload completes.
    /// </summary>
    public static void DisableSkip()
    {
        _isUploadingEfDEnhanced = false;
    }
    
    /// <summary>
    /// Prefix patch to skip SetItemDescription calls only for EfDEnhanced.
    /// Returns false to prevent the original method from executing.
    /// </summary>
    [HarmonyPrefix]
    static bool Prefix(UGCUpdateHandle_t handle, string pchDescription)
    {
        try
        {
            if (_isUploadingEfDEnhanced)
            {
                ModLogger.Log("WorkshopUpload", "Skipping SetItemDescription for EfDEnhanced to preserve workshop page");
                return false; // Skip original method for EfDEnhanced
            }
            return true; // Allow original method to execute for other mods
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"SteamUGCSetDescriptionPatch.Prefix failed: {ex}");
            return true; // On error, allow original method to proceed
        }
    }
}

/// <summary>
/// Patch to detect when EfDEnhanced is being uploaded to workshop.
/// </summary>
[HarmonyPatch(typeof(SteamWorkshopManager), "UploadWorkshopItem")]
public class WorkshopUploadTrackerPatch
{
    /// <summary>
    /// Check if the mod being uploaded is EfDEnhanced.
    /// </summary>
    [HarmonyPrefix]
    static void Prefix(string path)
    {
        try
        {
            // Try to process mod folder to get mod info
            if (ModManager.TryProcessModFolder(path, out var modInfo, isSteamItem: false, 0uL))
            {
                // Check if this is EfDEnhanced by workshop ID
                if (modInfo.publishedFileId == SteamUGCSetDescriptionPatch.EFDENHANCED_WORKSHOP_ID)
                {
                    SteamUGCSetDescriptionPatch.EnableSkipForEfDEnhanced();
                    ModLogger.Log("WorkshopUpload", $"Uploading EfDEnhanced (ID: {modInfo.publishedFileId}) - description will be preserved");
                }
                else
                {
                    ModLogger.Log("WorkshopUpload", $"Uploading other mod: {modInfo.name} - description will be updated normally");
                }
            }
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"WorkshopUploadTrackerPatch.Prefix failed: {ex}");
        }
    }
    
    /// <summary>
    /// Re-enable description updates after upload completes.
    /// </summary>
    [HarmonyPostfix]
    static void Postfix()
    {
        try
        {
            SteamUGCSetDescriptionPatch.DisableSkip();
            ModLogger.Log("WorkshopUpload", "Upload completed - description updates re-enabled for other mods");
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"WorkshopUploadTrackerPatch.Postfix failed: {ex}");
        }
    }
}

