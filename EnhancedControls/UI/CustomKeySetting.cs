using Kingmaker.Settings;
using Kingmaker.Settings.Entities;
using Kingmaker.UI.Models.SettingsUI.SettingAssets;

namespace EnhancedControls.UI;

public class CustomKeySetting
{
    public readonly string Key;
    public readonly string Title;
    public readonly string Tooltip;
    public readonly SettingsEntityKeyBindingPair SettingEntity;
    public UISettingsEntityKeyBinding UiSettingEntity { get; private set; }

    public CustomKeySetting(string key, string title, string tooltip, string DefaultKeyPairString)
    {
        Key = key;
        Title = title;
        Tooltip = tooltip + "\r\n\r\nALL SETTING CHANGES REQUIRE RESTART";
        SettingEntity = new(SettingsController.Instance, $"{ModSettings.PREFIX}.newcontrols.{Key}", new(DefaultKeyPairString), false, true);
    }

    public void BuildUIAndLink()
    {
        UiSettingEntity = OwlcatUITools.MakeKeyBind($"{ModSettings.PREFIX}.newcontrols.ui.{Key}", Title, Tooltip);
        UiSettingEntity.Setting = SettingEntity;
        UiSettingEntity.m_SettingKeyBindingPair = SettingEntity;
    }

    public KeyBindingPair GetValue() => SettingEntity.GetValue();
}
