using EnhancedControls.KeyboardBindings;
using EnhancedControls.UI;
using Kingmaker.Settings.Entities;
using System;

namespace EnhancedControls;

internal static class KeybindingPatchManager
{
    private static bool Initialized = false;
    internal static void Run()
    {
        if (Initialized) return;
        Initialized = true;

        var settings = ModSettings.Instance;
        {
            var highlightToggle = settings.HighlightToggle.GetValue().Binding1;
            TryRegister(highlightToggle, "HighlightToggle", () =>
            {
                HighlightToggle.RegisterBinding(highlightToggle);
                Main.HarmonyInstance.CreateClassProcessor(typeof(HighlightToggle.Patches)).Patch();
            });
        }

        {
            var separateEndTurn = settings.SeparateEndTurn.GetValue().Binding1;
            TryRegister(separateEndTurn, "SeparateEndTurn", () =>
            {
                SeparateEndTurn.RegisterBinding(separateEndTurn);
                Main.HarmonyInstance.CreateClassProcessor(typeof(SeparateEndTurn.Patches)).Patch();
            });
        }

        {
            var collectAll = settings.CollectAll.GetValue().Binding1;
            TryRegister(collectAll, "CollectAllAndClose", () =>
            {
                CollectAllAndClose.RegisterBinding(collectAll);
                Main.HarmonyInstance.CreateClassProcessor(typeof(CollectAllAndClose.Patches)).Patch();
            });
        }

        {
            var nextCharacter = settings.NextCharacter.GetValue().Binding1;
            TryRegister(nextCharacter, "NextCharacter", () =>
            {
                NextCharacter.RegisterBinding(nextCharacter);
                Main.HarmonyInstance.CreateClassProcessor(typeof(NextCharacter.Patches)).Patch();
            });
        }

        {
            var prevCharacter = settings.PrevCharacter.GetValue().Binding1;
            TryRegister(prevCharacter, "PrevCharacter", () =>
            {
                PrevCharacter.RegisterBinding(prevCharacter);
                Main.HarmonyInstance.CreateClassProcessor(typeof(PrevCharacter.Patches)).Patch();
            });
        }

        {
            var nextTab = settings.NextTab.GetValue().Binding1;
            TryRegister(nextTab, "NextTab", () =>
            {
                NextTab.RegisterBinding(nextTab);
                Main.HarmonyInstance.CreateClassProcessor(typeof(NextTab.Patches)).Patch();
            });
        }

        {
            var inventorySearch = settings.InventorySearch.GetValue().Binding1;
            TryRegister(inventorySearch, "InventorySearch", () =>
            {
                InventorySearchField.RegisterBinding(inventorySearch);
                Main.HarmonyInstance.CreateClassProcessor(typeof(InventorySearchField.Patches)).Patch();
            });
        }
    }

    private static void TryRegister(KeyBindingData binding, string name, Action action)
    {
        Main.log.Log($"{name} binding1: {binding}");
        if (!binding.Equals(default))
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Main.log.Error($"{name} registration exception: {ex.Message}");
            }
        }
        else
        {
            Main.log.Log($"{name} unbound, registration skipped");
        }
    }
}
