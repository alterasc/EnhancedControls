using EnhancedControls.KeyboardBindings;
using HarmonyLib;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows;
using System.Reflection;
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
        HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        Settings.Save(modEntry);
        return true;
    }
}

[HarmonyPatch(typeof(ServiceWindowsVM), nameof(ServiceWindowsVM.BindKeys))]
public static class KeybindAddition
{
    [HarmonyPostfix]
    public static void Add()
    {
        //Kingmaker.Game.LoadArea(BlueprintArea, BlueprintAreaEnterPoint, AutoSaveMode, SaveInfo, Action) : void @060005A1
        NextTab.Add();
        NextCharacter.Add();
        InventorySearchField.Add();
        HighlightToggle.Add();
    }
}