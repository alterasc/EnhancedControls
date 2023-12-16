using HarmonyLib;
using Kingmaker;
using Kingmaker.Code.UI.MVVM.View.Loot.PC;
using Kingmaker.Code.UI.MVVM.VM.Loot;
using Kingmaker.Settings.Entities;
using Kingmaker.UI.InputSystems.Enums;
using System;

namespace EnhancedControls.KeyboardBindings;

internal static class CollectAllAndClose
{
    private const string BIND_NAME = "EnhancedControls.CollectAllAndClose";

    internal static void RegisterBinding(KeyBindingData keyData)
    {
        Game.Instance.Keyboard.RegisterBinding(
            BIND_NAME,
            keyData,
            GameModesGroup.World,
            false);
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
        /// <summary>
        /// Binds button on creation of loot collector view
        /// </summary>
        [HarmonyPatch(typeof(LootCollectorPCView), nameof(LootCollectorPCView.BindViewImplementation))]
        [HarmonyPostfix]
        internal static void Add(LootCollectorPCView __instance)
        {
            __instance.AddDisposable(Bind());
        }
    }
}