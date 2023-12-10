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

    internal static void RegisterBinding()
    {
        try
        {
            var nextTabBind = new KeyBindingData(Main.Settings.NextTab);
            Game.Instance.Keyboard.RegisterBinding(
                       BIND_NAME,
                       nextTabBind.Key,
                       new GameModeType[] { GameModeType.Default, GameModeType.Pause },
                       nextTabBind.IsCtrlDown,
                       nextTabBind.IsAltDown,
                       nextTabBind.IsShiftDown);
        }
        catch (ArgumentException ex)
        {
            Main.log.Error($"Incorrect keybind format for NextTab action: {ex.Message}");
        }
    }

    internal static IDisposable Bind()
    {
        if (!Main.TryParseKeyBinding(Main.Settings.NextTab, out _)) return null;

        return Game.Instance.Keyboard.Bind(BIND_NAME, delegate
        {
            var uiContext = Game.Instance.RootUiContext;
            var currentWindow = Game.Instance.RootUiContext.CurrentServiceWindow;
            if (currentWindow == ServiceWindowsType.CharacterInfo)
            {
                var serviceWindowsVM = uiContext.SurfaceVM.StaticPartVM.ServiceWindowsVM;
                var characterInfoVM = serviceWindowsVM.CharacterInfoVM.Value;
                var pageType = characterInfoVM.m_CurrentPage.Value.PageType;
                if (pageType == CharInfoPageType.Summary)
                {

                }
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
}