using EnhancedControls.Settings;
using HarmonyLib;
using Kingmaker;
using Kingmaker.Code.UI.MVVM.View.SurfaceCombat.PC;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.View.SpaceCombat.PC;
using System.Collections.Generic;

namespace EnhancedControls.Features;

public class SeparateEndTurn : ModHotkeySettingEntry
{
    private const string _key = "separateendturn";
    private const string _title = "Separate End Turn";
    private const string _tooltip = "If this button is bound, then it removes End Turn functionality from Pause button (Space) and assigns this to this button";
    private const string _defaultValue = "%Space;;World;false";
    private const string BIND_NAME = $"{PREFIX}.newcontrols.ui.{_key}";

    public SeparateEndTurn() : base(_key, _title, _tooltip, _defaultValue) { }

    public override SettingStatus TryEnable() => TryEnableAndPatch(typeof(Patches));

    [HarmonyPatch]
    private static class Patches
    {
        /// <summary>
        /// Binds new end turn key on start of surface combat
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPatch(typeof(SurfaceHUDPCView), nameof(SurfaceHUDPCView.BindViewImplementation))]
        [HarmonyPostfix]
        private static void SurfaceCombatBind(SurfaceHUDPCView __instance)
        {
            __instance.AddDisposable(Game.Instance.Keyboard.Bind(BIND_NAME, EndTurn));
        }

        /// <summary>
        /// Binds new end turn key on start of space combat
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPatch(typeof(SpaceCombatServicePanelPCView), nameof(SpaceCombatServicePanelPCView.BindViewImplementation))]
        [HarmonyPostfix]
        private static void SpaceCombatBind(SpaceCombatServicePanelPCView __instance)
        {
            __instance.AddDisposable(Game.Instance.Keyboard.Bind(BIND_NAME, EndTurn));
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

        private static void EndTurn()
        {
            if (!UIUtility.IsGlobalMap())
            {
                Game.Instance.EndTurnBind();
            }
        }
    }
}