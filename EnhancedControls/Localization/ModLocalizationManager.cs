using Kingmaker.Localization;
using Kingmaker.Localization.Enums;
using Kingmaker.Localization.Shared;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace EnhancedControls.Localization;

internal class ModLocalizationManager
{
    private static MyLocalizationPack enPack;

    public static void Init()
    {
        enPack = LoadPack(Locale.enGB);

        ApplyLocalization(LocalizationManager.Instance.CurrentLocale);

        (LocalizationManager.Instance as ILocalizationProvider).LocaleChanged += ApplyLocalization;
    }

    public static void ApplyLocalization(Locale currentLocale)
    {
        var currentPack = LocalizationManager.Instance.CurrentPack;
        foreach (var entry in enPack.Strings)
        {
            currentPack.PutString(entry.Key, entry.Value.Text);
        }

        if (currentLocale != Locale.enGB)
        {
            var localized = LoadPack(currentLocale);
            if (localized != null)
            {
                foreach (var entry in localized.Strings)
                {
                    currentPack.PutString(entry.Key, entry.Value.Text);
                }
            }
        }
    }

    private static MyLocalizationPack LoadPack(Locale locale)
    {
        var localizationFolder = Path.Combine(Main.ModEntry.Path, "Localization");
        var packFile = Path.Combine(localizationFolder, locale.ToString() + ".json");
        if (File.Exists(packFile))
        {
            try
            {
                using StreamReader file = File.OpenText(packFile);
                using JsonReader jsonReader = new JsonTextReader(file);
                JsonSerializer serializer = new();
                var enLocalization = serializer.Deserialize<MyLocalizationPack>(jsonReader);
                return enLocalization;
            }
            catch (System.Exception ex)
            {
                Main.log.Error($"Failed to read or parse {locale} mod localization pack: {ex.Message}");
            }
        }
        else
        {
            Main.log.Log($"Missing localization pack for {locale}");
        }
        return null;
    }

    public static LocalizedString CreateString(string key, string value)
    {
        if (enPack.Strings.ContainsKey(key))
        {
            return new LocalizedString { m_ShouldProcess = false, m_Key = key };
        }
        else
        {
            Main.log.Log($"Missing localization string {key}");
            return new LocalizedString { m_ShouldProcess = false, m_Key = key };
        }
    }
}

public record class MyLocalizationPack
{
    [JsonProperty]
    public readonly Dictionary<string, MyLocalizationEntry> Strings;
}

public struct MyLocalizationEntry
{
    [JsonProperty]
    public readonly string Text;
};