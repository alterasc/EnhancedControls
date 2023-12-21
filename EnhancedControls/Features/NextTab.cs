using EnhancedControls.Settings;
using HarmonyLib;
using Kingmaker;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;

namespace EnhancedControls.Features;

public class NextTab : ModHotkeySettingEntry
{
    private const string _key = "nexttab";
    private const string _title = "Next Menu Tab";
    private const string _tooltip = "In character menu moves selection to next page tab  (so Summary -> Features -> Archetypes ->...)\r\nIn inventory moves inventory filter (All -> Weapons -> Armor -> ...)";
    private const string _defaultValue = "#E;;WorldFullscreenUI;false";
    private const string BIND_NAME = $"{PREFIX}.newcontrols.ui.{_key}";

    public NextTab() : base(_key, _title, _tooltip, _defaultValue) { }

    public override SettingStatus TryEnable() => TryEnableAndPatch(typeof(Patches));

    [HarmonyPatch]
    private static class Patches
    {
        /// <summary>
        /// Binds key after other service window keys are bound
        /// </summary>
        [HarmonyPatch(typeof(ServiceWindowsVM), nameof(ServiceWindowsVM.BindKeys))]
        [HarmonyPostfix]
        private static void Add(ServiceWindowsVM __instance)
        {
            __instance.AddDisposable(Game.Instance.Keyboard.Bind(BIND_NAME, SelectNexTab));
        }

        private static void SelectNexTab()
        {
            var uiContext = Game.Instance.RootUiContext;
            var currentWindow = Game.Instance.RootUiContext.CurrentServiceWindow;
            var serviceWindowsVM = uiContext.IsSpace ? uiContext.SpaceVM.StaticPartVM.ServiceWindowsVM : uiContext.SurfaceVM.StaticPartVM.ServiceWindowsVM;
            if (currentWindow == ServiceWindowsType.CharacterInfo)
            {
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
        }
    }
}