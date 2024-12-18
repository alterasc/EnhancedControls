using EnhancedControls.Settings;
using System;

namespace EnhancedControls.Features.Highlight;
internal class PartialHighlightNPCs : ModToggleSettingEntry
{
    private const string _key = "partialhighlightnpc";
    private const string _title = "Partial highlight shows NPC names";
    private const string _tooltip = "Makes partial highlight show names of most alive NPCs.";
    private const bool _defaultValue = false;

    protected override SettingStatus TryEnableAndPatch(params Type[] type)
    {
        return base.TryEnableAndPatch(type);
    }

    public override SettingStatus TryEnable()
    {
        var currentValue = SettingEntity.GetValue();
        HighlightManager.PartialHighlightNPCs = currentValue;
        return SettingStatus.WORKING;
    }

    public PartialHighlightNPCs() : base(_key, _title, _tooltip, _defaultValue) { }
}
