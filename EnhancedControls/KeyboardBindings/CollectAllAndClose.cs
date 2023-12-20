using EnhancedControls.Settings;
using HarmonyLib;
using Kingmaker;
using Kingmaker.Code.UI.MVVM.View.Loot.PC;
using Kingmaker.Code.UI.MVVM.VM.Loot;

namespace EnhancedControls.KeyboardBindings;

public class CollectAllAndClose : ModHotkeySettingEntry
{
    private const string _key = "collectallandclose";
    private const string _title = "Collect All and Close";
    private const string _tooltip = "Collects all loot and closes loot window";
    private const string _defaultValue = "F;;WorldFullscreenUI;false";
    private const string BIND_NAME = $"{PREFIX}.newcontrols.ui.{_key}";

    public CollectAllAndClose() : base(_key, _title, _tooltip, _defaultValue) { }

    public override SettingStatus TryEnable() => TryEnableAndPatch(typeof(Patches));

    [HarmonyPatch]
    private static class Patches
    {
        /// <summary>
        /// Binds button on creation of loot collector view
        /// </summary>
        [HarmonyPatch(typeof(LootCollectorPCView), nameof(LootCollectorPCView.BindViewImplementation))]
        [HarmonyPostfix]
        private static void Add(LootCollectorPCView __instance)
        {
            __instance.AddDisposable(Game.Instance.Keyboard.Bind(BIND_NAME, CollectAndClose));
        }

        private static void CollectAndClose()
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
        }
    }
}