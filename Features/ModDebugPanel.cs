using System;
using System.Linq;
using System.Text;
using EfDEnhanced.Utils;
using UnityEngine;

namespace EfDEnhanced.Features
{
    /// <summary>
    /// IMGUI调试面板 - 用于开发调试和运行时检查
    /// 按F8键显示/隐藏
    /// 使用Unity的IMGUI系统，无需预制体，快速简洁
    /// </summary>
    public class ModDebugPanel : MonoBehaviour
    {
        private bool _showDebug = false;
        private Vector2 _scrollPosition = Vector2.zero;
        private readonly StringBuilder _logBuilder = new StringBuilder();

        // GUI样式缓存
        private GUIStyle? _boxStyle;
        private GUIStyle? _labelStyle;
        private GUIStyle? _buttonStyle;

        private void Update()
        {
            // Toggle debug panel with F8
            if (Input.GetKeyDown(KeyCode.F8))
            {
                _showDebug = !_showDebug;
                ModLogger.Log("Debug", $"Debug panel {(_showDebug ? "opened" : "closed")}");
            }
        }

        private void OnGUI()
        {
            if (!_showDebug) return;

            InitializeStyles();

            // Debug panel area (top-right corner)
            GUILayout.BeginArea(new Rect(Screen.width - 420, 10, 410, Screen.height - 20));
            
            // Main container box
            GUILayout.BeginVertical(_boxStyle);
            
            DrawHeader();
            DrawModInfo();
            DrawQuestTrackerInfo();
            DrawSettingsInfo();
            DrawQuickActions();
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void InitializeStyles()
        {
            if (_boxStyle == null)
            {
                _boxStyle = new GUIStyle(GUI.skin.box)
                {
                    padding = new RectOffset(10, 10, 10, 10),
                    normal = { background = MakeTex(2, 2, new Color(0.1f, 0.1f, 0.1f, 0.95f)) }
                };
            }

            if (_labelStyle == null)
            {
                _labelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    normal = { textColor = Color.white }
                };
            }

            if (_buttonStyle == null)
            {
                _buttonStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 14,
                    padding = new RectOffset(10, 10, 5, 5)
                };
            }
        }

        private void DrawHeader()
        {
            var titleStyle = new GUIStyle(_labelStyle)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 0.8f, 0.2f) }
            };

            GUILayout.Label("EfDEnhanced Debug Panel", titleStyle);
            GUILayout.Space(5);
            
            var infoStyle = new GUIStyle(_labelStyle)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.gray }
            };
            GUILayout.Label("Press F8 to toggle", infoStyle);
            GUILayout.Space(10);
        }

        private void DrawModInfo()
        {
            DrawSectionHeader("Mod Information");
            
            GUILayout.Label($"Version: 2510202100", _labelStyle);
            GUILayout.Label($"Active: {ModBehaviour.Instance != null}", _labelStyle);
            GUILayout.Label($"Game Version: {Application.version}", _labelStyle);
            GUILayout.Label($"Unity Version: {Application.unityVersion}", _labelStyle);
            
            GUILayout.Space(10);
        }

        private void DrawQuestTrackerInfo()
        {
            DrawSectionHeader("Quest Tracker");
            
            GUILayout.Label($"Enabled: {ModSettings.EnableQuestTracker.Value}", _labelStyle);
            GUILayout.Label($"Tracked Quests: {QuestTrackingManager.TrackedQuestIds.Count}", _labelStyle);
            GUILayout.Label($"Position: ({ModSettings.TrackerPositionX.Value:F2}, {ModSettings.TrackerPositionY.Value:F2})", _labelStyle);
            GUILayout.Label($"Scale: {ModSettings.TrackerScale.Value:F2}", _labelStyle);
            GUILayout.Label($"Show Desc: {ModSettings.TrackerShowDescription.Value}", _labelStyle);
            
            // List tracked quests
            if (QuestTrackingManager.TrackedQuestIds.Count > 0)
            {
                GUILayout.Space(5);
                var scrollStyle = new GUIStyle(GUI.skin.box)
                {
                    padding = new RectOffset(5, 5, 5, 5)
                };
                
                _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, scrollStyle, GUILayout.Height(100));
                foreach (var questId in QuestTrackingManager.TrackedQuestIds)
                {
                    GUILayout.Label($"  - Quest ID: {questId}", _labelStyle);
                }
                GUILayout.EndScrollView();
            }
            
            GUILayout.Space(10);
        }

        private void DrawSettingsInfo()
        {
            DrawSectionHeader("Pre-Raid Check");
            
            GUILayout.Label($"Enabled: {ModSettings.EnableRaidCheck.Value}", _labelStyle);
            GUILayout.Label($"Check Weapon: {ModSettings.CheckWeapon.Value}", _labelStyle);
            GUILayout.Label($"Check Ammo: {ModSettings.CheckAmmo.Value}", _labelStyle);
            GUILayout.Label($"Check Meds: {ModSettings.CheckMeds.Value}", _labelStyle);
            GUILayout.Label($"Check Food: {ModSettings.CheckFood.Value}", _labelStyle);
            GUILayout.Label($"Check Weather: {ModSettings.CheckWeather.Value}", _labelStyle);
            
            GUILayout.Space(10);
        }

        private void DrawQuickActions()
        {
            DrawSectionHeader("Quick Actions");
            
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Reload Settings", _buttonStyle))
            {
                ModSettings.ReloadAll();
                ModLogger.Log("Debug", "Settings reloaded from storage");
            }
            
            if (GUILayout.Button("Reset Settings", _buttonStyle))
            {
                ModSettings.ResetToDefaults();
                ModLogger.Log("Debug", "Settings reset to defaults");
            }
            
            GUILayout.EndHorizontal();
            
            GUILayout.Space(5);
            
            if (GUILayout.Button("Clear Tracked Quests", _buttonStyle, GUILayout.Height(30)))
            {
                foreach (var questId in QuestTrackingManager.TrackedQuestIds.ToArray())
                {
                    QuestTrackingManager.SetQuestTracked(questId, false);
                }
                ModLogger.Log("Debug", "Cleared all tracked quests");
            }
            
            GUILayout.Space(5);
            
            // System info
            var sysInfoStyle = new GUIStyle(_labelStyle)
            {
                fontSize = 11,
                normal = { textColor = Color.gray }
            };
            GUILayout.Label($"FPS: {(int)(1f / Time.unscaledDeltaTime)}", sysInfoStyle);
            GUILayout.Label($"Memory: {(GC.GetTotalMemory(false) / 1024 / 1024)} MB", sysInfoStyle);
        }

        private void DrawSectionHeader(string title)
        {
            var headerStyle = new GUIStyle(_labelStyle)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.4f, 0.8f, 1f) }
            };
            
            GUILayout.Label(title, headerStyle);
            GUILayout.Space(3);
        }

        /// <summary>
        /// Create a simple colored texture for GUI backgrounds
        /// </summary>
        private Texture2D MakeTex(int width, int height, Color color)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
            {
                pix[i] = color;
            }

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();

            return result;
        }

        /// <summary>
        /// Static helper to create debug panel
        /// </summary>
        public static ModDebugPanel Create()
        {
            GameObject debugObj = new GameObject("ModDebugPanel");
            DontDestroyOnLoad(debugObj);
            
            var panel = debugObj.AddComponent<ModDebugPanel>();
            ModLogger.Log("Debug", "Debug panel created (Press F8 to toggle)");
            
            return panel;
        }
    }
}

