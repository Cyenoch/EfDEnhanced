using UnityEngine;

namespace EfDEnhanced.Utils;

/// <summary>
/// Centralized logging utility for Raid Ready Check mod.
/// All logs are prefixed with [EfDEnhanced] for easy filtering in Unity console.
/// </summary>
public static class ModLogger
{
    private const string ModPrefix = "[EfDEnhanced]";

    /// <summary>
    /// Log an informational message
    /// </summary>
    public static void Log(string message)
    {
        Debug.Log($"{ModPrefix} {message}");
    }

    /// <summary>
    /// Log a warning message
    /// </summary>
    public static void LogWarning(string message)
    {
        Debug.LogWarning($"{ModPrefix} {message}");
    }

    /// <summary>
    /// Log an error message
    /// </summary>
    public static void LogError(string message)
    {
        Debug.LogError($"{ModPrefix} {message}");
    }

    /// <summary>
    /// Log a message with a specific module/component identifier
    /// </summary>
    public static void Log(string component, string message)
    {
        Debug.Log($"{ModPrefix}[{component}] {message}");
    }

    /// <summary>
    /// Log a warning with a specific module/component identifier
    /// </summary>
    public static void LogWarning(string component, string message)
    {
        Debug.LogWarning($"{ModPrefix}[{component}] {message}");
    }

    /// <summary>
    /// Log an error with a specific module/component identifier
    /// </summary>
    public static void LogError(string component, string message)
    {
        Debug.LogError($"{ModPrefix}[{component}] {message}");
    }
}
