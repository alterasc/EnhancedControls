using HarmonyLib;
using Kingmaker;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Settings.Entities;
using Kingmaker.UI.Common;
using Kingmaker.UI.InputSystems.Enums;
using System;

namespace EnhancedControls.KeyboardBindings;

internal static class PrevTab
{
    private const string BIND_NAME = "EnhancedControls.PrevTab";

    internal static void RegisterBinding(KeyBindingData keyData)
    {
        Game.Instance.Keyboard.RegisterBinding(
            BIND_NAME,
            keyData,
            GameModesGroup.WorldFullscreenUI,
            false);
    }

    internal static IDisposable Bind()
    {
        return Game.Instance.Keyboard.Bind(BIND_NAME, delegate
        {
            var uiContext = Game.Instance.RootUiContext;
            var currentWindow = Game.Instance.RootUiContext.CurrentServiceWindow;
            var serviceWindowsVM = uiContext.IsSpace ? uiContext.SpaceVM.StaticPartVM.ServiceWindowsVM : uiContext.SurfaceVM.StaticPartVM.ServiceWindowsVM;
            if (currentWindow == ServiceWindowsType.CharacterInfo)
            {
                var characterInfoVM = serviceWindowsVM.CharacterInfoVM.Value;
                var pageType = characterInfoVM.m_CurrentPage.Value.PageType;
                CharInfoPageType prevTab = pageType switch
                {
                    CharInfoPageType.Biography => CharInfoPageType.FactionsReputation,
                    CharInfoPageType.FactionsReputation => CharInfoPageType.LevelProgression,
                    CharInfoPageType.LevelProgression => CharInfoPageType.Features,
                    CharInfoPageType.Features => CharInfoPageType.Summary,
                    _ => CharInfoPageType.Biography
                };
                EventBus.RaiseEvent(delegate (INewServiceWindowUIHandler h)
                {
                    h.HandleOpenCharacterInfoPage(prevTab);
                }, true);
            }
            else if (currentWindow == ServiceWindowsType.Inventory)
            {
                var inventoryVM = serviceWindowsVM.InventoryVM.Value;
                var inventoryStashVM = inventoryVM.StashVM;
                var itemsFilterVm = inventoryStashVM.ItemsFilter;
                var curValue = itemsFilterVm.CurrentFilter.Value;

                ItemsFilterType prevTab = curValue switch
                {
                    ItemsFilterType.ShipNoFilter => ItemsFilterType.NonUsable,
                    ItemsFilterType.NonUsable => ItemsFilterType.Notable,
                    ItemsFilterType.Notable => ItemsFilterType.Usable,
                    ItemsFilterType.Usable => ItemsFilterType.Accessories,
                    ItemsFilterType.Accessories => ItemsFilterType.Armor,
                    ItemsFilterType.Armor => ItemsFilterType.Weapon,
                    ItemsFilterType.Weapon => ItemsFilterType.NoFilter,
                    _ => ItemsFilterType.ShipNoFilter
                };
                itemsFilterVm.SetCurrentFilter(prevTab);
            }
        });
    }

    [HarmonyPatch]
    public static class Patches
    {
        /// <summary>
        /// Binds key after other service window keys are bound
        /// </summary>
        [HarmonyPatch(typeof(ServiceWindowsVM), nameof(ServiceWindowsVM.BindKeys))]
        [HarmonyPostfix]
        public static void Add(ServiceWindowsVM __instance)
        {
            __instance.AddDisposable(Bind());
        }
    }
}