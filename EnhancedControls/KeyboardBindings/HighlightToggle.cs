using HarmonyLib;
using Kingmaker;
using Kingmaker.Code.UI.MVVM.View.Space;
using Kingmaker.Code.UI.MVVM.View.Surface;
using Kingmaker.Code.UI.MVVM.View.SurfaceCombat.PC;
using Kingmaker.Controllers.MapObjects;
using Kingmaker.GameModes;
using Kingmaker.Settings.Entities;
using Kingmaker.UI.InputSystems.Enums;

namespace EnhancedControls.KeyboardBindings;

internal class HighlightToggle
{
    private const string BIND_NAME = "EnhancedControls.HighlightToggle";

    internal static void RegisterBinding(KeyBindingData keyData)
    {
        Game.Instance.Keyboard.RegisterBinding(
            BIND_NAME,
            keyData,
            GameModesGroup.World,
            false);
    }

    [HarmonyPatch]
    internal static class Patches
    {
        private static bool _highlightState = false;

        private static void ToggleHighlight()
        {
            if (Game.Instance.Player.IsInCombat) return;
            _highlightState = !InteractionHighlightController.Instance.m_IsHighlighting;
            InteractionHighlightController.Instance.Highlight(_highlightState);
        }

        /// <summary>
        /// After enabling InteractionHighlightController binds action 
        /// and restores highlight state to previously recorded
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(InteractionHighlightController), nameof(InteractionHighlightController.OnEnable))]
        private static void AfterOnEnable(InteractionHighlightController __instance)
        {
            Game.Instance.Keyboard.Bind(BIND_NAME, ToggleHighlight);
            __instance.Highlight(_highlightState);
        }

        /// <summary>
        /// Unbinds key on disabling InteractionHighlightController
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(InteractionHighlightController), nameof(InteractionHighlightController.OnDisable))]
        private static void AfterOnDisable()
        {
            Game.Instance.Keyboard.Unbind(BIND_NAME, ToggleHighlight);
        }

        /// <summary>
        /// Disables hightlight in cutscenes and dialogues
        /// </summary>
        /// <param name="gameMode"></param>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SurfaceHUDPCView), nameof(SurfaceHUDPCView.OnGameModeStart))]
        private static void DisableOnDialogAndCutscene(GameModeType gameMode)
        {
            if (gameMode == GameModeType.Cutscene || gameMode == GameModeType.Dialog)
            {
                InteractionHighlightController instance = InteractionHighlightController.Instance;
                if (instance == null)
                {
                    return;
                }
                instance.Highlight(false);
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
            InteractionHighlightController instance = InteractionHighlightController.Instance;
            if (instance == null) return;
            instance.Highlight(false);
        }

        /// <summary>
        /// Restore highlight toggle state after surface combat end
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SurfaceBaseView), nameof(SurfaceBaseView.DeactivateCombatInputLayer))]
        private static void RestoreStateOnCombatEnd()
        {
            InteractionHighlightController instance = InteractionHighlightController.Instance;
            if (instance == null) return;
            instance.Highlight(_highlightState);
        }

        /// <summary>
        /// Disables highlight on space combat start, because in space combat you can't perform actions
        /// while highlighting is up
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SpaceBaseView), nameof(SpaceBaseView.ActivateCombatInputLayer))]
        private static void DisableOnSpaceCombatStart()
        {
            InteractionHighlightController instance = InteractionHighlightController.Instance;
            if (instance == null) return;
            instance.Highlight(false);
        }
    }
}