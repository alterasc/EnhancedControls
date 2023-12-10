using EnhancedControls.KeyboardBindings;
using HarmonyLib;
using Kingmaker;
using Kingmaker.Code.UI.MVVM.View.SurfaceCombat.PC;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows;
using Kingmaker.Settings.Entities;
using Kingmaker.UI.MVVM.View.SpaceCombat.PC;
using System;
using System.Collections.Generic;
using System.Linq;
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
        HarmonyInstance.CreateClassProcessor(typeof(Binds)).Patch();

        // if separate End Turn button is set, patch it too
        if (TryParseKeyBinding(Settings.SeparateEndTurn, out _))
        {
            HarmonyInstance.CreateClassProcessor(typeof(SurfaceCombatEndTurnButton)).Patch();
            HarmonyInstance.CreateClassProcessor(typeof(SpaceCombatEndTurnButton)).Patch();
            HarmonyInstance.CreateClassProcessor(typeof(RemoveEndTurnFromPauseAction)).Patch();
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
    }
}


[HarmonyPatch(typeof(ServiceWindowsVM), nameof(ServiceWindowsVM.BindKeys))]
public static class Binds
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

[HarmonyPatch(typeof(SurfaceHUDPCView), nameof(SurfaceHUDPCView.BindViewImplementation))]
public static class SurfaceCombatEndTurnButton
{
    [HarmonyPostfix]
    public static void Add(SurfaceHUDPCView __instance)
    {
        __instance.AddDisposable(SeparateEndTurn.Bind());

    }
}

[HarmonyPatch(typeof(SpaceCombatServicePanelPCView), nameof(SpaceCombatServicePanelPCView.BindViewImplementation))]
public static class SpaceCombatEndTurnButton
{
    [HarmonyPostfix]
    public static void Add(SpaceCombatServicePanelPCView __instance)
    {
        __instance.AddDisposable(SeparateEndTurn.Bind());
    }
}


[HarmonyPatch(typeof(Game), nameof(Game.PauseAndTryEndTurnBind))]
public static class RemoveEndTurnFromPauseAction
{
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var original = new List<CodeInstruction>(instructions);
        var newInstructions = instructions;
        var endTurnBindMethod = AccessTools.Method(typeof(Game), nameof(Game.EndTurnBind));
        var endTurnCall = original.FindIndex(x => x.Calls(endTurnBindMethod));
        if (endTurnCall != -1)
        {
            //we take all instructions except the one that calls EndTurnBind
            //and previous one which loads instance on stack as argument
            newInstructions = original.Where((x, idx) => idx != endTurnCall && idx != endTurnCall - 1);
        }
        return newInstructions;
    }
}