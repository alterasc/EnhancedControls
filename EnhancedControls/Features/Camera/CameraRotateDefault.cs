using EnhancedControls.Settings;
using HarmonyLib;
using Kingmaker;
using Kingmaker.View;

namespace EnhancedControls.Features.Camera;

internal class CameraRotateDefault : ModHotkeySettingEntry
{
    private const string _key = "camerarotatedefault";
    private const string _title = "Rotate camera to point North";
    private const string _tooltip = "Rotates camera to point North";
    private const string _defaultValue = ";;CameraControls;false";
    private const string BIND_NAME = $"{PREFIX}.newcontrols.ui.{_key}";

    public CameraRotateDefault() : base(_key, _title, _tooltip, _defaultValue) { }

    public override SettingStatus TryEnable() => TryEnableAndPatch(typeof(Patches));

    [HarmonyPatch]
    private static class Patches
    {
        /// <summary>
        /// Binds button on binding of other camera buttons
        /// </summary>
        [HarmonyPatch(typeof(CameraRig), nameof(CameraRig.ApplyBindAction))]
        [HarmonyPostfix]
        private static void Add()
        {
            Game.Instance.Keyboard.Bind(BIND_NAME, ResetCamera);
        }
        /// <summary>
        /// Reset camera rotation offset
        /// </summary>
        private static void ResetCamera()
        {
            CameraRig.Instance.RotateToTimed(90f, 0.4f);
        }
    }
}
