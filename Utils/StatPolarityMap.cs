using System.Collections.Generic;
using ItemStatsSystem;

namespace EfDEnhanced.Utils
{
  /// <summary>
  /// Defines polarity (positive/negative/neutral) for weapon stat keys.
  /// Used for weapon comparison coloring in UI.
  /// 
  /// Positive Polarity: Higher values are better (damage, accuracy, etc.)
  /// Negative Polarity: Lower values are better (recoil, reload time, etc.)
  /// Neutral Polarity: Values have no clear "better" direction
  /// </summary>
  public static class StatPolarityMap
  {
    private static readonly Dictionary<string, Polarity> PolarityDefinitions = new()
    {
      // ===== DAMAGE & EFFECTIVENESS =====
      // Higher is better
      { "Damage", Polarity.Positive },
      { "ShootSpeed", Polarity.Positive },
      { "BulletSpeed", Polarity.Positive },
      { "BulletDistance", Polarity.Positive },
      { "CritRate", Polarity.Positive },
      { "CritDamageFactor", Polarity.Positive },
      { "ArmorPiercing", Polarity.Positive },
      { "ArmorBreak", Polarity.Positive },
      { "ExplosionDamageMultiplier", Polarity.Positive },

      // ===== MAGAZINE & AMMUNITION =====
      // Higher is better
      { "Capacity", Polarity.Positive },
      { "ShotCount", Polarity.Positive },
      { "BurstCount", Polarity.Positive },

      // ===== ACCURACY & CONTROL =====
      // Lower is better (less scatter/recoil is more accurate)
      { "DefaultScatter", Polarity.Negative },
      { "MaxScatter", Polarity.Negative },
      { "ScatterGrow", Polarity.Negative },
      { "DefaultScatterADS", Polarity.Negative },
      { "MaxScatterADS", Polarity.Negative },
      { "ScatterGrowADS", Polarity.Negative },
      { "RecoilVMin", Polarity.Negative },
      { "RecoilVMax", Polarity.Negative },
      { "RecoilHMin", Polarity.Negative },
      { "RecoilHMax", Polarity.Negative },
      { "RecoilTime", Polarity.Negative },

      // ===== RECOVERY & HANDLING =====
      // Lower is better (faster recovery)
      { "ScatterRecover", Polarity.Positive }, // Actually positive - faster recovery = higher value
      { "ScatterRecoverADS", Polarity.Positive },
      { "RecoilRecoverTime", Polarity.Negative },
      { "RecoilRecover", Polarity.Positive }, // Higher recover rate is better

      // ===== MOVEMENT & USABILITY =====
      // Higher is better (less movement penalty)
      { "MoveSpeedMultiplier", Polarity.Positive },
      { "AdsWalkSpeedMultiplier", Polarity.Positive },

      // ===== AIMING =====
      // Lower is better (faster ADS = lower time value)
      { "ADSTime", Polarity.Negative },
      { "ADSAimDistanceFactor", Polarity.Positive }, // Higher factor = better aim assist

      // ===== SOUND & DETECTION =====
      // Lower is better (less detectable)
      { "SoundRange", Polarity.Negative },

      // ===== VISUAL STATS =====
      // Higher is better (recoil scale affects control)
      { "RecoilScaleV", Polarity.Negative }, // Lower recoil scale = better control
      { "RecoilScaleH", Polarity.Negative },
      { "ScatterFactor", Polarity.Negative }, // Lower scatter factor = better accuracy
      { "ScatterFactorADS", Polarity.Negative },

      // ===== SPECIAL/BUFFS =====
      // Lower/Neutral
      { "Penetrate", Polarity.Neutral },
      { "BuffChance", Polarity.Positive },
      { "ShotAngle", Polarity.Neutral },
      { "FlashLight", Polarity.Neutral },
      { "ReloadTime", Polarity.Negative },
    };

    /// <summary>
    /// Get the polarity for a given stat key
    /// </summary>
    public static Polarity GetPolarity(string statKey)
    {
      if (string.IsNullOrEmpty(statKey))
      {
        return Polarity.Neutral;
      }

      if (PolarityDefinitions.TryGetValue(statKey, out var polarity))
      {
        return polarity;
      }

      // Default to neutral for unknown stats
      return Polarity.Neutral;
    }

    /// <summary>
    /// Check if a stat is defined in the polarity map
    /// </summary>
    public static bool IsDefined(string statKey)
    {
      return PolarityDefinitions.ContainsKey(statKey);
    }
  }
}
