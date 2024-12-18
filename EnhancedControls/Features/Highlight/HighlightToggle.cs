using EnhancedControls.Settings;
using HarmonyLib;
using Kingmaker.Code.UI.MVVM.View.Space;
using Kingmaker.Code.UI.MVVM.View.Surface;
using Kingmaker.Code.UI.MVVM.View.SurfaceCombat.PC;
using Kingmaker.Controllers.MapObjects;
using Kingmaker.GameModes;
using Kingmaker.View.Mechanics.Entities;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace EnhancedControls.Features.Highlight;

public class HighlightToggle : ModHotkeySettingEntry
{
    private const string _key = "highlighttoggle";
    private const string _title = "Toggle partial object highlight";
    private const string _tooltip = "Toggles partial highlighting of map objects and bodies with loot. To " +
        "highlight the rest, press normal highlight key.";
    private const string _defaultValue = "%R;;World;false";
    internal const string BIND_NAME = $"{PREFIX}.newcontrols.ui.{_key}";

    public HighlightToggle() : base(_key, _title, _tooltip, _defaultValue) { }

    [HarmonyPatch]
    private static class Patches
    {
        /// <summary>
        /// After enabling InteractionHighlightController binds action 
        /// and restores highlight state to previously recorded
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(InteractionHighlightController), nameof(InteractionHighlightController.OnEnable))]
        private static bool AfterOnEnable(InteractionHighlightController __instance)
        {
            HighlightManager.OnEnableController(__instance);
            return false;
        }

        /// <summary>
        /// Unbinds key on disabling InteractionHighlightController
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(InteractionHighlightController), nameof(InteractionHighlightController.OnDisable))]
        private static bool OnDisableReplacer(InteractionHighlightController __instance)
        {
            HighlightManager.OnDisableController(__instance);
            return false;
        }

        /// <summary>
        /// Disables hightlight in cutscenes and dialogues
        /// </summary>
        /// <param name="gameMode"></param>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SurfaceHUDPCView), nameof(SurfaceHUDPCView.OnGameModeStart))]
        private static void DisableOnDialogAndCutscene(GameModeType gameMode)
        {
            if (gameMode == GameModeType.Cutscene || gameMode == GameModeType.Dialog || gameMode == GameModeType.CutsceneGlobalMap)
            {
                HighlightManager.SuppressPassiveHighlight();
            }
            else
            {
                HighlightManager.RestorePassiveHighlight();
            }
        }

        /// <summary>
        /// Disables highlight on surface combat start, because in surface combat you can't perform actions
        /// while highlighting is up
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SurfaceBaseView), nameof(SurfaceBaseView.ActivateCombatInputLayer))]
        private static void DisableOnCombatStart()
        {
            HighlightManager.SuppressPassiveHighlight();
        }

        /// <summary>
        /// Restores highlight toggle state after surface combat end
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SurfaceBaseView), nameof(SurfaceBaseView.DeactivateCombatInputLayer))]
        private static void RestoreStateOnCombatEnd()
        {
            HighlightManager.RestorePassiveHighlight();
        }

        /// <summary>
        /// Disables highlight on space combat start, because in space combat you can't perform actions
        /// while highlighting is up
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SpaceBaseView), nameof(SpaceBaseView.ActivateCombatInputLayer))]
        private static void DisableOnSpaceCombatStart()
        {
            HighlightManager.SuppressPassiveHighlight();
        }

        /// <summary>
        /// Restores highlight on space combat end
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SpaceBaseView), nameof(SpaceBaseView.DeactivateCombatInputLayer))]
        private static void RestoreOnSpaceCombatStart()
        {
            HighlightManager.RestorePassiveHighlight();
        }

        /// <summary>
        /// Replaces normal check for highlight status with check that accounts
        /// for basic/full level of highlight and unit status
        /// </summary>
        /// <param name="instructions"></param>
        /// <returns></returns>
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(AbstractUnitEntityView), nameof(AbstractUnitEntityView.UpdateHighlight))]
        private static IEnumerable<CodeInstruction> ReplaceHighlightCheckForUnit(IEnumerable<CodeInstruction> instructions)
        {
            var highlightGetter = AccessTools.Method(typeof(InteractionHighlightController), "get_IsHighlighting");
            foreach (var instruction in instructions)
            {
                if (instruction.Calls(highlightGetter))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return CodeInstruction.Call(typeof(HighlightManager), nameof(HighlightManager.UnitHighlight));
                }
                else
                {
                    yield return instruction;
                }
            }
        }
    }
}