using Kingmaker;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows;
using Kingmaker.Controllers.MapObjects;
using Kingmaker.GameModes;
using Kingmaker.Settings.Entities;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.MVVM.View.ServiceWindows.Inventory.VisualSettings;
using System;

namespace EnhancedControls.KeyboardBindings;

internal class HighlightToggle
{
    internal static void Add()
    {
        var game = Game.Instance;
        try
        {
            var nextCharacterBind = new KeyBindingData(Main.Settings.HighlightToggle);

            game.Keyboard.RegisterBinding(
                "EnhancedControls.HighlightToggle",
                nextCharacterBind.Key,
                new GameModeType[] { GameModeType.Default, GameModeType.Pause },
                nextCharacterBind.IsCtrlDown,
                nextCharacterBind.IsAltDown,
                nextCharacterBind.IsShiftDown,
                Kingmaker.UI.InputSystems.Enums.TriggerType.KeyDown,
                KeyboardAccess.ModificationSide.Any,
                true);
            game.Keyboard.Bind("EnhancedControls.HighlightToggle", ActivateInventorySearchField);
        }
        catch (ArgumentException ex)
        {
            Main.log.Error($"Incorrect keybind format for NextCharacter action: {ex.Message}");
        }
    }

    internal static void ActivateInventorySearchField()
    {
        Main.log.Log("Object highlight toggled");
        InteractionHighlightController.Instance.SwitchHighlight();
    }
}
