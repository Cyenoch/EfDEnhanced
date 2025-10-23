using System;
using System.Collections.Generic;
using System.Linq;
using ItemStatsSystem;
using ItemStatsSystem.Items;

namespace EfDEnhanced.Utils;

/// <summary>
/// 物品检查辅助工具类
/// 提供统一的物品检查方法，减少重复代码
/// </summary>
public static class ItemCheckHelper
{
    /// <summary>
    /// 检查玩家是否满足基本装备要求
    /// </summary>
    public static BasicEquipmentCheck CheckBasicEquipment()
    {
        return ExceptionHelper.SafeExecute(() =>
        {
            return new BasicEquipmentCheck
            {
                HasWeapon = InventoryHelper.HasGun(ItemSourceFilter.CharacterInventory | ItemSourceFilter.PetInventory | ItemSourceFilter.SlotCollection),
                HasAmmo = InventoryHelper.HasAmmo(ItemSourceFilter.CharacterInventory | ItemSourceFilter.PetInventory | ItemSourceFilter.SlotCollection),
                HasMedicine = InventoryHelper.HasMedicine(ItemSourceFilter.CharacterInventory | ItemSourceFilter.PetInventory | ItemSourceFilter.SlotCollection),
                HasFood = InventoryHelper.HasFood(ItemSourceFilter.CharacterInventory | ItemSourceFilter.PetInventory | ItemSourceFilter.SlotCollection)
            };
        }, "CheckBasicEquipment", new BasicEquipmentCheck());
    }

    /// <summary>
    /// 检查弹药充足性
    /// </summary>
    public static List<LowAmmoWarning> CheckAmmoSufficiency()
    {
        return ExceptionHelper.SafeExecute(() =>
        {
            var warnings = new List<LowAmmoWarning>();
            var guns = InventoryHelper.GetGuns(ItemSourceFilter.CharacterInventory | ItemSourceFilter.PetInventory | ItemSourceFilter.SlotCollection);

            foreach (var gun in guns)
            {
                ExceptionHelper.SafeExecute(() =>
                {
                    CheckSingleGunAmmo(gun, warnings);
                }, $"CheckAmmoForGun_{gun.DisplayName}");
            }

            return warnings;
        }, "CheckAmmoSufficiency", new List<LowAmmoWarning>());
    }

    /// <summary>
    /// 检查单把枪的弹药是否充足
    /// </summary>
    private static void CheckSingleGunAmmo(Item gun, List<LowAmmoWarning> warnings)
    {
        try
        {
            var gunSetting = gun.GetComponent<ItemSetting_Gun>();
            if (gunSetting == null || gunSetting.Capacity <= 0) return;

            var gunCaliber = ItemTypeChecker.GetCaliber(gun);
            if (string.IsNullOrEmpty(gunCaliber)) return;

            int totalAmmoCount = InventoryHelper.CountAmmoByCaliber(gunCaliber, 
                ItemSourceFilter.CharacterInventory | ItemSourceFilter.PetInventory | ItemSourceFilter.SlotCollection);

            if (totalAmmoCount < gunSetting.Capacity)
            {
                warnings.Add(new LowAmmoWarning
                {
                    WeaponName = gun.DisplayName,
                    AmmoCaliber = gunCaliber,
                    CurrentAmmoCount = totalAmmoCount,
                    MagazineCapacity = gunSetting.Capacity
                });
            }
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"CheckSingleGunAmmo failed for {gun?.DisplayName ?? "unknown"}: {ex}");
        }
    }
}

/// <summary>
/// 基本装备检查结果
/// </summary>
public class BasicEquipmentCheck
{
    public bool HasWeapon { get; set; }
    public bool HasAmmo { get; set; }
    public bool HasMedicine { get; set; }
    public bool HasFood { get; set; }
    
    public bool IsComplete => HasWeapon && HasAmmo && HasMedicine && HasFood;
}
