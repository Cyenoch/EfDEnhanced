using System.Collections.Generic;
using ItemStatsSystem;

namespace EfDEnhanced.Utils
{
    /// <summary>
    /// Defines polarity (positive/negative/neutral) for weapon stat keys.
    /// Used for weapon comparison coloring in UI.
    /// Supports both ranged weapons (guns) and melee weapons.
    /// 
    /// Positive Polarity: Higher values are better (damage, accuracy, etc.)
    /// Negative Polarity: Lower values are better (recoil, reload time, etc.)
    /// Neutral Polarity: Values have no clear "better" direction
    /// </summary>
    public static class StatPolarityMap
    {
        // Use target-typed new (C# 9.0) and make readonly for thread-safety
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

      // ===== MELEE WEAPONS =====
      // Melee-specific stats
      { "AttackSpeed", Polarity.Positive },        // Higher attack speed is better
      { "AttackRange", Polarity.Positive },        // Longer reach is better
      { "StaminaCost", Polarity.Negative },        // Lower stamina cost is better
      
      // ===== GAIN/BONUS STATS =====
      // These are bonus/gain stats from attachments or buffs
      { "DamageGain", Polarity.Positive },         // Bonus damage
      { "CritRateGain", Polarity.Positive },       // Bonus crit rate
      { "CritDamageFactorGain", Polarity.Positive }, // Bonus crit damage multiplier
      { "ArmorPiercingGain", Polarity.Positive },  // Bonus armor piercing
      { "ArmorBreakGain", Polarity.Positive },     // Bonus armor break
      { "ReloadSpeedGain", Polarity.Positive },    // Reload speed bonus (higher = faster)
      { "GunDamageGain", Polarity.Positive },      // Gun damage bonus
      
      // ===== DURABILITY =====
      { "DurabilityCost", Polarity.Negative },     // Lower durability cost is better
      
      // ===== MULTIPLIERS =====
      { "BulletSpeedMultiplier", Polarity.Positive }, // Higher bullet speed multiplier is better
      { "GunDamageMultiplier", Polarity.Positive },   // Gun damage multiplier
      { "MeleeDamageMultiplier", Polarity.Positive }, // Melee damage multiplier
      { "GunCritRateGain", Polarity.Positive },       // Gun crit rate gain
      { "GunCritDamageGain", Polarity.Positive },     // Gun crit damage gain
      
      // ===== PLAYER STATS =====
      // Movement
      { "WalkSpeed", Polarity.Positive },          // Walk speed
      { "RunSpeed", Polarity.Positive },           // Run speed
      { "DashSpeed", Polarity.Positive },          // Dash/roll speed
      { "WalkAcc", Polarity.Positive },            // Walk acceleration
      { "RunAcc", Polarity.Positive },             // Run acceleration
      { "Moveability", Polarity.Positive },        // Movement ability
      { "TurnSpeed", Polarity.Positive },          // Turn speed
      { "AimTurnSpeed", Polarity.Positive },       // Aim turn speed
      { "DashCanControl", Polarity.Positive },     // Dash direction control
      
      // Sound detection
      { "WalkSoundRange", Polarity.Negative },     // Walk sound range (lower = stealthier)
      { "RunSoundRange", Polarity.Negative },      // Run sound range (lower = stealthier)
      
      // Survival stats
      { "MaxHealth", Polarity.Positive },          // Maximum health
      { "MaxEnergy", Polarity.Positive },          // Maximum energy
      { "EnergyCost", Polarity.Negative },         // Energy consumption
      { "MaxWeight", Polarity.Positive },          // Maximum weight capacity
      { "Stamina", Polarity.Positive },            // Stamina
      { "StaminaDrainRate", Polarity.Negative },   // Stamina drain rate
      { "StaminaRecoverTime", Polarity.Negative }, // Stamina recovery time
      { "StaminaRecoverRate", Polarity.Positive }, // Stamina recovery rate
      { "MaxWater", Polarity.Positive },           // Maximum water
      { "WaterCost", Polarity.Negative },          // Water consumption
      { "HungerDuratbility", Polarity.Positive },  // Hunger durability (max energy)
      { "HungerDurability", Polarity.Positive },   // Alternative spelling
      
      // Protection
      { "BodyArmor", Polarity.Positive },          // Body armor
      { "HeadArmor", Polarity.Positive },          // Head armor
      { "GasMask", Polarity.Positive },            // Gas protection
      
      // Combat
      { "Attack", Polarity.Positive },             // Attack power
      { "RecoilControl", Polarity.Positive },      // Recoil control
      
      // Efficiency
      { "FoodGain", Polarity.Positive },           // Food efficiency
      { "HealGain", Polarity.Positive },           // Healing efficiency
      
      // Inventory
      { "InventoryCapacity", Polarity.Positive },  // Inventory capacity
      
      // ===== ELEMENTAL RESISTANCE =====
      // Lower damage taken multipliers are better
      { "ElementFactor_Physics", Polarity.Negative },     // Physical damage taken multiplier
      { "ElementFactor_Fire", Polarity.Negative },        // Fire damage taken multiplier
      { "ElementFactor_Poison", Polarity.Negative },      // Poison damage taken multiplier
      { "ElementFactor_Electricity", Polarity.Negative }, // Electric damage taken multiplier
      { "ElementFactor_Space", Polarity.Negative },       // Space damage taken multiplier
    };

        /// <summary>
        /// Get the polarity for a given stat key
        /// </summary>
        public static Polarity GetPolarity(string statKey)
        {
            // Use null-coalescing and TryGetValue in one expression
            return string.IsNullOrEmpty(statKey)
              ? Polarity.Neutral
              : PolarityDefinitions.TryGetValue(statKey, out var polarity)
                ? polarity
                : Polarity.Neutral;
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
