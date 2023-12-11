using EnhancedControls.KeyboardBindings;
using HarmonyLib;
using Kingmaker;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows;
using Kingmaker.Settings.Entities;
using System;
using UnityModManagerNet;

namespace EnhancedControls;

[EnableReloading]
static class Main
{
    internal static Harmony HarmonyInstance;
    internal static UnityModManager.ModEntry.ModLogger log;
    internal static KeyBindSettings Settings;

    static bool Load(UnityModManager.ModEntry modEntry)
    {
        Settings = UnityModManager.ModSettings.Load<KeyBindSettings>(modEntry);
        log = modEntry.Logger;
        HarmonyInstance = new Harmony(modEntry.Info.Id);

        // binding registration patch happens always
        HarmonyInstance.CreateClassProcessor(typeof(BindingsRegistration)).Patch();
        // most binds too
        HarmonyInstance.CreateClassProcessor(typeof(UsualBinds)).Patch();

        // if separate End Turn button is set, patch it too
        if (TryParseKeyBinding(Settings.SeparateEndTurn, out _))
        {
            HarmonyInstance.CreateClassProcessor(typeof(SeparateEndTurn.BindPatches)).Patch();
        }

        // loot pickup
        if (TryParseKeyBinding(Settings.CollectAllAndClose, out _))
        {
            HarmonyInstance.CreateClassProcessor(typeof(CollectAllAndClose.BindPatches)).Patch();
        }

        Settings.Save(modEntry);
        return true;
    }

    public static bool TryParseKeyBinding(string keyBinding, out KeyBindingData? keyBindingData)
    {
        try
        {
            keyBindingData = new KeyBindingData(keyBinding);
            return true;
        }
        catch (ArgumentException)
        {
            keyBindingData = null;
        }
        return false;
    }
}

[HarmonyPatch(typeof(Game), nameof(Game.Initialize))]
public static class BindingsRegistration
{
    [HarmonyPostfix]
    public static void Add()
    {
        NextTab.RegisterBinding();
        NextCharacter.RegisterBinding();
        InventorySearchField.RegisterBinding();
        HighlightToggle.RegisterBinding();
        SeparateEndTurn.RegisterBinding();
        CollectAllAndClose.RegisterBinding();
    }
}


[HarmonyPatch(typeof(ServiceWindowsVM), nameof(ServiceWindowsVM.BindKeys))]
public static class UsualBinds
{
    [HarmonyPostfix]
    public static void Add(ServiceWindowsVM __instance)
    {
        __instance.AddDisposable(NextTab.Bind());
        __instance.AddDisposable(NextCharacter.Bind());
        __instance.AddDisposable(InventorySearchField.Bind());
        __instance.AddDisposable(HighlightToggle.Bind());
    }
}