using Kingmaker;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows;
using Kingmaker.GameModes;
using Kingmaker.Settings.Entities;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.MVVM.View.ServiceWindows.Inventory.VisualSettings;
using System;

namespace EnhancedControls.KeyboardBindings;

internal class InventorySearchField
{
    internal static void Add()
    {
        var game = Game.Instance;
        try
        {
            var nextCharacterBind = new KeyBindingData(Main.Settings.InventorySearch);

            game.Keyboard.RegisterBinding(
                "EnhancedControls.InventorySearch",
                nextCharacterBind.Key,
                new GameModeType[] { GameModeType.Default, GameModeType.Pause },
                nextCharacterBind.IsCtrlDown,
                nextCharacterBind.IsAltDown,
                nextCharacterBind.IsShiftDown,
                Kingmaker.UI.InputSystems.Enums.TriggerType.KeyDown,
                KeyboardAccess.ModificationSide.Any,
                true);
            game.Keyboard.Bind("EnhancedControls.InventorySearch", ActivateInventorySearchField);
        }
        catch (ArgumentException ex)
        {
            Main.log.Error($"Incorrect keybind format for NextCharacter action: {ex.Message}");
        }
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
