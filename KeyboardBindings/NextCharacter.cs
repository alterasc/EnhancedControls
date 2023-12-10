using Kingmaker;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.NameAndPortrait;
using Kingmaker.GameModes;
using Kingmaker.Settings.Entities;
using Kingmaker.UI.InputSystems;
using System;

namespace EnhancedControls.KeyboardBindings;

internal static class NextCharacter
{
    internal static void Add()
    {
        var game = Game.Instance;
        try
        {
            var nextCharacterBind = new KeyBindingData(Main.Settings.NextCharacter);

            game.Keyboard.RegisterBinding(
                "EnhancedControls.NextCharacter",
                nextCharacterBind.Key,
                new GameModeType[] { GameModeType.Default, GameModeType.Pause },
                nextCharacterBind.IsCtrlDown,
                nextCharacterBind.IsAltDown,
                nextCharacterBind.IsShiftDown,
                Kingmaker.UI.InputSystems.Enums.TriggerType.KeyDown,
                KeyboardAccess.ModificationSide.Any,
                true);
            game.Keyboard.Bind("EnhancedControls.NextCharacter", delegate
            {
                var uiContext = game.RootUiContext;
                var currentWindow = game.RootUiContext.CurrentServiceWindow;
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
            });
        }
        catch (ArgumentException ex)
        {
            Main.log.Error($"Incorrect keybind format for NextCharacter action: {ex.Message}");
        }
    }
}