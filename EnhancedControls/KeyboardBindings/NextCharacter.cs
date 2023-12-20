using EnhancedControls.Settings;
using HarmonyLib;
using Kingmaker;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.NameAndPortrait;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Sound;
using System.Collections.Generic;

namespace EnhancedControls.KeyboardBindings;

public class NextCharacter : ModHotkeySettingEntry
{
    private const string _key = "nextcharacter";
    private const string _title = "Next Character";
    private const string _tooltip = "Moves selection to the next character.";
    private const string _defaultValue = "Tab;;WorldFullscreenUI;false";
    private const string BIND_NAME = $"{PREFIX}.newcontrols.ui.{_key}";

    public NextCharacter() : base(_key, _title, _tooltip, _defaultValue) { }

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
            __instance.AddDisposable(Game.Instance.Keyboard.Bind(BIND_NAME, SelectNextCharacter));
        }

        private static void SelectNextCharacter()
        {
            var uiContext = Game.Instance.RootUiContext;
            var currentWindow = Game.Instance.RootUiContext.CurrentServiceWindow;
            if (currentWindow == ServiceWindowsType.CharacterInfo)
            {
                var serviceWindowsVM = uiContext.IsSpace ? uiContext.SpaceVM.StaticPartVM.ServiceWindowsVM : uiContext.SurfaceVM.StaticPartVM.ServiceWindowsVM;
                var characterInfoVM = serviceWindowsVM.CharacterInfoVM.Value;
                var nameAndPortraitVM = (CharInfoNameAndPortraitVM)characterInfoVM.ComponentVMs[CharInfoComponentType.NameAndPortrait].Value;
                nameAndPortraitVM.SelectNextCharacter();
            }
            else if (currentWindow == ServiceWindowsType.Inventory)
            {
                var serviceWindowsVM = uiContext.IsSpace ? uiContext.SpaceVM.StaticPartVM.ServiceWindowsVM : uiContext.SurfaceVM.StaticPartVM.ServiceWindowsVM;
                var inventoryVM = serviceWindowsVM.InventoryVM.Value;

                var nameAndPortraitVM = inventoryVM.NameAndPortraitVM;
                nameAndPortraitVM.SelectNextCharacter();
            }
            else if (uiContext.m_FullScreenUIType == Kingmaker.UI.Models.FullScreenUIType.Unknown)
            {
                List<BaseUnitEntity> actualGroup = Game.Instance.SelectionCharacter.ActualGroup;
                var curUnit = Game.Instance.SelectionCharacter.SelectedUnit;
                int num = (actualGroup.IndexOf(curUnit.Value) + 1) % actualGroup.Count;
                if (num < 0)
                {
                    num += actualGroup.Count;
                }
                Game.Instance.SelectionCharacter.SetSelected(actualGroup[num], false, false);
                if (actualGroup.Count == 1)
                {
                    UISounds.Instance.Sounds.Combat.CombatGridCantPerformActionClick.Play(null);
                }
            }
        }
    }
}