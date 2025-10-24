using HarmonyLib;
using ItemStatsSystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace EfDEnhanced.Patches
{
    /// <summary>
    /// Inventory.FindAll 在弹药装填/物品贩卖时寻找物品。只会在出库时调用，所以对入库没有影响。
    /// 这使得库存行为变成了后进先出，类stack，更符合直觉。
    /// 由于这些行为只会在玩家操作时调用一次，所以reverse操作几乎无性能影响。
    /// </summary>
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.FindAll))]
    static class ReverseInvBehavior
    {
        /*
        [HarmonyPrepare]
        static bool Enable()
        {
            return true
        }
        */
        [HarmonyPostfix]
        static void ReverseFindAll(ref List<Item> __result)
        {
            __result.Reverse();
        }
    }
}
