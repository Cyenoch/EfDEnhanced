using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Cysharp.Threading.Tasks;
using EfDEnhanced.Utils;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using ItemStatsSystem;
using UnityEngine;

namespace EfDEnhanced.Features;

public class DuckShitFeature : MonoBehaviour
{
    // 总水分消耗量
    private float totalHydrationConsumed = 0f;
    // 总能量消耗量
    private float totalEnergyConsumed = 0f;

    // 音频资源
    private static readonly Dictionary<string, Sound> fartSounds = [];
    private static bool soundsLoaded = false;

    private void Awake()
    {
        // 加载音频文件
        if (!soundsLoaded)
        {
            LoadFartSounds();
            soundsLoaded = true;
        }
    }

    private void LoadFartSounds()
    {
        try
        {
            string dllDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string[] audioNames = ["fart1", "fart2"];

            Sound sound = default(Sound);
            foreach (string name in audioNames)
            {
                string audioPath = Path.Combine(dllDir, name + ".mp3");

                if (!File.Exists(audioPath))
                {
                    ModLogger.LogWarning("DuckShit", $"音频文件不存在: {audioPath}");
                    continue;
                }

                // 使用 FMOD 创建音频
                FMOD.System coreSystem = RuntimeManager.CoreSystem;
                RESULT result = coreSystem.createSound(audioPath, MODE.DEFAULT, out sound);

                if (result == RESULT.OK)
                {
                    fartSounds.TryAdd(name, sound);
                    ModLogger.Log("DuckShit", $"音频加载成功: {name}.mp3");
                }
                else
                {
                    ModLogger.LogError("DuckShit", $"音频加载失败 ({name}): {result}");
                }
            }
        }
        catch (System.Exception ex)
        {
            ModLogger.LogError("DuckShit", $"加载音频时发生异常: {ex.Message}");
        }
    }

    private void PlayRandomFart(float volume = 0.75f)
    {
        try
        {
            if (fartSounds.Count == 0)
            {
                ModLogger.LogWarning("DuckShit", "没有可用的fart音频");
                return;
            }

            // 随机选择 fart1 或 fart2
            string soundName = Random.Range(0, 2) == 0 ? "fart1" : "fart2";

            if (!fartSounds.ContainsKey(soundName))
            {
                ModLogger.LogWarning("DuckShit", $"音频 {soundName} 未加载");
                return;
            }

            // 获取 FMOD 音效总线
            Bus bus = RuntimeManager.GetBus("bus:/Master/SFX");
            bus.getChannelGroup(out ChannelGroup channelGroup);

            // 播放音频
            FMOD.System coreSystem = RuntimeManager.CoreSystem;
            coreSystem.playSound(fartSounds[soundName], channelGroup, false, out FMOD.Channel channel);

            // 设置音量
            channel.setVolume(volume);

            ModLogger.Log("DuckShit", $"播放音效: {soundName}");
        }
        catch (System.Exception ex)
        {
            ModLogger.LogError("DuckShit", $"播放音频时发生异常: {ex.Message}");
        }
    }

    private float variableTickTimer = 0f;

    public void TickVariables(float deltaTime, float tickTime)
    {
        variableTickTimer += deltaTime;
        if (variableTickTimer < tickTime)
        {
            // ModLogger.Log("DuckShit", $"Waiting for tick: {variableTickTimer:F2}/{tickTime:F2}s");
            return;
        }

        variableTickTimer = 0f;
        if (CharacterMainControl.Main == null)
        {
            return;
        }

        var player = CharacterMainControl.Main;
        if (LevelManager.Instance.IsRaidMap && !player.Health.Invincible)
        {
            float energyIncrease = player.EnergyCostPerMin * tickTime / 60f;
            float hydrationIncrease = player.WaterCostPerMin * tickTime / 60f;
            totalEnergyConsumed += energyIncrease;
            totalHydrationConsumed += hydrationIncrease;
            // ModLogger.Log("DuckShit", $"Energy consumed: {energyIncrease}, Hydration consumed: {hydrationIncrease}");
        }

        if (totalEnergyConsumed >= 60 || totalHydrationConsumed >= 60)
        {
            // ModLogger.Log("DuckShit", "Threshold reached, triggering poop/quack event.");
            totalEnergyConsumed = 0;
            totalHydrationConsumed = 0;
            // DuckQuackFeature.Quack();
            PoopShit().Forget();
        }
    }

    private async UniTask PoopShit()
    {
        var player = CharacterMainControl.Main;
        if (player == null)
        {
            ModLogger.Log("DuckShit", "Cannot poop: no player instance!");
            return;
        }
        ModLogger.Log("DuckShit", $"Attempting to instantiate shit item at position {player.transform.position}, aim direction: {player.CurrentAimDirection}");
        var shit = await ItemAssetsCollection.InstantiateAsync(938);
        if (shit == null)
        {
            ModLogger.Log("DuckShit", "Failed to instantiate shit item (id 938).");
            return;
        }

        PlayRandomFart();

        await UniTask.Delay(300);

        // 反向投出
        shit.Drop(player.transform.position, true, -player.CurrentAimDirection, 0f);
        ModLogger.Log("DuckShit", $"Poop dropped at {player.transform.position}, direction: {-player.CurrentAimDirection}");
    }

    private void Update()
    {
        TickVariables(Time.deltaTime, 1f);
    }
}
