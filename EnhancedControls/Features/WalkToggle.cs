using EnhancedControls.Settings;
using HarmonyLib;
using Kingmaker;
using Kingmaker.Controllers.MapObjects;
using Kingmaker.Controllers.Rest;

namespace EnhancedControls.Features;

public class WalkToggle : ModHotkeySettingEntry
{
    private const string _key = "walktoggle";
    private const string _title = "Toggle Walk/Run/Sprint";
    private const string _tooltip = "Toggles movement animation between normal behavior(distance-based)/always walk/always run/always sprint.";
    private const string _defaultValue = "%Y;;World;false";
    private const string BIND_NAME = $"{PREFIX}.newcontrols.ui.{_key}";

    public WalkToggle() : base(_key, _title, _tooltip, _defaultValue) { }

    [HarmonyPatch]
    private static class Patches
    {
        /// <summary>
        /// After enabling CameraController binds action
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CameraController), nameof(CameraController.OnEnable))]
        private static void AfterOnEnable(InteractionHighlightController __instance)
        {
            MovementManager.IsToggleEnabled = true;
            Game.Instance.Keyboard.Bind(BIND_NAME, MovementManager.ToggleWalk);
        }

        /// <summary>
        /// Unbinds key on disabling CameraController
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CameraController), nameof(CameraController.OnDisable))]
        private static void AfterOnDisable()
        {
            MovementManager.IsToggleEnabled = false;
            Game.Instance.Keyboard.Unbind(BIND_NAME, MovementManager.ToggleWalk);
        }
    }
}