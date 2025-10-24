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
    /// 该功能可在模组设置中启用/禁用
    /// </summary>
    [HarmonyPatch(typeof(StockShopView), nameof(StockShopView.OnInteractionButtonClicked))]
    public static class FastBuySell
    {
        private const string COMPONENT_NAME = "FastBuySell";
        private static ItemDisplay? _currentHoveredDisplay;
        private static bool _eventsSubscribed = false;

        /// <summary>
        /// Initialize and subscribe to hover events
        /// Should be called from ModBehaviour.Awake()
        /// </summary>
        public static void Initialize()
        {
            try
            {
                if (!_eventsSubscribed)
                {
                    ItemDisplay.OnPointerEnterItemDisplay += ItemDisplay_OnPointerEnter;
                    ItemDisplay.OnPointerExitItemDisplay += ItemDisplay_OnPointerExit;
                    _eventsSubscribed = true;
                    Log(COMPONENT_NAME, "Subscribed to hover events");
                }
            }
            catch (Exception ex)
            {
                LogError($"{COMPONENT_NAME}: Failed to subscribe to events: {ex}");
            }
        }

        private static void ItemDisplay_OnPointerEnter(ItemDisplay display)
        {
            try
            {
                _currentHoveredDisplay = display;
                VerboseLog(COMPONENT_NAME, $"Hovering over: {display?.Target?.DisplayName ?? "unknown"}");
            }
            catch (Exception ex)
            {
                LogError($"{COMPONENT_NAME}: Error in OnPointerEnter: {ex}");
            }
        }

        private static void ItemDisplay_OnPointerExit(ItemDisplay display)
        {
            try
            {
                _currentHoveredDisplay = null;
                VerboseLog(COMPONENT_NAME, $"Exited: {display?.Target?.DisplayName ?? "unknown"}");
            }
            catch (Exception ex)
            {
                LogError($"{COMPONENT_NAME}: Error in OnPointerExit: {ex}");
            }
        }

        [HarmonyPrefix]
        static bool StockShopView_OnInteractionButtonClicked(StockShopView __instance)
        {
            try
            {
                // Check if feature is enabled
                if (!ModSettings.FastBuyEnabled.Value && !ModSettings.FastSellEnabled.Value)
                {
                    return true; // Continue with original method
                }

                // If user has already selected an item (clicked on it), let original method handle it
                if (ItemUIUtilities.SelectedItem)
                {
                    return true; // Continue with original method
                }

                // Check if we have a hovered item
                if (_currentHoveredDisplay == null)
                {
                    return true; // Continue with original method
                }

                // Check if the item target is valid
                if (_currentHoveredDisplay.Target == null)
                {
                    return true; // Continue with original method
                }

                bool handled = false;

                if (_currentHoveredDisplay.IsStockshopSample)
                {
                    // Buying from shop
                    if (ModSettings.FastBuyEnabled.Value)
                    {
                        ExecuteBuy(__instance);
                        handled = true;
                    }
                }
                else
                {
                    // Selling to shop
                    if (ModSettings.FastSellEnabled.Value)
                    {
                        ExecuteSell(__instance);
                        handled = true;
                    }
                }

                // If we handled the interaction, skip the original method
                return !handled;
            }
            catch (Exception ex)
            {
                ExceptionHelper.LogDetailedException(ex, $"{COMPONENT_NAME}.OnInteractionButtonClicked");
                return true; // On error, continue with original method
            }
        }

        private static void ExecuteBuy(StockShopView shopView)
        {
            try
            {
                if (_currentHoveredDisplay == null || _currentHoveredDisplay.Target == null)
                {
                    return;
                }

                VerboseLog(COMPONENT_NAME, $"Buying: {_currentHoveredDisplay.Target.DisplayName}");

                int itemTypeID = _currentHoveredDisplay.Target.TypeID;

                // Check if item requires unlock confirmation
                if (EconomyManager.IsWaitingForUnlockConfirm(itemTypeID))
                {
                    EconomyManager.ConfirmUnlock(itemTypeID);
                    Log(COMPONENT_NAME, $"Confirmed unlock: {_currentHoveredDisplay.Target.DisplayName}");
                    return; // Return here, next press will actually buy
                }

                // Call buy task using reflection (BuyTask is private)
                var buyTaskMethod = AccessTools.Method(typeof(StockShopView), "BuyTask");
                if (buyTaskMethod != null)
                {
                    var task = (UniTask)buyTaskMethod.Invoke(shopView, new object[] { itemTypeID });
                    task.Forget();
                    Log(COMPONENT_NAME, $"Initiated buy: {_currentHoveredDisplay.Target.DisplayName}");
                }
                else
                {
                    LogWarning($"{COMPONENT_NAME}: BuyTask method not found");
                }
            }
            catch (Exception ex)
            {
                LogError($"{COMPONENT_NAME}: Failed to buy item: {ex}");
            }
        }

        private static void ExecuteSell(StockShopView shopView)
        {
            try
            {
                if (_currentHoveredDisplay == null || _currentHoveredDisplay.Target == null)
                {
                    return;
                }

                // Check if shopView.Target is valid
                if (shopView.Target == null)
                {
                    VerboseLog(COMPONENT_NAME, "Cannot sell: shop target is null");
                    return;
                }

                // Check if item can be sold
                if (!_currentHoveredDisplay.Target.CanBeSold)
                {
                    VerboseLog(COMPONENT_NAME, $"Cannot sell: {_currentHoveredDisplay.Target.DisplayName} is not sellable");
                    return;
                }

                VerboseLog(COMPONENT_NAME, $"Selling: {_currentHoveredDisplay.Target.DisplayName}");

                // Execute sell as async task
                shopView.Target.Sell(_currentHoveredDisplay.Target).Forget();
                
                // Play sound effect using the sound path directly
                AudioManager.Post("UI/sell");

                Log(COMPONENT_NAME, $"Sold: {_currentHoveredDisplay.Target.DisplayName}");
            }
            catch (Exception ex)
            {
                LogError($"{COMPONENT_NAME}: Failed to sell item: {ex}");
            }
        }
    }
}
