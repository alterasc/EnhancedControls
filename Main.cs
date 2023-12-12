using EnhancedControls.KeyboardBindings;
using HarmonyLib;
using Kingmaker;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows;
using Kingmaker.Settings.Entities;
using System;
using UnityEngine;
using UnityModManagerNet;

namespace EnhancedControls;

#if DEBUG
[EnableReloading]
#endif
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
        modEntry.OnGUI = OnGUI;
        modEntry.OnSaveGUI = OnSaveGUI;
        Settings.Save(modEntry);
        return true;
    }

    static void OnGUI(UnityModManager.ModEntry modEntry)
    {
        GUIStyle redLabelStyle = new(GUI.skin.label);
        redLabelStyle.normal.textColor = Color.red;
        GUILayout.Label("All changes are applied ONLY AFTER RESTART", redLabelStyle);
        GUILayout.Space(5);
        GUILayout.Label("To change - use keycodes taken from https://docs.unity3d.com/ScriptReference/KeyCode.html");
        GUILayout.Label("To remove - leave field empty.");
        GUILayout.Label("Support for entering keys by pressing them will come later");

        GUILayout.Space(20);
        var labelWidth = GUILayout.Width(GUI.skin.label.CalcSize(new GUIContent("Lore (Imperium)                ")).x);
        // Iterate over fields
        foreach (var field in Settings.GetType().GetFields())
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(field.Name, labelWidth);
            field.SetValue(Settings, GUILayout.TextField(field.GetValue(Settings).ToString(), GUILayout.Width(150)));
            GUILayout.EndHorizontal();
        }

        if (GUI.changed)
        {
            Settings.Save(modEntry);
        }
    }
    static void OnSaveGUI(UnityModManager.ModEntry modEntry)
    {
        Settings.Save(modEntry);
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