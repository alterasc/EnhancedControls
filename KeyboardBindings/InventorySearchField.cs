using Kingmaker;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows;
using Kingmaker.GameModes;
using Kingmaker.Settings.Entities;
using Kingmaker.UI.MVVM.View.ServiceWindows.Inventory.VisualSettings;
using System;

namespace EnhancedControls.KeyboardBindings;

internal class InventorySearchField
{
    internal const string BIND_NAME = "EnhancedControls.InventorySearch";
    internal static void RegisterBinding()
    {
        try
        {
            var keyData = new KeyBindingData(Main.Settings.InventorySearch);

            Game.Instance.Keyboard.RegisterBinding(
                BIND_NAME,
                keyData.Key,
                new GameModeType[] { GameModeType.Default, GameModeType.Pause },
                keyData.IsCtrlDown,
                keyData.IsAltDown,
                keyData.IsShiftDown);
        }
        catch (ArgumentException ex)
        {
            Main.log.Error($"Incorrect keybind format for InventorySearch action: {ex.Message}");
        }
    }
    internal static IDisposable Bind()
    {
        if (!Main.TryParseKeyBinding(Main.Settings.InventorySearch, out _)) return null;
        return Game.Instance.Keyboard.Bind(BIND_NAME, ActivateInventorySearchField);
    }

    internal static void ActivateInventorySearchField()
    {
        var currentWindow = Game.Instance.RootUiContext.CurrentServiceWindow;
        if (currentWindow == ServiceWindowsType.Inventory)
        {
            var commonInventory = RootUIContext.Instance.m_UIView.transform.Find("SurfaceStaticPartPCView/StaticCanvas/ServiceWindowsPCView/InventoryPCView/InventoryRightCanvas/Background/CommonInventory/");
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
