using Kingmaker;
using Kingmaker.Controllers.MapObjects;
using Kingmaker.GameModes;
using Kingmaker.Settings.Entities;
using System;

namespace EnhancedControls.KeyboardBindings;

internal class HighlightToggle
{
    private const string BIND_NAME = "EnhancedControls.HighlightToggle";

    internal static void RegisterBinding()
    {
        try
        {
            var keyData = new KeyBindingData(Main.Settings.HighlightToggle);

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
            Main.log.Error($"Incorrect keybind format for HighlightToggle action: {ex.Message}");
        }
    }
    internal static IDisposable Bind()
    {
        if (!Main.TryParseKeyBinding(Main.Settings.HighlightToggle, out _)) return null;
        return Game.Instance.Keyboard.Bind(BIND_NAME, ActivateInventorySearchField);
    }

    internal static void ActivateInventorySearchField()
    {
        if (Game.Instance.Player.IsInCombat) return;
        InteractionHighlightController.Instance.SwitchHighlight();
    }
}
