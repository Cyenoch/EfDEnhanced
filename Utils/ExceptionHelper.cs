using System;
using System.Collections.Generic;

namespace EfDEnhanced.Utils
{
    /// <summary>
    /// 异常处理辅助类 - 提供细化的异常处理和日志
    /// </summary>
    public static class ExceptionHelper
    {
        /// <summary>
        /// 安全执行操作，捕获并记录异常
        /// </summary>
        /// <param name="action">要执行的操作</param>
        /// <param name="contextName">上下文名称（用于日志）</param>
        /// <param name="onError">错误时的回调（可选）</param>
        /// <returns>是否成功执行</returns>
        public static bool SafeExecute(Action action, string contextName, Action? onError = null)
        {
            try
            {
                action?.Invoke();
                return true;
            }
            catch (NullReferenceException ex)
            {
                ModLogger.LogError($"{contextName}: Null reference - {ex.Message}\nStack: {ex.StackTrace}");
                onError?.Invoke();
                return false;
            }
            catch (ArgumentException ex)
            {
                ModLogger.LogError($"{contextName}: Invalid argument - {ex.Message}");
                onError?.Invoke();
                return false;
            }
            catch (InvalidOperationException ex)
            {
                ModLogger.LogError($"{contextName}: Invalid operation - {ex.Message}");
                onError?.Invoke();
                return false;
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"{contextName}: Unexpected error - {ex.GetType().Name}: {ex.Message}\nStack: {ex.StackTrace}");
                onError?.Invoke();
                return false;
            }
        }

        /// <summary>
        /// 安全执行操作并返回结果
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="func">要执行的函数</param>
        /// <param name="contextName">上下文名称（用于日志）</param>
        /// <param name="defaultValue">失败时的默认返回值</param>
        /// <returns>执行结果或默认值</returns>
        public static T SafeExecute<T>(Func<T> func, string contextName, T defaultValue)
        {
            try
            {
                return func != null ? func() : defaultValue;
            }
            catch (NullReferenceException ex)
            {
                ModLogger.LogError($"{contextName}: Null reference - {ex.Message}");
                return defaultValue;
            }
            catch (ArgumentException ex)
            {
                ModLogger.LogError($"{contextName}: Invalid argument - {ex.Message}");
                return defaultValue;
            }
            catch (InvalidOperationException ex)
            {
                ModLogger.LogError($"{contextName}: Invalid operation - {ex.Message}");
                return defaultValue;
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"{contextName}: Unexpected error - {ex.GetType().Name}: {ex.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// 检查对象是否为null，如果是则记录错误
        /// </summary>
        /// <param name="obj">要检查的对象</param>
        /// <param name="objectName">对象名称（用于日志）</param>
        /// <param name="contextName">上下文名称（用于日志）</param>
        /// <returns>对象不为null时返回true</returns>
        public static bool CheckNotNull<T>(T? obj, string objectName, string contextName) where T : class
        {
            if (obj == null)
            {
                ModLogger.LogError($"{contextName}: {objectName} is null");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 检查集合是否为null或空
        /// </summary>
        public static bool CheckNotNullOrEmpty<T>(ICollection<T>? collection, string collectionName, string contextName)
        {
            if (collection == null)
            {
                ModLogger.LogError($"{contextName}: {collectionName} is null");
                return false;
            }
            
            if (collection.Count == 0)
            {
                ModLogger.LogWarning($"{contextName}: {collectionName} is empty");
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// 安全地获取集合元素，带边界检查
        /// </summary>
        public static T? SafeGet<T>(IList<T> list, int index, string contextName) where T : class
        {
            if (list == null)
            {
                ModLogger.LogError($"{contextName}: List is null");
                return null;
            }

            if (index < 0 || index >= list.Count)
            {
                ModLogger.LogError($"{contextName}: Index {index} out of range [0, {list.Count})");
                return null;
            }

            return list[index];
        }

        /// <summary>
        /// 记录异常详细信息（用于开发调试）
        /// </summary>
        public static void LogDetailedException(Exception ex, string contextName)
        {
            ModLogger.LogError($"=== Exception Details in {contextName} ===");
            ModLogger.LogError($"Type: {ex.GetType().FullName}");
            ModLogger.LogError($"Message: {ex.Message}");
            ModLogger.LogError($"Stack Trace:\n{ex.StackTrace}");
            
            if (ex.InnerException != null)
            {
                ModLogger.LogError($"Inner Exception: {ex.InnerException.GetType().Name}");
                ModLogger.LogError($"Inner Message: {ex.InnerException.Message}");
            }
        }
    }
}

