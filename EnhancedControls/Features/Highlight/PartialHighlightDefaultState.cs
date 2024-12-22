using EnhancedControls.Settings;

namespace EnhancedControls.Features.Highlight;
internal class PartialHighlightDefaultState : ModToggleSettingEntry
{
    private const string _key = "partialhighlightdefaultstate";
    private const string _title = "Partial highlighting default state";
    private const string _tooltip = "Partial highlighting default state. On = load into game, partial highlight toggled enabled, Off = disabled";
    private const bool _defaultValue = false;

    public PartialHighlightDefaultState() : base(_key, _title, _tooltip, _defaultValue) { }

    public override SettingStatus TryEnable()
    {
        var currentValue = SettingEntity.GetValue();
        HighlightManager.IsBasicHighlightToggledOn = currentValue;
        return SettingStatus.WORKING;
    }

}
