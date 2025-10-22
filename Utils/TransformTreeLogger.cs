using UnityEngine;
using Duckov.Modding;
using System.Collections.Generic;
using System;

namespace EfDEnhanced.Utils
{
    /// <summary>
    /// Transform树形结构日志工具
    /// 用于输出从Root节点到指定Transform节点的路径，
    /// 以及该节点的所有子节点的树形结构
    /// </summary>
    public static class TransformTreeLogger
    {
        private const string LOG_COMPONENT = "TransformTreeLogger";

        /// <summary>
        /// 记录从Root到指定Transform及其所有子节点的树形结构
        /// </summary>
        /// <param name="transform">要记录的Transform节点</param>
        /// <param name="maxDepth">最大递归深度（-1表示无限制）</param>
        public static void LogTransformTree(Transform transform, int maxDepth = -1)
        {
            if (transform == null)
            {
                ModLogger.LogWarning(LOG_COMPONENT, "Transform is null");
                return;
            }

            try
            {
                // 获取从目标节点到Root的完整路径
                var ancestorPath = GetAncestorPath(transform);
                ancestorPath.Reverse(); // 反转为从Root到目标节点

                ModLogger.Log(LOG_COMPONENT, "╔═══════════════════════════════════╗");
                ModLogger.Log(LOG_COMPONENT, "║     Transform Tree Structure      ║");
                ModLogger.Log(LOG_COMPONENT, "╚═══════════════════════════════════╝");

                // 输出从Root到目标节点的路径
                for (int i = 0; i < ancestorPath.Count; i++)
                {
                    bool isTarget = i == ancestorPath.Count - 1;
                    string marker = isTarget ? "→ [目标]" : "  ";
                    string indent = GetIndent(i);
                    ModLogger.Log(LOG_COMPONENT, $"{indent}{marker} {ancestorPath[i].name} ({ancestorPath[i].GetType()})");
                }

                // 输出目标节点的所有子节点
                if (transform.childCount > 0)
                {
                    LogChildrenRecursive(transform, ancestorPath.Count, maxDepth);
                }
                else
                {
                    ModLogger.Log(LOG_COMPONENT, $"{GetIndent(ancestorPath.Count)}  (无子节点)");
                }

                ModLogger.Log(LOG_COMPONENT, "╔═══════════════════════════════════╗");
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"{LOG_COMPONENT}: Failed to log transform tree: {ex}");
            }
        }

        /// <summary>
        /// 简化版本：只记录指定Transform及其子节点（不显示Root路径）
        /// </summary>
        /// <param name="transform">要记录的Transform节点</param>
        /// <param name="maxDepth">最大递归深度（-1表示无限制）</param>
        public static void LogTransformTreeSimple(Transform transform, int maxDepth = -1)
        {
            if (transform == null)
            {
                ModLogger.LogWarning(LOG_COMPONENT, "Transform is null");
                return;
            }

            try
            {
                ModLogger.Log(LOG_COMPONENT, $"[根] {transform.name}");

                if (transform.childCount > 0)
                {
                    LogChildrenRecursive(transform, 0, maxDepth);
                }
                else
                {
                    ModLogger.Log(LOG_COMPONENT, "  └─ (无子节点)");
                }
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"{LOG_COMPONENT}: Failed to log transform tree: {ex}");
            }
        }

        /// <summary>
        /// 输出完整路径（从Root到指定节点的所有父节点）
        /// </summary>
        /// <param name="transform">要记录的Transform节点</param>
        public static void LogTransformPath(Transform transform)
        {
            if (transform == null)
            {
                ModLogger.LogWarning(LOG_COMPONENT, "Transform is null");
                return;
            }

            try
            {
                var ancestorPath = GetAncestorPath(transform);
                ancestorPath.Reverse();

                ModLogger.Log(LOG_COMPONENT, "完整路径: " + string.Join(" / ", ConvertToNames(ancestorPath)));
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"{LOG_COMPONENT}: Failed to log transform path: {ex}");
            }
        }

        /// <summary>
        /// 递归记录子节点
        /// </summary>
        private static void LogChildrenRecursive(Transform parent, int startDepth, int maxDepth)
        {
            if (maxDepth >= 0 && startDepth >= maxDepth)
                return;

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                int currentDepth = startDepth + 1;

                bool isLastChild = i == parent.childCount - 1;
                string treeMarker = isLastChild ? "└─ " : "├─ ";
                string indent = GetIndent(currentDepth);

                ModLogger.Log(LOG_COMPONENT, $"{indent}{treeMarker}{child.name} ({child.GetType()})");

                // 递归处理子节点
                if (child.childCount > 0 && (maxDepth < 0 || currentDepth < maxDepth))
                {
                    LogChildrenRecursive(child, currentDepth, maxDepth);
                }
            }
        }

        /// <summary>
        /// 获取从指定Transform到Root的祖先路径
        /// </summary>
        private static List<Transform> GetAncestorPath(Transform transform)
        {
            var path = new List<Transform>();
            Transform current = transform;

            while (current != null)
            {
                path.Add(current);
                current = current.parent;
            }

            return path;
        }

        /// <summary>
        /// 获取缩进字符串
        /// </summary>
        private static string GetIndent(int depth)
        {
            return new string(' ', depth * 2);
        }

        /// <summary>
        /// 将Transform列表转换为名称列表
        /// </summary>
        private static List<string> ConvertToNames(List<Transform> transforms)
        {
            var names = new List<string>();
            foreach (var transform in transforms)
            {
                names.Add(transform.name);
            }
            return names;
        }
    }
}
