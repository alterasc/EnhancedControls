using Kingmaker;
using Kingmaker.GameModes;
using Kingmaker.Settings.Entities;
using System;

namespace EnhancedControls.KeyboardBindings;

internal static class SeparateEndTurn
{
    private const string BIND_NAME = "EnhancedControls.SeparateEndTurn";

    internal static void RegisterBinding()
    {
        try
        {
            var nextTabBind = new KeyBindingData(Main.Settings.SeparateEndTurn);
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
            Main.log.Error($"Incorrect keybind format for SeparateEndTurn action: {ex.Message}");
        }
    }

    internal static IDisposable Bind()
    {
        if (!Main.TryParseKeyBinding(Main.Settings.SeparateEndTurn, out _)) return null;

        return Game.Instance.Keyboard.Bind(BIND_NAME, Game.Instance.EndTurnBind);
    }
}