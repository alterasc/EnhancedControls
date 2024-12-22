using EnhancedControls.Settings;
using HarmonyLib;
using Kingmaker;
using Kingmaker.Controllers.MapObjects;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.View.Mechanics.Entities;
using System;
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

    public override SettingStatus TryEnable()
    {
        var baseStatus = base.TryEnable();
        if (baseStatus == SettingStatus.WORKING)
        {
            highlightGameModHandler ??= EventBus.Subscribe(new HighlightGameModeHandler());
        }
        return baseStatus;
    }

    private static IDisposable highlightGameModHandler;

    /// <summary>
    /// Disables hightlight in cutscenes and dialogues
    /// </summary>
    /// <param name="gameMode"></param>
    public class HighlightGameModeHandler : IGameModeHandler
    {
        public void OnGameModeStart(GameModeType gameMode)
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

        public void OnGameModeStop(GameModeType gameMode) { }
    }

    /// <summary>
    /// Patches of InteractionHighlightController
    /// </summary>
    [HarmonyPatch(typeof(InteractionHighlightController))]
    private static class HighlightControllerPatches
    {
        /// <summary>
        /// Does usual stuff, binds additional button and 
        /// restores passive highlight if necessary
        /// <param name="__instance"></param>
        [HarmonyPrefix]
        [HarmonyPatch(nameof(InteractionHighlightController.OnEnable))]
        private static bool OnEnableReplacer(InteractionHighlightController __instance)
        {
            HighlightManager.OnEnableController(__instance);
            return false;
        }

        /// <summary>
        /// Unbinds key on disabling InteractionHighlightController
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(nameof(InteractionHighlightController.OnDisable))]
        private static bool OnDisableReplacer(InteractionHighlightController __instance)
        {
            HighlightManager.OnDisableController(__instance);
            return false;
        }
    }

    [HarmonyPatch]
    private static class Patches
    {
        /// <summary>
        /// Suppresses highlight on combat start, restores on combat end
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Player), nameof(Player.IsInCombat), MethodType.Setter)]
        private static void BeforeIsInCombatSet(Player __instance, bool value)
        {
            if (__instance.IsInCombat != value)
            {
                if (value)
                {
                    HighlightManager.SuppressPassiveHighlight();
                }
                else
                {
                    HighlightManager.RestorePassiveHighlight();
                }
            }
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
            var highlightGetter = AccessTools.PropertyGetter(typeof(InteractionHighlightController), nameof(InteractionHighlightController.IsHighlighting));
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