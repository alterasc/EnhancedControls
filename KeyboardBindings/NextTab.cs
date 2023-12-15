using HarmonyLib;
using Kingmaker;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Settings.Entities;
using Kingmaker.UI.Common;
using System;

namespace EnhancedControls.KeyboardBindings;

internal static class NextTab
{
    private const string BIND_NAME = "EnhancedControls.NextTab";

    internal static void RegisterBinding(KeyBindingData keyBindingData)
    {
        Game.Instance.Keyboard.RegisterBinding(
                   BIND_NAME,
                   keyBindingData.Key,
                   new GameModeType[] { GameModeType.Default, GameModeType.Pause },
                   keyBindingData.IsCtrlDown,
                   keyBindingData.IsAltDown,
                   keyBindingData.IsShiftDown);
    }

    internal static IDisposable Bind()
    {
        return Game.Instance.Keyboard.Bind(BIND_NAME, delegate
        {
            var uiContext = Game.Instance.RootUiContext;
            var currentWindow = Game.Instance.RootUiContext.CurrentServiceWindow;
            if (currentWindow == ServiceWindowsType.CharacterInfo)
            {
                var serviceWindowsVM = uiContext.SurfaceVM.StaticPartVM.ServiceWindowsVM;
                var characterInfoVM = serviceWindowsVM.CharacterInfoVM.Value;
                var pageType = characterInfoVM.m_CurrentPage.Value.PageType;
                CharInfoPageType nextTab = pageType switch
                {
                    CharInfoPageType.Summary => CharInfoPageType.Features,
                    CharInfoPageType.Features => CharInfoPageType.LevelProgression,
                    CharInfoPageType.LevelProgression => CharInfoPageType.FactionsReputation,
                    CharInfoPageType.FactionsReputation => CharInfoPageType.Biography,
                    _ => CharInfoPageType.Summary
                };
                EventBus.RaiseEvent(delegate (INewServiceWindowUIHandler h)
                {
                    h.HandleOpenCharacterInfoPage(nextTab);
                }, true);
            }
            else if (currentWindow == ServiceWindowsType.Inventory)
            {
                var serviceWindowsVM = uiContext.SurfaceVM.StaticPartVM.ServiceWindowsVM;
                var inventoryVM = serviceWindowsVM.InventoryVM.Value;
                var inventoryStashVM = inventoryVM.StashVM;
                var itemsFilterVm = inventoryStashVM.ItemsFilter;
                var curValue = itemsFilterVm.CurrentFilter.Value;

                ItemsFilterType nextTab = curValue switch
                {
                    ItemsFilterType.NoFilter => ItemsFilterType.Weapon,
                    ItemsFilterType.Weapon => ItemsFilterType.Armor,
                    ItemsFilterType.Armor => ItemsFilterType.Accessories,
                    ItemsFilterType.Accessories => ItemsFilterType.Usable,
                    ItemsFilterType.Usable => ItemsFilterType.Notable,
                    ItemsFilterType.Notable => ItemsFilterType.NonUsable,
                    ItemsFilterType.NonUsable => ItemsFilterType.ShipNoFilter,
                    _ => ItemsFilterType.NoFilter
                };
                itemsFilterVm.SetCurrentFilter(nextTab);
            }
        });
    }

    [HarmonyPatch(typeof(ServiceWindowsVM), nameof(ServiceWindowsVM.BindKeys))]
    public static class Patches
    {
        [HarmonyPostfix]
        public static void Add(ServiceWindowsVM __instance)
        {
            __instance.AddDisposable(Bind());
        }
    }
}