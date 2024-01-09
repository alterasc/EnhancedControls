using EnhancedControls.Settings;
using HarmonyLib;
using Kingmaker;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows;
using Kingmaker.UI.MVVM.View.ServiceWindows.Inventory.VisualSettings;

namespace EnhancedControls.Features;

internal class InventorySearchField : ModHotkeySettingEntry
{
    private const string _key = "inventorysearch";
    private const string _title = "Search Inventory";
    private const string _tooltip = "Activates search input field when you're in inventory menu.";
    private const string _defaultValue = "%F;;WorldFullscreenUI;false";
    private const string BIND_NAME = $"{PREFIX}.newcontrols.ui.{_key}";

    public InventorySearchField() : base(_key, _title, _tooltip, _defaultValue) { }

    [HarmonyPatch]
    private static class Patches
    {
        /// <summary>
        /// Binds key after other service window keys are bound
        /// </summary>
        [HarmonyPatch(typeof(ServiceWindowsVM), nameof(ServiceWindowsVM.BindKeys))]
        [HarmonyPostfix]
        private static void Add(ServiceWindowsVM __instance)
        {
            __instance.AddDisposable(Game.Instance.Keyboard.Bind(BIND_NAME, ActivateInventorySearchField));
        }

        /// <summary>
        /// Tries to find inventory search field TMP and activate it
        /// </summary>
        private static void ActivateInventorySearchField()
        {
            var currentWindow = Game.Instance.RootUiContext.CurrentServiceWindow;
            if (currentWindow == ServiceWindowsType.Inventory)
            {
                var uiContext = Game.Instance.RootUiContext;
                var path = uiContext.IsSpace
                    ? "SpaceStaticPartPCView/StaticCanvas/ServiceWindowsPCView/InventoryPCView/InventoryRightCanvas/Background/CommonInventory/"
                    : "SurfaceStaticPartPCView/StaticCanvas/ServiceWindowsPCView/InventoryPCView/InventoryRightCanvas/Background/CommonInventory/";
                var commonInventory = RootUIContext.Instance.m_UIView.transform.Find(path);
                if (commonInventory != null)
                {
                    var stash = commonInventory.gameObject.GetComponent<InventoryStashPCView>();
                    var searchView = (ItemsFilterSearchPCView)stash?.m_ItemsFilter?.m_SearchView;
                    if (searchView != null)
                    {
                        var inputField = searchView.m_InputField;
                        inputField.ActivateInputField();
                        inputField.Select();
                    }
                }
            }
        }
    }
}
