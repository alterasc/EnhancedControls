﻿using EnhancedControls.Features;
using EnhancedControls.Features.Camera;
using EnhancedControls.Settings;
using EnhancedControls.UI;
using HarmonyLib;
using Kingmaker;
using Kingmaker.Settings;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Models.SettingsUI;
using System.Collections.Generic;
using System.Linq;

namespace EnhancedControls;

/// <summary>
/// Main class that holds state of all mod functionality by setting
/// </summary>
public class ModSettings
{
    public IReadOnlyList<ModSettingEntry> modSettings = new List<ModSettingEntry> {
        new HighlightToggle(),
        new SeparateEndTurn(),
        new CollectAllAndClose(),
        new InventorySearchField(),
        new NextCharacter(),
        new NextTab(),
        new PrevCharacter(),
        new PrevTab(),
        new TakenFeaturesLast()
    };

    public IReadOnlyList<ModSettingEntry> camera = new List<ModSettingEntry>
    {
        new CameraRotateDefault(),
        new RotateCameraWithRMB()
    };

    private bool Initialized = false;

    public void Initialize()
    {
        if (Initialized) return;
        Initialized = true;

        foreach (ModSettingEntry setting in modSettings)
        {
            setting.BuildUIAndLink();
            setting.TryEnable();
        }
        foreach (ModSettingEntry setting in camera)
        {
            setting.BuildUIAndLink();
            setting.TryEnable();
        }
        if (ModHotkeySettingEntry.ReSavingRequired)
        {
            SettingsController.Instance.SaveAll();
            Main.log.Log("Hotkey settings were migrated");
        }
    }

    private static readonly ModSettings instance = new();
    public static ModSettings Instance { get { return instance; } }
}

[HarmonyPatch]
public static class SettingsUIPatches
{
    /// <summary>
    /// Initializes mod features and adds setting group to Controls section of game settings
    /// </summary>
    [HarmonyPatch(typeof(UISettingsManager), nameof(UISettingsManager.Initialize))]
    [HarmonyPostfix]
    static void AddSettingsGroup()
    {
        if (Game.Instance.UISettingsManager.m_ControlSettingsList.Any(group => group.name?.StartsWith(ModSettingEntry.PREFIX) ?? false))
        {
            return;
        }
        ModSettings.Instance.Initialize();

        Game.Instance.UISettingsManager.m_ControlSettingsList.Add(
            OwlcatUITools.MakeSettingsGroup($"{ModSettingEntry.PREFIX}.maingroup", "Enhanced Controls",
                ModSettings.Instance.modSettings.Select(x => x.GetUISettings()).ToArray()
                ));

        Game.Instance.UISettingsManager.m_ControlSettingsList.Add(
            OwlcatUITools.MakeSettingsGroup($"{ModSettingEntry.PREFIX}.cameragroup", "Enhanced Controls - Camera",
                ModSettings.Instance.camera.Select(x => x.GetUISettings()).ToArray()
                ));
    }

    /// <summary>
    /// Allows registration of any key combination for custom hotkeys.
    /// This is because original Owlcat validation is too strict and
    /// prevents usage of same key even if executed actions between keys do not conflict
    /// </summary>
    [HarmonyPatch(typeof(KeyboardAccess), nameof(KeyboardAccess.CanBeRegistered))]
    [HarmonyPrefix]
    public static bool CanRegisterAnything(ref bool __result, string name)
    {
        if (name != null && name.StartsWith(ModSettingEntry.PREFIX))
        {
            __result = true;
            return false;
        }
        return true;
    }
}