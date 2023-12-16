using HarmonyLib;
using Kingmaker;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Models.SettingsUI;
using System.Linq;

namespace EnhancedControls.UI;

public class ModSettings
{
    internal const string PREFIX = "alterasc.enhancedcontrols";

    public CustomKeySetting HighlightToggle = new(
        "highlighttoggle",
        "Toggle Highlight Objects",
        "Toggles object highlighting.",
        "%R;;World;false");

    public CustomKeySetting SeparateEndTurn = new(
        "separateendturn",
        "Separate End Turn",
        "If this button is bound, then it removes End Turn functionality from Pause button (Space) and assigns this to this button.",
        "%Space;;World;false");

    public CustomKeySetting CollectAll = new(
        "collectallandclose",
        "Collect All and Close",
        "Collects all loot and closes loot window",
        "F;;World;false");

    public CustomKeySetting NextCharacter = new(
        "nextcharacter",
        "Next Character",
        "Moves selection to the next character.",
        "Tab;;World;false");

    public CustomKeySetting PrevCharacter = new(
        "prevcharacter",
        "Previous Character",
        "Moves selection to the previous character.",
        "#Tab;;World;false");

    public CustomKeySetting NextTab = new(
        "nexttab",
        "Next Menu Tab",
        "In character menu moves selection to next page tab  (so Summary -> Features -> Archetypes ->...)\r\nIn inventory moves inventory filter (All -> Weapons -> Armor -> ...)",
        "%Tab;;World;false");

    public CustomKeySetting PrevTab = new(
        "prevtab",
        "Previous Menu Tab",
        "In character menu moves selection to previous page tab  (so Archetypes -> Features -> Summary ->...)\r\nIn inventory moves inventory filter (Armor -> Weapons -> All -> ...)",
        "%#Tab;;World;false");

    public CustomKeySetting InventorySearch = new(
        "inventorysearch",
        "Search Inventory",
        "Activates search input field when you're in inventory menu",
        "%F;;World;false");




    private bool Initialized = false;

    public void Initialize()
    {
        if (Initialized) return;
        Initialized = true;

        HighlightToggle.BuildUIAndLink();
        SeparateEndTurn.BuildUIAndLink();
        CollectAll.BuildUIAndLink();
        InventorySearch.BuildUIAndLink();
        NextTab.BuildUIAndLink();
        PrevTab.BuildUIAndLink();
        NextCharacter.BuildUIAndLink();
        PrevCharacter.BuildUIAndLink();
    }

    private static readonly ModSettings instance = new();
    public static ModSettings Instance { get { return instance; } }
}

[HarmonyPatch]
public static class SettingsUIPatches
{
    /// <summary>
    /// Adds setting group to Controls section of game settings
    /// </summary>
    [HarmonyPatch(typeof(UISettingsManager), nameof(UISettingsManager.Initialize))]
    [HarmonyPostfix]
    static void AddSettingsGroup()
    {
        if (Game.Instance.UISettingsManager.m_ControlSettingsList.Any(group => group.name?.StartsWith(ModSettings.PREFIX) ?? false))
        {
            return;
        }
        ModSettings.Instance.Initialize();

        Game.Instance.UISettingsManager.m_ControlSettingsList.Add(
            OwlcatUITools.MakeSettingsGroup($"{ModSettings.PREFIX}.maingroup", "Enhanced Controls",
                ModSettings.Instance.HighlightToggle.UiSettingEntity,
                ModSettings.Instance.SeparateEndTurn.UiSettingEntity,
                ModSettings.Instance.CollectAll.UiSettingEntity,
                ModSettings.Instance.NextCharacter.UiSettingEntity,
                ModSettings.Instance.PrevCharacter.UiSettingEntity,
                ModSettings.Instance.NextTab.UiSettingEntity,
                ModSettings.Instance.PrevTab.UiSettingEntity,
                ModSettings.Instance.InventorySearch.UiSettingEntity
                ));

        KeybindingPatchManager.Run();
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
        if (name != null && name.StartsWith(ModSettings.PREFIX))
        {
            __result = true;
            return false;
        }
        return true;
    }
}