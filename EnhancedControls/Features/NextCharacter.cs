﻿using EnhancedControls.Common;
using EnhancedControls.Settings;
using HarmonyLib;
using Kingmaker;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.NameAndPortrait;

namespace EnhancedControls.Features;

public class NextCharacter : ModHotkeySettingEntry
{
    private const string _key = "nextcharacter";
    private const string _title = "Next Character";
    private const string _tooltip = "Moves selection to the next character.";
    private const string _defaultValue = "#D;;WorldFullscreenUI;false";
    private const string BIND_NAME = $"{PREFIX}.newcontrols.ui.{_key}";

    public NextCharacter() : base(_key, _title, _tooltip, _defaultValue) { }

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
                CharacterSelector.SelectNextCharacter();
            }
        }
    }
}