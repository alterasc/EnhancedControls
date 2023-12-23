using EnhancedControls.Localization;
using Kingmaker.UI.Models.SettingsUI;
using Kingmaker.UI.Models.SettingsUI.SettingAssets;
using UnityEngine;

namespace EnhancedControls.UI;

public static class OwlcatUITools
{
    public static UISettingsEntityBool MakeToggle(string key, string name, string tooltip)
    {
        var toggle = ScriptableObject.CreateInstance<UISettingsEntityBool>();
        toggle.m_Description = ModLocalizationManager.CreateString($"{key}.description", name);
        toggle.m_TooltipDescription = ModLocalizationManager.CreateString($"{key}.tooltip-description", tooltip);
        toggle.DefaultValue = false;
        return toggle;
    }

    public static UISettingsGroup MakeSettingsGroup(string key, string name, params UISettingsEntityBase[] settings)
    {
        UISettingsGroup group = ScriptableObject.CreateInstance<UISettingsGroup>();
        group.name = key;
        group.Title = ModLocalizationManager.CreateString(key, name);

        group.SettingsList = settings;

        return group;
    }
}
