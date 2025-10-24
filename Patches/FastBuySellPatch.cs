using Duckov.Economy;
using Duckov;
using Duckov.Economy.UI;
using Duckov.UI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using Cysharp.Threading.Tasks;
using EfDEnhanced.Utils;
using static EfDEnhanced.Utils.ModLogger;
namespace EfDEnhanced.Patches
{

    /// <summary>
    /// 此功能允许你在商店菜单里把鼠标滑动到物品上按F（即交互按键）来快速购买/贩卖物品
    /// TODO: 我没写开关，你来写配置嘎（
    /// </summary>
    [HarmonyPatch(typeof(StockShopView), nameof(StockShopView.OnInteractionButtonClicked))]
    public static class FastBuySell
    {
        const string COMPONENTNAME = "FastBuySell";
        public static bool Enabled = true;
        private static ItemDisplay? _current;
        [HarmonyPrepare]
        static bool GetHoveringItemDisplay(MethodBase original)
        {

            if(Enabled && original is null)
            {
                ItemDisplay.OnPointerEnterItemDisplay += ItemDisplay_OnPointerEnterItemDisplay;
                ItemDisplay.OnPointerExitItemDisplay += ItemDisplay_OnPointerExitItemDisplay;
            }
            return Enabled;
        }

        private static void ItemDisplay_OnPointerEnterItemDisplay(ItemDisplay display)
        {
            VerboseLog($"Pointer Enter ItemDisplay {display.Target.DisplayName}");
            _current = display;
        }
        private static void ItemDisplay_OnPointerExitItemDisplay(ItemDisplay display)
        {
            VerboseLog($"Pointer Exit ItemDisplay {display.Target.DisplayName}");
            _current =null;
        }


        [HarmonyPostfix]
        static void StockShopView_OnInteractionButtonClicked(StockShopView __instance)
        {
            if (!Enabled) return;
            if (__instance.Selection) return;
            if (_current != null)
            {
                if (_current.IsStockshopSample)
                {
                    VerboseLog(COMPONENTNAME, $"Buying {_current.Target.DisplayName}");

                    int itemTypeID = _current.Target.TypeID;
                    if (EconomyManager.IsWaitingForUnlockConfirm(itemTypeID))
                    {
                        EconomyManager.ConfirmUnlock(itemTypeID);
                    }
                    __instance.BuyTask(itemTypeID).Forget();
                    return;
                }
                VerboseLog(COMPONENTNAME, $"Selling {_current.Target.DisplayName}");

                __instance.Target.Sell(_current.Target).Forget();
                AudioManager.Post(__instance.sfx_Sell);
                ItemUIUtilities.Select(null);
                __instance.OnSelectionChanged();
            }


        }

    }
}
