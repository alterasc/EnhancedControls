using Kingmaker.Settings;
using Kingmaker.Settings.Entities;
using Kingmaker.UI.Models.SettingsUI.SettingAssets;

namespace EnhancedControls.UI;

public class CustomToggleSetting
{
    public readonly string Key;
    public readonly string Title;
    public readonly string Tooltip;
    public readonly SettingsEntityBool SettingEntity;
    public UISettingsEntityBool UiSettingEntity { get; private set; }

    public CustomToggleSetting(string key, string title, string tooltip, bool defaultValue)
    {
        Key = key;
        Title = title;
        Tooltip = tooltip + "\r\n\r\nALL SETTING CHANGES REQUIRE RESTART";
        SettingEntity = new(SettingsController.Instance, $"{ModSettings.PREFIX}.newcontrols.{Key}", defaultValue, false, true);
    }

    public void BuildUIAndLink()
    {
        UiSettingEntity = OwlcatUITools.MakeToggle($"{ModSettings.PREFIX}.newcontrols.ui.{Key}", Title, Tooltip);
        UiSettingEntity.Setting = SettingEntity;
        UiSettingEntity.LinkSetting(SettingEntity);
    }

    public bool GetValue() => SettingEntity.GetValue();
}
