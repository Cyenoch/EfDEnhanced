using System;
using Duckov;
using EfDEnhanced.Utils;
using UnityEngine;

namespace EfDEnhanced.Features
{
    /// <summary>
    /// Listens for the configured quack hotkey and triggers the player's quack voice.
    /// </summary>
    public class DuckQuackFeature : MonoBehaviour
    {
        private CharacterMainControl? _player;

        private void Awake()
        {
            ModLogger.Log("DuckQuack", "Duck quack feature initialized");
        }

        private void OnEnable()
        {
            ModLogger.Log("DuckQuack", "Duck quack feature enabled");
        }

        private void Update()
        {
            try
            {
                if (!ModSettings.EnableDuckQuack.Value)
                {
                    return;
                }

                _player ??= CharacterMainControl.Main;
                if (_player == null)
                {
                    return;
                }

                if (!Input.GetKeyDown(ModSettings.DuckQuackHotkey.Value))
                {
                    return;
                }

                if (ModSettings.DuckQuackHotkey.Value == KeyCode.Mouse0 || ModSettings.DuckQuackHotkey.Value == KeyCode.Mouse1 && Cursor.visible)
                {
                    return;
                }

                bool randomValue = UnityEngine.Random.value < 0.5f;

                AudioManager.Post("Char/Voice/vo_" + AudioManager.VoiceType.Duck.ToString().ToLower() + "_" + (randomValue ? "surprise" : "normal"));
                ModLogger.Log("DuckQuack", "Quack triggered");
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"DuckQuackFeature.Update failed: {ex}");
            }
        }
    }
}
