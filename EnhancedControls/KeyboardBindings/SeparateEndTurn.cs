using HarmonyLib;
using Kingmaker;
using Kingmaker.Code.UI.MVVM.View.SurfaceCombat.PC;
using Kingmaker.GameModes;
using Kingmaker.Settings.Entities;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.View.SpaceCombat.PC;
using System;
using System.Collections.Generic;

namespace EnhancedControls.KeyboardBindings;

internal static class SeparateEndTurn
{
    private const string BIND_NAME = "EnhancedControls.SeparateEndTurn";

    internal static void RegisterBinding(KeyBindingData nextTabBind)
    {
        Game.Instance.Keyboard.RegisterBinding(
                   BIND_NAME,
                   nextTabBind.Key,
                   new GameModeType[] { GameModeType.Default, GameModeType.Pause },
                   nextTabBind.IsCtrlDown,
                   nextTabBind.IsAltDown,
                   nextTabBind.IsShiftDown);
    }

    internal static IDisposable Bind()
    {
        return Game.Instance.Keyboard.Bind(BIND_NAME, delegate
        {
            if (!UIUtility.IsGlobalMap())
            {
                Game.Instance.EndTurnBind();
            }
        });
    }

    [HarmonyPatch]
    internal static class Patches
    {
        [HarmonyPatch(typeof(SurfaceHUDPCView), nameof(SurfaceHUDPCView.BindViewImplementation))]
        [HarmonyPostfix]
        internal static void SurfaceCombatBind(SurfaceHUDPCView __instance)
        {
            __instance.AddDisposable(Bind());
        }

        [HarmonyPatch(typeof(SpaceCombatServicePanelPCView), nameof(SpaceCombatServicePanelPCView.BindViewImplementation))]
        [HarmonyPostfix]
        public static void SpaceCombatBind(SpaceCombatServicePanelPCView __instance)
        {
            __instance.AddDisposable(Bind());
        }


        [HarmonyPatch(typeof(Game), nameof(Game.PauseAndTryEndTurnBind))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> RemoveEndTurnFromPauseButton(IEnumerable<CodeInstruction> instructions)
        {
            var original = new List<CodeInstruction>(instructions);
            var newInstructions = instructions;
            var endTurnBindMethod = AccessTools.Method(typeof(Game), nameof(Game.EndTurnBind));
            var endTurnCall = original.FindIndex(x => x.Calls(endTurnBindMethod));
            if (endTurnCall != -1)
            {
                //we take all instructions except the one that calls EndTurnBind
                //and previous one which loads instance on stack as argument
                original.RemoveRange(endTurnCall - 1, 2);
                return original;
            }
            return newInstructions;
        }
    }
}