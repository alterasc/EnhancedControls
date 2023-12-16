using HarmonyLib;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using System.Collections.Generic;

namespace EnhancedControls.Localization;

internal class ModLocalizationManager
{
    // All localized strings created in this mod, mapped to their localized key. Populated by CreateString.
    private static readonly Dictionary<string, LocalString> strings = new();

    public static LocalizedString CreateString(string key, string value)
    {
        // See if we used the text previously.
        // (It's common for many features to use the same localized text.
        // In that case, we reuse the old entry instead of making a new one.)
        if (strings.TryGetValue(value, out var localString))
        {
            return localString.LocalizedString;
        }
        LocalizationManager.Instance.CurrentPack.PutString(key, value);
        localString = new LocalString(value, new LocalizedString { m_ShouldProcess = false, m_Key = key });
        strings[key] = localString;
        return localString.LocalizedString;
    }
    [HarmonyPatch(typeof(LocalizationManager))]
    internal static class AddLocalizedStringsToPack
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(LocalizationManager.LoadPack), typeof(Kingmaker.Localization.Enums.Locale))]
        public static void AddMyLocalizationString(ref LocalizationPack __result)
        {
            foreach (var str in strings)
            {
                __result.PutString(str.Key, str.Value.Text);
            }
        }
    }

    private class LocalString
    {
        internal string Text;
        internal LocalizedString LocalizedString;

        public LocalString(string text, LocalizedString localizedString)
        {
            Text = text;
            LocalizedString = localizedString;
        }
    }
}
