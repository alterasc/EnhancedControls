using HarmonyLib;
using Kingmaker;
using Kingmaker.Code.UI.MVVM.View.SurfaceCombat.PC;
using Kingmaker.Settings.Entities;
using Kingmaker.UI.Common;
using Kingmaker.UI.InputSystems.Enums;
using Kingmaker.UI.MVVM.View.SpaceCombat.PC;
using System;
using System.Collections.Generic;

namespace EnhancedControls.KeyboardBindings;

internal static class SeparateEndTurn
{
    private const string BIND_NAME = "EnhancedControls.SeparateEndTurn";

    internal static void RegisterBinding(KeyBindingData keyBindingData)
    {
        Game.Instance.Keyboard.RegisterBinding(
            BIND_NAME,
            keyBindingData,
            GameModesGroup.World,
            false);
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
        /// <summary>
        /// Binds new end turn key on start of surface combat
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPatch(typeof(SurfaceHUDPCView), nameof(SurfaceHUDPCView.BindViewImplementation))]
        [HarmonyPostfix]
        internal static void SurfaceCombatBind(SurfaceHUDPCView __instance)
        {
            __instance.AddDisposable(Bind());
        }

        /// <summary>
        /// Binds new end turn key on start of space combat
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPatch(typeof(SpaceCombatServicePanelPCView), nameof(SpaceCombatServicePanelPCView.BindViewImplementation))]
        [HarmonyPostfix]
        public static void SpaceCombatBind(SpaceCombatServicePanelPCView __instance)
        {
            __instance.AddDisposable(Bind());
        }

        /// <summary>
        /// Removes call to EndTurnBind from PauseAndTryEndTurnBind
        /// preventing Pause button from ending turn
        /// </summary>
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