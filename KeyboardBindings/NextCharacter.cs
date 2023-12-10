using Kingmaker;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.NameAndPortrait;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.Settings.Entities;
using Kingmaker.UI.Sound;
using System;
using System.Collections.Generic;

namespace EnhancedControls.KeyboardBindings;

internal static class NextCharacter
{
    private const string BIND_NAME = "EnhancedControls.NextCharacter";

    internal static void RegisterBinding()
    {
        try
        {
            var keyData = new KeyBindingData(Main.Settings.NextCharacter);
            Game.Instance.Keyboard.RegisterBinding(
                BIND_NAME,
                keyData.Key,
                new GameModeType[] { GameModeType.Default, GameModeType.Pause },
                keyData.IsCtrlDown,
                keyData.IsAltDown,
                keyData.IsShiftDown);
        }
        catch (ArgumentException ex)
        {
            Main.log.Error($"Incorrect keybind format for NextCharacter action: {ex.Message}");
        }
    }

    internal static IDisposable Bind()
    {
        if (!Main.TryParseKeyBinding(Main.Settings.NextCharacter, out _)) return null;

        return Game.Instance.Keyboard.Bind(BIND_NAME, delegate
            {
                var uiContext = Game.Instance.RootUiContext;
                var currentWindow = Game.Instance.RootUiContext.CurrentServiceWindow;

                if (currentWindow == ServiceWindowsType.CharacterInfo)
                {
                    var serviceWindowsVM = uiContext.SurfaceVM.StaticPartVM.ServiceWindowsVM;
                    var characterInfoVM = serviceWindowsVM.CharacterInfoVM.Value;
                    var nameAndPortraitVM = (CharInfoNameAndPortraitVM)characterInfoVM.ComponentVMs[CharInfoComponentType.NameAndPortrait].Value;
                    nameAndPortraitVM.SelectNextCharacter();
                }
                else if (currentWindow == ServiceWindowsType.Inventory)
                {
                    var serviceWindowsVM = uiContext.SurfaceVM.StaticPartVM.ServiceWindowsVM;
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
            });
    }
}