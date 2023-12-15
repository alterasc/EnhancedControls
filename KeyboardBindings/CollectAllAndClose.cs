using HarmonyLib;
using Kingmaker;
using Kingmaker.Code.UI.MVVM.View.Loot.PC;
using Kingmaker.Code.UI.MVVM.VM.Loot;
using Kingmaker.GameModes;
using Kingmaker.Settings.Entities;
using System;

namespace EnhancedControls.KeyboardBindings;

internal static class CollectAllAndClose
{
    private const string BIND_NAME = "EnhancedControls.CollectAllAndClose";

    internal static void RegisterBinding(KeyBindingData keyBindingData)
    {
        Game.Instance.Keyboard.RegisterBinding(
                   BIND_NAME,
                   keyBindingData.Key,
                   new GameModeType[] { GameModeType.Default, GameModeType.Pause },
                   keyBindingData.IsCtrlDown,
                   keyBindingData.IsAltDown,
                   keyBindingData.IsShiftDown);

    }

    internal static IDisposable Bind()
    {
        return Game.Instance.Keyboard.Bind(BIND_NAME, delegate
        {
            LootCollectorVM lootCollectorVM = Game.Instance.RootUiContext.SurfaceVM?.StaticPartVM?.LootContextVM?.LootVM?.Value.LootCollector;
            if (lootCollectorVM != null)
            {
                lootCollectorVM.CollectAll();
                if (lootCollectorVM.Loot.ExtendedView.Value)
                {
                    lootCollectorVM.Close();
                }
            }
        });
    }

    [HarmonyPatch]
    internal static class Patches
    {
        [HarmonyPatch(typeof(LootCollectorPCView), nameof(LootCollectorPCView.BindViewImplementation))]
        [HarmonyPostfix]
        internal static void Add(LootCollectorPCView __instance)
        {
            __instance.AddDisposable(Bind());
        }
    }
}