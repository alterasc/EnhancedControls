using HarmonyLib;
using Kingmaker;
using Kingmaker.Code.UI.MVVM.View.Surface;
using Kingmaker.Code.UI.MVVM.View.SurfaceCombat.PC;
using Kingmaker.Controllers.MapObjects;
using Kingmaker.GameModes;
using Kingmaker.Settings.Entities;

namespace EnhancedControls.KeyboardBindings;

internal class HighlightToggle
{
    private const string BIND_NAME = "EnhancedControls.HighlightToggle";

    internal static void RegisterBinding(KeyBindingData keyData)
    {
        Game.Instance.Keyboard.RegisterBinding(
            BIND_NAME,
            keyData.Key,
            new GameModeType[] { GameModeType.Default, GameModeType.Pause },
            keyData.IsCtrlDown,
            keyData.IsAltDown,
            keyData.IsShiftDown);
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

        /**
         * After enabling binds action and restores highlight state to previously recorded
         */
        [HarmonyPostfix]
        [HarmonyPatch(typeof(InteractionHighlightController), nameof(InteractionHighlightController.OnEnable))]
        private static void AfterOnEnable(InteractionHighlightController __instance)
        {
            Game.Instance.Keyboard.Bind(BIND_NAME, ToggleHighlight);
            __instance.Highlight(_highlightState);
        }

        /**
         * Unbinds key on disabling
         */
        [HarmonyPostfix]
        [HarmonyPatch(typeof(InteractionHighlightController), nameof(InteractionHighlightController.OnDisable))]
        private static void AfterOnDisable(InteractionHighlightController __instance)
        {
            Game.Instance.Keyboard.Unbind(BIND_NAME, ToggleHighlight);
        }

        /**
         * Disables hightlight in cutscenes and dialogues
         */
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

        /**
         * Disables highlight on surface combat start, because in surface combat you can't perform actions
         * while highlighting is up
         */
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SurfaceBaseView), nameof(SurfaceBaseView.ActivateCombatInputLayer))]
        private static void DisableOnCombatStart()
        {
            InteractionHighlightController instance = InteractionHighlightController.Instance;
            if (instance == null) return;
            instance.Highlight(false);
        }

        /**
         * Restore highlight toggle state after combat end
         */
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SurfaceBaseView), nameof(SurfaceBaseView.DeactivateCombatInputLayer))]
        private static void RestoreStateOnCombatEnd()
        {
            InteractionHighlightController instance = InteractionHighlightController.Instance;
            if (instance == null) return;
            instance.Highlight(_highlightState);
        }
    }
}