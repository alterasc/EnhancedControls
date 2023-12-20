using EnhancedControls.Localization;
using EnhancedControls.Settings;
using HarmonyLib;
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

    static bool Load(UnityModManager.ModEntry modEntry)
    {
        log = modEntry.Logger;
        HarmonyInstance = new Harmony(modEntry.Info.Id);

        // patch to manage added localized strings
        HarmonyInstance.CreateClassProcessor(typeof(ModLocalizationManager.AddLocalizedStringsToPack)).Patch();

        // patches to configure mod
        HarmonyInstance.CreateClassProcessor(typeof(SettingsUIPatches)).Patch();

        modEntry.OnGUI = OnGUI;
#if DEBUG
        modEntry.OnUnload = OnUnload;
#endif
        return true;
    }

#if DEBUG
    static bool OnUnload(UnityModManager.ModEntry modEntry)
    {
        return true;
    }
#endif

    static void OnGUI(UnityModManager.ModEntry modEntry)
    {
        GUILayout.Label("This mod is configured through normal game settings. Go to Settings -> Controls -> scroll to the bottom");
    }
}